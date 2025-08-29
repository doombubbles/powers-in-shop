using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class SuperMonkeyBeacon : ModPowerTowerPro
{
    public static readonly ModSettingInt BeaconRechargeCost = new(2000)
    {
        description = "In in-game cash, not monkey money",
        icon = VanillaSprites.ChargedBeacon,
        category = PowersInShopMod.RechargeCosts
    };

    protected override int Order => 16;
    public override int BaseCost => 4000;
}