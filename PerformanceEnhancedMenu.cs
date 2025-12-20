using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceEnhancedMenu;

public static class PerformanceEnhancedMenu
{
    // Cache for upgrade grid lookups to avoid repeated GetEquippedUpgrade calls
    internal static Dictionary<(int gearId, int x, int y), UpgradeInstance> upgradeGridCache = new();

    // Cache for cell touching calculations
    internal static Dictionary<int, (int cellsTouching, int emptyCellsTouching, int uniqueUpgradesTouching, int numRaritiesTouching)> cellTouchingCache = new();

    // Cache for surrounding cells to avoid repeated computations
    internal static Dictionary<int, HashSet<(int x, int y)>> surroundingCellsCache = new();

    // UI optimization caches
    internal static Dictionary<GearUpgradeUI, (Vector2 position, bool borderActive)> upgradeUICache = new();
    internal static Dictionary<UpgradeEquipCell, Color> cellColorCache = new();

    // Debounce timers for expensive operations
    private static float lastUpgradeListUpdate = 0f;
    private static float lastBorderUpdate = 0f;
    private static readonly float UPDATE_THROTTLE = 0.1f; // 100ms throttle

    // Lazy calculation control
    internal static bool deferExpensiveCalculations = false;

    // Upgrade change throttling
    internal static float lastUpgradeCollectionTime = 0f;
    internal static readonly float UPGRADE_COLLECTION_THROTTLE = 0.2f; // 200ms throttle

    public static void ClearUICaches()
    {
        upgradeUICache.Clear();
        cellColorCache.Clear();
    }

    public static void ClearAllCaches()
    {
        upgradeGridCache.Clear();
        cellTouchingCache.Clear();
        surroundingCellsCache.Clear();
        ClearUICaches();
    }

    // Helper method to get cached equipped upgrade
    public static UpgradeInstance GetCachedEquippedUpgrade(IUpgradable gear, int x, int y)
    {
        // First check bounds to avoid caching invalid positions
        gear.Info.GetUpgradeGridSize(out var width, out var height);
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;

        int gearId = gear.Info.ID;
        var key = (gearId, x, y);

        if (!upgradeGridCache.TryGetValue(key, out var upgrade))
        {
            // Compute and cache the grid for this gear
            var grid = new UpgradeInstance[width, height];
            var gearData = PlayerData.GetGearData(gear);

            System.Collections.IList equippedUpgrades = StatCalcPatches.equippedUpgradesField.GetValue(gearData) as System.Collections.IList;
            if (equippedUpgrades != null)
            {
                foreach (var eq in equippedUpgrades)
                {
                    UpgradeInstance u = (UpgradeInstance)StatCalcPatches.getUpgradeMethod.Invoke(eq, null);
                    if (u != null)
                    {
                        using (var enumerator = u.GetEquippedCells(gear))
                        {
                            while (enumerator.MoveNext())
                            {
                                int cellX = enumerator.X;
                                int cellY = enumerator.Y;
                                if (cellX >= 0 && cellX < width && cellY >= 0 && cellY < height)
                                    grid[cellX, cellY] = u;
                            }
                        }
                    }
                }
            }

            // Cache all positions for this gear
            for (int gx = 0; gx < width; gx++)
            {
                for (int gy = 0; gy < height; gy++)
                {
                    upgradeGridCache[(gearId, gx, gy)] = grid[gx, gy];
                }
            }

            upgrade = grid[x, y];
        }

        return upgrade;
    }

    // Cached cell touching computation
    public static void ComputeCellTouchingStats(IUpgradable gear, UpgradeInstance upgrade)
    {
        if (cellTouchingCache.ContainsKey(upgrade.InstanceID))
            return;

        HashSet<(int x, int y)> surroundingCells;
        if (!surroundingCellsCache.TryGetValue(upgrade.InstanceID, out surroundingCells))
        {
            surroundingCells = new HashSet<(int x, int y)>();
            using (var enumerator = upgrade.GetEquippedCells(gear))
            {
                while (enumerator.MoveNext())
                {
                    int x = enumerator.X;
                    int y = enumerator.Y;
                    int offset = x % 2 == 0 ? -1 : 0;

                    // Add adjacent cells
                    surroundingCells.Add((x, y + 1));
                    surroundingCells.Add((x, y - 1));
                    surroundingCells.Add((x - 1, y + offset));
                    surroundingCells.Add((x - 1, y + 1 + offset));
                    surroundingCells.Add((x + 1, y + offset));
                    surroundingCells.Add((x + 1, y + 1 + offset));
                }
            }
            surroundingCellsCache[upgrade.InstanceID] = surroundingCells;
        }

        int cellsTouching = 0;
        int emptyCellsTouching = 0;
        HashSet<UpgradeInstance> uniqueUpgrades = new();
        HashSet<Rarity> rarities = new();

        foreach (var cell in surroundingCells)
        {
            // Check bounds first
            gear.Info.GetUpgradeGridSize(out var width, out var height);
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                continue;

            var neighbor = GetCachedEquippedUpgrade(gear, cell.x, cell.y);
            if (neighbor != null)
            {
                cellsTouching++;
                uniqueUpgrades.Add(neighbor);
                rarities.Add(neighbor.Upgrade.Rarity);
            }
            else
            {
                // Assume cells within bounds are playable (enabled)
                // This is a reasonable approximation - most grids don't have disabled cells in the middle
                emptyCellsTouching++;
            }
        }

        cellTouchingCache[upgrade.InstanceID] = (cellsTouching, emptyCellsTouching, uniqueUpgrades.Count, rarities.Count);
    }
}
