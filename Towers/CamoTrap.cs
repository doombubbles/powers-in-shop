using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers;

public class CamoTrap : ModTrackPower
{
    public override int Cost => PowersInShopMod.CamoTrapCost;
    protected override int Pierce => PowersInShopMod.CamoTrapPierce;
    protected override int Order => 7;

    protected override ProjectileModel GetProjectile(PowerModel powerModel)
    {
        return powerModel.GetBehavior<CamoTrapModel>().projectileModel;
    }
}