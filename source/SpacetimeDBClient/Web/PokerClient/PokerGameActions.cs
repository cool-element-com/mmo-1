using System;
using System.Threading.Tasks;

namespace PokerClient
{
    public class PokerGameActions
    {
        private static readonly Lazy<PokerGameActions> _instance = new Lazy<PokerGameActions>(() => new PokerGameActions());
        public static PokerGameActions Instance => _instance.Value;

        private CustomDbConnection? Connection => ConnectionManager.Instance.Connection;

        public async Task<string> CreateGameAsync(string gameName, decimal buyIn, int maxPlayers)
        {
            try
            {
                await ConnectionManager.Instance.CallReducerAsync("create_poker_game", new CreatePokerGameArgs
                {
                    GameName = gameName,
                    BuyIn = (uint)(buyIn * 100), // Convert to cents
                    MaxPlayers = (uint)maxPlayers
                });

                Console.WriteLine($"Successfully created game {gameName}");
                return "game_created"; // SpacetimeDB reducers don't return values directly
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
                await ConnectionManager.Instance.CallReducerAsync("join_poker_game", new JoinPokerGameArgs
                {
                    GameId = gameId,
                    PlayerName = playerName
                });

                Console.WriteLine($"Successfully joined game {gameId} as {playerName}");
                return "player_joined"; // You'll get actual data from table update events
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
                await ConnectionManager.Instance.CallReducerAsync("place_poker_bet", new PlacePokerBetArgs
                {
                    GameId = gameId,
                    PlayerId = playerId,
                    Amount = (uint)(amount * 100) // Convert to cents
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
                await ConnectionManager.Instance.CallReducerAsync("fold_poker_hand", new FoldPokerHandArgs
                {
                    GameId = gameId,
                    PlayerId = playerId
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
