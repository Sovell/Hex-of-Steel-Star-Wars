using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameData
{
	public const string EXTENSION_NAME = ".sav";

	private static GameData _instance;

	public bool isPBEM;

	public Guid sessionID;

	public Map map;

	public List<Player> listOfPlayers;

	public GameSettings gameSettings;

	public List<string> messages;

	public int tutorialStep;

	public bool offlinePBEM;

	public bool ironManMode;

	public bool endOfTurnPanelOpen;

	public bool gameFinishedButContinue;

	public bool violatedIronmanRule;

	private ModPropertyBag _modDataBag;

	[NonSerialized]
	public bool isFromMapEditor;

	private static bool _settingsRestoreHooked;

	public static GameData Instance => _instance ?? (_instance = new GameData());

	public ModPropertyBag ModDataBag => _modDataBag ?? (_modDataBag = new ModPropertyBag());

	public bool TryFindPlayerByCountry(string p_country, out Player o_player)
	{
		o_player = null;
		foreach (Player listOfPlayer in listOfPlayers)
		{
			if (listOfPlayer.Country == p_country)
			{
				o_player = listOfPlayer;
				return true;
			}
		}
		return false;
	}

	public bool TryFindPlayerById(Guid p_userId, out Player o_player)
	{
		o_player = null;
		foreach (Player listOfPlayer in listOfPlayers)
		{
			if (listOfPlayer.UserId == p_userId)
			{
				o_player = listOfPlayer;
				return true;
			}
		}
		return false;
	}

	public bool TryFindLocalPlayer(out Player o_localPlayer)
	{
		o_localPlayer = null;
		foreach (Player listOfPlayer in listOfPlayers)
		{
			if (listOfPlayer.IsLocalClientPlayer())
			{
				o_localPlayer = listOfPlayer;
				return true;
			}
		}
		return false;
	}

	public bool TryFindPlayerByName(string p_playerName, out Player o_player)
	{
		o_player = null;
		foreach (Player listOfPlayer in listOfPlayers)
		{
			if (listOfPlayer.Name == p_playerName)
			{
				o_player = listOfPlayer;
				return true;
			}
		}
		return false;
	}

	public Event GetEvent(int p_ID)
	{
		foreach (Event @event in map.Events)
		{
			if (@event.ID == p_ID)
			{
				return @event;
			}
		}
		return null;
	}

	public bool IsIronManModeON(out List<InvalidIronManReasons> o_ironManOFFReasons)
	{
		o_ironManOFFReasons = new List<InvalidIronManReasons>();
		if (violatedIronmanRule)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.VIOLATED_RULE);
		}
		if (!PlayerSettings.Instance.IsFogOfWar)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.FOW);
		}
		if (!PlayerSettings.Instance.ComplexSupplies)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.REALISTIC_SUPPLIES);
		}
		if (PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.INFINITE_AMMO_FUEL);
		}
		if (PlayerSettings.Instance.DifficultyLevel < 2)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.DIFFICULTY);
		}
		if (!PlayerSettings.Instance.IsWeather)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.WEATHER);
		}
		if (!PlayerSettings.Instance.ZoneOfControl)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.ZOC);
		}
		if (!PlayerSettings.Instance.Upkeep)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.UPKEEP);
		}
		if (!PlayerSettings.Instance.ProductionTakesTime)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.PRODUCTION_TAKES_TIME);
		}
		if (!PlayerSettings.Instance.ISHistoricalFocus)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.HISTORICAL_FOCUS);
		}
		if (map != null && !map.IsOfficial)
		{
			o_ironManOFFReasons.Add(InvalidIronManReasons.OFFICIAL_MAP);
		}
		if (listOfPlayers != null)
		{
			bool flag = true;
			foreach (Player listOfPlayer in listOfPlayers)
			{
				if (!listOfPlayer.IsComputer)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
			}
			bool flag2 = true;
			foreach (Player listOfPlayer2 in listOfPlayers)
			{
				if (listOfPlayer2.IsComputer)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
			}
			List<string> list = new List<string>();
			foreach (Player listOfPlayer3 in listOfPlayers)
			{
				listOfPlayer3.SetupIdeology();
				if (!listOfPlayer3.IsComputer && !list.Contains(listOfPlayer3.Ideology))
				{
					list.Add(listOfPlayer3.Ideology);
				}
			}
			if (list.Contains("Fascism") || list.Contains("National Socialism"))
			{
				if (list.Contains("Democracy") || list.Contains("Communism"))
				{
					o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
				}
			}
			else if (list.Contains("Democracy"))
			{
				if (list.Contains("Fascism") || list.Contains("National Socialism") || list.Contains("Communism"))
				{
					o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
				}
			}
			else if (list.Contains("Communism") && (list.Contains("Fascism") || list.Contains("National Socialism") || list.Contains("Democracy")))
			{
				o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
			}
			foreach (Player listOfPlayer4 in listOfPlayers)
			{
				if (listOfPlayer4.IsComputer)
				{
					if (listOfPlayer4.IncomeModifier < 10)
					{
						o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
						break;
					}
				}
				else if (listOfPlayer4.IncomeModifier > 10)
				{
					o_ironManOFFReasons.Add(InvalidIronManReasons.SCENARIO_SETUP);
					break;
				}
			}
		}
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.HasGameplayImpact())
			{
				o_ironManOFFReasons.Add(InvalidIronManReasons.MODS);
				break;
			}
		}
		return o_ironManOFFReasons.Count == 0;
	}

	public void SetIronManMode(bool p_value)
	{
		ironManMode = p_value;
	}

	public void GenerateGameSettings(bool p_lockSaveFile)
	{
		gameSettings = default;
		gameSettings.saveLocked = p_lockSaveFile;
		if (gameSettings.saveLocked)
		{
			gameSettings.researchON = PlayerSettings.Instance.TechTree;
			gameSettings.manpowerON = PlayerSettings.Instance.Manpower;
			gameSettings.policiesON = PlayerSettings.Instance.Policies;
			gameSettings.weatherON = PlayerSettings.Instance.IsWeather;
			gameSettings.fogOfWarON = PlayerSettings.Instance.IsFogOfWar;
			gameSettings.partisansON = PlayerSettings.Instance.Partisans;
			gameSettings.realisticPlanesON = PlayerSettings.Instance.RealisticPlanes;
			gameSettings.overrunON = PlayerSettings.Instance.Overrun;
			gameSettings.productionTakesTimeON = PlayerSettings.Instance.ProductionTakesTime;
			gameSettings.infiniteAmmoAndFuelON = PlayerSettings.Instance.IsInfiniteAmmoAndFuel;
			gameSettings.complexSuppliesON = PlayerSettings.Instance.ComplexSupplies;
			gameSettings.planeMoveAfterAttackON = PlayerSettings.Instance.OneMoreTilePlanes;
			gameSettings.moraleON = PlayerSettings.Instance.IsMorale;
			gameSettings.historicalFocusON = PlayerSettings.Instance.ISHistoricalFocus;
			gameSettings.winterDamageValue = PlayerSettings.Instance.WinterDamageValue;
			gameSettings.difficultyValue = PlayerSettings.Instance.DifficultyLevel;
			gameSettings.randomDamageValue = PlayerSettings.Instance.RandomDamageModifier;
			gameSettings.upkeepON = PlayerSettings.Instance.Upkeep;
			gameSettings.zoneOfControlON = PlayerSettings.Instance.ZoneOfControl;
			gameSettings.damageReductionRepeatedAttacks = PlayerSettings.Instance.DamageReductionRepeatedAttacks;
			gameSettings.finiteManpower = PlayerSettings.Instance.FiniteManpower;
			gameSettings.prototypesDisabled = PlayerSettings.Instance.PrototypesDisabled;
			gameSettings.noAAsupportDuringAirFight = PlayerSettings.Instance.NoAAsupportDuringAirFight;
			gameSettings.stricterFogOfWar = PlayerSettings.Instance.StricterFogOfWar;
		}
	}

	public void Save(FileInfo p_fileInfo)
	{
		if (p_fileInfo == null)
		{
			Debug.LogError("Save FileInfo is null or empty.");
		}
		else
		{
			SaveData(p_fileInfo.FullName);
		}
	}

	public void SaveAsScenarioFile(string p_fileName)
	{
		if (string.IsNullOrEmpty(p_fileName))
		{
			Debug.LogError("Save file name is null or empty.");
			return;
		}
		string text = Path.Combine(PrototypePaths.DataPath, "Maps");
		Directory.CreateDirectory(text);
		string p_fullPath = Path.Combine(text, p_fileName + ".sav");
		SaveData(p_fullPath);
	}

	public void SaveAsSaveFile(string p_fileName)
	{
		if (string.IsNullOrEmpty(p_fileName))
		{
			Debug.LogError("Save file name is null or empty.");
			return;
		}
		string text = ((!isPBEM && !MultiplayerManager.IsRealtimeMultiplayer()) ? Path.Combine(PrototypePaths.DataPath, "Saves") : Path.Combine(PrototypePaths.DataPath, "Saves", "Multiplayer"));
		Directory.CreateDirectory(text);
		string p_fullPath = Path.Combine(text, p_fileName + ".sav");
		SaveData(p_fullPath);
	}

	public void SaveToCloud(string p_fileName)
	{
		if (string.IsNullOrEmpty(p_fileName))
		{
			Debug.LogError("Save file name is null or empty.");
			return;
		}
		string text = Path.Combine(PrototypePaths.DataPath, "Saves", "Steam Cloud");
		Directory.CreateDirectory(text);
		string p_fullPath = Path.Combine(text, p_fileName + ".sav");
		SaveData(p_fullPath);
		SteamManager.instance.UploadSaveFileToSteamCloud(p_fileName, this);
	}

	private void SaveData(string p_fullPath)
	{
		if (string.IsNullOrEmpty(p_fullPath))
		{
			Debug.LogError("Save file full path is null or empty.");
			return;
		}
		if (map.Events == null)
		{
			map.Events = new List<Event>();
		}
		try
		{
			string directoryName = Path.GetDirectoryName(p_fullPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using (FileStream stream = new FileStream(p_fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using (DeflateStream serializationStream = new DeflateStream(stream, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: false))
				{
					new BinaryFormatter().Serialize(serializationStream, this);
				}
			}
		}
		catch (SerializationException ex)
		{
			Debug.LogError("SerializationException when writing save data to disk: " + ex.Message);
			throw;
		}
		catch (IOException ex2)
		{
			Debug.LogError("IO exception when writing save data to disk: " + ex2.Message);
			throw;
		}
	}

	private async Task SaveDataAsync(string p_fullPath)
	{
		byte[] payload;
		try
		{
			using (MemoryStream memoryStream = new MemoryStream(65536))
			{
				new BinaryFormatter().Serialize(memoryStream, this);
				payload = memoryStream.ToArray();
			}
		}
		catch (SerializationException ex)
		{
			throw new SerializationException("Failed to serialize snapshot: " + ex.Message, ex);
		}
		await Task.Run(() =>
		{
			try
			{
				using (FileStream stream = new FileStream(p_fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, useAsync: false))
				{
					using (DeflateStream deflateStream = new DeflateStream(stream, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: false))
					{
						deflateStream.Write(payload, 0, payload.Length);
					}
				}
			}
			catch (IOException ex2)
			{
				throw new IOException("I/O error while writing save data: " + ex2.Message, ex2);
			}
		});
	}

	public void Load(string p_filePath)
	{
		if (string.IsNullOrWhiteSpace(p_filePath) || !File.Exists(p_filePath))
		{
			throw new FileNotFoundException("Save file not found.", p_filePath);
		}
		GameData data;
		try
		{
			data = (GameData)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_filePath));
		}
		catch (InvalidDataException ex)
		{
			Debug.LogError("Compressed save is corrupted: " + ex.Message);
			throw;
		}
		catch (SerializationException ex2)
		{
			Debug.LogError("Save file is corrupted or incompatible: " + ex2.Message);
			throw;
		}
		catch (IOException ex3)
		{
			Debug.LogError("Could not read save file: " + ex3.Message);
			throw;
		}
		SetData(data);
	}

	public void SetData(GameData p_gameData)
	{
		isFromMapEditor = false;
		isPBEM = p_gameData.isPBEM;
		sessionID = p_gameData.sessionID;
		map = p_gameData.map;
		_modDataBag = p_gameData._modDataBag;
		listOfPlayers = p_gameData.listOfPlayers;
		messages = p_gameData.messages;
		gameSettings = p_gameData.gameSettings;
		tutorialStep = p_gameData.tutorialStep;
		offlinePBEM = p_gameData.offlinePBEM;
		ironManMode = p_gameData.ironManMode;
		endOfTurnPanelOpen = p_gameData.endOfTurnPanelOpen;
		gameFinishedButContinue = p_gameData.gameFinishedButContinue;
		violatedIronmanRule = p_gameData.violatedIronmanRule;
		SanitizeData(this);
	}

	public static void BackupPlayerSettingsForSession()
	{
		if (!MainMenu.gameSettingsBackup.isBackup)
		{
			MainMenu.gameSettingsBackup.Backup();
		}
		if (!_settingsRestoreHooked)
		{
			_settingsRestoreHooked = true;
			SceneManager.activeSceneChanged += OnActiveSceneChanged_RestoreSettings;
		}
	}

	public static void RestorePlayerSettingsBackup()
	{
		if (_settingsRestoreHooked)
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged_RestoreSettings;
			_settingsRestoreHooked = false;
		}
		if (MainMenu.gameSettingsBackup.isBackup)
		{
			MainMenu.gameSettingsBackup.LoadBackup();
		}
	}

	private static void OnActiveSceneChanged_RestoreSettings(Scene p_previousScene, Scene p_nextScene)
	{
		if (!(p_nextScene.name == "Game"))
		{
			RestorePlayerSettingsBackup();
		}
	}

	public static void SanitizeData(GameData p_gameData)
	{
		if (p_gameData.map == null)
		{
			Debug.LogError("Loaded GameData has a null map.");
			return;
		}
		if (p_gameData.map.turnCount == 0)
		{
			p_gameData.map.turnCount = 1;
		}
		foreach (Player listOfPlayer in p_gameData.listOfPlayers)
		{
			if (string.IsNullOrWhiteSpace(listOfPlayer.Faction))
			{
				Debug.LogWarning("Player '" + listOfPlayer.Name + "' has no faction. Defaulting to Neutral.");
				listOfPlayer.Faction = "Neutral";
			}
			if (listOfPlayer.ClearedFOW == null)
			{
				listOfPlayer.ClearedFOW = new List<Tile.Coordinates>();
			}
			if (listOfPlayer.ListOfUnitsResearched == null)
			{
				listOfPlayer.ListOfUnitsResearched = new List<string>();
			}
			if (listOfPlayer.ListActivePolicies == null)
			{
				listOfPlayer.ListActivePolicies = new List<Policy>();
			}
			if (listOfPlayer.Stats == null)
			{
				listOfPlayer.Stats = new Dictionary<ushort, Statistic>();
			}
			if (listOfPlayer.PurchasedUnits == null)
			{
				listOfPlayer.PurchasedUnits = new Dictionary<string, int>();
			}
			if (listOfPlayer.LineDatas == null)
			{
				listOfPlayer.LineDatas = new List<LineData>();
			}
			MigratePlayerResearchData(listOfPlayer);
		}
		if (p_gameData.map.Notifications == null)
		{
			p_gameData.map.Notifications = new List<Notification>();
		}
		if (p_gameData.map.Events == null)
		{
			p_gameData.map.Events = new List<Event>();
		}
		if (p_gameData.map.listOfUnitsThatNeedToBeKilled == null)
		{
			p_gameData.map.listOfUnitsThatNeedToBeKilled = new Dictionary<Unit, bool>();
		}
		if (p_gameData.map.WeatherChanceTable == null)
		{
			p_gameData.map.GenerateDefaultWeatherTable();
		}
		if (p_gameData.map.timePlayed <= 0f && !MultiplayerManager.IsRealtimeMultiplayer())
		{
			UnityEngine.Random.InitState(Environment.TickCount);
			p_gameData.map.WeatherSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
		p_gameData.map.CheckIncorrectEvents();
	}

	private static void MigratePlayerResearchData(Player p_player)
	{
		if (p_player == null)
		{
			return;
		}
		if (p_player.ListOfUnitsResearched != null)
		{
			List<string> list = new List<string>();
			foreach (string item in p_player.ListOfUnitsResearched)
			{
				string updatedUnitName = Utils.GetUpdatedUnitName(item, p_player.Country);
				if (!list.Contains(updatedUnitName))
				{
					list.Add(updatedUnitName);
				}
			}
			p_player.ListOfUnitsResearched = list;
		}
		if (p_player.CurrentUnitsResearch == null)
		{
			return;
		}
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		foreach (KeyValuePair<string, float> item2 in p_player.CurrentUnitsResearch)
		{
			string updatedUnitName2 = Utils.GetUpdatedUnitName(item2.Key, p_player.Country);
			if (dictionary.ContainsKey(updatedUnitName2))
			{
				dictionary[updatedUnitName2] = Mathf.Max(dictionary[updatedUnitName2], item2.Value);
			}
			else
			{
				dictionary.Add(updatedUnitName2, item2.Value);
			}
		}
		p_player.CurrentUnitsResearch = dictionary;
	}

	public void ResetData()
	{
		_instance = new GameData();
	}

	public static void ResetForScenarioCreation(List<Player> p_players)
	{
		if (p_players == null)
		{
			throw new ArgumentNullException("p_players");
		}
		_instance = new GameData
		{
			listOfPlayers = p_players,
			messages = new List<string>()
		};
		_instance.GenerateGameSettings(p_lockSaveFile: false);
	}
}
