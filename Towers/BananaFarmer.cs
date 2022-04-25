using Assets.Scripts.Models.Towers;

namespace PowersInShop.Towers
{
    public class BananaFarmer : ModPowerTower
    {
        public override string BaseTower => TowerType.BananaFarmer;
        public override int Cost => PowersInShopMod.BananaFarmerCost;
        public override int Order => 0;
    }
}