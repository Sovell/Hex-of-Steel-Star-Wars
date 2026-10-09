using System;
using System.Collections.Generic;
using System.Linq;

public static class Database
{
	public static byte maxEntrenchmentLevel = 3;

	public static float entrenchmentDamageModifier = 0.15f;

	public static int politicalUnitCost = 200;

	public static int veteranUnitCost = 100;

	public static int coreUnitCost = 100;

	public static int winterCost = 50;

	public static int mountaineerCost = 50;

	public static int horseCost = 50;

	public static int motorizeCost = 100;

	public static int mechanizeCost = 200;

	public static int dropTankCost = 50;

	public static int rocketsCost = 50;

	public static int bombCost = 50;

	public static int torpedoCost = 50;

	public static int AtomBombCost = 5000;

	public static List<string> seasons = new List<string> { "Summer", "Winter", "Desert" };

	public static List<string> factions = new List<string> { "Neutral", "Axis", "Allies", "Communists" };

	public static List<string> ideologies = new List<string> { "None", "Neutral", "Democracy", "Fascism", "National Socialism", "Communism" };

	public static List<string> unitTypes = new List<string> { "None", "Infantry", "Tank", "Plane", "Artillery", "AA", "Boat" };

	public static List<string> unitFilters = new List<string>
	{
		"None", "FilterInfantry", "FilterTank", "FilterAntitank", "FilterArtillery", "FilterAA", "FilterFighter", "FilterCAS", "FilterBomber", "FilterBoat",
		"FilterBuilding"
	};

	public static List<string> NATOtypes = new List<string>
	{
		"AA", "Armoured AA", "Armoured AT", "Armoured artillery", "Artillery", "Battleship", "Bomber", "CAS", "Carrier", "Commander",
		"Destroyer", "Droppable", "Engineers", "Fighter", "Heavy cruiser", "Horse AA", "Horse AT", "Horse artillery", "Infantry", "Infantry recon",
		"Light cruiser", "Mechanized AA", "Mechanized AT", "Mechanized artillery", "Mechanized engineers", "Mechanized infantry", "Mine", "Motorized AA", "Motorized AT", "Motorized artillery",
		"Motorized engineers", "Motorized infantry", "Paratroopers", "Plane", "Plane recon", "Submarine", "Supply plane", "Supply ship", "Train"
	};

	public static UnitModification coreOption => new UnitModification().Set("IsCoreUnit", true).Add("Cost", coreUnitCost);

	public static UnitModification winterTrainedOption => new UnitModification().Set("IsWinterSpecialized", true).Add("Cost", winterCost, (Unit u) => (u.OwnerPlayer != null && u.OwnerPlayer.Country != "Soviet Union" && u.OwnerPlayer.Country != "Finland") || (u.OwnerPlayer == null && u.Country != "Soviet Union" && u.Country != "Finland"));

	public static UnitModification mountaineersOption => new UnitModification().Set("IsMountaineer", true).Add("Cost", mountaineerCost);

	public static UnitModification politicalOption => new UnitModification().Set("IsPoliticalUnit", true).Add("Cost", politicalUnitCost);

	public static UnitModification atomBombOption => new UnitModification().Set("HasABomb", true).Add("Cost", AtomBombCost);

	public static UnitModification dropTankOption => new UnitModification().Set("HasDropTank", true).Add("MaxMP", -1.0).MultiplyAdd("MaxAutonomy", "BaseMaxAutonomy", 0.25)
		.Add("Cost", dropTankCost);

	public static UnitModification torpedoOption => new UnitModification().Set("HasTorpedo", true).Add("MaxMP", -1.0).MultiplyAdd("MaxAutonomy", "BaseMaxAutonomy", -0.10000000149011612)
		.Add("Cost", torpedoCost);

	public static UnitModification rocketsOption => new UnitModification().Set("HasRockets", true).Add("SoftDamage", 10.0).Add("HardDamage", 10.0)
		.Add("Cost", rocketsCost);

