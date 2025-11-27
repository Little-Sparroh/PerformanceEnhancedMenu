using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PerformanceEnhancedMenu;

public static class GlobblerPatches
{
    public static class GlobblerRarityStatsPatch
    {
        public static Type Type = AccessTools.TypeByName("UpgradeProperty_Globbler_RarityStats");

        public static MethodBase TargetApply()
        {
            var method = AccessTools.Method(Type, "Apply", new[] { typeof(IGear), typeof(UpgradeInstance), typeof(Pigeon.Math.Random).MakeByRefType() });
            return method;
        }

        public static bool ApplyPrefix(object __instance, IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
        {
            Globbler globbler = gear as Globbler;
            if (globbler == null)
            {
                return true;
            }

            FieldInfo efficiencyField = AccessTools.Field(Type, "efficiency");
            Range<float> efficiency = (Range<float>)efficiencyField.GetValue(__instance);
            FieldInfo globblometerField = AccessTools.Field(Type, "globblometer");
            int globblometerVal = (int)globblometerField.GetValue(__instance);
            FieldInfo reloadSpeedField = AccessTools.Field(Type, "reloadSpeed");
            float reloadSpeed = (float)reloadSpeedField.GetValue(__instance);
            FieldInfo fireRateField = AccessTools.Field(Type, "fireRate");
            float fireRate = (float)fireRateField.GetValue(__instance);
            FieldInfo damageField = AccessTools.Field(Type, "damage");
            float damage = (float)damageField.GetValue(__instance);

            float value = efficiency.GetValue(ref rand);

            int std = StatCalcPatches.rarityTouchingCounts.ContainsKey(upgrade.InstanceID) ? StatCalcPatches.rarityTouchingCounts[upgrade.InstanceID].std : (int)StatCalcPatches.getNumTouchingMethod.Invoke(null, new object[] { gear, upgrade, StatCalcPatches.rarityFlagsStandard });
            int rare = StatCalcPatches.rarityTouchingCounts.ContainsKey(upgrade.InstanceID) ? StatCalcPatches.rarityTouchingCounts[upgrade.InstanceID].rare : (int)StatCalcPatches.getNumTouchingMethod.Invoke(null, new object[] { gear, upgrade, StatCalcPatches.rarityFlagsRare });
            int epic = StatCalcPatches.rarityTouchingCounts.ContainsKey(upgrade.InstanceID) ? StatCalcPatches.rarityTouchingCounts[upgrade.InstanceID].epic : (int)StatCalcPatches.getNumTouchingMethod.Invoke(null, new object[] { gear, upgrade, StatCalcPatches.rarityFlagsEpic });
            int exo = StatCalcPatches.rarityTouchingCounts.ContainsKey(upgrade.InstanceID) ? StatCalcPatches.rarityTouchingCounts[upgrade.InstanceID].exo : (int)StatCalcPatches.getNumTouchingMethod.Invoke(null, new object[] { gear, upgrade, StatCalcPatches.rarityFlagsExotic });

            globbler.GlobblerData.globblometer += Mathf.CeilToInt((float)(std * globblometerVal) * value);
            globbler.GunData.reloadDuration *= 1f / (1f + (float)rare * reloadSpeed * value);
            globbler.GunData.fireInterval *= 1f / (1f + (float)epic * fireRate * value);
            globbler.GunData.damage *= 1f + (float)exo * damage * value;

            StatCalcPatches.rarityTouchingCounts[upgrade.InstanceID] = (std, rare, epic, exo);
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

            if (StatCalcPatches.rarityTouchingCounts.TryGetValue(upgrade.InstanceID, out var counts))
            {
                FieldInfo efficiencyField = AccessTools.Field(Type, "efficiency");
                Range<float> efficiency = (Range<float>)efficiencyField.GetValue(__instance);
                FieldInfo globblometerField = AccessTools.Field(Type, "globblometer");
                int globblometerVal = (int)globblometerField.GetValue(__instance);
                FieldInfo reloadSpeedField = AccessTools.Field(Type, "reloadSpeed");
                float reloadSpeed = (float)reloadSpeedField.GetValue(__instance);
                FieldInfo fireRateField = AccessTools.Field(Type, "fireRate");
                float fireRate = (float)fireRateField.GetValue(__instance);
                FieldInfo damageField = AccessTools.Field(Type, "damage");
                float damage = (float)damageField.GetValue(__instance);

                float value = efficiency.GetValue(ref rand);

                string stat1 = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "globblometer" });
                float val1 = Mathf.CeilToInt((float)(counts.std * globblometerVal) * value);
                bool cond1 = counts.std > 0;
                object[] args1 = new object[] { properties, stat1, val1, cond1, StatCalcPatches.overrideAdd };
                StatCalcPatches.addDynamicPropMethod.Invoke(null, args1);
                properties = (string)args1[0];

                string stat2 = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "reloadspeed" });
                float val2 = 1f + (float)counts.rare * reloadSpeed * value;
                bool cond2 = counts.rare > 0;
                object[] args2 = new object[] { properties, stat2, val2, cond2, StatCalcPatches.overrideMultiply };
                StatCalcPatches.addDynamicPropMethod.Invoke(null, args2);
                properties = (string)args2[0];

