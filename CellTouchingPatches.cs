using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class CellTouchingPatches
{
    public static class GetConnectedPrismCountRecursivePatch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UpgradeProperty_MiniCannon_Prism");
            var method = AccessTools.Method(type, "GetConnectedPrismCountRecursive", new[] { typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            if (StatCalcPatches.prismConnectedCounts.TryGetValue(upgrade.InstanceID, out int count))
            {
                __result = count;
                return false;
            }
            return true;
        }
    }

    public static class GetNumCellsTouchingThisPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(UpgradeProperty), "GetNumCellsTouchingThis", new[] { typeof(IUpgradable), typeof(UpgradeInstance), StatCalcPatches.rarityFlagsType });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, object rarities, ref int __result)
        {
            // Use the new comprehensive caching system
            PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);

            if (PerformanceEnhancedMenu.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
            {
                if (rarities.Equals(StatCalcPatches.rarityFlagsStandard))
                {
                    __result = stats.cellsTouching; // For now, return total - will need to filter by rarity if needed
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsRare))
                {
                    // This would need more complex logic to filter by rarity
                    // For performance, we'll fall back to original for now
                    return true;
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsEpic))
                {
                    return true;
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsExotic))
                {
                    return true;
                }
                else
                {
                    __result = stats.cellsTouching;
                    return false;
                }
                return false;
            }
            return true;
        }
    }

    public static class GetNumRaritiesTouchingThisPatch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UpgradeProperty");
            var method = AccessTools.Method(type, "GetNumRaritiesTouchingThis", new[] { typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
            if (PerformanceEnhancedMenu.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
            {
                __result = stats.numRaritiesTouching;
                return false;
            }
            return true;
        }
    }

    public static class GetNumCellsTouchingThisNonRarityPatch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UpgradeProperty");
            var method = AccessTools.Method(type, "GetNumCellsTouchingThis", new[] { typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            // If deferring expensive calculations, return 0 and mark for later computation
            if (PerformanceEnhancedMenu.deferExpensiveCalculations)
            {
                // Queue this upgrade for lazy calculation
                PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
                __result = 0; // Return default value during setup
                return false;
            }

            PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
            if (PerformanceEnhancedMenu.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
            {
                __result = stats.cellsTouching;
                return false;
            }
            return true;
        }
    }

    public static class GetNumEmptyCellsTouchingThisPatch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UpgradeProperty");
            var method = AccessTools.Method(type, "GetNumEmptyCellsTouchingThis", new[] { typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            // If deferring expensive calculations, return 0 and mark for later computation
            if (PerformanceEnhancedMenu.deferExpensiveCalculations)
            {
                PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
                __result = 0; // Return default value during setup
                return false;
            }

            PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
            if (PerformanceEnhancedMenu.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
            {
                __result = stats.emptyCellsTouching;
                return false;
            }
            return true;
        }
    }

    public static class GetNumUniqueUpgradesTouchingThisPatch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UpgradeProperty");
            var method = AccessTools.Method(type, "GetNumUniqueUpgradesTouchingThis", new[] { typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            PerformanceEnhancedMenu.ComputeCellTouchingStats(prefab, upgrade);
            if (PerformanceEnhancedMenu.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
            {
                __result = stats.uniqueUpgradesTouching;
                return false;
            }
            return true;
        }
    }
}
