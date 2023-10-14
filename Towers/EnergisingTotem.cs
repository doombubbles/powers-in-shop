using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using UnityEngine;
using UnityEngine.UI;

namespace PowersInShop.Towers;

public class EnergisingTotem : ModPowerTower
{
    public override string BaseTower => TowerType.EnergisingTotem;
    public override int Cost => PowersInShopMod.EnergisingTotemCost;
    protected override int Order => 7;

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        base.ModifyTowerModelForMatch(towerModel, gameModel);
        towerModel.GetBehavior<RateSupportModel>().multiplier = 1 - PowersInShopMod.TotemAttackSpeed;
        towerModel.GetBehavior<EnergisingTotemBehaviorModel>().monkeyMoneyCost = 0;
    }

    [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.OnButtonPress))]
    public class TSMThemeEnergisingTotem_OnButtonPress
    {
        [HarmonyPrefix]
        public static bool Prefix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
        {
            if (tower.worth > 0)
            {
                var cash = InGame.instance.GetCash();
                var cost = CostHelper.CostForDifficulty(PowersInShopMod.TotemRechargeCost, InGame.instance);
                if (cash < cost)
                {
                    return false;
                }

                InGame.instance.SetCash(cash - cost);

                /*var mm = Game.instance.playerService.Player.Data.monkeyMoney.Value;
                Game.instance.playerService.Player.Data.monkeyMoney.Value = mm + 20;*/
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TSMThemeEnergisingTotem), nameof(TSMThemeEnergisingTotem.Selected))]
    public class TSMThemeEnergisingTotem_Selected
    {
        [HarmonyPostfix]
        public static void Postfix(TSMThemeEnergisingTotem __instance, TowerToSimulation tower)
        {
            var towerModel = tower.tower.towerModel;

            var isPowerInShop = !towerModel.isPowerTower;

            var button = __instance.rechargeButton.gameObject;

            var cost = CostHelper.CostForDifficulty(PowersInShopMod.TotemRechargeCost, InGame.instance);

            var newText = button.GetComponentInChildrenByName<NK_TextMeshProUGUI>("Text");
            newText.enabled = true;
            newText.text = $"${cost:N0}";
            newText.margin = new Vector4(0, 25, 0, 0);
            newText.gameObject.SetActive(isPowerInShop);

            button.GetComponentInChildrenByName<Image>("MmIcon").enabled = !isPowerInShop;
            button.GetComponentInChildrenByName<NK_TextMeshProUGUI>("TextCount").enabled = !isPowerInShop;
        }
    }
}