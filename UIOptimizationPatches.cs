using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.UI;

public static class UIOptimizationPatches
{
    private static Dictionary<string, (UpgradeEquipCell cell, float lastUpdate)> raycastCache = new();
    private static Dictionary<string, (GearUpgradeUI ui, float lastUpdate)> uiRaycastCache = new();
    private static readonly float RAYCAST_CACHE_TIME = 0.05f;

    private static float lastExpensiveUpdate = 0f;
    private static readonly float EXPENSIVE_UPDATE_THROTTLE = 0.1f;

    public static class GearUpgradeUISetUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearUpgradeUI), "SetUpgrade", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static void Postfix(GearUpgradeUI __instance, UpgradeInstance upgrade, bool isPreview)
        {
            try
            {
                if (upgrade == null || isPreview)
                    return;

                var rarityData = Global.GetRarity(upgrade.Upgrade.Rarity);

                var buttonField = AccessTools.Field(typeof(GearUpgradeUI), "button");
                var button = buttonField.GetValue(__instance) as DefaultButton;

                if (button != null && button.MainGraphic != null)
                {
                    button.MainGraphic.material = rarityData.uiMat;
                }
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearUpgradeUISetUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private static UpgradeInstance currentlyScrappingUpgrade = null;

    public static class GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(GearDetailsWindow), "OnUpgradeCollectedOrDestroyed", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static bool Prefix(GearDetailsWindow __instance, UpgradeInstance upgrade, bool collected)
        {
            try
            {
                if (!collected)
                {
                    currentlyScrappingUpgrade = upgrade;

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

                    PerformanceEnhancedMenuPlugin.ClearAllCaches();

                    return false;
                }

                currentlyScrappingUpgrade = null;
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }


    }

    public static class OuroGearWindowOnUpgradeCollectedOrDestroyedPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(OuroGearWindow), "OnUpgradeCollectedOrDestroyed", new[] { typeof(UpgradeInstance), typeof(bool) });
            return method;
        }

        public static bool Prefix(OuroGearWindow __instance, UpgradeInstance upgrade, bool collected)
        {
            try
            {
                if (!collected)
                {
                    currentlyScrappingUpgrade = upgrade;

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

                    PerformanceEnhancedMenuPlugin.ClearAllCaches();

                    return false;
                }

                currentlyScrappingUpgrade = null;
                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in OuroGearWindowOnUpgradeCollectedOrDestroyedPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }


    }
}
