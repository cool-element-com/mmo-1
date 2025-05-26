// SpacetimeDB Schema for Poker Game Module
// This file should be used to create a module on your local SpacetimeDB instance

use spacetimedb::{spacetimedb, Identity, ReducerContext, Timestamp};

// Define the poker game table
#[spacetimedb(table)]
pub struct PokerGame {
    #[primarykey]
    pub game_id: String,
    pub name: String,
    pub status: String,
    pub current_round: u32,
    pub pot_amount: u64,
    pub buy_in: u64,
    pub max_players: u32,
    pub created_at: Timestamp,
    pub updated_at: Timestamp,
}

// Define the poker player table
#[spacetimedb(table)]
pub struct PokerPlayer {
    #[primarykey]
    pub player_id: String,
    pub game_id: String,
    pub name: String,
    pub chips: u64,
    pub is_active: bool,
    pub is_folded: bool,
    pub current_bet: u64,
    pub joined_at: Timestamp,
}

// Reducer to create a new game
#[spacetimedb(reducer)]
pub fn create_poker_game(ctx: ReducerContext, game_name: String, buy_in: u64, max_players: u32) -> Result<String, String> {
    let game_id = format!("game_{}", ctx.timestamp.micros());
    let now = ctx.timestamp;
    
    PokerGame::insert(PokerGame {
        game_id: game_id.clone(),
        name: game_name,
        status: "waiting".to_string(),
        current_round: 0,
        pot_amount: 0,
        buy_in,
        max_players,
        created_at: now,
        updated_at: now,
    })?;
    
    Ok(game_id)
}

// Reducer to join a game
#[spacetimedb(reducer)]
pub fn join_poker_game(ctx: ReducerContext, game_id: String, player_name: String) -> Result<String, String> {
    // Check if game exists
    let game = PokerGame::filter_by_game_id(&game_id)
        .first()
        .ok_or_else(|| "Game not found".to_string())?;
        
    // Check if game is full
    let player_count = PokerPlayer::filter_by_game_id(&game_id).count();
    if player_count >= game.max_players as usize {
        return Err("Game is full".to_string());
    }
    
    // Create player ID
    let player_id = format!("player_{}_{}", ctx.sender, ctx.timestamp.micros());
    let now = ctx.timestamp;
    
    // Insert player
    PokerPlayer::insert(PokerPlayer {
        player_id: player_id.clone(),
        game_id,
        name: player_name,
        chips: game.buy_in,
        is_active: true,
        is_folded: false,
        current_bet: 0,
        joined_at: now,
    })?;
    
    Ok(player_id)
}

// Reducer to place a bet
#[spacetimedb(reducer)]
pub fn place_poker_bet(ctx: ReducerContext, game_id: String, player_id: String, amount: u64) -> Result<(), String> {
    // Check if player exists
    let mut player = PokerPlayer::filter_by_player_id(&player_id)
        .first()
        .ok_or_else(|| "Player not found".to_string())?;
        
    // Check if player has enough chips
    if player.chips < amount {
        return Err("Not enough chips".to_string());
    }
    
    // Update player chips and bet
    player.chips -= amount;
    player.current_bet += amount;
    PokerPlayer::update_by_player_id(&player_id, player)?;
    
    // Update game pot
    let mut game = PokerGame::filter_by_game_id(&game_id)
        .first()
        .ok_or_else(|| "Game not found".to_string())?;
        
    game.pot_amount += amount;
    game.updated_at = ctx.timestamp;
    PokerGame::update_by_game_id(&game_id, game)?;
    
    Ok(())
}

// Reducer to fold a hand
#[spacetimedb(reducer)]
pub fn fold_poker_hand(ctx: ReducerContext, game_id: String, player_id: String) -> Result<(), String> {
    // Check if player exists
    let mut player = PokerPlayer::filter_by_player_id(&player_id)
        .first()
        .ok_or_else(|| "Player not found".to_string())?;
        
    // Update player status
    player.is_folded = true;
    PokerPlayer::update_by_player_id(&player_id, player)?;
    
    // Update game timestamp
    let mut game = PokerGame::filter_by_game_id(&game_id)
        .first()
        .ok_or_else(|| "Game not found".to_string())?;
        
    game.updated_at = ctx.timestamp;
    PokerGame::update_by_game_id(&game_id, game)?;
    
    Ok(())
}
