using System;

namespace PokerClient
{
    public class SpacetimeDBConfig
    {
        public string ServerAddress { get; set; }
        public string DatabaseName { get; set; }
        public string AuthToken { get; set; }
        public bool AutoReconnect { get; set; }
        public int ReconnectAttempts { get; set; }
        public TimeSpan ReconnectDelay { get; set; }

        public static SpacetimeDBConfig Development => new SpacetimeDBConfig
        {
            ServerAddress = "localhost:3000",
            DatabaseName = "poker_game", // Replace with your module address after publishing
            AutoReconnect = true,
            ReconnectAttempts = 5,
            ReconnectDelay = TimeSpan.FromSeconds(2)
        };

        public static SpacetimeDBConfig Production => new SpacetimeDBConfig
        {
            ServerAddress = "api.spacetimedb.com",
            DatabaseName = "poker_game", // Replace with your production module address
            AutoReconnect = true,
            ReconnectAttempts = 10,
            ReconnectDelay = TimeSpan.FromSeconds(5)
        };
    }
}
