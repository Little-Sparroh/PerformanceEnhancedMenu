using System.Reflection;
using HarmonyLib;

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
            try
            {
                if (StatCalcPatches.prismConnectedCounts.TryGetValue(upgrade.InstanceID, out int count))
                {
                    __result = count;
                    return false;
                }

                if (PerformanceEnhancedMenuPlugin.deferExpensiveCalculations)
                {
                    __result = 1;
                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetConnectedPrismCountRecursivePatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
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
            try
            {
                PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);

                if (PerformanceEnhancedMenuPlugin.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
                {
                    if (rarities.Equals(StatCalcPatches.rarityFlagsStandard))
                    {
                        __result = stats.cellsTouching;
                    }
                    else if (rarities.Equals(StatCalcPatches.rarityFlagsRare))
                    {
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
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetNumCellsTouchingThisPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
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
            try
            {
                PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                if (PerformanceEnhancedMenuPlugin.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
                {
                    __result = stats.numRaritiesTouching;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetNumRaritiesTouchingThisPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
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
            try
            {
                if (PerformanceEnhancedMenuPlugin.deferExpensiveCalculations)
                {
                    PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                    __result = 0;
                    return false;
                }

                PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                if (PerformanceEnhancedMenuPlugin.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
                {
                    __result = stats.cellsTouching;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetNumCellsTouchingThisNonRarityPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
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
            try
            {
                if (PerformanceEnhancedMenuPlugin.deferExpensiveCalculations)
                {
                    PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                    __result = 0;
                    return false;
                }

                PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                if (PerformanceEnhancedMenuPlugin.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
                {
                    __result = stats.emptyCellsTouching;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetNumEmptyCellsTouchingThisPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
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
            try
            {
                PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(prefab, upgrade);
                if (PerformanceEnhancedMenuPlugin.cellTouchingCache.TryGetValue(upgrade.InstanceID, out var stats))
                {
                    __result = stats.uniqueUpgradesTouching;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetNumUniqueUpgradesTouchingThisPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }
    }
}
