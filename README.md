# PerformanceEnhancedMenu

A performance-enhancing mod for MycoPunk that caches gear upgrade calculations to eliminate menu lag for heavy equipment.

## Description

This mod optimizes the performance of gear upgrade menus in MycoPunk by implementing caching systems for expensive calculations, throttling UI updates, and providing virtualization for large upgrade lists. It is designed to improve responsiveness when managing equipment with many upgrades or skins.

## Features

* **Upgrade Calculation Caching**: Caches upgrade grid lookups and cell adjacency calculations to avoid repeated expensive computations
* **Debounced Expensive Calculations**: Adds 5-second debouncing for upgrade equip/unequip actions to prevent performance hits during rapid bulk operations
* **Inventory Limits**: Limits upgrades and skins per gear to 256 items to prevent excessive inventory bloat, with automatic enforcement
* **Virtualized Upgrade Lists**: Automatic virtualization for upgrade/skin lists exceeding 1000 items to prevent UI performance degradation
* **UI Update Throttling**: Throttles expensive UI updates to reduce per-frame computational overhead
* **Smart Cache Management**: Implements cache invalidation and cleanup to maintain accuracy while optimizing performance

## Requirements

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible

## Installation

### Via Thunderstore (Recommended)
1. Install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)
2. Search for "PerformanceEnhancedMenu" by Sparroh
3. Download and install via the mod manager

### Manual Installation
1. Download the latest release from [GitHub](https://github.com/Little-Sparroh/PerformanceEnhancedMenu)
2. Extract the .dll file to `<MycoPunk Directory>/BepInEx/plugins/`

## Usage

The mod loads automatically when the game starts. It applies performance optimizations transparently without requiring user configuration. Check the BepInEx console for loading confirmation.

Key optimizations active:
- Cached calculations reduce lag in gear menus
- Debounced operations prevent slowdowns during rapid upgrade management
- Large inventories are virtualized for smooth scrolling
- Inventory limits are enforced automatically with notifications

## Building from Source

1. Clone this repository
2. Open the solution in Visual Studio, Rider, or your preferred C# IDE
3. Build in Release mode to generate the .dll

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

## Troubleshooting

* **Mod not loading?** Verify BepInEx is installed correctly and check logs for errors
* **Performance issues persist?** Some optimizations may be disabled if incompatible; check the changelog for known issues
* **Inventory limits causing issues?** Excess upgrades are moved to rented upgrades automatically; check your rented upgrades if items disappear
* **Cache-related problems?** The mod includes automatic cache management, but you can restart the game to clear all caches

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

## Authors

- Sparroh
- funlennysub (BepInEx template)
- [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

This project is licensed under the MIT License - see the LICENSE file for details
