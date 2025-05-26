using System;
using System.Threading.Tasks;

namespace PokerClient
{
    public class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> _instance = new Lazy<ConnectionManager>(() => new ConnectionManager());
        public static ConnectionManager Instance => _instance.Value;

        private SpacetimeDBConfig _config;
        private bool _isConnected;
        private int _reconnectAttempts;

        public bool IsConnected => _isConnected;

        public event EventHandler<bool> ConnectionStatusChanged;

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
                // Simulate connection to SpacetimeDB
                await Task.Delay(500);
                
                Console.WriteLine("Connected to SpacetimeDB");
                _isConnected = true;
                _reconnectAttempts = 0;
                ConnectionStatusChanged?.Invoke(this, true);
                
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
            if (_isConnected)
            {
                await Task.Delay(200);
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
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
