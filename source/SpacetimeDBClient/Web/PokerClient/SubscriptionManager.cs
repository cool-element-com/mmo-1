using System;
using System.Threading.Tasks;
using SpacetimeDB;
using PokerClient.Models;

namespace PokerClient
{
    public class SubscriptionManager
    {
        private static readonly Lazy<SubscriptionManager> _instance = new Lazy<SubscriptionManager>(() => new SubscriptionManager());
        public static SubscriptionManager Instance => _instance.Value;

        // Use ConnectionManager instead of direct client reference
        private DbConnection? Connection => ConnectionManager.Instance.Connection;

        public event EventHandler<PokerGame>? GameUpdated;
        public event EventHandler<PokerPlayer>? PlayerUpdated;

        private SubscriptionManager()
        {
            // Event handlers will be set up when connection is established
            ConnectionManager.Instance.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(object? sender, bool isConnected)
        {
            if (isConnected)
            {
                SetupSubscriptions();
            }
        }

        private async void SetupSubscriptions()
        {
            if (Connection == null) return;

            try
            {
                // Subscribe to poker tables
                await Connection.SubscribeAsync("SELECT * FROM poker_games");
                await Connection.SubscribeAsync("SELECT * FROM poker_players");
                
                Console.WriteLine("Subscribed to poker tables");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscription error: {ex.Message}");
            }
        }

        public async Task<bool> SubscribeToPokerGameAsync(string gameId)
        {
            if (!ConnectionManager.Instance.IsConnected || Connection == null)
            {
                Console.WriteLine("Cannot subscribe: not connected to SpacetimeDB");
                return false;
            }

            try
            {
                await Connection.SubscribeAsync($"SELECT * FROM poker_games WHERE game_id = '{gameId}'");
                await Connection.SubscribeAsync($"SELECT * FROM poker_players WHERE game_id = '{gameId}'");

                Console.WriteLine($"Subscribed to poker game {gameId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscription error: {ex.Message}");
                return false;
            }
        }

        // Remove the RowUpdate handler - this will be replaced with proper table event handlers
        // once you have generated code or can access the table objects
    }
}
