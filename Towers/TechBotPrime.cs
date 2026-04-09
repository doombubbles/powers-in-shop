using Il2CppAssets.Scripts.Models.Towers;

namespace PowersInShop.Towers;

public class TechBotPrime : ModPowerTowerPro
{
    public override string BaseTower => TowerType.TechBotPrime;
    protected override int Order => 18;
    public override int BaseCost => 1000;

}