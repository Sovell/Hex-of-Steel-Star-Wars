public class ScenarioSettlementCluster
{
	public ScenarioSettlementTier Tier { get; set; }

	public Tile CoreTile { get; private set; }

	public Tile FactoryTile { get; set; }

	public Tile AirfieldTile { get; set; }

	public int OwnerIndex { get; private set; }

	public bool IsCapital { get; private set; }

	public ScenarioSettlementCluster(ScenarioSettlementTier p_tier, Tile p_coreTile, int p_ownerIndex, bool p_isCapital)
	{
		Tier = p_tier;
		CoreTile = p_coreTile;
		OwnerIndex = p_ownerIndex;
		IsCapital = p_isCapital;
	}
}
