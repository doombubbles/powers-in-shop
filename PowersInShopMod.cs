﻿using System.Linq;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.TowerSets.Mods;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using PowersInShop;
using PowersInShop.Towers;

[assembly: MelonInfo(typeof(PowersInShopMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace PowersInShop;

public class PowersInShopMod : BloonsTD6Mod
{
    private static readonly ModSettingBool AllowInChimps = new(false)
    {
        icon = VanillaSprites.CHIMPSIcon
    };

    public static readonly ModSettingBool AllowInRestrictedModes = new(true)
    {
        description = "Determines whether power towers will be usable in Primary Only, Military Only and Magic Only game modes.",
        icon = VanillaSprites.MagicBtn2
    };

    public static readonly ModSettingCategory Costs = "Costs";
    
    public static readonly ModSettingInt BananaFarmerCost = new(500)
    {
        icon = VanillaSprites.BananaFarmerIcon,
        category = Costs
    };
    public static readonly ModSettingInt TechBotCost = new(500)
    {
        icon = VanillaSprites.TechbotIcon,
        category = Costs
    };
    public static readonly ModSettingInt PontoonCost = new(750)
    {
        icon = VanillaSprites.PontoonIcon,
        category = Costs
    };
    public static readonly ModSettingInt PortableLakeCost = new(750)
    {
        icon = VanillaSprites.PortableLakeIcon,
        category = Costs
    };
    public static readonly ModSettingInt EnergisingTotemCost = new(1000)
    {
        icon = VanillaSprites.EnergisingTotemIcon,
        category = Costs
    };
    public static readonly ModSettingInt RoadSpikesCost = new(50)
    {
        icon = VanillaSprites.HotSpikesIcon,
        category = Costs
    };
    public static readonly ModSettingInt GlueTrapCost = new(100)
    {
        icon = VanillaSprites.GlueTrapIcon,
        category = Costs
    };
    public static readonly ModSettingInt CamoTrapCost = new(100)
    {
        icon = VanillaSprites.CamoTrapIcon,
        category = Costs
    };
    public static readonly ModSettingInt MoabMineCost = new(500)
    {
        icon = VanillaSprites.MoabMineIcon,
        category = Costs
    };
    
    
    public static readonly ModSettingCategory Properties = "Properties";

    public static readonly ModSettingInt RoadSpikesPierce = new(20)
    {
        icon = VanillaSprites.HotSpikesIcon,
        category = Properties
    };
    public static readonly ModSettingInt GlueTrapPierce =  new(300)
    {
        icon = VanillaSprites.GlueTrapIcon,
        category = Properties
    };
    public static readonly ModSettingInt MoabMinePierce = new(1)
    {
        icon = VanillaSprites.MoabMineIcon,
        category = Properties
    };
    public static readonly ModSettingInt CamoTrapPierce = new(500)
    {
        icon = VanillaSprites.CamoTrapIcon,
        category = Properties
    };
    public static readonly ModSettingInt TotemRechargeCost = new(500)
    {
        description = "In in-game cash, not monkey money",
        icon = VanillaSprites.EnergisingTotemIcon,
        category = Properties
    };
    public static readonly ModSettingDouble TotemAttackSpeed = new(.15)
    {
        description = ".15 = 15%, down from the normal 25% boost so it isn't blatantly overpowered",
        icon = VanillaSprites.EnergisingTotemIcon,
        category = Properties
    };

    public override void OnGameObjectsReset()
    {
        EnergisingTotem.TSMThemeEnergisingTotem_Selected.lastOpened = false; //UI is reset, so we have to as well
    }

    public override void OnNewGameModel(GameModel result, List<ModModel> mods)
    {
        var chimps = mods.FirstOrDefault(model => model.name == "Clicks");
        if (chimps != null)
        {
            var chimpsMutators = chimps.mutatorMods.ToList();
            var existingLocks = ModContent.GetContent<ModPowerTower>()
                .ToDictionary(tower => tower.Id, tower => chimpsMutators.OfType<LockTowerModModel>()
                    .FirstOrDefault(model => model.towerToLock != tower.Id));

            if (AllowInChimps)
            {
                foreach (var lockTowerModel in existingLocks.Values.Where(lockTowerModel =>
                             lockTowerModel != null))
                {
                    chimpsMutators.Remove(lockTowerModel);
                }
            }
            else
            {
                foreach (var (id, lockTowerModel) in existingLocks)
                {
                    if (lockTowerModel == null)
                    {
                        chimpsMutators.Add(new LockTowerModModel("Clicks", id));
                    }
                }
            }

            chimps.mutatorMods = chimpsMutators.ToIl2CppReferenceArray();
        }
    }
}