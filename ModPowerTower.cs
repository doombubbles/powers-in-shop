using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;

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

    public static void MarkAsPowerFromShop(Tower tower)
    {
        tower.AddMutator(new RateSupportModel.RateSupportMutator(true, PowersInShopMod.MutatorId, 1, 10, null));
    }
}