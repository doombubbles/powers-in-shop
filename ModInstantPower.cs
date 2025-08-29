using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace PowersInShop;

/// <summary>
/// Base ModTower for instant use powers
/// </summary>
public abstract class ModInstantPower : ModFakeTower<Powers>, IPowerTower
{
    public override bool DontAddToShop => Cost < 0;
    public override string DisplayName => $"[{Name}]";
    public sealed override string Description => $"[{Name} Description]";

    public override string Icon => SpriteResizer.Scaled(PowerModel.icon.AssetGUID, .75f);

    public abstract int BaseCost { get; }

    public ModSettingInt _cost { get; set; } = null!;
    public sealed override int Cost => _cost;
    protected IPowerTower This => this;

    public static Simulation Sim => InGame.Bridge.Simulation;
    public static GameModel GameModel => InGame.instance == null ? Game.instance.model : Sim.model;
    public PowerModel PowerModel => GameModel.GetPowerWithId(Name);

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

    public override bool CanPlaceAt(UnityEngine.Vector2 at, Tower hoveredTower, ref string helperMessage)
    {
        if (TimeManager.inBetweenRounds && !PowerModel.canBeActivatedBetweenRounds)
        {
            helperMessage = "Can only be used when a round is active.";
            return false;
        }

        return InGame.Bridge.CheckPowerLocation(at, PowerModel);
    }

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        var currentPowerModel = gameModel.GetPowerWithId(Name);

        if (PowersInShopMod.ChangeIconsForSkins)
        {
            towerModel.icon.guidRef = SpriteResizer.Scaled(currentPowerModel.icon.AssetGUID, .75f);
        }

        towerModel.portrait = towerModel.icon;
    }

    public sealed override void OnPlace(UnityEngine.Vector2 at, TowerModel towerModelFake, Tower hoveredTower,
        float cost)
    {
        ActivatePower(new Vector2(at), PowerModel);
    }

    public virtual void ActivatePower(Vector2 at, PowerModel powerModel)
    {
        Sim.powerManager.Activate(at, ref powerModel, InGame.Bridge.MyPlayerNumber);
    }

}