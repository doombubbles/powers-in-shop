using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppAssets.Scripts.Models.TowerSets;
using BTD_Mod_Helper.Api.Towers;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;

namespace PowersInShop;

public class Powers : ModTowerSet
{
    public override bool AllowInRestrictedModes => PowersInShopMod.AllowInRestrictedModes;

    public override int GetTowerSetIndex(List<TowerSet> towerSets) => towerSets.IndexOf(TowerSet.Support) + 1;

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

    [HarmonyPatch(typeof(Simulation), nameof(Simulation.DistributeInventory))]
    internal static class Simulation_DistributeInventory
    {
        [HarmonyPostfix]
        internal static void Postfix(Simulation __instance,
            Il2CppSystem.Collections.Generic.Dictionary<int, PlayerInfo> playerInfos)
        {
            foreach (var id in playerInfos._keys)
            {
                var towerInventory = __instance.GetTowerInventory(id);
                var powerInventory = __instance.GetPowerInventory(id);

                foreach (var power in powerInventory.GetPowerInventoryMaxes()._keys)
                {
                    var powersInShopId = GetId<PowersInShopMod>(power);
                    if (towerInventory.towerMaxes.ContainsKey(powersInShopId))
                    {
                        towerInventory.towerMaxes[powersInShopId] = Math.Min(towerInventory.towerMaxes[powersInShopId],
                            powerInventory.GetPowerInventoryMaxes()[power]);
                    }
                }
            }
        }
    }
}