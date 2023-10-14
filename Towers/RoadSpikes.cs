namespace PowersInShop.Towers;

public class RoadSpikes : ModTrackPower
{
    public override int Cost => PowersInShopMod.RoadSpikesCost;
    protected override int Pierce => PowersInShopMod.RoadSpikesPierce;
    protected override int Order => 1;
}