using System;

[Serializable]
public class ScenarioGeneratorData
{
	[Serializable]
	public struct PlayerData
	{
		public string country;

		public string faction;

		public bool isPlayingFirst;

		public bool isComputer;
	}

	public int seed;

	public bool hasSeed;

	public PlayerData[] playersData;

	public int sizeX;

	public int sizeY;

	public int year;

	public int month;

	public int day;

	public int daysPerTurn;

	public string season;

	public float incomeMultiplier;

	public float manpowerMultiplier;

	public int waterAmount;

	public int thickForestAmount;

	public int thinForestAmount;

	public int hillsAmount;

	public int citiesAmount;

	public int dunesAmount;

	public int desertRockAmount;

	public int bocageAmount;

	public int marshAmount;

	public int mountainAmount;

	public bool generateAirfields;

	public bool generateFactories;

	public bool generateRoads;

	public bool generateRailroads;

	public bool generateRivers;

	public bool spawnUnits;

	public bool isIsland;

	public ScenarioGeneratorData()
	{
		seed = Guid.NewGuid().GetHashCode();
		hasSeed = true;
		playersData = new PlayerData[2]
		{
			new PlayerData
			{
				country = "Nazi Germany",
				faction = "Axis",
				isPlayingFirst = true,
				isComputer = false
			},
			new PlayerData
			{
				country = "Soviet Union",
				faction = "Communists",
				isPlayingFirst = false,
				isComputer = false
			}
		};
		sizeX = 50;
		sizeY = 50;
		year = 1941;
		month = 6;
		day = 22;
		daysPerTurn = 1;
		season = "Summer";
		incomeMultiplier = 3f;
		manpowerMultiplier = 3f;
		citiesAmount = 25;
		waterAmount = 30;
		thickForestAmount = 25;
		thinForestAmount = 25;
		hillsAmount = 10;
		dunesAmount = 0;
		desertRockAmount = 0;
		bocageAmount = 0;
		marshAmount = 5;
		mountainAmount = 15;
		generateRivers = true;
		generateRoads = true;
		generateRailroads = true;
		generateAirfields = true;
		generateFactories = true;
		spawnUnits = true;
		isIsland = false;
	}

	public static ScenarioGeneratorData CreateDefault()
	{
		return new ScenarioGeneratorData();
	}
}
