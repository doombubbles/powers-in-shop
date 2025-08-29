using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace PowersInShop;

public interface IPowerTower
{
    string Id { get; }
    string Name { get; }
    string DisplayName { get; }
    SpriteReference IconReference { get; }

    int BaseCost { get; }

    // ReSharper disable once InconsistentNaming
    ModSettingInt _cost { get; set; }

    void LoadImpl()
    {
        _cost = new ModSettingInt(BaseCost)
        {
            category = PowersInShopMod.Costs,
            displayName = DisplayName,
            description = $"In Game Cost for {DisplayName}. Set to a negative number to disable the buff."
        };

        ModContent.GetInstance<PowersInShopMod>().ModSettings[Name + "Cost"] = _cost;
    }

    void RegisterImpl()
    {
        PowersInShopMod.PowersByName[Name] = this;
        PowersInShopMod.PowersById[Id] = this;
        _cost.icon = IconReference.AssetGUID;
    }
}