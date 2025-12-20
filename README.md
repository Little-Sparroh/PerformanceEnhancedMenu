# PerformanceEnhancedMenu

A BepInEx mod for MycoPunk that optimizes performance for the gear upgrade menu system with comprehensive caching and UI improvements.

## Description

This client-side performance mod addresses major lag issues in the gear upgrade menus through a complete overhaul of how upgrade computations and UI operations are handled. Version 2.0 introduces extensive optimizations including:

- **Comprehensive Cell Touching Caching**: All cell adjacency calculations are cached globally, not just for specific upgrade types
- **UI Performance Optimizations**: GearDetailsWindow Update() method throttling and raycast caching (temporarily disabled due to compatibility issues)
- **Lazy Stat Calculation**: Expensive cell touching computations deferred during menu transitions, calculated on-demand when needed
- **Asynchronous Pre-calculation**: Expensive stats calculated in background when hovering over gear in selection menu
- **Deferred Skin Loading**: Massive skin collections (>500 skins) load asynchronously to prevent FPS drops
- **Upgrade Grid Caching**: Full grid lookups cached to avoid repeated computations
- **Smart Cache Management**: Automatic cache invalidation when upgrades change

The mod dramatically improves performance for gear with many upgrades by preventing expensive per-frame recalculations while maintaining 100% accurate stat calculations.

## Getting Started

### Dependencies

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Option 1: Via Thunderstore (Recommended)**
1. Download and install using the Thunderstore Mod Manager
2. Search for "PerformanceEnhancedMenu" under MycoPunk community
3. Install and enable the mod

**Option 2: Manual Installation**
1. Ensure BepInEx is installed for MycoPunk
2. Copy `PerformanceEnhancedMenu.dll` from the build folder
3. Place it in `<MycoPunk Game.Directory>/BepInEx/plugins/`
4. Launch the game

### Executing program

Once installed, the mod works automatically in the gear upgrade menus:

**Performance Improvements:**
- **Faster Menu Navigation:** Significantly reduced lag when opening gear details
- **Smooth Upgrade Cycling:** Instant response when equipping/unequipping upgrades
- **Optimized Stat Calculations:** Cached results prevent expensive re-computations
- **MiniCannon Prisms:** Cached prism connectivity avoids repeated graph traversal
- **Globbler Upgrades:** Cached rarity touching calculations for stat bonuses

**System Behavior:**
- Automatically detects and caches upgrade grid layouts
- Caches query results for prism connections and rarity counts
- Clears caches when upgrade changes occur to maintain accuracy
- Profiler-friendly with timing information in logs

## Help

* **Menu still slow?** This mod targets specific performance bottlenecks - extremely complex upgrade layouts may still have some latency
* **Incompatible with mods?** May not be compatible with mods that modify upgrade grid logic or stat calculations
* **Wrong stat values?** Caching can sometimes get stale - try force-refreshing caches by unequipping/re-equipping upgrades
* **Performance worse?** Disable the mod if you experience any issues - it caches a lot of data operationally
* **Not seeing improvements?** This mod primarily affects menus with many upgrades or complex stat calculations
* **BepInEx logs?** Check logs for detailed timing information and any skipped patches

## Authors

* Sparroh
* funlennysub (original mod template)
* [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

* This project is licensed under the MIT License - see the LICENSE.md file for details
