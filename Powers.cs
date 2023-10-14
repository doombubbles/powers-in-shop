using System.Collections.Generic;
using System.Linq;
using Il2CppAssets.Scripts.Models.TowerSets;
using BTD_Mod_Helper.Api.Towers;
using HarmonyLib;
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
                .Where(info => ModPowerTower.PowersByName.ContainsKey(info.standardPowerButton.powerModel.name))
                .ToDictionary(info => info.standardPowerButton.powerModel.name);

            foreach (var towerPurchaseButton in ShopMenu.instance.ActiveTowerButtons)
            {
                var towerBaseId = towerPurchaseButton.towerModel.baseId;
                if (!ModPowerTower.PowersById.TryGetValue(towerBaseId, out var power)) continue;

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
}