using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers;

public class EnergisingTotem : ModPowerTower
{
    public static readonly ModSettingDouble TotemAttackSpeed = new(.15)
    {
        description = ".15 = 15%, down by default from the normal 25% boost so it isn't as blatantly overpowered",
        category = PowersInShopMod.Properties,
        min = 0,
        icon = VanillaSprites.EnergisingTotemIcon
    };

    public static readonly ModSettingInt TotemRechargeCost = new(500)
    {
        description = "In in-game cash, not monkey money",
        category = PowersInShopMod.RechargeCosts,
        icon = VanillaSprites.EnergisingTotemIcon,
        min = 0
    };

    public override string BaseTower => TowerType.EnergisingTotem;
    protected override int Order => 7;
    public override int BaseCost => 1000;

    public override void MutateTower(TowerModel towerModel)
    {
        towerModel.GetBehavior<RateSupportModel>().multiplier = 1f / (1 + TotemAttackSpeed);
        towerModel.GetBehavior<EnergisingTotemBehaviorModel>().monkeyMoneyCost = 0;
    }
}