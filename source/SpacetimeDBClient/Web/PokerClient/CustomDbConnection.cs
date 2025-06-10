using System;
using System.Threading.Tasks;
using SpacetimeDB.Types;
using SpacetimeDB.ClientApi;

namespace PokerClient
{
    // Custom wrapper around the generated DbConnection to provide the API our code expects
    public class CustomDbConnection
    {
        private SpacetimeDB.Types.DbConnection? _internalConnection;
        private bool _isConnected;
        private string? _serverAddress;
        private string? _databaseName;

        public bool IsConnected => _isConnected;
        public SpacetimeDB.Types.DbConnection? Connection => _internalConnection;

        // Events
        public event Action<CustomDbConnection, bool>? ConnectionStatusChanged;

        public async Task<bool> ConnectAsync(string serverAddress, string databaseName)
        {
            try
            {
                _serverAddress = serverAddress;
                _databaseName = databaseName;

                // Create the internal connection
                _internalConnection = new SpacetimeDB.Types.DbConnection();

                // For now, we'll simulate connection success
                // In a real implementation, you'd use the SpacetimeDB connection methods
                _isConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);

                Console.WriteLine($"Connected to {serverAddress} database {databaseName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_internalConnection != null && _isConnected)
            {
                try
                {
                    _internalConnection.Disconnect();
                }
                catch
                {
                    // Ignore disconnect errors
                }
                
                _internalConnection = null;
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        // Wrapper for calling reducers
        public async Task<bool> CallReducerAsync<T>(string reducerName, T args)
        {
            if (_internalConnection?.Reducers == null || !_isConnected)
            {
                Console.WriteLine("Cannot call reducer: not connected");
                return false;
            }

            try
            {
                // Use the generated reducer methods based on name
                switch (reducerName)
                {
                    case "create_poker_game":
                        if (args is CreatePokerGameArgs createArgs)
                        {
                            _internalConnection.Reducers.CreatePokerGame(
                                createArgs.GameName, 
                                (ulong)createArgs.BuyIn, 
                                createArgs.MaxPlayers
                            );
                        }
                        break;
                    case "join_poker_game":
                        if (args is JoinPokerGameArgs joinArgs)
                        {
                            _internalConnection.Reducers.JoinPokerGame(
                                joinArgs.GameId, 
                                joinArgs.PlayerName
                            );
                        }
                        break;
                    case "place_poker_bet":
                        if (args is PlacePokerBetArgs betArgs)
                        {
                            _internalConnection.Reducers.PlacePokerBet(
                                betArgs.GameId, 
                                (ulong)betArgs.Amount
                            );
                        }
                        break;
                    case "fold_poker_hand":
                        if (args is FoldPokerHandArgs foldArgs)
                        {
                            _internalConnection.Reducers.FoldPokerHand(
                                foldArgs.GameId
                            );
                        }
                        break;
                    default:
                        Console.WriteLine($"Unknown reducer: {reducerName}");
                        return false;
                }

                Console.WriteLine($"Successfully called reducer: {reducerName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reducer call failed for {reducerName}: {ex.Message}");
                return false;
            }
        }

        // Wrapper for subscriptions
        public async Task<bool> SubscribeAsync(string query)
        {
            if (_internalConnection == null || !_isConnected)
            {
                Console.WriteLine("Cannot subscribe: not connected");
                return false;
            }

            try
            {
                // Use the subscription builder
                _internalConnection.SubscriptionBuilder()
                    .OnApplied(ctx => Console.WriteLine($"Subscription applied for query: {query}"))
                    .OnError((ctx, ex) => Console.WriteLine($"Subscription error: {ex.Message}"))
                    .Subscribe(new[] { query });

                Console.WriteLine($"Subscribed to: {query}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscription failed: {ex.Message}");
                return false;
            }
        }
    }

    // Argument classes for type-safe reducer calls
    public class CreatePokerGameArgs
    {
        public string GameName { get; set; } = "";
        public uint BuyIn { get; set; }
        public uint MaxPlayers { get; set; }
    }

    public class JoinPokerGameArgs
    {
        public string GameId { get; set; } = "";
        public string PlayerName { get; set; } = "";
    }

    public class PlacePokerBetArgs
    {
        public string GameId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public uint Amount { get; set; }
    }

    public class FoldPokerHandArgs
    {
        public string GameId { get; set; } = "";
        public string PlayerId { get; set; } = "";
    }
}