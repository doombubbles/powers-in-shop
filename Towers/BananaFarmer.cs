using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class BananaFarmer : ModPowerTower
{
    public override string BaseTower => TowerType.BananaFarmer;
    public override int Cost => PowersInShopMod.BananaFarmerCost;
    protected override int Order => 5;
}