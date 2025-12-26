using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.enhancedmenuperformance";
    public const string PluginName = "EnhancedMenuPerformance";
    public const string PluginVersion = "2.2.0";

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        var harmony = new Harmony(PluginGUID);

        MethodBase method;

        method = GeneralPatches.EquipUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(GeneralPatches.EquipUpgradePatch),
                    nameof(GeneralPatches.EquipUpgradePatch.Postfix)));
        }

        method = GeneralPatches.UnequipUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(GeneralPatches.UnequipUpgradePatch),
                    nameof(GeneralPatches.UnequipUpgradePatch.Postfix)));
        }

        method = GeneralPatches.GetEquippedUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(GeneralPatches.GetEquippedUpgradePatch),
                    nameof(GeneralPatches.GetEquippedUpgradePatch.Prefix)));
        }

        method = TransitionOptimizationPatches.GearDetailsWindowSetupPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch.Prefix)),
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch.Postfix)));
        }

        method = UIOptimizationPatches.GearUpgradeUISetUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearUpgradeUISetUpgradePatch),
                    nameof(UIOptimizationPatches.GearUpgradeUISetUpgradePatch.Postfix)));
        }

        method = UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch.Prefix)));
        }

        method = UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch),
                    nameof(UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch.Prefix)));
        }

        method = DebouncePatches.GearDataEquipUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(DebouncePatches.GearDataEquipUpgradePatch),
                    nameof(DebouncePatches.GearDataEquipUpgradePatch.Postfix)));
        }

        method = DebouncePatches.GearDataUnequipUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(DebouncePatches.GearDataUnequipUpgradePatch),
                    nameof(DebouncePatches.GearDataUnequipUpgradePatch.Postfix)));
        }

        method = PlayerDataPatches.UnequipFromAllPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(PlayerDataPatches.UnequipFromAllPatch),
                    nameof(PlayerDataPatches.UnequipFromAllPatch.Prefix)));
        }

        method = PlayerDataPatches.CollectInstancePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(PlayerDataPatches.CollectInstancePatch),
                    nameof(PlayerDataPatches.CollectInstancePatch.Prefix)));
        }

        method = PlayerDataPatches.OnAwakePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(PlayerDataPatches.OnAwakePatch),
                    nameof(PlayerDataPatches.OnAwakePatch.Postfix)));
        }

        Logger.LogInfo($"{PluginName} loaded");
    }
}
