# MMO-1: SpacetimeDB Poker Game PoC

A Proof of Concept project demonstrating a simple MMO-like poker game using SpacetimeDB for real-time synchronization between multiple clients.

## Project Structure

```
mmo-1/
├── source/
│   ├── spacetime-db/poker-module/     # Rust server module
│   │   └── src/lib.rs                 # Game logic and reducers
│   └── SpacetimeDBClient/Web/         # C# client application
│       └── PokerClient/               # Main client code
```

## Prerequisites

- **SpacetimeDB CLI** - Download from [spacetimedb.com](https://spacetimedb.com)
- **Rust** (latest stable) - Required for server module
- **.NET 6.0 SDK** or later - Required for C# client
- **Docker** (optional) - For containerized deployment

## Setup Instructions

### 1. Install SpacetimeDB

```bash
# Download and install SpacetimeDB CLI
curl -sSL https://install.spacetimedb.com | bash

# Verify installation
spacetime version
```

### 2. Start SpacetimeDB Server

```bash
# Start local SpacetimeDB instance
spacetime start

# Server will run on http://localhost:3000
# Web console available at http://localhost:8080
```

### 3. Publish the Poker Module

```bash
# Navigate to the poker module directory
cd source/spacetime-db/poker-module

# Publish the module to your local SpacetimeDB instance
spacetime publish poker-game

# Note: Save the module address that gets printed - you'll need it for the client
```

### 4. Generate C# Client Code

```bash
# Navigate to the client directory
cd source/SpacetimeDBClient/Web/PokerClient

# Create directory for generated code (if it doesn't exist)
mkdir -p Generated

# Generate C# client bindings
spacetime generate csharp --module-name poker-game --output-dir ./Generated

# This creates all necessary client code based on your module schema
```

### 5. Configure the Client

Update `Config.cs` with your module address:

```csharp
public static SpacetimeDBConfig Development => new SpacetimeDBConfig
{
    ServerAddress = "localhost:3000",
    DatabaseName = "YOUR_MODULE_ADDRESS_HERE", // Replace with actual module address
    AutoReconnect = true,
    ReconnectAttempts = 5,
    ReconnectDelay = TimeSpan.FromSeconds(2)
};
```

### 6. Build and Run the Client

```bash
# Build the client
dotnet build

# Run the client
dotnet run
```

## Game Features

### Server-Side (Rust)
- **PokerGame Table**: Stores game state, pot, buy-in, player limits
- **PokerPlayer Table**: Tracks player chips, bets, and game status
- **Reducers**:
  - `create_poker_game` - Create new poker games
  - `join_poker_game` - Players join existing games
  - `place_poker_bet` - Handle betting logic
  - `fold_poker_hand` - Player folds their hand

### Client-Side (C#)
- **Real-time Connection**: Automatic reconnection and error handling
- **Game State Sync**: Live updates when players join/leave or place bets
- **Event Handling**: Callbacks for connection status and game events

## Usage

1. **Start the server**: `spacetime start`
2. **Publish module**: `spacetime publish poker-game`
3. **Run client**: `dotnet run` in the PokerClient directory
4. **Multiple clients**: Run multiple instances to simulate multiplayer

## Development Workflow

1. **Modify Server Logic**: Edit `source/spacetime-db/poker-module/src/lib.rs`
2. **Republish Module**: Run `spacetime publish poker-game`
3. **Regenerate Client**: Run `spacetime generate csharp --module-name poker-game --output-dir ./Generated`
4. **Rebuild Client**: Run `dotnet build && dotnet run`

## Troubleshooting

### Common Issues

**"Module not found" error**
- Ensure SpacetimeDB server is running (`spacetime start`)
- Check that module was published successfully
- Verify module address matches in Config.cs

**Connection failures**
- Check that server is running on localhost:3000
- Verify firewall settings
- Ensure no other services are using port 3000

**Build errors in client**
- Run `dotnet restore` to ensure packages are installed
- Check that generated code exists in ./Generated directory
- Verify .NET SDK version with `dotnet --version`

**"Table not found" errors**
- Regenerate client code after module changes
- Ensure module schema matches client expectations
- Check SpacetimeDB logs for schema validation errors

### Useful Commands

```bash
# View running modules
spacetime list

# Check module logs
spacetime logs poker-game

# Reset local database
spacetime delete poker-game

# View module schema
spacetime describe poker-game
```

## Next Steps

This PoC demonstrates:
- ✅ Real-time client-server communication
- ✅ Multi-client synchronization
- ✅ Game state management
- ✅ Automatic reconnection

Potential enhancements:
- Card dealing and hand evaluation
- Tournament modes
- Player authentication
- Web-based client
- Advanced UI/UX

## Architecture Notes

- **SpacetimeDB** handles all real-time synchronization automatically
- **Server module** contains all game logic and validation
- **Client** is a thin layer that reacts to server state changes
- **Generated code** provides type-safe client bindings

This architecture ensures game integrity while providing seamless multiplayer experience.
