using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class PlayerDataPatches
{
    // Optimize UnequipFromAll to skip the expensive operation entirely during scrapping
    // Since we're scrapping the upgrade, we don't need to unequip it from gears first
    public static class UnequipFromAllPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData), "UnequipFromAll", new[] { typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(UpgradeInstance upgrade)
        {
            // During scrapping, we can skip the expensive unequip operation
            // The upgrade will be destroyed immediately after, so unequipping is unnecessary
            // This prevents iterating through all gears and triggering cascade of UI updates
            if (upgrade != null && !upgrade.RemoveAfterMission)
            {
                // Clear caches once to be safe
                PerformanceEnhancedMenu.ClearAllCaches();
                // Skip the original expensive method for non-mission upgrades being scrapped
                return false;
            }

            return true;
        }
    }
}
