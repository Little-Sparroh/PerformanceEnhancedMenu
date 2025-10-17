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
public class Plugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.performanceenhancedmenu";
    public const string PluginName = "PerformanceEnhancedMenu";
    public const string PluginVersion = "1.0.0";

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
    }
}