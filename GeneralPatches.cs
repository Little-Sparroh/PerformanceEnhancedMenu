using System;
using System.Reflection;
using HarmonyLib;

public static class GeneralPatches
{
    public static class EquipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "EquipUpgrade", new Type[] { typeof(UpgradeInstance), typeof(sbyte), typeof(sbyte), typeof(byte), typeof(bool) });
            return method;
        }

        public static void Postfix(PlayerData.GearData __instance, UpgradeInstance upgrade, sbyte x, sbyte y, byte rotation, bool sort)
        {
            try
            {
                if (StatCalcPatches.skipRecompute)
                {
                    return;
                }
                StatCalcPatches.ClearCaches();
                PerformanceEnhancedMenuPlugin.ClearAllCaches();
                StatCalcPatches.RecomputeTotals(__instance);
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in EquipUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
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
            try
            {
                StatCalcPatches.prismConnectedCounts.Remove(upgrade.InstanceID);
                StatCalcPatches.rarityTouchingCounts.Remove(upgrade.InstanceID);
                if (StatCalcPatches.skipRecompute)
                {
                    return;
                }
                StatCalcPatches.ClearCaches();
                PerformanceEnhancedMenuPlugin.ClearAllCaches();
                StatCalcPatches.RecomputeTotals(__instance);
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in UnequipUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static class GetEquippedUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData), "GetEquippedUpgrade", new[] { typeof(IUpgradable), typeof(int), typeof(int) });
            return method;
        }

        public static bool Prefix(IUpgradable gear, int x, int y, ref UpgradeInstance __result)
        {
            try
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
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GetEquippedUpgradePatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }
    }
}
