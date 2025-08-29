namespace PowersInShop.Towers;

public class RoadSpikes : ModTrackPower
{
    protected override int BasePierce => 20;
    protected override int Order => 1;
    public override int BaseCost => 50;
}