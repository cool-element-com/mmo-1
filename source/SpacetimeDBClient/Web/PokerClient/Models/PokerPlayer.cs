using System;

namespace PokerClient.Models
{
    // Remove [SpacetimeTable] - this is for server modules only!
    public class PokerPlayer
    {
        // Remove ALL [SpacetimeField] attributes - they're for server modules only!
        public string PlayerId { get; set; } = "";
        public string GameId { get; set; } = "";
        public string Name { get; set; } = "";
        public ulong Chips { get; set; }
        public bool IsActive { get; set; }
        public bool IsFolded { get; set; }
        public ulong CurrentBet { get; set; }
        public ulong JoinedAt { get; set; }

        // Helper properties for UI
        public decimal ChipsDecimal => Chips / 100m;
        public decimal CurrentBetDecimal => CurrentBet / 100m;
        public DateTime JoinedAtDateTime => DateTimeOffset.FromUnixTimeMilliseconds((long)JoinedAt).DateTime;

        public override string ToString()
        {
            return $"Player: {Name} (ID: {PlayerId}), Chips: {ChipsDecimal:C}, Active: {IsActive}, Folded: {IsFolded}";
        }
    }
}
