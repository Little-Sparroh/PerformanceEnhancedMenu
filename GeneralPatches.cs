using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class GeneralPatches
{
    public static class EquipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "EquipUpgrade", new Type[] { typeof(UpgradeInstance), typeof(sbyte), typeof(sbyte), typeof(byte) });
            return method;
        }

        public static void Postfix(PlayerData.GearData __instance, UpgradeInstance upgrade, sbyte x, sbyte y, byte rotation)
        {
            if (StatCalcPatches.skipRecompute)
            {
                return;
            }
            StatCalcPatches.ClearCaches();
            StatCalcPatches.RecomputeTotals(__instance);
        }
    }

    public static class UnequipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "UnequipUpgrade", new Type[] { typeof(UpgradeInstance) });
            return method;
        }

        public static void Postfix(PlayerData.GearData __instance, UpgradeInstance upgrade)
        {
            StatCalcPatches.prismConnectedCounts.Remove(upgrade.InstanceID);
            StatCalcPatches.rarityTouchingCounts.Remove(upgrade.InstanceID);
            if (StatCalcPatches.skipRecompute)
            {
                return;
            }
            StatCalcPatches.ClearCaches();
            StatCalcPatches.RecomputeTotals(__instance);
        }
    }

    public static class GetEquippedUpgradePatch
    {
        public static bool Prefix(IUpgradable gear, int x, int y, ref UpgradeInstance __result)
        {
            int gearId = gear.Info.ID;
            if (!StatCalcPatches.upgradeGridCache.TryGetValue(gearId, out var grid))
            {
                gear.Info.GetUpgradeGridSize(out var width, out var height);
                grid = new UpgradeInstance[width, height];
                var gearData = PlayerData.GetGearData(gear);
                System.Collections.IList equippedUpgrades = (System.Collections.IList)StatCalcPatches.equippedUpgradesField.GetValue(gearData);
                foreach (var eq in equippedUpgrades)
                {
                    UpgradeInstance u = (UpgradeInstance)StatCalcPatches.getUpgradeMethod.Invoke(eq, null);
                    if (u != null)
                    {
                        using (var enumerator = u.GetEquippedCells(gear))
                        {
                            while (enumerator.MoveNext())
                            {
                                grid[enumerator.X, enumerator.Y] = u;
                            }
                        }
                    }
                }
                StatCalcPatches.upgradeGridCache[gearId] = grid;
            }
            __result = grid[x, y];
            return false;
        }
    }
}
