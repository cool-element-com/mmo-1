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
pub fn create_poker_game(ctx: ReducerContext, game_name: String, buy_in: u64, max_players: u32) {
    let game_id = format!("game_{}", ctx.timestamp.into_micros_since_epoch());
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
    });
    
    println!("Created game: {}", game_id);
}

// Reducer to join a game
#[spacetimedb(reducer)]
pub fn join_poker_game(ctx: ReducerContext, game_id: String, player_name: String) {
    // Check if game exists
    let game = PokerGame::filter_by_game_id(&game_id)
        .next();
        
    if let Some(game) = game {
        // Check if game is full
        let player_count = PokerPlayer::filter_by_game_id(&game_id).count();
        if player_count >= game.max_players as usize {
            println!("Game is full");
            return;
        }
        
        // Create player ID
        let player_id = format!("player_{}_{}", ctx.sender, ctx.timestamp.into_micros_since_epoch());
        let now = ctx.timestamp;
        
        // Insert player
        PokerPlayer::insert(PokerPlayer {
            player_id: player_id.clone(),
            game_id,
            name: player_name.clone(),
            chips: game.buy_in,
            is_active: true,
            is_folded: false,
            current_bet: 0,
            joined_at: now,
        });
        
        println!("Player {} joined game with ID: {}", player_name, player_id);
    } else {
        println!("Game not found");
    }
}

// Reducer to place a bet
#[spacetimedb(reducer)]
pub fn place_poker_bet(ctx: ReducerContext, game_id: String, player_id: String, amount: u64) {
    // Check if player exists
    let player = PokerPlayer::filter_by_player_id(&player_id).next();
    
    if let Some(mut player) = player {
        // Check if player has enough chips
        if player.chips < amount {
            println!("Not enough chips");
            return;
        }
        
        // Update player chips and bet
        player.chips -= amount;
        player.current_bet += amount;
        PokerPlayer::update_by_player_id(&player_id, player);
        
        // Update game pot
        let game = PokerGame::filter_by_game_id(&game_id).next();
        if let Some(mut game) = game {
            game.pot_amount += amount;
            game.updated_at = ctx.timestamp;
            PokerGame::update_by_game_id(&game_id, game);
            
            println!("Player {} bet {} chips", player_id, amount);
        } else {
            println!("Game not found");
        }
    } else {
        println!("Player not found");
    }
}

// Reducer to fold a hand
#[spacetimedb(reducer)]
pub fn fold_poker_hand(ctx: ReducerContext, game_id: String, player_id: String) {
    // Check if player exists
    let player = PokerPlayer::filter_by_player_id(&player_id).next();
    
    if let Some(mut player) = player {
        // Update player status
        player.is_folded = true;
        PokerPlayer::update_by_player_id(&player_id, player);
        
        // Update game timestamp
        let game = PokerGame::filter_by_game_id(&game_id).next();
        if let Some(mut game) = game {
            game.updated_at = ctx.timestamp;
            PokerGame::update_by_game_id(&game_id, game);
            
            println!("Player {} folded", player_id);
        } else {
            println!("Game not found");
        }
    } else {
        println!("Player not found");
    }
}
