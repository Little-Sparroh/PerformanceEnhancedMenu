using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

public static class StatCalcPatches
{
    internal static Dictionary<long, string> statListCache = new();
    internal static Dictionary<long, string> displayPropertiesCache = new();
    internal static string tempOldProperties;

    internal static Dictionary<int, int> prismConnectedCounts = new();
    internal static Dictionary<int, (int std, int rare, int epic, int exo)> rarityTouchingCounts = new();
    internal static Dictionary<int, UpgradeInstance[,]> upgradeGridCache = new();

    internal static Type rarityFlagsType;
    internal static object rarityFlagsStandard;
    internal static object rarityFlagsRare;
    internal static object rarityFlagsEpic;
    internal static object rarityFlagsExotic;
    internal static MethodInfo getNumTouchingMethod;
    internal static Type iGlobblometerPropertyType;
    internal static MethodInfo modifyGlobblometerMethod;
    internal static Type upgradeEquipDataType;
    internal static MethodInfo getUpgradeMethod;
    internal static FieldInfo equippedUpgradesField = null;

    internal static bool skipRecompute = false;

    static StatCalcPatches()
    {
        try
        {
            rarityFlagsType = AccessTools.TypeByName("RarityFlags");
            rarityFlagsStandard = Enum.Parse(rarityFlagsType, "Standard");
            rarityFlagsRare = Enum.Parse(rarityFlagsType, "Rare");
            rarityFlagsEpic = Enum.Parse(rarityFlagsType, "Epic");
            rarityFlagsExotic = Enum.Parse(rarityFlagsType, "Exotic");
            getNumTouchingMethod = AccessTools.Method(typeof(UpgradeProperty), "GetNumCellsTouchingThis", new Type[] { typeof(IUpgradable), typeof(UpgradeInstance), rarityFlagsType });
            iGlobblometerPropertyType = AccessTools.TypeByName("IGlobblometerProperty");
            modifyGlobblometerMethod = AccessTools.Method(iGlobblometerPropertyType, "ModifyGlobblometer", new Type[] { typeof(int).MakeByRefType(), typeof(Pigeon.Math.Random), typeof(IUpgradable), typeof(UpgradeInstance) });
            upgradeEquipDataType = AccessTools.Inner(typeof(PlayerData), "UpgradeEquipData");
            getUpgradeMethod = AccessTools.Method(upgradeEquipDataType, "GetUpgrade");
            equippedUpgradesField = AccessTools.Field(typeof(PlayerData.GearData), "equippedUpgrades");
        }
        catch (Exception e)
        {
            SparrohPlugin.Logger.LogError($"Error initializing StatCalcPatches: {e.Message}\n{e.StackTrace}");
        }
    }

