using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Gameplay.Mods;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json.Linq;
using PowersInShop;

[assembly: MelonInfo(typeof(PowersInShopMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace PowersInShop;

public class PowersInShopMod : BloonsTD6Mod
{
    public static readonly Dictionary<string, IPowerTower> PowersByName = new();
    public static readonly Dictionary<string, IPowerTower> PowersById = new();

    private static readonly ModSettingBool AllowInChimps = new(false)
    {
        icon = VanillaSprites.CHIMPSIcon
    };

    public static readonly ModSettingBool AllowInRestrictedModes = new(true)
    {
        description =
            "Determines whether power towers will be usable in Primary Only, Military Only and Magic Only game modes.",
        icon = VanillaSprites.MagicBtn,
        requiresRestart = true
    };

    public static readonly ModSettingBool OverrideHotkeys = new(true)
    {
        description = "Disables the hotkeys for activating real powers, and assigns them to the shop ones instead.",
        icon = VanillaSprites.HotkeysIcon
    };

    public static readonly ModSettingBool ApplyTowerSkins = new(true)
    {
        description = "Whether to apply power tower skins like the Banana Costume to powers in shop.",
        icon = VanillaSprites.BananaCostumeFarmerPortrait
    };

    public static readonly ModSettingBool ChangeIconsForSkins = new(false)
    {
        description = "If applying tower skins, whether to also change the icons in the shop to reflect the skin.",
        icon = VanillaSprites.BananaCostumeFarmerIcon
    };

    public static readonly ModSettingBool DisqualifyMonkeyTeams = new(false)
    {
        description = "Whether using Powers in Shop towers will invalidate a Monkey Teams match",
        icon = VanillaSprites.MonkeyTeamsLogoSmall
    };

    #region Costs

    public static readonly ModSettingCategory Costs = "Costs";


    public static readonly ModSettingInt RoadSpikesCost = new(50)
    {
        icon = VanillaSprites.HotSpikesIcon,
        category = Costs
    };

    public static readonly ModSettingInt MoabMineCost = new(500)
    {
        icon = VanillaSprites.MoabMineIcon,
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

    public static readonly ModSettingInt TotemRechargeCost = new(500)
    {
        description = "In in-game cash, not monkey money",
        icon = VanillaSprites.EnergisingTotemIcon,
        category = Costs
    };

    #endregion

    #region Propeties

    public static readonly ModSettingCategory Properties = "Properties";

    public static readonly ModSettingInt RoadSpikesPierce = new(20)
    {
        icon = VanillaSprites.HotSpikesIcon,
        category = Properties
    };

    public static readonly ModSettingInt MoabMinePierce = new(1)
    {
        icon = VanillaSprites.MoabMineIcon,
        category = Properties
    };

    public static readonly ModSettingInt GlueTrapPierce = new(300)
    {
        icon = VanillaSprites.GlueTrapIcon,
        category = Properties
    };

    public static readonly ModSettingInt CamoTrapPierce = new(500)
    {
        icon = VanillaSprites.CamoTrapIcon,
        category = Properties
    };

    public static readonly ModSettingDouble TotemAttackSpeed = new(.15)
    {
        description = ".15 = 15%, down by default from the normal 25% boost so it isn't blatantly overpowered",
        icon = VanillaSprites.EnergisingTotemIcon,
        category = Properties,
        min = 0,
        max = .99
    };

    #endregion

    public override void OnSaveSettings(JObject settings)
    {
        foreach (var modPowerTower in ModContent.GetContent<ModTower>().Where(tower => tower is IPowerTower))
        {
            foreach (var towerModel in Game.instance.model.GetTowersWithBaseId(modPowerTower.Id).AsIEnumerable())
            {
                towerModel.cost = modPowerTower.Cost;
            }
        }

        var chimps = GameData.Instance.mods.FirstOrDefault(model => model.name == "Clicks");
        if (chimps == null) return;

        var chimpsMutators = chimps.mutatorMods.ToList();
        var existingLocks = ModContent.GetContent<ModTower>()
            .Where(tower => tower is IPowerTower)
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

    public override void OnNewGameModel(GameModel gameModel)
    {
        var powerTowers = ModContent.GetContent<ModPowerTower>().Select(tower => tower.Id).ToArray();
        foreach (var biohack in gameModel.GetTowersWithBaseId(TowerType.Benjamin).AsIEnumerable()
                     .SelectMany(model => model.GetDescendants<BiohackModel>().ToArray()))
        {
            biohack.filterTowers = biohack.filterTowers.Concat(powerTowers).ToArray();
        }
    }
}