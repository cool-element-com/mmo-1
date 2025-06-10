# SpacetimeDB Integration Instructions

This directory contains the structure for integrating with SpacetimeDB. Follow these instructions to properly set up the client code.

## Prerequisites
- SpacetimeDB CLI installed (download from the official SpacetimeDB website)
- .NET 6.0 SDK or later

## Steps to Generate Client Code

1. First, create a SpacetimeDB module using the schema.rs file:
   ```bash
   # Navigate to a directory where you want to create the module
   mkdir poker-module
   cd poker-module
   
   # Copy the schema.rs file
   cp /path/to/source/SpacetimeDBClient/schema.rs ./src/lib.rs
   
   # Publish the module
   spacetime publish
   ```

2. Generate the C# client code:
   ```bash
   # Navigate to the PokerClient directory
   cd /path/to/source/SpacetimeDBClient/Web/PokerClient
   
   # Create a directory for generated code
   mkdir -p Generated
   
   # Generate C# client code
   spacetime generate csharp --module-path /path/to/poker-module --output-dir ./Generated
   ```

3. Update the Config.cs file with your module address:
   ```csharp
   public static SpacetimeDBConfig Development => new SpacetimeDBConfig
   {
       ServerAddress = "localhost:3000",
       DatabaseName = "YOUR_MODULE_ADDRESS", // Replace with your module address
       AutoReconnect = true,
       ReconnectAttempts = 5,
       ReconnectDelay = TimeSpan.FromSeconds(2)
   };
   ```

4. Build and run the client:
   ```bash
   dotnet build
   dotnet run
   ```

## Project Structure

- `Models/` - Contains model classes that will be mapped to SpacetimeDB tables
- `Generated/` - Where the SpacetimeDB CLI will generate client code (create this directory)
- `Config.cs` - Configuration for SpacetimeDB connection
- `ConnectionManager.cs` - Manages connection to SpacetimeDB
- `Program.cs` - Main entry point for the application

## Note on SpacetimeDB Integration

SpacetimeDB does not provide a NuGet package. Instead, it uses code generation through its CLI tool to create client libraries specific to your schema. The generated code will include all necessary classes for connecting to SpacetimeDB, subscribing to tables, and calling reducers.
