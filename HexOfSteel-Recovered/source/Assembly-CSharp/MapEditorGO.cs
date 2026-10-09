using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapEditorGO : MonoBehaviour
{
	private struct RailroadUndoBatch
	{
		public List<TileUndoState> TileStates;

		public string PairKey;
	}

	public static Map mapCopy;

	public static MapEditorGO instance;

	public static Unit selectedUnitForRename;

	public static bool isAlwaysWinter;

	public static bool setTileValueBoolean;

	public static PaintMode paintMode;

	public static GameObject selectedTileGO;

	public static GameObject selectedPlacedTile;

	public static Tile selectedTile;

	[HideInInspector]
	public static string filter;

	public static string season;

	public static GameObject previousTile;

	public static string previousTileRoad;

	[Header("Setup")]
	public GameObject UI;

	public Transform unitParent;

	[Header("Prefabs")]
	public TileEditorGO tilePrefab;

	public GameObject unitPrefab;

	public GameObject cityName_Holder;

	[Header("Panels")]
	public GameObject store_Panel;

	public GameObject reserve_Panel;

	public GameObject devMenuPanel;

	public GameObject tooltip;

	public GameObject generators_Panel;

	[Header("Save as panel")]
	public GameObject saveAsPanel;

	public TMP_InputField saveAs_inputField;

	[Header("Brush")]
	public static int brushSize;

	public TextMeshProUGUI brushSize_Text;

	public GameObject brush;

	public Image brush_image;

	public GameObject selectedTileIcon;

	public Sprite[] brusheTypes_sprites;

	[Header("Scenario settings")]
	public TextMeshProUGUI mapCreationTime_text;

	public GameObject scenarioSettings_Panel;

	public GameObject campaignBox;

	public PlayerSlotMapEditor _playerSlot_prefab;

	public Toggle circularMap_toggle;

	public Toggle snowInWinter_toggle;

	public Toggle weatherDisabled_toggle;

	public TMP_InputField XPperScenario_InputField;

	public TMP_InputField XPperSecondaryVP_InputField;

	public TMP_InputField baseCoreUnitsValue_InputField;

	public TMP_InputField coreUnitsPerRank_InputField;

	public TMP_InputField yearMap_InputField;

	public TMP_InputField monthMap_InputField;

	public TMP_InputField dayMap_InputField;

	public TMP_InputField numberOfTurnsToWin_InputField;

	public TMP_InputField damageToWin_InputField;

	public TMP_InputField tilesToBombToWin_InputField;

	public TMP_InputField inputDaysThatGoByPerTurn_InputField;

	public TMP_InputField newSizeLEFT;

	public TMP_InputField newSizeRIGHT;

	public TMP_InputField newSizeUP;

	public TMP_InputField newSizeDOWN;

	public List<TMP_Dropdown> teamsDropdowns;

	[Header("Events")]
	public TMP_Dropdown tilesToBombToWin_dropdown;

	public TMP_Dropdown damageToWin_dropdown;

	public TextMeshProUGUI selectedTileCoordinates_text;

	public EventPanelGO events_panel;

	public Transform eventsParent;

	[Header("Tile right click")]
	public TextMeshProUGUI mapSize_Text;

	public TMP_InputField cityName_InputField;

	public TMP_InputField originalOwnerCountry_inputfield;

	public TMP_InputField customNameForUnit_Inputfield;

	public TMP_InputField customHealthForUnit_Inputfield;

	public TMP_InputField customManpower_Inputfield;

	public TMP_InputField customIncome_Inputfield;

	public TMP_InputField unitID_Inputfield;

	public TMP_InputField entrenchmentOnStart_Inputfield;

	public TMP_InputField unitLevelOnStart_Inputfield;

	public GameObject rightClick_Panel;

	public GameObject removeUnitButton;

	public GameObject relocateUnitButton;

	public GameObject addLevelButton;

	public GameObject removeLevelButton;

	public GameObject noAmmoTurn1Button;

	public GameObject noEntrenchmentTurn1Button;

	public GameObject setVpToBeHeldButton;

	[Header("Campaign panel")]
	public TMP_InputField nextMapVictory1_inputfield;

	public TMP_InputField nextMapVictory2_inputfield;

	public TMP_InputField nextMapDefeat1_inputfield;

	public TMP_InputField nextMapDefeat2_inputfield;

	public Toggle isOfficial;

	public Toggle isCampaign;

	public Toggle disableManpower;

	public Toggle disableResearch;

	public Toggle disableUpkeep;

	public Toggle disablePurchases;

	[Header("Weather panel")]
	[SerializeField]
	private GameObject _weatherPanel;

	[SerializeField]
	private TMP_InputField _weatherPrediction_maxTurnsForecast_inputfield;

	[SerializeField]
	private TMP_InputField _weatherPrediction_baseAccuracyValue_inputfield;

	[SerializeField]
	private TMP_InputField _weatherPrediction_dailyAccuracyLoss_inputfield;

	[SerializeField]
	private List<TMP_InputField> _weatherMonths_inputfields;

	public string country;

	public GameObject tileChoiceContainer;

	public GameObject parent;

	public Transform listOfPlayersContainer;

	[Header("Error window")]
	public GameObject errorPanel;

	public TextMeshProUGUI error_Description;

	[Header("Scenario descriptions etc")]
	public GameObject scenarioDescription_panel;

	public TMP_Dropdown playerListScenarioTexts_dropdown;

	public TMP_InputField scenarioName_InputField;

	public TMP_InputField scenarioDescription_InputField;

	[Header("Filters")]
	public Sprite road_WinterSprite;

	public Sprite road_SummerSprite;

	public Sprite road_DesertSprite;

	public GameObject roadGO;

	public Sprite city_WinterSprite;

	public Sprite city_SummerSprite;

	public Sprite city_DesertSprite;

	public GameObject cityGO;

	public Sprite river_WinterSprite;

	public Sprite river_SummerSprite;

	public Sprite river_DesertSprite;

	public GameObject riverGO;

	public Sprite junction_WinterSprite;

	public Sprite junction_SummerSprite;

	public GameObject junctionGO;

	public Sprite railways_WinterSprite;

	public Sprite railways_SummerSprite;

	public Sprite railways_DesertSprite;

	public GameObject railwaysGO;

	public Sprite forest_WinterSprite;

	public Sprite forest_SummerSprite;

	public GameObject forestGO;

	public Sprite defense_WinterSprite;

	public Sprite defense_SummerSprite;

	public Sprite defense_DesertSprite;

	public GameObject defenseGO;

	public Sprite harbour_WinterSprite;

	public Sprite harbour_SummerSprite;

	public Sprite harbour_DesertSprite;

	public GameObject harbourGO;

	public Sprite terrain_WinterSprite;

	public Sprite terrain_SummerSprite;

	public Sprite terrain_DesertSprite;

	public GameObject terrainGO;

	public Sprite roadBridge_WinterSprite;

	public Sprite roadBridge_SummerSprite;

	public Sprite roadBridge_DesertSprite;

	public GameObject roadBridgeGO;

	public Sprite RailroadBridge_WinterSprite;

	public Sprite RailroadBridge_SummerSprite;

	public Sprite RailroadBridge_DesertSprite;

	public GameObject RailroadBridgeGO;

	public GameObject coastGO;

	public Sprite coast_SummerSprite;

	public Sprite coast_WinterSprite;

	public Sprite coast_DesertSprite;

	public TileEditorGO[,] TableOfAllTiles;

	private int _citiesCount;

	private string _buttonChoice;

	private string _selectedTextEdition;

	private float _mapCreationTimer = 1f;

	private Vector3 _lastMousePosition;

	private List<Tile> _listOfTilesToChooseFrom;

	private List<string> _availableTeamsSecondary;

	private Stack<List<TileUndoState>> _riverUndoHistory = new Stack<List<TileUndoState>>();

	private Stack<RailroadUndoBatch> _railroadUndoHistory = new Stack<RailroadUndoBatch>();

	private bool _confirmationParameter1;

	private void Awake()
	{
		instance = this;
		filter = "FilterVP";
		brushSize = 1;
		brushSize_Text.text = "1";
		season = "Summer";
		selectedUnitForRename = null;
		MainMenu.isMainMenu = false;
		MainMenu.isEditor = true;
		UIManager.isUIOpen = false;
		paintMode = PaintMode.TILE_PLACING;
		_lastMousePosition = Input.mousePosition;
		_availableTeamsSecondary = new List<string> { "Both", "Axis", "Allies" };
		Populate();
		CreateTheMap();
		SetMiniatureFilter();
		if (MapEditorMenu.isLoadedMapEditor && GameData.Instance.map.NumberOfDaysThatGoesByPerTurn != 0)
		{
			inputDaysThatGoByPerTurn_InputField.SetTextWithoutNotify(GameData.Instance.map.NumberOfDaysThatGoesByPerTurn.ToString());
		}
		TurnManager.currPlayer = null;
		mapSize_Text.text = LocalizationManager.Translate("txt-map size") + " " + GameData.Instance.map.SizeX + " x " + GameData.Instance.map.SizeY;
	}

	private void Start()
	{
		Application.targetFrameRate = PlayerSettings.Instance.MaxFramerate;
		MapEditorMenu.isLoadedMapEditor = false;
		GenerateBorders();
		selectedPlacedTile = null;
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			int count = listOfPlayer.ListOfUnits.Count;
			for (int i = 0; i < count; i++)
			{
				if (i < listOfPlayer.ListOfUnits.Count && !listOfPlayer.ListOfUnits[i].IsReserve && !GameObject.Find(listOfPlayer.ListOfUnits[i].ID.ToString()))
				{
					Debug.Log("Cannot find " + listOfPlayer.ListOfUnits[i].Name + " " + listOfPlayer.ListOfUnits[i].ID);
					listOfPlayer.ListOfUnits.Remove(listOfPlayer.ListOfUnits[i]);
				}
			}
		}
		SteamManager.SetRichPresenceStatus(SteamStatuses.CREATING_MAP);
		GameData.Instance.gameFinishedButContinue = false;
	}

	private void Update()
	{
		UpdateMapCreationTime();
		if (EventSystem.current.currentSelectedGameObject == null || (!EventSystem.current.currentSelectedGameObject.TryGetComponent<InputField>(out var _) && !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _)))
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				SetPaintMode(PaintMode.TILE_PLACING);
			}
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				SetPaintMode(PaintMode.CAN_FREEZE);
			}
			else if (Input.GetKey(KeyCode.Alpha3))
			{
				SetPaintMode(PaintMode.CAN_SEASON_CHANGE);
			}
			else if (Input.GetKey(KeyCode.Alpha4))
			{
				SetPaintMode(PaintMode.UNIT_REMOVAL);
			}
		}
		brush_image.sprite = brusheTypes_sprites[(int)paintMode];
		if (Input.GetKeyDown(PlayerSettings.Instance.ShowHideUI))
		{
			ManageUI();
		}
		if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftMeta) || Input.GetKey(KeyCode.LeftControl)))
		{
			SaveTheMap();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ManageMapSettingsMenu();
			if (scenarioDescription_panel.activeSelf)
			{
				scenarioDescription_panel.SetActive(value: false);
			}
			if (errorPanel.activeSelf)
			{
				errorPanel.SetActive(value: false);
			}
			if (devMenuPanel.activeSelf)
			{
				devMenuPanel.SetActive(value: false);
			}
			if (saveAsPanel.activeSelf)
			{
				saveAsPanel.SetActive(value: false);
			}
			UIManager.HideToolTip();
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.HideAllUnits) && (EventSystem.current.currentSelectedGameObject == null || (!EventSystem.current.currentSelectedGameObject.TryGetComponent<InputField>(out var _) && !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _))))
		{
			ShowHideAllUnitsOnTheMap();
		}
		brush.SetActive(paintMode != PaintMode.TILE_PLACING || selectedTileGO != null);
		if (brush.activeSelf)
		{
			Vector3 vector = CameraGO.instance.cam.ScreenToWorldPoint(Input.mousePosition);
			brush.transform.position = new Vector3(vector.x, vector.y, -10f);
		}
		if (selectedPlacedTile != null)
		{
			selectedTileIcon.SetActive(value: true);
			selectedTileCoordinates_text.gameObject.SetActive(value: true);
			originalOwnerCountry_inputfield.gameObject.SetActive(value: true);
			cityName_InputField.gameObject.SetActive(value: true);
			customIncome_Inputfield.gameObject.SetActive(value: true);
			customManpower_Inputfield.gameObject.SetActive(value: true);
			selectedTileIcon.transform.position = new Vector3(selectedPlacedTile.transform.position.x, selectedPlacedTile.transform.position.y, -0.4f);
		}
		else
		{
			selectedTileIcon.SetActive(value: false);
			originalOwnerCountry_inputfield.gameObject.SetActive(value: false);
			cityName_InputField.gameObject.SetActive(value: false);
			customIncome_Inputfield.gameObject.SetActive(value: false);
			customManpower_Inputfield.gameObject.SetActive(value: false);
			selectedTileCoordinates_text.gameObject.SetActive(value: false);
		}
		customNameForUnit_Inputfield.gameObject.SetActive(selectedUnitForRename != null);
		customHealthForUnit_Inputfield.gameObject.SetActive(selectedUnitForRename != null);
		entrenchmentOnStart_Inputfield.gameObject.SetActive(selectedUnitForRename != null);
		unitLevelOnStart_Inputfield.gameObject.SetActive(selectedUnitForRename != null);
		unitID_Inputfield.gameObject.SetActive(selectedUnitForRename != null);
		if (Input.GetMouseButtonDown(1))
		{
			selectedUnitForRename = null;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0f && Input.GetKey(KeyCode.LeftControl))
		{
			ChangeBrushSize();
		}
	}

	private void CreateTheMap()
	{
		if (MapEditorMenu.isLoadedMapEditor)
		{
			GameData.Instance.Load(MainMenu.selectedMap.FullName);
			if (GameData.Instance.gameSettings.saveLocked || GameData.Instance.isPBEM || GameData.Instance.offlinePBEM)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-locked file"));
				Invoke("QuitToMainMenuConfirmation", 3f);
				return;
			}
		}
		else
		{
			for (short num = 0; num < GameData.Instance.map.SizeX; num++)
			{
				for (short num2 = 0; num2 < GameData.Instance.map.SizeY; num2++)
				{
					GameData.Instance.map.TilesTable[num, num2] = new Tile();
					GameData.Instance.map.TilesTable[num, num2].Name = "Plain";
					GameData.Instance.map.TilesTable[num, num2].Type = "Plain";
					GameData.Instance.map.TilesTable[num, num2].MPCost = 2;
					GameData.Instance.map.TilesTable[num, num2].Season = "Summer";
					GameData.Instance.map.TilesTable[num, num2].PosX = num;
					GameData.Instance.map.TilesTable[num, num2].PosY = num2;
				}
			}
			GameData.Instance.map.Date = new DateTime(1939, 9, 1);
			GameData.Instance.map.AllowSnowInWinter = true;
			GameData.Instance.map.XPPerScenario = 5;
			GameData.Instance.map.XPperSecondaryVP = 3;
		}
		if (string.IsNullOrEmpty(GameData.Instance.map.ScenarioName))
		{
			GameData.Instance.map.ScenarioName = "Untitled scenario";
		}
		if (string.IsNullOrEmpty(GameData.Instance.map.ScenarioDescription))
		{
			GameData.Instance.map.ScenarioDescription = "Empty description.";
		}
		TableOfAllTiles = new TileEditorGO[GameData.Instance.map.SizeX, GameData.Instance.map.SizeY];
		List<Unit> allUnits = Utils.GetAllUnits();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.PurchasedUnits == null)
			{
				listOfPlayer.PurchasedUnits = new Dictionary<string, int>();
			}
			if (listOfPlayer.ListActivePolicies == null)
			{
				listOfPlayer.ListActivePolicies = new List<Policy>();
			}
			listOfPlayer.SetupIdeology();
			listOfPlayer.UnitCategoriesQuantity = null;
			listOfPlayer.CheckForMissingUnitsCategories();
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				listOfUnit.CategoryID = 0;
				listOfUnit.CustomName = null;
				listOfUnit.OwnerPlayer = listOfPlayer;
				listOfPlayer.AssignUnitCategoryID(listOfUnit);
				Utils.UpdateUnitStatsOnTurnOne(listOfUnit, allUnits);
				listOfUnit.SetUnitCustomName();
			}
		}
		if (GameData.Instance.map.listOfVPThatNeedsToBeHeld == null)
		{
			GameData.Instance.map.listOfVPThatNeedsToBeHeld = new Dictionary<Tile, string>();
		}
		if (GameData.Instance.map.listOfUnitsThatNeedToBeKilled == null)
		{
			GameData.Instance.map.listOfUnitsThatNeedToBeKilled = new Dictionary<Unit, bool>();
		}
		if (GameData.Instance.map.Events == null)
		{
			GameData.Instance.map.Events = new List<Event>();
		}
		else
		{
			foreach (Event @event in GameData.Instance.map.Events)
			{
				if (string.IsNullOrEmpty(@event.Name))
				{
					@event.SetDefaultName();
				}
			}
		}
		Physics2D.simulationMode = SimulationMode2D.Script;
		Utils.GetAllTilesNeighbours();
		base.gameObject.SetActive(value: false);
		Utils.UpdateTilesData(p_nonDestructive: false);
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (short num3 = 0; num3 < sizeX; num3++)
		{
			for (short num4 = 0; num4 < sizeY; num4++)
			{
				TableOfAllTiles[num3, num4] = UnityEngine.Object.Instantiate(tilePrefab, transform);
				TableOfAllTiles[num3, num4].tile = tilesTable[num3, num4];
				TableOfAllTiles[num3, num4].name = num3 + " " + num4;
				TableOfAllTiles[num3, num4].tile.PosX = num3;
				TableOfAllTiles[num3, num4].tile.PosY = num4;
				if (num3 % 2 != 0)
				{
					TableOfAllTiles[num3, num4].transform.position = new Vector2((float)num3 * 4.4f, (float)num4 * 5.1f);
				}
				else
				{
					TableOfAllTiles[num3, num4].transform.position = new Vector2((float)num3 * 4.4f, (float)num4 * 5.1f + 2.55f);
				}
				if (MapEditorMenu.isLoadedMapEditor)
				{
					if (TableOfAllTiles[num3, num4].tile.Name == "Coast31")
					{
						TableOfAllTiles[num3, num4].tile.Name = "Lake";
					}
					Utils.SetAutoCoasts(TableOfAllTiles[num3, num4].tile);
					if (GameData.Instance.map.listOfVPThatNeedsToBeHeld.Count > 0 && GameData.Instance.map.listOfVPThatNeedsToBeHeld.ContainsKey(TableOfAllTiles[num3, num4].tile))
					{
						foreach (KeyValuePair<Tile, string> item in GameData.Instance.map.listOfVPThatNeedsToBeHeld)
						{
							if (item.Key == TableOfAllTiles[num3, num4].tile)
							{
								if (item.Value == "Axis")
								{
									TableOfAllTiles[num3, num4].spriteRenderer.color = Color.red;
								}
								else if (item.Value == "Allies")
								{
									TableOfAllTiles[num3, num4].spriteRenderer.color = Color.blue;
								}
								else if (item.Value == "Communists")
								{
									TableOfAllTiles[num3, num4].spriteRenderer.color = Color.blue;
								}
								else
								{
									TableOfAllTiles[num3, num4].spriteRenderer.color = Color.magenta;
								}
							}
						}
					}
					if (TableOfAllTiles[num3, num4].tile.IsVictoryPoint)
					{
						selectedPlacedTile = TableOfAllTiles[num3, num4].gameObject;
						foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
						{
							if (TableOfAllTiles[num3, num4].tile.OwnerName == listOfPlayer2.Name)
							{
								TableOfAllTiles[num3, num4].tile.ownerPlayer = listOfPlayer2;
								SetflagForVP(TableOfAllTiles[num3, num4].tile.ownerPlayer);
							}
						}
					}
					if (TableOfAllTiles[num3, num4].tile.CityName != null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(cityName_Holder, TableOfAllTiles[num3, num4].transform);
						TableOfAllTiles[num3, num4].cityName = gameObject.GetComponent<TextMeshPro>();
						gameObject.GetComponentInChildren<TextMeshPro>().text = TableOfAllTiles[num3, num4].tile.CityName;
						gameObject.name = TableOfAllTiles[num3, num4].tile.CityName;
						gameObject.transform.position = new Vector3(TableOfAllTiles[num3, num4].transform.position.x, TableOfAllTiles[num3, num4].transform.position.y - 3f, TableOfAllTiles[num3, num4].transform.position.z - 10f);
					}
					if (TableOfAllTiles[num3, num4].tile.AirUnit != null)
					{
						SpawnTheUnit(TableOfAllTiles[num3, num4].tile.AirUnit, TableOfAllTiles[num3, num4].gameObject);
					}
					if (TableOfAllTiles[num3, num4].tile.GroundUnit != null)
					{
						SpawnTheUnit(TableOfAllTiles[num3, num4].tile.GroundUnit, TableOfAllTiles[num3, num4].gameObject);
					}
					if (TableOfAllTiles[num3, num4].tile.Season == "Winter" && TableOfAllTiles[num3, num4].tile.CanFreezeInWinter)
					{
						TableOfAllTiles[num3, num4].tile.isFrozen = true;
					}
					if (TableOfAllTiles[num3, num4].tile.Season == "Desert")
					{
						if (TableOfAllTiles[num3, num4].tile.Name == "Desert1")
						{
							TableOfAllTiles[num3, num4].tile.Name = "Plain";
						}
						else if (TableOfAllTiles[num3, num4].tile.Name == "DesertHill")
						{
							TableOfAllTiles[num3, num4].tile.Name = "Hill";
						}
						else if (TableOfAllTiles[num3, num4].tile.Name == "DesertVillage")
						{
							TableOfAllTiles[num3, num4].tile.Name = "Village";
						}
						else if (TableOfAllTiles[num3, num4].tile.Name == "Desert forest")
						{
							TableOfAllTiles[num3, num4].tile.Name = "ThinForest";
						}
						else if (TableOfAllTiles[num3, num4].tile.Name == "DesertAirport")
						{
							TableOfAllTiles[num3, num4].tile.Name = "Airport";
						}
					}
				}
				TableOfAllTiles[num3, num4].spriteRenderer.sprite = Utils.GetTileSprite(TableOfAllTiles[num3, num4].tile);
			}
		}
		base.gameObject.SetActive(value: true);
		Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
	}

	private void UpdateMapCreationTime()
	{
		_mapCreationTimer += Time.deltaTime;
		if (_mapCreationTimer >= 1f)
		{
			_mapCreationTimer = 0f;
			if (Application.isFocused && Input.mousePosition != _lastMousePosition)
			{
				GameData.Instance.map.CreationTotalTime++;
				TimeSpan timeSpan = TimeSpan.FromSeconds(GameData.Instance.map.CreationTotalTime);
				int num = timeSpan.Days * 24 + timeSpan.Hours;
				string text;
				if (num > 0)
				{
					text = $"{num}h {timeSpan.Minutes:D2}min {timeSpan.Seconds:D2}s";
				}
				else
				{
					text = ((timeSpan.Minutes <= 0) ? $"{timeSpan.Seconds:D2}s" : $"{timeSpan.Minutes:D2}min {timeSpan.Seconds:D2}s");
				}
				mapCreationTime_text.text = LocalizationManager.Translate("txt-map creation time") + text;
			}
			_lastMousePosition = Input.mousePosition;
		}
	}

	public void ManageWeatherPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		_weatherPanel.SetActive(!_weatherPanel.activeSelf);
		if (_weatherPanel.activeSelf)
		{
			_weatherPrediction_maxTurnsForecast_inputfield.SetTextWithoutNotify(GameData.Instance.map.MaxTurnsPrediction.ToString());
			_weatherPrediction_baseAccuracyValue_inputfield.SetTextWithoutNotify(GameData.Instance.map.BaseWeatherPredictionAccuracy.ToString());
			_weatherPrediction_dailyAccuracyLoss_inputfield.SetTextWithoutNotify(GameData.Instance.map.DailyWeatherPredictionAccuracyLoss.ToString());
			for (int i = 0; i < _weatherMonths_inputfields.Count; i++)
			{
				_weatherMonths_inputfields[i].SetTextWithoutNotify(GameData.Instance.map.WeatherChanceTable[i].ToString());
			}
		}
	}

	public void SetBadWeatherMonthChance(TMP_InputField p_TMP_InputField)
	{
		SoundManager.instance.PlayUIClickSound();
		int num = _weatherMonths_inputfields.IndexOf(p_TMP_InputField);
		if (!byte.TryParse(p_TMP_InputField.text, out var result) && result <= 100)
		{
			if (string.IsNullOrEmpty(p_TMP_InputField.text))
			{
				p_TMP_InputField.SetTextWithoutNotify("0");
			}
			else
			{
				p_TMP_InputField.SetTextWithoutNotify("100");
			}
		}
		GameData.Instance.map.WeatherChanceTable[num] = (byte)Mathf.Clamp(byte.Parse(p_TMP_InputField.text), 0, 100);
	}

	public void SetWeatherMaxTurnsPrediction(TMP_InputField p_TMP_InputField)
	{
		SoundManager.instance.PlayUIClickSound();
		if (!byte.TryParse(p_TMP_InputField.text, out var result) && result <= 30)
		{
			if (string.IsNullOrEmpty(p_TMP_InputField.text))
			{
				p_TMP_InputField.SetTextWithoutNotify("1");
			}
			else
			{
				p_TMP_InputField.SetTextWithoutNotify("30");
			}
		}
		GameData.Instance.map.MaxTurnsPrediction = (byte)Mathf.Clamp(byte.Parse(p_TMP_InputField.text), 0, 30);
	}

	public void SetWeatherBasePredictionAccuracy(TMP_InputField p_TMP_InputField)
	{
		SoundManager.instance.PlayUIClickSound();
		if (!byte.TryParse(p_TMP_InputField.text, out var result) && result <= 100)
		{
			if (string.IsNullOrEmpty(p_TMP_InputField.text))
			{
				p_TMP_InputField.SetTextWithoutNotify("0");
			}
			else
			{
				p_TMP_InputField.SetTextWithoutNotify("100");
			}
		}
		GameData.Instance.map.BaseWeatherPredictionAccuracy = (byte)Mathf.Clamp(byte.Parse(p_TMP_InputField.text), 0, 100);
	}

	public void SetWeatherPredictionDailyAccuracyLoss(TMP_InputField p_TMP_InputField)
	{
		SoundManager.instance.PlayUIClickSound();
		if (!byte.TryParse(p_TMP_InputField.text, out var result) && result <= 200)
		{
			if (string.IsNullOrEmpty(p_TMP_InputField.text))
			{
				p_TMP_InputField.SetTextWithoutNotify("0");
			}
			else
			{
				p_TMP_InputField.SetTextWithoutNotify("200");
			}
		}
		GameData.Instance.map.DailyWeatherPredictionAccuracyLoss = (byte)Mathf.Clamp(byte.Parse(p_TMP_InputField.text), 0, 200);
	}

	private void SetPaintMode(PaintMode p_paintMode)
	{
		paintMode = p_paintMode;
		if (paintMode == PaintMode.TILE_PLACING)
		{
			TileEditorGO[,] tableOfAllTiles = TableOfAllTiles;
			foreach (TileEditorGO tileEditorGO in tableOfAllTiles)
			{
				UnityEngine.Object.Destroy(tileEditorGO.freezeIcon);
				UnityEngine.Object.Destroy(tileEditorGO.seasonChangeIcon);
			}
		}
		else if (paintMode == PaintMode.CAN_FREEZE)
		{
			TileEditorGO[,] tableOfAllTiles = TableOfAllTiles;
			foreach (TileEditorGO tileEditorGO2 in tableOfAllTiles)
			{
				UnityEngine.Object.Destroy(tileEditorGO2.freezeIcon);
				UnityEngine.Object.Destroy(tileEditorGO2.seasonChangeIcon);
				if (tileEditorGO2.tile.CanFreezeInWinter)
				{
					tileEditorGO2.freezeIcon = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Can Freeze"), tileEditorGO2.transform, worldPositionStays: false);
				}
			}
		}
		else
		{
			if (paintMode != PaintMode.CAN_SEASON_CHANGE)
			{
				return;
			}
			TileEditorGO[,] tableOfAllTiles = TableOfAllTiles;
			foreach (TileEditorGO tileEditorGO3 in tableOfAllTiles)
			{
				UnityEngine.Object.Destroy(tileEditorGO3.freezeIcon);
				UnityEngine.Object.Destroy(tileEditorGO3.seasonChangeIcon);
				if (tileEditorGO3.tile.SeasonNeverChanges)
				{
					tileEditorGO3.seasonChangeIcon = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/No Season Change"), tileEditorGO3.transform, worldPositionStays: false);
				}
			}
		}
	}

	private void SpawnTheUnit(Unit unitToSpawn, GameObject tile)
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name == unitToSpawn.OwnerName)
			{
				unitToSpawn.OwnerPlayer = listOfPlayer;
			}
		}
		tile.GetComponent<TileEditorGO>().SpawnUnit(unitToSpawn);
	}

	public void PlaytestMap()
	{
		SoundManager.instance.PlayUIClickSound();
		bool flag = false;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsPlayingFirst)
			{
				flag = true;
			}
			listOfPlayer.SetDefaultIncomeModifier();
		}
		if (!flag)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no playing first"));
			return;
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (!@event.IsEventReady(p_includingActions: true))
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-event not properly set up"));
				return;
			}
		}
		MainMenu.isEditor = false;
		MainMenu.isLoadedGame = false;
		GameData.Instance.GenerateGameSettings(p_lockSaveFile: false);
		GameData.Instance.SetIronManMode(GameData.Instance.IsIronManModeON(out var _));
		if (MainMenu.selectedMap == null)
		{
			MainMenu.selectedMap = new FileInfo(Application.persistentDataPath + "/Maps/" + MapEditorMenu.mapName + ".sav");
		}
		GameData.Instance.Save(MainMenu.selectedMap);
		GameData.Instance.Load(MainMenu.selectedMap.FullName);
		GameData.Instance.map.IsCampaign = false;
		GameData.Instance.isFromMapEditor = true;
		SceneManager.LoadScene("Game");
	}

	public void ResetSaveToPlayableMap()
	{
		string text = ((MainMenu.selectedMap != null) ? Path.GetFileNameWithoutExtension(MainMenu.selectedMap.Name) : MapEditorMenu.mapName);
		if (!text.EndsWith("_converted", StringComparison.OrdinalIgnoreCase))
		{
			text += "_converted";
		}
		MainMenu.selectedMap = new FileInfo(Path.Combine(Application.persistentDataPath, "Maps", text + ".sav"));
		MapEditorMenu.mapName = text;
		GameData.Instance.isPBEM = false;
		GameData.Instance.sessionID = Guid.Empty;
		GameData.Instance.offlinePBEM = false;
		GameData.Instance.map.XPAwarded = false;
		GameData.Instance.violatedIronmanRule = false;
		GameData.Instance.map.WeatherSeed = 0;
		GameData.Instance.map.turnCount = 0;
		GameData.Instance.map.timePlayed = 0f;
		GameData.Instance.map.BackUpPlayerPlaying = null;
		GameData.Instance.map.Notifications = new List<Notification>();
		GameData.Instance.GenerateGameSettings(p_lockSaveFile: false);
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.PlayersAtWar = null;
			listOfPlayer.AlliedPlayers = null;
			listOfPlayer.CurrentUnitsResearch = new Dictionary<string, float>();
			listOfPlayer.Stats = new Dictionary<ushort, Statistic>();
			listOfPlayer.PurchasedUnits = new Dictionary<string, int>();
			listOfPlayer.ListActivePolicies = new List<Policy>();
			listOfPlayer.TotalDamageDealt = 0f;
			listOfPlayer.TotalMoneyEarned = 0;
			listOfPlayer.TotalMoneySpent = 0;
			listOfPlayer.TotalTilesBombed = 0f;
			listOfPlayer.TotalUnitsBought = 0;
			listOfPlayer.TotalUnitsDestroyed = 0;
			listOfPlayer.TotalUnitsLost = 0;
			listOfPlayer.IsEndTurnPanel = false;
			listOfPlayer.IsPlaying = false;
			listOfPlayer.IsPlayingFirst = false;
			listOfPlayer.TurnMoneyEarned = 0;
			listOfPlayer.TurnMoneySpent = 0;
			listOfPlayer.TurnUnitsBought = 0;
			listOfPlayer.TurnUnitsDestroyed = 0;
			listOfPlayer.TurnUnitsLost = 0;
			listOfPlayer.SafariKillCount = 0;
			listOfPlayer.NumberOfVPEverConquered = 0;
			listOfPlayer.Rank = 1;
			listOfPlayer.XP = 0;
		}
		GameData.Instance.Save(MainMenu.selectedMap);
		UIManager.ShowMessage(LocalizationManager.Translate("info-successful conversion"));
	}

	public void SetSnowInWinter_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.AllowSnowInWinter = !GameData.Instance.map.AllowSnowInWinter;
	}

	public void ManageUI()
	{
		SoundManager.instance.PlayUIClickSound();
		UI.SetActive(!UI.activeSelf);
		rightClick_Panel.SetActive(value: false);
		tooltip.SetActive(value: false);
	}

	public void CountCitiesOnTheMap()
	{
		SoundManager.instance.PlayUIClickSound();
		_citiesCount = 0;
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (GameData.Instance.map.TilesTable[i, j].Type == "City" || GameData.Instance.map.TilesTable[i, j].Type == "Harbour")
				{
					_citiesCount++;
				}
			}
		}
		UIManager.ShowMessage(LocalizationManager.Translate("info-cities count") + _citiesCount, 1.5f);
	}

	public void SetOriginalOwnerVP()
	{
		if ((bool)selectedPlacedTile && selectedPlacedTile.TryGetComponent<TileEditorGO>(out var component) && component.tile != null && component.tile.IsVictoryPoint)
		{
			component.tile.OriginalCountryOwner = originalOwnerCountry_inputfield.text;
		}
	}

	public void SetEditorIncomeOverwrite()
	{
		if ((bool)selectedPlacedTile && selectedPlacedTile.TryGetComponent<TileEditorGO>(out var component) && component.tile != null && short.TryParse(customIncome_Inputfield.text, out var result))
		{
			component.tile.EditorOverwrite_income = result;
		}
	}

	public void SetEditorManpowerOverwrite()
	{
		if ((bool)selectedPlacedTile && selectedPlacedTile.TryGetComponent<TileEditorGO>(out var component) && component.tile != null && int.TryParse(customManpower_Inputfield.text, out var result))
		{
			component.tile.EditorOverwrite_manpower = result;
		}
	}

	public void SetMonthForMap()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!string.IsNullOrWhiteSpace(monthMap_InputField.text))
		{
			int num = int.Parse(monthMap_InputField.text);
			if (num > 12)
			{
				monthMap_InputField.text = "12";
				num = 12;
			}
			else if (num < 1)
			{
				monthMap_InputField.text = "1";
				num = 1;
			}
			DateTime date = new DateTime(GameData.Instance.map.Date.Year, num, GameData.Instance.map.Date.Day);
			GameData.Instance.map.Date = date;
		}
	}

	public void SetDayForMap()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!string.IsNullOrWhiteSpace(dayMap_InputField.text))
		{
			int num = int.Parse(dayMap_InputField.text);
			if (num > 31)
			{
				dayMap_InputField.text = "1";
				num = 1;
			}
			else if (num < 1)
			{
				dayMap_InputField.text = "1";
				num = 1;
			}
			DateTime date = new DateTime(GameData.Instance.map.Date.Year, GameData.Instance.map.Date.Month, num);
			GameData.Instance.map.Date = date;
		}
	}

	private void ChangeBrushSize()
	{
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			brushSize++;
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			brushSize--;
		}
		if (brushSize < 1)
		{
			brushSize = 1;
		}
		brushSize_Text.text = brushSize.ToString();
		if (brushSize == 1)
		{
			brush.transform.localScale = new Vector2(brushSize, brushSize);
		}
		else
		{
			brush.transform.localScale = new Vector2((float)brushSize * 1.7f, (float)brushSize * 1.7f);
		}
	}

	public void RemoveAllUnits_Button(bool p_fromVPsonly)
	{
		_confirmationParameter1 = p_fromVPsonly;
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("txt-confirmation remove all units"), RemoveAllUnitsConfirmation);
	}

	private void RemoveAllUnitsConfirmation()
	{
		RemoveAllUnits(_confirmationParameter1);
	}

	private void RemoveAllUnits(bool p_fromVPsonly)
	{
		SoundManager.instance.PlayUIClickSound();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		Player o_player;
		if (p_fromVPsonly)
		{
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					if (!tilesTable[i, j].IsVictoryPoint)
					{
						continue;
					}
					if (tilesTable[i, j].GroundUnit != null)
					{
						UnityEngine.Object.Destroy(TableOfAllTiles[i, j].groundUnitGO);
						TableOfAllTiles[i, j].groundUnitGO = null;
						GameData.Instance.TryFindPlayerByName(tilesTable[i, j].GroundUnit.OwnerName, out o_player);
						if (tilesTable[i, j].GroundUnit.MaxPurchaseTime > 0 && o_player.PurchasedUnits.ContainsKey(tilesTable[i, j].GroundUnit.Name))
						{
							o_player.PurchasedUnits[tilesTable[i, j].GroundUnit.Name]--;
						}
						o_player.RemoveUnit(tilesTable[i, j].GroundUnit, p_sendRPC: false);
						tilesTable[i, j].GroundUnit = null;
					}
					if (tilesTable[i, j].AirUnit != null)
					{
						UnityEngine.Object.Destroy(TableOfAllTiles[i, j].airUnitGO);
						TableOfAllTiles[i, j].airUnitGO = null;
						GameData.Instance.TryFindPlayerByName(tilesTable[i, j].AirUnit.OwnerName, out o_player);
						if (tilesTable[i, j].AirUnit.MaxPurchaseTime > 0 && o_player.PurchasedUnits.ContainsKey(tilesTable[i, j].AirUnit.Name))
						{
							o_player.PurchasedUnits[tilesTable[i, j].AirUnit.Name]--;
						}
					}
				}
			}
			return;
		}
		for (int k = 0; k < sizeX; k++)
		{
			for (int l = 0; l < sizeY; l++)
			{
				if (tilesTable[k, l].AirUnit != null && tilesTable[k, l].AirUnit.MaxPurchaseTime > 0)
				{
					GameData.Instance.TryFindPlayerByName(tilesTable[k, l].AirUnit.OwnerName, out o_player);
					if (o_player.PurchasedUnits.ContainsKey(tilesTable[k, l].AirUnit.Name))
					{
						o_player.PurchasedUnits[tilesTable[k, l].AirUnit.Name]--;
					}
				}
				tilesTable[k, l].AirUnit = null;
				if (tilesTable[k, l].GroundUnit != null && tilesTable[k, l].GroundUnit.MaxPurchaseTime > 0)
				{
					GameData.Instance.TryFindPlayerByName(tilesTable[k, l].GroundUnit.OwnerName, out o_player);
					if (o_player.PurchasedUnits.ContainsKey(tilesTable[k, l].GroundUnit.Name))
					{
						o_player.PurchasedUnits[tilesTable[k, l].GroundUnit.Name]--;
					}
				}
				tilesTable[k, l].GroundUnit = null;
			}
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.ListOfUnits = new List<Unit>();
		}
		Transform[] componentsInChildren = unitParent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != unitParent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	public void ManageStoreMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no player selected"));
			return;
		}
		selectedTileGO = null;
		selectedPlacedTile = null;
		if (store_Panel.activeSelf)
		{
			store_Panel.SetActive(value: false);
			return;
		}
		store_Panel.SetActive(value: true);
		UIManager.isUIOpen = true;
	}

	public void ChangeTileSeason()
	{
		SoundManager.instance.PlayUIClickSound();
		if (season == "Summer")
		{
			season = "Winter";
		}
		else if (season == "Winter")
		{
			season = "Desert";
		}
		else
		{
			season = "Summer";
		}
		SetMiniatureFilter();
		Populate();
	}

	private void SetMiniatureFilter()
	{
		if (season == "Summer")
		{
			roadGO.GetComponent<Image>().sprite = road_SummerSprite;
			cityGO.GetComponent<Image>().sprite = city_SummerSprite;
			riverGO.GetComponent<Image>().sprite = river_SummerSprite;
			junctionGO.GetComponent<Image>().sprite = junction_SummerSprite;
			railwaysGO.GetComponent<Image>().sprite = railways_SummerSprite;
			forestGO.GetComponent<Image>().sprite = forest_SummerSprite;
			defenseGO.GetComponent<Image>().sprite = defense_SummerSprite;
			harbourGO.GetComponent<Image>().sprite = harbour_SummerSprite;
			terrainGO.GetComponent<Image>().sprite = terrain_SummerSprite;
			roadBridgeGO.GetComponent<Image>().sprite = roadBridge_SummerSprite;
			RailroadBridgeGO.GetComponent<Image>().sprite = RailroadBridge_SummerSprite;
			coastGO.GetComponent<Image>().sprite = coast_SummerSprite;
		}
		else if (season == "Winter")
		{
			roadGO.GetComponent<Image>().sprite = road_WinterSprite;
			cityGO.GetComponent<Image>().sprite = city_WinterSprite;
			riverGO.GetComponent<Image>().sprite = river_WinterSprite;
			junctionGO.GetComponent<Image>().sprite = junction_WinterSprite;
			railwaysGO.GetComponent<Image>().sprite = railways_WinterSprite;
			forestGO.GetComponent<Image>().sprite = forest_WinterSprite;
			defenseGO.GetComponent<Image>().sprite = defense_WinterSprite;
			harbourGO.GetComponent<Image>().sprite = harbour_WinterSprite;
			terrainGO.GetComponent<Image>().sprite = terrain_WinterSprite;
			roadBridgeGO.GetComponent<Image>().sprite = roadBridge_WinterSprite;
			RailroadBridgeGO.GetComponent<Image>().sprite = RailroadBridge_WinterSprite;
			coastGO.GetComponent<Image>().sprite = coast_WinterSprite;
		}
		else if (season == "Desert")
		{
			roadGO.GetComponent<Image>().sprite = road_DesertSprite;
			cityGO.GetComponent<Image>().sprite = city_DesertSprite;
			riverGO.GetComponent<Image>().sprite = river_DesertSprite;
			terrainGO.GetComponent<Image>().sprite = terrain_DesertSprite;
			defenseGO.GetComponent<Image>().sprite = defense_DesertSprite;
			harbourGO.GetComponent<Image>().sprite = harbour_DesertSprite;
			roadBridgeGO.GetComponent<Image>().sprite = roadBridge_DesertSprite;
			RailroadBridgeGO.GetComponent<Image>().sprite = RailroadBridge_DesertSprite;
			coastGO.GetComponent<Image>().sprite = coast_DesertSprite;
			railwaysGO.GetComponent<Image>().sprite = railways_DesertSprite;
		}
	}

	public void SetNumberOfTurnsToWin()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(numberOfTurnsToWin_InputField.text))
		{
			GameData.Instance.map.NumberOfTurnsToWin = 0;
		}
		else
		{
			GameData.Instance.map.NumberOfTurnsToWin = int.Parse(numberOfTurnsToWin_InputField.text);
		}
	}

	public void SetDamageToWin()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(damageToWin_InputField.text))
		{
			GameData.Instance.map.NumberOfDamageToWin = 0;
		}
		else
		{
			GameData.Instance.map.NumberOfDamageToWin = int.Parse(damageToWin_InputField.text);
		}
	}

	public void SetTilesToBombToWin()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(tilesToBombToWin_InputField.text))
		{
			GameData.Instance.map.NumberOfTilesToBombToWin = 0;
		}
		else
		{
			GameData.Instance.map.NumberOfTilesToBombToWin = int.Parse(tilesToBombToWin_InputField.text);
		}
	}

	public void SetFactionDamageToWin(TMP_Dropdown p_dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DamageToWinFaction = _availableTeamsSecondary[p_dropdown.value];
	}

	public void SetFactionTilesToBombToWin(TMP_Dropdown p_dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.TilesToBombFaction = _availableTeamsSecondary[p_dropdown.value];
	}

	public void ChangePlayerFixedIncome(TMP_InputField inputField, Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(inputField.text))
		{
			p_player.FixedIncome = 0;
		}
		else
		{
			p_player.FixedIncome = int.Parse(inputField.text);
		}
	}

	public void ChangePlayerHQpoints(TMP_InputField inputField, Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(inputField.text))
		{
			p_player.HighCommandPoints = 0;
		}
		else
		{
			p_player.HighCommandPoints = short.Parse(inputField.text);
		}
	}

	public void SetXPendScenario()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.XPPerScenario = int.Parse(XPperScenario_InputField.text);
	}

	public void SetBaseCoreUnitValue()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.BaseCoreUnitValue = int.Parse(baseCoreUnitsValue_InputField.text);
	}

	public void SetPerRankCoreUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.NumberOfNewCoreUnitPerRank = int.Parse(coreUnitsPerRank_InputField.text);
	}

	public void SetXPSecondaryObjectives()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.XPperSecondaryVP = int.Parse(XPperSecondaryVP_InputField.text);
	}

	public void SetCityName()
	{
		SoundManager.instance.PlayUIClickSound();
		if (selectedPlacedTile.GetComponent<TileEditorGO>().cityName == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(cityName_Holder);
			selectedPlacedTile.GetComponent<TileEditorGO>().cityName = gameObject.GetComponent<TextMeshPro>();
			selectedPlacedTile.GetComponent<TileEditorGO>().tile.CityName = cityName_InputField.text;
			gameObject.GetComponentInChildren<TextMeshPro>().text = selectedPlacedTile.GetComponent<TileEditorGO>().tile.CityName;
			gameObject.name = selectedPlacedTile.GetComponent<TileEditorGO>().tile.CityName;
			gameObject.transform.SetParent(selectedPlacedTile.transform);
			gameObject.transform.position = new Vector3(selectedPlacedTile.transform.position.x, selectedPlacedTile.transform.position.y - 3f, selectedPlacedTile.transform.position.z - 1.1f);
		}
		else
		{
			selectedPlacedTile.GetComponent<TileEditorGO>().tile.CityName = cityName_InputField.text;
			selectedPlacedTile.GetComponentInChildren<TextMeshPro>().text = selectedPlacedTile.GetComponent<TileEditorGO>().tile.CityName;
		}
	}

	public void SelectPlayer(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		TurnManager.currPlayer = p_player;
		selectedTileGO = null;
		PlayerSlotMapEditor[] componentsInChildren = listOfPlayersContainer.GetComponentsInChildren<PlayerSlotMapEditor>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RefreshSelectedPlayer();
		}
	}

	public void ShowPlayerInfos(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		int num = 0;
		int num2 = 0;
		foreach (Unit listOfUnit in p_player.ListOfUnits)
		{
			listOfUnit.OwnerPlayer = p_player;
			Utils.UpdateUnitStatsOnTurnOne(listOfUnit, Utils.GetAllUnits());
			listOfUnit.CalculateAndSetManpowerCost();
			num += listOfUnit.manpowerCost;
			num2 -= listOfUnit.GetUpkeepCost(p_getReserveCost: false);
		}
		int num3 = 0;
		int num4 = p_player.FixedIncome;
		Utils.UpdateTilesData(p_nonDestructive: false);
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				if (tilesTable[i, j].IsVictoryPoint && tilesTable[i, j].OwnerName == p_player.Name)
				{
					if (!string.IsNullOrEmpty(tilesTable[i, j].OriginalCountryOwner))
					{
						num4 += tilesTable[i, j].OccupiedMoneyProductionPerTurn;
						num3 += tilesTable[i, j].OccupiedManpower;
					}
					else
					{
						num4 = ((tilesTable[i, j].EditorOverwrite_income == 0) ? (num4 + tilesTable[i, j].MoneyProductionPerTurn) : (num4 + tilesTable[i, j].EditorOverwrite_income));
						num3 = ((tilesTable[i, j].EditorOverwrite_manpower == 0) ? (num3 + tilesTable[i, j].Manpower) : (num3 + tilesTable[i, j].EditorOverwrite_manpower));
					}
				}
			}
		}
		int p_manpowerValue = num3 - num;
		GameObject gameObject = UIManager.ShowMessage(LocalizationManager.GetLocalizedCountry(p_player.Country).ToUpper() + "\n---------------------------- \n" + LocalizationManager.Translate("txt-income summary") + ": \n" + LocalizationManager.Translate("txt-base income") + ((num4 > 0) ? ("+" + num4) : ((object)num4))?.ToString() + "\n" + LocalizationManager.Translate("txt-units upkeep") + ": " + num2 + "\n" + LocalizationManager.Translate("txt-current income") + ((num4 + num2 > 0) ? ("+" + (num4 + num2)) : ((object)(num4 + num2)))?.ToString() + "\n---------------------------- \n" + LocalizationManager.Translate("tggl-manpower") + ": " + Utils.GetManpowerForUI(num) + " / " + Utils.GetManpowerForUI(num3) + " (" + Utils.GetManpowerForUI(p_manpowerValue) + ")", 5f);
		gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150f);
		GameObject gameObject2 = UIManager.ShowMessage(LocalizationManager.GetLocalizedCountry(p_player.Country).ToUpper() + "\n---------------------------- \n" + LocalizationManager.Translate("txt-income summary") + ": \n" + LocalizationManager.Translate("txt-base income") + ((num4 > 0) ? ("+" + num4) : ((object)num4))?.ToString() + "\n" + LocalizationManager.Translate("txt-units upkeep") + ": " + num2 + "\n" + LocalizationManager.Translate("txt-current income") + ((num4 + num2 > 0) ? ("+" + (num4 + num2)) : ((object)(num4 + num2)))?.ToString() + "\n---------------------------- \n" + LocalizationManager.Translate("tggl-manpower") + ": " + Utils.GetManpowerForUI(num) + " / " + Utils.GetManpowerForUI(num3) + " (" + Utils.GetManpowerForUI(p_manpowerValue) + ")", 5f);
		gameObject2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150f);
		gameObject2.GetComponentInChildren<Button>().gameObject.SetActive(value: false);
		gameObject2.GetComponentInChildren<Image>().raycastTarget = false;
		GameObject gameObject3 = UIManager.ShowMessage(LocalizationManager.GetLocalizedCountry(p_player.Country).ToUpper() + "\n---------------------------- \n" + LocalizationManager.Translate("txt-income summary") + ": \n" + LocalizationManager.Translate("txt-base income") + ((num4 > 0) ? ("+" + num4) : ((object)num4))?.ToString() + "\n" + LocalizationManager.Translate("txt-units upkeep") + ": " + num2 + "\n" + LocalizationManager.Translate("txt-current income") + ((num4 + num2 > 0) ? ("+" + (num4 + num2)) : ((object)(num4 + num2)))?.ToString() + "\n---------------------------- \n" + LocalizationManager.Translate("tggl-manpower") + ": " + Utils.GetManpowerForUI(num) + " / " + Utils.GetManpowerForUI(num3) + " (" + Utils.GetManpowerForUI(p_manpowerValue) + ")", 5f);
		gameObject3.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150f);
		gameObject3.GetComponentInChildren<Button>().gameObject.SetActive(value: false);
		gameObject3.GetComponentInChildren<Image>().raycastTarget = false;
		gameObject2.transform.SetParent(gameObject.transform);
		gameObject3.transform.SetParent(gameObject.transform);
		gameObject.GetComponentInChildren<Button>().transform.SetParent(gameObject3.transform);
	}

	private void RefreshListOfPlayerSlots()
	{
		PlayerSlotMapEditor[] componentsInChildren = listOfPlayersContainer.GetComponentsInChildren<PlayerSlotMapEditor>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		foreach (Player player in GameData.Instance.listOfPlayers)
		{
			PlayerSlotMapEditor playerSlotInstance = UnityEngine.Object.Instantiate(_playerSlot_prefab, listOfPlayersContainer);
			playerSlotInstance.Setup(player);
			playerSlotInstance.playOrder_inputfield.SetTextWithoutNotify(GameData.Instance.listOfPlayers.IndexOf(player).ToString());
			playerSlotInstance.playOrder_inputfield.onEndEdit.AddListener(delegate
			{
				ReOrderPlayers(player, playerSlotInstance.playOrder_inputfield);
			});
			playerSlotInstance.playerSelection_button.onClick.AddListener(() =>
			{
				SelectPlayer(player);
			});
			playerSlotInstance.info_button.onClick.AddListener(() =>
			{
				ShowPlayerInfos(player);
			});
			playerSlotInstance.removePlayer_button.onClick.AddListener(() =>
			{
				RemovePlayer(player);
			});
			playerSlotInstance.playingFirst_toggle.onValueChanged.AddListener(delegate
			{
				SetPlaysFirst(player);
			});
			playerSlotInstance.AI_toggle.onValueChanged.AddListener(delegate
			{
				SetPlayerAsComputer(player);
			});
			playerSlotInstance.startIncome_inputfield.SetTextWithoutNotify(player.MoneyAmount.ToString());
			playerSlotInstance.startIncome_inputfield.onValueChanged.AddListener(delegate
			{
				SetMoneyAmount(playerSlotInstance.startIncome_inputfield, player);
			});
			playerSlotInstance.fixedIncome_inputfield.SetTextWithoutNotify(player.FixedIncome.ToString());
			playerSlotInstance.fixedIncome_inputfield.onValueChanged.AddListener(delegate
			{
				ChangePlayerFixedIncome(playerSlotInstance.fixedIncome_inputfield, player);
			});
			playerSlotInstance.startHQpoints_inputfield.SetTextWithoutNotify(player.HighCommandPoints.ToString());
			playerSlotInstance.startHQpoints_inputfield.onValueChanged.AddListener(delegate
			{
				ChangePlayerHQpoints(playerSlotInstance.startHQpoints_inputfield, player);
			});
			foreach (string country in Database.Countries)
			{
				playerSlotInstance.country_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
			}
			playerSlotInstance.country_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(player.Country, Database.Countries));
			playerSlotInstance.country_dropdown.RefreshShownValue();
			playerSlotInstance.country_dropdown.onValueChanged.AddListener(delegate
			{
				SetCountryForPlayer(player, playerSlotInstance.country_dropdown);
			});
			foreach (string faction in Database.factions)
			{
				playerSlotInstance.faction_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(faction)));
			}
			if (!string.IsNullOrEmpty(player.Country))
			{
				playerSlotInstance.faction_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(player.Country)));
			}
			if (Database.factions.Contains(player.Faction))
			{
				playerSlotInstance.faction_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(player.Faction, Database.factions));
			}
			else
			{
				playerSlotInstance.faction_dropdown.SetValueWithoutNotify(Database.factions.Count + 1);
			}
			playerSlotInstance.faction_dropdown.onValueChanged.AddListener(delegate
			{
				SetFactionForPlayer(player, playerSlotInstance.faction_dropdown);
			});
			foreach (string ideology in Database.ideologies)
			{
				playerSlotInstance.ideology_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedIdeology(ideology)));
			}
			playerSlotInstance.ideology_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(player.Ideology, Database.ideologies));
			playerSlotInstance.ideology_dropdown.onValueChanged.AddListener(delegate
			{
				SetIdeologyForPlayer(player, playerSlotInstance.ideology_dropdown);
			});
		}
	}

	private void ReOrderPlayers(Player p_player, TMP_InputField p_TMP_InputField)
	{
		if (!int.TryParse(p_TMP_InputField.text, out var result))
		{
			p_TMP_InputField.SetTextWithoutNotify(GameData.Instance.listOfPlayers.IndexOf(p_player).ToString());
			return;
		}
		if (result >= GameData.Instance.listOfPlayers.Count || result < 0)
		{
			p_TMP_InputField.SetTextWithoutNotify(GameData.Instance.listOfPlayers.IndexOf(p_player).ToString());
			return;
		}
		GameData.Instance.listOfPlayers.Remove(p_player);
		GameData.Instance.listOfPlayers.Insert(result, p_player);
		RefreshListOfPlayerSlots();
	}

	public void SetPlaysFirst(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.IsPlayingFirst = false;
			if (p_player.Name == listOfPlayer.Name)
			{
				listOfPlayer.IsPlayingFirst = !listOfPlayer.IsPlayingFirst;
			}
		}
		PlayerSlotMapEditor[] componentsInChildren = listOfPlayersContainer.GetComponentsInChildren<PlayerSlotMapEditor>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RefreshPlayingFirst();
		}
	}

	public void SetPlayerAsComputer(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (p_player.Name == listOfPlayer.Name)
			{
				listOfPlayer.IsComputer = !listOfPlayer.IsComputer;
			}
		}
	}

	public void SetCountryForPlayer(Player p_player, TMP_Dropdown p_Dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		GameObject gameObject = selectedPlacedTile;
		p_player.Country = Database.Countries.ElementAt(p_Dropdown.value);
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OwnerName == p_player.Name)
				{
					selectedPlacedTile = TableOfAllTiles[i, j].gameObject;
					SetflagForVP(p_player);
				}
				if (GameData.Instance.map.TilesTable[i, j].AirUnit != null && GameData.Instance.map.TilesTable[i, j].AirUnit.OwnerName == p_player.Name)
				{
					TableOfAllTiles[i, j].airUnitGO.GetComponent<SpriteRenderer>().sprite = Utils.GetUnitBackground(GameData.Instance.map.TilesTable[i, j].AirUnit, null);
				}
				if (GameData.Instance.map.TilesTable[i, j].GroundUnit != null && GameData.Instance.map.TilesTable[i, j].GroundUnit.OwnerName == p_player.Name)
				{
					TableOfAllTiles[i, j].groundUnitGO.GetComponent<SpriteRenderer>().sprite = Utils.GetUnitBackground(GameData.Instance.map.TilesTable[i, j].GroundUnit, null);
				}
			}
		}
		selectedPlacedTile = gameObject;
		RefreshListOfPlayerSlots();
		GenerateBorders();
	}

	private void SetFactionForPlayer(Player p_player, TMP_Dropdown p_Dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		List<string> list = new List<string>(Database.factions);
		if (!string.IsNullOrEmpty(p_player.Country))
		{
			list.Add(p_player.Country);
		}
		p_player.Faction = list[p_Dropdown.value];
	}

	private void SetIdeologyForPlayer(Player p_player, TMP_Dropdown p_Dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		p_player.Ideology = Database.ideologies[p_Dropdown.value];
	}

	public void SetIsCampaign_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.IsCampaign = !GameData.Instance.map.IsCampaign;
		campaignBox.SetActive(GameData.Instance.map.IsCampaign);
	}

	public void SetDisableResearch_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DisableResearch = !GameData.Instance.map.DisableResearch;
	}

	public void SetDisableManpower_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DisableManpower = !GameData.Instance.map.DisableManpower;
	}

	public void SetDisableUpkeep_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DisableUpkeep = !GameData.Instance.map.DisableUpkeep;
	}

	public void SetDisablePurchases_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DisablePurchases = !GameData.Instance.map.DisablePurchases;
	}

	public void SetCircularMap_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.CircularMap = !GameData.Instance.map.CircularMap;
		Utils.GetAllTilesNeighbours();
	}

	public void SetIsOfficial_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.IsOfficial = !GameData.Instance.map.IsOfficial;
	}

	public void SetDisabledWeather_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.DisableWeather = !GameData.Instance.map.DisableWeather;
	}

	public void RemoveTilesRIGHT()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeRIGHT.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num >= GameData.Instance.map.SizeX)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX - num, GameData.Instance.map.SizeY];
		for (int i = 0; i < array.GetLength(0); i++)
		{
			for (int j = 0; j < array.GetLength(1); j++)
			{
				array[i, j] = GameData.Instance.map.TilesTable[i, j];
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void RemoveTilesLEFT()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeLEFT.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num >= GameData.Instance.map.SizeX)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num % 2 != 0)
		{
			num++;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX - num, GameData.Instance.map.SizeY];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				array[num2, num3] = GameData.Instance.map.TilesTable[num2 + num, num3];
				array[num2, num3].PosX = num2;
				array[num2, num3].PosY = num3;
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void RemoveTilesUP()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeUP.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num >= GameData.Instance.map.SizeY)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX, GameData.Instance.map.SizeY - num];
		for (int i = 0; i < array.GetLength(0); i++)
		{
			for (int j = 0; j < array.GetLength(1); j++)
			{
				array[i, j] = GameData.Instance.map.TilesTable[i, j];
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void RemoveTilesDOWN()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeDOWN.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num >= GameData.Instance.map.SizeY)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX, GameData.Instance.map.SizeY - num];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				array[num2, num3] = GameData.Instance.map.TilesTable[num2, num3 + num];
				array[num2, num3].PosX = num2;
				array[num2, num3].PosY = num3;
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void AddTilesDOWN()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeDOWN.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX, GameData.Instance.map.SizeY + num];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				if (num3 < num)
				{
					array[num2, num3] = new Tile
					{
						PosX = num2,
						PosY = num3,
						DamageModificator = 0,
						MPCost = 1,
						Season = "Summer",
						Name = "Sea",
						Type = "Sea"
					};
				}
				else
				{
					array[num2, num3] = GameData.Instance.map.TilesTable[num2, num3 - num];
					array[num2, num3].PosX = num2;
					array[num2, num3].PosY = num3;
				}
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void AddTilesUP()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeUP.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX, GameData.Instance.map.SizeY + num];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				if (num3 < GameData.Instance.map.SizeY)
				{
					array[num2, num3] = GameData.Instance.map.TilesTable[num2, num3];
				}
				else
				{
					array[num2, num3] = new Tile
					{
						PosX = num2,
						PosY = num3,
						DamageModificator = 0,
						MPCost = 1,
						Season = "Summer",
						Name = "Sea",
						Type = "Sea"
					};
				}
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void AddTilesRIGHT()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeRIGHT.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX + num, GameData.Instance.map.SizeY];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				if (num2 < GameData.Instance.map.SizeX)
				{
					array[num2, num3] = GameData.Instance.map.TilesTable[num2, num3];
				}
				else
				{
					array[num2, num3] = new Tile
					{
						PosX = num2,
						PosY = num3,
						DamageModificator = 0,
						MPCost = 1,
						Season = "Summer",
						Name = "Sea",
						Type = "Sea"
					};
				}
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	public void AddTilesLEFT()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(newSizeLEFT.text);
		if (num <= 0)
		{
			errorPanel.SetActive(value: true);
			error_Description.color = Color.red;
			error_Description.text = LocalizationManager.Translate("error-invalid input resize");
			return;
		}
		if (num % 2 != 0)
		{
			num++;
		}
		Tile[,] array = new Tile[GameData.Instance.map.SizeX + num, GameData.Instance.map.SizeY];
		for (short num2 = 0; num2 < array.GetLength(0); num2++)
		{
			for (short num3 = 0; num3 < array.GetLength(1); num3++)
			{
				if (num2 < num)
				{
					array[num2, num3] = new Tile
					{
						PosX = num2,
						PosY = num3,
						DamageModificator = 0,
						MPCost = 1,
						Season = "Summer",
						Name = "Sea",
						Type = "Sea"
					};
				}
				else
				{
					array[num2, num3] = GameData.Instance.map.TilesTable[num2 - num, num3];
					array[num2, num3].PosX = num2;
					array[num2, num3].PosY = num3;
				}
			}
		}
		GameData.Instance.map.TilesTable = array;
		GameData.Instance.map.SizeX = array.GetLength(0);
		GameData.Instance.map.SizeY = array.GetLength(1);
		SaveBackupMapResize();
	}

	private void SaveBackupMapResize()
	{
		Dictionary<Tile, string> dictionary = new Dictionary<Tile, string>(GameData.Instance.map.listOfVPThatNeedsToBeHeld);
		GameData.Instance.map.IsOfficial = false;
		GameData.Instance.map.listOfVPThatNeedsToBeHeld = new Dictionary<Tile, string>();
		GameData.Instance.map.listOfUnitsThatNeedToBeKilled = new Dictionary<Unit, bool>();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				foreach (KeyValuePair<Tile, string> item in dictionary)
				{
					if (item.Key == tilesTable[i, j])
					{
						GameData.Instance.map.listOfVPThatNeedsToBeHeld.Add(tilesTable[i, j], item.Value);
					}
				}
				if (tilesTable[i, j].AirUnit != null && tilesTable[i, j].AirUnit.IsKillGoal)
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(tilesTable[i, j].AirUnit, value: false);
				}
				if (tilesTable[i, j].GroundUnit != null && tilesTable[i, j].GroundUnit.IsKillGoal)
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(tilesTable[i, j].GroundUnit, value: false);
				}
			}
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			List<Unit> list = new List<Unit>();
			for (int k = 0; k < sizeX; k++)
			{
				for (int l = 0; l < sizeY; l++)
				{
					if (tilesTable[k, l].GroundUnit != null && tilesTable[k, l].GroundUnit.OwnerName == listOfPlayer.Name)
					{
						list.Add(tilesTable[k, l].GroundUnit);
					}
					if (tilesTable[k, l].AirUnit != null && tilesTable[k, l].AirUnit.OwnerName == listOfPlayer.Name)
					{
						list.Add(tilesTable[k, l].AirUnit);
					}
				}
			}
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				if (listOfUnit.IsReserve)
				{
					list.Add(listOfUnit);
				}
			}
			listOfPlayer.ListOfUnits = new List<Unit>();
			foreach (Unit item2 in list)
			{
				item2.OwnerName = listOfPlayer.Name;
				listOfPlayer.ListOfUnits.Add(item2);
			}
		}
		if (MainMenu.selectedMap == null)
		{
			MainMenu.selectedMap = new FileInfo(Application.persistentDataPath + "/Maps/" + MapEditorMenu.mapName + ".sav");
		}
		MainMenu.selectedMap = new FileInfo(Application.persistentDataPath + "/Maps/" + Path.GetFileNameWithoutExtension(MainMenu.selectedMap.FullName) + "_resized_" + GameData.Instance.map.SizeX + "x" + GameData.Instance.map.SizeY + ".sav");
		GameData.Instance.Save(MainMenu.selectedMap);
		MapEditorMenu.isLoadedMapEditor = true;
		UIManager.isUIOpen = false;
		SceneManager.LoadScene("MapEditor");
	}

	public void ManageScenarioDescription(string p_value)
	{
		if (string.IsNullOrEmpty(_selectedTextEdition) || p_value == _selectedTextEdition || (p_value != _selectedTextEdition && !scenarioDescription_panel.activeSelf))
		{
			scenarioDescription_panel.SetActive(!scenarioDescription_panel.activeSelf);
		}
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		_selectedTextEdition = p_value;
		playerListScenarioTexts_dropdown.options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry("None"))
		};
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			playerListScenarioTexts_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(listOfPlayer.Country)));
		}
		playerListScenarioTexts_dropdown.SetValueWithoutNotify(0);
		playerListScenarioTexts_dropdown.RefreshShownValue();
		RefreshScenarioTextValues();
	}

	public void SetScenarioNameAndDescription()
	{
		SoundManager.instance.PlayUIClickSound();
		switch (_selectedTextEdition)
		{
		case "introduction":
			GameData.Instance.map.ScenarioName = scenarioName_InputField.text;
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				GameData.Instance.map.ScenarioDescription = scenarioDescription_InputField.text;
			}
			else
			{
				GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioDescription = scenarioDescription_InputField.text;
			}
			break;
		case "victory":
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				GameData.Instance.map.ScenarioVictoryText = scenarioDescription_InputField.text;
			}
			else
			{
				GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioVictoryText = scenarioDescription_InputField.text;
			}
			break;
		case "defeat":
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				GameData.Instance.map.ScenarioDefeatText = scenarioDescription_InputField.text;
			}
			else
			{
				GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioDefeatText = scenarioDescription_InputField.text;
			}
			break;
		}
	}

	public void RefreshScenarioTextValues()
	{
		switch (_selectedTextEdition)
		{
		case "introduction":
			scenarioName_InputField.SetTextWithoutNotify(GameData.Instance.map.ScenarioName);
			scenarioName_InputField.gameObject.SetActive(value: true);
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.map.ScenarioDescription);
			}
			else
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioDescription);
			}
			break;
		case "victory":
			scenarioName_InputField.SetTextWithoutNotify("");
			scenarioName_InputField.gameObject.SetActive(value: false);
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.map.ScenarioVictoryText);
			}
			else
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioVictoryText);
			}
			break;
		case "defeat":
			scenarioName_InputField.SetTextWithoutNotify("");
			scenarioName_InputField.gameObject.SetActive(value: false);
			if (playerListScenarioTexts_dropdown.value == 0)
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.map.ScenarioDefeatText);
			}
			else
			{
				scenarioDescription_InputField.SetTextWithoutNotify(GameData.Instance.listOfPlayers[playerListScenarioTexts_dropdown.value - 1].ScenarioDefeatText);
			}
			break;
		}
	}

	public void ManageIsAlwaysWinter()
	{
		SoundManager.instance.PlayUIClickSound();
		isAlwaysWinter = !isAlwaysWinter;
	}

	public void CloseScenarioDescriptionMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		scenarioDescription_panel.SetActive(value: false);
	}

	public void ManageCampaignWindows()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		nextMapVictory1_inputfield.SetTextWithoutNotify(GameData.Instance.map.NextMap);
		nextMapVictory2_inputfield.SetTextWithoutNotify(GameData.Instance.map.NextMap2);
		nextMapDefeat1_inputfield.SetTextWithoutNotify(GameData.Instance.map.NextMapDefeat);
		nextMapDefeat2_inputfield.SetTextWithoutNotify(GameData.Instance.map.NextMap2Defeat);
		devMenuPanel.SetActive(!devMenuPanel.activeSelf);
	}

	public void SetNextMap1()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.NextMap = nextMapVictory1_inputfield.text;
	}

	public void SetNextMap2()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.NextMap2 = nextMapVictory2_inputfield.text;
	}

	public void SetNextMap1Defeat()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.NextMapDefeat = nextMapDefeat1_inputfield.text;
	}

	public void SetNextMap2Defeat()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.NextMap2Defeat = nextMapDefeat2_inputfield.text;
	}

	public void ManageMapSettingsMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		if (scenarioSettings_Panel.activeSelf)
		{
			scenarioSettings_Panel.SetActive(value: false);
			UIManager.isUIOpen = false;
		}
		else
		{
			if (scenarioSettings_Panel.activeSelf || scenarioDescription_panel.activeSelf || store_Panel.activeSelf || errorPanel.activeSelf || reserve_Panel.activeSelf)
			{
				return;
			}
			scenarioSettings_Panel.SetActive(value: true);
			campaignBox.SetActive(GameData.Instance.map.IsCampaign);
			RefreshListOfPlayerSlots();
			yearMap_InputField.SetTextWithoutNotify(GameData.Instance.map.Date.Year.ToString());
			monthMap_InputField.SetTextWithoutNotify(GameData.Instance.map.Date.Month.ToString());
			dayMap_InputField.SetTextWithoutNotify(GameData.Instance.map.Date.Day.ToString());
			XPperScenario_InputField.SetTextWithoutNotify(GameData.Instance.map.XPPerScenario.ToString());
			XPperSecondaryVP_InputField.SetTextWithoutNotify(GameData.Instance.map.XPperSecondaryVP.ToString());
			baseCoreUnitsValue_InputField.SetTextWithoutNotify(GameData.Instance.map.BaseCoreUnitValue.ToString());
			coreUnitsPerRank_InputField.SetTextWithoutNotify(GameData.Instance.map.NumberOfNewCoreUnitPerRank.ToString());
			isOfficial.SetIsOnWithoutNotify(GameData.Instance.map.IsOfficial);
			isCampaign.SetIsOnWithoutNotify(GameData.Instance.map.IsCampaign);
			disableManpower.SetIsOnWithoutNotify(GameData.Instance.map.DisableManpower);
			disableResearch.SetIsOnWithoutNotify(GameData.Instance.map.DisableResearch);
			disableUpkeep.SetIsOnWithoutNotify(GameData.Instance.map.DisableUpkeep);
			disablePurchases.SetIsOnWithoutNotify(GameData.Instance.map.DisablePurchases);
			circularMap_toggle.SetIsOnWithoutNotify(GameData.Instance.map.CircularMap);
			snowInWinter_toggle.SetIsOnWithoutNotify(GameData.Instance.map.AllowSnowInWinter);
			weatherDisabled_toggle.SetIsOnWithoutNotify(GameData.Instance.map.DisableWeather);
			numberOfTurnsToWin_InputField.SetTextWithoutNotify(GameData.Instance.map.NumberOfTurnsToWin.ToString());
			damageToWin_InputField.SetTextWithoutNotify(GameData.Instance.map.NumberOfDamageToWin.ToString());
			tilesToBombToWin_InputField.SetTextWithoutNotify(GameData.Instance.map.NumberOfTilesToBombToWin.ToString());
			foreach (TMP_Dropdown teamsDropdown in teamsDropdowns)
			{
				teamsDropdown.ClearOptions();
				foreach (string item in _availableTeamsSecondary)
				{
					teamsDropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(item)));
				}
				if (teamsDropdown == tilesToBombToWin_dropdown)
				{
					for (int i = 0; i < teamsDropdown.options.Count; i++)
					{
						if (_availableTeamsSecondary.ElementAt(i) == GameData.Instance.map.TilesToBombFaction)
						{
							teamsDropdown.SetValueWithoutNotify(i);
						}
					}
				}
				else if (teamsDropdown == damageToWin_dropdown)
				{
					for (int j = 0; j < teamsDropdown.options.Count; j++)
					{
						if (_availableTeamsSecondary.ElementAt(j) == GameData.Instance.map.DamageToWinFaction)
						{
							teamsDropdown.SetValueWithoutNotify(j);
						}
					}
				}
				teamsDropdown.RefreshShownValue();
			}
			UIManager.isUIOpen = true;
		}
	}

	public void ChangeSeasonOfAllTilesOnTheMap()
	{
		SoundManager.instance.PlayUIClickSound();
		bool flag = false;
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (!TableOfAllTiles[i, j].tile.SeasonNeverChanges && !(TableOfAllTiles[i, j].tile.Season == "Desert") && !TableOfAllTiles[i, j].tile.Name.ToLower().Contains("jungle"))
				{
					flag = TableOfAllTiles[i, j].tile.Season == "Summer";
					break;
				}
			}
		}
		for (int k = 0; k < GameData.Instance.map.SizeX; k++)
		{
			for (int l = 0; l < GameData.Instance.map.SizeY; l++)
			{
				if (TableOfAllTiles[k, l].tile.SeasonNeverChanges || TableOfAllTiles[k, l].tile.Season == "Desert" || TableOfAllTiles[k, l].tile.Name.ToLower().Contains("jungle"))
				{
					continue;
				}
				if (flag)
				{
					TableOfAllTiles[k, l].tile.Season = "Winter";
					if (TableOfAllTiles[k, l].tile.CanFreezeInWinter)
					{
						TableOfAllTiles[k, l].tile.isFrozen = true;
					}
				}
				else
				{
					TableOfAllTiles[k, l].tile.Season = "Summer";
					TableOfAllTiles[k, l].tile.isFrozen = false;
				}
				TableOfAllTiles[k, l].spriteRenderer.sprite = Utils.GetTileSprite(TableOfAllTiles[k, l].tile);
			}
		}
	}

	public void SetMoneyAmount(TMP_InputField inputField, Player player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrEmpty(inputField.text))
		{
			player.MoneyAmount = 0;
		}
		else
		{
			player.MoneyAmount = int.Parse(inputField.text);
		}
	}

	public void SetNumberOfDaysThatGoByPerTurn()
	{
		SoundManager.instance.PlayUIClickSound();
		if (inputDaysThatGoByPerTurn_InputField.text == "")
		{
			GameData.Instance.map.NumberOfDaysThatGoesByPerTurn = 0;
		}
		else if (int.Parse(inputDaysThatGoByPerTurn_InputField.text) < 0)
		{
			inputDaysThatGoByPerTurn_InputField.SetTextWithoutNotify("0");
			GameData.Instance.map.NumberOfDaysThatGoesByPerTurn = 0;
		}
		else
		{
			GameData.Instance.map.NumberOfDaysThatGoesByPerTurn = int.Parse(inputDaysThatGoByPerTurn_InputField.GetComponent<TMP_InputField>().text);
		}
	}

	public void SetMapYear()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!(yearMap_InputField.text == ""))
		{
			int num = int.Parse(yearMap_InputField.text);
			if (num >= 0)
			{
				DateTime date = new DateTime(num, GameData.Instance.map.Date.Month, GameData.Instance.map.Date.Day);
				GameData.Instance.map.Date = date;
			}
		}
	}

	public void ResetTerrain()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("txt-confirmation remove all units"), ResetTerrainConfirmation);
	}

	private void ResetTerrainConfirmation()
	{
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (GameData.Instance.map.TilesTable[i, j].Name.Contains("Hill") || GameData.Instance.map.TilesTable[i, j].Name.Contains("HillForest") || GameData.Instance.map.TilesTable[i, j].Name.Contains("ThinForest") || GameData.Instance.map.TilesTable[i, j].Name.Contains("ThickForest") || GameData.Instance.map.TilesTable[i, j].Name.Contains("Bocage") || (GameData.Instance.map.TilesTable[i, j].Season == "Desert" && GameData.Instance.map.TilesTable[i, j].Type == "Plain"))
				{
					GameData.Instance.map.TilesTable[i, j].Name = "Plain";
					GameData.Instance.map.TilesTable[i, j].Type = "Plain";
					GameData.Instance.map.TilesTable[i, j].MPCost = 2;
					GameData.Instance.map.TilesTable[i, j].DamageModificator = 0;
					TableOfAllTiles[i, j].spriteRenderer.sprite = Utils.GetTileSprite(GameData.Instance.map.TilesTable[i, j]);
				}
			}
		}
	}

	public void RandomizeTerrain()
	{
		SoundManager.instance.PlayUIClickSound();
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (!(GameData.Instance.map.TilesTable[i, j].Name != "Plain"))
				{
					int num = UnityEngine.Random.Range(0, 101);
					if (num <= 1)
					{
						GameData.Instance.map.TilesTable[i, j].Name = "Hill";
						GameData.Instance.map.TilesTable[i, j].Type = "Hill";
					}
					else if (num > 1 && num <= 2 && GameData.Instance.map.TilesTable[i, j].Season != "Desert")
					{
						GameData.Instance.map.TilesTable[i, j].Name = "HillForest";
						GameData.Instance.map.TilesTable[i, j].Type = "Hill";
					}
					else if (num > 2 && num <= 20)
					{
						GameData.Instance.map.TilesTable[i, j].Name = "ThinForest";
						GameData.Instance.map.TilesTable[i, j].Type = "ThinForest";
					}
					else if (num > 20 && num <= 40 && GameData.Instance.map.TilesTable[i, j].Season != "Desert")
					{
						GameData.Instance.map.TilesTable[i, j].Name = "ThickForest";
						GameData.Instance.map.TilesTable[i, j].Type = "ThickForest";
					}
					else if (num > 40 && num <= 100)
					{
						GameData.Instance.map.TilesTable[i, j].Name = "Plain";
						GameData.Instance.map.TilesTable[i, j].Type = "Plain";
					}
					TableOfAllTiles[i, j].GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(GameData.Instance.map.TilesTable[i, j]);
				}
			}
		}
	}

	public void SetOwner()
	{
		SoundManager.instance.PlayUIClickSound();
		_buttonChoice = EventSystem.current.currentSelectedGameObject.name;
		TileEditorGO component = selectedPlacedTile.GetComponent<TileEditorGO>();
		if ((_buttonChoice == "Set Major VP" || _buttonChoice == "Set Normal VP" || _buttonChoice == "Set Secondary VP") && TurnManager.currPlayer != null)
		{
			if (_buttonChoice == "Set Normal VP")
			{
				if ((!component.tile.IsVictoryPoint || (!component.tile.SecondaryObjective && !component.tile.PrimaryObjective)) && (component.tile.ownerPlayer == null || component.tile.OwnerName == TurnManager.currPlayer.Name))
				{
					component.tile.IsVictoryPoint = !component.tile.IsVictoryPoint;
				}
				component.tile.PrimaryObjective = false;
				component.tile.SecondaryObjective = false;
			}
			else if (_buttonChoice == "Set Major VP")
			{
				if ((!component.tile.IsVictoryPoint || component.tile.PrimaryObjective) && (component.tile.ownerPlayer == null || component.tile.OwnerName == TurnManager.currPlayer.Name))
				{
					component.tile.IsVictoryPoint = !component.tile.IsVictoryPoint;
				}
				component.tile.PrimaryObjective = component.tile.IsVictoryPoint;
				component.tile.SecondaryObjective = false;
			}
			else if (_buttonChoice == "Set Secondary VP")
			{
				if ((!component.tile.IsVictoryPoint || component.tile.SecondaryObjective) && (component.tile.ownerPlayer == null || component.tile.OwnerName == TurnManager.currPlayer.Name))
				{
					component.tile.IsVictoryPoint = !component.tile.IsVictoryPoint;
				}
				component.tile.PrimaryObjective = false;
				component.tile.SecondaryObjective = component.tile.IsVictoryPoint;
			}
			if (component.tile.IsVictoryPoint)
			{
				component.tile.OwnerName = TurnManager.currPlayer.Name;
				component.tile.terrainOwner = TurnManager.currPlayer;
				GameData.Instance.TryFindPlayerByName(component.tile.OwnerName, out var o_player);
				component.tile.ownerPlayer = o_player;
				SetflagForVP(TurnManager.currPlayer);
			}
			else
			{
				selectedPlacedTile.GetComponent<TileEditorGO>().DestroyFlag();
				component.tile.OwnerName = null;
				component.tile.ownerPlayer = null;
				component.tile.terrainOwner = null;
			}
			GenerateBorders();
		}
		else if (_buttonChoice == "Reset")
		{
			component.RemoveUnits();
			if (component.tile.CityName != null)
			{
				UnityEngine.Object.Destroy(selectedPlacedTile.transform.GetComponentInChildren<TextMeshPro>().gameObject);
			}
			component.tile.CityName = null;
			if (component.tile.IsVictoryPoint)
			{
				component.DestroyFlag();
			}
			component.tile.IsVictoryPoint = false;
			component.tile.PrimaryObjective = false;
			component.tile.SecondaryObjective = false;
			component.tile.OwnerName = null;
			component.tile.ownerPlayer = null;
			component.tile.terrainOwner = null;
			component.tile.OriginalCountryOwner = null;
			component.tile.EditorOverwrite_income = 0;
			component.tile.EditorOverwrite_manpower = 0;
			GenerateBorders();
		}
		else if (_buttonChoice == "Remove units")
		{
			component.RemoveUnits();
		}
		else if (_buttonChoice == "VP to be held")
		{
			component.ManageVPToBeHeld();
		}
		else if (_buttonChoice == "Add level")
		{
			if (component.tile.AirUnit != null && component.tile.AirUnit.Lvl < 5)
			{
				int lvl = component.tile.AirUnit.Lvl;
				while (component.tile.AirUnit.Lvl == lvl)
				{
					component.tile.AirUnit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
				component.tile.AirUnit.CurrHP = component.tile.AirUnit.MaxHP;
				if (component.tile.AirUnit.Lvl >= 5)
				{
					component.tile.AirUnit.Lvl = 5;
				}
				UIManager.ShowMessage(LocalizationManager.Translate("info-unit's level") + component.tile.AirUnit.Lvl, 1f);
			}
			if (component.tile.GroundUnit != null && component.tile.GroundUnit.Lvl < 5)
			{
				int lvl2 = component.tile.GroundUnit.Lvl;
				while (component.tile.GroundUnit.Lvl == lvl2)
				{
					component.tile.GroundUnit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
				component.tile.GroundUnit.CurrHP = component.tile.GroundUnit.MaxHP;
				if (component.tile.GroundUnit.Lvl >= 5)
				{
					component.tile.GroundUnit.Lvl = 5;
				}
				UIManager.ShowMessage(LocalizationManager.Translate("info-unit's level") + component.tile.GroundUnit.Lvl, 1f);
			}
		}
		else if (_buttonChoice == "Remove level")
		{
			if (component.tile.AirUnit != null && component.tile.AirUnit.Lvl > 1)
			{
				int lvl3 = component.tile.AirUnit.Lvl;
				while (component.tile.AirUnit.Lvl == lvl3)
				{
					component.tile.AirUnit.AddXP(-1, p_canAddHero: false, p_showXPUI: false);
				}
				component.tile.AirUnit.CurrHP = component.tile.AirUnit.MaxHP;
				if (component.tile.AirUnit.Lvl <= 0)
				{
					component.tile.AirUnit.Lvl = 1;
				}
				UIManager.ShowMessage(LocalizationManager.Translate("info-unit's level") + component.tile.AirUnit.Lvl, 1f);
			}
			if (component.tile.GroundUnit != null && component.tile.GroundUnit.Lvl > 1)
			{
				int lvl4 = component.tile.GroundUnit.Lvl;
				while (component.tile.GroundUnit.Lvl == lvl4)
				{
					component.tile.GroundUnit.AddXP(-1, p_canAddHero: false, p_showXPUI: false);
				}
				component.tile.GroundUnit.CurrHP = component.tile.GroundUnit.MaxHP;
				if (component.tile.GroundUnit.Lvl <= 0)
				{
					component.tile.GroundUnit.Lvl = 1;
				}
				UIManager.ShowMessage(LocalizationManager.Translate("info-unit's level") + component.tile.GroundUnit.Lvl, 1f);
			}
		}
		else if (_buttonChoice == "No ammo")
		{
			if (component.tile.AirUnit != null)
			{
				component.tile.AirUnit.NoAmmoTurn1 = !component.tile.AirUnit.NoAmmoTurn1;
				UIManager.ShowMessage(LocalizationManager.Translate("info-no supplies start") + component.tile.AirUnit.NoAmmoTurn1, 1f);
			}
			if (component.tile.GroundUnit != null)
			{
				component.tile.GroundUnit.NoAmmoTurn1 = !component.tile.GroundUnit.NoAmmoTurn1;
				UIManager.ShowMessage(LocalizationManager.Translate("info-no supplies start") + component.tile.GroundUnit.NoAmmoTurn1, 1f);
			}
		}
		else if (_buttonChoice == "No entrenchment")
		{
			if (component.tile.AirUnit != null)
			{
				component.tile.AirUnit.NoEntrenchmentTurn1 = !component.tile.AirUnit.NoEntrenchmentTurn1;
				UIManager.ShowMessage(LocalizationManager.Translate("info-no entrenchment start") + component.tile.AirUnit.NoEntrenchmentTurn1, 1f);
			}
			if (component.tile.GroundUnit != null)
			{
				component.tile.GroundUnit.NoEntrenchmentTurn1 = !component.tile.GroundUnit.NoEntrenchmentTurn1;
				UIManager.ShowMessage(LocalizationManager.Translate("info-no entrenchment start") + component.tile.GroundUnit.NoEntrenchmentTurn1, 1f);
			}
		}
		if (!component.tile.IsVictoryPoint)
		{
			foreach (Event @event in GameData.Instance.map.Events)
			{
				List<Coordinates> list = new List<Coordinates>();
				foreach (Coordinates item in @event.VPsToBeLost)
				{
					if (item.posX == component.tile.PosX && item.posY == component.tile.PosY)
					{
						list.Add(item);
					}
				}
				foreach (Coordinates item2 in list)
				{
					@event.VPsToBeLost.Remove(item2);
				}
			}
		}
		rightClick_Panel.SetActive(value: false);
		tooltip.SetActive(value: false);
	}

	public void RenameSelectedUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedUnitForRename.ForcedCustomName = customNameForUnit_Inputfield.text;
		if (selectedUnitForRename.ForcedCustomName == selectedUnitForRename.Name)
		{
			selectedUnitForRename.ForcedCustomName = null;
		}
	}

	public void ChangeHPmodifierOnScenarioStart()
	{
		SoundManager.instance.PlayUIClickSound();
		if (customHealthForUnit_Inputfield.text.Count() == 0)
		{
			selectedUnitForRename.HPmodifierOnScenarioStart = null;
			customHealthForUnit_Inputfield.SetTextWithoutNotify("100%");
			return;
		}
		if (byte.TryParse(customHealthForUnit_Inputfield.text.Replace("%", ""), out var result))
		{
			selectedUnitForRename.HPmodifierOnScenarioStart = (byte)Mathf.Clamp(result, 1, 100);
		}
		customHealthForUnit_Inputfield.SetTextWithoutNotify(selectedUnitForRename.HPmodifierOnScenarioStart + "%");
	}

	public void ChangeUnitLevel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (unitLevelOnStart_Inputfield.text.Count() == 0)
		{
			selectedUnitForRename.Lvl = 1;
			selectedUnitForRename.XP = 0;
		}
		else
		{
			if (!byte.TryParse(unitLevelOnStart_Inputfield.text, out var result))
			{
				return;
			}
			result = (byte)Mathf.Clamp(result, 1, Unit.MAX_LEVEL);
			if (result > selectedUnitForRename.Lvl)
			{
				while (selectedUnitForRename.Lvl < result)
				{
					selectedUnitForRename.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
			}
			else
			{
				while (selectedUnitForRename.Lvl > result)
				{
					selectedUnitForRename.AddXP(-1, p_canAddHero: false, p_showXPUI: false);
				}
			}
		}
	}

	public void ChangeEntrenchmentLevelOnScenarioStart()
	{
		SoundManager.instance.PlayUIClickSound();
		byte result;
		if (entrenchmentOnStart_Inputfield.text.Count() == 0)
		{
			selectedUnitForRename.EntrenchmentLevelOnStart = null;
		}
		else if (byte.TryParse(entrenchmentOnStart_Inputfield.text, out result))
		{
			selectedUnitForRename.EntrenchmentLevelOnStart = (byte)Mathf.Clamp(result, 0, 10);
		}
	}

	public void SendUnitBackIntoReserve()
	{
		TileEditorGO component = selectedPlacedTile.GetComponent<TileEditorGO>();
		if (component.tile.GroundUnit != null || component.tile.AirUnit != null)
		{
			if (component.tile.AirUnit != null)
			{
				component.tile.AirUnit.IsReserve = true;
			}
			if (component.tile.GroundUnit != null)
			{
				component.tile.GroundUnit.IsReserve = true;
			}
		}
		component.tile.GroundUnit = null;
		component.tile.AirUnit = null;
		UnityEngine.Object.Destroy(component.groundUnitGO);
		UnityEngine.Object.Destroy(component.airUnitGO);
		rightClick_Panel.SetActive(value: false);
		GetComponent<ReservePanelGO>().GenerateButtonsList();
		tooltip.SetActive(value: false);
	}

	private void SetflagForVP(Player pOwner)
	{
		selectedPlacedTile.GetComponent<TileEditorGO>().DestroyFlag();
		GameObject gameObject;
		if (selectedPlacedTile.GetComponent<TileEditorGO>().tile.PrimaryObjective)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Flags/Major VP"));
		}
		else
		{
			gameObject = ((!selectedPlacedTile.GetComponent<TileEditorGO>().tile.SecondaryObjective) ? UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Flags/Normal VP")) : UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Flags/Secondary VP")));
		}
		selectedPlacedTile.GetComponent<TileEditorGO>().flag = gameObject;
		gameObject.transform.position = new Vector3(selectedPlacedTile.transform.position.x - 1f, selectedPlacedTile.transform.position.y + 2.3f, -1f);
		gameObject.transform.SetParent(selectedPlacedTile.GetComponent<Transform>());
		gameObject.name = "flag";
		gameObject.transform.Find("Flag representation").GetComponentInChildren<SpriteRenderer>().sprite = Utils.GetFlagSprite(pOwner.Country);
	}

	public void AddNewPlayer()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = -1;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			string text = string.Empty;
			for (int i = 0; i < listOfPlayer.Name.Count(); i++)
			{
				if (char.IsDigit(listOfPlayer.Name[i]))
				{
					text += listOfPlayer.Name[i];
				}
			}
			if (int.Parse(text) > num)
			{
				num = int.Parse(text);
			}
		}
		Player player = new Player("Player" + (num + 1));
		player.CheckForMissingUnitsCategories();
		GameData.Instance.listOfPlayers.Add(player);
		RefreshListOfPlayerSlots();
	}

	public void RemovePlayer(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (GameData.Instance.listOfPlayers.Count == 2)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid player count"));
			return;
		}
		for (int i = 0; i < GameData.Instance.listOfPlayers.Count; i++)
		{
			if (p_player.Name == GameData.Instance.listOfPlayers[i].Name)
			{
				GameData.Instance.listOfPlayers.RemoveAt(i);
				break;
			}
		}
		for (int j = 0; j < GameData.Instance.map.SizeX; j++)
		{
			for (int k = 0; k < GameData.Instance.map.SizeY; k++)
			{
				if (GameData.Instance.map.TilesTable[j, k].GroundUnit != null && GameData.Instance.map.TilesTable[j, k].GroundUnit.OwnerName == p_player.Name)
				{
					GameData.Instance.map.TilesTable[j, k].GroundUnit = null;
					UnityEngine.Object.Destroy(TableOfAllTiles[j, k].groundUnitGO.gameObject);
				}
				if (GameData.Instance.map.TilesTable[j, k].AirUnit != null && GameData.Instance.map.TilesTable[j, k].AirUnit.OwnerName == p_player.Name)
				{
					GameData.Instance.map.TilesTable[j, k].AirUnit = null;
					UnityEngine.Object.Destroy(TableOfAllTiles[j, k].airUnitGO.gameObject);
				}
				if (GameData.Instance.map.TilesTable[j, k].OwnerName == p_player.Name)
				{
					GameData.Instance.map.TilesTable[j, k].ownerPlayer = null;
					GameData.Instance.map.TilesTable[j, k].IsVictoryPoint = false;
					TableOfAllTiles[j, k].DestroyFlag();
				}
			}
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			foreach (EventAction eventAction in @event.EventActions)
			{
				if (!string.IsNullOrEmpty(eventAction.TargetPlayerName1) && !GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var _))
				{
					eventAction.TargetPlayerName1 = null;
				}
				if (!string.IsNullOrEmpty(eventAction.TargetPlayerName2) && !GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName2, out var _))
				{
					eventAction.TargetPlayerName2 = null;
				}
			}
		}
		GameData.Instance.map.listOfUnitsThatNeedToBeKilled = new Dictionary<Unit, bool>();
		for (int l = 0; l < GameData.Instance.map.SizeX; l++)
		{
			for (int m = 0; m < GameData.Instance.map.SizeY; m++)
			{
				if (GameData.Instance.map.TilesTable[l, m].GroundUnit != null && GameData.Instance.map.TilesTable[l, m].GroundUnit.IsKillGoal)
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(GameData.Instance.map.TilesTable[l, m].GroundUnit, value: false);
				}
				if (GameData.Instance.map.TilesTable[l, m].AirUnit != null && GameData.Instance.map.TilesTable[l, m].AirUnit.IsKillGoal)
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(GameData.Instance.map.TilesTable[l, m].AirUnit, value: false);
				}
			}
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				if (listOfUnit.IsKillGoal && listOfUnit.IsReserve && !GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ContainsKey(listOfUnit))
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(listOfUnit, value: false);
				}
			}
		}
		RefreshListOfPlayerSlots();
		GenerateBorders();
	}

	public void ShowHideAllUnitsOnTheMap()
	{
		SoundManager.instance.PlayUIClickSound();
		unitParent.gameObject.SetActive(!unitParent.gameObject.activeSelf);
	}

	public void CheckMapReadyToPlay()
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (string.IsNullOrWhiteSpace(listOfPlayer.Country) || listOfPlayer.Country == "None")
			{
				errorPanel.SetActive(value: true);
				error_Description.color = Color.red;
				error_Description.text = listOfPlayer.Name + LocalizationManager.Translate("error-player without country");
				return;
			}
		}
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (string.IsNullOrWhiteSpace(listOfPlayer2.Faction) || listOfPlayer2.Faction == "None")
			{
				errorPanel.SetActive(value: true);
				error_Description.color = Color.red;
				error_Description.text = listOfPlayer2.Name + LocalizationManager.Translate("error-player without faction");
				return;
			}
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (Player listOfPlayer3 in GameData.Instance.listOfPlayers)
		{
			dictionary.Add(listOfPlayer3.Name, value: false);
		}
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OwnerName != null)
				{
					dictionary[GameData.Instance.map.TilesTable[i, j].OwnerName] = true;
				}
			}
		}
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			if (!item.Value)
			{
				errorPanel.SetActive(value: true);
				error_Description.color = Color.red;
				error_Description.text = item.Key + LocalizationManager.Translate("error-player missing vp");
				return;
			}
		}
		errorPanel.SetActive(value: true);
		error_Description.color = Color.green;
		error_Description.text = LocalizationManager.Translate("info-map ready to play");
	}

	public void ManageSaveAsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		saveAsPanel.SetActive(!saveAsPanel.activeSelf);
	}

	public void SaveTheMap()
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (!@event.IsEventReady(p_includingActions: true))
			{
				errorPanel.SetActive(value: true);
				error_Description.color = Color.red;
				error_Description.text = @event.Name + LocalizationManager.Translate("error-event not properly set up");
				return;
			}
		}
		selectedTileGO = null;
		GameData.Instance.ironManMode = false;
		if (MainMenu.selectedMap == null)
		{
			MainMenu.selectedMap = new FileInfo(Application.persistentDataPath + "/Maps/" + MapEditorMenu.mapName + ".sav");
		}
		GameData.Instance.Save(MainMenu.selectedMap);
		errorPanel.SetActive(value: true);
		error_Description.color = Color.green;
		error_Description.text = LocalizationManager.Translate("info-successful saving");
		Invoke("AutoCloseSavePopup", 2f);
		SteamManager.UnlockAchievement("MapEditor");
	}

	public void SaveMapAs()
	{
		SoundManager.instance.PlayUIClickSound();
		if (saveAs_inputField.text == "")
		{
			return;
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (!@event.IsEventReady(p_includingActions: true))
			{
				errorPanel.SetActive(value: true);
				error_Description.color = Color.red;
				error_Description.text = @event.Name + LocalizationManager.Translate("error-event not properly set up");
				return;
			}
		}
		selectedTileGO = null;
		GameData.Instance.ironManMode = false;
		GameData.Instance.map.IsOfficial = false;
		MainMenu.selectedMap = new FileInfo(Application.persistentDataPath + "/Maps/" + saveAs_inputField.text + ".sav");
		GameData.Instance.Save(MainMenu.selectedMap);
		errorPanel.SetActive(value: true);
		error_Description.color = Color.green;
		error_Description.text = LocalizationManager.Translate("info-successful saving");
		SteamManager.UnlockAchievement("MapEditor");
	}

	private void AutoCloseSavePopup()
	{
		if (errorPanel.activeSelf)
		{
			CloseErrorPanel();
		}
	}

	public void CloseErrorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		errorPanel.SetActive(value: false);
	}

	public void SelectTile(Tile tile)
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = GameObject.Find(EventSystem.current.currentSelectedGameObject.name);
		selectedTile = new Tile
		{
			Name = tile.Name,
			Type = tile.Type,
			Season = tile.Season,
			MPCost = tile.MPCost,
			DamageModificator = tile.DamageModificator,
			MoneyProductionPerTurn = tile.MoneyProductionPerTurn,
			Manpower = tile.Manpower,
			MoneyReward = tile.MoneyReward,
			IsReinforcementTile = tile.IsReinforcementTile,
			Railroad = tile.Railroad,
			Bridge = tile.Bridge
		};
		previousTile = null;
		previousTileRoad = null;
	}

	public void SetFilter()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedTileGO = null;
		filter = EventSystem.current.currentSelectedGameObject.name;
		previousTile = null;
		if (filter.Contains("Desert"))
		{
			season = "Desert";
		}
		Populate();
	}

	public void ToggleGeneratorsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		generators_Panel.SetActive(!generators_Panel.activeSelf);
	}

	public void GenerateRailroads()
	{
		SoundManager.instance.PlayUIClickSound();
		List<TileUndoState> currentBatch = new List<TileUndoState>();
		List<Tile> list = RailroadGenerator.GenerateRailroads(GameData.Instance.map, 1, (Tile t) =>
		{
			bool flag = false;
			foreach (TileUndoState item in currentBatch)
			{
				if (item.tile == t)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				currentBatch.Add(new TileUndoState
				{
					tile = t,
					oldName = t.Name,
					oldType = t.Type,
					oldFilterType = t.FilterType,
					oldBridge = t.Bridge,
					oldRailroad = t.Railroad
				});
			}
		}, (Tile t) =>
		{
			if (TableOfAllTiles != null && TableOfAllTiles[t.PosX, t.PosY] != null)
			{
				TableOfAllTiles[t.PosX, t.PosY].tile = t;
				TableOfAllTiles[t.PosX, t.PosY].GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(t);
			}
		});
		string railroadPairKey = RailroadGenerator.GetRailroadPairKey(list);
		if (currentBatch.Count > 0)
		{
			_railroadUndoHistory.Push(new RailroadUndoBatch
			{
				TileStates = currentBatch,
				PairKey = railroadPairKey
			});
		}
		Utils.UpdateTilesData(p_nonDestructive: true);
		FlashTiles(list);
	}

	public void UndoRailroadsGeneration()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_railroadUndoHistory.Count == 0)
		{
			Debug.Log("No railroad generation history to undo.");
			return;
		}
		RailroadUndoBatch railroadUndoBatch = _railroadUndoHistory.Pop();
		foreach (TileUndoState tileState in railroadUndoBatch.TileStates)
		{
			tileState.tile.Name = tileState.oldName;
			tileState.tile.Type = tileState.oldType;
			tileState.tile.FilterType = tileState.oldFilterType;
			tileState.tile.Bridge = tileState.oldBridge;
			tileState.tile.Railroad = tileState.oldRailroad;
			tileState.tile.SetRuntimeOptimizations();
			if (TableOfAllTiles != null && TableOfAllTiles[tileState.tile.PosX, tileState.tile.PosY] != null)
			{
				TileEditorGO tileEditorGO = TableOfAllTiles[tileState.tile.PosX, tileState.tile.PosY];
				tileEditorGO.GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(tileState.tile);
				tileEditorGO.tile = tileState.tile;
			}
		}
		RailroadGenerator.RemoveGeneratedRailroadPair(GameData.Instance.map, railroadUndoBatch.PairKey);
		Debug.Log($"Undid railroad generation. History size: {_railroadUndoHistory.Count}");
	}

	public void GenerateRivers()
	{
		SoundManager.instance.PlayUIClickSound();
		List<TileUndoState> currentBatch = new List<TileUndoState>();
		List<Tile> riverTiles = RiverGenerator.GenerateRivers(GameData.Instance.map, 1, (Tile t) =>
		{
			bool flag = false;
			foreach (TileUndoState item in currentBatch)
			{
				if (item.tile == t)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				currentBatch.Add(new TileUndoState
				{
					tile = t,
					oldName = t.Name,
					oldType = t.Type,
					oldFilterType = t.FilterType,
					oldBridge = t.Bridge,
					oldRailroad = t.Railroad
				});
			}
		}, (Tile t) =>
		{
			if (TableOfAllTiles != null && TableOfAllTiles[t.PosX, t.PosY] != null)
			{
				TableOfAllTiles[t.PosX, t.PosY].tile = t;
				TableOfAllTiles[t.PosX, t.PosY].GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(t);
			}
		});
		if (currentBatch.Count > 0)
		{
			_riverUndoHistory.Push(currentBatch);
		}
		Utils.UpdateTilesData(p_nonDestructive: true);
		FlashTiles(riverTiles);
	}

	public void UndoRiverGeneration()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_riverUndoHistory.Count == 0)
		{
			Debug.Log("No river generation history to undo.");
			return;
		}
		foreach (TileUndoState item in _riverUndoHistory.Pop())
		{
			item.tile.Name = item.oldName;
			item.tile.Type = item.oldType;
			item.tile.FilterType = item.oldFilterType;
			item.tile.Bridge = item.oldBridge;
			item.tile.Railroad = item.oldRailroad;
			item.tile.SetRuntimeOptimizations();
			if (TableOfAllTiles != null && TableOfAllTiles[item.tile.PosX, item.tile.PosY] != null)
			{
				TileEditorGO tileEditorGO = TableOfAllTiles[item.tile.PosX, item.tile.PosY];
				tileEditorGO.GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(item.tile);
				tileEditorGO.tile = item.tile;
			}
		}
		Debug.Log($"Undid river generation. History size: {_riverUndoHistory.Count}");
	}

	private void FlashTiles(List<Tile> riverTiles)
	{
		foreach (Tile riverTile in riverTiles)
		{
			if (TableOfAllTiles != null && TableOfAllTiles[riverTile.PosX, riverTile.PosY] != null)
			{
				StartCoroutine(CR_FlashTile(TableOfAllTiles[riverTile.PosX, riverTile.PosY]));
			}
		}
	}

	private IEnumerator CR_FlashTile(TileEditorGO p_tileEditorGO)
	{
		yield return new WaitForSeconds(0.1f);
		GameObject highlightObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Highlight ring"));
		highlightObj.transform.position = new Vector3(p_tileEditorGO.transform.position.x, p_tileEditorGO.transform.position.y, -0.48f);
		SpriteRenderer highlightSR = highlightObj.GetComponent<SpriteRenderer>();
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t2 = t / 0.5f;
			highlightSR.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
			yield return null;
		}
		highlightSR.color = Color.white;
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t3 = t / 0.5f;
			highlightSR.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t3);
			yield return null;
		}
		highlightSR.color = new Color(1f, 1f, 1f, 0f);
		UnityEngine.Object.Destroy(highlightObj);
	}

	public void GenerateRoadsOnMap()
	{
		Utils.UpdateTilesData(p_nonDestructive: false);
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
				TableOfAllTiles[m, n].GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(tile5);
			}
		}
	}

	public void QuitToMainMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("txt-confirmation quit to main menu"), QuitToMainMenuConfirmation);
	}

	private void QuitToMainMenuConfirmation()
	{
		UIManager.isUIOpen = false;
		StartCoroutine(CR_QuitToMainMenu());
	}

	private IEnumerator CR_QuitToMainMenu()
	{
		UI.SetActive(value: false);
		yield return null;
		Physics2D.simulationMode = SimulationMode2D.Script;
		yield return null;
		CameraGO.instance.cam.cullingMask = 0;
		CameraGO.instance.cam.orthographicSize = 5f;
		yield return null;
		yield return DestroyChildrenInBatches(transform);
		AsyncOperation loadMainMenu = SceneManager.LoadSceneAsync(0);
		while (!loadMainMenu.isDone)
		{
			yield return null;
		}
		yield return Resources.UnloadUnusedAssets();
	}

	private IEnumerator DestroyChildrenInBatches(Transform parent, int batchSize = 1000)
	{
		while (parent != null && parent.childCount > 0)
		{
			int childCount = parent.childCount;
			int num = Mathf.Max(0, childCount - batchSize);
			for (int num2 = childCount - 1; num2 >= num; num2--)
			{
				UnityEngine.Object.Destroy(parent.GetChild(num2).gameObject);
			}
			yield return null;
		}
	}

	private void Populate()
	{
		_listOfTilesToChooseFrom = new List<Tile>();
		Transform[] componentsInChildren = tileChoiceContainer.GetComponent<Transform>().GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.gameObject.name != tileChoiceContainer.name)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		foreach (Tile allTile in Utils.GetAllTiles())
		{
			if (allTile.Season == season && (allTile.FilterType == filter || allTile.FilterType == "FilterAll"))
			{
				_listOfTilesToChooseFrom.Add(allTile);
			}
		}
		_listOfTilesToChooseFrom = _listOfTilesToChooseFrom.OrderBy((Tile x) => x.Name).ToList();
		foreach (Tile tile in _listOfTilesToChooseFrom)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EditorTileButton"), tileChoiceContainer.transform);
			gameObject.name = tile.Name;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.GetComponent<Image>().sprite = Utils.GetTileSprite(tile);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.Name;
			gameObject.GetComponent<Button>().onClick.AddListener(() =>
			{
				SelectTile(tile);
			});
		}
	}

	public void SpawnUnitsOnMap(bool p_garrisonOnly)
	{
		SoundManager.instance.PlayUIClickSound();
		unitParent.gameObject.SetActive(value: false);
		bool flag = Utils.GetModdedUnits().Count > 0 && (GameData.Instance.map.Date.Year >= 1946 || GameData.Instance.map.Date.Year <= 1930);
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			List<Unit> list = new List<Unit>();
			foreach (Unit item in from u in Utils.GetAllUnits()
				orderby u.Name
				select u)
			{
				if (item.Country != listOfPlayer.Country || (item.MaxPurchaseTime > 0 && listOfPlayer.PurchasedUnits.ContainsKey(item.Name) && listOfPlayer.PurchasedUnits[item.Name] >= item.MaxPurchaseTime))
				{
					continue;
				}
				if (item.FilterType == "FilterAA")
				{
					if (item.Type == "Tank")
					{
						continue;
					}
				}
				else
				{
					if (!(item.FilterType == "FilterInfantry"))
					{
						continue;
					}
					if (p_garrisonOnly)
					{
						if (!item.GetNameToLower().Contains("garrison"))
						{
							continue;
						}
					}
					else if (flag)
					{
						if (item.MaxMP == 0)
						{
							continue;
						}
					}
					else if (!item.GetNameToLower().Contains("light infantry") && !item.GetNameToLower().Contains("heavy infantry"))
					{
						continue;
					}
				}
				if (new DateTime(item.ProductionYear, Mathf.Clamp(item.ProductionMonth, 1, 12), 1) <= GameData.Instance.map.Date && GameData.Instance.map.Date.Year <= item.EndUseYear)
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				continue;
			}
			int sizeX = GameData.Instance.map.SizeX;
			int sizeY = GameData.Instance.map.SizeY;
			for (int num = 0; num < sizeX; num++)
			{
				for (int num2 = 0; num2 < sizeY; num2++)
				{
					if (!GameData.Instance.map.TilesTable[num, num2].IsVictoryPoint || GameData.Instance.map.TilesTable[num, num2].Type == "Sea" || GameData.Instance.map.TilesTable[num, num2].IsReinforcementTile || !(GameData.Instance.map.TilesTable[num, num2].OwnerName == listOfPlayer.Name) || GameData.Instance.map.TilesTable[num, num2].GroundUnit != null)
					{
						continue;
					}
					string text = string.Empty;
					if (GameData.Instance.map.TilesTable[num, num2].Type == "Airport" || GameData.Instance.map.TilesTable[num, num2].Type == "Factory" || GameData.Instance.map.TilesTable[num, num2].Type == "Refinery")
					{
						int num3 = 0;
						foreach (Unit item2 in list)
						{
							if (item2.FilterType == "FilterAA")
							{
								num3++;
							}
						}
						if (num3 > 0)
						{
							text = "FilterAA";
						}
					}
					if (text == string.Empty)
					{
						text = "FilterInfantry";
					}
					int index = UnityEngine.Random.Range(0, list.Count);
					int num4 = 0;
					while (list[index].FilterType != text)
					{
						index = UnityEngine.Random.Range(0, list.Count);
						if (list[index].FilterType == text || num4 > 500)
						{
							break;
						}
						num4++;
					}
					Unit unit = list[index].Clone(p_generateNewID: true);
					unit.GenerateNewID();
					unit.Lvl = 1;
					unit.XP = 0;
					unit.Morale = Unit.MORALE_DEFAULT;
					unit.CustomName = null;
					unit.CalculateAndSetUnitPrice();
					unit.IsDefensive = true;
					unit.OwnerPlayer = listOfPlayer;
					unit.OwnerName = listOfPlayer.Name;
					listOfPlayer.AddUnit(unit, p_sendRPC: false);
					if (unit.MaxPurchaseTime > 0)
					{
						if (listOfPlayer.PurchasedUnits.ContainsKey(unit.Name))
						{
							listOfPlayer.PurchasedUnits[unit.Name]++;
						}
						else
						{
							listOfPlayer.PurchasedUnits.Add(unit.Name, 1);
						}
					}
					TableOfAllTiles[num, num2].SpawnUnit(unit);
				}
			}
		}
		unitParent.gameObject.SetActive(value: true);
	}

	private void RefreshLandOwners()
	{
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		Queue<Tile> queue = new Queue<Tile>();
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				tilesTable[i, j].OnQueue = false;
				tilesTable[i, j].terrainOwner = null;
				if (tilesTable[i, j].IsVictoryPoint && tilesTable[i, j].ownerPlayer != null)
				{
					queue.Enqueue(tilesTable[i, j]);
					tilesTable[i, j].OnQueue = true;
				}
			}
		}
		while (queue.Count != 0)
		{
			Tile tile = queue.Dequeue();
			foreach (Tile neighbour in tile.neighbours)
			{
				if (!neighbour.OnQueue && (neighbour.Name == "Lake" || neighbour.Type != "Sea"))
				{
					if (tile.ownerPlayer != null)
					{
						neighbour.terrainOwner = tile.ownerPlayer;
					}
					else
					{
						neighbour.terrainOwner = tile.terrainOwner;
					}
					queue.Enqueue(neighbour);
					neighbour.OnQueue = true;
				}
			}
		}
	}

	public void GenerateBorders()
	{
		Vector3 position = new Vector3(-1.33f, 2.31f);
		Vector3 position2 = new Vector3(-1f * position.x, position.y);
		Vector3 position3 = new Vector3(2.65f, 0f);
		Vector3 position4 = new Vector3(-1f * position3.x, position3.y);
		Vector3 position5 = new Vector3(position.x, -1f * position.y);
		Vector3 position6 = new Vector3(-1f * position.x, -1f * position.y);
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		RefreshLandOwners();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (string.IsNullOrEmpty(listOfPlayer.Country) || dictionary.ContainsKey(listOfPlayer.Country))
			{
				continue;
			}
			Sprite sprite = null;
			foreach (Sprite moddedUnitsGraphic in ModsManager.moddedUnitsGraphics)
			{
				if (moddedUnitsGraphic.name.Contains(listOfPlayer.Country) && moddedUnitsGraphic.name.Contains("Counter full square"))
				{
					sprite = moddedUnitsGraphic;
					break;
				}
			}
			if (sprite == null)
			{
				sprite = Resources.Load<Sprite>("Units/" + listOfPlayer.Country + "/Counter full square");
			}
			if ((bool)sprite)
			{
				dictionary.Add(listOfPlayer.Country, sprite.texture.GetPixel(sprite.texture.width / 2, sprite.texture.height / 2));
			}
			else
			{
				dictionary.Add(listOfPlayer.Country, Color.white);
			}
		}
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile = tilesTable[i, j];
				LineRenderer lineRenderer = TableOfAllTiles[i, j].lineRenderer;
				int num = 0;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = false;
				for (byte b = 0; b < tile.amountOfNeighbours; b++)
				{
					Tile tile2 = tile.neighbours[b];
					if (tile.terrainOwner != null && tile2.terrainOwner != null && tile2.terrainOwner != tile.terrainOwner)
					{
						num++;
						if (tile2.PosX == tile.PosX && tile2.PosY == tile.PosY + 1)
						{
							flag = true;
						}
						else if (tile2.PosX == tile.PosX && tile2.PosY == tile.PosY - 1)
						{
							flag2 = true;
						}
						if (tile.PosX % 2 == 0)
						{
							if (tile2.PosX == tile.PosX - 1 && tile2.PosY == tile.PosY + 1)
							{
								flag4 = true;
							}
							else if (tile2.PosX == tile.PosX - 1 && tile2.PosY == tile.PosY)
							{
								flag6 = true;
							}
							else if (tile2.PosX == tile.PosX + 1 && tile2.PosY == tile.PosY + 1)
							{
								flag3 = true;
							}
							else if (tile2.PosX == tile.PosX + 1 && tile2.PosY == tile.PosY)
							{
								flag5 = true;
							}
						}
						else if (tile2.PosX == tile.PosX - 1 && tile2.PosY == tile.PosY)
						{
							flag4 = true;
						}
						else if (tile2.PosX == tile.PosX - 1 && tile2.PosY == tile.PosY - 1)
						{
							flag6 = true;
						}
						else if (tile2.PosX == tile.PosX + 1 && tile2.PosY == tile.PosY)
						{
							flag3 = true;
						}
						else if (tile2.PosX == tile.PosX + 1 && tile2.PosY == tile.PosY - 1)
						{
							flag5 = true;
						}
					}
				}
				if (num == 0)
				{
					lineRenderer.enabled = false;
					continue;
				}
				lineRenderer.enabled = true;
				lineRenderer.loop = false;
				string key = tilesTable[i, j].terrainOwner.Country;
				lineRenderer.startColor = dictionary[key];
				lineRenderer.endColor = lineRenderer.startColor;
				switch (num)
				{
				case 1:
					if (flag)
					{
						lineRenderer.SetPosition(0, position);
						lineRenderer.SetPosition(1, position2);
						lineRenderer.SetPosition(2, position2);
						lineRenderer.SetPosition(3, position2);
						lineRenderer.SetPosition(4, position2);
						lineRenderer.SetPosition(5, position2);
					}
					else if (flag3)
					{
						lineRenderer.SetPosition(0, position2);
						lineRenderer.SetPosition(1, position3);
						lineRenderer.SetPosition(2, position3);
						lineRenderer.SetPosition(3, position3);
						lineRenderer.SetPosition(4, position3);
						lineRenderer.SetPosition(5, position3);
					}
					else if (flag5)
					{
						lineRenderer.SetPosition(0, position3);
						lineRenderer.SetPosition(1, position6);
						lineRenderer.SetPosition(2, position6);
						lineRenderer.SetPosition(3, position6);
						lineRenderer.SetPosition(4, position6);
						lineRenderer.SetPosition(5, position6);
					}
					else if (flag2)
					{
						lineRenderer.SetPosition(0, position6);
						lineRenderer.SetPosition(1, position5);
						lineRenderer.SetPosition(2, position5);
						lineRenderer.SetPosition(3, position5);
						lineRenderer.SetPosition(4, position5);
						lineRenderer.SetPosition(5, position5);
					}
					else if (flag6)
					{
						lineRenderer.SetPosition(0, position5);
						lineRenderer.SetPosition(1, position4);
						lineRenderer.SetPosition(2, position4);
						lineRenderer.SetPosition(3, position4);
						lineRenderer.SetPosition(4, position4);
						lineRenderer.SetPosition(5, position4);
					}
					else if (flag4)
					{
						lineRenderer.SetPosition(0, position4);
						lineRenderer.SetPosition(1, position);
						lineRenderer.SetPosition(2, position);
						lineRenderer.SetPosition(3, position);
						lineRenderer.SetPosition(4, position);
						lineRenderer.SetPosition(5, position);
					}
					else
					{
						lineRenderer.enabled = false;
					}
					break;
				case 2:
					if (flag & flag3)
					{
						lineRenderer.SetPosition(0, position);
						lineRenderer.SetPosition(1, position2);
						lineRenderer.SetPosition(2, position3);
						lineRenderer.SetPosition(3, position3);
						lineRenderer.SetPosition(4, position3);
						lineRenderer.SetPosition(5, position3);
					}
					else if (flag3 & flag5)
					{
						lineRenderer.SetPosition(0, position2);
						lineRenderer.SetPosition(1, position3);
						lineRenderer.SetPosition(2, position6);
						lineRenderer.SetPosition(3, position6);
						lineRenderer.SetPosition(4, position6);
						lineRenderer.SetPosition(5, position6);
					}
					else if (flag5 & flag2)
					{
						lineRenderer.SetPosition(0, position3);
						lineRenderer.SetPosition(1, position6);
						lineRenderer.SetPosition(2, position5);
						lineRenderer.SetPosition(3, position5);
						lineRenderer.SetPosition(4, position5);
						lineRenderer.SetPosition(5, position5);
					}
					else if (flag2 & flag6)
					{
						lineRenderer.SetPosition(0, position6);
						lineRenderer.SetPosition(1, position5);
						lineRenderer.SetPosition(2, position4);
						lineRenderer.SetPosition(3, position4);
						lineRenderer.SetPosition(4, position4);
						lineRenderer.SetPosition(5, position4);
					}
					else if (flag6 & flag4)
					{
						lineRenderer.SetPosition(0, position5);
						lineRenderer.SetPosition(1, position4);
						lineRenderer.SetPosition(2, position);
						lineRenderer.SetPosition(3, position);
						lineRenderer.SetPosition(4, position);
						lineRenderer.SetPosition(5, position);
					}
					else if (flag4 & flag)
					{
						lineRenderer.SetPosition(0, position4);
						lineRenderer.SetPosition(1, position);
						lineRenderer.SetPosition(2, position2);
						lineRenderer.SetPosition(3, position2);
						lineRenderer.SetPosition(4, position2);
						lineRenderer.SetPosition(5, position2);
					}
					else
					{
						lineRenderer.enabled = false;
					}
					break;
				case 3:
					if (flag & flag3 & flag5)
					{
						lineRenderer.SetPosition(0, position);
						lineRenderer.SetPosition(1, position2);
						lineRenderer.SetPosition(2, position3);
						lineRenderer.SetPosition(3, position6);
						lineRenderer.SetPosition(4, position6);
						lineRenderer.SetPosition(5, position6);
					}
					else if (flag3 & flag5 & flag2)
					{
						lineRenderer.SetPosition(0, position2);
						lineRenderer.SetPosition(1, position3);
						lineRenderer.SetPosition(2, position6);
						lineRenderer.SetPosition(3, position5);
						lineRenderer.SetPosition(4, position5);
						lineRenderer.SetPosition(5, position5);
					}
					else if (flag5 & flag2 & flag6)
					{
						lineRenderer.SetPosition(0, position3);
						lineRenderer.SetPosition(1, position6);
						lineRenderer.SetPosition(2, position5);
						lineRenderer.SetPosition(3, position4);
						lineRenderer.SetPosition(4, position4);
						lineRenderer.SetPosition(5, position4);
					}
					else if (flag2 & flag6 & flag4)
					{
						lineRenderer.SetPosition(0, position6);
						lineRenderer.SetPosition(1, position5);
						lineRenderer.SetPosition(2, position4);
						lineRenderer.SetPosition(3, position);
						lineRenderer.SetPosition(4, position);
						lineRenderer.SetPosition(5, position);
					}
					else if (flag6 & flag4 & flag)
					{
						lineRenderer.SetPosition(0, position5);
						lineRenderer.SetPosition(1, position4);
						lineRenderer.SetPosition(2, position);
						lineRenderer.SetPosition(3, position2);
						lineRenderer.SetPosition(4, position2);
						lineRenderer.SetPosition(5, position2);
					}
					else if (flag4 & flag & flag3)
					{
						lineRenderer.SetPosition(0, position4);
						lineRenderer.SetPosition(1, position);
						lineRenderer.SetPosition(2, position2);
						lineRenderer.SetPosition(3, position3);
						lineRenderer.SetPosition(4, position3);
						lineRenderer.SetPosition(5, position3);
					}
					else
					{
						lineRenderer.enabled = false;
					}
					break;
				case 4:
					if (flag & flag3 & flag5 & flag2)
					{
						lineRenderer.SetPosition(0, position);
						lineRenderer.SetPosition(1, position2);
						lineRenderer.SetPosition(2, position3);
						lineRenderer.SetPosition(3, position6);
						lineRenderer.SetPosition(4, position5);
						lineRenderer.SetPosition(5, position5);
					}
					else if (flag3 & flag5 & flag2 & flag6)
					{
						lineRenderer.SetPosition(0, position2);
						lineRenderer.SetPosition(1, position3);
						lineRenderer.SetPosition(2, position6);
						lineRenderer.SetPosition(3, position5);
						lineRenderer.SetPosition(4, position4);
						lineRenderer.SetPosition(5, position4);
					}
					else if (flag5 & flag2 & flag6 & flag4)
					{
						lineRenderer.SetPosition(0, position3);
						lineRenderer.SetPosition(1, position6);
						lineRenderer.SetPosition(2, position5);
						lineRenderer.SetPosition(3, position4);
						lineRenderer.SetPosition(4, position);
						lineRenderer.SetPosition(5, position);
					}
					else if (flag2 & flag6 & flag4 & flag)
					{
						lineRenderer.SetPosition(0, position6);
						lineRenderer.SetPosition(1, position5);
						lineRenderer.SetPosition(2, position4);
						lineRenderer.SetPosition(3, position);
						lineRenderer.SetPosition(4, position2);
						lineRenderer.SetPosition(5, position2);
					}
					else if (flag6 & flag4 & flag & flag3)
					{
						lineRenderer.SetPosition(0, position5);
						lineRenderer.SetPosition(1, position4);
						lineRenderer.SetPosition(2, position);
						lineRenderer.SetPosition(3, position2);
						lineRenderer.SetPosition(4, position3);
						lineRenderer.SetPosition(5, position3);
					}
					else if (flag4 & flag & flag3 & flag5)
					{
						lineRenderer.SetPosition(0, position4);
						lineRenderer.SetPosition(1, position);
						lineRenderer.SetPosition(2, position2);
						lineRenderer.SetPosition(3, position3);
						lineRenderer.SetPosition(4, position6);
						lineRenderer.SetPosition(5, position6);
					}
					else
					{
						lineRenderer.enabled = false;
					}
					break;
				case 5:
					if (flag & flag3 & flag5 & flag2 & flag6)
					{
						lineRenderer.SetPosition(0, position);
						lineRenderer.SetPosition(1, position2);
						lineRenderer.SetPosition(2, position3);
						lineRenderer.SetPosition(3, position6);
						lineRenderer.SetPosition(4, position5);
						lineRenderer.SetPosition(5, position4);
					}
					else if (flag3 & flag5 & flag2 & flag6 & flag4)
					{
						lineRenderer.SetPosition(0, position2);
						lineRenderer.SetPosition(1, position3);
						lineRenderer.SetPosition(2, position6);
						lineRenderer.SetPosition(3, position5);
						lineRenderer.SetPosition(4, position4);
						lineRenderer.SetPosition(5, position);
					}
					else if (flag5 & flag2 & flag6 & flag4 & flag)
					{
						lineRenderer.SetPosition(0, position3);
						lineRenderer.SetPosition(1, position6);
						lineRenderer.SetPosition(2, position5);
						lineRenderer.SetPosition(3, position4);
						lineRenderer.SetPosition(4, position);
						lineRenderer.SetPosition(5, position2);
					}
					else if (flag2 & flag6 & flag4 & flag & flag3)
					{
						lineRenderer.SetPosition(0, position6);
						lineRenderer.SetPosition(1, position5);
						lineRenderer.SetPosition(2, position4);
						lineRenderer.SetPosition(3, position);
						lineRenderer.SetPosition(4, position2);
						lineRenderer.SetPosition(5, position3);
					}
					else if (flag6 & flag4 & flag & flag3 & flag5)
					{
						lineRenderer.SetPosition(0, position5);
						lineRenderer.SetPosition(1, position4);
						lineRenderer.SetPosition(2, position);
						lineRenderer.SetPosition(3, position2);
						lineRenderer.SetPosition(4, position3);
						lineRenderer.SetPosition(5, position6);
					}
					else if (flag4 & flag & flag3 & flag5 & flag2)
					{
						lineRenderer.SetPosition(0, position4);
						lineRenderer.SetPosition(1, position);
						lineRenderer.SetPosition(2, position2);
						lineRenderer.SetPosition(3, position3);
						lineRenderer.SetPosition(4, position6);
						lineRenderer.SetPosition(5, position5);
					}
					else
					{
						lineRenderer.enabled = false;
					}
					break;
				case 6:
					lineRenderer.SetPosition(0, position);
					lineRenderer.SetPosition(1, position2);
					lineRenderer.SetPosition(2, position3);
					lineRenderer.SetPosition(3, position6);
					lineRenderer.SetPosition(4, position5);
					lineRenderer.SetPosition(5, position4);
					lineRenderer.loop = true;
					break;
				default:
					lineRenderer.enabled = false;
					break;
				}
			}
		}
	}

	public void ManageEventsPanel_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		if (events_panel.gameObject.activeSelf)
		{
			events_panel.Close();
		}
		else
		{
			events_panel.Open();
		}
	}

	private void OnApplicationQuit()
	{
		if (SteamAPI.IsSteamRunning())
		{
			SteamFriends.ClearRichPresence();
		}
	}
}
