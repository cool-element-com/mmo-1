using System;
using System.Threading.Tasks;

namespace PokerClient
{
    public class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> _instance = new Lazy<ConnectionManager>(() => new ConnectionManager());
        public static ConnectionManager Instance => _instance.Value;

        private CustomDbConnection? _connection;
        private SpacetimeDBConfig _config;
        private bool _isConnected;
        private int _reconnectAttempts;

        public bool IsConnected => _isConnected;
        public CustomDbConnection? Connection => _connection;

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
                _connection = new CustomDbConnection();
                _connection.ConnectionStatusChanged += OnConnectionStatusChanged;
                
                var success = await _connection.ConnectAsync(_config.ServerAddress, _config.DatabaseName);
                _isConnected = success;
                _reconnectAttempts = 0;
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                _isConnected = false;
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

        // Simplified callback method
        private void OnConnectionStatusChanged(CustomDbConnection connection, bool isConnected)
        {
            _isConnected = isConnected;
            Console.WriteLine($"Connection status changed: {(isConnected ? "Connected" : "Disconnected")}");
            ConnectionStatusChanged?.Invoke(this, isConnected);
            
            if (!isConnected && _config.AutoReconnect)
            {
                Task.Run(ReconnectAsync);
            }
        }

        // Method to call reducers - delegate to our custom connection
        public async Task<bool> CallReducerAsync<T>(string reducerName, T args)
        {
            if (_connection == null || !_isConnected)
            {
                Console.WriteLine("Cannot call reducer: not connected");
                return false;
            }

            return await _connection.CallReducerAsync(reducerName, args);
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
