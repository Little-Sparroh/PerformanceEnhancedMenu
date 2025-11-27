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
            if (StatCalcPatches.rarityTouchingCounts.TryGetValue(upgrade.InstanceID, out var counts))
            {
                if (rarities.Equals(StatCalcPatches.rarityFlagsStandard))
                {
                    __result = counts.std;
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsRare))
                {
                    __result = counts.rare;
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsEpic))
                {
                    __result = counts.epic;
                }
                else if (rarities.Equals(StatCalcPatches.rarityFlagsExotic))
                {
                    __result = counts.exo;
                }
                else
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }

    public static class GetNumRaritiesTouchingThisPatch
    {
        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            if (StatCalcPatches.numRaritiesTouching.TryGetValue(upgrade.InstanceID, out int count))
            {
                __result = count;
                return false;
            }
            return true;
        }
    }

    public static class GetNumCellsTouchingThisNonRarityPatch
    {
        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            if (StatCalcPatches.numCellsTouching.TryGetValue(upgrade.InstanceID, out int count))
            {
                __result = count;
                return false;
            }
            return true;
        }
    }

    public static class GetNumEmptyCellsTouchingThisPatch
    {
        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            if (StatCalcPatches.numEmptyCellsTouching.TryGetValue(upgrade.InstanceID, out int count))
            {
                __result = count;
                return false;
            }
            return true;
        }
    }

    public static class GetNumUniqueUpgradesTouchingThisPatch
    {
        public static bool Prefix(IUpgradable prefab, UpgradeInstance upgrade, ref int __result)
        {
            if (StatCalcPatches.numUniqueUpgradesTouching.TryGetValue(upgrade.InstanceID, out int count))
            {
                __result = count;
                return false;
            }
            return true;
        }
    }
}
