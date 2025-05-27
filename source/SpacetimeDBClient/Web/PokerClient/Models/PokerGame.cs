using System;
using SpacetimeDB.Client.Attributes;

namespace PokerClient.Models
{
    [SpacetimeTable("poker_games")]
    public class PokerGame
    {
        [SpacetimeField("game_id")]
        public string GameId { get; set; }

        [SpacetimeField("name")]
        public string Name { get; set; }

        [SpacetimeField("status")]
        public string Status { get; set; }

        [SpacetimeField("current_round")]
        public int CurrentRound { get; set; }

        [SpacetimeField("pot_amount")]
        public decimal PotAmount { get; set; }

        [SpacetimeField("buy_in")]
        public decimal BuyIn { get; set; }

        [SpacetimeField("max_players")]
        public int MaxPlayers { get; set; }

        [SpacetimeField("created_at")]
        public DateTime CreatedAt { get; set; }

        [SpacetimeField("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return $"Game: {Name} (ID: {GameId}), Status: {Status}, Pot: {PotAmount}, Round: {CurrentRound}";
        }
    }
}
