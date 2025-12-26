using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;

public static class PlayerDataPatches
{
    private const int UPGRADE_LIMIT = 256;

    public static class UnequipFromAllPatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData), "UnequipFromAll", new[] { typeof(UpgradeInstance) });
            return method;
        }

        public static bool Prefix(UpgradeInstance upgrade)
        {
            try
            {
                if (upgrade != null && !upgrade.RemoveAfterMission)
                {
                    PerformanceEnhancedMenu.ClearAllCaches();
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in UnequipFromAllPatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }
    }

    public static class CollectInstancePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData), "CollectInstance", new Type[] { typeof(UpgradeInstance), typeof(PlayerData.UnlockFlags) });
            return method;
        }

        public static bool Prefix(UpgradeInstance instance, PlayerData.UnlockFlags flags = PlayerData.UnlockFlags.None)
        {
            try
            {
                if (instance == null || instance.Upgrade == null || instance.Favorite) return true;
                IUpgradable gear = instance.Gear;
                if (gear == null) return true;
                bool isSkin = instance.Upgrade.IsSkin();
                int currentCount = GetTotalCount(gear, isSkin);
                if (currentCount >= UPGRADE_LIMIT)
                {
                    PlayerData.Instance.rentedUpgrades.Add(instance);
                    if (PlayerLook.Instance != null)
                        PlayerLook.Instance.AddTextChatMessage("Upgrade limit reached, stored in lost loot.", null);
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in CollectInstancePatch.Prefix: {e.Message}\n{e.StackTrace}");
                return true;
            }
        }
    }

    public static class OnAwakePatch
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PlayerData), "OnAwake");
            return method;
        }

        public static void Postfix()
        {
            try
            {
                EnforceUpgradeLimit();
            }
            catch (Exception e)
            {
                SparrohPlugin.Logger.LogError($"Error in OnAwakePatch.Postfix: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    static void EnforceUpgradeLimit()
    {
        try
        {
            foreach (var kvp in PlayerData.Instance.collectedGear)
            {
                IUpgradable gear = kvp.Value.Gear;
                if (gear == null) continue;
                EnforceLimitForGear(gear, false);
                EnforceLimitForGear(gear, true);
            }
        }
        catch (Exception e)
        {
            SparrohPlugin.Logger.LogError($"Error in EnforceUpgradeLimit: {e.Message}\n{e.StackTrace}");
        }
    }

    static void EnforceLimitForGear(IUpgradable gear, bool skins)
    {
        try
        {
            var list = skins ? PlayerData.GetAllSkins(gear) : PlayerData.GetAllUpgrades(gear);
            List<UpgradeInstance> allInstances = new List<UpgradeInstance>();
            foreach (var info in list)
            {
                if (info.Instances != null)
                    allInstances.AddRange(info.Instances.Where(inst => !inst.Favorite && !inst.RemoveAfterMission));
            }
            if (allInstances.Count <= UPGRADE_LIMIT) return;

            allInstances.Sort((a, b) => b.InstanceID.CompareTo(a.InstanceID));
            int excess = allInstances.Count - UPGRADE_LIMIT;
            for (int i = 0; i < excess; i++)
            {
                UpgradeInstance inst = allInstances[i];
                var info = PlayerData.GetUpgradeInfo(gear, inst.Upgrade);
                if (info.Instances != null)
                    info.Instances.Remove(inst);
                PlayerData.Instance.rentedUpgrades.Add(inst);
            }
        }
        catch (Exception e)
        {
            SparrohPlugin.Logger.LogError($"Error in EnforceLimitForGear: {e.Message}\n{e.StackTrace}");
        }
    }

    static int GetTotalCount(IUpgradable gear, bool skins)
    {
        var list = skins ? PlayerData.GetAllSkins(gear) : PlayerData.GetAllUpgrades(gear);
        int count = 0;
        foreach (var info in list)
        {
            if (info.Instances != null)
                count += info.Instances.Count(inst => !inst.RemoveAfterMission);
        }
        return count;
    }
}
