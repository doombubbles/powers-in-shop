using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Models.Gameplay.Mods;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using MelonLoader;
using Newtonsoft.Json.Linq;
using PowersInShop;

[assembly: MelonInfo(typeof(PowersInShopMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace PowersInShop;

public class PowersInShopMod : BloonsTD6Mod
{
    public const string MutatorId = nameof(PowersInShop);

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

    public static readonly ModSettingBool ChangeIconsForSkins = new(false)
    {
        description = "Whether to change the icons in the shop to reflect the power skin being used.",
        icon = VanillaSprites.BananaCostumeFarmerIcon
    };

    #region Costs

    public static readonly ModSettingCategory Costs = "Costs";

    public static readonly ModSettingCategory RechargeCosts = "Recharge Costs";
    #endregion

    public static readonly ModSettingCategory Properties = "Properties";

    public override void OnSaveSettings(JObject settings)
    {
        foreach (var modPowerTower in ModContent.GetContent<ModTower>().Where(tower => tower is IPowerTower))
        {
            foreach (var towerModel in Game.instance.model.GetTowersWithBaseId(modPowerTower.Id).AsIEnumerable())
            {
                towerModel.cost = modPowerTower.Cost;
            }

            modPowerTower.AddOrRemoveFromShop();
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

    public override void OnTowerSaved(Tower tower, TowerSaveDataModel saveData)
    {
        if (tower.IsMutatedBy(MutatorId))
        {
            saveData.metaData[MutatorId] = "true";
        }
    }

    public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
    {
        if (saveData.metaData.ContainsKey(MutatorId))
        {
            ModPowerTower.MarkAsPowerFromShop(tower);
        }
    }

    public static void SyncInventoryCounts(TowerInventory ti, PowerInventory pi, string powerName,
        Func<int, int, int> operation)
    {
        var id = ModContent.GetId<PowersInShopMod>(powerName);
        if (!ModContent.TryFind(id, out ModTower powerTower)) return;

        var count = operation(
            ti.towerCounts.TryGetValue(powerTower.Id, out var towerCount) ? towerCount : 0,
            pi.powerCounts.TryGetValue(powerTower.Name, out var powerCount) ? powerCount : 0
        );
        ti.towerCounts[powerTower.Id] = count;
        pi.powerCounts[powerTower.Name] = count;

        ShopMenu.instance?.GetTowerButtonFromBaseId(id)?.MarkDirty();
    }
}