public class ScenarioGenerationRequest
{
	public int SizeX { get; set; }

	public int SizeY { get; set; }

	public int PlayersAmount { get; set; }

	public string Season { get; set; }

	public float IncomeMultiplier { get; set; }

	public float ManpowerMultiplier { get; set; }

	public int WaterAmount { get; set; }

	public int ThickForestAmount { get; set; }

	public int ThinForestAmount { get; set; }

	public int HillsAmount { get; set; }

	public int CitiesAmount { get; set; }

	public int DunesAmount { get; set; }

	public int DesertRockAmount { get; set; }

	public int BocageAmount { get; set; }

	public int MarshAmount { get; set; }

	public int MountainAmount { get; set; }

	public bool GenerateAirfields { get; set; }

	public bool GenerateFactories { get; set; }

	public bool GenerateSeas { get; set; }

	public int RuntimeSeed { get; set; }
}
