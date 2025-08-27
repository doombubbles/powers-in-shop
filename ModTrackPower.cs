using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.Cosmetics.PowerAssetChanges;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Effects;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Powers.Effects;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Powers;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using UnityEngine;

namespace PowersInShop;

public abstract class ModTrackPower : ModFakeTower<Powers>, IPowerTower
{
    public override bool DontAddToShop => Cost < 0;
    public override string DisplayName => $"[{Name}]";
    public sealed override string Description => $"[{Name} Description]";

    public override string Icon => SpriteResizer.Scaled(PowerModel.icon.AssetGUID, .75f);

    public static Simulation Sim => InGame.Bridge.Simulation;
    public static GameModel GameModel => InGame.instance == null ? Game.instance.model : Sim.model;
    public PowerModel PowerModel => GameModel.GetPowerWithId(Name);

    protected abstract int Pierce { get; }

    public override void Register()
    {
        base.Register();
        PowersInShopMod.PowersByName[Name] = this;
        PowersInShopMod.PowersById[Id] = this;
    }

    public override AudioClipReference PlacementSound =>
        PowerModel.GetBehavior<CreateSoundOnPowerModel>().sound.assetId;

    public override EffectModel PlacementEffect =>
        PowerModel.GetBehavior<CreateEffectOnPowerModel>().effectModel;

    public override bool CanPlaceAt(Vector2 at, Tower hoveredTower, ref string helperMessage) =>
        InGame.Bridge.CheckPowerLocation(at, PowerModel);

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        towerModel.AddBehavior(PowerModel.GetDescendant<ProjectileModel>());
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        var currentPowerModel = gameModel.GetPowerWithId(Name);

        var projectile = towerModel.GetBehavior<ProjectileModel>();
        towerModel.RemoveBehavior<ProjectileModel>();

        var currentPowerProj = gameModel.GetPowerWithId(Name).GetDescendant<ProjectileModel>();

        var newProjectile = currentPowerProj.Duplicate();

        newProjectile.pierce *= Pierce / projectile.pierce;

        towerModel.AddBehavior(newProjectile);

        if (PowersInShopMod.ChangeIconsForSkins)
        {
            towerModel.icon.guidRef = SpriteResizer.Scaled(currentPowerModel.icon.AssetGUID, .75f);
        }

        towerModel.portrait = towerModel.icon;
    }

    public override void OnPlace(Vector2 at, TowerModel towerModelFake, Tower hoveredTower, float cost)
    {
        var proj = towerModelFake.GetBehavior<ProjectileModel>();
        var owner = InGame.Bridge.MyPlayerNumber;

        var power = new TrackPower
        {
            sim = Sim
        };

        power.CreateProjectile(new Il2CppAssets.Scripts.Simulation.SMath.Vector2(at), proj, owner);
    }

    [HarmonyPatch(typeof(CosmeticHelper), nameof(CosmeticHelper.ApplyAssetChangesToPowerModel))]
    internal static class CosmeticHelper_ApplyAssetChangesToPowerModel
    {
        [HarmonyPostfix]
        internal static void Postfix(PowerModel pm, PowerAssetChange pac)
        {
            if (!PowersInShopMod.ChangeIconsForSkins ||
                !PowersInShopMod.PowersByName.TryGetValue(pm.name, out var power) ||
                power is not ModTrackPower trackPower) return;

            var item = GameData.Instance.trophyStoreItems
                .GetAllItems()
                .First(storeItem => storeItem.itemTypes.Any(data => data.itemTarget.id == pac.id));

            var tower = CosmeticHelper.rootGameModel.GetTowerWithName(trackPower.Id);

            tower.portrait = tower.icon = new SpriteReference(SpriteResizer.Scaled(item.icon.AssetGUID, .75f));
        }
    }

}