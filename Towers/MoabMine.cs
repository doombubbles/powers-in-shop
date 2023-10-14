namespace PowersInShop.Towers;

public class MoabMine : ModTrackPower
{
    public override int Cost => PowersInShopMod.MoabMineCost;
    protected override int Pierce => PowersInShopMod.MoabMinePierce;
    protected override int Order => 2;
}