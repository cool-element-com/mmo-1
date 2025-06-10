using System;

namespace PokerClient.Models
{
    // Remove [SpacetimeTable] - this is for server modules only!
    public class PokerGame
    {
        // Remove ALL [SpacetimeField] attributes - they're for server modules only!
        public string GameId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public int CurrentRound { get; set; }
        public ulong PotAmount { get; set; }
        public ulong BuyIn { get; set; }
        public uint MaxPlayers { get; set; }
        public ulong CreatedAt { get; set; }
        public ulong UpdatedAt { get; set; }

        // Helper properties for UI
        public decimal PotAmountDecimal => PotAmount / 100m;
        public decimal BuyInDecimal => BuyIn / 100m;
        public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeMilliseconds((long)CreatedAt).DateTime;
        public DateTime UpdatedAtDateTime => DateTimeOffset.FromUnixTimeMilliseconds((long)UpdatedAt).DateTime;

        public override string ToString()
        {
            return $"Game: {Name} (ID: {GameId}), Status: {Status}, Pot: {PotAmountDecimal:C}, Round: {CurrentRound}";
        }
    }
}
