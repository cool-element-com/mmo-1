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

        private SpacetimeDBClient Client => ConnectionManager.Instance.Client;

        public event EventHandler<PokerGame> GameUpdated;
        public event EventHandler<PokerPlayer> PlayerUpdated;

        private SubscriptionManager()
        {
            // Register for row update events
            Client.OnRowsUpdated += (sender, args) =>
            {
                foreach (var update in args.Updates)
                {
                    HandleRowUpdate(update);
                }
            };
        }

        public async Task<bool> SubscribeToPokerGameAsync(string gameId)
        {
            try
            {
                // Subscribe to game state
                await Client.SubscribeAsync(
                    "SELECT * FROM poker_games WHERE game_id = $1",
                    new object[] { gameId }
                );

                // Subscribe to player data
                await Client.SubscribeAsync(
                    "SELECT * FROM poker_players WHERE game_id = $1",
                    new object[] { gameId }
                );

                Console.WriteLine($"Subscribed to poker game {gameId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscription error: {ex.Message}");
                return false;
            }
        }

        private void HandleRowUpdate(RowUpdate update)
        {
            switch (update.TableName)
            {
                case "poker_games":
                    var game = update.DeserializeAs<PokerGame>();
                    Console.WriteLine($"Game updated: {game.GameId}");
                    GameUpdated?.Invoke(this, game);
                    break;

                case "poker_players":
                    var player = update.DeserializeAs<PokerPlayer>();
                    Console.WriteLine($"Player updated: {player.PlayerId}");
                    PlayerUpdated?.Invoke(this, player);
                    break;

                default:
                    Console.WriteLine($"Unhandled table update: {update.TableName}");
                    break;
            }
        }
    }
}
