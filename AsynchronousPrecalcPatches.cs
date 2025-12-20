using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class AsynchronousPrecalcPatches
{
    // Track which gears are currently being pre-calculated
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
            if (__instance.Gear != null)
            {
                // Start pre-calculating stats for this gear in the background
                StartPrecalculation(__instance.Gear);
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
            // Stop pre-calculation when no longer hovering
            currentlyPrecalculating = null;
        }
    }

    private static void StartPrecalculation(IUpgradable gear)
    {
        if (currentlyPrecalculating == gear)
            return; // Already pre-calculating this gear

        currentlyPrecalculating = gear;

        // Start coroutine to pre-calculate expensive stats
        var precalcCoroutine = PrecalculateStats(gear);
        if (Menu.Instance != null)
        {
            Menu.Instance.StartCoroutine(precalcCoroutine);
        }
    }

    private static IEnumerator PrecalculateStats(IUpgradable gear)
    {
        // Pre-calculate upgrade grid
        PerformanceEnhancedMenu.GetCachedEquippedUpgrade(gear, 0, 0); // This will populate the grid cache

        // Pre-calculate all equipped upgrades' cell touching stats
        var gearData = PlayerData.GetGearData(gear);
        System.Collections.IList equippedUpgrades = StatCalcPatches.equippedUpgradesField.GetValue(gearData) as System.Collections.IList;

        if (equippedUpgrades != null)
        {
            // Create a copy of the list to avoid collection modification during enumeration
            var upgradeCopies = new List<object>();
            foreach (var eq in equippedUpgrades)
            {
                upgradeCopies.Add(eq);
            }

            foreach (var eq in upgradeCopies)
            {
                if (currentlyPrecalculating != gear)
                    yield break; // Stop if no longer hovering over this gear

                UpgradeInstance upgrade = (UpgradeInstance)StatCalcPatches.getUpgradeMethod.Invoke(eq, null);
                if (upgrade != null)
                {
                    // This will populate the cell touching cache
                    PerformanceEnhancedMenu.ComputeCellTouchingStats(gear, upgrade);
                }

                // Yield to avoid blocking the main thread too much
                yield return null;
            }
        }

        // Pre-calculate prism connectivity if this is a MiniCannon
        if (gear is global::MiniCannon)
        {
            StatCalcPatches.RecomputeTotals(gearData);
        }

        // Mark as complete
        currentlyPrecalculating = null;
    }
}
