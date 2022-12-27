using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers;

public class RoadSpikes : ModTrackPower
{
    public override int Cost => PowersInShopMod.RoadSpikesCost;
    protected override int Pierce => PowersInShopMod.RoadSpikesPierce;
    protected override int Order => 5;

    protected override ProjectileModel GetProjectile(PowerModel powerModel)
    {
        return powerModel.GetBehavior<RoadSpikesModel>().projectileModel;
    }
}