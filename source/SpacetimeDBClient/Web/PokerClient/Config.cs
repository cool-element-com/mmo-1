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
            DatabaseName = "c2005eec15129cff598f91d6c4e283fff0e45cf8bc9e4a31be6f3cf600d6dfd2", // Replace with your module address after publishing
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
