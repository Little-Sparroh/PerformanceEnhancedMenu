# Changelog

## 2.3.1 (2026-01-02)

### Bug Fixes
* **Game Update Compatibility**: Updated EquipUpgrade method patch to handle new bool parameter added in recent game update, preventing Harmony patch failures

## 2.3.0 (2025-12-24)

### New Features
* **Virtualized Upgrade List**: Automatic virtualization for upgrade/skin lists exceeding 1000 items to prevent UI performance degradation

### Performance Improvements
* Dramatically reduced memory usage and rendering overhead for large inventories
* Smooth scrolling performance maintained regardless of inventory size

### Technical Enhancements
* New VirtualizedUpgradeList.cs with object pooling and scroll-based visibility management
* Integration into TransitionOptimizationPatches.cs for seamless activation
* Smart threshold-based activation (>1000 items) to avoid overhead for smaller inventories

## 2.2.0 (2025-12-23)

### New Features
* **Inventory Limit System**: Limits upgrades and skins per gear to 256 items to prevent excessive inventory bloat
* **Automatic Enforcement**: Excess upgrades are moved to rented upgrades on game load and during collection
* **User Feedback**: Chat message notification when upgrade limit is reached

### Technical Enhancements
* Integrated inventory limit patches into PlayerDataPatches.cs
* Added Harmony patches for PlayerData.CollectInstance and PlayerData.OnAwake methods
* Seamless integration with existing performance optimizations

## 2.1.0 (2025-12-21)

### New Features
* **Debounced Expensive Calculations**: Added 5-second debouncing for upgrade equip/unequip actions to prevent performance hits during rapid bulk operations
* **Smart Calculation Deferral**: Expensive stat recalculations (like MiniCannon prism connectivity) are deferred during rapid equip/unequip sequences
* **Automatic Resume**: Calculations resume normally after 5 seconds of inactivity

### Performance Improvements
* Prevents expensive RecomputeTotals calls during rapid upgrade management
* Reduces computational overhead when equipping/unequipping multiple upgrades quickly
* Maintains responsiveness during bulk upgrade operations
* Defers expensive prism connectivity calculations (DFS graph building) during rapid operations
* Returns safe default values for prism stats when deferring calculations

### Technical Enhancements
* New DebouncePatches.cs with Harmony patches for PlayerData.GearData equip/unequip methods
* Debounce coroutine management in PerformanceEnhancedMenu.cs
* Modified AsynchronousPrecalcPatches.cs to respect defer flags

## 2.0.0 (2025-12-20)

### Major Features
* **Complete UI Performance Overhaul**: GearDetailsWindow Update() method optimized with throttling and cached raycast results
* **Comprehensive Cell Touching Caching**: All cell adjacency calculations now cached globally, not just for specific upgrades
* **Advanced Cache Management**: Smart cache invalidation and periodic cleanup to maintain accuracy
* **Upgrade Grid Optimization**: Full grid lookups cached to prevent repeated expensive computations

### Performance Improvements
* Dramatically reduced per-frame computational overhead in gear menus
* Cached hover state detection to reduce raycasting frequency
* Throttled expensive UI updates to 10Hz instead of 60Hz
* Optimized border update loops for equipped upgrades
* Reduced pattern display recalculation frequency during rotation

### Technical Enhancements
* New PerformanceEnhancedMenu.cs core caching system
* UIOptimizationPatches.cs for UI-specific optimizations
* Expanded CellTouchingPatches.cs with global caching
* TransitionOptimizationPatches.cs for menu transition performance
* Lazy stat calculation system to defer expensive computations during setup
* AsynchronousPrecalcPatches.cs for background pre-calculation when hovering over gear
* SkinOptimizationPatches.cs for deferred loading of massive skin collections
* Automatic cache management with configurable timeouts
* Improved memory efficiency with proper cache cleanup

### Bug Fixes
* Fixed potential memory leaks from stale cache entries
* Improved cache consistency when switching between gear items
* Better handling of edge cases in upgrade grid calculations

## 1.1.0 (2025-11-26)

### Misc
* Removed Green Prism Bugfix from the code
* Tried to make it slightly more performant
* Cleaned up code for release
* Improved Thunderstore description

## 1.0.0 (2025-10-07)

### Features
* Complete rewrite of upgrade computation performance
* Cached upgrade grid lookups to avoid repeated grid scanning
* Prism connectivity caching for MiniCannon upgrades
* Rarity touching count caching for Globbler upgrades
* Optimized upgrade stat list and display property generation
* Fast globblometer count retrieval for Globbler property upgrades
* Reduced computational overhead in gear details menu

### Tech
* Add MinVer
* Add thunderstore.toml for [tcli](https://github.com/thunderstore-io/thunderstore-cli)
* Add LICENSE and CHANGELOG.md
