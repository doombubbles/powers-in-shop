using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Unity;

namespace PowersInShop;

/// <summary>
/// Base ModTower for instant use track powers
/// </summary>
public abstract class ModTrackPower : ModInstantPower
{
    private ModSettingInt pierce = null!;

    protected abstract int BasePierce { get; }

    public override IEnumerable<ModContent> Load()
    {
        pierce = new ModSettingInt(BasePierce)
        {
            category = PowersInShopMod.Properties,
            min = 0
        };

        return base.Load();
    }

    public override void Register()
    {
        base.Register();
        pierce.icon = IconReference.AssetGUID;
    }

    public override void ActivatePower(Vector2 at, PowerModel powerModel)
    {
        var baseProjectile = Game.instance.model.GetPowerWithId(Name).GetDescendant<ProjectileModel>();

        powerModel = powerModel.Duplicate();
        powerModel.GetDescendant<ProjectileModel>().pierce *= pierce / baseProjectile.pierce;

        base.ActivatePower(at, powerModel);
    }

}