namespace PowersInShop.Towers;

public class CamoTrap : ModTrackPower
{
    protected override int BasePierce => 500;
    protected override int Order => 4;
    public override int BaseCost => 600;
}