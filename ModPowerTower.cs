namespace PowersInShop;

/// <summary>
/// ModTower for normal Powers Towers
/// </summary>
public abstract class ModPowerTower : ModPowerTowerBase
{
    public sealed override int TopPathUpgrades => 0;
    public sealed override int MiddlePathUpgrades => 0;
    public sealed override int BottomPathUpgrades => 0;

    public sealed override string Description => $"[{Name} Description]";

}