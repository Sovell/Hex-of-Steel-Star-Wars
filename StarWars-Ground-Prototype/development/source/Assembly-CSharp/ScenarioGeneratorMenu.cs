using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenarioGeneratorMenu : MonoBehaviour
{
	[Header("Prefabs and containers")]
	[SerializeField]
	private GameObject _playerSlot_prefab;

	[SerializeField]
	private Transform _playersSlotsContainer;

	[Header("Inputfields")]
	[SerializeField]
	private TMP_InputField _seed_inputfield;

	[SerializeField]
	private TMP_InputField _numberOfPlayers_inputfield;

	[SerializeField]
	private TMP_InputField _sizeX_inputfield;

	[SerializeField]
	private TMP_InputField _sizeY_inputfield;

	[SerializeField]
	private TMP_InputField _year_inputfield;

	[SerializeField]
	private TMP_InputField _month_inputfield;

	[SerializeField]
	private TMP_InputField _day_inputfield;

	[SerializeField]
	private TMP_InputField _daysPerTurn_inputfield;

	[Header("Sliders")]
	[SerializeField]
	private Slider _incomeMultiplier_slider;

	[SerializeField]
	private Slider _manpowerMultiplier_slider;

	[SerializeField]
	private Slider _waterAmount_slider;

	[SerializeField]
	private Slider _thickForestAmount_slider;

	[SerializeField]
	private Slider _thinForestAmount_slider;

	[SerializeField]
	private Slider _hillsAmount_slider;

	[SerializeField]
	private Slider _cities_slider;

	[SerializeField]
	private Slider _dunes_slider;

	[SerializeField]
	private Slider _desertRock_slider;

	[SerializeField]
	private Slider _bocage_slider;

	[SerializeField]
	private Slider _marsh_slider;

	[SerializeField]
	private Slider _mountain_slider;

	[SerializeField]
	private Toggle airfieldGenerationToggle;

	[SerializeField]
	private Toggle randomizedCountriesToggle;

	[SerializeField]
	private Toggle randomizedFactionToggle;

	[SerializeField]
	private Toggle factoriesGenerationToggle;

	[SerializeField]
	private Toggle spawnUnitsToggle;

	[SerializeField]
	private Toggle roadGenerationToggle;

	[SerializeField]
	private Toggle railroadsGenerationToggle;

	[SerializeField]
	private Toggle riversGenerationToggle;

	[SerializeField]
	private Toggle islandsOnlyToggle;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI _incomeMultiplier_text;

	[SerializeField]
	private TextMeshProUGUI _manpowerMultiplier_text;

	[SerializeField]
	private TextMeshProUGUI _waterAmount_text;

	[SerializeField]
	private TextMeshProUGUI _thickForestAmount_text;

	[SerializeField]
	private TextMeshProUGUI _thinForestAmount_text;

	[SerializeField]
	private TextMeshProUGUI _hillsAmount_text;

	[SerializeField]
	private TextMeshProUGUI _citiesAmount_text;

	[SerializeField]
	private TextMeshProUGUI _dunesAmount_text;

	[SerializeField]
	private TextMeshProUGUI _desertRockAmount_text;

	[SerializeField]
	private TextMeshProUGUI _mountainAmount_text;

	[SerializeField]
	private TextMeshProUGUI _marshAmount_text;

	[SerializeField]
	private TextMeshProUGUI _bocageAmount_text;

	[Header("Dropdowns")]
	[SerializeField]
	private TMP_Dropdown _biome_dropdown;

	private int _playersAmount;

	private string _season;

	private int _currentScenarioSeed;

	private bool _hasScenarioSeed;

	private void OnEnable()
	{
		_biome_dropdown.ClearOptions();
		foreach (string season in Database.seasons)
		{
			_biome_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(season)));
		}
		_biome_dropdown.SetValueWithoutNotify(0);
		_biome_dropdown.RefreshShownValue();
		LoadScenarioGeneratorData();
	}

	private void OnDisable()
	{
		SaveScenarioGeneratorData();
	}

	private void Start()
	{
		RefreshSliderTexts();
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
	}

	private void LoadScenarioGeneratorData()
	{
		ScenarioGeneratorData scenarioGeneratorData = GetScenarioGeneratorData();
		ApplyScenarioGeneratorData(scenarioGeneratorData);
	}

	private void ApplyScenarioGeneratorData(ScenarioGeneratorData p_scenarioGeneratorData)
	{
		ScenarioGeneratorData scenarioGeneratorData = NormalizeScenarioGeneratorData(p_scenarioGeneratorData);
		int num = Database.seasons.IndexOf(scenarioGeneratorData.season);
		_currentScenarioSeed = scenarioGeneratorData.seed;
		_hasScenarioSeed = true;
		SetSeedInputFieldText(_currentScenarioSeed);
		_numberOfPlayers_inputfield.SetTextWithoutNotify(scenarioGeneratorData.playersData.Length.ToString());
		_sizeX_inputfield.SetTextWithoutNotify(scenarioGeneratorData.sizeX.ToString());
		_sizeY_inputfield.SetTextWithoutNotify(scenarioGeneratorData.sizeY.ToString());
		_year_inputfield.SetTextWithoutNotify(scenarioGeneratorData.year.ToString());
		_month_inputfield.SetTextWithoutNotify(scenarioGeneratorData.month.ToString());
		_day_inputfield.SetTextWithoutNotify(scenarioGeneratorData.day.ToString());
		_daysPerTurn_inputfield.SetTextWithoutNotify(scenarioGeneratorData.daysPerTurn.ToString());
		_incomeMultiplier_slider.SetValueWithoutNotify(scenarioGeneratorData.incomeMultiplier);
		_manpowerMultiplier_slider.SetValueWithoutNotify(scenarioGeneratorData.manpowerMultiplier);
		_waterAmount_slider.SetValueWithoutNotify(scenarioGeneratorData.waterAmount);
		_thickForestAmount_slider.SetValueWithoutNotify(scenarioGeneratorData.thickForestAmount);
		_thinForestAmount_slider.SetValueWithoutNotify(scenarioGeneratorData.thinForestAmount);
		_hillsAmount_slider.SetValueWithoutNotify(scenarioGeneratorData.hillsAmount);
		_cities_slider.SetValueWithoutNotify(scenarioGeneratorData.citiesAmount);
		_dunes_slider.SetValueWithoutNotify(scenarioGeneratorData.dunesAmount);
		_desertRock_slider.SetValueWithoutNotify(scenarioGeneratorData.desertRockAmount);
		_bocage_slider.SetValueWithoutNotify(scenarioGeneratorData.bocageAmount);
		_marsh_slider.SetValueWithoutNotify(scenarioGeneratorData.marshAmount);
		_mountain_slider.SetValueWithoutNotify(scenarioGeneratorData.mountainAmount);
		airfieldGenerationToggle.SetIsOnWithoutNotify(scenarioGeneratorData.generateAirfields);
		factoriesGenerationToggle.SetIsOnWithoutNotify(scenarioGeneratorData.generateFactories);
		spawnUnitsToggle.SetIsOnWithoutNotify(scenarioGeneratorData.spawnUnits);
		roadGenerationToggle.SetIsOnWithoutNotify(scenarioGeneratorData.generateRoads);
		railroadsGenerationToggle.SetIsOnWithoutNotify(scenarioGeneratorData.generateRailroads);
		riversGenerationToggle.SetIsOnWithoutNotify(scenarioGeneratorData.generateRivers);
		islandsOnlyToggle.SetIsOnWithoutNotify(scenarioGeneratorData.isIsland);
		if (num < 0)
		{
			num = 0;
		}
		_biome_dropdown.SetValueWithoutNotify(num);
		_biome_dropdown.RefreshShownValue();
		ApplyPlayersData(scenarioGeneratorData.playersData);
		RefreshSliderTexts();
		RefreshSeedInputField();
	}

	private ScenarioGeneratorData NormalizeScenarioGeneratorData(ScenarioGeneratorData p_scenarioGeneratorData)
	{
		ScenarioGeneratorData scenarioGeneratorData = p_scenarioGeneratorData ?? new ScenarioGeneratorData();
		ScenarioGeneratorData scenarioGeneratorData2 = new ScenarioGeneratorData();
		if (!scenarioGeneratorData.hasSeed)
		{
			scenarioGeneratorData.seed = scenarioGeneratorData2.seed;
			scenarioGeneratorData.hasSeed = true;
		}
		if (scenarioGeneratorData.playersData == null || scenarioGeneratorData.playersData.Length < 2)
		{
			scenarioGeneratorData.playersData = scenarioGeneratorData2.playersData;
		}
		else
		{
			scenarioGeneratorData.playersData = NormalizePlayersData(scenarioGeneratorData.playersData, scenarioGeneratorData2.playersData);
		}
		if (scenarioGeneratorData.sizeX < 5)
		{
			scenarioGeneratorData.sizeX = scenarioGeneratorData2.sizeX;
		}
		if (scenarioGeneratorData.sizeY < 5)
		{
			scenarioGeneratorData.sizeY = scenarioGeneratorData2.sizeY;
		}
		if (scenarioGeneratorData.year < 1)
		{
			scenarioGeneratorData.year = scenarioGeneratorData2.year;
		}
		if (scenarioGeneratorData.month < 1 || scenarioGeneratorData.month > 12)
		{
			scenarioGeneratorData.month = scenarioGeneratorData2.month;
		}
		if (scenarioGeneratorData.day < 1 || scenarioGeneratorData.day > DateTime.DaysInMonth(scenarioGeneratorData.year, scenarioGeneratorData.month))
		{
			scenarioGeneratorData.day = scenarioGeneratorData2.day;
		}
		if (scenarioGeneratorData.daysPerTurn < 0)
		{
			scenarioGeneratorData.daysPerTurn = scenarioGeneratorData2.daysPerTurn;
		}
		if (float.IsNaN(scenarioGeneratorData.incomeMultiplier) || float.IsInfinity(scenarioGeneratorData.incomeMultiplier))
		{
			scenarioGeneratorData.incomeMultiplier = scenarioGeneratorData2.incomeMultiplier;
		}
		else
		{
			scenarioGeneratorData.incomeMultiplier = Mathf.Clamp(scenarioGeneratorData.incomeMultiplier, 0f, 5f);
		}
		if (float.IsNaN(scenarioGeneratorData.manpowerMultiplier) || float.IsInfinity(scenarioGeneratorData.manpowerMultiplier))
		{
			scenarioGeneratorData.manpowerMultiplier = scenarioGeneratorData2.manpowerMultiplier;
		}
		else
		{
			scenarioGeneratorData.manpowerMultiplier = Mathf.Clamp(scenarioGeneratorData.manpowerMultiplier, 0f, 5f);
		}
		if (string.IsNullOrEmpty(scenarioGeneratorData.season) || !Database.seasons.Contains(scenarioGeneratorData.season))
		{
			scenarioGeneratorData.season = scenarioGeneratorData2.season;
		}
		return scenarioGeneratorData;
	}

	private ScenarioGeneratorData.PlayerData[] NormalizePlayersData(ScenarioGeneratorData.PlayerData[] p_playersData, ScenarioGeneratorData.PlayerData[] p_defaultPlayersData)
	{
		ScenarioGeneratorData.PlayerData[] array = new ScenarioGeneratorData.PlayerData[p_playersData.Length];
		bool flag = false;
		for (int i = 0; i < p_playersData.Length; i++)
		{
			ScenarioGeneratorData.PlayerData playerData = p_playersData[i];
			ScenarioGeneratorData.PlayerData playerData2 = p_defaultPlayersData[Mathf.Min(i, p_defaultPlayersData.Length - 1)];
			if (string.IsNullOrEmpty(playerData.country))
			{
				playerData.country = playerData2.country;
			}
			if (string.IsNullOrEmpty(playerData.faction))
			{
				playerData.faction = playerData2.faction;
			}
			if (playerData.isPlayingFirst)
			{
				flag = true;
			}
			array[i] = playerData;
		}
		if (!flag && array.Length != 0)
		{
			array[0].isPlayingFirst = true;
		}
		return array;
	}

	private void ApplyPlayersData(ScenarioGeneratorData.PlayerData[] p_playersData)
	{
		ScenarioGeneratorData scenarioGeneratorData = new ScenarioGeneratorData();
		ScenarioGeneratorData.PlayerData[] array = p_playersData;
		if (array == null || array.Length < 2)
		{
			array = scenarioGeneratorData.playersData;
		}
		_playersAmount = array.Length;
		_numberOfPlayers_inputfield.SetTextWithoutNotify(_playersAmount.ToString());
		GameData.Instance.listOfPlayers = new List<Player>();
		for (int i = 0; i < array.Length; i++)
		{
			ScenarioGeneratorData.PlayerData playerData = array[i];
			Player player = new Player("Player" + (i + 1));
			player.Country = playerData.country;
			player.Faction = playerData.faction;
			player.IsPlayingFirst = playerData.isPlayingFirst;
			player.IsComputer = playerData.isComputer;
			GameData.Instance.listOfPlayers.Add(player);
		}
		RefreshPlayersSlotUI();
	}

	private ScenarioGeneratorData GetScenarioGeneratorData()
	{
		if (PlayerSettings.Instance == null)
		{
			return new ScenarioGeneratorData();
		}
		if (PlayerSettings.Instance.ScenarioGeneratorData == null)
		{
			PlayerSettings.Instance.ScenarioGeneratorData = new ScenarioGeneratorData();
		}
		else
		{
			PlayerSettings.Instance.ScenarioGeneratorData = NormalizeScenarioGeneratorData(PlayerSettings.Instance.ScenarioGeneratorData);
		}
		return PlayerSettings.Instance.ScenarioGeneratorData;
	}

	private void SaveScenarioGeneratorData()
	{
		if (PlayerSettings.Instance != null)
		{
			PlayerSettings.Instance.ScenarioGeneratorData = CreateScenarioGeneratorDataFromUI();
			PlayerSettings.Save();
		}
	}

	private ScenarioGeneratorData CreateScenarioGeneratorDataFromUI()
	{
		ScenarioGeneratorData scenarioGeneratorData = new ScenarioGeneratorData();
		ScenarioGeneratorData scenarioGeneratorData2 = new ScenarioGeneratorData();
		List<Player> listOfPlayers = GameData.Instance.listOfPlayers;
		string season = scenarioGeneratorData.season;
		if (_biome_dropdown != null && _biome_dropdown.value >= 0 && _biome_dropdown.value < Database.seasons.Count)
		{
			season = Database.seasons[_biome_dropdown.value];
		}
		scenarioGeneratorData2.seed = GetScenarioSeedForPersistence();
		scenarioGeneratorData2.hasSeed = true;
		scenarioGeneratorData2.sizeX = ParseInputFieldValue(_sizeX_inputfield, scenarioGeneratorData.sizeX);
		scenarioGeneratorData2.sizeY = ParseInputFieldValue(_sizeY_inputfield, scenarioGeneratorData.sizeY);
		scenarioGeneratorData2.year = ParseInputFieldValue(_year_inputfield, scenarioGeneratorData.year);
		scenarioGeneratorData2.month = ParseInputFieldValue(_month_inputfield, scenarioGeneratorData.month);
		scenarioGeneratorData2.day = ParseInputFieldValue(_day_inputfield, scenarioGeneratorData.day);
		scenarioGeneratorData2.daysPerTurn = ParseInputFieldValue(_daysPerTurn_inputfield, scenarioGeneratorData.daysPerTurn);
		scenarioGeneratorData2.incomeMultiplier = _incomeMultiplier_slider.value;
		scenarioGeneratorData2.manpowerMultiplier = _manpowerMultiplier_slider.value;
		scenarioGeneratorData2.season = season;
		scenarioGeneratorData2.waterAmount = (int)_waterAmount_slider.value;
		scenarioGeneratorData2.thickForestAmount = (int)_thickForestAmount_slider.value;
		scenarioGeneratorData2.thinForestAmount = (int)_thinForestAmount_slider.value;
		scenarioGeneratorData2.hillsAmount = (int)_hillsAmount_slider.value;
		scenarioGeneratorData2.citiesAmount = (int)_cities_slider.value;
		scenarioGeneratorData2.dunesAmount = (int)_dunes_slider.value;
		scenarioGeneratorData2.desertRockAmount = (int)_desertRock_slider.value;
		scenarioGeneratorData2.bocageAmount = (int)_bocage_slider.value;
		scenarioGeneratorData2.marshAmount = (int)_marsh_slider.value;
		scenarioGeneratorData2.mountainAmount = (int)_mountain_slider.value;
		scenarioGeneratorData2.generateAirfields = airfieldGenerationToggle.isOn;
		scenarioGeneratorData2.generateFactories = factoriesGenerationToggle.isOn;
		scenarioGeneratorData2.spawnUnits = spawnUnitsToggle.isOn;
		scenarioGeneratorData2.generateRoads = roadGenerationToggle.isOn;
		scenarioGeneratorData2.generateRailroads = railroadsGenerationToggle.isOn;
		scenarioGeneratorData2.generateRivers = riversGenerationToggle.isOn;
		scenarioGeneratorData2.isIsland = islandsOnlyToggle.isOn;
		if (listOfPlayers == null || listOfPlayers.Count == 0)
		{
			scenarioGeneratorData2.playersData = scenarioGeneratorData.playersData;
			return scenarioGeneratorData2;
		}
		scenarioGeneratorData2.playersData = new ScenarioGeneratorData.PlayerData[listOfPlayers.Count];
		for (int i = 0; i < listOfPlayers.Count; i++)
		{
			scenarioGeneratorData2.playersData[i] = new ScenarioGeneratorData.PlayerData
			{
				country = listOfPlayers[i].Country,
				faction = listOfPlayers[i].Faction,
				isPlayingFirst = listOfPlayers[i].IsPlayingFirst,
				isComputer = listOfPlayers[i].IsComputer
			};
		}
		return scenarioGeneratorData2;
	}

	public void OnSeedInputFieldValueChanged()
	{
		if (int.TryParse(_seed_inputfield.text, out var result))
		{
			_currentScenarioSeed = result;
			_hasScenarioSeed = true;
		}
	}

	private int GetScenarioSeedForPersistence()
	{
		if (_hasScenarioSeed)
		{
			return _currentScenarioSeed;
		}
		if (TryGetSeedFromInputField(out var o_seed))
		{
			_currentScenarioSeed = o_seed;
			_hasScenarioSeed = true;
			return _currentScenarioSeed;
		}
		_currentScenarioSeed = GenerateNewScenarioSeed();
		_hasScenarioSeed = true;
		return _currentScenarioSeed;
	}

	private int ParseInputFieldValue(TMP_InputField p_inputField, int p_fallbackValue)
	{
		if (p_inputField == null)
		{
			return p_fallbackValue;
		}
		if (int.TryParse(p_inputField.text, out var result))
		{
			return result;
		}
		return p_fallbackValue;
	}

	private void RefreshSliderTexts()
	{
		_incomeMultiplier_text.text = Utils.Truncate(_incomeMultiplier_slider.value * 100f, 0) + "%";
		_manpowerMultiplier_text.text = Utils.Truncate(_manpowerMultiplier_slider.value * 100f, 0) + "%";
		_waterAmount_text.text = _waterAmount_slider.value + "%";
		_thickForestAmount_text.text = _thickForestAmount_slider.value + "%";
		_thinForestAmount_text.text = _thinForestAmount_slider.value + "%";
		_hillsAmount_text.text = _hillsAmount_slider.value + "%";
		_citiesAmount_text.text = _cities_slider.value + "%";
		_dunesAmount_text.text = _dunes_slider.value + "%";
		_desertRockAmount_text.text = _desertRock_slider.value + "%";
		_mountainAmount_text.text = _mountain_slider.value + "%";
		_bocageAmount_text.text = _bocage_slider.value + "%";
		_marshAmount_text.text = _marsh_slider.value + "%";
	}

	public void StartGenerationProcess_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		if (AreWeGoodToStartGenerating())
		{
			ResetGameDataForGeneration();
			int num = ResolveScenarioSeed();
			UnityEngine.Random.InitState(num);
			Debug.Log("Scenario generator seed: " + num);
			InitializePlayers();
			InitializeMap(num);
			GiveMoneyToPlayers();
			SpawnGarrisons();
			GenerateRoads();
			GenerateRailroads();
			GenerateRivers();
			new ScenarioTerrainStage().FinalizeBiomesAfterRivers(GameData.Instance.map, CreateGenerationRequest(num));
			PrepareScenarioForRuntime();
			Utils.UpdateTilesData(p_nonDestructive: true);
			SaveScenario();
			StartScenario();
		}
	}

	private void ResetGameDataForGeneration()
	{
		List<Player> listOfPlayers = GameData.Instance.listOfPlayers;
		List<Player> list = new List<Player>(listOfPlayers.Count);
		foreach (Player item2 in listOfPlayers)
		{
			Player item = new Player(item2.Name)
			{
				Country = item2.Country,
				Faction = item2.Faction,
				IsPlayingFirst = item2.IsPlayingFirst,
				IsComputer = item2.IsComputer
			};
			list.Add(item);
		}
		GameData.ResetForScenarioCreation(list);
	}

	private void InitializePlayers()
	{
		if (!randomizedFactionToggle.isOn)
		{
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.Faction = listOfPlayer.Country;
		}
	}

	private void GiveMoneyToPlayers()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.MoneyAmount = 0;
			int num = 0;
			for (int i = 0; i < GameData.Instance.map.SizeX; i++)
			{
				for (int j = 0; j < GameData.Instance.map.SizeY; j++)
				{
					if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OwnerName == listOfPlayer.Name)
					{
						num += GameData.Instance.map.TilesTable[i, j].MoneyProductionPerTurn;
					}
				}
			}
			listOfPlayer.MoneyAmount += num * 5;
		}
	}

	private void InitializeMap(int p_seed)
	{
		Map map = new Map(int.Parse(_sizeX_inputfield.text), int.Parse(_sizeY_inputfield.text));
		map.Date = new DateTime(int.Parse(_year_inputfield.text), int.Parse(_month_inputfield.text), int.Parse(_day_inputfield.text));
		map.NumberOfDaysThatGoesByPerTurn = int.Parse(_daysPerTurn_inputfield.text);
		map.ScenarioName = LocalizationManager.Translate("txt-scenario") + ": " + LocalizationManager.GetLocalizedShortDate(map.Date);
		GameData.Instance.map = map;
		new RuntimeScenarioGenerator().Generate(CreateGenerationRequest(p_seed));
		Utils.UpdateTilesData(p_nonDestructive: true);
	}

	private void SaveScenario()
	{
		string text = $"{DateTime.Now.Day:D2}-{DateTime.Now.Month:D2}-{DateTime.Now.Year:D4} {DateTime.Now.Hour:D2}h{DateTime.Now.Minute:D2}";
		string text2 = "New scenario - " + text;
		GameData.Instance.SaveAsScenarioFile(text2);
		MainMenu.selectedMap = new FileInfo(PrototypePaths.DataPath + "/Maps/" + text2 + ".sav");
	}

	private void PrepareScenarioForRuntime()
	{
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		foreach (Tile tile in tilesTable)
		{
			tile.neighbours = null;
			tile.isPartOfArea = false;
			tile.landAreaIndex = null;
			tile.seaAreaIndex = null;
			tile.riverAreaIndex = null;
			tile.ownerPlayer = null;
			tile.terrainOwner = null;
			tile.terrainOwnerName = null;
		}
	}

	private void StartScenario()
	{
		MainMenu.isLoadedGame = false;
		MainMenu.instance.loading_Panel.SetActive(value: true);
		SceneManager.LoadScene("Game");
	}

	private ScenarioGenerationRequest CreateGenerationRequest(int p_runtimeSeed)
	{
		return new ScenarioGenerationRequest
		{
			RuntimeSeed = p_runtimeSeed,
			SizeX = GameData.Instance.map.SizeX,
			SizeY = GameData.Instance.map.SizeY,
			PlayersAmount = GameData.Instance.listOfPlayers.Count,
			IncomeMultiplier = _incomeMultiplier_slider.value,
			ManpowerMultiplier = _manpowerMultiplier_slider.value,
			Season = _season,
			WaterAmount = (int)_waterAmount_slider.value,
			ThickForestAmount = (int)_thickForestAmount_slider.value,
			ThinForestAmount = (int)_thinForestAmount_slider.value,
			HillsAmount = (int)_hillsAmount_slider.value,
			CitiesAmount = (int)_cities_slider.value,
			DunesAmount = (int)_dunes_slider.value,
			DesertRockAmount = (int)_desertRock_slider.value,
			BocageAmount = (int)_bocage_slider.value,
			MarshAmount = (int)_marsh_slider.value,
			MountainAmount = (int)_mountain_slider.value,
			GenerateAirfields = airfieldGenerationToggle.isOn,
			GenerateFactories = factoriesGenerationToggle.isOn,
			GenerateSeas = islandsOnlyToggle.isOn
		};
	}

	private void RefreshSeedInputField()
	{
		if (TryGetSeedFromInputField(out var o_seed))
		{
			_currentScenarioSeed = o_seed;
			_hasScenarioSeed = true;
		}
		else if (!_hasScenarioSeed)
		{
			_currentScenarioSeed = GenerateNewScenarioSeed();
			_hasScenarioSeed = true;
		}
		SetSeedInputFieldText(_currentScenarioSeed);
	}

	private int ResolveScenarioSeed()
	{
		if (TryGetSeedFromInputField(out var o_seed))
		{
			_currentScenarioSeed = o_seed;
			_hasScenarioSeed = true;
		}
		else
		{
			if (!_hasScenarioSeed)
			{
				_currentScenarioSeed = GenerateNewScenarioSeed();
			}
			_hasScenarioSeed = true;
		}
		SetSeedInputFieldText(_currentScenarioSeed);
		return _currentScenarioSeed;
	}

	private bool TryGetSeedFromInputField(out int o_seed)
	{
		o_seed = 0;
		if (_seed_inputfield == null)
		{
			return false;
		}
		return int.TryParse(_seed_inputfield.text, out o_seed);
	}

	private void SetSeedInputFieldText(int p_seed)
	{
		if (!(_seed_inputfield == null))
		{
			_seed_inputfield.SetTextWithoutNotify(p_seed.ToString());
		}
	}

	private int GenerateNewScenarioSeed()
	{
		return Guid.NewGuid().GetHashCode();
	}

	private void GenerateRivers()
	{
		if (riversGenerationToggle.isOn)
		{
			int targetRivers = UnityEngine.Random.Range(1, Mathf.Max(GameData.Instance.map.SizeX * GameData.Instance.map.SizeY / 250, 1));
			RiverGenerator.GenerateRivers(GameData.Instance.map, targetRivers);
			Utils.UpdateTilesData(p_nonDestructive: true);
		}
	}

	private void GenerateRailroads()
	{
		if (!railroadsGenerationToggle.isOn || GameData.Instance.map.Date.Year < 1900)
		{
			return;
		}
		int num = 0;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		int upperBound = tilesTable.GetUpperBound(0);
		int upperBound2 = tilesTable.GetUpperBound(1);
		for (int i = tilesTable.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = tilesTable.GetLowerBound(1); j <= upperBound2; j++)
			{
				if (RailroadGenerator.IsMajorRailroadNode(tilesTable[i, j]))
				{
					num++;
				}
			}
		}
		if (num >= 2)
		{
			int p_targetRailroads = UnityEngine.Random.Range(1, num / 5 + 1);
			RailroadGenerator.GenerateRailroads(GameData.Instance.map, p_targetRailroads, RailroadGenerator.IsMajorRailroadNode, null, null);
			Utils.UpdateTilesData(p_nonDestructive: true);
		}
	}

	private void GenerateRoads()
	{
		if (!roadGenerationToggle.isOn)
		{
			return;
		}
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				Tile tile = GameData.Instance.map.TilesTable[i, j];
				if (!tile.IsVictoryPoint || tile.hasBeenLinked || (!(tile.Type == "City") && !(tile.Type == "Harbour")))
				{
					continue;
				}
				bool flag = false;
				foreach (Tile neighbour in tile.neighbours)
				{
					if (neighbour.Type != "Sea" && neighbour.Type != "Mountain")
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				foreach (Tile item in Utils.GetAllTilesInRadiusAround(tile, 30))
				{
					if ((!(item.Type == "City") && !(item.Type == "Harbour")) || item.hasBeenLinked)
					{
						continue;
					}
					Queue<Tile> queue = new Queue<Tile>();
					for (int k = 0; k < GameData.Instance.map.SizeX; k++)
					{
						for (int l = 0; l < GameData.Instance.map.SizeY; l++)
						{
							GameData.Instance.map.TilesTable[k, l].OnQueue = false;
							GameData.Instance.map.TilesTable[k, l].cost = int.MaxValue;
							GameData.Instance.map.TilesTable[k, l].parent = null;
						}
					}
					Tile tile2 = tile;
					Tile tile3 = item;
					tile2.cost = 0;
					queue.Enqueue(tile2);
					while (queue.Count != 0)
					{
						Tile tile4 = queue.Dequeue();
						tile4.OnQueue = false;
						foreach (Tile neighbour2 in tile4.neighbours)
						{
							int num = 0;
							bool flag2 = true;
							if (neighbour2.Type == "Sea" || neighbour2.Type == "Mountain")
							{
								flag2 = false;
							}
							else if (neighbour2.IsVictoryPoint || neighbour2.hasRoads)
							{
								num = num;
							}
							else if (neighbour2.Type == "River")
							{
								num += 30;
							}
							else
							{
								num = ((!(neighbour2.Type == "Plain")) ? (num + neighbour2.MPCost * 5) : (num + 5));
							}
							int num2 = tile4.cost + num;
							if ((num2 < neighbour2.cost) & flag2)
							{
								neighbour2.cost = num2;
								neighbour2.parent = tile4;
								if (!neighbour2.OnQueue)
								{
									queue.Enqueue(neighbour2);
									neighbour2.OnQueue = true;
								}
							}
						}
					}
					if (tile3.parent == null)
					{
						continue;
					}
					List<Tile> list = new List<Tile>();
					while (tile3 != null)
					{
						list.Add(tile3);
						tile3 = tile3.parent;
					}
					list.Reverse();
					list.RemoveAt(0);
					foreach (Tile item2 in list)
					{
						if (!item2.IsVictoryPoint)
						{
							item2.hasRoads = true;
						}
					}
					tile.hasBeenLinked = true;
					break;
				}
			}
		}
		for (int m = 0; m < GameData.Instance.map.SizeX; m++)
		{
			for (int n = 0; n < GameData.Instance.map.SizeY; n++)
			{
				Tile tile5 = GameData.Instance.map.TilesTable[m, n];
				if (!tile5.hasRoads)
				{
					continue;
				}
				int num3 = 0;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				bool flag10 = false;
				bool flag11 = false;
				bool flag12 = false;
				bool flag13 = false;
				bool flag14 = false;
				foreach (Tile neighbour3 in tile5.neighbours)
				{
					if (!neighbour3.IsVictoryPoint && !neighbour3.hasRoads)
					{
						continue;
					}
					num3++;
					if (neighbour3.PosX == tile5.PosX && neighbour3.PosY == tile5.PosY + 1)
					{
						flag3 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag9 = true;
						}
					}
					else if (neighbour3.PosX == tile5.PosX && neighbour3.PosY == tile5.PosY - 1)
					{
						flag4 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag10 = true;
						}
					}
					if (tile5.PosX % 2 == 0)
					{
						if (neighbour3.PosX == tile5.PosX - 1 && neighbour3.PosY == tile5.PosY + 1)
						{
							flag6 = true;
							if (neighbour3.IsVictoryPoint)
							{
								flag12 = true;
							}
						}
						else if (neighbour3.PosX == tile5.PosX - 1 && neighbour3.PosY == tile5.PosY)
						{
							flag8 = true;
							if (neighbour3.IsVictoryPoint)
							{
								flag14 = true;
							}
						}
						else if (neighbour3.PosX == tile5.PosX + 1 && neighbour3.PosY == tile5.PosY + 1)
						{
							flag5 = true;
							if (neighbour3.IsVictoryPoint)
							{
								flag11 = true;
							}
						}
						else if (neighbour3.PosX == tile5.PosX + 1 && neighbour3.PosY == tile5.PosY)
						{
							flag7 = true;
							if (neighbour3.IsVictoryPoint)
							{
								flag13 = true;
							}
						}
					}
					else if (neighbour3.PosX == tile5.PosX - 1 && neighbour3.PosY == tile5.PosY)
					{
						flag6 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag12 = true;
						}
					}
					else if (neighbour3.PosX == tile5.PosX - 1 && neighbour3.PosY == tile5.PosY - 1)
					{
						flag8 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag14 = true;
						}
					}
					else if (neighbour3.PosX == tile5.PosX + 1 && neighbour3.PosY == tile5.PosY)
					{
						flag5 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag11 = true;
						}
					}
					else if (neighbour3.PosX == tile5.PosX + 1 && neighbour3.PosY == tile5.PosY - 1)
					{
						flag7 = true;
						if (neighbour3.IsVictoryPoint)
						{
							flag13 = true;
						}
					}
				}
				if (num3 >= 4)
				{
					int num4 = 0;
					while (num3 >= 4)
					{
						if (flag9)
						{
							flag3 = false;
						}
						else if (flag10)
						{
							flag4 = false;
						}
						else if (flag11)
						{
							flag5 = false;
						}
						else if (flag13)
						{
							flag7 = false;
						}
						else if (flag14)
						{
							flag8 = false;
						}
						else if (flag12)
						{
							flag6 = false;
						}
						num3--;
						num4++;
						if (num4 > 3)
						{
							if (flag3)
							{
								flag3 = false;
							}
							else if (flag4)
							{
								flag4 = false;
							}
							else if (flag5)
							{
								flag5 = false;
							}
							else if (flag7)
							{
								flag7 = false;
							}
							else if (flag7)
							{
								flag8 = false;
							}
							else if (flag6)
							{
								flag6 = false;
							}
							num3--;
						}
					}
				}
				if (num3 == 0)
				{
					break;
				}
				tile5.Name = "Road";
				tile5.Type = "Road";
				switch (num3)
				{
				case 2:
					if (flag3 & flag4)
					{
						tile5.Name += "1";
					}
					else if (flag5 & flag6)
					{
						tile5.Name += "2";
					}
					else if (flag7 & flag8)
					{
						tile5.Name += "3";
					}
					else if (flag5 & flag8)
					{
						tile5.Name += "4";
					}
					else if (flag6 & flag7)
					{
						tile5.Name += "5";
					}
					else if (flag4 & flag6)
					{
						tile5.Name += "10";
					}
					else if (flag3 & flag8)
					{
						tile5.Name += "11";
					}
					else if (flag3 & flag7)
					{
						tile5.Name += "12";
					}
					else if (flag4 & flag5)
					{
						tile5.Name += "13";
					}
					break;
				case 3:
					if (flag3 & flag4 & flag7)
					{
						tile5.Name += "6";
					}
					else if (flag3 & flag4 & flag5)
					{
						tile5.Name += "7";
					}
					else if (flag3 & flag4 & flag6)
					{
						tile5.Name += "8";
					}
					else if (flag3 & flag4 & flag8)
					{
						tile5.Name += "9";
					}
					else if (flag3 & flag8 & flag7)
					{
						tile5.Name += "14";
					}
					else if (flag4 & flag6 & flag5)
					{
						tile5.Name += "15";
					}
					else if (flag8 & flag7 & flag5)
					{
						tile5.Name += "16";
					}
					else if (flag8 & flag3 & flag5)
					{
						tile5.Name += "17";
					}
					else if (flag6 & flag7 & flag5)
					{
						tile5.Name += "18";
					}
					else if (flag4 & flag6 & flag7)
					{
						tile5.Name += "19";
					}
					else if (flag4 & flag8 & flag5)
					{
						tile5.Name += "20";
					}
					else if (flag6 & flag8 & flag5)
					{
						tile5.Name += "21";
					}
					else if (flag6 & flag3 & flag7)
					{
						tile5.Name += "22";
					}
					else if (flag8 & flag6 & flag7)
					{
						tile5.Name += "23";
					}
					else if (flag5 & flag3 & flag6)
					{
						tile5.Name += "2";
					}
					else if (flag7 & flag4 & flag8)
					{
						tile5.Name += "3";
					}
					else if (flag4 & flag6 & flag8)
					{
						tile5.Name += "10";
					}
					else if (flag3 & flag8 & flag6)
					{
						tile5.Name += "11";
					}
					else if (flag3 & flag5 & flag7)
					{
						tile5.Name += "12";
					}
					else if (flag4 & flag7 & flag5)
					{
						tile5.Name += "13";
					}
					break;
				default:
					tile5.Name = "Plain";
					tile5.Type = "Plain";
					break;
				case 4:
					break;
				}
			}
		}
		Utils.UpdateTilesData(p_nonDestructive: true);
	}

	private void SpawnGarrisons()
	{
		if (!spawnUnitsToggle.isOn)
		{
			return;
		}
		List<Unit> allUnits = Utils.GetAllUnits();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			List<Unit> list = new List<Unit>();
			foreach (Unit item in allUnits)
			{
				if (item.MaxPurchaseTime <= 0 && !item.IsPartisans && !item.IsCommander && !item.IsCavalry && !(item.Country != listOfPlayer.Country) && (!(item.FilterType != "FilterInfantry") || !(item.FilterType != "FilterAA")) && !(item.Type == "Boat") && !(item.Type == "Plane") && !(item.Type == "Tank") && !(item.Name == "Garrison") && !item.IsEngineers && !(item.Name == "Militia") && !(item.Name == "Volkssturm") && !item.Name.Contains("AT infantry") && new DateTime(item.ProductionYear, Mathf.Max(item.ProductionMonth, 1), 1) <= GameData.Instance.map.Date && GameData.Instance.map.Date.Year <= item.EndUseYear)
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				continue;
			}
			for (int i = 0; i < GameData.Instance.map.SizeX; i++)
			{
				for (int j = 0; j < GameData.Instance.map.SizeY; j++)
				{
					Tile tile = GameData.Instance.map.TilesTable[i, j];
					if (!tile.IsVictoryPoint || tile.Type == "Sea" || tile.OwnerName != listOfPlayer.Name)
					{
						continue;
					}
					string text = "";
					if (tile.Type == "Airport" || tile.Type == "Factory" || tile.Type == "Refinery")
					{
						int num = 0;
						foreach (Unit item2 in list)
						{
							if (item2.FilterType == "FilterAA")
							{
								num++;
							}
						}
						if (num > 0)
						{
							text = "FilterAA";
						}
					}
					if (text == "")
					{
						text = "FilterInfantry";
					}
					int index = UnityEngine.Random.Range(0, list.Count);
					int num2 = 0;
					while (list[index].FilterType != text)
					{
						index = UnityEngine.Random.Range(0, list.Count);
						if (list[index].FilterType == text || num2 > 1000)
						{
							break;
						}
						num2++;
					}
					Unit unit = list[index].Clone(p_generateNewID: true);
					unit.GenerateNewID();
					unit.Lvl = 1;
					unit.XP = 0;
					unit.CustomName = null;
					unit.CalculateAndSetUnitPrice();
					unit.IsDefensive = true;
					tile.GroundUnit = unit;
					listOfPlayer.AddUnit(unit, p_sendRPC: false);
				}
			}
		}
	}

	public void ManagePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(!gameObject.activeSelf);
	}

	public void ChangeWaterAmountSlider()
	{
		_waterAmount_text.text = _waterAmount_slider.value + "%";
	}

	public void ChangeThickForestAmountSlider()
	{
		_thickForestAmount_text.text = _thickForestAmount_slider.value + "%";
	}

	public void ChangeThinForestAmountSlider()
	{
		_thinForestAmount_text.text = _thinForestAmount_slider.value + "%";
	}

	public void ChangeHillsAmountSlider()
	{
		_hillsAmount_text.text = _hillsAmount_slider.value + "%";
	}

	public void ChangeDunesAmountSlider()
	{
		_dunesAmount_text.text = _dunes_slider.value + "%";
	}

	public void ChangeDesertRocksAmountSlider()
	{
		_desertRockAmount_text.text = _desertRock_slider.value + "%";
	}

	public void ChangeCitiesAmountSlider()
	{
		_citiesAmount_text.text = _cities_slider.value + "%";
	}

	public void ChangeMountainAmountSlider()
	{
		_mountainAmount_text.text = _mountain_slider.value + "%";
	}

	public void ChangeBocageAmountSlider()
	{
		_bocageAmount_text.text = _bocage_slider.value + "%";
	}

	public void ChangeMarshAmountSlider()
	{
		_marshAmount_text.text = _marsh_slider.value + "%";
	}

	public void ChangeIncomeMultiplierSlider()
	{
		_incomeMultiplier_text.text = Utils.Truncate(_incomeMultiplier_slider.value * 100f, 0) + "%";
	}

	public void ChangeManpowerMultiplierSlider()
	{
		_manpowerMultiplier_text.text = Utils.Truncate(_manpowerMultiplier_slider.value * 100f, 0) + "%";
	}

	public void RefreshPlayersSlotUI()
	{
		Transform[] componentsInChildren = _playersSlotsContainer.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != _playersSlotsContainer)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_playerSlot_prefab, _playersSlotsContainer);
			gameObject.name = listOfPlayer.Name;
			componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform2 in componentsInChildren)
			{
				if (transform2.gameObject.name == "Player name text")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = listOfPlayer.Name;
				}
				else if (transform2.gameObject.name == "Plays first toggle")
				{
					if (listOfPlayer.IsPlayingFirst)
					{
						transform2.GetComponent<Toggle>().isOn = true;
					}
					transform2.GetComponent<Toggle>().onValueChanged.AddListener(delegate
					{
						SetPlaysFirstForPlayer();
					});
				}
				else if (transform2.gameObject.name == "AI playing toggle")
				{
					if (listOfPlayer.IsComputer)
					{
						transform2.GetComponent<Toggle>().isOn = true;
					}
					transform2.GetComponent<Toggle>().onValueChanged.AddListener(delegate
					{
						SetIsAIForPlayer();
					});
				}
				else if (transform2.gameObject.name == "Countries dropdown")
				{
					foreach (string country in Database.Countries)
					{
						transform2.gameObject.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
					}
					if (listOfPlayer.Country != null)
					{
						transform2.gameObject.GetComponent<TMP_Dropdown>().SetValueWithoutNotify(Database.Countries.IndexOf(listOfPlayer.Country));
					}
					transform2.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate
					{
						SelectCountryForPlayer();
					});
				}
				else
				{
					if (!(transform2.gameObject.name == "Factions dropdown"))
					{
						continue;
					}
					foreach (string faction in Database.factions)
					{
						transform2.gameObject.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedFaction(faction)));
					}
					if (listOfPlayer.Faction != null)
					{
						transform2.gameObject.GetComponent<TMP_Dropdown>().SetValueWithoutNotify(Database.factions.IndexOf(listOfPlayer.Faction));
					}
					transform2.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate
					{
						SelectFactionForPlayer();
					});
				}
			}
		}
	}

	public void ChangeAmountOfPlayers()
	{
		try
		{
			int count = Database.Countries.Count;
			_playersAmount = int.Parse(_numberOfPlayers_inputfield.text);
			_playersAmount = Mathf.Clamp(_playersAmount, 2, count);
			_numberOfPlayers_inputfield.SetTextWithoutNotify(_playersAmount.ToString());
			GameData.Instance.listOfPlayers = new List<Player>();
			for (int i = 1; i < _playersAmount + 1; i++)
			{
				Player item = new Player("Player" + i);
				GameData.Instance.listOfPlayers.Add(item);
			}
			RefreshPlayersSlotUI();
		}
		catch
		{
		}
	}

	public void SelectCountryForPlayer()
	{
		GameObject gameObject = EventSystem.current.currentSelectedGameObject.transform.parent.transform.parent.transform.parent.transform.parent.transform.parent.gameObject;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!(gameObject.name == listOfPlayer.Name))
			{
				continue;
			}
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Countries dropdown")
				{
					listOfPlayer.Country = Database.Countries[transform.GetComponent<TMP_Dropdown>().value];
				}
			}
		}
	}

	public void SelectFactionForPlayer()
	{
		GameObject gameObject = EventSystem.current.currentSelectedGameObject.transform.parent.transform.parent.transform.parent.transform.parent.transform.parent.gameObject;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!(gameObject.name == listOfPlayer.Name))
			{
				continue;
			}
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Factions dropdown")
				{
					listOfPlayer.Faction = Database.factions[transform.GetComponent<TMP_Dropdown>().value];
				}
			}
		}
	}

	public void SetPlaysFirstForPlayer()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.name;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name == text)
			{
				listOfPlayer.IsPlayingFirst = true;
			}
			else
			{
				listOfPlayer.IsPlayingFirst = false;
			}
		}
		RefreshPlayersSlotUI();
	}

	public void SetIsAIForPlayer()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.name;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name == text)
			{
				listOfPlayer.IsComputer = !listOfPlayer.IsComputer;
			}
		}
	}

	public void SetRandomCountryForAll()
	{
		if (GameData.Instance.listOfPlayers == null || GameData.Instance.listOfPlayers.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>(Database.Countries);
		list.Remove("None");
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			try
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				listOfPlayer.Country = list[index];
				list.RemoveAt(index);
			}
			catch (Exception ex)
			{
				UIManager.ShowMessage(ex.Message);
			}
		}
		RefreshPlayersSlotUI();
	}

	public void SetAIForAll()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.IsComputer = !listOfPlayer.IsComputer;
		}
		RefreshPlayersSlotUI();
	}

	private bool AreWeGoodToStartGenerating()
	{
		if (!int.TryParse(_numberOfPlayers_inputfield.text, out var _))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-wrong players count"));
			return false;
		}
		if (int.Parse(_numberOfPlayers_inputfield.text) < 2)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-wrong players count"));
			return false;
		}
		if (!int.TryParse(_sizeX_inputfield.text, out var _) || !int.TryParse(_sizeY_inputfield.text, out var _))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-wrong map size"));
			return false;
		}
		if (int.Parse(_sizeX_inputfield.text) < 5 || int.Parse(_sizeY_inputfield.text) < 5)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-wrong map size"));
			return false;
		}
		try
		{
			new DateTime(int.Parse(_year_inputfield.text), int.Parse(_month_inputfield.text), int.Parse(_day_inputfield.text));
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-wrong date"));
			return false;
		}
		if (!randomizedFactionToggle.isOn)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.Faction == null)
				{
					UIManager.ShowMessage(listOfPlayer.Name + LocalizationManager.Translate("error-player no faction"));
					return false;
				}
			}
		}
		bool flag = false;
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer2.IsPlayingFirst)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no playing first"));
			return false;
		}
		foreach (Player listOfPlayer3 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer3.Country == null)
			{
				UIManager.ShowMessage(listOfPlayer3.Name + LocalizationManager.Translate("error-player no country"));
				return false;
			}
		}
		_season = Database.seasons[_biome_dropdown.value];
		_playersAmount = int.Parse(_numberOfPlayers_inputfield.text);
		return true;
	}
}
