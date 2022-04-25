using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers.Mods;
using Assets.Scripts.Models.TowerSets.Mods;
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
    private static readonly ModSettingBool AllowInChimps = new ModSettingBool(false)
    {
        icon = VanillaSprites.CHIMPSIcon
    };

    public static readonly ModSettingBool AllowInRestrictedModes = new ModSettingBool(true)
    {
        description = "Determines whether power towers will be usable in Primary Only, Military Only and Magic Only game modes.",
        icon = VanillaSprites.MagicBtn
    };

    public static readonly ModSettingInt BananaFarmerCost = new ModSettingInt(500)
    {
        icon = VanillaSprites.BananaFarmerIcon
    };
    public static readonly ModSettingInt TechBotCost = new ModSettingInt(500)
    {
        icon = VanillaSprites.TechbotIcon
    };
    public static readonly ModSettingInt PontoonCost = new ModSettingInt(750)
    {
        icon = VanillaSprites.PontoonIcon
    };
    public static readonly ModSettingInt PortableLakeCost = new ModSettingInt(750)
    {
        icon = VanillaSprites.PortableLakeIcon
    };
    public static readonly ModSettingInt EnergisingTotemCost = new ModSettingInt(1000)
    {
        icon = VanillaSprites.EnergisingTotemIcon
    };
    public static readonly ModSettingInt RoadSpikesCost = new ModSettingInt(50)
    {
        icon = VanillaSprites.HotSpikesIcon
    };
    public static readonly ModSettingInt GlueTrapCost = new ModSettingInt(100)
    {
        icon = VanillaSprites.GlueTrapIcon
    };
    public static readonly ModSettingInt CamoTrapCost = new ModSettingInt(100)
    {
        icon = VanillaSprites.CamoTrapIcon
    };
    public static readonly ModSettingInt MoabMineCost = new ModSettingInt(500)
    {
        icon = VanillaSprites.MoabMineIcon
    };

    public static readonly ModSettingInt RoadSpikesPierce = new ModSettingInt(20)
    {
        icon = VanillaSprites.HotSpikesIcon
    };
    public static readonly ModSettingInt GlueTrapPierce =  new ModSettingInt(300)
    {
        icon = VanillaSprites.GlueTrapIcon
    };
    public static readonly ModSettingInt MoabMinePierce = new ModSettingInt(1)
    {
        icon = VanillaSprites.MoabMineIcon
    };
    public static readonly ModSettingInt CamoTrapPierce = new ModSettingInt(500)
    {
        icon = VanillaSprites.CamoTrapIcon
    };
    public static readonly ModSettingInt TotemRechargeCost = new ModSettingInt(500)
    {
        description = "In in-game cash, not monkey money",
        icon = VanillaSprites.EnergisingTotemIcon
    };
        
    public static readonly ModSettingDouble TotemAttackSpeed = new ModSettingDouble(.15)
    {
        description = ".15 = 15%, down from the normal 25% boost so it isn't blatantly overpowered",
        icon = VanillaSprites.EnergisingTotemIcon
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