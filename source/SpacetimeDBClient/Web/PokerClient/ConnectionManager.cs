using System;
using System.Threading.Tasks;
using SpacetimeDB; // Remove SpacetimeDB.Types - it doesn't exist

namespace PokerClient
{
    public class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> _instance = new Lazy<ConnectionManager>(() => new ConnectionManager());
        public static ConnectionManager Instance => _instance.Value;

        private DbConnection? _connection; // Changed from SpacetimeDBClient
        private SpacetimeDBConfig _config;
        private bool _isConnected;
        private int _reconnectAttempts;

        public bool IsConnected => _isConnected;
        public DbConnection? Connection => _connection; // Changed from SpacetimeDBClient

        public event EventHandler<bool>? ConnectionStatusChanged;

        private ConnectionManager()
        {
            _config = SpacetimeDBConfig.Development;
            _isConnected = false;
            _reconnectAttempts = 0;
        }

        public void Configure(SpacetimeDBConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                // Use proper DbConnection builder pattern
                var builder = DbConnection.Builder()
                    .OnConnect(OnConnectedCallback)
                    .OnConnectError(OnConnectErrorCallback)
                    .OnDisconnect(OnDisconnectedCallback)
                    .OnIdentityReceived(OnIdentityReceivedCallback)
                    .OnSubscriptionApplied(OnSubscriptionAppliedCallback);

                if (!string.IsNullOrEmpty(_config.AuthToken))
                {
                    builder = builder.WithToken(_config.AuthToken);
                }

                _connection = builder.Build();
                await _connection.ConnectAsync(_config.ServerAddress, _config.DatabaseName);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null && _isConnected)
            {
                await _connection.DisconnectAsync();
                _connection = null;
                _isConnected = false;
            }
        }

        // Callback methods
        private void OnConnectedCallback(DbConnection connection, Identity identity, string token)
        {
            Console.WriteLine($"Connected to SpacetimeDB with identity: {identity}");
            _isConnected = true;
            _reconnectAttempts = 0;
            ConnectionStatusChanged?.Invoke(this, true);
        }

        private void OnConnectErrorCallback(DbConnection connection, SpacetimeDbException error)
        {
            Console.WriteLine($"Connection error: {error.Message}");
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
            
            if (_config.AutoReconnect)
            {
                Task.Run(ReconnectAsync);
            }
        }

        private void OnDisconnectedCallback(DbConnection connection, SpacetimeDbException? error)
        {
            string message = error?.Message ?? "Disconnected";
            Console.WriteLine($"Disconnected from SpacetimeDB: {message}");
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
            
            if (_config.AutoReconnect && error != null)
            {
                Task.Run(ReconnectAsync);
            }
        }

        private void OnIdentityReceivedCallback(DbConnection connection, Identity identity, string token)
        {
            Console.WriteLine($"Identity received: {identity}");
        }

        private void OnSubscriptionAppliedCallback(DbConnection connection)
        {
            Console.WriteLine("Subscription applied - initial data received");
        }

        // Method to call reducers
        public async Task<bool> CallReducerAsync<T>(string reducerName, T args)
        {
            if (_connection?.Reducers == null || !_isConnected)
            {
                Console.WriteLine("Cannot call reducer: not connected");
                return false;
            }

            try
            {
                await _connection.Reducers.CallAsync(reducerName, args);
                Console.WriteLine($"Successfully called reducer: {reducerName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reducer call failed for {reducerName}: {ex.Message}");
                return false;
            }
        }

        private async Task ReconnectAsync()
        {
            while (!_isConnected && _reconnectAttempts < _config.ReconnectAttempts)
            {
                _reconnectAttempts++;
                Console.WriteLine($"Attempting to reconnect ({_reconnectAttempts}/{_config.ReconnectAttempts})...");
                
                try
                {
                    await Task.Delay(_config.ReconnectDelay);
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnection attempt failed: {ex.Message}");
                }
            }

            if (!_isConnected)
            {
                Console.WriteLine("Failed to reconnect after multiple attempts");
            }
        }
    }
}
