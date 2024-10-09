using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Data.Cosmetics.PowerAssetChanges;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace PowersInShop;

public abstract class ModPowerTower : ModTower<Powers>
{
    public static readonly Dictionary<string, ModPowerTower> PowersByName = new();
    public static readonly Dictionary<string, ModPowerTower> PowersById = new();

    public sealed override int TopPathUpgrades => 0;
    public sealed override int MiddlePathUpgrades => 0;
    public sealed override int BottomPathUpgrades => 0;
    public override bool DontAddToShop => Cost < 0;

    public override string DisplayName => $"[{Name}]";

    public sealed override string Description => $"[{Name} Description]";

    public sealed override SpriteReference IconReference => PortraitReference;

    public sealed override SpriteReference PortraitReference
    {
        get
        {
            var powerWithName = Game.instance.model.GetPowerWithName(Name);
            return powerWithName.tower?.portrait ?? powerWithName.icon;
        }
    }

    public sealed override bool IncludeInMonkeyTeams => false;

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

                try
                {
                    CosmeticHelper.ApplyAssetChangesToPowerTowerModel(powersInShopTower, pac);
                }
                catch (Exception e)
                {
                    ModHelper.Warning<PowersInShopMod>($"Failed to apply cosmetic changed to {modPowerTower.Id}");
                    ModHelper.Warning<PowersInShopMod>(e);
                }
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

    [HarmonyPatch]
    internal static class InGame_TowerCreated_0
    {
        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod() => typeof(InGame)
            .GetNestedTypes()
            .SelectMany(type => type.GetMethods())
            .First(method => method.Name.Contains($"_{nameof(InGame.TowerCreated)}_"));

        [HarmonyPrefix]
        internal static bool Prefix(TowerDetailsModel x, ref bool __result)
        {
            if (!PowersById.ContainsKey(x.towerId) || PowersInShopMod.DisqualifyMonkeyTeams) return true;

            __result = false;
            return false;
        }
    }

}