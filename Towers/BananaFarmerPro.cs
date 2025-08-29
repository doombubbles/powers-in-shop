using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class BananaFarmerPro : ModPowerTowerPro
{
    public override string BaseTower => TowerType.BananaFarmerPro;
    protected override int Order => 15;
    public override int BaseCost => 1000;
}