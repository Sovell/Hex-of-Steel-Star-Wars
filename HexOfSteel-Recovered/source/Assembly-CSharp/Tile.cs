using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile
{
	[Serializable]
	public readonly struct Coordinates : IEquatable<Coordinates>
	{
		public readonly short X;

		public readonly short Y;

		public Coordinates(short x, short y)
		{
			short x2 = x;
			short y2 = y;
			X = x2;
			Y = y2;
		}

		public Coordinates(Tile p_tile)
			: this(p_tile.PosX, p_tile.PosY)
		{
		}

		public static Coordinates FromTile(Tile p_tile)
		{
			return new Coordinates(p_tile.PosX, p_tile.PosY);
		}

		public bool Equals(Coordinates p_other)
		{
			if (X == p_other.X)
			{
				return Y == p_other.Y;
			}
			return false;
		}

		public override string ToString()
		{
			return $"X: {X}, Y: {Y}";
		}
	}

	[NonSerialized]
	public short? landAreaIndex;

	[NonSerialized]
	public short? seaAreaIndex;

	[NonSerialized]
	public short? riverAreaIndex;

	[NonSerialized]
	public short PosX;

	[NonSerialized]
	public short PosY;

	[NonSerialized]
	public bool isFrozen;

	[NonSerialized]
	public bool isBorderHex;

	[NonSerialized]
	public List<Tile> neighbours;

	[NonSerialized]
	public Player ownerPlayer;

	[NonSerialized]
	public Player terrainOwner;

	[NonSerialized]
	public bool isVisible;

	[NonSerialized]
	public bool isRoadOrPlain;

	[NonSerialized]
	public TileTypes enumType;

	[NonSerialized]
	public TileSeason enumSeason;

	[NonSerialized]
	public Tile parent;

	[NonSerialized]
	public TileGO tileGO;

	[NonSerialized]
	public int cost;

	[NonSerialized]
	public int supplyCost;

	[NonSerialized]
	public bool hasTerrainOwnerPlayer;

	[NonSerialized]
	public bool hasOwnerPlayer;

	[NonSerialized]
	public bool visited;

	[NonSerialized]
	public bool OnQueue;

	[NonSerialized]
	public bool isShortestPath;

	[NonSerialized]
	public bool applyZoneOfControl;

	[NonSerialized]
	public string terrainOwnerName;

	[NonSerialized]
	public byte amountOfNeighbours;

	[NonSerialized]
	public bool isCoastLineTile;

	[NonSerialized]
	public bool isJungleTile;

	[NonSerialized]
	public bool isLakeTile;

	[NonSerialized]
	public bool isNavalConvoyRoute;

	[NonSerialized]
	public bool isOccupied;

	[NonSerialized]
	public bool isPartOfArea;

	[NonSerialized]
	public bool isValidHarbourForAI;

	[NonSerialized]
	public bool isSonarTile;

	[NonSerialized]
	public bool isRADARtile;

	[NonSerialized]
	public bool unreachable;

	[NonSerialized]
	public bool hasRoads;

	[NonSerialized]
	public bool hasBeenLinked;

	public string Name { get; set; }

	public string Type { get; set; }

	public string FilterType { get; set; }

	public string Season { get; set; }

	public string OwnerName { get; set; }

	public string OriginalCountryOwner { get; set; }

	public string CityName { get; set; }

	public byte MPCost { get; set; }

	public byte NumberOfTurnsAtLowProd { get; set; }

	public short DamageModificator { get; set; }

	public short MoneyReward { get; set; }

	public short MoneyProductionPerTurn { get; set; }

	public short OccupiedMoneyProductionPerTurn { get; set; }

	public byte NumberOfTurnsSinceVPGotCaptured { get; set; }

	public int EditorOverwrite_manpower { get; set; }

	public short EditorOverwrite_income { get; set; }

	public int Manpower { get; set; }

	public int OccupiedManpower { get; set; }

	public bool IsVictoryPoint { get; set; }

	public bool PrimaryObjective { get; set; }

	public bool SecondaryObjective { get; set; }

	public bool IsBridgeDestroyed { get; set; }

	public bool IsOfficial { get; set; }

	public bool IsReinforcementTile { get; set; }

	public bool Bridge { get; set; }

	public bool Railroad { get; set; }

	public bool Capturable { get; set; }

	public bool CanFreezeInWinter { get; set; }

	public bool SeasonNeverChanges { get; set; }

	public Unit AirUnit { get; set; }

	public Unit GroundUnit { get; set; }

	public Coordinates GetCoord()
	{
		return Coordinates.FromTile(this);
	}

	public void SetupForAStar()
	{
		parent = null;
		visited = false;
		OnQueue = false;
		cost = int.MaxValue;
		supplyCost = int.MaxValue;
		applyZoneOfControl = false;
		isSonarTile = false;
		isRADARtile = false;
		hasOwnerPlayer = ownerPlayer != null;
		hasTerrainOwnerPlayer = terrainOwner != null;
		terrainOwnerName = (hasOwnerPlayer ? terrainOwner.Name : string.Empty);
	}

	public bool HasEnemyAround(Unit p_unitToCheckWith)
	{
		if (p_unitToCheckWith.Type != "Plane")
		{
			foreach (Tile neighbour in neighbours)
			{
				if (neighbour.GroundUnit != null && neighbour.GroundUnit.OwnerPlayer != null && neighbour.GroundUnit.OwnerPlayer.Faction != p_unitToCheckWith.OwnerPlayer.Faction && neighbour.tileGO.GetComponent<SpriteRenderer>().color != MainMenu.fogOfWarColor)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsWW1TrenchTile()
	{
		if (enumType == TileTypes.TRENCH)
		{
			if (GameData.Instance.map.Date.Year >= 1914)
			{
				return GameData.Instance.map.Date.Year > 1919;
			}
			return true;
		}
		return false;
	}

	public byte GetDestroyedTurnsAmount()
	{
		if (enumType == TileTypes.AIRPORT)
		{
			return 3;
		}
		if (enumType == TileTypes.CITY)
		{
			return 4;
		}
		if (enumType == TileTypes.FACTORY || enumType == TileTypes.REFINERY)
		{
			return 5;
		}
		return 5;
	}

	public bool IsDestroyable()
	{
		if (!Bridge && !IsVictoryPoint)
		{
			return false;
		}
		if (Name.Contains("destroyed"))
		{
			return false;
		}
		if (!Bridge && FilterType != "FilterVP" && FilterType != "FilterHarbour")
		{
			return false;
		}
		if (Utils.GetTileSprite(new Tile
		{
			Season = Season,
			Name = Name + " destroyed",
			Type = Type
		}) == null)
		{
			return false;
		}
		return true;
	}

	public void DestroyTile(bool p_sendRPC)
	{
		if (!Name.Contains("destroyed"))
		{
			Name += " destroyed";
			if (Bridge)
			{
				IsBridgeDestroyed = true;
			}
			NumberOfTurnsAtLowProd = GetDestroyedTurnsAmount();
			tileGO.SetSprite();
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncTile", GetCoord(), this);
			}
		}
	}

	public void SetRuntimeOptimizations()
	{
		SetTileType();
		SetTileSeason();
		isRoadOrPlain = enumType == TileTypes.PLAIN || enumType == TileTypes.ROAD;
		isCoastLineTile = enumType == TileTypes.SEA && FilterType == "FilterCoast";
		isJungleTile = Name.ToLower().Contains("jungle");
		isNavalConvoyRoute = MoneyProductionPerTurn > 0 && IsVictoryPoint && Capturable && enumType == TileTypes.SEA;
	}

	public void SetTileType()
	{
		if (Type == "Sea")
		{
			enumType = TileTypes.SEA;
			isLakeTile = Name == "Lake";
		}
		else if (FilterType == "FilterForest")
		{
			enumType = TileTypes.FOREST;
		}
		else if (Type == "Plain" || Type == "Road" || Type == "Bridge")
		{
			enumType = TileTypes.PLAIN;
		}
		else if (Type == "Road")
		{
			enumType = TileTypes.ROAD;
		}
		else if (Type == "Hill")
		{
			enumType = TileTypes.HILL;
		}
		else if (Type == "Marsh")
		{
			enumType = TileTypes.MARSH;
		}
		else if (Type == "River")
		{
			enumType = TileTypes.RIVER;
		}
		else if (Type == "City")
		{
			enumType = TileTypes.CITY;
		}
		else if (Type == "Airport")
		{
			enumType = TileTypes.AIRPORT;
		}
		else if (Type == "Harbour")
		{
			enumType = TileTypes.HARBOUR;
		}
		else if (Type == "Mountain")
		{
			enumType = TileTypes.MOUNTAIN;
		}
		else if (Type == "Factory")
		{
			enumType = TileTypes.FACTORY;
		}
		else if (Type == "Refinery")
		{
			enumType = TileTypes.REFINERY;
		}
		else if (Type == "Trench")
		{
			enumType = TileTypes.TRENCH;
		}
	}

	public void SetTileSeason()
	{
		if (Season == "Winter")
		{
			enumSeason = TileSeason.WINTER;
		}
		else if (Season == "Desert")
		{
			enumSeason = TileSeason.DESERT;
		}
		else
		{
			enumSeason = TileSeason.SUMMER;
		}
	}

	public byte GetMinimumOccupancyForWork()
	{
		if (enumType == TileTypes.AIRPORT || Name.ToLower().Contains("village"))
		{
			return 2;
		}
		if (enumType == TileTypes.CITY && Name.ToLower().Contains("town"))
		{
			return 3;
		}
		if (enumType == TileTypes.FACTORY || enumType == TileTypes.HARBOUR || (enumType == TileTypes.CITY && Name.ToLower().Contains("city")))
		{
			return 5;
		}
		return 3;
	}

	public string GetLocalizedDisplayName()
	{
		string text;
		if (IsReinforcementTile)
		{
			text = LocalizationManager.Translate("Reinforcement tile");
		}
		else if (Type == "City")
		{
			text = LocalizationManager.Translate(Name.Replace(" destroyed", string.Empty));
		}
		else if (Type == "Trench")
		{
			text = LocalizationManager.Translate("Fortifications");
		}
		else if (Name == "Lake")
		{
			text = LocalizationManager.Translate("Lake");
		}
		else if (Name.Contains("SmallHarbour"))
		{
			text = LocalizationManager.Translate("Small harbour");
		}
		else if (Bridge)
		{
			text = LocalizationManager.Translate("Bridge");
		}
		else if (Railroad)
		{
			text = LocalizationManager.Translate("Railroad");
		}
		else
		{
			text = ((!isNavalConvoyRoute) ? LocalizationManager.Translate(Type) : LocalizationManager.Translate("Naval Convoy Route"));
		}
		if (text == "No translation")
		{
			text = LocalizationManager.Translate(Type);
		}
		if (text.Contains("destroyed"))
		{
			text = text.Replace(" destroyed", "");
		}
		return text;
	}

	public int GetAnnexTileCost(Player p_playerRequesting)
	{
		if (MoneyProductionPerTurn == 0)
		{
			return 750;
		}
		return MoneyProductionPerTurn * 50;
	}

	public int GetRequestOwnershipCost(Player p_playerRequesting)
	{
		if (MoneyProductionPerTurn == 0)
		{
			ownerPlayer.CalculateTurnIncome();
			return ownerPlayer.baseIncome;
		}
		if (OriginalCountryOwner == p_playerRequesting.Country)
		{
			return MoneyProductionPerTurn * 15;
		}
		if (OriginalCountryOwner == ownerPlayer.Country)
		{
			return MoneyProductionPerTurn * 100;
		}
		return MoneyProductionPerTurn * 50;
	}

	public void SetBorderHex()
	{
		isBorderHex = false;
		if (terrainOwner == null)
		{
			return;
		}
		foreach (Tile neighbour in neighbours)
		{
			if (neighbour.terrainOwner != null && neighbour.terrainOwner != terrainOwner)
			{
				isBorderHex = true;
				break;
			}
		}
	}

	public bool IsValidSupplySource()
	{
		if ((IsVictoryPoint && ownerPlayer.isAlliedWithGivenPlayer && (IsReinforcementTile || enumType == TileTypes.CITY || enumType == TileTypes.HARBOUR || enumType == TileTypes.AIRPORT)) || (GroundUnit != null && GroundUnit.IsSupplyDepot && GroundUnit.OwnerPlayer.isAlliedWithGivenPlayer))
		{
			return true;
		}
		return false;
	}

	public float GetSuppliesOutput()
	{
		float num = 0f;
		if (GroundUnit != null && GroundUnit.IsSupplyDepot && GroundUnit.OwnerPlayer.isAlliedWithGivenPlayer)
		{
			num = 1f;
		}
		else
		{
			if (enumType == TileTypes.AIRPORT)
			{
				num = 0.5f;
			}
			else
			{
				string text = Name.ToLower();
				if (text.Contains("village"))
				{
					num = 0.3f;
				}
				else if (text.Contains("small"))
				{
					num = 0.3f;
				}
				else
				{
					num = ((enumType != TileTypes.CITY && enumType != TileTypes.HARBOUR) ? 0.75f : 1f);
				}
			}
			if (NumberOfTurnsAtLowProd > 0)
			{
				num *= 0.25f;
			}
			if (IsVictoryPoint && ownerPlayer != null && !string.IsNullOrEmpty(OriginalCountryOwner) && OriginalCountryOwner != ownerPlayer.Country)
			{
				num *= 0.75f;
			}
		}
		return Mathf.Clamp01(num);
	}

	public int GetTileSuppliesPenalty(Unit p_unit)
	{
		if (Railroad)
		{
			return 0;
		}
		int num = MPCost;
		switch (enumType)
		{
		case TileTypes.RIVER:
		case TileTypes.MARSH:
		case TileTypes.FOREST:
			num *= 2;
			break;
		case TileTypes.HILL:
			num *= 3;
			if (p_unit.IsMountaineer)
			{
				num /= 2;
			}
			break;
		case TileTypes.MOUNTAIN:
			num *= 4;
			if (p_unit.IsMountaineer)
			{
				num /= 2;
			}
			break;
		}
		return num;
	}
}
