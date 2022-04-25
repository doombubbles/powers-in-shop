using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers
{
    public class RoadSpikes : ModTrackPower
    {
        public override int Cost => PowersInShopMod.RoadSpikesCost;
        protected override int Pierce => PowersInShopMod.RoadSpikesPierce;
        public override int Order => 5;

        protected override ProjectileModel GetProjectile(PowerModel powerModel)
        {
            return powerModel.GetBehavior<RoadSpikesModel>().projectileModel;
        }
    }
}