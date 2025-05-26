using System;
using System.Threading.Tasks;
using PokerClient.Models;

namespace PokerClient
{
    class Program
    {
        static string _currentGameId;
        static string _currentPlayerId;

        static async Task Main(string[] args)
        {
            Console.WriteLine("SpacetimeDB Poker Client");
            Console.WriteLine("========================");

            try
            {
                // Configure and connect
                ConnectionManager.Instance.Configure(SpacetimeDBConfig.Development);
                Console.WriteLine("Connecting to SpacetimeDB...");
                
                bool connected = await ConnectionManager.Instance.ConnectAsync();
                if (!connected)
                {
                    Console.WriteLine("Failed to connect. Exiting...");
                    return;
                }

                // Register for game updates
                SubscriptionManager.Instance.GameUpdated += (sender, game) =>
                {
                    Console.WriteLine($"Game update received: {game}");
                };

                SubscriptionManager.Instance.PlayerUpdated += (sender, player) =>
                {
                    Console.WriteLine($"Player update received: {player}");
                };

                // Simple command loop
                bool running = true;
                while (running)
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("1. Create new game");
                    Console.WriteLine("2. Join game");
                    Console.WriteLine("3. Place bet");
                    Console.WriteLine("4. Fold hand");
                    Console.WriteLine("0. Exit");
                    
                    Console.Write("\nEnter command: ");
                    string command = Console.ReadLine();

                    try
                    {
                        switch (command)
                        {
                            case "1":
                                Console.Write("Enter game name: ");
                                string gameName = Console.ReadLine();
                                Console.Write("Enter buy-in amount: ");
                                if (decimal.TryParse(Console.ReadLine(), out decimal buyIn))
                                {
                                    Console.Write("Enter max players: ");
                                    if (int.TryParse(Console.ReadLine(), out int maxPlayers))
                                    {
                                        _currentGameId = await PokerGameActions.Instance.CreateGameAsync(gameName, buyIn, maxPlayers);
                                        await SubscriptionManager.Instance.SubscribeToPokerGameAsync(_currentGameId);
                                    }
                                }
                                break;
                                
                            case "2":
                                if (string.IsNullOrEmpty(_currentGameId))
                                {
                                    Console.Write("Enter game ID: ");
                                    _currentGameId = Console.ReadLine();
                                }
                                
                                Console.Write("Enter player name: ");
                                string playerName = Console.ReadLine();
                                _currentPlayerId = await PokerGameActions.Instance.JoinGameAsync(_currentGameId, playerName);
                                await SubscriptionManager.Instance.SubscribeToPokerGameAsync(_currentGameId);
                                break;
                                
                            case "3":
                                if (string.IsNullOrEmpty(_currentGameId) || string.IsNullOrEmpty(_currentPlayerId))
                                {
                                    Console.WriteLine("You must create or join a game first");
                                    break;
                                }
                                
                                Console.Write("Enter bet amount: ");
                                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                                {
                                    await PokerGameActions.Instance.PlaceBetAsync(_currentGameId, _currentPlayerId, amount);
                                }
                                break;
                                
                            case "4":
                                if (string.IsNullOrEmpty(_currentGameId) || string.IsNullOrEmpty(_currentPlayerId))
                                {
                                    Console.WriteLine("You must create or join a game first");
                                    break;
                                }
                                
                                await PokerGameActions.Instance.FoldHandAsync(_currentGameId, _currentPlayerId);
                                break;
                                
                            case "0":
                                running = false;
                                break;
                                
                            default:
                                Console.WriteLine("Unknown command");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing command: {ex.Message}");
                    }
                }

                // Disconnect when done
                await ConnectionManager.Instance.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
