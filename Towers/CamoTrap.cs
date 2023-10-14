namespace PowersInShop.Towers;

public class CamoTrap : ModTrackPower
{
    public override int Cost => PowersInShopMod.CamoTrapCost;
    protected override int Pierce => PowersInShopMod.CamoTrapPierce;
    protected override int Order => 4;
}