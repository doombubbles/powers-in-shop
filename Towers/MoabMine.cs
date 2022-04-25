using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace PowersInShop.Towers
{
    public class MoabMine : ModTrackPower
    {
        public override int Cost => PowersInShopMod.MoabMineCost;
        protected override int Pierce => PowersInShopMod.MoabMinePierce;
        public override int Order => 8;

        protected override ProjectileModel GetProjectile(PowerModel powerModel)
        {
            return powerModel.GetBehavior<MoabMineModel>().projectileModel;
        }
    }
}