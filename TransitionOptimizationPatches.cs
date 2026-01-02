using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class TransitionOptimizationPatches
{
    private static Dictionary<IUpgradable, GameObject> gearPreviewCache = new();

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
            try
            {
                PerformanceEnhancedMenuPlugin.deferExpensiveCalculations = true;

                OptimizeGearPreviewSetup(__instance, upgradable);

                return true;
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowSetupPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }

        public static void Postfix(GearDetailsWindow __instance, IUpgradable upgradable)
        {
            try
            {
                PerformanceEnhancedMenuPlugin.deferExpensiveCalculations = false;

                PostSetupOptimizations(__instance, upgradable);
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDetailsWindowSetupPatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private static void OptimizeGearPreviewSetup(GearDetailsWindow instance, IUpgradable upgradable)
    {
        var characterPreviewField = AccessTools.Field(typeof(GearDetailsWindow), "characterPreview");
        var characterPreview = characterPreviewField.GetValue(instance) as Transform;

        if (characterPreview != null)
        {
            if (characterPreview.childCount > 0)
            {
                for (int i = characterPreview.childCount - 1; i >= 0; i--)
                {
                    var child = characterPreview.GetChild(i);
                    if (upgradable is Character character && Global.Instance.Characters[i] == upgradable)
                    {
                        continue;
                    }
                    child.gameObject.SetActive(false);
                }
            }

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
        instance.StartCoroutine(DelayedSetupOperations(instance, upgradable));
    }

    private static System.Collections.IEnumerator DelayedSetupOperations(GearDetailsWindow instance, IUpgradable upgradable)
    {
        yield return null;

        var setupSkinMaterialsMethod = AccessTools.Method(typeof(GearDetailsWindow), "SetupSkinMaterials");
        setupSkinMaterialsMethod?.Invoke(instance, null);

    }
}
