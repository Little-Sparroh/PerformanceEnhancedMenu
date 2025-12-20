using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceEnhancedMenu;

public static class TransitionOptimizationPatches
{
    // Cache for gear preview models to avoid recreation
    private static Dictionary<IUpgradable, GameObject> gearPreviewCache = new();

    // Cache for character preview models
    private static Dictionary<Character, GameObject> characterPreviewCache = new();

    public static class GearDetailsWindowSetupPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "Setup", new[] { typeof(IUpgradable) });
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance, IUpgradable upgradable)
        {
            // Enable lazy calculation mode to defer expensive cell touching computations
            PerformanceEnhancedMenu.deferExpensiveCalculations = true;

            // Optimize gear preview setup
            OptimizeGearPreviewSetup(__instance, upgradable);

            // Continue with original setup
            return true;
        }

        public static void Postfix(GearDetailsWindow __instance, IUpgradable upgradable)
        {
            // Disable lazy calculation mode now that setup is complete
            PerformanceEnhancedMenu.deferExpensiveCalculations = false;

            // Additional optimizations after setup
            PostSetupOptimizations(__instance, upgradable);
        }
    }

    public static class GearDetailsWindowSetupUpgradesPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "SetupUpgrades", new[] { typeof(IUpgradable), typeof(bool), typeof(bool) });
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance, IUpgradable upgradable, bool skins, bool resetScroll, ref bool __state)
        {
            // Check if we should use virtualization for large collections
            __state = ShouldUseVirtualization(__instance, upgradable, skins);
            return __state; // Return true to continue with original, false to use virtualization
        }

        public static void Postfix(GearDetailsWindow __instance, IUpgradable upgradable, bool skins, bool resetScroll, bool __state)
        {
            if (!__state) // If we skipped the original method for virtualization
            {
                // Initialize virtualization system
                InitializeVirtualization(__instance, upgradable, skins, resetScroll);
            }
        }
    }

    private static void OptimizeGearPreviewSetup(GearDetailsWindow instance, IUpgradable upgradable)
    {
        // Access characterPreview via reflection since it's private
        var characterPreviewField = AccessTools.Field(typeof(GearDetailsWindow), "characterPreview");
        var characterPreview = characterPreviewField.GetValue(instance) as Transform;

        if (characterPreview != null)
        {
            // Clear existing previews efficiently
            if (characterPreview.childCount > 0)
            {
                for (int i = characterPreview.childCount - 1; i >= 0; i--)
                {
                    var child = characterPreview.GetChild(i);
                    if (upgradable is Character character && Global.Instance.Characters[i] == upgradable)
                    {
                        // Keep this one
                        continue;
                    }
                    child.gameObject.SetActive(false);
                }
            }

            // For character previews, reuse existing objects when possible
            if (upgradable is Character charUpgradable)
            {
                bool previewExists = false;
                for (int i = 0; i < characterPreview.childCount; i++)
                {
                    var child = characterPreview.GetChild(i);
                    if (child.gameObject.activeSelf && Global.Instance.Characters[i] == upgradable)
                    {
                        previewExists = true;
                        break;
                    }
                }

                if (!previewExists)
                {
                    // Only activate the needed preview
                    if (charUpgradable.Index < characterPreview.childCount)
                    {
                        characterPreview.GetChild(charUpgradable.Index).gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private static void PostSetupOptimizations(GearDetailsWindow instance, IUpgradable upgradable)
    {
        // Defer non-critical setup operations
        instance.StartCoroutine(DelayedSetupOperations(instance, upgradable));
    }

    private static System.Collections.IEnumerator DelayedSetupOperations(GearDetailsWindow instance, IUpgradable upgradable)
    {
        // Wait a frame to let the UI settle
        yield return null;

        // Update skin materials (this can be expensive)
        var setupSkinMaterialsMethod = AccessTools.Method(typeof(GearDetailsWindow), "SetupSkinMaterials");
        setupSkinMaterialsMethod?.Invoke(instance, null);

        // Other non-critical operations can be added here
    }

    private static bool LimitInitialUpgradeCreation(GearDetailsWindow instance, IUpgradable upgradable, bool skins)
    {
        // For performance, we'll let the original method run but with optimizations
        // The real optimization comes from our cell touching caches
        return true;
    }

    private static void OptimizedSetupUpgrades(GearDetailsWindow instance, IUpgradable upgradable, bool skins, bool resetScroll)
    {
        // This would be a full reimplementation with lazy loading
        // For now, the caching optimizations are the main benefit
    }

    private static bool ShouldUseVirtualization(GearDetailsWindow instance, IUpgradable upgradable, bool skins)
    {
        // Use virtualization for large upgrade collections (>1000 upgrades)
        // This prevents UI lag when opening gear details with massive collections

        if (skins)
        {
            // For skins, check skin count
            return upgradable.Info.SkinCount() > 1000;
        }
        else
        {
            // For upgrades, check total upgrade count
            var gearData = PlayerData.GetGearData(upgradable);
            if (gearData != null)
            {
                // Include both owned upgrades and global upgrades for characters
                int totalUpgrades = upgradable.Info.UpgradeCount();
                if (upgradable is Character)
                {
                    totalUpgrades += Global.Instance.Info.UpgradeCount();
                }
                return totalUpgrades > 1000;
            }
        }

        return false;
    }

    private static void InitializeVirtualization(GearDetailsWindow instance, IUpgradable upgradable, bool skins, bool resetScroll)
    {
        // Get the upgrade list components via reflection
        var upgradeListParentField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListParent");
        var upgradeListParent = upgradeListParentField.GetValue(instance) as RectTransform;

        var upgradeListScrollbarField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListScrollbar");
        var scrollRect = upgradeListScrollbarField.GetValue(instance) as ScrollRect;

        var upgradeUIPrefabField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeUIPrefab");
        var upgradeUIPrefab = upgradeUIPrefabField.GetValue(instance) as GearUpgradeUI;

        if (upgradeListParent != null && scrollRect != null && upgradeUIPrefab != null)
        {
            // Initialize the virtualized list system
            VirtualizedUpgradeList.Initialize(instance, upgradeListParent, scrollRect);

            // Collect all upgrades
            var allUpgrades = new List<UpgradeInstance>();

            if (skins)
            {
                // Add skins
                PlayerData.SkinEnumerator skinEnumerator = new PlayerData.SkinEnumerator(upgradable);
                while (skinEnumerator.MoveNext())
                {
                    allUpgrades.Add(skinEnumerator.Upgrade);
                }
            }
            else
            {
                // Add owned upgrades
                PlayerData.UpgradeEnumerator upgradeEnumerator = new PlayerData.UpgradeEnumerator(upgradable);
                while (upgradeEnumerator.MoveNext())
                {
                    allUpgrades.Add(upgradeEnumerator.Upgrade);
                }

                // Add global upgrades for characters
                if (upgradable is Character)
                {
                    PlayerData.UpgradeEnumerator globalEnumerator = new PlayerData.UpgradeEnumerator(Global.Instance);
                    while (globalEnumerator.MoveNext())
                    {
                        allUpgrades.Add(globalEnumerator.Upgrade);
                    }
                }
            }

            // Sort upgrades (simplified sorting)
            // In a full implementation, we'd replicate the complex sorting logic from GearDetailsWindow

            // Set the data for virtualization
            VirtualizedUpgradeList.SetUpgradeData(allUpgrades, upgradeUIPrefab);

            // Update the upgrade count display
            UpdateUpgradeCountDisplay(instance, allUpgrades.Count, skins);
        }
    }

    private static void UpdateUpgradeCountDisplay(GearDetailsWindow instance, int totalCount, bool skins)
    {
        var upgradesDiscoveredTextField = AccessTools.Field(typeof(GearDetailsWindow), "upgradesDiscoveredText");
        var upgradesDiscoveredText = upgradesDiscoveredTextField.GetValue(instance) as TMPro.TextMeshProUGUI;

        if (upgradesDiscoveredText != null)
        {
            // Simplified count display - in full implementation, we'd calculate discovered vs total
            upgradesDiscoveredText.text = $"{totalCount}";
            upgradesDiscoveredText.gameObject.SetActive(true);
        }
    }

    // Clear caches when transitioning between menus
    public static void ClearTransitionCaches()
    {
        gearPreviewCache.Clear();
        characterPreviewCache.Clear();
    }
}
