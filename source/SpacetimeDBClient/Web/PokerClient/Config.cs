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
            DatabaseName = "c200358c4ee7a927a8e9e4449ca29426a85f4424cea799b68f8e6d307fbd462d", // poker-game module identity
            AutoReconnect = true,
            ReconnectAttempts = 5,
            ReconnectDelay = TimeSpan.FromSeconds(2)
        };

        public static SpacetimeDBConfig Production => new SpacetimeDBConfig
        {
            ServerAddress = "api.spacetimedb.com",
            DatabaseName = "YOUR_PRODUCTION_MODULE_ADDRESS", // Replace with your production module address
            AutoReconnect = true,
            ReconnectAttempts = 10,
            ReconnectDelay = TimeSpan.FromSeconds(5)
        };
    }
}