                string stat3 = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "firerate" });
                float val3 = 1f + (float)counts.epic * fireRate * value;
                bool cond3 = counts.epic > 0;
                object[] args3 = new object[] { properties, stat3, val3, cond3, StatCalcPatches.overrideMultiply };
                StatCalcPatches.addDynamicPropMethod.Invoke(null, args3);
                properties = (string)args3[0];

                string stat4 = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "damage" });
                float val4 = 1f + (float)counts.exo * damage * value;
                bool cond4 = counts.exo > 0;
                object[] args4 = new object[] { properties, stat4, val4, cond4, StatCalcPatches.overrideMultiply };
                StatCalcPatches.addDynamicPropMethod.Invoke(null, args4);
                properties = (string)args4[0];

                return false;
            }

            return true;
        }
    }

    public static class GlobblerGlobblometerSpeedPatch
    {
        public static Type Type = AccessTools.TypeByName("UpgradeProperty_Globbler_GlobblometerSpeed");

        public static MethodBase TargetModifyProperties()
        {
            Type refString = typeof(string).MakeByRefType();
            Type randomType = AccessTools.TypeByName("Pigeon.Math.Random");
            var method = AccessTools.Method(Type, "ModifyProperties", new[] { refString, randomType, typeof(IUpgradable), typeof(UpgradeInstance) });
            return method;
        }

        public static bool ModifyPropertiesPrefix(object __instance, ref string properties, Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
        {
            if (!(gear is Globbler))
            {
                return true;
            }

            FieldInfo speedField = AccessTools.Field(Type, "speed");
            Range<float> speed = (Range<float>)speedField.GetValue(__instance);

            float val = speed.GetValue(ref rand);
            int count = StatCalcPatches.globblerTotalGlobblometer;

            string stat = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "add_s" });
            float addVal = (float)count * val;
            bool condition = count > 0;
            object[] args = new object[] { properties, stat, addVal, condition, StatCalcPatches.overrideAdd };
            StatCalcPatches.addDynamicPropMethod.Invoke(null, args);
            properties = (string)args[0];

            return false;
        }
    }

    public static class GlobblerGlobblometerAmmoPatch
    {
        public static Type Type = AccessTools.TypeByName("UpgradeProperty_Globbler_GlobblometerAmmo");

        public static MethodBase TargetApply()
        {
            var method = AccessTools.Method(Type, "Apply", new[] { typeof(IGear), typeof(UpgradeInstance), typeof(Pigeon.Math.Random).MakeByRefType() });
            return method;
        }

        public static bool ApplyPrefix(object __instance, IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
        {
            Globbler globbler = gear as Globbler;
            if (globbler == null)
            {
                return true;
            }

            FieldInfo ammoField = AccessTools.Field(Type, "ammo");
            Range<float> ammo = (Range<float>)ammoField.GetValue(__instance);

            float val = ammo.GetValue(ref rand);
            int count = StatCalcPatches.globblerTotalGlobblometer;

            globbler.GunData.magazineSize += Mathf.RoundToInt(val * (float)count);

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
            if (!(gear is Globbler))
            {
                return true;
            }

            FieldInfo ammoField = AccessTools.Field(Type, "ammo");
            Range<float> ammo = (Range<float>)ammoField.GetValue(__instance);

            float val = ammo.GetValue(ref rand);
            int count = StatCalcPatches.globblerTotalGlobblometer;

            string stat = (string)StatCalcPatches.textBlocksGetStringMethod.Invoke(null, new object[] { "ammo" });
            int addVal = Mathf.RoundToInt((float)count * val);
            bool condition = count > 0;
            object[] args = new object[] { properties, stat, addVal, condition, StatCalcPatches.overrideAdd };
            StatCalcPatches.addDynamicPropMethod.Invoke(null, args);
            properties = (string)args[0];

            return false;
        }
    }
}
