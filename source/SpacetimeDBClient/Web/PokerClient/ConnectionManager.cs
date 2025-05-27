using System;
using System.Threading.Tasks;
using SpacetimeDB.ClientSDK;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Concurrent;

namespace PokerClient
{
    public class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> _instance = new Lazy<ConnectionManager>(() => new ConnectionManager());
        public static ConnectionManager Instance => _instance.Value;

        private SpacetimeDBClient _client;
        private SpacetimeDBConfig _config;
        private bool _isConnected;
        private int _reconnectAttempts;

        public bool IsConnected => _isConnected;
        public SpacetimeDBClient Client => _client;

        public event EventHandler<bool> ConnectionStatusChanged;

        private ConnectionManager()
        {
            _client = new SpacetimeDBClient();
            _config = SpacetimeDBConfig.Development;
            _isConnected = false;
            _reconnectAttempts = 0;

            // Register connection event handlers
            _client.OnConnected += (sender, args) =>
            {
                Console.WriteLine("Connected to SpacetimeDB");
                _isConnected = true;
                _reconnectAttempts = 0;
                ConnectionStatusChanged?.Invoke(this, true);
            };

            _client.OnDisconnected += (sender, args) =>
            {
                Console.WriteLine("Disconnected from SpacetimeDB");
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
                
                if (_config.AutoReconnect)
                {
                    Task.Run(ReconnectAsync);
                }
            };
        }

        public void Configure(SpacetimeDBConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await _client.ConnectAsync(_config.ServerAddress, _config.DatabaseName);
                
                if (!string.IsNullOrEmpty(_config.AuthToken))
                {
                    await _client.AuthenticateAsync(_config.AuthToken);
                }
                
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
                await _client.DisconnectAsync();
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
