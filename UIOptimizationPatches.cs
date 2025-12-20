using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using Pigeon.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceEnhancedMenu;

public static class UIOptimizationPatches
{
    // Cache for raycast results to reduce frequency
    private static Dictionary<string, (UpgradeEquipCell cell, float lastUpdate)> raycastCache = new();
    private static Dictionary<string, (GearUpgradeUI ui, float lastUpdate)> uiRaycastCache = new();
    private static readonly float RAYCAST_CACHE_TIME = 0.05f; // 50ms cache

    // Throttle expensive update operations
    private static float lastExpensiveUpdate = 0f;
    private static readonly float EXPENSIVE_UPDATE_THROTTLE = 0.1f; // 100ms

    public static class GearDetailsWindowUpdatePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "Update");
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance)
        {
            // Always allow pattern display and critical updates
            if (GearDetailsWindow.SelectedUpgrade != null)
                return true;

            // Check if we have a large number of upgrades that would benefit from throttling
            var upgradable = __instance.UpgradablePrefab;
            if (upgradable != null)
            {
                var gearData = PlayerData.GetGearData(upgradable);
                int upgradeCount = gearData?.EquippedUpgradeCount ?? 0;

                // Only throttle if we have many upgrades (>50)
                if (upgradeCount > 50 && Time.time - lastExpensiveUpdate < EXPENSIVE_UPDATE_THROTTLE)
                    return false; // Skip expensive update operations
            }

            lastExpensiveUpdate = Time.time;

            // Cache raycast results to avoid repeated expensive operations
            string hoverKey = "hover_" + __instance.GetHashCode();
            UpgradeEquipCell hoveredCell = null;

            if (raycastCache.TryGetValue(hoverKey, out var cached) &&
                Time.time - cached.lastUpdate < RAYCAST_CACHE_TIME)
            {
                hoveredCell = cached.cell;
            }
            else
            {
                UIRaycaster.RaycastForNearest<UpgradeEquipCell>((RectTransform)__instance.transform, out hoveredCell);
                raycastCache[hoverKey] = (hoveredCell, Time.time);
            }

            // Handle hover state changes with caching
            var lastHoveredField = AccessTools.Field(typeof(GearDetailsWindow), "lastHoveredCell");
            var currentLastHovered = (UpgradeEquipCell)lastHoveredField.GetValue(null);

            if (hoveredCell != currentLastHovered)
            {
                if (currentLastHovered != null)
                    currentLastHovered.OnHover(false);
                if (hoveredCell != null)
                    hoveredCell.OnHover(true);
                lastHoveredField.SetValue(null, hoveredCell);
            }

            return true;
        }

        public static void Postfix(GearDetailsWindow __instance)
        {
            // Clear old cache entries periodically
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                ClearOldCacheEntries();
            }

            // Clear raycast cache when upgrade selection changes to ensure proper hover detection
            if (GearDetailsWindow.SelectedUpgrade != null)
            {
                string hoverKey = "hover_" + __instance.GetHashCode();
                raycastCache.Remove(hoverKey);
            }
        }
    }

    public static class GearDetailsWindowSetupUpgradesPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "SetupUpgrades", new[] { typeof(IUpgradable), typeof(bool), typeof(bool) });
            return method;
        }

        public static void Postfix(IUpgradable upgradable, bool skins, bool resetScroll)
        {
            // Clear UI caches when upgrades change
            PerformanceEnhancedMenu.ClearUICaches();
        }
    }

    public static class GearDetailsWindowOnUpgradesChangedPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "OnUpgradesChanged");
            return method;
        }

        public static void Postfix()
        {
            // Clear caches when upgrades change
            PerformanceEnhancedMenu.ClearAllCaches();
        }
    }

    // Clear raycast cache and force Update method to run immediately when selecting upgrades
    public static class GearDetailsWindowSelectUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "SelectUpgrade", new[] { typeof(GearUpgradeUI) });
            return method;
        }

        public static void Postfix(GearDetailsWindow __instance)
        {
            // Clear raycast cache when selecting upgrades to ensure fresh hover detection
            string hoverKey = "hover_" + __instance.GetHashCode();
            raycastCache.Remove(hoverKey);

            // Force the Update method to run immediately to position the pattern display
            if (GearDetailsWindow.SelectedUpgrade != null)
            {
                var updateMethod = AccessTools.Method(typeof(GearDetailsWindow), "Update");
                updateMethod.Invoke(__instance, null);
            }
        }
    }

    // Force stat recalculation when upgrade UI is hovered (lazy loading)
    public static class GearUpgradeUIOnHoverPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearUpgradeUI), "OnHover", new[] { typeof(bool) });
            return method;
        }

        public static void Postfix(GearUpgradeUI __instance, bool hover)
        {
            if (hover && __instance.Upgrade != null)
            {
                // Force recalculation of cell touching stats when upgrade is hovered
                // This ensures stats are accurate when the player actually looks at them
                var gearDetailsWindow = __instance.GetComponentInParent<GearDetailsWindow>();
                if (gearDetailsWindow != null)
                {
                    PerformanceEnhancedMenu.ComputeCellTouchingStats(gearDetailsWindow.UpgradablePrefab, __instance.Upgrade);
                }
            }
        }
    }

    // Disable shimmer effect for unseen upgrades
    public static class GearUpgradeUISetUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearUpgradeUI), "SetUpgrade", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static void Postfix(GearUpgradeUI __instance, UpgradeInstance upgrade, bool isPreview)
        {
            if (upgrade == null || isPreview)
                return;

            // Disable shimmer effect - always use normal material
            var rarityData = Global.GetRarity(upgrade.Upgrade.Rarity);

            // Access private button field via reflection
            var buttonField = AccessTools.Field(typeof(GearUpgradeUI), "button");
            var button = buttonField.GetValue(__instance) as DefaultButton;

            if (button != null && button.MainGraphic != null)
            {
                button.MainGraphic.material = rarityData.uiMat;
            }
        }
    }

    // Flag to track when we're in a scrapping operation
    private static UpgradeInstance currentlyScrappingUpgrade = null;

    // Optimize upgrade collection/destruction event handling
    public static class GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "OnUpgradeCollectedOrDestroyed", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance, UpgradeInstance upgrade, bool collected)
        {
            if (!collected) // scrapping operation
            {
                currentlyScrappingUpgrade = upgrade;

                // Skip the expensive SetupUpgrades call and manually remove the UI element
                var upgradeUIsField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeUIs");
                var upgradeUIs = upgradeUIsField.GetValue(null) as List<GearUpgradeUI>;

                if (upgradeUIs != null)
                {
                    for (int i = 0; i < upgradeUIs.Count; i++)
                    {
                        var ui = upgradeUIs[i];
                        if (ui != null && ui.Upgrade == upgrade && ui.gameObject.activeSelf)
                        {
                            ui.gameObject.SetActive(false);
                            break;
                        }
                    }
                }

                // Clear caches immediately
                PerformanceEnhancedMenu.ClearAllCaches();

                // Skip the expensive SetupUpgrades call - UI will be rebuilt naturally on next action
                return false; // Skip the original method
            }

            currentlyScrappingUpgrade = null;
            return true; // Allow normal processing for collecting
        }


    }

    // Optimize upgrade collection/destruction event handling for OuroGearWindow
    public static class OuroGearWindowOnUpgradeCollectedOrDestroyedPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(OuroGearWindow), "OnUpgradeCollectedOrDestroyed", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static bool Prefix(OuroGearWindow __instance, UpgradeInstance upgrade, bool collected)
        {
            if (!collected) // scrapping operation
            {
                currentlyScrappingUpgrade = upgrade;

                // Skip the expensive SetupUpgrades call and manually remove the UI element
                var upgradeUIsField = AccessTools.Field(typeof(OuroGearWindow), "upgradeUIs");
                var upgradeUIs = upgradeUIsField.GetValue(null) as List<GearUpgradeUI>;

                if (upgradeUIs != null)
                {
                    for (int i = 0; i < upgradeUIs.Count; i++)
                    {
                        var ui = upgradeUIs[i];
                        if (ui != null && ui.Upgrade == upgrade && ui.gameObject.activeSelf)
                        {
                            ui.gameObject.SetActive(false);
                            break;
                        }
                    }
                }

                // Clear caches immediately
                PerformanceEnhancedMenu.ClearAllCaches();

                // Skip the expensive SetupUpgrades call - UI will be rebuilt naturally on next action
                return false; // Skip the original method
            }

            currentlyScrappingUpgrade = null;
            return true; // Allow normal processing for collecting
        }


    }



    // Note: Border update optimization removed due to private field access issues
    // The core caching system in PerformanceEnhancedMenu.cs still provides significant benefits

    // Optimize pattern display updates during rotation
    public static class GearDetailsWindowSetSelectedUpgradeRotationPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "SetSelectedUpgradeRotation", new[] { typeof(int), typeof(bool) });
            return method;
        }

        public static bool Prefix(ref bool fromAction)
        {
            // Allow immediate updates for user-initiated actions (like selecting upgrades)
            if (fromAction)
                return true;

            // For automatic rotation updates (not from user input), throttle
            if (Time.time - lastExpensiveUpdate < EXPENSIVE_UPDATE_THROTTLE)
                return false;

            return true;
        }
    }

    private static void ClearOldCacheEntries()
    {
        float currentTime = Time.time;
        var keysToRemove = new List<string>();

        foreach (var kvp in raycastCache)
        {
            if (currentTime - kvp.Value.lastUpdate > RAYCAST_CACHE_TIME * 2)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
            raycastCache.Remove(key);

        keysToRemove.Clear();

        foreach (var kvp in uiRaycastCache)
        {
            if (currentTime - kvp.Value.lastUpdate > RAYCAST_CACHE_TIME * 2)
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
            uiRaycastCache.Remove(key);
    }
}
