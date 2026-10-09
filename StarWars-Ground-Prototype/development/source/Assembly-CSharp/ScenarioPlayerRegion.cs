using System.Collections.Generic;

public class ScenarioPlayerRegion
{
	public Player Player { get; private set; }

	public int Index { get; private set; }

	public Tile AnchorTile { get; set; }

	public List<ScenarioSettlementCluster> Settlements { get; private set; }

	public ScenarioPlayerRegion(Player p_player, int p_index)
	{
		Player = p_player;
		Index = p_index;
		Settlements = new List<ScenarioSettlementCluster>();
	}
}
