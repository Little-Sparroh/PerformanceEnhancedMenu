# Changelog

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
