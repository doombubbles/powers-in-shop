using BTD_Mod_Helper.Api;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities.Behaviors;

namespace PowersInShop.Towers;

public class TechBot : ModPowerTower
{
    public override string BaseTower => TowerType.TechBot;
    public override int Cost => PowersInShopMod.TechBotCost;
    protected override int Order => 6;

    [HarmonyPatch(typeof(TechBotLink), nameof(TechBotLink.DoesTowerHaveValidAbilities))]
    internal static class TechBotLink_DoesTowerHaveValidAbilities
    {
        [HarmonyPrefix]
        internal static bool Prefix(Tower tower, ref bool __result)
        {
            if (tower.towerModel.baseId != TowerID<TechBot>()) return true;

            __result = false;
            return false;
        }
    }
}