    internal static void RecomputeTotals(PlayerData.GearData gearData)
    {
        try
        {
            if (PerformanceEnhancedMenuPlugin.deferExpensiveCalculations)
            {
                return;
            }

            rarityTouchingCounts.Clear();

            if (gearData.Gear is Globbler)
            {
                int total = 0;

                List<UpgradeInstance> needingRarityComputation = new List<UpgradeInstance>();

                System.Collections.IList equippedUpgradesList = (System.Collections.IList)equippedUpgradesField.GetValue(gearData);
                List<object> tempList = new List<object>();
                foreach (var eq in equippedUpgradesList)
                {
                    tempList.Add(eq);
                }

                foreach (var eq in tempList)
                {
                    UpgradeInstance u = (UpgradeInstance)getUpgradeMethod.Invoke(eq, null);
                    Pigeon.Math.Random rand = new Pigeon.Math.Random(u.Seed);

                    bool hasRarityStats = false;
                    if (u.Upgrade is GenericGunUpgrade ggu && ggu.Properties.HasProperties)
                    {
                        UpgradePropertyList props = ggu.Properties;
                        for (int i = 0; i < props.Count; i++)
                        {
                            UpgradeProperty prop = props[i];
                            if (prop.GetType().FullName == "UpgradeProperty_Globbler_RarityStats")
                            {
                                hasRarityStats = true;
                            }
                            if (iGlobblometerPropertyType.IsAssignableFrom(prop.GetType()))
                            {
                                object[] args = new object[] { total, rand, gearData.Gear, u };
                                modifyGlobblometerMethod.Invoke(prop, args);
                                total = (int)args[0];
                            }
                        }
                    }

                    if (hasRarityStats)
                    {
                        needingRarityComputation.Add(u);
                    }

                    if (iGlobblometerPropertyType.IsAssignableFrom(u.Upgrade.GetType()))
                    {
                        object[] args = new object[] { total, rand, gearData.Gear, u };
                        modifyGlobblometerMethod.Invoke(u.Upgrade, args);
                        total = (int)args[0];
                    }
                }

                if (needingRarityComputation.Count > 0)
                {
                    foreach (var u in needingRarityComputation)
                    {
                        int std = (int)getNumTouchingMethod.Invoke(null, new object[] { gearData.Gear, u, rarityFlagsStandard });
                        int rare = (int)getNumTouchingMethod.Invoke(null, new object[] { gearData.Gear, u, rarityFlagsRare });
                        int epic = (int)getNumTouchingMethod.Invoke(null, new object[] { gearData.Gear, u, rarityFlagsEpic });
                        int exo = (int)getNumTouchingMethod.Invoke(null, new object[] { gearData.Gear, u, rarityFlagsExotic });
                        rarityTouchingCounts[u.InstanceID] = (std, rare, epic, exo);
                    }
                }
            }

            if (gearData.Gear is MiniCannon)
            {
                prismConnectedCounts.Clear();
                Dictionary<UpgradeInstance, List<UpgradeInstance>> graph = new Dictionary<UpgradeInstance, List<UpgradeInstance>>();
                List<UpgradeInstance> prisms = new List<UpgradeInstance>();
                Dictionary<int, HashSet<(int, int)>> upgradeSurroundings = new Dictionary<int, HashSet<(int, int)>>();

                System.Collections.IList equippedUpgradesList2 = (System.Collections.IList)equippedUpgradesField.GetValue(gearData);
                List<object> tempList = new List<object>();
                foreach (var eq in equippedUpgradesList2)
                {
                    tempList.Add(eq);
                }

                foreach (var eq in tempList)
                {
                    UpgradeInstance u = (UpgradeInstance)getUpgradeMethod.Invoke(eq, null);
                    bool isPrism = false;
                    if (u.Upgrade is GenericGunUpgrade ggu && ggu.Properties.HasProperties)
                    {
                        UpgradePropertyList props = ggu.Properties;
                        for (int i = 0; i < props.Count; i++)
                        {
                            if (props[i].GetType().FullName == "UpgradeProperty_MiniCannon_Prism")
                            {
                                isPrism = true;
                                break;
                            }
                        }
                    }
                    if (isPrism)
                    {
                        prisms.Add(u);
                        graph[u] = new List<UpgradeInstance>();
                    }
                }

                MethodInfo getSurroundingMethod = AccessTools.Method(typeof(PlayerData), "GetSurroundingCells", new[] { typeof(IUpgradable), typeof(UpgradeInstance), typeof(HashSet<(int, int)>).MakeByRefType() });
                MethodInfo getEquippedUpgradeMethod = AccessTools.Method(typeof(PlayerData), "GetEquippedUpgrade", new[] { typeof(IUpgradable), typeof(int), typeof(int) });

                foreach (UpgradeInstance u in prisms)
                {
                    HashSet<(int, int)> surrounding = new HashSet<(int, int)>();
                    object[] args = new object[] { gearData.Gear, u, surrounding };
                    getSurroundingMethod.Invoke(null, args);
                    surrounding = (HashSet<(int, int)>)args[2];
                    upgradeSurroundings[u.InstanceID] = surrounding;
                }

                foreach (UpgradeInstance u in prisms)
                {
                    HashSet<(int, int)> surrounding = upgradeSurroundings[u.InstanceID];

                    foreach (var cell in surrounding)
                    {
                        UpgradeInstance neighbor = (UpgradeInstance)getEquippedUpgradeMethod.Invoke(null, new object[] { gearData.Gear, cell.Item1, cell.Item2 });
                        if (neighbor != null && graph.ContainsKey(neighbor))
                        {
                            graph[u].Add(neighbor);
                        }
                    }
                }

                HashSet<UpgradeInstance> visited = new HashSet<UpgradeInstance>();
                foreach (UpgradeInstance prism in prisms)
                {
                    if (!visited.Contains(prism))
                    {
                        List<UpgradeInstance> component = DFS(graph, prism, visited);
                        int size = component.Count;
                        foreach (var c in component)
                        {
                            prismConnectedCounts[c.InstanceID] = size;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            SparrohPlugin.Logger.LogError($"Error in RecomputeTotals: {e.Message}\n{e.StackTrace}");
        }
    }

    internal static List<UpgradeInstance> DFS(Dictionary<UpgradeInstance, List<UpgradeInstance>> graph, UpgradeInstance node, HashSet<UpgradeInstance> visited)
    {
        List<UpgradeInstance> component = new List<UpgradeInstance>();
        Stack<UpgradeInstance> stack = new Stack<UpgradeInstance>();
        stack.Push(node);
        while (stack.Count > 0)
        {
            UpgradeInstance current = stack.Pop();
            if (visited.Add(current))
            {
                component.Add(current);
                foreach (UpgradeInstance neighbor in graph[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }
        return component;
    }

    internal static void ClearCaches()
    {
        statListCache.Clear();
        displayPropertiesCache.Clear();
        upgradeGridCache.Clear();
    }
}
