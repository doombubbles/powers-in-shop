using Assets.Scripts.Models.Towers;

namespace PowersInShop.Towers
{
    public class TechBot : ModPowerTower
    {
        public override string BaseTower => TowerType.TechBot;
        public override int Cost => PowersInShopMod.TechBotCost;
        public override int Order => 1;
    }
}