using System.Collections;
using System.Reflection;
using HarmonyLib;
using Pigeon.UI;
using UnityEngine;
using UnityEngine.UI;

public static class SkinOptimizationPatches
{
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
            try
            {
                if (!skins) return true;

                currentWindow = __instance;
                skinsDeferredLoaded = false;

                var gearData = PlayerData.GetGearData(upgradable);
                if (gearData != null)
                {
                    int skinCount = upgradable.Info.SkinCount();

                    __state = true;
                    StartDeferredSkinLoading(__instance, upgradable, resetScroll);
                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowSetupUpgradesSkinModePatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }

        public static void Postfix(GearDetailsWindow __instance, IUpgradable upgradable, bool skins, bool resetScroll, bool __state)
        {
            try
            {
                if (__state && skins)
                {
                    SetupMinimalSkinUI(__instance, upgradable);
                }
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowSetupUpgradesSkinModePatch.Postfix: {e.Message}\n{e.StackTrace}");
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
        yield return null;

        ShowSkinLoadingIndicator(window);

        yield return null;

        LoadSkinsDeferred(window, upgradable, resetScroll);

        skinsDeferredLoaded = true;
        HideSkinLoadingIndicator(window);
    }

    private static void ShowSkinLoadingIndicator(GearDetailsWindow window)
    {
        var upgradeListParentField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListParent");
        var upgradeListParent = upgradeListParentField.GetValue(window) as RectTransform;

        if (upgradeListParent != null)
        {
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
        var gearData = PlayerData.GetGearData(upgradable);
        var inSkinModeField = AccessTools.Field(typeof(GearDetailsWindow), "inSkinMode");
        inSkinModeField.SetValue(window, true);

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

        var enableGridLockMethod = AccessTools.Method(typeof(GearDetailsWindow), "EnableGridLock", new[] { typeof(bool), typeof(bool) });
        enableGridLockMethod?.Invoke(window, new object[] { false, false });

        UpdateSkinsExclamation(window, upgradable);
    }

    private static void LoadSkinsDeferred(GearDetailsWindow window, IUpgradable upgradable, bool resetScroll)
    {
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

        }
    }

    public static class GearDetailsWindowOnSkinAreaInteractionPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "Update");
            return method;
        }

        public static void Postfix(GearDetailsWindow __instance)
        {
            try
            {
                if (currentWindow == __instance && !skinsDeferredLoaded)
                {
                    var upgradeListScrollbarField = AccessTools.Field(typeof(GearDetailsWindow), "upgradeListScrollbar");
                    var scrollbar = upgradeListScrollbarField.GetValue(__instance) as ScrollBar;

                    if (scrollbar != null && scrollbar.gameObject.activeSelf)
                    {
                        var inSkinModeField = AccessTools.Field(typeof(GearDetailsWindow), "inSkinMode");
                        bool inSkinMode = (bool)inSkinModeField.GetValue(__instance);

                        if (inSkinMode && !skinsDeferredLoaded)
                        {
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
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowOnSkinAreaInteractionPatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
