using System;
using System.Threading.Tasks;
using SpacetimeDB; // Remove SpacetimeDB.Types - it doesn't exist
using PokerClient;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Poker Client...");

        // Configure connection
        var config = new SpacetimeDBConfig
        {
            ServerAddress = "localhost:3000", // Adjust as needed
            DatabaseName = "poker-game", // Adjust as needed
            AutoReconnect = true,
            ReconnectAttempts = 5,
            ReconnectDelay = TimeSpan.FromSeconds(2)
        };

        ConnectionManager.Instance.Configure(config);

        // Set up event handlers
        ConnectionManager.Instance.ConnectionStatusChanged += (sender, connected) =>
        {
            Console.WriteLine($"Connection status: {(connected ? "Connected" : "Disconnected")}");
        };

        // Connect
        bool connected = await ConnectionManager.Instance.ConnectAsync();
        if (!connected)
        {
            Console.WriteLine("Failed to connect to SpacetimeDB");
            return;
        }

        // Keep the application running
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        await ConnectionManager.Instance.DisconnectAsync();
        Console.WriteLine("Goodbye!");
    }
}
