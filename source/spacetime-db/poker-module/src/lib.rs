use spacetimedb::{table, reducer, Table, ReducerContext, Identity, Timestamp};

#[table(name = poker_game, public)]
pub struct PokerGame {
    #[primary_key]
    game_id: String,
    name: String,
    status: String,
    current_round: u32,
    pot_amount: u64,
    buy_in: u64,
    max_players: u32,
    created_at: Timestamp,
    updated_at: Timestamp,
}

#[table(name = poker_player, public)]
pub struct PokerPlayer {
    #[primary_key]
    player_id: String,
    game_id: String,
    identity: Identity,
    name: String,
    chips: u64,
    is_active: bool,
    is_folded: bool,
    current_bet: u64,
    joined_at: Timestamp,
}

#[reducer]
pub fn create_poker_game(ctx: &ReducerContext, game_name: String, buy_in: u64, max_players: u32) -> Result<(), String> {
    let game_id = format!("game_{}_{}", game_name, ctx.timestamp.to_micros_since_unix_epoch());
    
    ctx.db.poker_game().insert(PokerGame {
        game_id: game_id.clone(),
        name: game_name,
        status: "waiting".to_string(),
        current_round: 0,
        pot_amount: 0,
        buy_in,
        max_players,
        created_at: ctx.timestamp,
        updated_at: ctx.timestamp,
    });
    
    log::info!("Created game: {}", game_id);
    Ok(())
}

#[reducer]
pub fn join_poker_game(ctx: &ReducerContext, game_id: String, player_name: String) -> Result<(), String> {
    // Check if game exists
    let game = ctx.db.poker_game().game_id().find(&game_id);
    
    if let Some(game) = game {
        // Check if game is full
        let player_count = ctx.db.poker_player().iter().filter(|p| p.game_id == game_id).count();
        if player_count >= game.max_players as usize {
            return Err("Game is full".to_string());
        }
        
        // Check if player already joined this game
        let existing_player = ctx.db.poker_player().iter()
            .find(|p| p.game_id == game_id && p.identity == ctx.sender);
        if existing_player.is_some() {
            return Err("Player already in this game".to_string());
        }
        
        // Create player ID
        let player_id = format!("player_{}_{}", ctx.sender, ctx.timestamp.to_micros_since_unix_epoch());
        
        // Insert player
        ctx.db.poker_player().insert(PokerPlayer {
            player_id: player_id.clone(),
            game_id,
            identity: ctx.sender,
            name: player_name.clone(),
            chips: game.buy_in,
            is_active: true,
            is_folded: false,
            current_bet: 0,
            joined_at: ctx.timestamp,
        });
        
        log::info!("Player {} joined game with ID: {}", player_name, player_id);
        Ok(())
    } else {
        Err("Game not found".to_string())
    }
}

#[reducer]
pub fn place_poker_bet(ctx: &ReducerContext, game_id: String, amount: u64) -> Result<(), String> {
    // Find player by identity in this game
    let player = ctx.db.poker_player().iter()
        .find(|p| p.game_id == game_id && p.identity == ctx.sender);
    
    if let Some(player) = player {
        // Check if player has enough chips
        if player.chips < amount {
            return Err("Not enough chips".to_string());
        }
        
        // Capture player_id before moving
        let player_id = player.player_id.clone();
        
        // Update player chips and bet
        let updated_player = PokerPlayer {
            chips: player.chips - amount,
            current_bet: player.current_bet + amount,
            ..player
        };
        ctx.db.poker_player().player_id().update(updated_player);
        
        // Update game pot
        if let Some(game) = ctx.db.poker_game().game_id().find(&game_id) {
            let updated_game = PokerGame {
                pot_amount: game.pot_amount + amount,
                updated_at: ctx.timestamp,
                ..game
            };
            ctx.db.poker_game().game_id().update(updated_game);
            
            log::info!("Player {} bet {} chips", player_id, amount);
            Ok(())
        } else {
            Err("Game not found".to_string())
        }
    } else {
        Err("Player not found in this game".to_string())
    }
}

#[reducer]
pub fn fold_poker_hand(ctx: &ReducerContext, game_id: String) -> Result<(), String> {
    // Find player by identity in this game
    let player = ctx.db.poker_player().iter()
        .find(|p| p.game_id == game_id && p.identity == ctx.sender);
    
    if let Some(player) = player {
        // Capture player_id before moving
        let player_id = player.player_id.clone();
        
        // Update player status
        let updated_player = PokerPlayer {
            is_folded: true,
            ..player
        };
        ctx.db.poker_player().player_id().update(updated_player);
        
        // Update game timestamp
        if let Some(game) = ctx.db.poker_game().game_id().find(&game_id) {
            let updated_game = PokerGame {
                updated_at: ctx.timestamp,
                ..game
            };
            ctx.db.poker_game().game_id().update(updated_game);
            
            log::info!("Player {} folded", player_id);
            Ok(())
        } else {
            Err("Game not found".to_string())
        }
    } else {
        Err("Player not found in this game".to_string())
    }
}