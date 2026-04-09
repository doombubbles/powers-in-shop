using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class MonkeyBoostPro : ModPowerTowerPro
{
    public static readonly ModSettingInt HypeRechargeCost = new(1000)
    {
        description = "In in-game cash, not monkey money",
        icon = VanillaSprites.Condition,
        category = PowersInShopMod.RechargeCosts
    };

    public override string BaseTower => "MonkeyBoostPro";
    protected override int Order => 17;
    public override int BaseCost => 4000;

    public override void MutateTower(TowerModel towerModel)
    {
        base.MutateTower(towerModel);

        if (towerModel.towerSelectionMenuThemeId == "Default")
        {
            towerModel.towerSelectionMenuThemeId = "SuperMonkeyBeacon";
        }
    }
}