	public static UnitModification bombsOption => new UnitModification().Set("HasBombs", true).Add("MaxMP", -1.0).Add("SoftDamage", 10.0)
		.Add("HardDamage", 10.0)
		.MultiplyAdd("MaxAutonomy", "BaseMaxAutonomy", -0.25)
		.MultiplyAdd("AirDamage", "BaseAirDamage", -0.25, (Unit u) => u.FilterType == "FilterFighter")
		.Add("Cost", bombCost);

	public static UnitModification horseTowedOption => new UnitModification().Set("IsHorsed", true).Set("MaxMP", 3).Set("MaxAutonomy", 80)
		.Add("Cost", horseCost);

	public static UnitModification motorizedOption => new UnitModification().Set("IsMotorized", true).Set("MaxMP", 6).Set("MaxAutonomy", 50)
		.Add("Cost", motorizeCost);

	public static UnitModification mechanizedOption => new UnitModification().Set("IsMechanized", true).Set("MaxMP", 5).Set("MaxAutonomy", 30)
		.Add("Armour", 10.0)
		.Add("SoftDamage", 5.0, (Unit u) => u.Type != "AA" && u.Type != "Artillery")
		.Add("Cost", mechanizeCost);

	public static UnitModification trainTransportOption => new UnitModification().Set("IsTrain", true).Set("MaxMP", 60).Set("MaxAutonomy", 600);

	public static UnitModification planeTransportOption => new UnitModification().Set("IsDropable", true).Set("MaxMP", 8).Set("MaxAutonomy", 40)
		.Set("Type", "Plane");

	public static UnitModification shipTransportOption => new UnitModification().Set("IsOnBoat", true).Set("MaxMP", 5).Set("MaxAutonomy", 100)
		.Set("Type", "Boat");

	public static List<string> Countries
	{
		get
		{
			List<string> list = new List<string>();
			list.Add("Nazi Germany");
			list.Add("Soviet Union");
			list.Add("USA");
			list.Add("Croatia");
			list.Add("France");
			list.Add("Poland");
			list.Add("Greece");
			list.Add("Finland");
			list.Add("Romania");
			list.Add("Bulgaria");
			list.Add("Hungary");
			list.Add("Iceland");
			list.Add("Italy");
			list.Add("Australia");
			list.Add("Canada");
			list.Add("Great Britain");
			list.Add("Yugoslavia");
			list.Add("Belgium");
			list.Add("Netherlands");
			list.Add("Luxembourg");
			list.Add("Switzerland");
			list.Add("Sweden");
			list.Add("Japan");
			list.Add("Norway");
			list.Add("Spain");
			list.Add("Spanish Republic");
			list.Add("Denmark");
			list.Add("Turkey");
			list.Add("China");
			list.Add("Slovakia");
			list.Add("New Zealand");
			list.Add("British Raj");
			list.Add("Vichy France");
			list.Add("Mongolia");
			list.Add("Thailand");
			list.Add("Portugal");
			list.Add("Ireland");
			list.Add("Philippines");
			list.Add("Iran");
			list.Add("Iraq");
			list.Add("Saudi Arabia");
			list.Add("Latvia");
			list.Add("Lithuania");
			list.Add("Estonia");
			list.Add("Manchukuo");
			list.Add("Communist China");
			list.Add("Neutral");
			list.AddRange(ModsManager.GetModdedCountries(p_sort: false));
			List<string> list2 = list.Distinct().ToList();
			list2.Sort(StringComparer.Ordinal);
			list2.Insert(0, "None");
			return list2;
		}
	}

	public static HashSet<string> Majors
	{
		get
		{
			HashSet<string> hashSet = new HashSet<string> { "USA", "Nazi Germany", "Soviet Union", "Great Britain", "Italy", "France", "Japan" };
			if (ModsManager.ModdedCountriesCount > 0)
			{
				hashSet.Add(".Austria");
				hashSet.Add(".France");
				hashSet.Add(".Russia");
				hashSet.Add(".Prussia");
				hashSet.Add(".United Kingdom");
			}
			return hashSet;
		}
	}

