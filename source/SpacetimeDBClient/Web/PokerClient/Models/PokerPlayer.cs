using System;
using SpacetimeDB.Client.Attributes;

namespace PokerClient.Models
{
    [SpacetimeTable("poker_players")]
    public class PokerPlayer
    {
        [SpacetimeField("player_id")]
        public string PlayerId { get; set; }

        [SpacetimeField("game_id")]
        public string GameId { get; set; }

        [SpacetimeField("name")]
        public string Name { get; set; }

        [SpacetimeField("chips")]
        public decimal Chips { get; set; }

        [SpacetimeField("is_active")]
        public bool IsActive { get; set; }

        [SpacetimeField("is_folded")]
        public bool IsFolded { get; set; }

        [SpacetimeField("current_bet")]
        public decimal CurrentBet { get; set; }

        [SpacetimeField("joined_at")]
        public DateTime JoinedAt { get; set; }

        public override string ToString()
        {
            return $"Player: {Name} (ID: {PlayerId}), Chips: {Chips}, Active: {IsActive}, Folded: {IsFolded}";
        }
    }
}
