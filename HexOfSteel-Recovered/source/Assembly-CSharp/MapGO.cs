using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapGO : MonoBehaviour
{
	public const float DISTANCE_BETWEEN_TWO_TILES = 5.15f;

	public static MapGO instance;

	public static bool IsAIOnly;

	public static Unit unitBeforeRightClickStats;

	public static UnitGO selectedUnitGO;

	public static int majorVPsAmount;

	public static List<TileGO> victoryPoints;

	public static List<TileGO> currentPlayerVPs;

	public static int numberOfCapturedMajorVPByFaction;

	public static bool isOver;

	public static bool INeedToMakeABackupOfTheCampaign;

	public static Player onSaveLoadedHumanPlayer;

	public GameObject panelAllStatsSelectedUnit;

	public GameObject panelAllStatsOtherUnit;

	public GameObject leftSideUI_Panel;

	public static Stopwatch timeElapsedSinceGameStarted;

	[Header("Lists")]
	public static HashSet<TileGO> radarDetections;

	public static HashSet<TileGO> sonarDetections;

	public static HashSet<TileGO> listOfSpawnableTiles;

	public static List<GameObject> listOfTargetedUnit;

	public static List<TileGO> listOfPotentialTilesAboveTheMap;

	public static List<UnitGO> listOfAllGeneralsOnTheMap;

	public static List<TileGO> listOfAllBridgesOnTheMap;

	public bool isDebugMode;

	[Header("Transform")]
	public Transform unitsParent;

	public Transform targetsParent;

	public Transform commandersParent;

	public Transform potentialTiles_parent;

	public Transform politicalMap_parent;

	[Header("GameObjects")]
	public GameObject selectedUnitIcon;

	public UnitGO unitPrefab;

	public UnitGO unitCorePrefab;

	public TileGO baseTilePrefab;

	public GameObject startScenario_Panel;

	public GameObject unitStatsPanel;

	public GameObject selectedUnitStatsPanel;

	public GameObject otherUnitStatsPanel;

	public GameObject moreStatsPanel;

	public GameObject potentialDamagePanel;

	[Header("Political map")]
	[SerializeField]
	private GameObject _politicalMapOpacityBox;

	[SerializeField]
	private AnimationCurve _politicalNameSize_curve;

	[SerializeField]
	private AnimationCurve _countryNamesOpacity_curve;

	public PoliticalHexRenderer _politicalHexRenderer_prefab;

	public TMP_Text _politicalCountryName_textmesh;

	[SerializeField]
	private Slider _politicalMapOpacity_slider;

	public List<Unit> listOfAllUnits;

	public Vector3 selectedUnitIconOriginalScale;

	public Vector3 selectedUnitIconOriginalScaleBIG;

	private PoliticalHexRenderer[,] _politicalHexRenderers;

	private List<TMP_Text> _countryNames = new List<TMP_Text>();

	private void Awake()
	{
		instance = this;
		MainMenu.isEditor = false;
		UIManager.isUIOpen = true;
		isOver = false;
		numberOfCapturedMajorVPByFaction = 0;
		majorVPsAmount = 0;
		IsAIOnly = false;
		UIManager.isNATO = PlayerSettings.Instance.UseNATObyDefault;
		MainMenu.isMainMenu = false;
		Time.timeScale = 1f;
		QualitySettings.pixelLightCount = 0;
		timeElapsedSinceGameStarted = new Stopwatch();
		timeElapsedSinceGameStarted.Start();
		TurnManager.currPlayer = null;
		TurnManager.humanPlayer = null;
		radarDetections = new HashSet<TileGO>();
		sonarDetections = new HashSet<TileGO>();
		listOfSpawnableTiles = new HashSet<TileGO>();
		listOfTargetedUnit = new List<GameObject>();
		listOfPotentialTilesAboveTheMap = new List<TileGO>();
		victoryPoints = new List<TileGO>();
		currentPlayerVPs = new List<TileGO>();
		listOfAllBridgesOnTheMap = new List<TileGO>();
		listOfAllGeneralsOnTheMap = new List<UnitGO>();
		listOfAllUnits = Utils.GetAllUnits();
		selectedUnitIconOriginalScale = selectedUnitIcon.transform.localScale;
		selectedUnitIconOriginalScaleBIG = selectedUnitIconOriginalScale * 3f;
		if (GameData.Instance.map.DisableManpower)
		{
			if (PlayerSettings.Instance.Manpower)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-scenario disable manpower"));
			}
			PlayerSettings.Instance.Manpower = false;
		}
		if (GameData.Instance.map.DisableResearch)
		{
			if (PlayerSettings.Instance.TechTree)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-scenario disable research"));
			}
			PlayerSettings.Instance.TechTree = false;
		}
		if (GameData.Instance.map.DisableUpkeep)
		{
			if (PlayerSettings.Instance.Upkeep)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-scenario disable upkeep"));
			}
			PlayerSettings.Instance.Upkeep = false;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			hashSet.Add(listOfPlayer.Country);
		}
		for (int i = 0; i < listOfAllUnits.Count; i++)
		{
			if (hashSet.Contains(listOfAllUnits[i].Country))
			{
				listOfAllUnits[i].CalculateAndSetUnitPrice();
			}
		}
		listOfAllUnits = listOfAllUnits.OrderBy((Unit x) => x.Cost).ToList();
		if (GameData.Instance.map.listOfVPThatNeedsToBeHeld == null)
		{
			GameData.Instance.map.listOfVPThatNeedsToBeHeld = new Dictionary<Tile, string>();
		}
		if (GameData.Instance.messages == null)
		{
			GameData.Instance.messages = new List<string>();
		}
		if (PlayerSettings.Instance.MoreContrastSelectedUnit)
		{
			selectedUnitIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/UI/Selected unit more contrast");
		}
		GameData.Instance.map.IsWinter = GameData.Instance.map.Date.Month >= 11 || GameData.Instance.map.Date.Month < 4;
		Utils.UpdateTilesData(p_nonDestructive: true);
		SetupDiplomacyOnMapStarts();
		GenerateMapAndInitializePlayers();
		UIManager.instance.UpdateVPtext();
		if (INeedToMakeABackupOfTheCampaign)
		{
			if (GameData.Instance.map.ScenarioName != "")
			{
				GameData.Instance.SaveAsSaveFile(Path.GetFileNameWithoutExtension(MainMenu.selectedMap.FullName));
			}
			else
			{
				GameData.Instance.SaveAsSaveFile(GameData.Instance.map.ScenarioName);
			}
		}
		INeedToMakeABackupOfTheCampaign = false;
	}

	private void Start()
	{
		LeanTween.init(1024);
		Policies.InitializePolicies();
		if (!GameData.Instance.map.InitializedManpower)
		{
			GameData.Instance.map.InitializedManpower = true;
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				listOfPlayer.SetAvailableManpower(listOfPlayer.CalculateMaxManpower() - listOfPlayer.GetFieldedManpower());
			}
		}
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshMoneyAmountUI();
		UIManager.instance.RefreshIncomePerTurnUI();
		if (MainMenu.isLoadedGame && GameData.Instance.endOfTurnPanelOpen)
		{
			TurnManager.instance.ShowHumanEndTurnPanel();
		}
		Utils.AssignGeographicalAreas();
		InitializeAIHarboursValidity();
		StartCoroutine(CR_GeneratePoliticalMap());
		if (!MainMenu.isLoadedGame)
		{
			StartCoroutine(CR_MakeUnitsFaceRightDirection());
		}
		SteamManager.SetRichPresenceStatus(SteamStatuses.DEFAULT);
	}

	public void StartScenario()
	{
		SoundManager.instance.PlayUIClickSound();
		startScenario_Panel.SetActive(value: false);
		UIManager.isUIOpen = false;
		if (TurnManager.currPlayer.IsComputer && (!MultiplayerManager.IsRealtimeMultiplayer() || Utils.IsMasterClient()))
		{
			AI.instance.startTurn = true;
			TurnManager.instance.ScenarioStartAnimateHourglass();
		}
	}

	private void GenerateMapAndInitializePlayers()
	{
		LoadPrevCampaignPlayers();
		GameData.BackupPlayerSettingsForSession();
		if (GameData.Instance.gameSettings.saveLocked || GameData.Instance.isPBEM || MultiplayerManager.IsRealtimeMultiplayer())
		{
			PlayerSettings.ApplyGameSettings(GameData.Instance.gameSettings);
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.CurrentUnitsResearch == null)
			{
				listOfPlayer.CurrentUnitsResearch = new Dictionary<string, float>();
				listOfPlayer.SetDefaultResearchPointsAmount(listOfAllUnits);
			}
			if (listOfPlayer.ListOfUnitsResearched.Count == 0 && PlayerSettings.Instance.TechTree && !GameData.Instance.map.IsCampaign)
			{
				listOfPlayer.SetDefaultResearchPointsAmount(listOfAllUnits);
			}
			listOfPlayer.SetupIdeology();
			if (!MainMenu.isLoadedGame)
			{
				listOfPlayer.UnitCategoriesQuantity = null;
			}
			bool flag = listOfPlayer.UnitCategoriesQuantity == null;
			listOfPlayer.CheckForMissingUnitsCategories();
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				listOfUnit.OwnerPlayer = listOfPlayer;
				listOfUnit.OwnerName = listOfPlayer.Name;
				listOfUnit.SetCommissionDate();
				listOfUnit.CalculateAndSetManpowerCost();
				if (listOfUnit.IsReserve)
				{
					Utils.UpdateUnitStatsOnTurnOne(listOfUnit, listOfAllUnits);
					listOfUnit.SetDefaultAuthorizedRotation();
				}
				if (flag)
				{
					listOfPlayer.AssignUnitCategoryID(listOfUnit);
				}
				listOfUnit.SetUnitCustomName();
			}
		}
		if (MainMenu.isLoadedGame)
		{
			foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer2.IsPlaying)
				{
					TurnManager.currPlayer = listOfPlayer2;
					break;
				}
			}
			if (TurnManager.currPlayer == null)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-curr player is null"));
				if (GameData.Instance.map.BackUpPlayerPlaying != null)
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-loading player backup"));
					for (byte b = 0; b < GameData.Instance.listOfPlayers.Count; b++)
					{
						if (GameData.Instance.listOfPlayers[b].Name == GameData.Instance.map.BackUpPlayerPlaying)
						{
							GameData.Instance.listOfPlayers[(b + 1) % GameData.Instance.listOfPlayers.Count].IsPlaying = true;
							TurnManager.currPlayer = GameData.Instance.listOfPlayers[(b + 1) % GameData.Instance.listOfPlayers.Count];
							break;
						}
					}
					if (TurnManager.currPlayer == null)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("error-could not load player backup"));
					}
					else
					{
						UIManager.ShowMessage(LocalizationManager.Translate("info-loaded player backup"));
					}
				}
				else
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-could not load player backup"));
					foreach (Player listOfPlayer3 in GameData.Instance.listOfPlayers)
					{
						if (listOfPlayer3.IsPlayingFirst)
						{
							listOfPlayer3.IsPlaying = true;
							TurnManager.currPlayer = listOfPlayer3;
						}
					}
				}
			}
			if (TurnManager.humanPlayer == null)
			{
				foreach (Player listOfPlayer4 in GameData.Instance.listOfPlayers)
				{
					if (!listOfPlayer4.IsComputer && listOfPlayer4.Faction == TurnManager.currPlayer.Faction && listOfPlayer4.IsPlaying)
					{
						TurnManager.humanPlayer = listOfPlayer4;
					}
				}
			}
			if (TurnManager.humanPlayer == null)
			{
				foreach (Player listOfPlayer5 in GameData.Instance.listOfPlayers)
				{
					if (!listOfPlayer5.IsComputer && listOfPlayer5.Faction == TurnManager.currPlayer.Faction)
					{
						TurnManager.humanPlayer = listOfPlayer5;
					}
				}
			}
		}
		else
		{
			foreach (Event @event in GameData.Instance.map.Events)
			{
				@event.ExecutionsLeft = @event.MaxExecutions;
			}
			if (!GameData.Instance.isPBEM && !MultiplayerManager.IsRealtimeMultiplayer())
			{
				GameData.Instance.map.FileName = Utils.GetFileNameWithoutExtension(MainMenu.selectedMap);
			}
			foreach (Player listOfPlayer6 in GameData.Instance.listOfPlayers)
			{
				if (GameData.Instance.map.IsCampaign && PlayerSettings.Instance.Policies && listOfPlayer6.HighCommandPoints == 0 && listOfPlayer6.IsComputer)
				{
					listOfPlayer6.HighCommandPoints = 100;
				}
				if (listOfPlayer6.IsPlayingFirst)
				{
					listOfPlayer6.IsPlaying = true;
					TurnManager.currPlayer = listOfPlayer6;
					if (!listOfPlayer6.IsComputer)
					{
						TurnManager.humanPlayer = listOfPlayer6;
					}
				}
				listOfPlayer6.MoneyAmount = (int)((float)listOfPlayer6.MoneyAmount * ((float)(int)listOfPlayer6.IncomeModifier / 10f));
				foreach (Unit listOfUnit2 in listOfPlayer6.ListOfUnits)
				{
					listOfUnit2.ActionPoints = 2;
				}
			}
		}
		if (TurnManager.humanPlayer == null)
		{
			IsAIOnly = true;
			foreach (Player listOfPlayer7 in GameData.Instance.listOfPlayers)
			{
				if (!listOfPlayer7.IsComputer)
				{
					TurnManager.humanPlayer = listOfPlayer7;
					IsAIOnly = false;
					break;
				}
			}
		}
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			GameData.Instance.TryFindLocalPlayer(out TurnManager.humanPlayer);
		}
		onSaveLoadedHumanPlayer = TurnManager.humanPlayer;
		if (TurnManager.currPlayer.IsComputer)
		{
			TurnManager.currPlayer.IsPlaying = false;
		}
		Utils.GetAllTilesNeighbours();
		Physics2D.simulationMode = SimulationMode2D.Script;
		gameObject.SetActive(value: false);
		unitsParent.gameObject.SetActive(value: false);
		Map map = GameData.Instance.map;
		int sizeX = map.SizeX;
		int sizeY = map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (short num = 0; num < sizeX; num++)
		{
			for (short num2 = 0; num2 < sizeY; num2++)
			{
				tilesTable[num, num2].tileGO = Object.Instantiate(baseTilePrefab, transform);
				TileGO tileGO = tilesTable[num, num2].tileGO;
				tileGO.name = num + " " + num2;
				Utils.SetAutoCoasts(tilesTable[num, num2]);
				tileGO.tile = tilesTable[num, num2];
				if (num % 2 != 0)
				{
					tileGO.transform.position = new Vector2((float)num * 4.42f, (float)num2 * 5.1f);
				}
				else
				{
					tileGO.transform.position = new Vector2((float)num * 4.42f, (float)num2 * 5.1f + 2.55f);
				}
				if (tilesTable[num, num2].AirUnit != null && !tilesTable[num, num2].AirUnit.IsDead)
				{
					RespawnPlayerUnits(tileGO, tilesTable[num, num2].AirUnit);
				}
				else
				{
					tilesTable[num, num2].AirUnit = null;
				}
				if (tilesTable[num, num2].GroundUnit != null && !tilesTable[num, num2].GroundUnit.IsDead)
				{
					RespawnPlayerUnits(tileGO, tilesTable[num, num2].GroundUnit);
				}
				else
				{
					tilesTable[num, num2].GroundUnit = null;
				}
				if (tilesTable[num, num2].IsVictoryPoint)
				{
					victoryPoints.Add(tileGO);
					if (!MainMenu.isLoadedGame)
					{
						tilesTable[num, num2].NumberOfTurnsSinceVPGotCaptured = byte.MaxValue;
					}
					tileGO.SetVPFlag();
					if (string.IsNullOrEmpty(tilesTable[num, num2].OriginalCountryOwner))
					{
						tilesTable[num, num2].OriginalCountryOwner = tilesTable[num, num2].ownerPlayer.Country;
					}
					if (tilesTable[num, num2].PrimaryObjective)
					{
						majorVPsAmount++;
					}
				}
				if (!string.IsNullOrEmpty(tilesTable[num, num2].CityName))
				{
					tileGO.SetCityName();
				}
				if (tilesTable[num, num2].Bridge)
				{
					listOfAllBridgesOnTheMap.Add(tileGO);
				}
				if (!MainMenu.isLoadedGame && map.turnCount <= 1 && tilesTable[num, num2].enumSeason == TileSeason.DESERT)
				{
					if (tilesTable[num, num2].Name == "Desert1")
					{
						tilesTable[num, num2].Name = "Plain";
					}
					else if (tilesTable[num, num2].Name == "DesertHill")
					{
						tilesTable[num, num2].Name = "Hill";
					}
					else if (tilesTable[num, num2].Name == "DesertVillage")
					{
						tilesTable[num, num2].Name = "Village";
					}
					else if (tilesTable[num, num2].Name == "Desert forest")
					{
						tilesTable[num, num2].Name = "ThinForest";
					}
					else if (tilesTable[num, num2].Name == "DesertAirport")
					{
						tilesTable[num, num2].Name = "Airport";
					}
				}
				tileGO.SetSprite();
			}
		}
		gameObject.SetActive(value: true);
		unitsParent.gameObject.SetActive(value: true);
		TurnManager.instance.ManageWeather();
		Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
	}

	public void UpdateVpsList()
	{
		victoryPoints.Clear();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				if (tilesTable[i, j].IsVictoryPoint)
				{
					victoryPoints.Add(tilesTable[i, j].tileGO);
				}
			}
		}
	}

	private void LoadPrevCampaignPlayers()
	{
		if (MainMenu.listOfPlayersToBeSavedBetweenCampaign != null && GameData.Instance.map.turnCount == 1)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.IsComputer)
				{
					continue;
				}
				foreach (Player item in MainMenu.listOfPlayersToBeSavedBetweenCampaign)
				{
					if (item.Country != listOfPlayer.Country)
					{
						continue;
					}
					foreach (Unit listOfUnit in item.ListOfUnits)
					{
						if (listOfUnit.IsCoreUnit)
						{
							listOfPlayer.ListOfUnits.Add(listOfUnit);
						}
					}
					listOfPlayer.XP = item.XP;
					listOfPlayer.Rank = item.Rank;
					listOfPlayer.IncomeModifier = item.IncomeModifier;
					listOfPlayer.HighCommandPoints = item.HighCommandPoints;
					listOfPlayer.ListActivePolicies = item.ListActivePolicies;
				}
			}
		}
		MainMenu.listOfPlayersToBeSavedBetweenCampaign = null;
	}

	public void ShowPotentialtileColor(bool isDeployment)
	{
		Dictionary<TileGO, GameObject> dictionary = new Dictionary<TileGO, GameObject>();
		foreach (TileGO item in listOfPotentialTilesAboveTheMap)
		{
			string text = ((!(item.tile.Season == "Winter") || (!(item.tile.Type != "Sea") && !item.tile.isFrozen)) ? "Summer" : "Winter");
			string text2;
			if (isDeployment)
			{
				text2 = "deploy";
			}
			else if (selectedUnitGO.unit.IsOnBoat || selectedUnitGO.unit.IsTrain)
			{
				text2 = "move";
			}
			else if (selectedUnitGO.unit.IsHorsed)
			{
				text2 = "move horse";
			}
			else if (selectedUnitGO.unit.IsMotorized)
			{
				text2 = "move motorized";
			}
			else
			{
				text2 = ((!selectedUnitGO.unit.IsMechanized) ? "move" : "move mechanized");
			}
			GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/" + text + " " + text2), potentialTiles_parent);
			if (ModsManager.moddedTilesGraphics.Count > 0)
			{
				gameObject.name = gameObject.name.Replace("(Clone)", "");
				foreach (Sprite moddedTilesGraphic in ModsManager.moddedTilesGraphics)
				{
					if (moddedTilesGraphic.name == gameObject.name)
					{
						gameObject.GetComponent<SpriteRenderer>().sprite = moddedTilesGraphic;
						break;
					}
				}
			}
			gameObject.name = item.name;
			gameObject.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, potentialTiles_parent.position.z + 0.2f);
			dictionary.Add(item, gameObject);
		}
		if (selectedUnitGO == null || selectedUnitGO.unit.Type != "Plane")
		{
			return;
		}
		Tile tile = selectedUnitGO.tileGO.tile;
		List<Tile> list = new List<Tile>();
		foreach (TileGO victoryPoint in victoryPoints)
		{
			if ((victoryPoint.tile.IsReinforcementTile || victoryPoint.tile.enumType == TileTypes.AIRPORT) && victoryPoint.tile.OwnerName != null && selectedUnitGO.unit.OwnerPlayer.IsAlliedWith(victoryPoint.tile.ownerPlayer))
			{
				list.Add(victoryPoint.tile);
			}
		}
		if (selectedUnitGO.unit.CanBeRefueledOnCarrier)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (!listOfPlayer.IsAlliedWith(selectedUnitGO.unit.OwnerPlayer))
				{
					continue;
				}
				foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
				{
					if (!listOfUnit.IsDead && !listOfUnit.IsReserve && listOfUnit.IsCarrier && !(listOfUnit.unitGO == null) && !(listOfUnit.unitGO.tileGO == null))
					{
						list.Add(listOfUnit.unitGO.tileGO.tile);
					}
				}
			}
		}
		Color color = new Color(1f, 0f, 0f, 0.4f);
		int currAutonomy = selectedUnitGO.unit.CurrAutonomy;
		foreach (KeyValuePair<TileGO, GameObject> item2 in dictionary)
		{
			Tile tile2 = item2.Key.tile;
			int heuristicDistance = Utils.GetHeuristicDistance(tile, tile2);
			int num = int.MaxValue;
			foreach (Tile item3 in list)
			{
				int heuristicDistance2 = Utils.GetHeuristicDistance(tile2, item3);
				if (heuristicDistance2 < num)
				{
					num = heuristicDistance2;
				}
			}
			if ((long)heuristicDistance + (long)num > currAutonomy)
			{
				item2.Value.GetComponent<SpriteRenderer>().color = color;
			}
		}
	}

	public void DestroyPotentialTilesAboveMap()
	{
		if (listOfPotentialTilesAboveTheMap.Count == 0)
		{
			return;
		}
		Transform[] componentsInChildren = potentialTiles_parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != potentialTiles_parent)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		listOfPotentialTilesAboveTheMap.Clear();
	}

	public void ClearCommanderTiles()
	{
		Transform[] componentsInChildren = commandersParent.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (i > 0)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
	}

	public void UnselectUnit()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (selectedUnitIcon.activeSelf)
		{
			LeanTween.cancel(selectedUnitIcon);
			LeanTween.scale(selectedUnitIcon, selectedUnitIconOriginalScaleBIG, 0.1f).setEaseInQuart().setOnComplete(() =>
			{
				selectedUnitIcon.SetActive(value: false);
			});
			Color color = selectedUnitIcon.GetComponent<SpriteRenderer>().color;
			color.a = 0f;
			LeanTween.color(selectedUnitIcon, color, 0.06f).setEaseInQuart();
		}
		if (ReservePanelGO.selectedUnit != null)
		{
			RemoveFogOfWarAroundVictoryPoints();
			ReservePanelGO.selectedUnit = null;
		}
		ClearTargetedUnits();
		DestroyPotentialTilesAboveMap();
		instance.panelAllStatsSelectedUnit.SetActive(value: false);
		instance.panelAllStatsOtherUnit.SetActive(value: false);
		instance.leftSideUI_Panel.SetActive(value: false);
		UIManager.instance.DeleteAllWaypoints();
		if (!(selectedUnitGO == null) && !selectedUnitGO.isMoving)
		{
			if (selectedUnitGO != null)
			{
				Object.Destroy(UIManager.instance.unitMovePath);
				Object.Destroy(UIManager.instance.supplyLine);
				selectedUnitGO.canUndo = false;
				selectedUnitGO.RemoveFogOfWarAroundUnit();
				selectedUnitGO = null;
			}
			UIManager.instance.resupplyCost_Panel.SetActive(value: false);
		}
	}

	public static bool IsThereACertainTileAround(TileGO p_tileGO, string p_type, bool p_compareToGivenPlayer = false)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tileGO.tile, 1))
		{
			switch (p_type)
			{
			case "Railroad":
				if (item.Railroad)
				{
					return true;
				}
				break;
			case "Reinforcement":
				if (!item.IsReinforcementTile || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "Airport":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.AIRPORT || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "Factory":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.FACTORY || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "Harbour":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.HARBOUR || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "SmallHarbour":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.HARBOUR || !item.Name.ToLower().Contains("small") || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "BigHarbour":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.HARBOUR || item.Name.ToLower().Contains("small") || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "Village":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.CITY || !item.Name.ToLower().Contains("village") || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "Town":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.CITY || !item.Name.ToLower().Contains("town") || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			case "City":
				if (item.NumberOfTurnsAtLowProd != 0 || item.enumType != TileTypes.CITY || !item.Name.ToLower().Contains("city") || item.ownerPlayer == null)
				{
					break;
				}
				if (p_compareToGivenPlayer)
				{
					if (item.ownerPlayer.isAlliedWithGivenPlayer)
					{
						return true;
					}
				}
				else if (item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public static void ClearTargetedUnits()
	{
		listOfTargetedUnit.Clear();
		TargetGO[] componentsInChildren = instance.targetsParent.GetComponentsInChildren<TargetGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	public static void CheckIfEndOfScenario()
	{
		if (GameData.Instance.gameFinishedButContinue)
		{
			return;
		}
		GameOutcomes gameOutcomes = GameOutcomes.NONE;
		if ((CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP || CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP) && HasReachedTurnLimit() == GameStates.NOT_SETUP && CheckEndOfScenarioHoldingVPs() == GameStates.NOT_SETUP && CheckEndOfScenarioTargetsUnits() == GameStates.NOT_SETUP)
		{
			if (CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllMajorVPs());
			}
			else if (CheckEndOfScenarioHoldingAllVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllVPs());
			}
		}
		else if (HasReachedTurnLimit() != GameStates.NOT_SETUP && HasReachedMaxDamageToDeal() != GameStates.NOT_SETUP)
		{
			if (HasReachedMaxDamageToDeal() != GameStates.ONGOING)
			{
				gameOutcomes = ConvertGameStateInGameResult(HasReachedMaxDamageToDeal());
			}
		}
		else if (HasReachedTurnLimit() != GameStates.NOT_SETUP && HasReachedMaxTilesToBomb() != GameStates.NOT_SETUP)
		{
			if (HasReachedMaxTilesToBomb() != GameStates.ONGOING)
			{
				gameOutcomes = ConvertGameStateInGameResult(HasReachedMaxTilesToBomb());
			}
		}
		else if ((CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP || CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP) && HasReachedTurnLimit() != GameStates.NOT_SETUP && CheckEndOfScenarioHoldingVPs() == GameStates.NOT_SETUP && CheckEndOfScenarioTargetsUnits() == GameStates.NOT_SETUP)
		{
			if (CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllMajorVPs());
			}
			else if (CheckEndOfScenarioHoldingAllVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllVPs());
			}
		}
		else if ((CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP || CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP) && HasReachedTurnLimit() != GameStates.NOT_SETUP && CheckEndOfScenarioHoldingVPs() != GameStates.NOT_SETUP && CheckEndOfScenarioTargetsUnits() == GameStates.NOT_SETUP)
		{
			if (HasReachedTurnLimit() == GameStates.TURN_LIMIT_REACHED)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingVPs());
			}
			else if (CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllMajorVPs());
			}
			else if (CheckEndOfScenarioHoldingAllVPs() != GameStates.ONGOING && CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllVPs());
			}
		}
		else if (CheckEndOfScenarioTargetsUnits() != GameStates.NOT_SETUP && HasReachedTurnLimit() == GameStates.NOT_SETUP && CheckEndOfScenarioHoldingVPs() == GameStates.NOT_SETUP && (CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP || CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP))
		{
			if (CheckEndOfScenarioTargetsUnits() != GameStates.ONGOING)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioTargetsUnits());
			}
			else if (CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP && CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.ONGOING)
			{
				string text = "";
				foreach (TileGO victoryPoint in victoryPoints)
				{
					if (victoryPoint.tile.PrimaryObjective)
					{
						text = victoryPoint.tile.ownerPlayer.Faction;
						break;
					}
				}
				if (text == victoryPoints.ElementAt(0).tile.ownerPlayer.Faction)
				{
					gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllMajorVPs());
				}
			}
			else if (CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP && CheckEndOfScenarioHoldingAllVPs() != GameStates.ONGOING && victoryPoints.ElementAt(0).tile.ownerPlayer.Faction == GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ElementAt(0).Key.OwnerPlayer.Faction)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingAllVPs());
			}
		}
		else if (CheckEndOfScenarioHoldingVPs() != GameStates.NOT_SETUP && HasReachedTurnLimit() != GameStates.NOT_SETUP && CheckEndOfScenarioTargetsUnits() == GameStates.NOT_SETUP)
		{
			if (HasReachedTurnLimit() != GameStates.ONGOING)
			{
				gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingVPs());
			}
		}
		else if (CheckEndOfScenarioTargetsUnits() != GameStates.NOT_SETUP && HasReachedTurnLimit() != GameStates.NOT_SETUP && CheckEndOfScenarioHoldingVPs() == GameStates.NOT_SETUP)
		{
			if (HasReachedTurnLimit() == GameStates.TURN_LIMIT_REACHED)
			{
				gameOutcomes = ((CheckEndOfScenarioTargetsUnits() != GameStates.ONGOING) ? ConvertGameStateInGameResult(CheckEndOfScenarioTargetsUnits()) : ((!(TurnManager.currPlayer.Faction == GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ElementAt(0).Key.OwnerPlayer.Faction)) ? GameOutcomes.VICTORY : GameOutcomes.DEFEAT));
			}
			else if (CheckEndOfScenarioTargetsUnits() != GameStates.ONGOING)
			{
				gameOutcomes = ((TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction != GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ElementAt(0).Key.OwnerPlayer.Faction) ? GameOutcomes.VICTORY : GameOutcomes.DEFEAT);
			}
		}
		else if (HasReachedTurnLimit() != GameStates.NOT_SETUP && HasReachedTurnLimit() != GameStates.ONGOING)
		{
			gameOutcomes = ConvertGameStateInGameResult(CheckEndOfScenarioHoldingVPs());
		}
		if (gameOutcomes != GameOutcomes.NONE)
		{
			UIManager.instance.EndTheGame(gameOutcomes);
		}
	}

	public static GameOutcomes ConvertGameStateInGameResult(GameStates p_gameState)
	{
		switch (p_gameState)
		{
		case GameStates.VICTORY:
			return GameOutcomes.VICTORY;
		case GameStates.DEFEAT:
			return GameOutcomes.DEFEAT;
		default:
			UnityEngine.Debug.LogError("Cannot convert game state in game result: " + p_gameState);
			return GameOutcomes.DEFEAT;
		}
	}

	public static GameStates HasReachedTurnLimit()
	{
		if (GameData.Instance.map.NumberOfTurnsToWin == 0)
		{
			return GameStates.NOT_SETUP;
		}
		if (GameData.Instance.map.turnCount <= GameData.Instance.map.NumberOfTurnsToWin)
		{
			return GameStates.ONGOING;
		}
		return GameStates.TURN_LIMIT_REACHED;
	}

	public static GameStates HasReachedMaxDamageToDeal()
	{
		if (GameData.Instance.map.NumberOfDamageToWin == 0)
		{
			return GameStates.NOT_SETUP;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.TotalDamageDealt >= (float)GameData.Instance.map.NumberOfDamageToWin)
			{
				if (!(GameData.Instance.map.DamageToWinFaction == "Both") && !(GameData.Instance.map.DamageToWinFaction == listOfPlayer.Faction))
				{
					continue;
				}
				if (IsAIOnly)
				{
					return GameStates.VICTORY;
				}
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer) && o_localPlayer.isAlliedWithCurrentPlayer)
					{
						return GameStates.VICTORY;
					}
					return GameStates.DEFEAT;
				}
				if (TurnManager.currPlayer.Faction == TurnManager.humanPlayer.Faction)
				{
					return GameStates.VICTORY;
				}
				return GameStates.DEFEAT;
			}
			if (HasReachedTurnLimit() != GameStates.TURN_LIMIT_REACHED)
			{
				continue;
			}
			if (GameData.Instance.map.DamageToWinFaction == "Both")
			{
				return GameStates.DEFEAT;
			}
			if (GameData.Instance.map.DamageToWinFaction == listOfPlayer.Faction)
			{
				if (IsAIOnly)
				{
					return GameStates.DEFEAT;
				}
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer2) && o_localPlayer2.IsAlliedWith(listOfPlayer))
					{
						return GameStates.DEFEAT;
					}
					return GameStates.VICTORY;
				}
				if (listOfPlayer.Faction == TurnManager.humanPlayer.Faction)
				{
					return GameStates.DEFEAT;
				}
				return GameStates.VICTORY;
			}
			if (IsAIOnly)
			{
				return GameStates.VICTORY;
			}
			if (MultiplayerManager.IsRealtimeMultiplayer())
			{
				if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer3) && o_localPlayer3.IsAlliedWith(listOfPlayer))
				{
					return GameStates.VICTORY;
				}
				return GameStates.DEFEAT;
			}
			if (listOfPlayer.Faction == TurnManager.humanPlayer.Faction)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		return GameStates.ONGOING;
	}

	public static GameStates HasReachedMaxTilesToBomb()
	{
		if (GameData.Instance.map.NumberOfTilesToBombToWin == 0)
		{
			return GameStates.NOT_SETUP;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.TotalTilesBombed >= (float)GameData.Instance.map.NumberOfTilesToBombToWin)
			{
				if (!(GameData.Instance.map.TilesToBombFaction == "Both") && !(GameData.Instance.map.TilesToBombFaction == listOfPlayer.Faction))
				{
					continue;
				}
				if (IsAIOnly)
				{
					return GameStates.VICTORY;
				}
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer) && o_localPlayer.isAlliedWithCurrentPlayer)
					{
						return GameStates.VICTORY;
					}
					return GameStates.DEFEAT;
				}
				if (TurnManager.currPlayer.Faction == TurnManager.humanPlayer.Faction)
				{
					return GameStates.VICTORY;
				}
				return GameStates.DEFEAT;
			}
			if (HasReachedTurnLimit() != GameStates.TURN_LIMIT_REACHED)
			{
				continue;
			}
			if (GameData.Instance.map.TilesToBombFaction == "Both")
			{
				return GameStates.DEFEAT;
			}
			if (GameData.Instance.map.TilesToBombFaction == listOfPlayer.Faction)
			{
				if (IsAIOnly)
				{
					return GameStates.DEFEAT;
				}
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer2) && o_localPlayer2.IsAlliedWith(listOfPlayer))
					{
						return GameStates.DEFEAT;
					}
					return GameStates.VICTORY;
				}
				if (listOfPlayer.Faction == TurnManager.humanPlayer.Faction)
				{
					return GameStates.DEFEAT;
				}
				return GameStates.VICTORY;
			}
			if (IsAIOnly)
			{
				return GameStates.VICTORY;
			}
			if (MultiplayerManager.IsRealtimeMultiplayer())
			{
				if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer3) && o_localPlayer3.IsAlliedWith(listOfPlayer))
				{
					return GameStates.VICTORY;
				}
				return GameStates.DEFEAT;
			}
			if (listOfPlayer.Faction == TurnManager.humanPlayer.Faction)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		return GameStates.ONGOING;
	}

	public static GameStates CheckEndOfScenarioHoldingAllVPs()
	{
		if (victoryPoints.Count == 0)
		{
			return GameStates.NOT_SETUP;
		}
		string faction = victoryPoints.ElementAt(0).tile.ownerPlayer.Faction;
		bool flag = true;
		foreach (TileGO victoryPoint in victoryPoints)
		{
			if (victoryPoint.tile.ownerPlayer != null && (victoryPoint.tile.ownerPlayer.Faction != faction || victoryPoint.tile.ownerPlayer.Faction == "Neutral"))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (TurnManager.humanPlayer != null && faction == TurnManager.humanPlayer.Faction)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		return GameStates.ONGOING;
	}

	public static void RecountCapturedMajorVPs()
	{
		numberOfCapturedMajorVPByFaction = 0;
		Player currPlayer = TurnManager.currPlayer;
		foreach (TileGO victoryPoint in victoryPoints)
		{
			if (victoryPoint.tile.IsVictoryPoint && victoryPoint.tile.PrimaryObjective && victoryPoint.tile.ownerPlayer != null && (!(currPlayer.Faction == "Neutral") || victoryPoint.tile.ownerPlayer == currPlayer) && victoryPoint.tile.ownerPlayer.Faction == currPlayer.Faction)
			{
				numberOfCapturedMajorVPByFaction++;
			}
		}
	}

	public static GameStates CheckEndOfScenarioHoldingAllMajorVPs()
	{
		if (majorVPsAmount == 0)
		{
			return GameStates.NOT_SETUP;
		}
		if (numberOfCapturedMajorVPByFaction == majorVPsAmount)
		{
			if (TurnManager.currPlayer.Faction == "Neutral")
			{
				return GameStates.ONGOING;
			}
			if (TurnManager.humanPlayer != null && TurnManager.currPlayer.Faction == TurnManager.humanPlayer.Faction)
			{
				return GameStates.VICTORY;
			}
			if (IsAIOnly)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		return GameStates.ONGOING;
	}

	public static GameStates CheckEndOfScenarioHoldingVPs()
	{
		if (GameData.Instance.map.listOfVPThatNeedsToBeHeld.Count == 0)
		{
			return GameStates.NOT_SETUP;
		}
		if (GameData.Instance.map.turnCount <= GameData.Instance.map.NumberOfTurnsToWin)
		{
			return GameStates.ONGOING;
		}
		bool flag = true;
		for (int i = 0; i < GameData.Instance.map.listOfVPThatNeedsToBeHeld.Count; i++)
		{
			if (GameData.Instance.map.TilesTable[GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(i).Key.PosX, GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(i).Key.PosY].ownerPlayer.Faction != GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(i).Value)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction == GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(0).Key.ownerPlayer.Faction)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		string text = "";
		foreach (KeyValuePair<Tile, string> item in GameData.Instance.map.listOfVPThatNeedsToBeHeld)
		{
			if (item.Value != item.Key.ownerPlayer.Faction)
			{
				text = item.Key.ownerPlayer.Faction;
				break;
			}
		}
		if (TurnManager.humanPlayer == null)
		{
			if (TurnManager.currPlayer.Faction == text)
			{
				return GameStates.VICTORY;
			}
			return GameStates.DEFEAT;
		}
		if (TurnManager.humanPlayer.Faction == TurnManager.currPlayer.Faction && TurnManager.currPlayer.Faction == text)
		{
			return GameStates.VICTORY;
		}
		return GameStates.DEFEAT;
	}

	public static GameStates CheckEndOfScenarioTargetsUnits()
	{
		if (GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Count() == 0)
		{
			return GameStates.NOT_SETUP;
		}
		foreach (KeyValuePair<Unit, bool> item in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
		{
			if (item.Key.OwnerPlayer != null)
			{
				continue;
			}
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (item.Key.OwnerName == listOfPlayer.Name)
				{
					item.Key.OwnerPlayer = listOfPlayer;
				}
			}
		}
		bool flag = false;
		string text = "";
		string text2 = "";
		string text3 = "";
		foreach (KeyValuePair<Unit, bool> item2 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
		{
			if (text != "" && text != item2.Key.OwnerPlayer.Faction)
			{
				text2 = text;
				text3 = item2.Key.OwnerPlayer.Faction;
				flag = true;
			}
			text = item2.Key.OwnerPlayer.Faction;
		}
		if (flag)
		{
			bool flag2 = true;
			bool flag3 = true;
			foreach (KeyValuePair<Unit, bool> item3 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
			{
				if (!item3.Value)
				{
					if (item3.Key.OwnerPlayer.Faction == text2)
					{
						flag2 = false;
					}
					else
					{
						flag3 = false;
					}
				}
			}
			if (flag2 | flag3)
			{
				if (flag2)
				{
					if (TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction == text2)
					{
						return GameStates.DEFEAT;
					}
					return GameStates.VICTORY;
				}
				if (TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction == text3)
				{
					return GameStates.DEFEAT;
				}
				return GameStates.VICTORY;
			}
			return GameStates.ONGOING;
		}
		bool flag4 = true;
		foreach (KeyValuePair<Unit, bool> item4 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
		{
			if (!item4.Value)
			{
				flag4 = false;
				break;
			}
		}
		if (flag4)
		{
			string faction = GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ElementAt(0).Key.OwnerPlayer.Faction;
			if (TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction == faction)
			{
				return GameStates.DEFEAT;
			}
			return GameStates.VICTORY;
		}
		return GameStates.ONGOING;
	}

	private void RespawnPlayerUnits(TileGO p_tileGO, Unit p_unit)
	{
		if (p_unit.OwnerPlayer == null)
		{
			if (GameData.Instance.TryFindPlayerByName(p_unit.OwnerName, out var o_player))
			{
				p_unit.OwnerPlayer = o_player;
				if (p_unit.Type == "Plane")
				{
					p_tileGO.airUnitGO = null;
					p_tileGO.tile.AirUnit = null;
				}
				else
				{
					p_tileGO.groundUnitGO = null;
					p_tileGO.tile.GroundUnit = null;
				}
				p_unit.IsReserve = true;
				p_unit.SetRedeploymentTime();
			}
			return;
		}
		UnitGO unitGO = (p_unit.IsCoreUnit ? Object.Instantiate(unitCorePrefab, unitsParent) : Object.Instantiate(unitPrefab, unitsParent));
		if (p_unit.Type == "Plane")
		{
			p_tileGO.airUnitGO = unitGO;
		}
		else
		{
			p_tileGO.groundUnitGO = unitGO;
			if (p_unit.MaxMP > 0)
			{
				if (GameData.Instance.map.turnCount == 1 && !MainMenu.isLoadedGame && (p_tileGO.tile.IsVictoryPoint || p_tileGO.tile.enumType == TileTypes.TRENCH))
				{
					p_unit.AsleepState = AsleepStates.SLEEP;
				}
			}
			else
			{
				p_unit.AsleepState = AsleepStates.SLEEP;
			}
		}
		p_unit.unitGO = unitGO;
		unitGO.unit = p_unit;
		unitGO.tileGO = p_tileGO;
		unitGO.transform.position = new Vector3(p_tileGO.transform.position.x, p_tileGO.transform.position.y, -1f);
		unitGO.name = p_unit.ID.ToString();
		if (p_unit.IsCommander)
		{
			listOfAllGeneralsOnTheMap.Add(unitGO);
		}
		Utils.UpdateUnitStatsOnTurnOne(p_unit, listOfAllUnits);
		if (p_tileGO.tile.enumType == TileTypes.SEA && !p_tileGO.tile.Bridge && !p_tileGO.tile.isFrozen && unitGO.unit.Type != "Boat" && unitGO.unit.Type != "Plane")
		{
			unitGO.unit.IsOnBoat = true;
			unitGO.unit.Type = "Boat";
			Utils.UpdateUnitStatsOnTurnOne(p_unit, listOfAllUnits);
		}
		p_unit.SetDefaultAuthorizedRotation();
		unitGO.SetSprite();
		unitGO.SetUnitRotation();
		if (!unitGO.unit.OwnerPlayer.IsComputer && unitGO.unit.OwnerPlayer.IsPlaying && (!MultiplayerManager.IsRealtimeMultiplayer() || TurnManager.currPlayer.IsLocalClientPlayer()))
		{
			unitGO.subContainer_GO.SetActive(value: true);
		}
	}

	public static void PlaceFogOfWarEverywhere()
	{
		GameData.Instance.map.majorVPsPerFaction = new Dictionary<string, int>();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		if (PlayerSettings.Instance.IsFogOfWar)
		{
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					tilesTable[i, j].isVisible = false;
					tilesTable[i, j].tileGO.SetFogOfWar();
				}
			}
		}
		else
		{
			for (int k = 0; k < sizeX; k++)
			{
				for (int l = 0; l < sizeY; l++)
				{
					tilesTable[k, l].isVisible = true;
				}
			}
		}
		CheckIfEndOfScenario();
	}

	public static void RemoveFogOfWarAroundUnits()
	{
		Player player = ((!IsAIOnly && TurnManager.humanPlayer != null) ? TurnManager.humanPlayer : TurnManager.currPlayer);
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				TileGO tileGO = tilesTable[i, j].tileGO;
				if (tileGO.airUnitGO == null && tileGO.groundUnitGO == null)
				{
					continue;
				}
				if (tileGO.airUnitGO != null && tileGO.groundUnitGO == null)
				{
					if (tileGO.airUnitGO.unit.OwnerPlayer.Faction == player.Faction)
					{
						tileGO.airUnitGO.gameObject.SetActive(value: true);
					}
					tileGO.airUnitGO.RemoveFogOfWarAroundUnit();
					continue;
				}
				if (tileGO.airUnitGO == null && tileGO.groundUnitGO != null)
				{
					if (IsAIOnly || tileGO.groundUnitGO.unit.OwnerPlayer.Faction == player.Faction)
					{
						tileGO.groundUnitGO.gameObject.SetActive(value: true);
					}
					tileGO.groundUnitGO.RemoveFogOfWarAroundUnit();
					continue;
				}
				if (tileGO.airUnitGO.unit.OwnerPlayer.Faction == player.Faction)
				{
					tileGO.airUnitGO.gameObject.SetActive(value: true);
					tileGO.groundUnitGO.gameObject.SetActive(value: false);
				}
				else
				{
					tileGO.airUnitGO.gameObject.SetActive(value: false);
					tileGO.groundUnitGO.gameObject.SetActive(value: true);
				}
				tileGO.airUnitGO.RemoveFogOfWarAroundUnit();
				tileGO.groundUnitGO.RemoveFogOfWarAroundUnit();
			}
		}
	}

	public static void RemoveFogOfWarAroundVictoryPoints()
	{
		RefreshAlliancesAndWarsForCurrentPlayers();
		foreach (TileGO victoryPoint in victoryPoints)
		{
			if (victoryPoint.tile.ownerPlayer == null)
			{
				continue;
			}
			bool isAlliedWithCurrentPlayer = victoryPoint.tile.ownerPlayer.isAlliedWithCurrentPlayer;
			bool flag = victoryPoint.tile.ownerPlayer.isAlliedWithHumanPlayer || (IsAIOnly & isAlliedWithCurrentPlayer);
			if (!flag && !isAlliedWithCurrentPlayer)
			{
				continue;
			}
			bool flag2 = !victoryPoint.tile.isNavalConvoyRoute;
			if (flag)
			{
				victoryPoint.RemoveFogOfWar();
			}
			if (isAlliedWithCurrentPlayer)
			{
				victoryPoint.tile.isVisible = true;
			}
			if (!flag2)
			{
				continue;
			}
			foreach (Tile neighbour in victoryPoint.tile.neighbours)
			{
				if (isAlliedWithCurrentPlayer)
				{
					neighbour.isVisible = true;
				}
				if (flag)
				{
					neighbour.tileGO.CopyTileGOFogOfWarSetting(victoryPoint);
				}
			}
		}
	}

	public static void ProcessFogOfWar(bool p_refreshTerrainOwnershipOnly = false)
	{
		int num = 0;
		Queue<Tile> queue = new Queue<Tile>();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				tilesTable[i, j].OnQueue = false;
				if (tilesTable[i, j].IsVictoryPoint && tilesTable[i, j].ownerPlayer != null)
				{
					queue.Enqueue(tilesTable[i, j]);
					tilesTable[i, j].OnQueue = true;
					num++;
				}
			}
		}
		RefreshAlliancesAndWarsForCurrentPlayers();
		bool stricterFogOfWar = PlayerSettings.Instance.StricterFogOfWar;
		bool flag = TurnManager.humanPlayer == null;
		Player currPlayer = TurnManager.currPlayer;
		Player player = null;
		if (!flag)
		{
			player = TurnManager.humanPlayer;
		}
		while (num > 0)
		{
			Tile tile = queue.Dequeue();
			num--;
			if (!stricterFogOfWar && !p_refreshTerrainOwnershipOnly)
			{
				if (flag)
				{
					if (tile.terrainOwner.isAlliedWithCurrentPlayer)
					{
						tile.tileGO.RemoveFogOfWar();
						tile.isVisible = true;
					}
				}
				else if (currPlayer == player)
				{
					if (tile.terrainOwner.isAlliedWithCurrentPlayer)
					{
						tile.tileGO.RemoveFogOfWar();
						tile.isVisible = true;
					}
				}
				else
				{
					if (tile.terrainOwner.isAlliedWithHumanPlayer)
					{
						tile.tileGO.RemoveFogOfWar();
					}
					if (tile.terrainOwner.isAlliedWithCurrentPlayer)
					{
						tile.isVisible = true;
					}
				}
			}
			if (tile.IsVictoryPoint && tile.ownerPlayer != null)
			{
				tile.isOccupied = tile.ownerPlayer.Country != tile.OriginalCountryOwner;
			}
			for (byte b = 0; b < tile.amountOfNeighbours; b++)
			{
				Tile tile2 = tile.neighbours[b];
				if (!tile2.OnQueue && (tile2.enumType != TileTypes.SEA || tile2.isLakeTile))
				{
					if (tile.ownerPlayer != null)
					{
						tile2.terrainOwner = tile.ownerPlayer;
						tile2.isOccupied = tile.ownerPlayer.Country != tile.OriginalCountryOwner;
					}
					else
					{
						tile2.terrainOwner = tile.terrainOwner;
						tile2.isOccupied = tile.isOccupied;
					}
					if (!stricterFogOfWar && !p_refreshTerrainOwnershipOnly)
					{
						if (flag)
						{
							if (tile.terrainOwner.isAlliedWithCurrentPlayer)
							{
								tile2.tileGO.RemoveFogOfWar();
								tile2.isVisible = true;
							}
						}
						else if (currPlayer == player)
						{
							if (tile.terrainOwner.isAlliedWithCurrentPlayer)
							{
								tile2.tileGO.RemoveFogOfWar();
								tile2.isVisible = true;
							}
						}
						else
						{
							if (tile.terrainOwner.isAlliedWithHumanPlayer)
							{
								tile2.tileGO.RemoveFogOfWar();
							}
							if (tile.terrainOwner.isAlliedWithCurrentPlayer)
							{
								tile2.isVisible = true;
							}
						}
					}
					queue.Enqueue(tile2);
					tile2.OnQueue = true;
					num++;
				}
			}
		}
	}

	public static void RemovePlayerSavedFOW()
	{
		List<Tile.Coordinates> list = TurnManager.currPlayer?.ClearedFOW;
		if (list == null || list.Count == 0)
		{
			return;
		}
		Map map = GameData.Instance.map;
		Tile[,] tilesTable = map.TilesTable;
		Player humanPlayer = TurnManager.humanPlayer;
		foreach (Tile.Coordinates item in list)
		{
			if (item.X >= map.SizeX || item.Y >= map.SizeY)
			{
				continue;
			}
			Tile tile = tilesTable[item.X, item.Y];
			tile.isVisible = true;
			if (humanPlayer != null && !humanPlayer.isAlliedWithCurrentPlayer)
			{
				continue;
			}
			tile.tileGO.RemoveFogOfWar();
			radarDetections.Remove(tile.tileGO);
			sonarDetections.Remove(tile.tileGO);
			if (tile.tileGO.airUnitGO != null && tile.tileGO.groundUnitGO != null)
			{
				if (UIManager.showAirUnit)
				{
					tile.tileGO.airUnitGO.gameObject.SetActive(value: true);
					tile.tileGO.groundUnitGO.gameObject.SetActive(value: false);
				}
				else
				{
					tile.tileGO.groundUnitGO.gameObject.SetActive(value: true);
					tile.tileGO.airUnitGO.gameObject.SetActive(value: false);
				}
			}
			else if (tile.tileGO.airUnitGO != null)
			{
				tile.tileGO.airUnitGO.gameObject.SetActive(value: true);
			}
			else if (tile.tileGO.groundUnitGO != null)
			{
				tile.tileGO.groundUnitGO.gameObject.SetActive(value: true);
			}
			if (tile.tileGO.airUnitGO != null)
			{
				tile.tileGO.airUnitGO.UpdateCounter();
			}
			if (tile.tileGO.groundUnitGO != null)
			{
				tile.tileGO.groundUnitGO.UpdateCounter();
			}
		}
	}

	public void DrawBordersOnMap()
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
		bool showBorders = PlayerSettings.Instance.ShowBorders;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (dictionary.ContainsKey(listOfPlayer.Country))
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
				LineRenderer lineRenderer = tile.tileGO.lineRenderer;
				if (!showBorders)
				{
					lineRenderer.enabled = false;
					continue;
				}
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
				string country = tilesTable[i, j].terrainOwner.Country;
				lineRenderer.startColor = dictionary[country];
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
		RefreshPoliticalMap(p_overwriteBeingDisabled: false);
		if (SupplyMapOverlay.instance != null)
		{
			SupplyMapOverlay.instance.RefreshOverlay(TurnManager.currPlayer);
		}
	}

	private void SetupDiplomacyOnMapStarts()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Truces == null)
			{
				listOfPlayer.Truces = new Dictionary<string, int>();
			}
			if (listOfPlayer.PlayersAtWar != null)
			{
				continue;
			}
			listOfPlayer.PlayersAtWar = new HashSet<string>();
			if (listOfPlayer.AlliedPlayers != null)
			{
				continue;
			}
			listOfPlayer.AlliedPlayers = new HashSet<string>();
			foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.Name == listOfPlayer2.Name)
				{
					listOfPlayer.AlliedPlayers.Add(listOfPlayer2.Name);
				}
				else if (listOfPlayer.Country == "Neutral")
				{
					if (listOfPlayer2.Faction != "Neutral")
					{
						listOfPlayer.PlayersAtWar.Add(listOfPlayer2.Name);
					}
				}
				else if (listOfPlayer2.Country == "Neutral")
				{
					if (listOfPlayer.Faction != "Neutral")
					{
						listOfPlayer.PlayersAtWar.Add(listOfPlayer2.Name);
					}
				}
				else if (listOfPlayer.Faction != "Neutral" && listOfPlayer2.Faction != "Neutral")
				{
					if (listOfPlayer.Faction == listOfPlayer2.Faction)
					{
						listOfPlayer.AlliedPlayers.Add(listOfPlayer2.Name);
					}
					else
					{
						listOfPlayer.PlayersAtWar.Add(listOfPlayer2.Name);
					}
				}
			}
		}
	}

	public static void RefreshAlliancesAndWarsForCurrentPlayers()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.isAtWarWithCurrentPlayer = TurnManager.currPlayer.IsAtWarWith(listOfPlayer);
			listOfPlayer.isAlliedWithCurrentPlayer = TurnManager.currPlayer.IsAlliedWith(listOfPlayer);
			listOfPlayer.noRelationWithCurrentPlayer = !listOfPlayer.isAlliedWithCurrentPlayer && !listOfPlayer.isAtWarWithCurrentPlayer;
			if (TurnManager.humanPlayer != null)
			{
				listOfPlayer.isAtWarWithHumanPlayer = TurnManager.humanPlayer.IsAtWarWith(listOfPlayer);
				listOfPlayer.isAlliedWithHumanPlayer = TurnManager.humanPlayer.IsAlliedWith(listOfPlayer);
				listOfPlayer.noRelationWithHumanPlayer = !listOfPlayer.isAlliedWithHumanPlayer && !listOfPlayer.isAtWarWithHumanPlayer;
			}
		}
	}

	private IEnumerator CR_MakeUnitsFaceRightDirection()
	{
		yield return null;
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tiles = GameData.Instance.map.TilesTable;
		int[,] visited = new int[sizeX, sizeY];
		int currentVisitId = 0;
		Queue<Tile> tileQueue = new Queue<Tile>(128);
		Queue<int> depthQueue = new Queue<int>(128);
		int unitsProcessedThisFrame = 0;
		for (short x = 0; x < sizeX; x++)
		{
			for (short y = 0; y < sizeY; y++)
			{
				Tile tile = tiles[x, y];
				if (tile.GroundUnit != null || tile.AirUnit != null)
				{
					currentVisitId++;
					tileQueue.Clear();
					depthQueue.Clear();
					tileQueue.Enqueue(tile);
					depthQueue.Enqueue(0);
					visited[x, y] = currentVisitId;
					float num = 0f;
					float num2 = 0f;
					float num3 = 0f;
					float num4 = 0f;
					float num5 = 0f;
					float num6 = 0f;
					Player player = tile.GroundUnit?.OwnerPlayer;
					Player player2 = tile.AirUnit?.OwnerPlayer;
					bool flag = player != null;
					bool flag2 = player2 != null;
					while (tileQueue.Count > 0)
					{
						Tile tile2 = tileQueue.Dequeue();
						int num7 = depthQueue.Dequeue();
						if (num7 > 0 && tile2.terrainOwner != null)
						{
							float num8 = 1f / (float)((num7 + 1) * (num7 + 1));
							if (flag && tile2.terrainOwner != player && !player.IsAlliedWith(tile2.terrainOwner))
							{
								num += num8;
								num2 += tile2.tileGO.transform.position.x * num8;
								num3 += tile2.tileGO.transform.position.y * num8;
							}
							if (flag2 && tile2.terrainOwner != player2 && !player2.IsAlliedWith(tile2.terrainOwner))
							{
								num4 += num8;
								num5 += tile2.tileGO.transform.position.x * num8;
								num6 += tile2.tileGO.transform.position.y * num8;
							}
						}
						if (num7 >= 15)
						{
							continue;
						}
						for (int i = 0; i < tile2.neighbours.Count; i++)
						{
							Tile tile3 = tile2.neighbours[i];
							if (visited[tile3.PosX, tile3.PosY] != currentVisitId)
							{
								visited[tile3.PosX, tile3.PosY] = currentVisitId;
								tileQueue.Enqueue(tile3);
								depthQueue.Enqueue(num7 + 1);
							}
						}
					}
					Vector3 position = tile.tileGO.transform.position;
					if (flag && num > 0f && !UIManager.isNATO)
					{
						float num9 = num2 / num - position.x;
						float num10 = num3 / num - position.y;
						UnitGO unitGO = tile.GroundUnit.unitGO;
						if (unitGO.unit.AllowedRotation == AllowedRotations.LEFT_RIGHT)
						{
							if (num9 > 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.EAST;
							}
							else if (num9 < 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.WEST;
							}
							unitGO.SetUnitRotation();
						}
						else if (unitGO.unit.AllowedRotation == AllowedRotations.ALL)
						{
							if (num9 > 0f && num10 >= 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.NORTH_EAST;
							}
							else if (num9 > 0f && num10 < 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.SOUTH_EAST;
							}
							else if (num9 < 0f && num10 >= 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.NORTH_WEST;
							}
							else if (num9 < 0f && num10 < 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.SOUTH_WEST;
							}
							else if (num10 > 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.NORTH;
							}
							else if (num10 < 0f)
							{
								unitGO.unit.FacingRotation = FacingRotations.SOUTH;
							}
							unitGO.SetUnitRotation();
						}
					}
					if (flag2 && num4 > 0f && !UIManager.isNATO)
					{
						float num11 = num5 / num4 - position.x;
						float num12 = num6 / num4 - position.y;
						UnitGO unitGO2 = tile.AirUnit.unitGO;
						if (unitGO2.unit.AllowedRotation == AllowedRotations.LEFT_RIGHT)
						{
							if (num11 > 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.EAST;
							}
							else if (num11 < 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.WEST;
							}
							unitGO2.SetUnitRotation();
						}
						else if (unitGO2.unit.AllowedRotation == AllowedRotations.ALL)
						{
							if (num11 > 0f && num12 >= 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.NORTH_EAST;
							}
							else if (num11 > 0f && num12 < 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.SOUTH_EAST;
							}
							else if (num11 < 0f && num12 >= 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.NORTH_WEST;
							}
							else if (num11 < 0f && num12 < 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.SOUTH_WEST;
							}
							else if (num12 > 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.NORTH;
							}
							else if (num12 < 0f)
							{
								unitGO2.unit.FacingRotation = FacingRotations.SOUTH;
							}
							unitGO2.SetUnitRotation();
						}
					}
					unitsProcessedThisFrame++;
					if (unitsProcessedThisFrame > 50)
					{
						unitsProcessedThisFrame = 0;
						yield return null;
					}
				}
			}
		}
	}

	private void InitializeAIHarboursValidity()
	{
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		int[,] array = new int[sizeX, sizeY];
		int num = 0;
		Queue<Tile> queue = new Queue<Tile>(1024);
		for (short num2 = 0; num2 < sizeX; num2++)
		{
			for (short num3 = 0; num3 < sizeY; num3++)
			{
				Tile tile = tilesTable[num2, num3];
				if (tile.IsVictoryPoint && !tile.isValidHarbourForAI && tile.enumType == TileTypes.HARBOUR && tile.ownerPlayer != null)
				{
					num++;
					queue.Clear();
					queue.Enqueue(tile);
					array[num2, num3] = num;
					while (queue.Count > 0)
					{
						Tile tile2 = queue.Dequeue();
						for (int i = 0; i < tile2.neighbours.Count; i++)
						{
							Tile tile3 = tile2.neighbours[i];
							if (tile3.IsVictoryPoint && tile3.OwnerName != tile.OwnerName)
							{
								tile.isValidHarbourForAI = true;
								if (tile3.enumType == TileTypes.HARBOUR || tile3.IsReinforcementTile)
								{
									tile3.isValidHarbourForAI = true;
								}
							}
							if (array[tile3.PosX, tile3.PosY] != num)
							{
								array[tile3.PosX, tile3.PosY] = num;
								if (tile3.enumType == TileTypes.SEA)
								{
									queue.Enqueue(tile3);
								}
							}
						}
					}
				}
			}
		}
	}

	private IEnumerator CR_GeneratePoliticalMap()
	{
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		int instantiationsPerFrame = Mathf.Max(500, sizeX * sizeY / 60);
		Tile[,] tiles = GameData.Instance.map.TilesTable;
		_politicalHexRenderers = new PoliticalHexRenderer[sizeX, sizeY];
		Vector3 hexPlacement = default;
		int instantiationsThisFrame = 0;
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				if (tiles[x, y].terrainOwner != null)
				{
					_politicalHexRenderers[x, y] = Object.Instantiate(_politicalHexRenderer_prefab, politicalMap_parent);
					if (x % 2 == 0)
					{
						hexPlacement.Set((float)x * 4.42f, (float)y * 5.1f + 2.55f, -0.95f);
					}
					else
					{
						hexPlacement.Set((float)x * 4.42f, (float)y * 5.1f, -0.95f);
					}
					_politicalHexRenderers[x, y].Place(hexPlacement);
					instantiationsThisFrame++;
					if (instantiationsThisFrame >= instantiationsPerFrame)
					{
						instantiationsThisFrame = 0;
						yield return null;
					}
				}
			}
		}
	}

	public void TogglePoliticalMap()
	{
		if (politicalMap_parent.gameObject.activeSelf)
		{
			politicalMap_parent.gameObject.SetActive(value: false);
			_politicalMapOpacityBox.SetActive(value: false);
			return;
		}
		if (SupplyMapOverlay.instance != null && SupplyMapOverlay.instance.IsVisible())
		{
			SupplyMapOverlay.instance.ToggleSupplyMap(TurnManager.currPlayer);
		}
		RefreshPoliticalMap(p_overwriteBeingDisabled: true);
		politicalMap_parent.gameObject.SetActive(value: true);
		_politicalMapOpacityBox.SetActive(value: true);
	}

	public void RefreshPoliticalMap(bool p_overwriteBeingDisabled)
	{
		if (!politicalMap_parent.gameObject.activeSelf && !p_overwriteBeingDisabled)
		{
			return;
		}
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
		Unit unit = new Unit
		{
			FilterType = "FilterBuilding"
		};
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!dictionary.ContainsKey(listOfPlayer.Country))
			{
				unit.Country = listOfPlayer.Country;
				Sprite unitBackground = Utils.GetUnitBackground(unit, null);
				Color value;
				if (!(unitBackground != null) || !(unitBackground.name != "Null"))
				{
					value = ((listOfPlayer.Country == "Neutral") ? Color.white : Color.magenta);
				}
				else
				{
					value = unitBackground.texture.GetPixel(unitBackground.texture.width / 2, unitBackground.texture.height / 2);
				}
				value.a = _politicalMapOpacity_slider.value;
				dictionary.Add(listOfPlayer.Country, value);
			}
		}
		if (_politicalHexRenderers != null)
		{
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					if (tilesTable[i, j].terrainOwner != null && _politicalHexRenderers[i, j] != null)
					{
						_politicalHexRenderers[i, j].SetSpriteAndColor(dictionary[tilesTable[i, j].terrainOwner.Country], tilesTable[i, j].isOccupied);
					}
				}
			}
		}
		GenerateCountryNames();
	}

	private void ClearCountryNames()
	{
		foreach (TMP_Text countryName in _countryNames)
		{
			Object.Destroy(countryName.gameObject);
		}
		_countryNames.Clear();
	}

	public void UpdateCountryNamesOpacity()
	{
		if (!politicalMap_parent.gameObject.activeSelf)
		{
			return;
		}
		float a = _countryNamesOpacity_curve.Evaluate(CameraGO.instance.cam.orthographicSize);
		foreach (TMP_Text countryName in _countryNames)
		{
			Color color = countryName.color;
			countryName.color = new Color(color.r, color.g, color.b, a);
		}
	}

	private void GenerateCountryNames()
	{
		ClearCountryNames();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				tilesTable[i, j].isPartOfArea = false;
			}
		}
		Dictionary<Player, List<PoliticalArea>> dictionary = new Dictionary<Player, List<PoliticalArea>>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!dictionary.ContainsKey(listOfPlayer))
			{
				dictionary.Add(listOfPlayer, new List<PoliticalArea>());
			}
		}
		bool circularMap = GameData.Instance.map.CircularMap;
		for (int k = 0; k < sizeX; k++)
		{
			for (int l = 0; l < sizeY; l++)
			{
				Tile tile = tilesTable[k, l];
				if (tile.terrainOwner == null)
				{
					continue;
				}
				Queue<Tile> queue = new Queue<Tile>();
				queue.Enqueue(tile);
				PoliticalArea item = default;
				while (queue.Count != 0)
				{
					foreach (Tile neighbour in queue.First().neighbours)
					{
						if (neighbour.isPartOfArea || neighbour.terrainOwner == null || neighbour.terrainOwner != tile.terrainOwner)
						{
							continue;
						}
						if (circularMap)
						{
							if (tile.PosX == sizeX - 1)
							{
								if (neighbour.PosX == 0)
								{
									continue;
								}
							}
							else if (tile.PosX == 0 && neighbour.PosX == sizeX - 1)
							{
								continue;
							}
						}
						item.tilesAmount++;
						item.sumOfAllTilesWorldPositions += neighbour.tileGO.transform.position;
						neighbour.isPartOfArea = true;
						queue.Enqueue(neighbour);
					}
					queue.Dequeue();
				}
				if (item.tilesAmount > 1)
				{
					dictionary[tile.terrainOwner].Add(item);
				}
			}
		}
		foreach (KeyValuePair<Player, List<PoliticalArea>> item2 in dictionary)
		{
			foreach (PoliticalArea item3 in item2.Value)
			{
				TMP_Text tMP_Text = Object.Instantiate(_politicalCountryName_textmesh, politicalMap_parent);
				tMP_Text.text = LocalizationManager.GetLocalizedCountry(item2.Key.Country.Replace(".", ""));
				tMP_Text.rectTransform.sizeDelta = new Vector2(_politicalNameSize_curve.Evaluate(item3.tilesAmount), 75f);
				Vector3 position = item3.sumOfAllTilesWorldPositions / item3.tilesAmount;
				position.z = -0.95f;
				tMP_Text.transform.position = position;
				_countryNames.Add(tMP_Text);
			}
		}
	}

	private void OnApplicationQuit()
	{
		GameData.RestorePlayerSettingsBackup();
		PlayerSettings.Save();
		if (SteamAPI.IsSteamRunning())
		{
			SteamFriends.ClearRichPresence();
		}
		SteamAPI.Shutdown();
	}
}
