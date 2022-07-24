using Assets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class PortableLake : ModPowerTower
{
    public override string BaseTower => TowerType.PortableLake;
    public override int Cost => PowersInShopMod.PortableLakeCost;
    protected override int Order => 3;
}