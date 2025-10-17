using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class MiniCannonPatches
{
    public static class MiniCannonPrismPatch
    {
        public static Type Type = AccessTools.TypeByName("UpgradeProperty_MiniCannon_Prism");
        public static MethodInfo GetConnectedMethod = AccessTools.Method(Type, "GetConnectedPrismCountRecursive", new Type[] { typeof(IUpgradable), typeof(UpgradeInstance) });

        public static MethodBase TargetApply()
        {
            var method = AccessTools.Method(Type, "Apply", new[] { typeof(IGear), typeof(UpgradeInstance), typeof(Pigeon.Math.Random).MakeByRefType() });
            return method;
        }

        public static bool ApplyPrefix(object __instance, IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
        {
            MiniCannon miniCannon = gear as MiniCannon;
            if (miniCannon == null)
            {
                return true;
            }

            FieldInfo rarityField = AccessTools.Field(Type, "rarity");
            Rarity rarity = (Rarity)rarityField.GetValue(__instance);
            FieldInfo valueField = AccessTools.Field(Type, "value");
            Range<float> valueRange = (Range<float>)valueField.GetValue(__instance);

            int connected = StatCalcPatches.prismConnectedCounts.ContainsKey(upgrade.InstanceID) ? StatCalcPatches.prismConnectedCounts[upgrade.InstanceID] : (int)GetConnectedMethod.Invoke(null, new object[] { gear, upgrade });

            float num = valueRange.GetValue(ref rand);
            float multiplier = 1f + (float)connected * num;

            switch (rarity)
            {
                case Rarity.Standard:
                    miniCannon.GunData.rangeData.maxDamageRange *= multiplier;
                    break;
                case Rarity.Rare:
                    miniCannon.GunData.spreadData.spreadSize *= multiplier;
                    break;
                case Rarity.Epic:
                    miniCannon.GunData.fireInterval *= 1f / multiplier;
                    break;
                case Rarity.Exotic:
                    miniCannon.GunData.damage *= multiplier;
                    break;
            }

            StatCalcPatches.prismConnectedCounts[upgrade.InstanceID] = connected;
            return false;
        }

        public static MethodBase TargetModifyProperties()
        {
            Type refString = typeof(string).MakeByRefType();
            Type randomType = AccessTools.TypeByName("Pigeon.Math.Random");
            var method = AccessTools.Method(Type, "ModifyProperties", new[] { refString, randomType, typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool ModifyPropertiesPrefix(object __instance, ref string properties, Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
        {
            if (upgrade == null)
            {
                return true;
            }

            if (StatCalcPatches.prismConnectedCounts.TryGetValue(upgrade.InstanceID, out int connected))
            {
                FieldInfo valueField = AccessTools.Field(Type, "value");
                Range<float> valueRange = (Range<float>)valueField.GetValue(__instance);
                FieldInfo rarityField = AccessTools.Field(Type, "rarity");
                Rarity rarity = (Rarity)rarityField.GetValue(__instance);

                float num = valueRange.GetValue(ref rand);

                string statKey = rarity switch
                {
                    Rarity.Standard => "range",
                    Rarity.Rare => "spread",
                    Rarity.Epic => "firerate",
                    _ => "damage",
                };

                string stat = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { statKey });
                float multiplier = 1f + (float)connected * num;
                bool condition = connected > 0;

                object[] args = new object[] { properties, stat, multiplier, condition, StatCalcPatches.overrideMultiply };
                StatCalcPatches.addDynamicPropMethod.Invoke(null, args);
                properties = (string)args[0];

                return false;
            }

            return true;
        }
    }
}