	public static HashSet<string> Minors
	{
		get
		{
			HashSet<string> hashSet = new HashSet<string>
			{
				"Afghanistan", "Belgium", "Brazil", "British Raj", "Bulgaria", "Croatia", "China", "Communist China", "Denmark", "Estonia",
				"Finland", "Greece", "Hungary", "Iran", "Iraq", "Ireland", "Latvia", "Lithuania", "Luxembourg", "Manchukuo",
				"Mongolia", "Netherlands", "New Zealand", "Norway", "Philippines", "Portugal", "Romania", "Saudi Arabia", "Slovakia", "Spain",
				"Spanish Republic", "Sweden", "Switzerland", "Thailand", "Yugoslavia"
			};
			if (ModsManager.ModdedCountriesCount > 0)
			{
				hashSet.Add(".Baden");
				hashSet.Add(".Bavaria");
				hashSet.Add(".Berber States");
				hashSet.Add(".Brunswick");
				hashSet.Add(".Denmark");
				hashSet.Add(".Etruria");
				hashSet.Add(".Genoa");
				hashSet.Add(".Great Britain");
				hashSet.Add(".Hannover");
				hashSet.Add(".Hesse");
				hashSet.Add(".Holland");
				hashSet.Add(".Italy");
				hashSet.Add(".Mecklenburg");
				hashSet.Add(".Modena");
				hashSet.Add(".Naples");
				hashSet.Add(".Nassau");
				hashSet.Add(".Netherlands");
				hashSet.Add(".Oldenburg");
				hashSet.Add(".Ottomans");
				hashSet.Add(".Papal States");
				hashSet.Add(".Parma");
				hashSet.Add(".Persia");
				hashSet.Add(".Portugal");
				hashSet.Add(".Sardinia");
				hashSet.Add(".Saxony");
				hashSet.Add(".Sicily");
				hashSet.Add(".Spain");
				hashSet.Add(".Sweden");
				hashSet.Add(".Switzerland");
				hashSet.Add(".Tuscany");
				hashSet.Add(".Venice");
				hashSet.Add(".Warsaw");
				hashSet.Add(".Westphalia");
				hashSet.Add(".Wurttemberg");
				hashSet.Add(".Wurzburg");
				hashSet.Add(".Westphalia");
			}
			return hashSet;
		}
	}

	public static HashSet<string> NavalMajors => new HashSet<string> { "USA", ".USA", "Great Britain", ".Great Britain", "United Kingdom", ".United Kingdom", "Japan", ".Japan", "Italy", ".Italy" };

	public static HashSet<string> NavalMinors => new HashSet<string> { "New Zealand", "Netherlands", "Spain" };

	public static bool TryGetUnit(string p_name, string p_country, out Unit o_unit)
	{
		Unit unit = OfficialUnits.GetInstance().Units.FirstOrDefault((Unit u) => u.Name == p_name && u.Country == p_country);
		if (unit == null)
		{
			o_unit = null;
			return false;
		}
		Unit unit2 = unit.Clone(p_generateNewID: true);
		o_unit = unit2;
		return true;
	}

	public static bool TryGetTile(string p_name, string p_season, out Tile o_tile)
	{
		List<Tile> list = new List<Tile>();
		list.AddRange(OfficialTiles.GetInstance().Tiles);
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.CustomTiles != null && activeMod.CustomTiles.ListOfCustomTiles != null && activeMod.CustomTiles.ListOfCustomTiles.Count > 0)
			{
				list.AddRange(activeMod.CustomTiles.ListOfCustomTiles);
			}
		}
		o_tile = list.FirstOrDefault((Tile t) => t.Name == p_name && t.Season == p_season);
		return o_tile != null;
	}
}
