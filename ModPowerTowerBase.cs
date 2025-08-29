using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace PowersInShop;

/// <summary>
/// Base ModTower for placeable power towers
/// </summary>
public abstract class ModPowerTowerBase : ModTower<Powers>, IPowerTower, IModSettings
{
    public override bool DontAddToShop => Cost < 0;
    public override string DisplayName => $"[{Name}]";

    public abstract int BaseCost { get; }
    public ModSettingInt _cost { get; set; } = null!;
    public sealed override int Cost => _cost;
    protected IPowerTower This => this;

    public override string BaseTower => Name;
    public PowerModel PowerModel => Game.instance.model.GetPowerWithId(Name);

    public override TowerModel GetBaseTowerModel(params int[] tiers) => PowerModel.tower.MakeCopy(Id);

    public sealed override SpriteReference IconReference => PortraitReference;
    public sealed override SpriteReference PortraitReference =>
        new(PowerModel.tower.portrait?.AssetGUID ?? PowerModel.tower.icon?.AssetGUID ?? PowerModel.icon.AssetGUID);

    public sealed override bool IncludeInMonkeyTeams => false;

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
    }

    public override IEnumerable<ModContent> Load()
    {
        This.LoadImpl();
        return base.Load();
    }

    public override void Register()
    {
        This.RegisterImpl();
        base.Register();
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        var powerTower = gameModel.GetPowerWithId(Name).tower;

        var baseId = towerModel.baseId;
        var name = towerModel.name;
        var cost = towerModel.cost;
        var icon = towerModel.icon;

        towerModel.CopyFrom(powerTower);

        towerModel.baseId = baseId;
        towerModel.name = name;
        towerModel.cost = cost;
        towerModel.icon = PowersInShopMod.ChangeIconsForSkins ? towerModel.portrait : icon;
        towerModel.powerName = null;
    }

    public virtual void MutateTower(TowerModel towerModel)
    {
    }
}