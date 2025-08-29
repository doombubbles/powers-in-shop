using System.Collections.Generic;
using Il2CppAssets.Scripts.Models.TowerSets;
using BTD_Mod_Helper.Api.Towers;

namespace PowersInShop;

public class Powers : ModTowerSet
{
    public override bool AllowInRestrictedModes => PowersInShopMod.AllowInRestrictedModes;

    public override int GetTowerSetIndex(List<TowerSet> towerSets) => towerSets.IndexOf(TowerSet.Support) + 1;

}