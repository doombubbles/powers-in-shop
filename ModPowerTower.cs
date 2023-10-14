using System.Collections.Generic;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Data.Cosmetics.PowerAssetChanges;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Utils;

namespace PowersInShop;

public abstract class ModPowerTower : ModTower<Powers>
{
    public static readonly Dictionary<string, ModPowerTower> PowersByName = new();
    public static readonly Dictionary<string, ModPowerTower> PowersById = new();

    public sealed override int TopPathUpgrades => 0;
    public sealed override int MiddlePathUpgrades => 0;
    public sealed override int BottomPathUpgrades => 0;
    public override bool DontAddToShop => Cost < 0;

    public override string DisplayName =>
        Game.instance.GetLocalizationManager().GetText(Name);

    public sealed override string Description =>
        Game.instance.GetLocalizationManager().GetText(Name + " Description");

    public sealed override SpriteReference IconReference => PortraitReference;

    public sealed override SpriteReference PortraitReference
    {
        get
        {
            var powerWithName = Game.instance.model.GetPowerWithName(Name);
            return powerWithName.tower?.portrait ?? powerWithName.icon;
        }
    }

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
    }

    public override void Register()
    {
        base.Register();
        PowersByName[Name] = this;
        PowersById[Id] = this;
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        var power = gameModel.GetPowerWithName(Name);

        if (power.tower != null)
        {
            towerModel.RemoveChild(towerModel.behaviors.Cast<Il2CppSystem.Collections.Generic.ICollection<Model>>());
            towerModel.behaviors = power.tower.behaviors;
            towerModel.AddChild(towerModel.behaviors.Cast<Il2CppSystem.Collections.Generic.ICollection<Model>>());

            towerModel.range = power.tower.range;
            towerModel.towerSelectionMenuThemeId = power.tower.towerSelectionMenuThemeId;
        }
    }

    [HarmonyPatch(typeof(CosmeticHelper), nameof(CosmeticHelper.ApplyAssetChangesToPowerTowerModel))]
    internal static class CosmeticHelper_ApplyAssetChangesToPowerTowerModel
    {
        [HarmonyPostfix]
        private static void Postfix(TowerModel towerModel, PowerAssetChange pac)
        {
            if (!PowersInShopMod.ApplyTowerSkins) return;

            if (PowersByName.TryGetValue(towerModel.baseId, out var modPowerTower))
            {
                var powersInShopTower = CosmeticHelper.rootGameModel.GetTowerFromId(modPowerTower.Id);

                CosmeticHelper.ApplyAssetChangesToPowerTowerModel(powersInShopTower, pac);
            }
            else if (PowersById.ContainsKey(towerModel.baseId))
            {
                towerModel.GetBehavior<DisplayModel>().display = towerModel.display;

                if (PowersInShopMod.ChangeIconsForSkins)
                {
                    towerModel.icon = towerModel.portrait;
                }
            }
        }
    }
}