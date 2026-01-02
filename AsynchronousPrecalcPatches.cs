using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

public static class AsynchronousPrecalcPatches
{
    private static IUpgradable currentlyPrecalculating = null;

    public static class GearSlotOnPointerEnterPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearSlot), "OnPointerEnter", new[] { typeof(UnityEngine.EventSystems.PointerEventData) });
            return method;
        }

        public static void Postfix(GearSlot __instance)
        {
            try
            {
                if (__instance.Gear != null)
                {
                    StartPrecalculation(__instance.Gear);
                }
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearSlotOnPointerEnterPatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static class GearSlotOnPointerExitPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearSlot), "OnPointerExit", new[] { typeof(UnityEngine.EventSystems.PointerEventData) });
            return method;
        }

        public static void Postfix()
        {
            try
            {
                currentlyPrecalculating = null;
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearSlotOnPointerExitPatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private static void StartPrecalculation(IUpgradable gear)
    {
        if (currentlyPrecalculating == gear)
            return;

        currentlyPrecalculating = gear;

        var precalcCoroutine = PrecalculateStats(gear);
        if (Menu.Instance != null)
        {
            Menu.Instance.StartCoroutine(precalcCoroutine);
        }
    }

    private static IEnumerator PrecalculateStats(IUpgradable gear)
    {
        PerformanceEnhancedMenuPlugin.GetCachedEquippedUpgrade(gear, 0, 0);

        var gearData = PlayerData.GetGearData(gear);
        System.Collections.IList equippedUpgrades = StatCalcPatches.equippedUpgradesField.GetValue(gearData) as System.Collections.IList;

        if (equippedUpgrades != null)
        {
            var upgradeCopies = new List<object>();
            foreach (var eq in equippedUpgrades)
            {
                upgradeCopies.Add(eq);
            }

            foreach (var eq in upgradeCopies)
            {
                if (currentlyPrecalculating != gear)
                    yield break;

                UpgradeInstance upgrade = (UpgradeInstance)StatCalcPatches.getUpgradeMethod.Invoke(eq, null);
                if (upgrade != null)
                {
                    PerformanceEnhancedMenuPlugin.ComputeCellTouchingStats(gear, upgrade);
                }
                
                yield return null;
            }
        }

        if (gear is global::MiniCannon && !PerformanceEnhancedMenuPlugin.deferExpensiveCalculations)
        {
            StatCalcPatches.RecomputeTotals(gearData);
        }

        currentlyPrecalculating = null;
    }
}
