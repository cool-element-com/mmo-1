using System;
using System.Threading.Tasks;

namespace PokerClient
{
    public class MinimalClient
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🃏 Poker Client Starting...");
            
            try
            {
                // Configure connection
                var config = SpacetimeDBConfig.Development;
                ConnectionManager.Instance.Configure(config);
                
                // Connect to SpacetimeDB
                var connected = await ConnectionManager.Instance.ConnectAsync();
                
                if (!connected)
                {
                    Console.WriteLine("❌ Failed to connect to SpacetimeDB");
                    return;
                }
                
                Console.WriteLine("✅ Connected to SpacetimeDB!");
                
                // Set up subscriptions
                await SubscriptionManager.Instance.SubscribeToPokerGameAsync("test-game");
                
                // Demo: Create a poker game
                Console.WriteLine("🎮 Creating a demo poker game...");
                await PokerGameActions.Instance.CreateGameAsync("Demo Game", 10.00m, 6);
                
                // Keep running and listening for events
                Console.WriteLine("🎮 Poker client is running. Press any key to exit...");
                Console.ReadKey();
                
                // Disconnect
                await ConnectionManager.Instance.DisconnectAsync();
                Console.WriteLine("👋 Disconnected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}