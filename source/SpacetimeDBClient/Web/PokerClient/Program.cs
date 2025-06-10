using System;
using System.Linq;
using System.Threading.Tasks;
using SpacetimeDB.Types;
using PokerClient;

class Program
{
    private static PokerGameClient? _pokerClient;
    private static bool _running = true;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== SpacetimeDB Poker Client ===");
        Console.WriteLine();

        // Configure connection
        var config = SpacetimeDBConfig.Development;
        ConnectionManager.Instance.Configure(config);

        // Create poker client
        _pokerClient = new PokerGameClient();
        SetupPokerEventHandlers();

        // Connect to SpacetimeDB
        Console.WriteLine("Connecting to SpacetimeDB...");
        bool connected = await ConnectionManager.Instance.ConnectAsync();
        if (!connected)
        {
            Console.WriteLine("Failed to connect to SpacetimeDB");
            Console.WriteLine("Make sure SpacetimeDB is running: spacetime start");
            return;
        }

        Console.WriteLine("Connected! Waiting for initial data sync...");
        await Task.Delay(2000); // Give time for subscription to sync

        // Main game loop
        await RunGameLoop();

        // Cleanup
        await ConnectionManager.Instance.DisconnectAsync();
        Console.WriteLine("Goodbye!");
    }

    private static void SetupPokerEventHandlers()
    {
        if (_pokerClient == null) return;

        _pokerClient.GameCreated += (sender, game) =>
        {
            Console.WriteLine($"🎮 New game created: {game.Name} (Buy-in: ${game.BuyInDecimal:F2})");
        };

        _pokerClient.PlayerJoined += (sender, player) =>
        {
            Console.WriteLine($"👤 {player.Name} joined the game with ${player.ChipsDecimal:F2} chips");
        };

        _pokerClient.PlayerBetPlaced += (sender, player) =>
        {
            Console.WriteLine($"💰 {player.Name} bet ${player.CurrentBetDecimal:F2} (Chips: ${player.ChipsDecimal:F2})");
        };

        _pokerClient.PlayerFolded += (sender, player) =>
        {
            Console.WriteLine($"🃏 {player.Name} folded");
        };

        _pokerClient.ErrorOccurred += (sender, error) =>
        {
            Console.WriteLine($"❌ Error: {error}");
        };
    }

    private static async Task RunGameLoop()
    {
        if (_pokerClient == null) return;

        while (_running)
        {
            Console.WriteLine();
            Console.WriteLine("=== Poker Game Menu ===");
            Console.WriteLine("1. List available games");
            Console.WriteLine("2. Create new game");
            Console.WriteLine("3. Join game");
            Console.WriteLine("4. Place bet");
            Console.WriteLine("5. Fold");
            Console.WriteLine("6. Show current game status");
            Console.WriteLine("7. Show my player info");
            Console.WriteLine("0. Exit");
            Console.Write("Choose option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (input)
                {
                    case "1":
                        await ListAvailableGames();
                        break;
                    case "2":
                        await CreateNewGame();
                        break;
                    case "3":
                        await JoinGame();
                        break;
                    case "4":
                        await PlaceBet();
                        break;
                    case "5":
                        await Fold();
                        break;
                    case "6":
                        await ShowGameStatus();
                        break;
                    case "7":
                        await ShowPlayerInfo();
                        break;
                    case "0":
                        _running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            if (_running)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private static async Task ListAvailableGames()
    {
        if (_pokerClient == null) return;

        var games = _pokerClient.GetAvailableGames().ToList();
        if (!games.Any())
        {
            Console.WriteLine("No games available. Create a new game to get started!");
            return;
        }

        Console.WriteLine("Available Games:");
        foreach (var game in games)
        {
            var players = _pokerClient.GetPlayersInGame(game.GameId).ToList();
            Console.WriteLine($"🎮 {game.Name}");
            Console.WriteLine($"   Game ID: {game.GameId}");
            Console.WriteLine($"   Buy-in: ${game.BuyInDecimal:F2}");
            Console.WriteLine($"   Players: {players.Count}/{game.MaxPlayers}");
            Console.WriteLine($"   Pot: ${game.PotAmountDecimal:F2}");
            Console.WriteLine($"   Status: {game.Status}");
            Console.WriteLine();
        }
    }

    private static async Task CreateNewGame()
    {
        if (_pokerClient == null) return;

        Console.Write("Game name: ");
        var gameName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(gameName))
        {
            Console.WriteLine("Invalid game name.");
            return;
        }

        Console.Write("Buy-in amount ($): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal buyIn) || buyIn <= 0)
        {
            Console.WriteLine("Invalid buy-in amount.");
            return;
        }

        Console.Write("Max players (2-8): ");
        if (!uint.TryParse(Console.ReadLine(), out uint maxPlayers) || maxPlayers < 2 || maxPlayers > 8)
        {
            Console.WriteLine("Invalid number of players (must be 2-8).");
            return;
        }

        Console.WriteLine($"Creating game '{gameName}' with ${buyIn:F2} buy-in for {maxPlayers} players...");
        var gameId = await _pokerClient.CreateGameAsync(gameName, buyIn, maxPlayers);
        
        if (gameId != null)
        {
            Console.WriteLine($"✅ Game created successfully! Game ID: {gameId}");
        }
    }

    private static async Task JoinGame()
    {
        if (_pokerClient == null) return;

        Console.Write("Game ID: ");
        var gameId = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(gameId))
        {
            Console.WriteLine("Invalid game ID.");
            return;
        }

        Console.Write("Your player name: ");
        var playerName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Console.WriteLine("Invalid player name.");
            return;
        }

        Console.WriteLine($"Joining game {gameId} as {playerName}...");
        var playerId = await _pokerClient.JoinGameAsync(gameId, playerName);
        
        if (playerId != null)
        {
            Console.WriteLine($"✅ Joined game successfully! Player ID: {playerId}");
        }
    }

    private static async Task PlaceBet()
    {
        if (_pokerClient == null) return;

        var currentPlayer = _pokerClient.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Console.WriteLine("You need to join a game first.");
            return;
        }

        Console.WriteLine($"Your chips: ${currentPlayer.ChipsDecimal:F2}");
        Console.Write("Bet amount ($): ");
        
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Invalid bet amount.");
            return;
        }

        if (amount > currentPlayer.ChipsDecimal)
        {
            Console.WriteLine("You don't have enough chips for that bet.");
            return;
        }

        Console.WriteLine($"Placing bet of ${amount:F2}...");
        var success = await _pokerClient.PlaceBetAsync(amount);
        
        if (success)
        {
            Console.WriteLine($"✅ Bet placed successfully!");
        }
    }

    private static async Task Fold()
    {
        if (_pokerClient == null) return;

        var currentPlayer = _pokerClient.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Console.WriteLine("You need to join a game first.");
            return;
        }

        Console.WriteLine("Folding your hand...");
        var success = await _pokerClient.FoldAsync();
        
        if (success)
        {
            Console.WriteLine($"✅ Hand folded!");
        }
    }

    private static async Task ShowGameStatus()
    {
        if (_pokerClient == null) return;

        var currentGame = _pokerClient.GetCurrentGame();
        if (currentGame == null)
        {
            Console.WriteLine("You're not in a game yet.");
            return;
        }

        var players = _pokerClient.GetPlayersInGame(currentGame.GameId).ToList();

        Console.WriteLine($"🎮 Game: {currentGame.Name}");
        Console.WriteLine($"   Game ID: {currentGame.GameId}");
        Console.WriteLine($"   Status: {currentGame.Status}");
        Console.WriteLine($"   Round: {currentGame.CurrentRound}");
        Console.WriteLine($"   Pot: ${currentGame.PotAmountDecimal:F2}");
        Console.WriteLine($"   Buy-in: ${currentGame.BuyInDecimal:F2}");
        Console.WriteLine($"   Players ({players.Count}/{currentGame.MaxPlayers}):");

        foreach (var player in players.OrderBy(p => p.Name))
        {
            var status = player.IsFolded ? "FOLDED" : (player.IsActive ? "ACTIVE" : "INACTIVE");
            Console.WriteLine($"     👤 {player.Name}: ${player.ChipsDecimal:F2} chips, ${player.CurrentBetDecimal:F2} bet [{status}]");
        }
    }

    private static async Task ShowPlayerInfo()
    {
        if (_pokerClient == null) return;

        var currentPlayer = _pokerClient.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Console.WriteLine("You're not in a game yet.");
            return;
        }

        Console.WriteLine($"👤 Player: {currentPlayer.Name}");
        Console.WriteLine($"   Player ID: {currentPlayer.PlayerId}");
        Console.WriteLine($"   Game ID: {currentPlayer.GameId}");
        Console.WriteLine($"   Chips: ${currentPlayer.ChipsDecimal:F2}");
        Console.WriteLine($"   Current Bet: ${currentPlayer.CurrentBetDecimal:F2}");
        Console.WriteLine($"   Status: {(currentPlayer.IsFolded ? "FOLDED" : (currentPlayer.IsActive ? "ACTIVE" : "INACTIVE"))}");
        Console.WriteLine($"   Joined: {currentPlayer.JoinedAtDateTime:yyyy-MM-dd HH:mm:ss}");
    }
}
