using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PerformanceEnhancedMenu;

public static class SkinOptimizationPatches
{
    // Flag to track if skins are deferred loaded
    private static bool skinsDeferredLoaded = false;
    private static GearDetailsWindow currentWindow;

    public static class GearDetailsWindowSetupUpgradesSkinModePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "SetupUpgrades", new[] { typeof(IUpgradable), typeof(bool), typeof(bool) });
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance, IUpgradable upgradable, bool skins, bool resetScroll, ref bool __state)
        {
            if (!skins) return true; // Only modify skin mode

            currentWindow = __instance;
            skinsDeferredLoaded = false;

            // Always defer skin loading to prevent performance issues
            // Skin UI elements are more expensive than upgrade elements
            var gearData = PlayerData.GetGearData(upgradable);
            if (gearData != null)
            {
                int skinCount = upgradable.Info.SkinCount();
                Plugin.Logger.LogInfo($"[PerformanceEnhancedMenu] Gear {upgradable.Info.Name} has {skinCount} skins - deferring loading");

                __state = true; // Skip original method
                StartDeferredSkinLoading(__instance, upgradable, resetScroll);
                return false;
            }

            return true; // Fallback to original method if gear data is null
        }

        public static void Postfix(GearDetailsWindow __instance, IUpgradable upgradable, bool skins, bool resetScroll, bool __state)
        {
            if (__state && skins) // We deferred loading
            {
                // Setup minimal UI state
                SetupMinimalSkinUI(__instance, upgradable);
            }
        }
    }

    private static void StartDeferredSkinLoading(GearDetailsWindow window, IUpgradable upgradable, bool resetScroll)
    {
        if (Menu.Instance != null)
        {
            Menu.Instance.StartCoroutine(DeferredSkinLoadingCoroutine(window, upgradable, resetScroll));
        }
    }

    private static IEnumerator DeferredSkinLoadingCoroutine(GearDetailsWindow window, IUpgradable upgradable, bool resetScroll)
    {
        // Wait one frame to let UI settle
        yield return null;

        // Show loading indicator
        ShowSkinLoadingIndicator(window);

        // Wait another frame before starting heavy loading
        yield return null;

        // Now do the actual skin loading
        LoadSkinsDeferred(window, upgradable, resetScroll);

        skinsDeferredLoaded = true;
        HideSkinLoadingIndicator(window);
    }

    private static void ShowSkinLoadingIndicator(GearDetailsWindow window)
    {
        // Access the upgrade list parent via reflection
        var upgradeListParentField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListParent");
        var upgradeListParent = upgradeListParentField.GetValue(window) as RectTransform;

        if (upgradeListParent != null)
        {
            // Create a loading text in the center
            var loadingText = new GameObject("Skin Loading Indicator").AddComponent<TMPro.TextMeshProUGUI>();
            loadingText.transform.SetParent(upgradeListParent, false);
            loadingText.transform.localPosition = Vector3.zero;
            loadingText.text = "Loading skins...";
            loadingText.fontSize = 24;
            loadingText.alignment = TMPro.TextAlignmentOptions.Center;
            loadingText.color = Color.white;
            loadingText.gameObject.SetActive(true);
        }
    }

    private static void HideSkinLoadingIndicator(GearDetailsWindow window)
    {
        var upgradeListParentField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListParent");
        var upgradeListParent = upgradeListParentField.GetValue(window) as RectTransform;

        if (upgradeListParent != null)
        {
            // Find and destroy loading indicator
            for (int i = upgradeListParent.childCount - 1; i >= 0; i--)
            {
                var child = upgradeListParent.GetChild(i);
                if (child.name == "Skin Loading Indicator")
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }
    }

    private static void SetupMinimalSkinUI(GearDetailsWindow window, IUpgradable upgradable)
    {
        // Setup basic UI elements without loading all skins
        var gearData = PlayerData.GetGearData(upgradable);
        var inSkinModeField = AccessTools.Field(typeof(GearDetailsWindow), "inSkinMode");
        inSkinModeField.SetValue(window, true);

        // Update toggle button
        var skinsButtonField = AccessTools.Field(typeof(GearDetailsWindow), "skinsButton");
        var skinsButton = skinsButtonField.GetValue(window) as UnityEngine.UI.Button;
        if (skinsButton != null)
        {
            var textComponent = skinsButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "upgrades";
            }
            var triangle = skinsButton.GetComponentInChildren<Triangle>();
            if (triangle != null)
            {
                triangle.SetFlip(true);
            }
        }

        // Enable grid lock
        var enableGridLockMethod = AccessTools.Method(typeof(GearDetailsWindow), "EnableGridLock", new[] { typeof(bool), typeof(bool) });
        enableGridLockMethod?.Invoke(window, new object[] { false, false });

        // Set skins exclamation if needed
        UpdateSkinsExclamation(window, upgradable);
    }

    private static void LoadSkinsDeferred(GearDetailsWindow window, IUpgradable upgradable, bool resetScroll)
    {
        // Now call the original SetupUpgrades method for skins
        var setupUpgradesMethod = AccessTools.Method(typeof(GearDetailsWindow), "SetupUpgrades", new[] { typeof(IUpgradable), typeof(bool), typeof(bool) });
        setupUpgradesMethod?.Invoke(window, new object[] { upgradable, true, resetScroll });
    }

    private static void UpdateSkinsExclamation(GearDetailsWindow window, IUpgradable upgradable)
    {
        var skinsExclamationField = AccessTools.Field(typeof(GearDetailsWindow), "skinsExclamation");
        var skinsExclamation = skinsExclamationField.GetValue(window) as Graphic;

        if (skinsExclamation != null)
        {
            bool hasUnseenSkins = PlayerData.HasAnyUnseenSkins(upgradable);
            skinsExclamation.gameObject.SetActive(hasUnseenSkins);

            // Skins exclamation animation is handled by the original system
        }
    }

    // Patch to trigger loading when user interacts with skin area
    public static class GearDetailsWindowOnSkinAreaInteractionPatch
    {
        public static MethodBase TargetMethod()
        {
            // We'll patch Update to check for interactions
            var method = AccessTools.Method(typeof(GearDetailsWindow), "Update");
            return method;
        }

        public static void Postfix(GearDetailsWindow __instance)
        {
            if (currentWindow == __instance && !skinsDeferredLoaded)
            {
                // Check if user is scrolling or hovering in skin area
                var upgradeListScrollbarField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListScrollbar");
                var scrollbar = upgradeListScrollbarField.GetValue(__instance) as ScrollBar;

                if (scrollbar != null && scrollbar.gameObject.activeSelf)
                {
                    // If scrollbar value changed or user is hovering, trigger loading
                    var inSkinModeField = AccessTools.Field(typeof(GearDetailsWindow), "inSkinMode");
                    bool inSkinMode = (bool)inSkinModeField.GetValue(__instance);

                    if (inSkinMode && !skinsDeferredLoaded)
                    {
                        // Trigger loading immediately on first interaction
                        var upgradable = __instance.UpgradablePrefab;

                        if (upgradable != null)
                        {
                            LoadSkinsDeferred(__instance, upgradable, true);
                            skinsDeferredLoaded = true;
                            HideSkinLoadingIndicator(__instance);
                        }
                    }
                }
            }
        }
    }
}
