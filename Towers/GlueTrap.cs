namespace PowersInShop.Towers;

public class GlueTrap : ModTrackPower
{
    public override int Cost => PowersInShopMod.GlueTrapCost;
    protected override int Pierce => PowersInShopMod.GlueTrapPierce;
    protected override int Order => 3;
}