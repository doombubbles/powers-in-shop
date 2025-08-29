using System.Linq;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.Cosmetics.PowerAssetChanges;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Upgrade;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace PowersInShop.Patches;

/// <summary>
/// Handle track power icon changes
/// </summary>
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

/// <summary>
/// Show power pro upgrades as unlocked
/// </summary>
[HarmonyPatch(typeof(Btd6Player), nameof(Btd6Player.HasUnlockedTower))]
internal static class Btd6Player_HasUnlockedTower
{
    [HarmonyPrefix]
    internal static bool Prefix(Btd6Player __instance, string? towerId, ref bool __result)
    {
        if (towerId != null && ModPowerTowerPro.ById.TryGetValue(towerId, out var powerPro))
        {
            __result = __instance.Data.powersProSaveData?.dataByPower?.TryGetValue(powerPro.Name, out var saveData) ==
                       true &&
                       saveData.seenUnlockPro.ValueBool;

            return false;
        }

        return true;
    }
}

/// <summary>
/// Show power pro upgrades as unlocked
/// </summary>
[HarmonyPatch(typeof(Btd6Player), nameof(Btd6Player.HasUpgrade))]
internal static class Btd6Player_HasUpgrade
{
    [HarmonyPrefix]
    internal static bool Prefix(Btd6Player __instance, string upgradeId, ref bool __result)
    {
        if (ModPowerTowerPro.Upgrades.TryGetValue(upgradeId, out var power))
        {
            var upgrade = __instance.Data.Model.GetPowerProUpgrade(upgradeId);
            __result = __instance.Data.powersProSaveData?.dataByPower?.TryGetValue(power, out var saveData) == true &&
                       saveData.unlockedTier.ValueInt > upgrade.tier;

            return false;
        }

        return true;
    }
}

/// <summary>
/// Don't show acquire button on the powers pro upgrades in the screen
/// </summary>
[HarmonyPatch(typeof(SelectedUpgrade), nameof(SelectedUpgrade.UpdateButtonState))]
internal static class SelectedUpgrade_UpdateButtonState
{
    [HarmonyPostfix]
    internal static void Postfix(SelectedUpgrade __instance)
    {
        __instance.aquireButton.transform.parent.gameObject
            .SetActive(__instance.SelectedDetails?.upgrade?.Is<PowerProUpgradeModel>() != true);
    }
}

/// <summary>
/// Set all unacquired powers pro upgrades as looking locked
/// </summary>
[HarmonyPatch(typeof(UpgradeScreen), nameof(UpgradeScreen.ResetUpgradeUnlocks))]
internal static class UpgradeScreen_ResetUpgradeUnlocks
{
    [HarmonyPostfix]
    internal static void Postfix(Il2CppReferenceArray<UpgradeDetails> upgrades)
    {
        foreach (var upgrade in upgrades.Where(upgrade =>
                     upgrade.upgrade?.Is<PowerProUpgradeModel>() == true && !upgrade.hasUpgrade))
        {
            upgrade.SetLocked();
        }
    }
}

/// <summary>
/// Allow looking at powers pro upgrades in sandbox
/// </summary>
[HarmonyPatch(typeof(InGame), nameof(InGame.ShowUpgradeTree), [])]
internal static class InGame_ShowUpgradeTree
{
    public static bool showUpgradeTree = true;

    [HarmonyPrefix]
    internal static void Prefix() => showUpgradeTree = true;

    [HarmonyPostfix]
    internal static void Postfix() => showUpgradeTree = false;

    [HarmonyPatch(typeof(ReadonlyInGameData), nameof(ReadonlyInGameData.ArePowersAllowed))]
    internal static class ReadonlyInGameData_ArePowersAllowed
    {
        [HarmonyPrefix]
        internal static bool Prefix(ref bool __result)
        {
            if (showUpgradeTree)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}