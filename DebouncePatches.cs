using System.Reflection;
using HarmonyLib;

public static class DebouncePatches
{
    public static class GearDataEquipUpgradePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData.GearData), "EquipUpgrade", new[] { typeof(UpgradeInstance), typeof(sbyte), typeof(sbyte), typeof(byte) });
            return method;
        }

        public static void Postfix()
        {
            try
            {
                PerformanceEnhancedMenu.StartDebounce();
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
                PerformanceEnhancedMenu.StartDebounce();
            }
            catch (System.Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in GearDataUnequipUpgradePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
