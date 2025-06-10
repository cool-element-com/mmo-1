using System;
using System.Threading.Tasks;
using SpacetimeDB.Types;

namespace PokerClient
{
    class SimplePokerClient
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SpacetimeDB Poker Client ===");
            Console.WriteLine();

            try
            {
                // Configure connection
                var config = SpacetimeDBConfig.Development;
                Console.WriteLine($"Connecting to SpacetimeDB at {config.ServerAddress}...");
                Console.WriteLine($"Module: {config.DatabaseName}");

                // Create connection using the generated DbConnection
                var conn = DbConnection.Builder()
                    .OnConnect((conn, identity, token) =>
                    {
                        Console.WriteLine($"✅ Connected! Identity: {identity}");
                        
                        // Subscribe to all tables
                        conn.SubscriptionBuilder()
                            .OnApplied(ctx =>
                            {
                                Console.WriteLine("📡 Subscription applied - data synchronized");
                                ShowCurrentState(conn);
                            })
                            .SubscribeToAllTables();
                    })
                    .OnConnectError((conn, error) =>
                    {
                        Console.WriteLine($"❌ Connection error: {error.Message}");
                    })
                    .Build();

                // Connect to the database
                await conn.ConnectAsync(config.ServerAddress, config.DatabaseName);

                // Wait for initial sync
                await Task.Delay(2000);

                // Interactive menu
                await RunInteractiveMenu(conn);

                // Disconnect
                await conn.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine("Make sure SpacetimeDB is running: spacetime start");
            }

            Console.WriteLine("Goodbye!");
        }

        static async Task RunInteractiveMenu(DbConnection conn)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Poker Game Menu ===");
                Console.WriteLine("1. Create new game");
                Console.WriteLine("2. Join game");
                Console.WriteLine("3. Place bet");
                Console.WriteLine("4. Fold hand");
                Console.WriteLine("5. Show game state");
                Console.WriteLine("0. Exit");
                Console.Write("Choose option: ");

                var input = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            await CreateGame(conn);
                            break;
                        case "2":
                            await JoinGame(conn);
                            break;
                        case "3":
                            await PlaceBet(conn);
                            break;
                        case "4":
                            await FoldHand(conn);
                            break;
                        case "5":
                            ShowCurrentState(conn);
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static async Task CreateGame(DbConnection conn)
        {
            Console.Write("Game name: ");
            var gameName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(gameName))
            {
                Console.WriteLine("Invalid game name.");
                return;
            }

            Console.Write("Buy-in amount (cents): ");
            if (!ulong.TryParse(Console.ReadLine(), out ulong buyIn) || buyIn == 0)
            {
                Console.WriteLine("Invalid buy-in amount.");
                return;
            }

            Console.Write("Max players (2-8): ");
            if (!uint.TryParse(Console.ReadLine(), out uint maxPlayers) || maxPlayers < 2 || maxPlayers > 8)
            {
                Console.WriteLine("Invalid number of players.");
                return;
            }

            Console.WriteLine($"Creating game '{gameName}'...");
            await conn.Reducers.CreatePokerGame(gameName, buyIn, maxPlayers);
            Console.WriteLine("✅ Game creation request sent!");
        }

        static async Task JoinGame(DbConnection conn)
        {
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
            await conn.Reducers.JoinPokerGame(gameId, playerName);
            Console.WriteLine("✅ Join request sent!");
        }

        static async Task PlaceBet(DbConnection conn)
        {
            Console.Write("Game ID: ");
            var gameId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                Console.WriteLine("Invalid game ID.");
                return;
            }

            Console.Write("Bet amount (cents): ");
            if (!ulong.TryParse(Console.ReadLine(), out ulong amount) || amount == 0)
            {
                Console.WriteLine("Invalid bet amount.");
                return;
            }

            Console.WriteLine($"Placing bet of {amount} cents...");
            await conn.Reducers.PlacePokerBet(gameId, amount);
            Console.WriteLine("✅ Bet request sent!");
        }

        static async Task FoldHand(DbConnection conn)
        {
            Console.Write("Game ID: ");
            var gameId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(gameId))
            {
                Console.WriteLine("Invalid game ID.");
                return;
            }

            Console.WriteLine("Folding hand...");
            await conn.Reducers.FoldPokerHand(gameId);
            Console.WriteLine("✅ Fold request sent!");
        }

        static void ShowCurrentState(DbConnection conn)
        {
            Console.WriteLine("📊 Current Game State:");
            Console.WriteLine();

            // Show all games
            Console.WriteLine("🎮 Games:");
            foreach (var game in conn.Db.PokerGame)
            {
                Console.WriteLine($"  ID: {game.GameId}");
                Console.WriteLine($"  Name: {game.Name}");
                Console.WriteLine($"  Status: {game.Status}");
                Console.WriteLine($"  Buy-in: {game.BuyIn} cents");
                Console.WriteLine($"  Pot: {game.PotAmount} cents");
                Console.WriteLine($"  Players: ?/{game.MaxPlayers}");
                Console.WriteLine();
            }

            // Show all players
            Console.WriteLine("👥 Players:");
            foreach (var player in conn.Db.PokerPlayer)
            {
                Console.WriteLine($"  ID: {player.PlayerId}");
                Console.WriteLine($"  Name: {player.Name}");
                Console.WriteLine($"  Game: {player.GameId}");
                Console.WriteLine($"  Chips: {player.Chips} cents");
                Console.WriteLine($"  Current Bet: {player.CurrentBet} cents");
                Console.WriteLine($"  Active: {player.IsActive}");
                Console.WriteLine($"  Folded: {player.IsFolded}");
                Console.WriteLine();
            }

            if (!conn.Db.PokerGame.Any() && !conn.Db.PokerPlayer.Any())
            {
                Console.WriteLine("No games or players yet. Create a game to get started!");
            }
        }
    }
}