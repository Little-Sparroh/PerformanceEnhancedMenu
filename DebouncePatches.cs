using System.Reflection;
using HarmonyLib;

public static class DebouncePatches
{
    public static class GearDataEquipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "EquipUpgrade", new[] { typeof(UpgradeInstance), typeof(sbyte), typeof(sbyte), typeof(byte), typeof(bool) });
            return method;
        }

        public static void Postfix(PlayerData.GearData __instance, UpgradeInstance upgrade, sbyte x, sbyte y, byte rotation, bool sort)
        {
            try
            {
                PerformanceEnhancedMenuPlugin.StartDebounce();
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDataEquipUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static class GearDataUnequipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "UnequipUpgrade", new[] { typeof(UpgradeInstance) });
            return method;
        }

        public static void Postfix()
        {
            try
            {
                PerformanceEnhancedMenuPlugin.StartDebounce();
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDataUnequipUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
