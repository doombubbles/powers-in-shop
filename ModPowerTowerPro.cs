using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Bridge;

namespace PowersInShop;

/// <summary>
/// ModTower for Powers Pro
/// </summary>
public abstract class ModPowerTowerPro : ModPowerTowerBase
{
    public static readonly Dictionary<string, ModPowerTowerPro> ByName = [];
    public static readonly Dictionary<string, ModPowerTowerPro> ById = [];
    public static readonly Dictionary<string, string> Upgrades = [];

    public sealed override int TopPathUpgrades => 3;
    public sealed override int MiddlePathUpgrades => 3;
    public sealed override int BottomPathUpgrades => 3;

    public override bool DontApplyModUpgrades => true;


    public sealed override string Description => $"[{Name}Power Description]";

    public override bool IsValidCrosspath(params int[] tiers) => tiers.Count(tier => tier > 0) <= 1;

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        base.ModifyBaseTowerModel(towerModel);

        foreach (var appliedUpgrade in towerModel.appliedUpgrades)
        {
            Upgrades.TryAdd(appliedUpgrade, Name);
        }
    }

    public override void Register()
    {
        base.Register();
        ById[Id] = this;
        ByName[Name] = this;
        foreach (var upgrades in Upgrades
                     .Where(pair => pair.Value == Name)
                     .Select(pair => Game.instance.model.GetPowerProUpgrade(pair.Key))
                     .GroupBy(model => model.tier))
        {
            var xpCost = upgrades.Max(model => model.xpCost);
            foreach (var upgrade in upgrades)
            {
                upgrade.xpCost = xpCost;
            }
        }
    }


    public override TowerModel GetBaseTowerModel(params int[] tiers) => Game.instance.model
        .GetPowerProModel(Name)
        .towerModels
        .First(model => model.CheckTiersAreEqual(tiers[0], tiers[1], tiers[2]))
        .MakeCopy(Id);

    public override int? MaxUpgradePips(TowerToSimulation tower, int path, int defaultMax) => Math.Min(3, defaultMax);
}