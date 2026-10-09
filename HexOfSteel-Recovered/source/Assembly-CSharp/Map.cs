using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class Map
{
	public Dictionary<Tile, string> listOfVPThatNeedsToBeHeld;

	public Dictionary<Unit, bool> listOfUnitsThatNeedToBeKilled;

	[NonSerialized]
	public Dictionary<string, int> majorVPsPerFaction;

	[NonSerialized]
	public HashSet<string> generatedRailroadPairs;

	public bool IsTutorial { get; set; }

	public int SizeX { get; set; }

	public int SizeY { get; set; }

	public Tile[,] TilesTable { get; set; }

	public string NextMap { get; set; }

	public string NextMap2 { get; set; }

	public string NextMapDefeat { get; set; }

	public string NextMap2Defeat { get; set; }

	public int NumberOfDaysThatGoesByPerTurn { get; set; }

	public string FileName { get; set; }

	public string ScenarioName { get; set; }

	public string ScenarioDescription { get; set; }

	public string ScenarioVictoryText { get; set; }

	public string ScenarioDefeatText { get; set; }

	public string OriginalCampaignCountry { get; set; }

	public string BackUpPlayerPlaying { get; set; }

	public int turnCount { get; set; }

	public int WeatherSeed { get; set; }

	public byte[] WeatherChanceTable { get; set; }

	public byte BaseWeatherPredictionAccuracy { get; set; }

	public byte MaxTurnsPrediction { get; set; }

	public byte DailyWeatherPredictionAccuracyLoss { get; set; }

	public int EventIDs { get; set; }

	public bool CircularMap { get; set; }

	public bool IsLastScenario { get; set; }

	public bool AllowSnowInWinter { get; set; }

	public bool DisableWeather { get; set; }

	public bool IsOfficial { get; set; }

	public bool IsCampaign { get; set; }

	public bool IsWinter { get; set; }

	public bool XPAwarded { get; set; }

	public bool DisableManpower { get; set; }

	public bool DisableResearch { get; set; }

	public bool DisableUpkeep { get; set; }

	public bool DisablePurchases { get; set; }

	public bool InitializedManpower { get; set; }

	public int NumberOfTurnsToWin { get; set; }

	public int NumberOfDamageToWin { get; set; }

	public int NumberOfTilesToBombToWin { get; set; }

	public string DamageToWinFaction { get; set; }

	public string TilesToBombFaction { get; set; }

	public DateTime Date { get; set; }

	public int XPPerScenario { get; set; }

	public int XPperMajorVP { get; set; }

	public int XPperSecondaryVP { get; set; }

	public int XPperNormalVP { get; set; }

	public int BaseCoreUnitValue { get; set; }

	public int NumberOfNewCoreUnitPerRank { get; set; }

	public float timePlayed { get; set; }

	public float CreationTotalTime { get; set; }

	public float OverlayPosX { get; set; }

	public float OverlayPosY { get; set; }

	public float OverlayScale { get; set; }

	public float OverlayOpacity { get; set; }

	public string OverlayPath { get; set; }

	public List<Event> Events { get; set; }

	public List<Notification> Notifications { get; set; }

	public Map(int p_width, int p_height)
	{
		SizeX = p_width;
		SizeY = p_height;
		TilesTable = new Tile[p_width, p_height];
		Date = new DateTime(1939, 9, 1);
		GenerateDefaultWeatherTable();
		XPPerScenario = 5;
		XPperSecondaryVP = 3;
		BaseCoreUnitValue = 5;
		NumberOfNewCoreUnitPerRank = 2;
		Events = new List<Event>();
		Notifications = new List<Notification>();
		listOfVPThatNeedsToBeHeld = new Dictionary<Tile, string>();
		listOfUnitsThatNeedToBeKilled = new Dictionary<Unit, bool>();
		generatedRailroadPairs = new HashSet<string>();
		DamageToWinFaction = "Both";
		TilesToBombFaction = "Both";
		OverlayOpacity = 1f;
	}

	public void RemoveEvent(int eventID)
	{
		for (int i = 0; i < Events.Count; i++)
		{
			if (Events[i].ID == eventID)
			{
				Events.RemoveAt(i);
				break;
			}
		}
	}

	public void RemoveLowHealthNotificationForUnit(int p_unitID, string p_ownerName, string p_description)
	{
		Notifications.RemoveAll((Notification notification) => notification.Type == NotificationTypes.UNIT_LOW_HEALTH && notification.OwnerName == p_ownerName && ((p_unitID != 0 && notification.UnitID == p_unitID) || (notification.UnitID == 0 && notification.Description == p_description)));
	}

	public int GetNewEventID()
	{
		EventIDs++;
		return EventIDs;
	}

	public void GenerateDefaultWeatherTable()
	{
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
		WeatherSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		WeatherChanceTable = new byte[12]
		{
			35, 30, 35, 40, 15, 8, 5, 15, 35, 50,
			35, 40
		};
		if (NumberOfDaysThatGoesByPerTurn >= 14)
		{
			MaxTurnsPrediction = 3;
			DailyWeatherPredictionAccuracyLoss = 3;
		}
		else if (NumberOfDaysThatGoesByPerTurn >= 7)
		{
			MaxTurnsPrediction = 4;
			DailyWeatherPredictionAccuracyLoss = 4;
		}
		else if (NumberOfDaysThatGoesByPerTurn >= 5)
		{
			MaxTurnsPrediction = 5;
			DailyWeatherPredictionAccuracyLoss = 5;
		}
		else if (NumberOfDaysThatGoesByPerTurn >= 3)
		{
			MaxTurnsPrediction = 6;
			DailyWeatherPredictionAccuracyLoss = 6;
		}
		else if (NumberOfDaysThatGoesByPerTurn >= 2)
		{
			MaxTurnsPrediction = 7;
			DailyWeatherPredictionAccuracyLoss = 7;
		}
		else if (NumberOfDaysThatGoesByPerTurn >= 1)
		{
			MaxTurnsPrediction = 8;
			DailyWeatherPredictionAccuracyLoss = 10;
		}
		else
		{
			MaxTurnsPrediction = 8;
			DailyWeatherPredictionAccuracyLoss = 10;
		}
		BaseWeatherPredictionAccuracy = (byte)Mathf.Clamp(100 - NumberOfDaysThatGoesByPerTurn * DailyWeatherPredictionAccuracyLoss, 0, 100);
	}

	public WeatherPredictionData GetNextTurnWeatherPredictionData()
	{
		return GetWeatherPredictionData(ForecastType.NEXT)[0];
	}

	public WeatherPredictionData GetCurrentTurnWeatherPredictionData()
	{
		if (turnCount <= 1)
		{
			return new WeatherPredictionData
			{
				turnNumber = 1,
				date = Date,
				actualWeather = WeatherType.CLEAR,
				predictedWeather = WeatherType.CLEAR,
				predictionAccuracy = 100f
			};
		}
		return GetWeatherPredictionData(ForecastType.CURRENT)[0];
	}

	public List<WeatherPredictionData> GetWeatherPredictionData(ForecastType p_forecastType)
	{
		List<WeatherPredictionData> list = new List<WeatherPredictionData>();
		byte b = BaseWeatherPredictionAccuracy;
		int num = 1;
		System.Random random = new System.Random(WeatherSeed);
		for (int i = 1; i < turnCount + MaxTurnsPrediction + 1; i++)
		{
			if (i < turnCount)
			{
				random.Next(0, 100);
				continue;
			}
			if (i == turnCount && p_forecastType != ForecastType.CURRENT)
			{
				random.Next(0, 100);
				continue;
			}
			DateTime date = ((p_forecastType != ForecastType.CURRENT || i != turnCount) ? Date.AddDays(NumberOfDaysThatGoesByPerTurn * num) : Date);
			int num2 = DateTime.DaysInMonth(date.Year, date.Month);
			float num3 = random.Next(1, num2 + 1);
			int num4 = (int)((float)(int)WeatherChanceTable[date.Month - 1] / 100f * (float)num2);
			int num5 = new System.Random(i + WeatherSeed).Next(0, 101);
			if (b >= 100 || i == turnCount)
			{
				num5 = b;
			}
			WeatherPredictionData weatherPredictionData = new WeatherPredictionData
			{
				turnNumber = i,
				date = date,
				predictionAccuracy = Utils.Truncate((int)b, 0)
			};
			if (num3 > (float)num4)
			{
				weatherPredictionData.actualWeather = WeatherType.CLEAR;
				if (num5 > b)
				{
					weatherPredictionData.predictedWeather = WeatherType.BAD;
				}
				else
				{
					weatherPredictionData.predictedWeather = WeatherType.CLEAR;
				}
			}
			else
			{
				weatherPredictionData.actualWeather = WeatherType.BAD;
				if (num5 > b)
				{
					weatherPredictionData.predictedWeather = WeatherType.CLEAR;
				}
				else
				{
					weatherPredictionData.predictedWeather = WeatherType.BAD;
				}
			}
			list.Add(weatherPredictionData);
			if (p_forecastType != ForecastType.FULL)
			{
				break;
			}
			b = (byte)Mathf.Clamp(b - DailyWeatherPredictionAccuracyLoss * NumberOfDaysThatGoesByPerTurn, 0, BaseWeatherPredictionAccuracy);
			num++;
		}
		return list;
	}

	public void SetIsWinterBasedOnCurrentDate()
	{
		IsWinter = IsDateWinter(Date);
	}

	public bool IsDateWinter(DateTime p_dateTime)
	{
		if ((p_dateTime.Month != 11 || p_dateTime.Day < 15) && p_dateTime.Month != 12 && p_dateTime.Month != 1 && p_dateTime.Month != 2)
		{
			if (p_dateTime.Month == 3)
			{
				return p_dateTime.Day < 15;
			}
			return false;
		}
		return true;
	}

	public void CheckIncorrectEvents()
	{
		if (Events == null)
		{
			Events = new List<Event>();
			return;
		}
		foreach (Event @event in Events)
		{
			if (@event.EventActions == null)
			{
				@event.EventActions = new List<EventAction>();
			}
			foreach (EventAction eventAction in @event.EventActions)
			{
				if (eventAction.Unit != null)
				{
					eventAction.Unit = eventAction.Unit.Clone(p_generateNewID: true);
				}
			}
		}
	}

	public bool FindTile(int p_posX, int p_posY, out Tile o_tile)
	{
		o_tile = null;
		if (p_posX >= TilesTable.GetLength(0))
		{
			return false;
		}
		if (p_posY >= TilesTable.GetLength(1))
		{
			return false;
		}
		o_tile = TilesTable[p_posX, p_posY];
		return true;
	}

	public bool IsPlayerOnTheMap(Player p_player)
	{
		if (p_player == null)
		{
			Debug.LogWarning("Player is null.");
			return false;
		}
		for (int i = 0; i < SizeX; i++)
		{
			for (int j = 0; j < SizeY; j++)
			{
				if (TilesTable[i, j].IsVictoryPoint && TilesTable[i, j].OwnerName == p_player.Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void EventTriggerVPLoss(Tile p_tile, Player p_playerCapturing, Player p_playerLosing)
	{
		if (MultiplayerManager.IsRealtimeMultiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		foreach (Event @event in Events)
		{
			if (@event.CheckType == CheckTypes.VP_LOSS)
			{
				if (@event.CheckTileCoordinateX == p_tile.PosX && @event.CheckTileCoordinateY == p_tile.PosY)
				{
					TurnManager.instance.ProcessSingleEvent(@event, new EventData(p_playerCapturing, p_playerLosing));
				}
			}
			else if (@event.CheckType == CheckTypes.VPS_LOSSES)
			{
				if (@event.IsValidForLossTrigger(p_tile) && @event.AreAllVPsOccupied(this, p_tile, p_playerCapturing))
				{
					TurnManager.instance.ProcessSingleEvent(@event, new EventData(p_playerCapturing, p_playerLosing));
				}
			}
			else
			{
				if (@event.CheckType != CheckTypes.VPS_LOSSES_ONE_OF)
				{
					continue;
				}
				foreach (Coordinates item in @event.VPsToBeLost)
				{
					if (item.posX == p_tile.PosX && item.posY == p_tile.PosY)
					{
						TurnManager.instance.ProcessSingleEvent(@event, new EventData(p_playerCapturing, p_playerLosing));
						break;
					}
				}
			}
		}
	}

	public int GetFloodFillSize(Tile p_tile)
	{
		for (int i = 0; i < SizeX; i++)
		{
			for (int j = 0; j < SizeY; j++)
			{
				TilesTable[i, j].SetupForAStar();
			}
		}
		Queue<Tile> queue = new Queue<Tile>();
		queue.Enqueue(p_tile);
		int num = 0;
		while (queue.Count > 0)
		{
			foreach (Tile neighbour in queue.Dequeue().neighbours)
			{
				if (neighbour.enumType == p_tile.enumType && !neighbour.visited)
				{
					num++;
					neighbour.visited = true;
					queue.Enqueue(neighbour);
				}
			}
		}
		return num;
	}

	public Dictionary<short, int> GetWaterBodiesSize()
	{
		Dictionary<short, int> dictionary = new Dictionary<short, int>();
		for (int i = 0; i < SizeX; i++)
		{
			for (int j = 0; j < SizeY; j++)
			{
				short? seaAreaIndex = TilesTable[i, j].seaAreaIndex;
				if (seaAreaIndex.HasValue)
				{
					if (dictionary.TryGetValue(seaAreaIndex.Value, out var value))
					{
						dictionary[seaAreaIndex.Value] = value + 1;
					}
					else
					{
						dictionary.Add(seaAreaIndex.Value, 1);
					}
				}
			}
		}
		return dictionary;
	}
}
