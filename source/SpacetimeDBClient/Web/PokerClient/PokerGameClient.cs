using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpacetimeDB.Types;

namespace PokerClient
{
    public class PokerGameClient
    {
        private readonly ConnectionManager _connectionManager;
        private string? _currentPlayerId;
        private string? _currentGameId;

        public event EventHandler<PokerGame>? GameCreated;
        public event EventHandler<PokerPlayer>? PlayerJoined;
        public event EventHandler<PokerPlayer>? PlayerBetPlaced;
        public event EventHandler<PokerPlayer>? PlayerFolded;
        public event EventHandler<string>? ErrorOccurred;

        public PokerGameClient()
        {
            _connectionManager = ConnectionManager.Instance;
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _connectionManager.ConnectionStatusChanged += (sender, connected) =>
            {
                if (connected)
                {
                    Console.WriteLine("Connected to SpacetimeDB - subscribing to poker tables");
                    SubscribeToTables();
                }
            };
        }

        private void SubscribeToTables()
        {
            if (_connectionManager.Connection == null) return;

            try
            {
                // Subscribe to all poker game data
                _connectionManager.Connection.SubscriptionBuilder()
                    .OnApplied(ctx =>
                    {
                        Console.WriteLine("Subscription applied - poker data synchronized");
                        SetupReducerCallbacks();
                    })
                    .OnError((ctx, ex) =>
                    {
                        Console.WriteLine($"Subscription error: {ex.Message}");
                        ErrorOccurred?.Invoke(this, ex.Message);
                    })
                    .Subscribe(new[]
                    {
                        "SELECT * FROM PokerGame",
                        "SELECT * FROM PokerPlayer"
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set up subscription: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }

        private void SetupReducerCallbacks()
        {
            if (_connectionManager.Connection?.Reducers == null) return;

            // Listen for game creation events
            _connectionManager.Connection.Reducers.OnCreatePokerGame((ctx, args) =>
            {
                Console.WriteLine($"Game created: {args.GameName}");
                var game = _connectionManager.Connection.Db.PokerGame.FirstOrDefault(g => g.Name == args.GameName);
                if (game != null)
                {
                    GameCreated?.Invoke(this, game);
                }
            });

            // Listen for player join events
            _connectionManager.Connection.Reducers.OnJoinPokerGame((ctx, args) =>
            {
                Console.WriteLine($"Player {args.PlayerName} joined game {args.GameId}");
                var player = _connectionManager.Connection.Db.PokerPlayer.FirstOrDefault(p => 
                    p.GameId == args.GameId && p.Name == args.PlayerName);
                if (player != null)
                {
                    PlayerJoined?.Invoke(this, player);
                }
            });

            // Listen for bet events
            _connectionManager.Connection.Reducers.OnPlacePokerBet((ctx, args) =>
            {
                Console.WriteLine($"Player {args.PlayerId} bet {args.Amount} in game {args.GameId}");
                var player = _connectionManager.Connection.Db.PokerPlayer.PlayerId.Get(args.PlayerId);
                if (player != null)
                {
                    PlayerBetPlaced?.Invoke(this, player);
                }
            });

            // Listen for fold events
            _connectionManager.Connection.Reducers.OnFoldPokerHand((ctx, args) =>
            {
                Console.WriteLine($"Player {args.PlayerId} folded in game {args.GameId}");
                var player = _connectionManager.Connection.Db.PokerPlayer.PlayerId.Get(args.PlayerId);
                if (player != null)
                {
                    PlayerFolded?.Invoke(this, player);
                }
            });
        }

        public async Task<string?> CreateGameAsync(string gameName, decimal buyIn, uint maxPlayers)
        {
            if (_connectionManager.Connection?.Reducers == null)
            {
                ErrorOccurred?.Invoke(this, "Not connected to server");
                return null;
            }

            try
            {
                // Convert decimal to cents (ulong)
                ulong buyInCents = (ulong)(buyIn * 100);
                
                var gameId = await _connectionManager.Connection.Reducers.CreatePokerGame(gameName, buyInCents, maxPlayers);
                Console.WriteLine($"Created game: {gameName} with ID: {gameId}");
                return gameId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create game: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
                return null;
            }
        }

        public async Task<string?> JoinGameAsync(string gameId, string playerName)
        {
            if (_connectionManager.Connection?.Reducers == null)
            {
                ErrorOccurred?.Invoke(this, "Not connected to server");
                return null;
            }

            try
            {
                var playerId = await _connectionManager.Connection.Reducers.JoinPokerGame(gameId, playerName);
                _currentPlayerId = playerId;
                _currentGameId = gameId;
                Console.WriteLine($"Joined game {gameId} as {playerName} (Player ID: {playerId})");
                return playerId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to join game: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
                return null;
            }
        }

        public async Task<bool> PlaceBetAsync(decimal amount)
        {
            if (_connectionManager.Connection?.Reducers == null || _currentPlayerId == null || _currentGameId == null)
            {
                ErrorOccurred?.Invoke(this, "Not connected or not in a game");
                return false;
            }

            try
            {
                // Convert decimal to cents (ulong)
                ulong amountCents = (ulong)(amount * 100);
                
                await _connectionManager.Connection.Reducers.PlacePokerBet(_currentGameId, _currentPlayerId, amountCents);
                Console.WriteLine($"Placed bet of ${amount:F2}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to place bet: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
                return false;
            }
        }

        public async Task<bool> FoldAsync()
        {
            if (_connectionManager.Connection?.Reducers == null || _currentPlayerId == null || _currentGameId == null)
            {
                ErrorOccurred?.Invoke(this, "Not connected or not in a game");
                return false;
            }

            try
            {
                await _connectionManager.Connection.Reducers.FoldPokerHand(_currentGameId, _currentPlayerId);
                Console.WriteLine("Folded hand");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fold: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex.Message);
                return false;
            }
        }

        public IEnumerable<PokerGame> GetAvailableGames()
        {
            if (_connectionManager.Connection?.Db?.PokerGame == null)
                return Enumerable.Empty<PokerGame>();

            return _connectionManager.Connection.Db.PokerGame.Where(g => g.Status == "waiting");
        }

        public IEnumerable<PokerPlayer> GetPlayersInGame(string gameId)
        {
            if (_connectionManager.Connection?.Db?.PokerPlayer == null)
                return Enumerable.Empty<PokerPlayer>();

            return _connectionManager.Connection.Db.PokerPlayer.FilterByGameId(gameId);
        }

        public PokerGame? GetCurrentGame()
        {
            if (_connectionManager.Connection?.Db?.PokerGame == null || _currentGameId == null)
                return null;

            return _connectionManager.Connection.Db.PokerGame.GameId.Get(_currentGameId);
        }

        public PokerPlayer? GetCurrentPlayer()
        {
            if (_connectionManager.Connection?.Db?.PokerPlayer == null || _currentPlayerId == null)
                return null;

            return _connectionManager.Connection.Db.PokerPlayer.PlayerId.Get(_currentPlayerId);
        }
    }
}