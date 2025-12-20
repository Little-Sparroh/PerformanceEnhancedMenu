using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace PerformanceEnhancedMenu;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.performanceenhancedmenu";
    public const string PluginName = "PerformanceEnhancedMenu";
    public const string PluginVersion = "2.0.0";

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        var harmony = new Harmony(PluginGUID);

        MethodBase method;

        // General patches
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

        /*
        // Cell touching patches - commented out due to incorrect cached values
        method = CellTouchingPatches.GetConnectedPrismCountRecursivePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetConnectedPrismCountRecursivePatch),
                    nameof(CellTouchingPatches.GetConnectedPrismCountRecursivePatch.Prefix)));
        }

        method = CellTouchingPatches.GetNumCellsTouchingThisPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetNumCellsTouchingThisPatch),
                    nameof(CellTouchingPatches.GetNumCellsTouchingThisPatch.Prefix)));
        }

        method = CellTouchingPatches.GetNumRaritiesTouchingThisPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetNumRaritiesTouchingThisPatch),
                    nameof(CellTouchingPatches.GetNumRaritiesTouchingThisPatch.Prefix)));
        }

        method = CellTouchingPatches.GetNumCellsTouchingThisNonRarityPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetNumCellsTouchingThisNonRarityPatch),
                    nameof(CellTouchingPatches.GetNumCellsTouchingThisNonRarityPatch.Prefix)));
        }

        method = CellTouchingPatches.GetNumEmptyCellsTouchingThisPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetNumEmptyCellsTouchingThisPatch),
                    nameof(CellTouchingPatches.GetNumEmptyCellsTouchingThisPatch.Prefix)));
        }

        method = CellTouchingPatches.GetNumUniqueUpgradesTouchingThisPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(CellTouchingPatches.GetNumUniqueUpgradesTouchingThisPatch),
                    nameof(CellTouchingPatches.GetNumUniqueUpgradesTouchingThisPatch.Prefix)));
        }
        */
        
        /*
        // UI optimization patches - re-enabled with fixes for pattern display
        method = UIOptimizationPatches.GearDetailsWindowUpdatePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowUpdatePatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowUpdatePatch.Prefix)),
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowUpdatePatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowUpdatePatch.Postfix)));
        }

        method = UIOptimizationPatches.GearDetailsWindowSetupUpgradesPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowSetupUpgradesPatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowSetupUpgradesPatch.Postfix)));
        }

        method = UIOptimizationPatches.GearDetailsWindowOnUpgradesChangedPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowOnUpgradesChangedPatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowOnUpgradesChangedPatch.Postfix)));
        }

        method = UIOptimizationPatches.GearDetailsWindowSelectUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowSelectUpgradePatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowSelectUpgradePatch.Postfix)));
        }

        // Rotation patch - only throttle automatic rotations, not user-initiated ones
        method = UIOptimizationPatches.GearDetailsWindowSetSelectedUpgradeRotationPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowSetSelectedUpgradeRotationPatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowSetSelectedUpgradeRotationPatch.Prefix)));
        }
        */

        // Transition optimization patches
        method = TransitionOptimizationPatches.GearDetailsWindowSetupPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch.Prefix)),
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupPatch.Postfix)));
        }

        // Virtualization patches disabled - core optimizations are sufficient
        // These can be re-enabled with proper testing for edge cases
        /*
        method = TransitionOptimizationPatches.GearDetailsWindowSetupUpgradesPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupUpgradesPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupUpgradesPatch.Prefix)),
                new HarmonyMethod(typeof(TransitionOptimizationPatches.GearDetailsWindowSetupUpgradesPatch),
                    nameof(TransitionOptimizationPatches.GearDetailsWindowSetupUpgradesPatch.Postfix)));
        }
        */

        // Lazy calculation trigger patch
        method = UIOptimizationPatches.GearUpgradeUIOnHoverPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearUpgradeUIOnHoverPatch),
                    nameof(UIOptimizationPatches.GearUpgradeUIOnHoverPatch.Postfix)));
        }

        // Disable shimmer effect for new upgrades
        method = UIOptimizationPatches.GearUpgradeUISetUpgradePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearUpgradeUISetUpgradePatch),
                    nameof(UIOptimizationPatches.GearUpgradeUISetUpgradePatch.Postfix)));
        }

        // Optimize upgrade collection/destruction event handling
        method = UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch),
                    nameof(UIOptimizationPatches.GearDetailsWindowOnUpgradeCollectedOrDestroyedPatch.Prefix)));
        }

        // Optimize upgrade collection/destruction event handling for OuroGearWindow
        method = UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch),
                    nameof(UIOptimizationPatches.OuroGearWindowOnUpgradeCollectedOrDestroyedPatch.Prefix)));
        }



        /*
        // Asynchronous pre-calculation patches - commented out as they cause cached values to be incorrect
        method = AsynchronousPrecalcPatches.GearSlotOnPointerEnterPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(AsynchronousPrecalcPatches.GearSlotOnPointerEnterPatch),
                    nameof(AsynchronousPrecalcPatches.GearSlotOnPointerEnterPatch.Postfix)));
        }

        method = AsynchronousPrecalcPatches.GearSlotOnPointerExitPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(AsynchronousPrecalcPatches.GearSlotOnPointerExitPatch),
                    nameof(AsynchronousPrecalcPatches.GearSlotOnPointerExitPatch.Postfix)));
        }
        */

        /*
        // Player data optimization patches
        method = PlayerDataPatches.UnequipFromAllPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(PlayerDataPatches.UnequipFromAllPatch),
                    nameof(PlayerDataPatches.UnequipFromAllPatch.Prefix)));
        }
        */

        // Skin optimization patches - disabled due to functionality issues
        // The deferred loading prevents skins from loading entirely
        /*
        method = SkinOptimizationPatches.GearDetailsWindowSetupUpgradesSkinModePatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method,
                new HarmonyMethod(typeof(SkinOptimizationPatches.GearDetailsWindowSetupUpgradesSkinModePatch),
                    nameof(SkinOptimizationPatches.GearDetailsWindowSetupUpgradesSkinModePatch.Prefix)),
                new HarmonyMethod(typeof(SkinOptimizationPatches.GearDetailsWindowSetupUpgradesSkinModePatch),
                    nameof(SkinOptimizationPatches.GearDetailsWindowSetupUpgradesSkinModePatch.Postfix)));
        }

        method = SkinOptimizationPatches.GearDetailsWindowOnSkinAreaInteractionPatch.TargetMethod();
        if (method != null)
        {
            harmony.Patch(method, null,
                new HarmonyMethod(typeof(SkinOptimizationPatches.GearDetailsWindowOnSkinAreaInteractionPatch),
                    nameof(SkinOptimizationPatches.GearDetailsWindowOnSkinAreaInteractionPatch.Postfix)));
        }
        */

        Logger.LogInfo("PerformanceEnhancedMenu v2.0.0 loaded successfully with comprehensive optimizations!");
    }
}
