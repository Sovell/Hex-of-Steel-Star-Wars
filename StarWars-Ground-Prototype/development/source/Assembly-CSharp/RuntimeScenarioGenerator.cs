public class RuntimeScenarioGenerator
{
	private readonly ScenarioTerrainStage _terrainStage;

	private readonly ScenarioRegionStage _regionStage;

	private readonly ScenarioSettlementStage _settlementStage;

	public RuntimeScenarioGenerator()
	{
		_terrainStage = new ScenarioTerrainStage();
		_regionStage = new ScenarioRegionStage();
		_settlementStage = new ScenarioSettlementStage();
	}

	public virtual void Generate(ScenarioGenerationRequest p_request)
	{
		ScenarioGenerationContext p_context = new ScenarioGenerationContext(GameData.Instance.map, p_request);
		ResetPlayersForGeneration();
		InitializeTiles(p_context);
		Utils.GetAllTilesNeighbours();
		_terrainStage.Execute(p_context);
		Utils.UpdateTilesData(p_nonDestructive: true);
		_regionStage.Execute(p_context);
		_settlementStage.Execute(p_context);
		Utils.UpdateTilesData(p_nonDestructive: true);
	}

	public virtual void InitializeTiles(ScenarioGenerationContext p_context)
	{
		for (short num = 0; num < p_context.Map.SizeX; num++)
		{
			for (short num2 = 0; num2 < p_context.Map.SizeY; num2++)
			{
				Tile tile = new Tile
				{
					Name = "Plain",
					Type = "Plain",
					Season = p_context.Request.Season,
					PosX = num,
					PosY = num2
				};
				p_context.Map.TilesTable[num, num2] = tile;
			}
		}
	}

	protected virtual void ResetPlayersForGeneration()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.MoneyAmount = 0;
		}
	}
}
