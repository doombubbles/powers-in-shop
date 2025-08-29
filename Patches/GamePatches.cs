using System.Linq;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using UnityEngine;
using Vector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace PowersInShop.Patches;

/// <summary>
/// Handle syncing Power Hotkeys to powers in shop
/// </summary>
[HarmonyPatch(typeof(Hotkeys), nameof(Hotkeys.Setup))]
internal static class Hotkeys_Setup
{
    [HarmonyPostfix]
    private static void Postfix(Hotkeys __instance)
    {
        if (!PowersInShopMod.OverrideHotkeys) return;

        var hotkeysForPowersInShop = __instance.powerHotkeys.ToArray()
            .Where(info => PowersInShopMod.PowersByName.ContainsKey(info.standardPowerButton.powerModel.name))
            .DistinctBy(info => info.standardPowerButton.powerModel.name)
            .ToDictionary(info => info.standardPowerButton.powerModel.name);

        foreach (var towerPurchaseButton in ShopMenu.instance.ActiveTowerButtons)
        {
            var towerBaseId = towerPurchaseButton.TowerModel.baseId;
            if (!PowersInShopMod.PowersById.TryGetValue(towerBaseId, out var power)) continue;

            HotkeyButton hotkeyButton;
            if (hotkeysForPowersInShop.TryGetValue(power.Name, out var powerHotkey))
            {
                __instance.powerHotkeys.Remove(powerHotkey);
                hotkeyButton = powerHotkey.hotkeyButton;
            }
            else
            {
                hotkeyButton = __instance.hotKeyButtonSet.GetButton(power.Name);
            }

            __instance.towerHotkeys.Add(new Hotkeys.TowerHotkeyInfo
            {
                towerBaseId = towerBaseId,
                hotkeyButton = hotkeyButton,
                towerPurchaseButton = towerPurchaseButton
            });
        }
    }
}

/// <summary>
/// Replace power ModTowers with the real power TowerModels
/// </summary>
[HarmonyPatch(typeof(InputManager), nameof(InputManager.CreatePlacementTower))]
internal static class InputManager_CreatePlacementTower
{
    internal static TowerModel? nextPlace;
    internal static float nextCost;

    [HarmonyPrefix]
    internal static void Prefix(InputManager __instance, Vector2 pos)
    {
        if (__instance.placementModel.GetModTower() is not ModPowerTowerBase powerTower) return;

        var newTower = __instance.Bridge.Model.GetPowerWithId(powerTower.Name).tower.Duplicate();

        var owner = InGame.Bridge.MyPlayerNumber;
        var sim = InGame.Bridge.Simulation;

        var height = sim.Map.GetTerrainHeight(new Il2CppAssets.Scripts.Simulation.SMath.Vector2(pos));
        var at = new Vector3(pos.x, pos.y, height);

        var towerInventory = sim.GetTowerInventory(owner);
        var cost = sim.towerManager.GetTowerCost(__instance.placementModel, at, towerInventory, owner);

        sim.RemoveCash(cost, Simulation.CashType.Normal, owner, Simulation.CashSource.TowerBrought);

        __instance.placementModel = newTower;
        nextCost = cost;
        nextPlace = newTower;
    }

    [HarmonyPatch(typeof(Tower), nameof(Tower.OnPlace))]
    internal static class Tower_OnPlace
    {
        [HarmonyPostfix]
        internal static void Postfix(Tower __instance)
        {
            if (__instance.towerModel.name != nextPlace?.name) return;

            __instance.worth = nextCost;

            ModPowerTower.MarkAsPowerFromShop(__instance);

            nextPlace = null;
            nextCost = 0;
        }
    }
}

/// <summary>
/// Hijack a mutator to track powers from shop
/// </summary>
[HarmonyPatch(typeof(RateSupportModel.RateSupportMutator), nameof(RateSupportModel.RateSupportMutator.Mutate))]
internal static class RateSupportMutator_Mutate
{
    [HarmonyPrefix]
    internal static bool Prefix(RateSupportModel.RateSupportMutator __instance, Model model, ref bool __result)
    {
        if (__instance.id != PowersInShopMod.MutatorId || !model.Is(out TowerModel towerModel)) return true;

        if (PowersInShopMod.PowersByName[towerModel.baseId] is ModPowerTowerBase powerTower)
        {
            powerTower.MutateTower(towerModel);
        }

        __result = true;
        return false;
    }
}