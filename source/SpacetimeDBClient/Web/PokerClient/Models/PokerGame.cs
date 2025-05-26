using System;
using System.Collections.Generic;

namespace PokerClient.Models
{
    // This would have SpacetimeDB attributes in a real implementation
    public class PokerGame
    {
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int CurrentRound { get; set; }
        public decimal PotAmount { get; set; }
        public decimal BuyIn { get; set; }
        public int MaxPlayers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return $"Game: {Name} (ID: {GameId}), Status: {Status}, Pot: {PotAmount}, Round: {CurrentRound}";
        }
    }
}
