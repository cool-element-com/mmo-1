using System;
using System.Threading.Tasks;
using SpacetimeDB.Client;

namespace PokerClient
{
    public class PokerGameActions
    {
        private static readonly Lazy<PokerGameActions> _instance = new Lazy<PokerGameActions>(() => new PokerGameActions());
        public static PokerGameActions Instance => _instance.Value;

        private SpacetimeDBClient Client => ConnectionManager.Instance.Client;

        public async Task<string> CreateGameAsync(string gameName, decimal buyIn, int maxPlayers)
        {
            try
            {
                var result = await Client.Reducer.CallAsync<string>("create_poker_game", new
                {
                    game_name = gameName,
                    buy_in = (ulong)buyIn,
                    max_players = (uint)maxPlayers
                });

                Console.WriteLine($"Successfully created game {gameName} with ID {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create game: {ex.Message}");
                throw;
            }
        }

        public async Task<string> JoinGameAsync(string gameId, string playerName)
        {
            try
            {
                var result = await Client.Reducer.CallAsync<string>("join_poker_game", new
                {
                    game_id = gameId,
                    player_name = playerName
                });

                Console.WriteLine($"Successfully joined game {gameId} as {playerName} with player ID {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to join game: {ex.Message}");
                throw;
            }
        }

        public async Task PlaceBetAsync(string gameId, string playerId, decimal amount)
        {
            try
            {
                await Client.Reducer.CallAsync("place_poker_bet", new
                {
                    game_id = gameId,
                    player_id = playerId,
                    amount = (ulong)amount
                });

                Console.WriteLine($"Successfully placed bet of {amount} for player {playerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to place bet: {ex.Message}");
                throw;
            }
        }

        public async Task FoldHandAsync(string gameId, string playerId)
        {
            try
            {
                await Client.Reducer.CallAsync("fold_poker_hand", new
                {
                    game_id = gameId,
                    player_id = playerId
                });

                Console.WriteLine($"Player {playerId} folded their hand");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fold hand: {ex.Message}");
                throw;
            }
        }
    }
}
