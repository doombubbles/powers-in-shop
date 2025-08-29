using System.Collections.Generic;
using System.Reflection;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using PowersInShop.Towers;
using UnityEngine;
using UnityEngine.UI;

namespace PowersInShop.Patches;

[HarmonyPatch]
public class TSMThemeRecharge_OnButtonPress
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.OnButtonPress),
            [typeof(TowerToSimulation), typeof(TSMButton)]);
        yield return AccessTools.Method(typeof(TSMThemeSuperMonkeyBeacon),
            nameof(TSMThemeSuperMonkeyBeacon.OnButtonPress), [typeof(TowerToSimulation), typeof(TSMButton)]);
    }

    [HarmonyPrefix]
    public static bool Prefix(TSMThemeDefault __instance, TowerToSimulation tower)
    {
        if (!PowersInShopMod.IsPowerFromShop(tower.tower)) return true;

        int baseCost;

        if (__instance.Is<TSMThemeEnergisingTotem>())
        {
            baseCost = EnergisingTotem.TotemRechargeCost;
        }
        else if (__instance.Is<TSMThemeSuperMonkeyBeacon>())
        {
            baseCost = SuperMonkeyBeacon.BeaconRechargeCost;
        }
        else
        {
            return true;
        }
        var cost = CostHelper.CostForDifficulty(baseCost, InGame.instance);

        var cash = InGame.Bridge.GetCash();
        if (cash < cost)
        {
            return false;
        }

        InGame.Bridge.Simulation.RemoveCash(cost, Simulation.CashType.Powers, tower.owner,
            Simulation.CashSource.Normal);
        tower.PerformCustomUIAction();

        return false;
    }
}

[HarmonyPatch]
internal static class TSMThemeRecharge_Selected
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.Selected),
            [typeof(TowerToSimulation), typeof(bool)]);
        yield return AccessTools.Method(typeof(TSMThemeSuperMonkeyBeacon), nameof(TSMThemeSuperMonkeyBeacon.Selected),
            [typeof(TowerToSimulation), typeof(bool)]);
    }

    [HarmonyPostfix]
    internal static void Postfix(TSMThemeDefault __instance, TowerToSimulation tower)
    {
        var isPowerInShop = PowersInShopMod.IsPowerFromShop(tower.tower);

        GameObject button;
        int baseCost;

        if (__instance.Is(out TSMThemeEnergisingTotem energisingTotem))
        {
            button = energisingTotem.rechargeButton.gameObject;
            baseCost = EnergisingTotem.TotemRechargeCost;
        }
        else if (__instance.Is(out TSMThemeSuperMonkeyBeacon superMonkeyBeacon))
        {
            button = superMonkeyBeacon.rechargeButton.gameObject;
            baseCost = SuperMonkeyBeacon.BeaconRechargeCost;
        }
        else
        {
            return;
        }

        var cost = CostHelper.CostForDifficulty(baseCost, InGame.instance);

        var newText = button.GetComponentInChildrenByName<NK_TextMeshProUGUI>("Text");
        newText.enabled = true;
        newText.text = $"${cost:N0}";
        newText.margin = new Vector4(0, 25, 0, 0);
        newText.gameObject.SetActive(isPowerInShop);

        button.GetComponentInChildrenByName<Image>("MmIcon").enabled = !isPowerInShop;
        button.GetComponentInChildrenByName<NK_TextMeshProUGUI>("TextCount").enabled = !isPowerInShop;
    }
}