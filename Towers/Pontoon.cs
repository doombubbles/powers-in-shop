using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class Pontoon : ModPowerTower
{
    public override string BaseTower => TowerType.Pontoon;
    public override int Cost => PowersInShopMod.PontoonCost;
    protected override int Order => 8;
}