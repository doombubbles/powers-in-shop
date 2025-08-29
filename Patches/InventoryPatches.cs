using System;
using System.Linq;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Simulation.Towers;

namespace PowersInShop.Patches;

/// <summary>
/// Synchronize inventory towerMaxes and powerMaxes synced
/// </summary>
[HarmonyPatch(typeof(Simulation), nameof(Simulation.StockStandardPowerInventory))]
internal static class Simulation_StockStandardPowerInventory
{
    [HarmonyPostfix]
    internal static void Postfix(Simulation __instance, PowerInventory pi)
    {
        var key = __instance.powerInventories.Entries().First(tuple => tuple.value == pi).key;
        var ti = __instance.towerInventories[key];

        foreach (var powerTower in ModContent.GetContent<ModTower>().OfType<IPowerTower>())
        {
            var count = Il2CppAssets.Scripts.Simulation.SMath.Math.Min(
                ti.towerMaxes.TryGetValue(powerTower.Id, out var towerCount) ? towerCount : int.MaxValue,
                pi.powerMaxes.TryGetValue(powerTower.Name, out var powerCount) ? powerCount : int.MaxValue
            );
            ti.towerMaxes[powerTower.Id] = count;
            pi.powerMaxes[powerTower.Name] = count;
        }
    }
}

/// <summary>
/// Handle inventory syncing, setting tower worth
/// </summary>
[HarmonyPatch(typeof(Tower), nameof(Tower.OnPlace))]
internal static class Tower_OnPlace
{
    [HarmonyPostfix]
    internal static void Postfix(Tower __instance)
    {
        if (!__instance.towerModel.isPowerTower) return;

        var sim = __instance.Sim;
        var pi = sim.powerInventories[__instance.owner];
        var ti = sim.towerInventories[__instance.owner];
        PowersInShopMod.SyncInventoryCounts(ti, pi, __instance.towerModel.powerName, Math.Max);
    }
}

/// <summary>
/// Handle inventory syncing
/// </summary>
[HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
internal static class Tower_OnDestroy
{
    [HarmonyPostfix]
    internal static void Postfix(Tower __instance)
    {
        if (!__instance.towerModel.isPowerTower) return;

        var sim = __instance.Sim;
        var pi = sim.powerInventories[__instance.owner];
        var ti = sim.towerInventories[__instance.owner];

        pi.powerCounts[__instance.towerModel.powerName]--;

        PowersInShopMod.SyncInventoryCounts(ti, pi, __instance.towerModel.powerName, Math.Min);
    }
}