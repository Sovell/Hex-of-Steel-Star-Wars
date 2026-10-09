using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
	private struct BorderDebugEntry
	{
		public Vector3 position;

		public Color color;

		public string label;
	}

	private class VPDangerData
	{
		public int dangerValue;

		public int enemyToMatch;

		public int matchingUnitsDeployed;
	}

	private class RetreatTile
	{
		public Tile tile;

		public int alliesAround;

		public int enemiesAround;

		public int terrainModifier;

		public float distanceToAverageEnemiesPosition;
	}

	private enum RunTimes
	{
		FIRST,
		SECOND
	}

	public static AI instance;

	public static bool isPlaying;

	public static Queue<AIWaitInstruction> AIWaitInstructions;

	public static Player playerNeedsToRedeployItsUnitsNow;

	[Header("Setup")]
	public Slider loadingBar;

	public GameObject AIplayingPanel;

	[SerializeField]
	private TextMeshProUGUI AIisPlaying_Text;

	[SerializeField]
	private ReservePanelGO reservePanel;

	[SerializeField]
	private GameObject rightUI_Panel;

	[SerializeField]
	private GameObject rightUI_PaneliPad;

	[SerializeField]
	private GameObject leftUI_Panel;

	[SerializeField]
	private AnimationCurve _purchase_curve;

	[Header("RUNTIME DEBUG INFOS")]
	[Header("Settings")]
	[SerializeField]
	private bool _fastAI;

	[SerializeField]
	private bool _fastAI_AlliesOnly;

	[SerializeField]
	private int _difficultyLevel;

	[SerializeField]
	private bool _isHistoricalFocusON;

	[SerializeField]
	private bool _upkeepSettingON;

	[SerializeField]
	private bool _manpowerSettingON;

	[SerializeField]
	private bool _productionTakesTimeON;

	[SerializeField]
	private bool _isCampaign;

	[SerializeField]
	private bool _isOfficialMapWithLandingControl;

	[SerializeField]
	private bool _isEuropeScenario;

	[SerializeField]
	private bool _isWorldScenario;

	[SerializeField]
	private bool _isPacificScenario;

	[Header("Networking")]
	[SerializeField]
	private bool _isRealTimeMultiplayer;

	[Header("Currently selected unit")]
	[SerializeField]
	private bool _ignoreOrder;

	[SerializeField]
	private bool _goingForRepair;

	[Header("Player infos/stats")]
	[SerializeField]
	private string _currPlayerCountry;

	[SerializeField]
	private bool _areWeWinning;

	[SerializeField]
	private bool _isMajor;

	[SerializeField]
	private bool _isMinor;

	[SerializeField]
	private bool _isNavalMinor;

	[SerializeField]
	private bool _isNavalPower;

	[SerializeField]
	private bool _canPerformLandings;

	[Header("Resources")]
	[SerializeField]
	private int _our_money;

	[SerializeField]
	private int _our_production;

	[SerializeField]
	private float _warIncomeRatio;

	[SerializeField]
	private int _currentUpkeepCost;

	[SerializeField]
	private int _currentAvailableManpower;

	[SerializeField]
	private int _maxUnitPurchaseAmount;

	[Header("Territory")]
	[SerializeField]
	private int _baseCoreTerritory;

	[SerializeField]
	private int _currentCoreTerritory;

	[SerializeField]
	private float _coreTerritoryPercent;

	[Header("Policies")]
	[SerializeField]
	private bool _hasCarefulPlanning;

	[Header("Our army")]
	[SerializeField]
	private int _alliance_production;

	[Space]
	[SerializeField]
	private int _numberOfOurCommanders;

	[Space]
	[SerializeField]
	private int _numberOfOurPlanes;

	[SerializeField]
	private int _numberOfOurFighters;

	[SerializeField]
	private int _numberOfOurBombers;

	[SerializeField]
	private float _ourPlanesToGroundUnitsRatio;

	[Space]
	[SerializeField]
	private int _numberOfAlliedFighters;

	[Space]
	[SerializeField]
	private int _numberOfOurLandingCrafts;

	[SerializeField]
	private int _numberOfOurShips;

	[SerializeField]
	private int _numberOfOurCarriers;

	[SerializeField]
	private int _numberOfOurSubmarines;

	[SerializeField]
	private int _numberOfOurDestroyers;

	[Space]
	[SerializeField]
	private int _numberOfOurArtillery;

	[Space]
	[SerializeField]
	private int _numberOfOurAntitanks;

	[SerializeField]
	private int _numberOfOurATinfantry;

	[Space]
	[SerializeField]
	private int _numberOfOurTanks;

	[Space]
	[SerializeField]
	private int _numberOfOurInfantry;

	[SerializeField]
	private int _numberOfOurEngineers;

	[Space]
	[SerializeField]
	private int _numberOfOurLaunchSites;

	[SerializeField]
	private int _numberOfOurDeployedLaunchSites;

	[SerializeField]
	private int _numberOfOurRadars;

	[Space]
	[SerializeField]
	private int _numberOfOurAA;

	[SerializeField]
	private int _numberOfOurActiveAA;

	[SerializeField]
	private int _numberOfOurArmouredTrains;

	[Header("Enemies army")]
	[SerializeField]
	private int _numberOfEnemyVPs;

	[SerializeField]
	private int _numberOfEnemyLandVPs;

	[Space]
	[SerializeField]
	private int _enemies_production;

	[Space]
	[SerializeField]
	private int _numberOfEnemyShips;

	[SerializeField]
	private int _numberOfEnemySubmarines;

	[Space]
	[SerializeField]
	private int _numberOfEnemyTanks;

	[Space]
	[SerializeField]
	private int _numberOfEnemyPlanes;

	[SerializeField]
	private int _numberOfEnemyFighters;

	[SerializeField]
	private int _numberOfEnemyHeavyBombers;

	[SerializeField]
	private int _numberOfEnemyTorpedoBombers;

	[Space]
	[SerializeField]
	private int _numberOfEnemyAT;

	[Space]
	[SerializeField]
	private int _numberOfEnemyLandMines;

	[Space]
	[SerializeField]
	private bool _enemyHasTanks;

	[SerializeField]
	private bool _enemyHasPlanes;

	[SerializeField]
	private bool _enemyHasShips;

	[Header("Map settings infos")]
	[SerializeField]
	private int _mapSizeX;

	[SerializeField]
	private int _mapSizeY;

	[SerializeField]
	private int _totalMapSize;

	[Header("Geography")]
	[SerializeField]
	private int _numberOfRivers;

	[SerializeField]
	private int _numberOfHillsAndMountains;

	[SerializeField]
	private int _numberOfForestTiles;

	[SerializeField]
	private int _numberOfSeaTiles;

	[SerializeField]
	private int _numberOfDesertTiles;

	[SerializeField]
	private float _hillAndMountainsPercentage;

	[SerializeField]
	private float _desertTilesPercentage;

	[SerializeField]
	private float _seaTilesPercentage;

	[SerializeField]
	private float _forestPercentage;

	[Header("Purchase / Store")]
	[SerializeField]
	private int _purchaseUnitsLoopCounter;

	[SerializeField]
	private int _purchasedUnits;

	public bool supplyDepotNeeded;

	[SerializeField]
	private bool _engineersNeeded;

	[SerializeField]
	private bool _ownsEngineers;

	[SerializeField]
	private bool _hasInfantry;

	[SerializeField]
	private bool _hasTanks;

	[SerializeField]
	private bool _hasArtillery;

	[SerializeField]
	private bool _hasAT;

	[SerializeField]
	private bool _hasAA;

	[SerializeField]
	private bool _hasShips;

	[SerializeField]
	private bool _hasFighters;

	[SerializeField]
	private bool _hasCAS;

	[SerializeField]
	private bool _hasStrategicBombers;

	[SerializeField]
	private bool _hasBarrageBalloons;

	[SerializeField]
	private bool _hasSupplyDepots;

	[SerializeField]
	private bool _hasEngineers;

	[SerializeField]
	private bool _hasATinfantry43;

	[SerializeField]
	private bool _hasAirport;

	[SerializeField]
	private bool _hasHarbour;

	[SerializeField]
	private bool _hasHeavyProductionMeans;

	[SerializeField]
	private bool _boughtCommanderThisTurn;

	[SerializeField]
	private bool _boughtArmouredTrainThisTurn;

	[SerializeField]
	private bool _boughtSupplyDepotThisTurn;

	[Header("Border Guarding")]
	[SerializeField]
	private bool _showBorderGuardDebug;

	[SerializeField]
	private int _numberOfBorderGuardsNeeded;

	[Header("Misc")]
	public bool startTurn;

	public bool needToRefreshEnemyVPs;

	[SerializeField]
	private int _lastPlayedUnitID;

	[SerializeField]
	private int _playedSameUnitCount;

	private int _currYear;

	private DateTime _currDate;

	private int _randNumber;

	private int _pathingCount;

	private int _currentTurnCount;

	private List<Unit> _purchasableUnits;

	private Dictionary<TileGO, float> _enemyVPsByDistance;

	private Dictionary<Player, int> _dicOfPlayersAndTheirIncome;

	private Dictionary<Player, bool> _dicPlayersIntact;

	private Dictionary<TileGO, VPDangerData> _VPdangerLevels;

	private HashSet<TileGO> _enemyVPsGO;

	private Dictionary<UnitGO, int> _dicOfUnitsWeCanKill;

	private Dictionary<TileGO, int> _dicGroundDamage;

	private List<TileGO> _friendliesToEscort;

	private List<TileGO> _paratroopersToEscort;

	private Dictionary<UnitGO, int> _dicOfDamagePerUnit;

	private Dictionary<TileGO, float> _allAirportAndTheirDistance;

	private Dictionary<TileGO, int> _dicBomberDamage;

	private Dictionary<TileGO, int> _dicUnescortedBomberDamage;

	private Dictionary<TileGO, int> _dicFighterDamage;

	private Dictionary<TileGO, int> _dicBuildingsToProtect;

	private Dictionary<TileGO, int> _dicBbuildingDamage;

	private List<TileGO> _friendlyBombers;

	private List<TileGO> _radarSignals;

	private Dictionary<TileGO, int> _carrierDamage;

	private Dictionary<TileGO, int> _submarineDamage;

	private Dictionary<TileGO, int> _landingCraftsDamage;

	private List<TileGO> _listOfSonarSignals;

	private List<Tile> _bestPath;

	private readonly HashSet<Tile> _reachableTilesForCurrentPathing = new HashSet<Tile>();

	private bool _hasCompleteReachabilityMap;

	private int _reachabilityPlayersAtWarCount;

	private Tile _neighbour;

	private Tile _source;

	private Tile _goal;

	private Tile[,] _tiles;

	private TileGO _VPGOfromNeighbourGoal;

	private TileGO _tileGOForMovement;

	private TileGO _tileGOForAttack;

	private List<UnitGO> _listOfUnitsWeCanKill;

	private List<Unit> _generalsList;

	private List<Unit> _reconUnits;

	private List<Unit> _stratBombers;

	private List<Unit> _stratBombersCanAttack;

	private List<Unit> _tacticalBombers;

	private List<Unit> _tacticalBombersCanAttack;

	private List<Unit> _airTransports;

	private List<Unit> _fighters;

	private List<Unit> _fightersCanAttack;

	private List<Unit> _ships;

	private List<Unit> _submarines;

	private List<Unit> _navalTransports;

	private List<Unit> _carriers;

	private List<Unit> _artillery;

	private List<Unit> _engineers;

	private List<Unit> _antiAir;

	private List<Unit> _others;

	private int _lastUnitIDusingUnreachable;

	private bool _isTutorial;

	private bool _isAllyPlaying;

	private bool _canCameraFollowAI;

	private bool _cameraNeedFocusOnUnit;

	private bool _backupQuickmovementRule;

	private float _distanceCamToUnit = float.MaxValue;

	private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

	private readonly HashSet<short> _ourLandMassesIndexes = new HashSet<short>();

	private readonly List<BorderDebugEntry> _debugBorderTiles = new List<BorderDebugEntry>();

	private readonly List<BorderDebugEntry> _debugGuardedTiles = new List<BorderDebugEntry>();

	private readonly List<BorderDebugEntry> _debugGuardUnits = new List<BorderDebugEntry>();

	private readonly HashSet<string> _scenariosWithLandingControl = new HashSet<string>
	{
		"Europe", "world", "World", "WORLD", "Berlin", "Poland", "Eastern Front", "Monte", "Montecassino", "Grapeshot",
		"Sea Lion", "Winter War", "Continuation War"
	};

	private const int MIN_WALKING_TURNS_FOR_REDEPLOY = 10;

	private const int MIN_TURNS_SAVED_BY_REDEPLOY = 3;

	private const float MAX_REDEPLOY_TO_WALKING_TURNS_RATIO = 0.75f;

	private const int MIN_REDEPLOY_DELAY_IN_TURNS = 1;

	private const int AIR_MP_COST_PER_HEX = 1;

	private void Awake()
	{
		instance = this;
		isPlaying = false;
	}

	private void Start()
	{
		_bestPath = new List<Tile>();
		_enemyVPsGO = new HashSet<TileGO>();
		AIWaitInstructions = new Queue<AIWaitInstruction>();
		_allAirportAndTheirDistance = new Dictionary<TileGO, float>();
		_enemyVPsByDistance = new Dictionary<TileGO, float>();
		_dicOfDamagePerUnit = new Dictionary<UnitGO, int>();
		_dicOfUnitsWeCanKill = new Dictionary<UnitGO, int>();
		_dicBomberDamage = new Dictionary<TileGO, int>();
		_dicUnescortedBomberDamage = new Dictionary<TileGO, int>();
		_dicFighterDamage = new Dictionary<TileGO, int>();
		_dicGroundDamage = new Dictionary<TileGO, int>();
		_dicBbuildingDamage = new Dictionary<TileGO, int>();
		_dicBuildingsToProtect = new Dictionary<TileGO, int>();
		_friendliesToEscort = new List<TileGO>();
		_paratroopersToEscort = new List<TileGO>();
		_friendlyBombers = new List<TileGO>();
		_radarSignals = new List<TileGO>();
		_carrierDamage = new Dictionary<TileGO, int>();
		_submarineDamage = new Dictionary<TileGO, int>();
		_landingCraftsDamage = new Dictionary<TileGO, int>();
		_listOfSonarSignals = new List<TileGO>();
		_listOfUnitsWeCanKill = new List<UnitGO>();
		_dicPlayersIntact = new Dictionary<Player, bool>();
		_dicOfPlayersAndTheirIncome = new Dictionary<Player, int>();
		_generalsList = new List<Unit>();
		_reconUnits = new List<Unit>();
		_stratBombers = new List<Unit>();
		_stratBombersCanAttack = new List<Unit>();
		_tacticalBombers = new List<Unit>();
		_tacticalBombersCanAttack = new List<Unit>();
		_fighters = new List<Unit>();
		_fightersCanAttack = new List<Unit>();
		_airTransports = new List<Unit>();
		_submarines = new List<Unit>();
		_ships = new List<Unit>();
		_navalTransports = new List<Unit>();
		_carriers = new List<Unit>();
		_artillery = new List<Unit>();
		_engineers = new List<Unit>();
		_antiAir = new List<Unit>();
		_others = new List<Unit>();
		_VPdangerLevels = new Dictionary<TileGO, VPDangerData>();
		_backupQuickmovementRule = PlayerSettings.Instance.IsQuickMovement;
		_mapSizeX = GameData.Instance.map.SizeX;
		_mapSizeY = GameData.Instance.map.SizeY;
		_totalMapSize = _mapSizeX * _mapSizeY;
		_isOfficialMapWithLandingControl = false;
		_isTutorial = GameData.Instance.map.IsTutorial;
		_isCampaign = GameData.Instance.map.IsCampaign;
		_isRealTimeMultiplayer = MultiplayerManager.IsRealtimeMultiplayer();
		_upkeepSettingON = PlayerSettings.Instance.Upkeep;
		_manpowerSettingON = PlayerSettings.Instance.Manpower;
		_productionTakesTimeON = PlayerSettings.Instance.ProductionTakesTime;
		_isHistoricalFocusON = PlayerSettings.Instance.ISHistoricalFocus;
		_fastAI = PlayerSettings.Instance.FastAI;
		_fastAI_AlliesOnly = PlayerSettings.Instance.FastAllies;
		_difficultyLevel = PlayerSettings.Instance.DifficultyLevel;
		if (_isHistoricalFocusON || (_isCampaign && GameData.Instance.map.IsOfficial))
		{
			foreach (string item in _scenariosWithLandingControl)
			{
				if (!string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && GameData.Instance.map.ScenarioName.Contains(item))
				{
					_isOfficialMapWithLandingControl = true;
					break;
				}
			}
		}
		_isEuropeScenario = !string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && GameData.Instance.map.ScenarioName.ToLower().Contains("europe");
		_isWorldScenario = !string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && GameData.Instance.map.ScenarioName.ToLower().Contains("world");
		_isPacificScenario = !string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && GameData.Instance.map.ScenarioName.ToLower().Contains("pacific");
		if (MapGO.IsAIOnly)
		{
			rightUI_Panel.SetActive(value: false);
			if ((bool)leftUI_Panel)
			{
				leftUI_Panel.SetActive(value: false);
			}
			if ((bool)rightUI_PaneliPad)
			{
				rightUI_PaneliPad.SetActive(value: false);
			}
		}
		InitializeCampaignBalance();
	}

	private void LateUpdate()
	{
		Player currPlayer = TurnManager.currPlayer;
		if (MapGO.isOver || !currPlayer.IsComputer)
		{
			return;
		}
		if (startTurn)
		{
			startTurn = false;
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			isPlaying = true;
			_numberOfBorderGuardsNeeded = 0;
			_isAllyPlaying = currPlayer.isAlliedWithHumanPlayer;
			_engineersNeeded = false;
			supplyDepotNeeded = false;
			needToRefreshEnemyVPs = false;
			_currentTurnCount = GameData.Instance.map.turnCount;
			_currPlayerCountry = currPlayer.Country;
			_isMinor = currPlayer.IsMinor();
			_isMajor = currPlayer.IsMajor();
			_isNavalPower = currPlayer.IsNavalMajor();
			_isNavalMinor = currPlayer.IsNavalMinor();
			_canPerformLandings = !_isHistoricalFocusON || !_isMinor || _isNavalMinor;
			_tiles = GameData.Instance.map.TilesTable;
			_currDate = GameData.Instance.map.Date;
			_currYear = _currDate.Year;
			if (_isOfficialMapWithLandingControl)
			{
				if (_currPlayerCountry == "Great Britain")
				{
					if (_isEuropeScenario || _isWorldScenario)
					{
						_canPerformLandings = _currDate >= new DateTime(1943, 7, 1);
					}
					else if (_numberOfDesertTiles == 0)
					{
						_canPerformLandings = false;
					}
					else
					{
						_canPerformLandings = _currYear >= 1940;
					}
				}
				else if (_currPlayerCountry == "Canada")
				{
					_canPerformLandings = _currDate >= new DateTime(1944, 6, 1);
				}
				else if (_currPlayerCountry == "France")
				{
					_canPerformLandings = _currDate >= new DateTime(1944, 6, 1);
				}
				else if (_currPlayerCountry == "Nazi Germany")
				{
					_canPerformLandings = _currDate >= new DateTime(1940, 4, 9) && _currDate <= new DateTime(1940, 11, 1);
				}
				else if (_currPlayerCountry == "Soviet Union")
				{
					if (_isEuropeScenario || _isWorldScenario)
					{
						_canPerformLandings = false;
					}
					else
					{
						_canPerformLandings = _currDate >= new DateTime(1942, 1, 1);
					}
				}
				else if (_currPlayerCountry == "Italy")
				{
					if (_isEuropeScenario)
					{
						_canPerformLandings = _currDate >= new DateTime(1940, 10, 28) && _currDate <= new DateTime(1941, 6, 1);
					}
					else
					{
						_canPerformLandings = _currDate >= new DateTime(1941, 1, 1) && _currDate <= new DateTime(1943, 7, 1);
					}
				}
				else if (_currPlayerCountry == "Vichy France")
				{
					if (_isEuropeScenario || _isWorldScenario)
					{
						_canPerformLandings = false;
					}
				}
				else if (_currPlayerCountry == "China")
				{
					_canPerformLandings = false;
				}
			}
			else
			{
				if (_currYear >= 1700 && _currYear <= 1820 && (_currPlayerCountry == ".United Kingdom" || _currPlayerCountry == ".France" || _currPlayerCountry == ".Austria" || _currPlayerCountry == ".Prussia" || _currPlayerCountry == ".Russia"))
				{
					_canPerformLandings = false;
				}
				if (_currYear >= 1910 && _currYear <= 1920 && (_currPlayerCountry == ".Austria-Hungary" || _currPlayerCountry == ".Russian Empire" || _currPlayerCountry == ".Albania" || _currPlayerCountry == ".Belgium" || _currPlayerCountry == ".Serbia" || _currPlayerCountry == ".Poland" || _currPlayerCountry == ".Montenegro" || _currPlayerCountry == ".Bulgaria" || _currPlayerCountry == ".Romania" || _currPlayerCountry == ".Persia" || _currPlayerCountry == ".Nejd" || _currPlayerCountry == ".RSFSR" || _currPlayerCountry == ".Finland" || _currPlayerCountry == ".Norway" || _currPlayerCountry == ".Sweden" || _currPlayerCountry == ".Netherlands" || _currPlayerCountry == ".Switzerland" || _currPlayerCountry == ".Ottoman Empire" || _currPlayerCountry == ".German Empire"))
				{
					_canPerformLandings = false;
				}
			}
			AIplayingPanel.SetActive(value: true);
			rightUI_Panel.SetActive(value: false);
			if ((bool)leftUI_Panel)
			{
				leftUI_Panel.SetActive(value: false);
			}
			if ((bool)rightUI_PaneliPad)
			{
				rightUI_PaneliPad.SetActive(value: false);
			}
			AIisPlaying_Text.text = LocalizationManager.GetLocalizedCountry(_currPlayerCountry).Replace(".", "") + LocalizationManager.Translate("txt-AI playing");
			currPlayer.IsEndTurnPanel = true;
			_hasCarefulPlanning = currPlayer.HasPolicy(Policies.Type.Careful_planning);
			RefreshCameraFollowAuthorization();
			if (!_isRealTimeMultiplayer || (_isRealTimeMultiplayer && Utils.IsMasterClient()))
			{
				UpdateLoadingBar(0f);
				TransferVPs();
				CountOwnUnits(currPlayer);
				CountEnemyUnits(currPlayer);
				CalculateIncomeForAllPlayers();
				GatherEnemyVictoryPoints(currPlayer);
				DeployUnits(currPlayer, currPlayer.ShouldGuardBorders());
				CheckWinningWar(currPlayer);
				GoToWar();
				UpgradeDowngradeUnits();
				StartCoroutine(CR_PlayAllUnits(currPlayer));
				if (_isRealTimeMultiplayer)
				{
					StartCoroutine(CR_SyncAIPlayingBar());
				}
			}
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.ToggleAISpeed) && !MultiplayerManager.IsRealtimeMultiplayer() && (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _)))
		{
			PlayerSettings.Instance.FastAI = !PlayerSettings.Instance.FastAI;
			_fastAI = PlayerSettings.Instance.FastAI;
			RefreshCameraFollowAuthorization();
		}
		if (!_canCameraFollowAI || !_cameraNeedFocusOnUnit)
		{
			return;
		}
		UnitGO selectedUnitGO = MapGO.selectedUnitGO;
		if (selectedUnitGO == null)
		{
			return;
		}
		Transform transform = null;
		if (!selectedUnitGO.tileGO.isInFogOfWar)
		{
			transform = selectedUnitGO.transform;
		}
		else
		{
			UnitGO targetedUnitGO = selectedUnitGO.targetedUnitGO;
			if (targetedUnitGO != null && !targetedUnitGO.tileGO.isInFogOfWar)
			{
				transform = targetedUnitGO.transform;
			}
		}
		if (!(transform == null))
		{
			Transform transform2 = CameraGO.instance.transform;
			Vector3 position = transform.position;
			position.z = -80f;
			transform2.position = Vector3.Lerp(transform2.position, position, Time.deltaTime * 5f);
			_distanceCamToUnit = Vector2.Distance(new Vector2(transform2.position.x, transform2.position.y), new Vector2(position.x, position.y));
		}
	}

	public bool IsEuropeWithHistoricalFocus()
	{
		if (_isEuropeScenario)
		{
			return _isHistoricalFocusON;
		}
		return false;
	}

	public void PlayerControlAboutToChange(Player p_player)
	{
		if (TurnManager.currPlayer == p_player && p_player.IsComputer)
		{
			isPlaying = false;
			StopAllCoroutines();
			MapGO.instance.UnselectUnit();
			AIplayingPanel.SetActive(value: false);
		}
	}

	private bool ShouldAIWaitForFocusOnUnit(UnitGO p_unitGO)
	{
		if (_canCameraFollowAI && _distanceCamToUnit >= 1f && p_unitGO != null)
		{
			if (p_unitGO.tileGO.isInFogOfWar)
			{
				if (p_unitGO.targetedUnitGO != null)
				{
					return !p_unitGO.targetedUnitGO.tileGO.isInFogOfWar;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool ValidSelectedUnitGO(UnitGO p_unitGO)
	{
		if (p_unitGO != null && p_unitGO.unit != null && p_unitGO.unit.CurrHP > 0)
		{
			return !p_unitGO.unit.IsDead;
		}
		return false;
	}

	private void UpdateLoadingBar(float p_value)
	{
		loadingBar.value = Mathf.Clamp01(p_value);
	}

	private void InitializeCampaignBalance()
	{
		if (MainMenu.isLoadedGame || !GameData.Instance.map.IsCampaign || GameData.Instance.map.timePlayed > 0f)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsComputer)
			{
				continue;
			}
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				if (listOfUnit.IsCoreUnit)
				{
					listOfUnit.CalculateAndSetUnitPrice();
					listOfUnit.CalculateAndSetManpowerCost();
					num += listOfUnit.Cost;
					num2 += listOfUnit.manpowerCost;
				}
			}
		}
		if (num == 0 && num2 == 0)
		{
			return;
		}
		float num3 = (float)_difficultyLevel * 0.5f;
		int num4 = 30;
		int num5 = Mathf.RoundToInt((float)num * num3 / (float)num4);
		int num6 = Mathf.RoundToInt((float)num2 * num3);
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer2.IsComputer)
			{
				listOfPlayer2.AddMoney(num5, p_sendRPC: false);
				listOfPlayer2.BonusIncome += num5;
				listOfPlayer2.BonusManpower += num6;
			}
		}
	}

	private void RefreshCameraFollowAuthorization()
	{
		_canCameraFollowAI = true;
		if (_fastAI)
		{
			_canCameraFollowAI = false;
		}
		else if (_fastAI_AlliesOnly && _isAllyPlaying)
		{
			_canCameraFollowAI = false;
		}
		else if (!PlayerSettings.Instance.FollowAIMoves)
		{
			_canCameraFollowAI = false;
		}
	}

	private IEnumerator CR_SyncAIPlayingBar()
	{
		WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
		float previousValue = -1f;
		while (TurnManager.currPlayer.IsComputer)
		{
			if (loadingBar.value != previousValue)
			{
				MultiplayerManager.RaiseEvent(95, loadingBar.value);
			}
			previousValue = loadingBar.value;
			yield return waitForSeconds;
		}
	}

	private IEnumerator ProcessAIWaitInstructionsAndMovement()
	{
		while (AIWaitInstructions.Count > 0)
		{
			AIWaitInstruction aIWaitInstruction = AIWaitInstructions.Dequeue();
			if (aIWaitInstruction.waitTime == 0f)
			{
				yield return _waitForEndOfFrame;
			}
			else
			{
				yield return new WaitForSeconds(aIWaitInstruction.waitTime);
			}
		}
	}

	private IEnumerator CR_PlayAllUnits(Player p_player)
	{
		if (p_player.ShouldGuardBorders())
		{
			yield return CR_GuardBorders(p_player);
		}
		if (p_player.Faction == "Neutral")
		{
			if (_numberOfBorderGuardsNeeded > 0 && p_player.ShouldGuardBorders())
			{
				PerformPurchases(p_player, p_isForBorderGuarding: true);
			}
		}
		else
		{
			_reconUnits.Clear();
			_stratBombersCanAttack.Clear();
			_tacticalBombersCanAttack.Clear();
			_fightersCanAttack.Clear();
			_stratBombers.Clear();
			_navalTransports.Clear();
			_carriers.Clear();
			_tacticalBombers.Clear();
			_airTransports.Clear();
			_submarines.Clear();
			_ships.Clear();
			_fighters.Clear();
			_artillery.Clear();
			_engineers.Clear();
			_antiAir.Clear();
			_generalsList.Clear();
			_others.Clear();
			foreach (Unit listOfUnit in p_player.ListOfUnits)
			{
				if (listOfUnit.IsReserve || listOfUnit.CurrHP == 0 || listOfUnit.unitGO == null)
				{
					_others.Add(listOfUnit);
				}
				else if (listOfUnit.IsOnBoat)
				{
					_navalTransports.Add(listOfUnit);
				}
				else if (listOfUnit.IsParatrooper && listOfUnit.Type == "Plane")
				{
					_airTransports.Add(listOfUnit);
				}
				else if (listOfUnit.IsRecon && !listOfUnit.IsSubmarine)
				{
					_reconUnits.Add(listOfUnit);
				}
				else if (listOfUnit.IsCommander || listOfUnit.IsSupplyDepot)
				{
					_generalsList.Add(listOfUnit);
				}
				else if (listOfUnit.FilterType == "FilterBomber")
				{
					if (listOfUnit.IsReserve || !listOfUnit.unitGO || !listOfUnit.unitGO.tileGO || !listOfUnit.unitGO.tileGO.groundUnitGO || !listOfUnit.unitGO.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						_stratBombers.Add(listOfUnit);
					}
					else
					{
						_stratBombersCanAttack.Add(listOfUnit);
					}
				}
				else if (listOfUnit.FilterType == "FilterCAS")
				{
					if (listOfUnit.IsReserve || !listOfUnit.unitGO || !listOfUnit.unitGO.tileGO || !listOfUnit.unitGO.tileGO.groundUnitGO || !listOfUnit.unitGO.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						_tacticalBombers.Add(listOfUnit);
					}
					else
					{
						_tacticalBombersCanAttack.Add(listOfUnit);
					}
				}
				else if (listOfUnit.FilterType == "FilterFighter")
				{
					if (listOfUnit.IsReserve || !listOfUnit.unitGO || (!listOfUnit.HasBombs && !listOfUnit.HasRockets) || !listOfUnit.unitGO.tileGO || !listOfUnit.unitGO.tileGO.groundUnitGO || !listOfUnit.unitGO.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						_fighters.Add(listOfUnit);
					}
					else
					{
						_fightersCanAttack.Add(listOfUnit);
					}
				}
				else if (listOfUnit.IsCarrier)
				{
					_carriers.Add(listOfUnit);
				}
				else if (listOfUnit.IsSubmarine)
				{
					_submarines.Add(listOfUnit);
				}
				else if (listOfUnit.Type == "FilterBoat")
				{
					_ships.Add(listOfUnit);
				}
				else if (listOfUnit.FilterType == "FilterArtillery")
				{
					_artillery.Add(listOfUnit);
				}
				else if (listOfUnit.IsEngineers)
				{
					_engineers.Add(listOfUnit);
				}
				else if (listOfUnit.FilterType == "FilterAA")
				{
					_antiAir.Add(listOfUnit);
				}
				else
				{
					_others.Add(listOfUnit);
				}
			}
			p_player.ListOfUnits = SortUnitsForBestStrategicPlayOrder(_reconUnits).Concat(SortUnitsForBestStrategicPlayOrder(_stratBombersCanAttack)).Concat(SortUnitsForBestStrategicPlayOrder(_stratBombers)).Concat(SortUnitsForBestStrategicPlayOrder(_tacticalBombersCanAttack))
				.Concat(SortUnitsForBestStrategicPlayOrder(_tacticalBombers))
				.Concat(SortUnitsForBestStrategicPlayOrder(_airTransports))
				.Concat(SortUnitsForBestStrategicPlayOrder(_fightersCanAttack))
				.Concat(SortUnitsForBestStrategicPlayOrder(_fighters))
				.Concat(SortUnitsForBestStrategicPlayOrder(_submarines))
				.Concat(SortUnitsForBestStrategicPlayOrder(_carriers))
				.Concat(SortUnitsForBestStrategicPlayOrder(_ships))
				.Concat(SortUnitsForBestStrategicPlayOrder(_artillery))
				.Concat(SortUnitsForBestStrategicPlayOrder(_navalTransports))
				.Concat(SortUnitsForBestStrategicPlayOrder(_engineers))
				.Concat(SortUnitsForBestStrategicPlayOrder(_others))
				.Concat(SortUnitsForBestStrategicPlayOrder(_antiAir))
				.Concat(SortUnitsForBestStrategicPlayOrder(_generalsList))
				.ToList();
			for (byte runTime = 1; runTime <= 2; runTime++)
			{
				float loadingbarProgress = 0f;
				float unitsCount = p_player.ListOfUnits.Count;
				for (int i = 0; i < p_player.ListOfUnits.Count && !MapGO.isOver; i++)
				{
					_ignoreOrder = false;
					_cameraNeedFocusOnUnit = false;
					AIWaitInstructions.Clear();
					if (p_player.ListOfUnits[i].CurrHP == 0 || p_player.ListOfUnits[i].IsDead || p_player.ListOfUnits[i].OwnerName != p_player.Name)
					{
						p_player.RemoveUnit(p_player.ListOfUnits[i], p_sendRPC: true);
						i--;
						continue;
					}
					if (p_player.ListOfUnits[i].unitGO == null)
					{
						p_player.ListOfUnits[i].IsReserve = true;
					}
					if (p_player.ListOfUnits[i].IsReserve)
					{
						continue;
					}
					if (i != 0)
					{
						loadingbarProgress++;
						UpdateLoadingBar(loadingbarProgress / unitsCount);
					}
					if ((p_player.ListOfUnits[i].MaxMP == 0 && (p_player.ListOfUnits[i].Range == 0 || p_player.ListOfUnits[i].MaxAmmo == 0)) || (runTime == 2 && p_player.ListOfUnits[i].HasAttacked && p_player.ListOfUnits[i].CurrMP == 0))
					{
						continue;
					}
					while (Time.timeScale == 0f)
					{
						yield return _waitForEndOfFrame;
					}
					_tileGOForAttack = null;
					_tileGOForMovement = null;
					MapGO.selectedUnitGO = p_player.ListOfUnits[i].unitGO;
					MapGO.selectedUnitGO.targetedUnitGO = null;
					if (MapGO.selectedUnitGO.unit.ID == _lastPlayedUnitID)
					{
						_playedSameUnitCount++;
					}
					else
					{
						_playedSameUnitCount = 0;
					}
					_lastPlayedUnitID = MapGO.selectedUnitGO.unit.ID;
					if (!MapGO.selectedUnitGO.unit.IsDefensive && ((MapGO.selectedUnitGO.tileGO.tile.IsVictoryPoint && MapGO.selectedUnitGO.tileGO.tile.Capturable && MapGO.selectedUnitGO.tileGO.tile.enumType != TileTypes.HARBOUR) || MapGO.selectedUnitGO.tileGO.tile.IsWW1TrenchTile()) && MapGO.selectedUnitGO.unit.Type != "Plane" && MapGO.selectedUnitGO.unit.Type != "Tank" && MapGO.selectedUnitGO.unit.FilterType != "FilterArtillery")
					{
						MapGO.selectedUnitGO.unit.IsDefensive = true;
					}
					MapGO.selectedUnitGO.gameObject.SetActive(value: true);
					if (MapGO.selectedUnitGO.unit.Type == "Plane")
					{
						UIManager.showAirUnit = true;
						if (MapGO.selectedUnitGO.tileGO.groundUnitGO != null)
						{
							MapGO.selectedUnitGO.tileGO.groundUnitGO.gameObject.SetActive(value: false);
						}
						if (MapGO.selectedUnitGO.unit.CurrAutonomy < MapGO.selectedUnitGO.unit.MaxMP && MapGO.selectedUnitGO.unit.Type == "Plane")
						{
							MapGO.selectedUnitGO.unit.CurrAutonomy = MapGO.selectedUnitGO.unit.MaxMP;
						}
					}
					else
					{
						UIManager.showAirUnit = false;
						if (MapGO.selectedUnitGO.tileGO.airUnitGO != null)
						{
							MapGO.selectedUnitGO.tileGO.airUnitGO.gameObject.SetActive(value: false);
						}
						if (runTime == 1 && MapGO.selectedUnitGO.unit.IsSubmarine && MapGO.selectedUnitGO.unit.IsSubmerged)
						{
							MapGO.selectedUnitGO.DiveUndiveSubmarine(p_sendRPC: true);
						}
						if (MapGO.selectedUnitGO.unit.CurrAutonomy < MapGO.selectedUnitGO.unit.MaxMP && MapGO.selectedUnitGO.unit.Type == "Boat")
						{
							MapGO.selectedUnitGO.unit.CurrAutonomy = MapGO.selectedUnitGO.unit.MaxMP;
						}
						RepairOrBlowupBridge(MapGO.selectedUnitGO);
					}
					MapGO.selectedUnitGO.unit.shouldMoveOutNow = ShouldLeaveTileForSomeoneElse(MapGO.selectedUnitGO, MapGO.selectedUnitGO.tileGO, RunTimes.SECOND);
					DoResupply(MapGO.selectedUnitGO, MapGO.selectedUnitGO.unit.shouldMoveOutNow);
					if (needToRefreshEnemyVPs)
					{
						GatherEnemyVictoryPoints(p_player);
					}
					switch (runTime)
					{
					case 1:
						if (MapGO.selectedUnitGO.unit.IsPontoonBridge && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.RIVER && !MapGO.selectedUnitGO.tileGO.tile.isFrozen)
						{
							MapGO.selectedUnitGO.unit.OthersNeedUsAsBridge = true;
						}
						break;
					case 2:
						if (MapGO.selectedUnitGO.unit.OthersNeedUsAsBridge && !MapGO.selectedUnitGO.unit.OthersNeedUsAsBridgeNotArtificially)
						{
							MapGO.selectedUnitGO.unit.OthersNeedUsAsBridge = false;
						}
						break;
					}
					if (MapGO.selectedUnitGO.unit.ActionPoints >= 2 && (MapGO.selectedUnitGO.unit.shouldMoveOutNow || MapGO.selectedUnitGO.unit.FilterType == "FilterArtillery" || MapGO.selectedUnitGO.unit.FilterType == "FilterAA" || MapGO.selectedUnitGO.unit.Type == "Plane" || MapGO.selectedUnitGO.unit.FilterType == "FilterBoat"))
					{
						CheckForAttack(MapGO.selectedUnitGO);
						_cameraNeedFocusOnUnit = false;
						if (_tileGOForAttack != null)
						{
							if (_canCameraFollowAI)
							{
								_cameraNeedFocusOnUnit = true;
								_distanceCamToUnit = float.MaxValue;
								while (ShouldAIWaitForFocusOnUnit(MapGO.selectedUnitGO))
								{
									yield return new WaitForSeconds(0.3f);
								}
							}
							if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
							{
								i--;
								continue;
							}
							if (MapGO.selectedUnitGO.targetedUnitGO != null)
							{
								MapGO.selectedUnitGO.AttackUnit(MapGO.selectedUnitGO.targetedUnitGO, p_isRetaliationPossible: true, p_isSupportPossible: true, p_isAmbush: false);
							}
							else if (MapGO.selectedUnitGO.unit.FilterType == "FilterBomber" && _tileGOForAttack.tile.MoneyProductionPerTurn > 0 && _tileGOForAttack.groundUnitGO == null && (MapGO.selectedUnitGO.unit.IsMissile || (!MapGO.selectedUnitGO.unit.IsRecon && MapGO.selectedUnitGO.unit.FilterType == "FilterBomber")))
							{
								MapGO.selectedUnitGO.AttackTile(_tileGOForAttack);
							}
							yield return ProcessAIWaitInstructionsAndMovement();
						}
					}
					if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
					{
						i--;
						continue;
					}
					if (MapGO.selectedUnitGO.unit.FilterType != "FilterArtillery" || (MapGO.selectedUnitGO.unit.FilterType == "FilterArtillery" && runTime == 2))
					{
						_pathingCount = 0;
						_VPGOfromNeighbourGoal = null;
						Move(MapGO.selectedUnitGO);
						if (_tileGOForMovement != null)
						{
							if (_canCameraFollowAI)
							{
								_cameraNeedFocusOnUnit = true;
								_distanceCamToUnit = float.MaxValue;
								while (ShouldAIWaitForFocusOnUnit(MapGO.selectedUnitGO))
								{
									yield return new WaitForSeconds(0.3f);
								}
							}
							if (ValidSelectedUnitGO(MapGO.selectedUnitGO))
							{
								MapGO.selectedUnitGO.MoveUnitToTile(_tileGOForMovement);
								while (ValidSelectedUnitGO(MapGO.selectedUnitGO) && MapGO.selectedUnitGO.isMoving)
								{
									yield return _waitForEndOfFrame;
								}
							}
							yield return ProcessAIWaitInstructionsAndMovement();
						}
						if (ValidSelectedUnitGO(MapGO.selectedUnitGO) && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR && i > 0 && _playedSameUnitCount < 2)
						{
							i--;
							_playedSameUnitCount++;
							continue;
						}
					}
					if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
					{
						i--;
						continue;
					}
					RepairOrBlowupBridge(MapGO.selectedUnitGO);
					if (MapGO.selectedUnitGO.unit.IsDropable && !TurnManager.isBadWeather && MapGO.selectedUnitGO.tileGO.groundUnitGO == null && MapGO.selectedUnitGO.goal != null && MapGO.selectedUnitGO.goal == MapGO.selectedUnitGO.tileGO && MapGO.selectedUnitGO.tileGO.tile.enumType != TileTypes.SEA && MapGO.selectedUnitGO.tileGO.tile.enumType != TileTypes.RIVER && (MapGO.selectedUnitGO.unit.IsMountaineer || MapGO.selectedUnitGO.tileGO.tile.enumType != TileTypes.MOUNTAIN))
					{
						MapGO.selectedUnitGO.DropParatrooper(p_sendRPC: true);
						yield return ProcessAIWaitInstructionsAndMovement();
					}
					CheckForAttack(MapGO.selectedUnitGO);
					if (_tileGOForAttack != null)
					{
						if (_canCameraFollowAI)
						{
							_cameraNeedFocusOnUnit = true;
							_distanceCamToUnit = float.MaxValue;
							while (ShouldAIWaitForFocusOnUnit(MapGO.selectedUnitGO))
							{
								yield return new WaitForSeconds(0.3f);
							}
						}
						if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
						{
							i--;
							continue;
						}
						if (MapGO.selectedUnitGO.targetedUnitGO != null)
						{
							MapGO.selectedUnitGO.AttackUnit(MapGO.selectedUnitGO.targetedUnitGO, p_isRetaliationPossible: true, p_isSupportPossible: true, p_isAmbush: false);
						}
						else if (MapGO.selectedUnitGO.unit.FilterType == "FilterBomber" && _tileGOForAttack.tile.MoneyProductionPerTurn > 0 && _tileGOForAttack.groundUnitGO == null && (MapGO.selectedUnitGO.unit.IsMissile || (!MapGO.selectedUnitGO.unit.IsRecon && MapGO.selectedUnitGO.unit.FilterType == "FilterBomber")))
						{
							MapGO.selectedUnitGO.AttackTile(_tileGOForAttack);
						}
						yield return ProcessAIWaitInstructionsAndMovement();
					}
					if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
					{
						i--;
						continue;
					}
					if (MapGO.selectedUnitGO.unit.shouldMoveOutNow || MapGO.selectedUnitGO.unit.GetHPPercent(p_forDamagePurposes: false) <= 0.5f || (PlayerSettings.Instance.OneMoreTilePlanes && MapGO.selectedUnitGO.unit.Type == "Plane"))
					{
						_pathingCount = 0;
						_VPGOfromNeighbourGoal = null;
						Move(MapGO.selectedUnitGO);
						if (_tileGOForMovement != null)
						{
							if (_canCameraFollowAI)
							{
								_cameraNeedFocusOnUnit = true;
								_distanceCamToUnit = float.MaxValue;
								while (ShouldAIWaitForFocusOnUnit(MapGO.selectedUnitGO))
								{
									yield return new WaitForSeconds(0.3f);
								}
							}
							if (ValidSelectedUnitGO(MapGO.selectedUnitGO))
							{
								MapGO.selectedUnitGO.MoveUnitToTile(_tileGOForMovement);
								while (ValidSelectedUnitGO(MapGO.selectedUnitGO) && MapGO.selectedUnitGO.isMoving)
								{
									yield return _waitForEndOfFrame;
								}
							}
							yield return ProcessAIWaitInstructionsAndMovement();
						}
					}
					if (!ValidSelectedUnitGO(MapGO.selectedUnitGO))
					{
						i--;
						continue;
					}
					if (runTime == 1)
					{
						DoResupply(MapGO.selectedUnitGO, p_ignoreHP: false);
					}
					else
					{
						if (MapGO.selectedUnitGO.unit.IsOnBoat && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR)
						{
							MapGO.selectedUnitGO.EmbarkDisembarkBoat(p_sendRPC: true);
						}
						if (MapGO.selectedUnitGO.unit.ActionPoints >= 2 && (MapGO.selectedUnitGO.unit.CurrAmmo != MapGO.selectedUnitGO.unit.MaxAmmo || MapGO.selectedUnitGO.unit.CurrAutonomy != MapGO.selectedUnitGO.unit.MaxAutonomy))
						{
							MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
							MapGO.selectedUnitGO.unit.ResupplyAmmoAndFuel();
						}
					}
					if (MapGO.selectedUnitGO.unit.IsSubmarine && !MapGO.selectedUnitGO.unit.IsSubmerged)
					{
						MapGO.selectedUnitGO.DiveUndiveSubmarine(p_sendRPC: true);
					}
					yield return ProcessAIWaitInstructionsAndMovement();
					MapGO.selectedUnitGO = null;
				}
			}
		}
		MapGO.instance.UnselectUnit();
		yield return _waitForEndOfFrame;
		while (Time.timeScale == 0f)
		{
			yield return _waitForEndOfFrame;
		}
		foreach (Unit listOfUnit2 in p_player.ListOfUnits)
		{
			listOfUnit2.OthersNeedUsAsBridge = false;
			listOfUnit2.OthersNeedUsAsBridgeNotArtificially = false;
		}
		PerformPurchases(p_player, p_isForBorderGuarding: false);
		if (p_player.MoneyAmount > 0)
		{
			InvitePlayersToFaction(p_player);
			SendMoneyToAllies();
		}
		PickPolicies();
		ResearchTechnologies();
		PlayerSettings.Instance.IsQuickMovement = _backupQuickmovementRule;
		MapGO.selectedUnitGO = null;
		UpdateLoadingBar(1f);
		yield return _waitForEndOfFrame;
		while (Time.timeScale == 0f)
		{
			yield return _waitForEndOfFrame;
		}
		isPlaying = false;
		if (playerNeedsToRedeployItsUnitsNow == p_player)
		{
			playerNeedsToRedeployItsUnitsNow = null;
		}
		TurnManager.instance.NextTurn(p_sendRPC: true);
		if (!TurnManager.currPlayer.IsComputer)
		{
			AIplayingPanel.SetActive(value: false);
			UIManager.isUIOpen = false;
		}
		TurnManager.currPlayer.IsEndTurnPanel = false;
		if (_isRealTimeMultiplayer)
		{
			if (TurnManager.currPlayer.IsLocalClientPlayer())
			{
				if (MainMenu.isTablet && (bool)rightUI_PaneliPad)
				{
					rightUI_PaneliPad.SetActive(value: true);
				}
				else
				{
					rightUI_Panel.SetActive(value: true);
				}
				yield break;
			}
			rightUI_Panel.SetActive(value: false);
			if ((bool)rightUI_PaneliPad)
			{
				rightUI_PaneliPad.SetActive(value: false);
			}
			if ((bool)leftUI_Panel)
			{
				leftUI_Panel.SetActive(value: false);
			}
		}
		else
		{
			if ((bool)leftUI_Panel)
			{
				leftUI_Panel.SetActive(value: true);
			}
			if (MainMenu.isTablet && (bool)rightUI_PaneliPad)
			{
				rightUI_PaneliPad.SetActive(value: true);
			}
			else
			{
				rightUI_Panel.SetActive(value: true);
			}
		}
	}

	private void PerformPurchases(Player p_player, bool p_isForBorderGuarding)
	{
		_maxUnitPurchaseAmount = (int)((float)_difficultyLevel * _purchase_curve.Evaluate(_totalMapSize));
		if (p_isForBorderGuarding)
		{
			_maxUnitPurchaseAmount = ((_numberOfBorderGuardsNeeded != 0) ? Mathf.Max(_numberOfBorderGuardsNeeded / 2, 1) : 0);
		}
		_purchasedUnits = 0;
		_purchaseUnitsLoopCounter = 0;
		_boughtCommanderThisTurn = false;
		_boughtSupplyDepotThisTurn = false;
		_boughtArmouredTrainThisTurn = false;
		if (p_player.MoneyAmount <= 0)
		{
			return;
		}
		p_player.CalculateTurnIncome();
		bool flag = true;
		if (!p_isForBorderGuarding)
		{
			if (p_player.PlayersAtWar.Count == 0 || _enemies_production == 0)
			{
				flag = false;
			}
			else if (MapGO.currentPlayerVPs.Count == 0)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		CountOwnUnits(p_player);
		CountEnemyUnits(p_player);
		_currentUpkeepCost = p_player.CalculateTotalUnitsUpkeep(p_applyReserveModifier: false);
		_currentAvailableManpower = p_player.GetAvailableManpower();
		if (!GameData.Instance.map.DisablePurchases)
		{
			for (int i = 0; i < _maxUnitPurchaseAmount; i++)
			{
				PurchaseUnits(p_player, i == 0, p_isForBorderGuarding);
			}
		}
		DeployUnits(p_player, p_isForBorderGuarding);
	}

	private void InvitePlayersToFaction(Player p_player)
	{
		if (p_player.Faction == "Neutral" || (!_isTutorial && (_isHistoricalFocusON || _isCampaign)))
		{
			return;
		}
		List<Player> list = new List<Player>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsComputer && !(listOfPlayer.Name == p_player.Name) && listOfPlayer.noRelationWithCurrentPlayer && !(listOfPlayer.Faction != "Neutral") && p_player.MoneyAmount >= p_player.GetAllianceInvitationCost(listOfPlayer))
			{
				list.Add(listOfPlayer);
			}
		}
		if (list.Count != 0)
		{
			Player player = list[UnityEngine.Random.Range(0, list.Count)];
			int allianceInvitationCost = p_player.GetAllianceInvitationCost(player);
			SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Sign peace"));
			p_player.RemoveMoney(allianceInvitationCost, p_sendRPC: true);
			player.AddMoney(allianceInvitationCost, p_sendRPC: true);
			player.ChangeFaction(p_player.Faction, p_getNewAllianceWars: true, p_sendRPC: true);
			UIManager.ShowRelationshipChangeMessage(player.Country, _currPlayerCountry, LocalizationManager.Translate("relationship-new alliance"));
			MultiplayerManager.RPC("RPC_AllianceMade", player.Name, p_player.Name);
			TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		}
	}

	private IEnumerator CR_GuardBorders(Player p_player)
	{
		_numberOfBorderGuardsNeeded = 0;
		List<BorderData> borders = p_player.GetBorders(GameData.Instance.map);
		if (borders.Count == 0)
		{
			yield break;
		}
		float p_value = 0f;
		UpdateLoadingBar(p_value);
		List<Unit> list = GatherUnitsForBorderDefense(p_player);
		int count = list.Count;
		foreach (Unit item in list)
		{
			if (!(item.unitGO == null))
			{
				DoResupply(item.unitGO, p_ignoreHP: false);
			}
		}
		HashSet<Tile> reachableTiles = new HashSet<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		foreach (Unit item2 in list)
		{
			Tile tile = item2.unitGO.tileGO.tile;
			if (reachableTiles.Add(tile))
			{
				queue.Enqueue(tile);
			}
		}
		List<TileGO> currentPlayerVPs = MapGO.currentPlayerVPs;
		for (int i = 0; i < currentPlayerVPs.Count; i++)
		{
			Tile tile2 = currentPlayerVPs[i].tile;
			if (tile2.enumType != TileTypes.SEA && tile2.enumType != TileTypes.MOUNTAIN && reachableTiles.Add(tile2))
			{
				queue.Enqueue(tile2);
			}
		}
		while (queue.Count > 0)
		{
			Tile tile3 = queue.Dequeue();
			for (int j = 0; j < tile3.neighbours.Count; j++)
			{
				Tile tile4 = tile3.neighbours[j];
				if (tile4 != null && tile4.terrainOwner == p_player && !reachableTiles.Contains(tile4) && tile4.enumType != TileTypes.SEA && tile4.enumType != TileTypes.MOUNTAIN && (tile4.enumType != TileTypes.RIVER || (tile4.Bridge && !tile4.IsBridgeDestroyed)) && (tile3.enumType != TileTypes.RIVER || (tile3.Bridge && !tile3.IsBridgeDestroyed)))
				{
					reachableTiles.Add(tile4);
					queue.Enqueue(tile4);
				}
			}
		}
		if (_showBorderGuardDebug)
		{
			_debugBorderTiles.Clear();
			_debugGuardedTiles.Clear();
			_debugGuardUnits.Clear();
			foreach (Unit item3 in list)
			{
				if (!(item3.unitGO == null) && !(item3.unitGO.tileGO == null))
				{
					_debugGuardUnits.Add(new BorderDebugEntry
					{
						position = item3.unitGO.transform.position,
						color = Color.cyan,
						label = item3.Name
					});
				}
			}
		}
		borders.Sort((BorderData a, BorderData b) =>
		{
			int dangerLevel3 = a.OtherPlayer.GetDangerLevel(p_player);
			int dangerLevel4 = b.OtherPlayer.GetDangerLevel(p_player);
			return (dangerLevel3 != dangerLevel4) ? dangerLevel4.CompareTo(dangerLevel3) : b.OtherPlayer.ListOfUnits.Count.CompareTo(a.OtherPlayer.ListOfUnits.Count);
		});
		List<List<Tile>> list2 = new List<List<Tile>>();
		List<TileGO> ourVPs = MapGO.currentPlayerVPs;
		int num = 0;
		foreach (BorderData item4 in borders)
		{
			List<Tile> list3 = new List<Tile>(item4.MyTiles);
			Player otherPlayer = item4.OtherPlayer;
			list3.RemoveAll((Tile item) => !reachableTiles.Contains(item));
			list3.RemoveAll((Tile tile16) =>
			{
				if (tile16.enumType == TileTypes.RIVER || tile16.enumType == TileTypes.MOUNTAIN || tile16.enumType == TileTypes.SEA)
				{
					return true;
				}
				bool flag = false;
				for (int k = 0; k < tile16.neighbours.Count; k++)
				{
					Tile tile17 = tile16.neighbours[k];
					if (tile17 != null && tile17.terrainOwner == otherPlayer && tile17.enumType != TileTypes.RIVER && tile17.enumType != TileTypes.MOUNTAIN && tile17.enumType != TileTypes.SEA)
					{
						flag = true;
						break;
					}
				}
				return !flag;
			});
			list3.RemoveAll((Tile tile16) =>
			{
				for (int k = 0; k < tile16.neighbours.Count; k++)
				{
					Tile tile17 = tile16.neighbours[k];
					if (tile17 != null && tile17.terrainOwner == otherPlayer && tile17.GroundUnit != null && tile17.GroundUnit.MaxMP > 0)
					{
						return false;
					}
				}
				for (int l = 0; l < ourVPs.Count; l++)
				{
					if (Utils.GetHeuristicDistance(tile16, ourVPs[l].tile) <= 6)
					{
						return false;
					}
				}
				return true;
			});
			list3.Sort((Tile a, Tile b) =>
			{
				bool flag = false;
				bool flag2 = false;
				for (int k = 0; k < a.neighbours.Count; k++)
				{
					Tile tile16 = a.neighbours[k];
					if (tile16 != null && tile16.Bridge && !tile16.IsBridgeDestroyed && tile16.terrainOwner == otherPlayer)
					{
						flag = true;
						break;
					}
				}
				for (int l = 0; l < b.neighbours.Count; l++)
				{
					Tile tile17 = b.neighbours[l];
					if (tile17 != null && tile17.Bridge && !tile17.IsBridgeDestroyed && tile17.terrainOwner == otherPlayer)
					{
						flag2 = true;
						break;
					}
				}
				if (flag != flag2)
				{
					if (!flag)
					{
						return 1;
					}
					return -1;
				}
				int num34 = 0;
				int num35 = 0;
				for (int m = 0; m < a.neighbours.Count; m++)
				{
					Tile tile18 = a.neighbours[m];
					if (tile18 != null && tile18.terrainOwner == otherPlayer && (!otherPlayer.IsComputer || otherPlayer.Faction != "Neutral") && tile18.GroundUnit != null && tile18.GroundUnit.MaxMP > 0)
					{
						num34 += tile18.GroundUnit.GetUnitDangerValue();
					}
				}
				for (int n = 0; n < b.neighbours.Count; n++)
				{
					Tile tile19 = b.neighbours[n];
					if (tile19 != null && tile19.terrainOwner == otherPlayer && (!otherPlayer.IsComputer || otherPlayer.Faction != "Neutral") && tile19.GroundUnit != null && tile19.GroundUnit.MaxMP > 0)
					{
						num35 += tile19.GroundUnit.GetUnitDangerValue();
					}
				}
				if (num34 != num35)
				{
					return num35.CompareTo(num34);
				}
				int num36 = a.PosX.CompareTo(b.PosX);
				return (num36 == 0) ? a.PosY.CompareTo(b.PosY) : num36;
			});
			if (_showBorderGuardDebug)
			{
				float t = ((borders.Count > 1) ? ((float)num / (float)(borders.Count - 1)) : 0f);
				Color color = Color.Lerp(Color.red, Color.green, t);
				foreach (Tile item5 in list3)
				{
					if (!(item5.tileGO == null))
					{
						_debugBorderTiles.Add(new BorderDebugEntry
						{
							position = item5.tileGO.transform.position,
							color = color,
							label = item4.OtherPlayer.Country + " (Danger=" + item4.OtherPlayer.GetDangerLevel(p_player) + ")"
						});
					}
				}
			}
			num++;
			list2.Add(list3);
		}
		int num2 = 0;
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			num2 += list2[num3].Count;
		}
		if (num2 == 0)
		{
			yield break;
		}
		int idealGuards = 0;
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			int count2 = list2[num4].Count;
			if (count2 != 0)
			{
				int dangerLevel = borders[num4].OtherPlayer.GetDangerLevel(p_player);
				int num5;
				if (dangerLevel >= 3)
				{
					num5 = 2;
				}
				else
				{
					num5 = ((dangerLevel < 1) ? 4 : 3);
				}
				idealGuards += Mathf.Max(1, Mathf.CeilToInt((float)count2 / (float)num5));
			}
		}
		idealGuards = Mathf.Max(1, idealGuards);
		int actualGuards = 0;
		List<Tile> tilesToGuard = new List<Tile>();
		HashSet<Tile> hashSet = new HashSet<Tile>();
		for (int num6 = 0; num6 < list2.Count; num6++)
		{
			for (int num7 = 0; num7 < list2[num6].Count; num7++)
			{
				hashSet.Add(list2[num6][num7]);
			}
		}
		int num8 = Mathf.Max(idealGuards, count);
		for (int num9 = 0; num9 < list2.Count; num9++)
		{
			List<Tile> list4 = list2[num9];
			int count3 = list4.Count;
			if (count3 == 0)
			{
				continue;
			}
			Player otherPlayer2 = borders[num9].OtherPlayer;
			int dangerLevel2 = borders[num9].OtherPlayer.GetDangerLevel(p_player);
			int num10;
			if (dangerLevel2 >= 3)
			{
				num10 = 2;
			}
			else
			{
				num10 = ((dangerLevel2 < 1) ? 4 : 3);
			}
			int num11 = Mathf.Max(1, Mathf.CeilToInt((float)count3 / (float)num10));
			int num12 = 0;
			HashSet<Tile> hashSet2 = new HashSet<Tile>(list4);
			HashSet<Tile> hashSet3 = new HashSet<Tile>(list4);
			while (hashSet3.Count > 0 && tilesToGuard.Count < num8 && num12 < num11)
			{
				Tile tile5 = null;
				int num13 = -1;
				int num14 = -1;
				int num15 = -1;
				int num16 = -1;
				for (int num17 = 0; num17 < count3; num17++)
				{
					Tile tile6 = list4[num17];
					if (!hashSet3.Contains(tile6))
					{
						continue;
					}
					int num18 = 1;
					for (int num19 = 0; num19 < tile6.neighbours.Count; num19++)
					{
						Tile tile7 = tile6.neighbours[num19];
						if (tile7 != null && hashSet3.Contains(tile7))
						{
							num18++;
						}
					}
					int num20 = 0;
					for (int num21 = 0; num21 < tile6.neighbours.Count; num21++)
					{
						Tile tile8 = tile6.neighbours[num21];
						if (tile8 != null && tile8.terrainOwner == otherPlayer2 && tile8.enumType != TileTypes.RIVER && tile8.enumType != TileTypes.MOUNTAIN && tile8.enumType != TileTypes.SEA)
						{
							num20 = ((tile8.GroundUnit == null || tile8.GroundUnit.MaxMP <= 0) ? (num20 + 1) : (num20 + tile8.GroundUnit.GetUnitDangerValue()));
						}
					}
					int num22 = 0;
					for (int num23 = 0; num23 < tile6.neighbours.Count; num23++)
					{
						if (tile6.neighbours[num23] != null)
						{
							num22++;
						}
					}
					int damageModificator = tile6.DamageModificator;
					if (num18 > num13 || (num18 == num13 && num20 > num14) || (num18 == num13 && num20 == num14 && damageModificator > num15) || (num18 == num13 && num20 == num14 && damageModificator == num15 && num22 > num16))
					{
						num13 = num18;
						num14 = num20;
						num15 = damageModificator;
						num16 = num22;
						tile5 = tile6;
					}
				}
				if (tile5 == null)
				{
					break;
				}
				Tile tile9 = tile5;
				if (tile5.GroundUnit == null || tile5.GroundUnit.OwnerPlayer != p_player)
				{
					int damageModificator2 = tile5.DamageModificator;
					for (int num24 = 0; num24 < tile5.neighbours.Count; num24++)
					{
						Tile tile10 = tile5.neighbours[num24];
						if (tile10 != null && tile10.enumType != TileTypes.RIVER && tile10.enumType != TileTypes.MOUNTAIN && tile10.enumType != TileTypes.SEA && tile10.terrainOwner == p_player && hashSet.Contains(tile10) && (hashSet3.Contains(tile10) || tile10 == tile5) && (tile10.GroundUnit == null || tile10.GroundUnit.OwnerPlayer == p_player) && tile10.DamageModificator > damageModificator2)
						{
							damageModificator2 = tile10.DamageModificator;
							tile9 = tile10;
						}
					}
				}
				tilesToGuard.Add(tile9);
				num12++;
				hashSet3.Remove(tile9);
				hashSet3.Remove(tile5);
				for (int num25 = 0; num25 < tile5.neighbours.Count; num25++)
				{
					Tile tile11 = tile5.neighbours[num25];
					if (tile11 != null && hashSet2.Contains(tile11) && hashSet3.Contains(tile11))
					{
						hashSet3.Remove(tile11);
					}
				}
				if (tile9 == tile5)
				{
					continue;
				}
				for (int num26 = 0; num26 < tile9.neighbours.Count; num26++)
				{
					Tile tile12 = tile9.neighbours[num26];
					if (tile12 != null && hashSet2.Contains(tile12) && hashSet3.Contains(tile12))
					{
						hashSet3.Remove(tile12);
					}
				}
			}
		}
		if (_showBorderGuardDebug)
		{
			int count4 = tilesToGuard.Count;
			for (int num27 = 0; num27 < count4; num27++)
			{
				Tile tile13 = tilesToGuard[num27];
				if (!(tile13.tileGO == null))
				{
					_debugGuardedTiles.Add(new BorderDebugEntry
					{
						position = tile13.tileGO.transform.position,
						color = Color.yellow,
						label = num27 + 1 + "/" + count4
					});
				}
			}
			Debug.Log("[BorderGuard] " + _currPlayerCountry + " | Borders: " + borders.Count + " | Total border tiles: " + num2 + " | Tiles to guard: " + tilesToGuard.Count + " | Units available: " + count);
			for (int num28 = 0; num28 < borders.Count; num28++)
			{
				BorderData borderData = borders[num28];
				Debug.Log("  [" + num28 + "] " + borderData.OtherPlayer.Country + " | Ideology: " + borderData.OtherPlayer.Ideology + " | Danger: " + borderData.OtherPlayer.GetDangerLevel(p_player) + " | Units: " + borderData.OtherPlayer.ListOfUnits.Count + " | Border tiles: " + borderData.MyTilesCount);
			}
		}
		int totalGuardPositions = tilesToGuard.Count;
		HashSet<Unit> hashSet4 = new HashSet<Unit>();
		for (int num29 = tilesToGuard.Count - 1; num29 >= 0; num29--)
		{
			Tile tile14 = tilesToGuard[num29];
			if (tile14.GroundUnit != null && tile14.GroundUnit.OwnerPlayer == p_player)
			{
				hashSet4.Add(tile14.GroundUnit);
				actualGuards++;
				tilesToGuard.RemoveAt(num29);
			}
		}
		List<Unit> availableUnits = new List<Unit>();
		for (int num30 = 0; num30 < count; num30++)
		{
			Unit unit = list[num30];
			if (unit != null && !(unit.unitGO == null) && !(unit.unitGO.tileGO == null) && unit.ActionPoints != 0 && !hashSet4.Contains(unit))
			{
				availableUnits.Add(unit);
			}
		}
		int guardCount = tilesToGuard.Count;
		for (int g = 0; g < guardCount; g++)
		{
			p_value = (float)g / (float)guardCount;
			UpdateLoadingBar(p_value);
			Tile tile15 = tilesToGuard[g];
			if (tile15.GroundUnit != null)
			{
				continue;
			}
			if (availableUnits.Count == 0)
			{
				break;
			}
			while (Time.timeScale == 0f)
			{
				yield return _waitForEndOfFrame;
			}
			int num31 = -1;
			int num32 = int.MaxValue;
			for (int num33 = 0; num33 < availableUnits.Count; num33++)
			{
				Unit unit2 = availableUnits[num33];
				if (tile15.landAreaIndex == unit2.unitGO.tileGO.tile.landAreaIndex)
				{
					int heuristicDistance = Utils.GetHeuristicDistance(unit2.unitGO.tileGO.tile, tile15);
					if (heuristicDistance < num32)
					{
						num32 = heuristicDistance;
						num31 = num33;
					}
				}
			}
			if (num31 == -1)
			{
				continue;
			}
			if (num31 != 0)
			{
				Unit value = availableUnits[num31];
				availableUnits[num31] = availableUnits[0];
				availableUnits[0] = value;
			}
			for (int u = 0; u < availableUnits.Count; u++)
			{
				Unit unit3 = availableUnits[u];
				if (tile15.landAreaIndex != unit3.unitGO.tileGO.tile.landAreaIndex)
				{
					continue;
				}
				unit3.unitGO.SelectUnit(p_playAnimation: false);
				if (MapGO.isOver)
				{
					yield break;
				}
				if (unit3.unitGO.tileGO.tile != tile15)
				{
					unit3.unitGO.GetPathToTile(tile15, p_waypointPath: true);
					if (unit3.unitGO.currentPath.Count == 0)
					{
						continue;
					}
					_tileGOForMovement = null;
					unit3.unitGO.gameObject.SetActive(value: true);
					unit3.unitGO.AIpathToVP = new List<TileGO>(unit3.unitGO.currentPath);
					unit3.unitGO.ShowPotentialPaths();
					GetClickedTile(unit3.unitGO);
					if (_tileGOForMovement == null)
					{
						continue;
					}
					if (_canCameraFollowAI)
					{
						_cameraNeedFocusOnUnit = true;
						_distanceCamToUnit = float.MaxValue;
						while (ShouldAIWaitForFocusOnUnit(MapGO.selectedUnitGO))
						{
							yield return new WaitForSeconds(0.3f);
						}
					}
					unit3.unitGO.MoveUnitToTile(_tileGOForMovement);
					while (unit3.unitGO.isMoving)
					{
						yield return null;
					}
				}
				MapGO.instance.UnselectUnit();
				availableUnits.RemoveAt(u);
				actualGuards++;
				break;
			}
		}
		_numberOfBorderGuardsNeeded = Mathf.Max(0, totalGuardPositions - actualGuards);
		if (_showBorderGuardDebug)
		{
			Debug.Log("[BorderGuard] " + _currPlayerCountry + " | Ideal guards: " + idealGuards + " | Guard positions: " + totalGuardPositions + " | Actual guards: " + actualGuards + " | Guards needed: " + _numberOfBorderGuardsNeeded);
		}
		yield return null;
	}

	private List<Unit> GatherUnitsForBorderDefense(Player p_player)
	{
		List<Unit> list = new List<Unit>();
		foreach (Unit listOfUnit in p_player.ListOfUnits)
		{
			if (!listOfUnit.IsDead && !listOfUnit.IsReserve && !listOfUnit.IsDefensive && listOfUnit.MaxMP > 0 && !listOfUnit.IsCommander && !listOfUnit.IsPartisans && !listOfUnit.IsMissile && !listOfUnit.IsTrain && !listOfUnit.IsArmouredTrain && !(listOfUnit.unitGO == null) && !(listOfUnit.Type == "Plane") && !(listOfUnit.Type == "Boat") && !(listOfUnit.FilterType == "FilterArtillery") && !(listOfUnit.FilterType == "FilterAA") && !(listOfUnit.unitGO.tileGO == null) && !listOfUnit.unitGO.tileGO.tile.IsVictoryPoint && listOfUnit.unitGO.tileGO.tile.terrainOwner == p_player)
			{
				list.Add(listOfUnit);
			}
		}
		return list;
	}

	private void DoResupply(UnitGO p_unitGO, bool p_ignoreHP)
	{
		if (TurnManager.currPlayer.MoneyAmount <= 0 || !ValidSelectedUnitGO(p_unitGO) || p_unitGO.unit.ActionPoints < 2)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		float hPPercent = p_unitGO.unit.GetHPPercent(p_forDamagePurposes: false);
		if (p_unitGO.unit.Type == "Plane" && !p_unitGO.unit.IsParatrooper)
		{
			if (p_unitGO.unit.CurrAmmo < p_unitGO.unit.MaxAmmo)
			{
				flag3 = true;
			}
			if (p_unitGO.unit.CurrAutonomy < p_unitGO.unit.MaxAutonomy)
			{
				flag2 = true;
			}
			if (p_unitGO.unit.CurrHP < p_unitGO.unit.MaxHP && (hPPercent <= 0.5f || !CanDealDamage(p_unitGO) || (hPPercent < 0.8f && UnityEngine.Random.Range(0, p_unitGO.unit.MaxHP + 1) >= p_unitGO.unit.CurrHP)))
			{
				flag = true;
			}
		}
		else if (p_unitGO.unit.Type == "Boat")
		{
			if (p_unitGO.unit.CurrAmmo < p_unitGO.unit.MaxAmmo)
			{
				flag3 = true;
			}
			if (p_unitGO.unit.CurrAutonomy < p_unitGO.unit.MaxAutonomy)
			{
				flag2 = true;
			}
			if (p_unitGO.unit.CurrHP < p_unitGO.unit.MaxHP)
			{
				flag = true;
			}
		}
		else
		{
			if (p_unitGO.unit.CurrAmmo <= p_unitGO.unit.MaxAmmo / 2)
			{
				flag3 = true;
			}
			if (p_unitGO.unit.CurrAutonomy <= p_unitGO.unit.MaxAutonomy / 4)
			{
				flag2 = true;
			}
			if (p_unitGO.unit.CurrHP < p_unitGO.unit.MaxHP && (hPPercent <= 0.5f || (p_unitGO.tileGO.tile.IsVictoryPoint && hPPercent < 0.9f) || (!CanDealDamage(p_unitGO) && !p_unitGO.unit.IsCommander) || (hPPercent < 0.8f && UnityEngine.Random.Range(0, p_unitGO.unit.MaxHP + 1) >= p_unitGO.unit.CurrHP)))
			{
				flag = true;
			}
		}
		if (p_ignoreHP)
		{
			flag = false;
		}
		if (!(flag | flag3 | flag2))
		{
			return;
		}
		p_unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
		if (p_unitGO.unit.SupplyPercentage > 0f)
		{
			if (flag3)
			{
				p_unitGO.unit.ResupplyAmmo();
			}
			if (flag2)
			{
				p_unitGO.unit.ResupplyFuel();
			}
			if (flag)
			{
				p_unitGO.unit.Heal();
			}
		}
	}

	private void RepairOrBlowupBridge(UnitGO p_unitGO)
	{
		if (p_unitGO == null || p_unitGO.unit == null || p_unitGO.unit.IsDead || !p_unitGO.tileGO.tile.Bridge || p_unitGO.unit.HasAttacked || p_unitGO.unit.ActionPoints <= 0 || (!p_unitGO.unit.IsEngineers && !p_unitGO.unit.IsParatrooper && !p_unitGO.unit.IsPartisans))
		{
			return;
		}
		if (p_unitGO.tileGO.tile.IsBridgeDestroyed)
		{
			if (p_unitGO.unit.IsEngineers)
			{
				p_unitGO.tileGO.tile.IsBridgeDestroyed = false;
				p_unitGO.tileGO.tile.Name = p_unitGO.tileGO.tile.Name.Replace(" destroyed", string.Empty);
				p_unitGO.tileGO.SetSprite();
				p_unitGO.unit.HasAttacked = true;
				p_unitGO.unit.AddRemoveActionPoints(-1);
				p_unitGO.ManageUnitInfoContainer();
				if (!p_unitGO.tileGO.isInFogOfWar)
				{
					SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy building"));
				}
				MultiplayerManager.RPC("RPC_SyncTile", p_unitGO.tileGO.tile.GetCoord(), p_unitGO.tileGO.tile);
			}
		}
		else if (p_unitGO.unit.IsPartisans && (p_unitGO.tileGO.tile.terrainOwner == null || p_unitGO.tileGO.tile.terrainOwner.isAtWarWithCurrentPlayer))
		{
			p_unitGO.tileGO.tile.IsBridgeDestroyed = true;
			p_unitGO.tileGO.tile.Name += " destroyed";
			p_unitGO.tileGO.SetSprite();
			p_unitGO.unit.HasAttacked = true;
			p_unitGO.unit.AddRemoveActionPoints(-1);
			p_unitGO.ManageUnitInfoContainer();
			if (!p_unitGO.tileGO.isInFogOfWar)
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetAttackSound("Explosion1"));
			}
			MultiplayerManager.RPC("RPC_SyncTile", p_unitGO.tileGO.tile.GetCoord(), p_unitGO.tileGO.tile);
		}
	}

	private void Move(UnitGO p_unitGO)
	{
		_tileGOForMovement = null;
		PlayerSettings.Instance.IsQuickMovement = _backupQuickmovementRule;
		if (p_unitGO.unit.ActionPoints == 0)
		{
			return;
		}
		short num = 1;
		if (_hasCarefulPlanning)
		{
			num++;
		}
		if (p_unitGO.unit.IsRecon)
		{
			num++;
		}
		if ((p_unitGO.tileGO.tile.enumType != TileTypes.HARBOUR && (p_unitGO.unit.CurrMP == 0 || p_unitGO.unit.NumberOfTimesTheUnitMoved >= num)) || (p_unitGO.unit.IsEngineers && p_unitGO.tileGO.tile.IsBridgeDestroyed) || (p_unitGO.unit.IsPontoonBridge && p_unitGO.tileGO.tile.enumType == TileTypes.RIVER && !p_unitGO.tileGO.tile.isFrozen && p_unitGO.unit.OthersNeedUsAsBridge))
		{
			return;
		}
		if (p_unitGO.tileGO.tile.IsVictoryPoint && p_unitGO.unit.MaxMP > 0 && p_unitGO.tileGO.tile.enumType == TileTypes.HARBOUR && p_unitGO.unit.Type != "Plane" && p_unitGO.unit.FilterType != "FilterBoat" && !p_unitGO.unit.IsArmouredTrain)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (Tile neighbour in p_unitGO.tileGO.tile.neighbours)
			{
				if (neighbour.enumType == TileTypes.RIVER || neighbour.enumType == TileTypes.SEA)
				{
					continue;
				}
				if (neighbour.IsVictoryPoint && neighbour.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					num3++;
				}
				if (neighbour.tileGO.groundUnitGO != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer && !neighbour.tileGO.groundUnitGO.unit.IsArmouredTrain && neighbour.tileGO.groundUnitGO.unit.MaxMP > 0)
				{
					num2++;
					if (neighbour.IsVictoryPoint && neighbour.ownerPlayer.isAlliedWithCurrentPlayer)
					{
						num4++;
					}
				}
			}
			if (_canPerformLandings && p_unitGO.unit.NumberOfTimesTheUnitMoved < 2 && ((num3 == 0 && num2 > 0) || (num3 > 0 && num4 >= num3 && num2 > num4)) && !AreThereEnemiesAround(p_unitGO.tileGO, 3, p_includeAir: true, p_useFogOfWar: true))
			{
				if (!p_unitGO.unit.IsOnBoat)
				{
					p_unitGO.EmbarkDisembarkBoat(p_sendRPC: true);
				}
			}
			else if (p_unitGO.unit.IsOnBoat)
			{
				p_unitGO.EmbarkDisembarkBoat(p_sendRPC: true);
			}
		}
		_goingForRepair = false;
		p_unitGO.goal = GetTurnMovementGoal(p_unitGO);
		if (p_unitGO.goal == null)
		{
			if (ShouldSendUnitBackToReserve(p_unitGO))
			{
				_pathingCount = int.MaxValue;
				p_unitGO.unit.RedeployUnit(p_removeMoney: false, p_sendRPC: true);
			}
			return;
		}
		if (!_ignoreOrder && !p_unitGO.unit.IsSubmarine && !p_unitGO.unit.IsArmouredTrain && !p_unitGO.unit.IsOnBoat && p_unitGO.unit.Type != "Plane" && (!p_unitGO.goal.tile.IsVictoryPoint || (p_unitGO.goal.tile.IsVictoryPoint && p_unitGO.unit.FilterType == "FilterBoat") || (p_unitGO.goal.tile.IsVictoryPoint && !p_unitGO.goal.tile.isVisible)) && (p_unitGO.unit.IsCommander || p_unitGO.unit.IsSupplyDepot || p_unitGO.unit.IsCarrier || (p_unitGO.unit.FilterType == "FilterArtillery" && p_unitGO.unit.Range > 1) || (p_unitGO.unit.FilterType == "FilterBoat" && p_unitGO.unit.Range > 1) || p_unitGO.unit.IsRecon))
		{
			if (p_unitGO.unit.IsCommander || p_unitGO.unit.IsSupplyDepot || p_unitGO.unit.IsCarrier)
			{
				int range = p_unitGO.unit.Range;
				if (p_unitGO.unit.IsCarrier)
				{
					range = 6;
					if (Vector2.Distance(p_unitGO.tileGO.transform.position, p_unitGO.goal.transform.position) <= 6f * (float)range && !MapGO.IsThereACertainTileAround(p_unitGO.goal, "Harbour"))
					{
						return;
					}
				}
				else
				{
					foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, range))
					{
						if (_enemyVPsGO.Contains(item.tileGO) && (item.enumType != TileTypes.SEA || !(p_unitGO.unit.FilterType != "FilterBoat")))
						{
							return;
						}
					}
				}
			}
			else if (!_goingForRepair && (p_unitGO.unit.IsRecon || p_unitGO.unit.FilterType == "FilterArtillery" || p_unitGO.unit.FilterType == "FilterBoat") && ((!p_unitGO.goal.tile.isVisible && p_unitGO.unit.FilterType != "FilterBoat") || (p_unitGO.goal.tile.isVisible && p_unitGO.unit.FilterType == "FilterArtillery") || (p_unitGO.goal.tile.isVisible && p_unitGO.unit.FilterType == "FilterBoat")))
			{
				bool flag = p_unitGO.unit.Range > 1 && p_unitGO.unit.FilterType == "FilterArtillery";
				if (flag && CanDealDamage(p_unitGO))
				{
					p_unitGO.goal = p_unitGO.tileGO;
					return;
				}
				TileGO tileGO = null;
				float num5 = float.MaxValue;
				int p_radius = (p_unitGO.unit.IsRecon ? p_unitGO.unit.Visibility : p_unitGO.unit.Range);
				foreach (Tile item2 in Utils.GetAllTilesInRadiusAround(p_unitGO.goal.tile, p_radius))
				{
					if (!p_unitGO.listOfpotentialTiles.Contains(item2.tileGO) || item2.tileGO.groundUnitGO != null || (flag && AreThereEnemiesAround(item2.tileGO, 1, p_includeAir: false, p_useFogOfWar: false)))
					{
						continue;
					}
					float num6 = Vector2.Distance(item2.tileGO.transform.position, p_unitGO.goal.transform.position);
					if (num6 > 6f)
					{
						num6 += Vector2.Distance(p_unitGO.tileGO.transform.position, item2.tileGO.transform.position);
						if (num6 < num5)
						{
							tileGO = item2.tileGO;
							num5 = Vector2.Distance(item2.tileGO.transform.position, p_unitGO.transform.position);
						}
					}
				}
				if (tileGO != null)
				{
					p_unitGO.goal = tileGO;
				}
			}
		}
		DeclareWarOnCountry(p_unitGO, p_unitGO.goal);
		p_unitGO.gameObject.SetActive(value: true);
		GetPathToGoal(p_unitGO, RunTimes.FIRST);
		if (!ValidSelectedUnitGO(p_unitGO))
		{
			return;
		}
		if (p_unitGO.unit.Type != "Boat" && p_unitGO.unit.Type != "Plane" && PlayerSettings.Instance.WinterDamageValue > 0 && p_unitGO.unit.CurrHP - p_unitGO.currentPath.Count * PlayerSettings.Instance.WinterDamageValue <= 0 && (p_unitGO.tileGO.tile.enumSeason == TileSeason.WINTER || p_unitGO.goal.tile.enumSeason == TileSeason.WINTER))
		{
			DoResupply(p_unitGO, p_ignoreHP: false);
			return;
		}
		if (ShouldSendUnitBackToReserve(p_unitGO))
		{
			_pathingCount = int.MaxValue;
			p_unitGO.unit.RedeployUnit(p_removeMoney: false, p_sendRPC: true);
			return;
		}
		if (p_unitGO.unit.shouldPerformDirectSwap && p_unitGO.unit.swapeeUnitID != 0)
		{
			foreach (Tile neighbour2 in p_unitGO.tileGO.tile.neighbours)
			{
				if (neighbour2.GroundUnit != null && p_unitGO.unit.swapeeUnitID == neighbour2.GroundUnit.ID && neighbour2.GroundUnit.swapeeUnitID == p_unitGO.unit.ID && p_unitGO.CanSwapUnits(neighbour2.GroundUnit.unitGO))
				{
					p_unitGO.SwapUnits(neighbour2.GroundUnit.unitGO);
					return;
				}
			}
		}
		GetClickedTile(p_unitGO);
	}

	private TileGO GetTurnMovementGoal(UnitGO p_unitGO)
	{
		_VPGOfromNeighbourGoal = null;
		if (_lastUnitIDusingUnreachable != p_unitGO.unit.ID)
		{
			for (short num = 0; num < _mapSizeX; num++)
			{
				for (short num2 = 0; num2 < _mapSizeY; num2++)
				{
					_tiles[num, num2].unreachable = false;
				}
			}
		}
		p_unitGO.ShowPotentialPaths();
		if (p_unitGO.unit.Type == "Plane")
		{
			if (!p_unitGO.unit.IsParatrooper && !p_unitGO.unit.IsMissile && !p_unitGO.unit.HasAttacked)
			{
				_allAirportAndTheirDistance.Clear();
				Vector3 position = p_unitGO.tileGO.transform.position;
				foreach (TileGO victoryPoint in MapGO.victoryPoints)
				{
					if (victoryPoint.tile.unreachable || (victoryPoint.tile.enumType != TileTypes.AIRPORT && !victoryPoint.tile.IsReinforcementTile) || victoryPoint.tile.NumberOfTurnsAtLowProd > 0 || !victoryPoint.tile.ownerPlayer.isAlliedWithCurrentPlayer)
					{
						continue;
					}
					if (victoryPoint.airUnitGO == null)
					{
						if (!_allAirportAndTheirDistance.ContainsKey(victoryPoint))
						{
							_allAirportAndTheirDistance.Add(victoryPoint, Vector2.Distance(position, victoryPoint.transform.position));
						}
						continue;
					}
					foreach (Tile neighbour in victoryPoint.tile.neighbours)
					{
						if (neighbour.tileGO.airUnitGO == null && (neighbour.terrainOwner == null || !neighbour.terrainOwner.noRelationWithCurrentPlayer) && !_allAirportAndTheirDistance.ContainsKey(neighbour.tileGO))
						{
							_allAirportAndTheirDistance.Add(neighbour.tileGO, Vector2.Distance(position, neighbour.tileGO.transform.position));
						}
					}
				}
				if (p_unitGO.unit.CanBeRefueledOnCarrier)
				{
					foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
					{
						if (listOfUnit.IsCarrier && !listOfUnit.IsDead && (bool)listOfUnit.unitGO && !_allAirportAndTheirDistance.ContainsKey(listOfUnit.unitGO.tileGO))
						{
							_allAirportAndTheirDistance.Add(listOfUnit.unitGO.tileGO, Vector2.Distance(position, listOfUnit.unitGO.tileGO.transform.position));
						}
					}
				}
				if (_allAirportAndTheirDistance.Count > 0)
				{
					_allAirportAndTheirDistance = _allAirportAndTheirDistance.OrderBy((KeyValuePair<TileGO, float> x) => x.Value).ToDictionary((KeyValuePair<TileGO, float> x) => x.Key, (KeyValuePair<TileGO, float> x) => x.Value);
					float hPPercent = p_unitGO.unit.GetHPPercent(p_forDamagePurposes: false);
					bool flag = false;
					if (TurnManager.isBadWeather)
					{
						if (p_unitGO.unit.CurrAmmo < p_unitGO.unit.MaxAmmo || p_unitGO.unit.CurrAutonomy < p_unitGO.unit.MaxAutonomy / 2 || hPPercent <= 0.75f)
						{
							flag = true;
						}
					}
					else if (hPPercent <= 0.5f || _allAirportAndTheirDistance.ElementAt(0).Value + (float)(int)p_unitGO.unit.CurrMP * 5.2f > (float)(p_unitGO.unit.CurrAutonomy * 6) || (p_unitGO.unit.CurrAmmo == 0 && p_unitGO.unit.MaxAmmo > 0))
					{
						flag = true;
					}
					if (flag)
					{
						_goingForRepair = true;
						return _allAirportAndTheirDistance.ElementAt(0).Key;
					}
				}
			}
		}
		else if (p_unitGO.unit.Type == "Boat")
		{
			bool flag2 = false;
			foreach (TileGO victoryPoint2 in MapGO.victoryPoints)
			{
				if (victoryPoint2.tile.enumType == TileTypes.HARBOUR && victoryPoint2.tile.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				Queue<Tile> queue = new Queue<Tile>();
				int num3 = 0;
				for (short num4 = 0; num4 < _mapSizeX; num4++)
				{
					for (short num5 = 0; num5 < _mapSizeY; num5++)
					{
						_tiles[num4, num5].SetupForAStar();
					}
				}
				Tile tile = p_unitGO.tileGO.tile;
				tile.cost = 0;
				queue.Enqueue(tile);
				num3++;
				Tile tile2 = null;
				Unit unit = p_unitGO.unit;
				bool isConvoy = unit.IsConvoy;
				bool isPatrolBoat = unit.IsPatrolBoat;
				while (num3 > 0)
				{
					Tile tile3 = queue.Dequeue();
					num3--;
					tile3.OnQueue = false;
					for (byte b = 0; b < tile3.amountOfNeighbours; b++)
					{
						int num6 = 0;
						bool flag3 = true;
						_neighbour = tile3.neighbours[b];
						num6 += _neighbour.MPCost;
						if (_neighbour.enumType != TileTypes.SEA)
						{
							if (isPatrolBoat)
							{
								if (_neighbour.enumType == TileTypes.HARBOUR || _neighbour.enumType == TileTypes.CITY || _neighbour.enumType == TileTypes.RIVER || _neighbour.Bridge)
								{
									if (!_neighbour.hasOwnerPlayer || !_neighbour.terrainOwner.isAlliedWithCurrentPlayer)
									{
										flag3 = false;
									}
								}
								else
								{
									flag3 = false;
								}
							}
							else if (_neighbour.enumType == TileTypes.HARBOUR)
							{
								if (!_neighbour.hasOwnerPlayer || !_neighbour.terrainOwner.isAlliedWithCurrentPlayer)
								{
									flag3 = false;
								}
							}
							else
							{
								flag3 = false;
							}
						}
						if (_neighbour.isVisible && _neighbour.GroundUnit != null && !_neighbour.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
						{
							flag3 = false;
						}
						int num7 = tile3.cost + num6;
						if ((num7 < _neighbour.cost) & flag3)
						{
							_neighbour.cost = num7;
							_neighbour.parent = tile3;
							if (!_neighbour.OnQueue)
							{
								queue.Enqueue(_neighbour);
								_neighbour.OnQueue = true;
								num3++;
								if ((_neighbour.enumType == TileTypes.HARBOUR || ((_neighbour.enumType == TileTypes.CITY) & isPatrolBoat)) && _neighbour.hasOwnerPlayer && _neighbour.ownerPlayer.isAlliedWithCurrentPlayer && (!isConvoy || !(_neighbour.OwnerName == unit.OwnerName)))
								{
									foreach (Tile neighbour2 in _neighbour.neighbours)
									{
										if ((neighbour2.enumType == TileTypes.SEA || ((neighbour2.enumType == TileTypes.RIVER) & isPatrolBoat)) && (neighbour2.GroundUnit == null || !neighbour2.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer || (!(neighbour2.GroundUnit.GetHPPercent(p_forDamagePurposes: false) < 0.9f) && (neighbour2.GroundUnit.CurrAmmo != 0 || neighbour2.GroundUnit.IsCommander))))
										{
											if (tile2 != null && neighbour2.cost < tile2.cost)
											{
												tile2 = neighbour2;
											}
											if (tile2 == null)
											{
												tile2 = neighbour2;
											}
										}
									}
								}
							}
						}
					}
				}
				if (tile2 != null)
				{
					if (isConvoy)
					{
						_goingForRepair = true;
						return tile2.tileGO;
					}
					if (unit.GetHPPercent(p_forDamagePurposes: false) < 0.6f || (unit.CurrAmmo == 0 && !unit.IsCommander) || (tile2.cost + 10 > unit.CurrAutonomy && unit.CurrAutonomy < unit.MaxAutonomy))
					{
						bool flag4 = false;
						foreach (Tile neighbour3 in p_unitGO.tileGO.tile.neighbours)
						{
							if (neighbour3.Capturable && neighbour3.IsVictoryPoint && neighbour3.ownerPlayer != null && neighbour3.ownerPlayer.isAtWarWithCurrentPlayer && neighbour3.GroundUnit == null && (neighbour3.enumType == TileTypes.SEA || p_unitGO.unit.FilterType != "FilterBoat"))
							{
								flag4 = true;
								break;
							}
						}
						if (!flag4)
						{
							_goingForRepair = true;
							return tile2.tileGO;
						}
					}
				}
			}
		}
		_enemyVPsByDistance.Clear();
		bool isRecon = p_unitGO.unit.IsRecon;
		bool flag5 = p_unitGO.unit.FilterType == "FilterBoat";
		bool flag6 = p_unitGO.unit.Type == "Plane";
		bool isDropable = p_unitGO.unit.IsDropable;
		bool flag7 = _currPlayerCountry == "USA" || _currPlayerCountry == "Great Britain";
		DateTime dateTime = new DateTime(1943, 7, 1);
		DateTime dateTime2 = new DateTime(1944, 6, 1);
		Vector3 position2 = p_unitGO.tileGO.transform.position;
		HashSet<short> hashSet = new HashSet<short>(_ourLandMassesIndexes);
		if (p_unitGO.tileGO.tile.landAreaIndex.HasValue)
		{
			hashSet.Add(p_unitGO.tileGO.tile.landAreaIndex.Value);
		}
		foreach (TileGO item in _enemyVPsGO)
		{
			if (item.tile.unreachable || (!flag5 && item.tile.enumType == TileTypes.SEA && (!flag6 || !isRecon)) || (flag6 && _isHistoricalFocusON && _isEuropeScenario && _currYear >= 1941 && _currYear <= 1943 && item.tile.landAreaIndex.HasValue && !hashSet.Contains(item.tile.landAreaIndex.Value) && item.tile.OriginalCountryOwner == "Great Britain") || (!flag5 && (!flag6 | isDropable) && item.tile.landAreaIndex.HasValue && !hashSet.Contains(item.tile.landAreaIndex.Value) && (!_canPerformLandings || (_isHistoricalFocusON && _isOfficialMapWithLandingControl && item.tile.OriginalCountryOwner == "Great Britain"))))
			{
				continue;
			}
			float value = Vector2.Distance(item.transform.position, position2);
			if (_isHistoricalFocusON && _isEuropeScenario && _canPerformLandings && !flag5 && (!flag6 | isDropable) && flag7 && _currDate < dateTime2 && item.tile.OriginalCountryOwner != _currPlayerCountry)
			{
				if (p_unitGO.tileGO.tile.PosY >= 112 && item.tile.PosY <= 112)
				{
					continue;
				}
				if (_currDate >= dateTime)
				{
					if (item.tile.OriginalCountryOwner != "Italy" && item.tile.ownerPlayer.Country != "Vichy France" && item.tile.ownerPlayer.Country != "Iceland")
					{
						continue;
					}
				}
				else if (item.tile.enumSeason != TileSeason.DESERT && item.tile.ownerPlayer.Country != "Iceland")
				{
					continue;
				}
			}
			_enemyVPsByDistance.Add(item, value);
		}
		_enemyVPsByDistance = (from x in _enemyVPsByDistance
			orderby x.Value, x.Key.tile.PrimaryObjective descending
			select x).ToDictionary((KeyValuePair<TileGO, float> x) => x.Key, (KeyValuePair<TileGO, float> x) => x.Value);
		if (p_unitGO.unit.FilterType == "FilterBoat")
		{
			_dicOfUnitsWeCanKill.Clear();
			_dicGroundDamage.Clear();
			_carrierDamage.Clear();
			_landingCraftsDamage.Clear();
			_friendliesToEscort.Clear();
			_listOfSonarSignals.Clear();
			_submarineDamage.Clear();
			bool flag8 = p_unitGO.unit.IsCarrier || p_unitGO.unit.GetNameToLower().Contains("battle");
			TileGO tileGO = null;
			foreach (TileGO item2 in p_unitGO.listOfpotentialTiles.OrderByDescending((TileGO x) => x.tile.MoneyProductionPerTurn))
			{
				if (item2.tile.unreachable)
				{
					continue;
				}
				if (item2.tile.IsVictoryPoint && _enemyVPsByDistance.ContainsKey(item2) && item2.tile.GroundUnit == null)
				{
					return item2;
				}
				foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(item2.tile, p_unitGO.unit.Range))
				{
					if (item3.unreachable)
					{
						continue;
					}
					if (MapGO.sonarDetections.Contains(item3.tileGO) && item3.GroundUnit != null && item3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && !_listOfSonarSignals.Contains(item3.tileGO))
					{
						_listOfSonarSignals.Add(item3.tileGO);
					}
					if (!item3.isVisible || item3.tileGO.groundUnitGO == null || item3.tileGO == p_unitGO.tileGO)
					{
						continue;
					}
					if (item3.tileGO.groundUnitGO.unit.IsSubmarine)
					{
						if (item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && !_submarineDamage.ContainsKey(item3.tileGO))
						{
							p_unitGO.unit.GetPotentialDamage(item3.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
							_submarineDamage.Add(item3.tileGO, p_unitGO.unit.FinalDamage);
						}
					}
					else if (item3.tileGO.groundUnitGO.unit.IsCarrier || item3.tileGO.groundUnitGO.unit.IsConvoy || item3.tileGO.groundUnitGO.unit.GetNameToLower().Contains("battle"))
					{
						if (item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
						{
							if (!flag8 && !_friendliesToEscort.Contains(item3.tileGO))
							{
								_friendliesToEscort.Add(item3.tileGO);
							}
						}
						else if (item3.tileGO.groundUnitGO.unit.IsCarrier && item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && !_carrierDamage.ContainsKey(item3.tileGO))
						{
							p_unitGO.unit.GetPotentialDamage(item3.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
							_carrierDamage.Add(item3.tileGO, p_unitGO.unit.FinalDamage);
						}
					}
					else if (item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						p_unitGO.unit.GetPotentialDamage(item3.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
						if (item3.tileGO.groundUnitGO.unit.IsOnBoat)
						{
							if (!_landingCraftsDamage.ContainsKey(item3.tileGO))
							{
								_landingCraftsDamage.Add(item3.tileGO, p_unitGO.unit.FinalDamage);
							}
						}
						else if (!_dicGroundDamage.ContainsKey(item3.tileGO))
						{
							_dicGroundDamage.Add(item3.tileGO, p_unitGO.unit.FinalDamage);
						}
					}
					if (item3.tileGO.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0 && item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && !_dicOfUnitsWeCanKill.ContainsKey(item3.tileGO.groundUnitGO))
					{
						_dicOfUnitsWeCanKill.Add(item3.tileGO.groundUnitGO, item3.tileGO.groundUnitGO.unit.Cost);
					}
				}
			}
			_dicOfUnitsWeCanKill = _dicOfUnitsWeCanKill.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Key.unit.Cost).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
			foreach (KeyValuePair<UnitGO, int> item4 in _dicOfUnitsWeCanKill)
			{
				foreach (Tile item5 in Utils.GetAllTilesInRadiusAround(item4.Key.tileGO.tile, p_unitGO.unit.Range))
				{
					if (p_unitGO.listOfpotentialTiles.Contains(item5.tileGO))
					{
						return item5.tileGO;
					}
				}
			}
			if (p_unitGO.unit.IsKillGoal)
			{
				TileGO furthestWalkableTileFromEnemy = GetFurthestWalkableTileFromEnemy(p_unitGO, 10);
				if (furthestWalkableTileFromEnemy != null)
				{
					_ignoreOrder = true;
					return furthestWalkableTileFromEnemy;
				}
			}
			if (p_unitGO.unit.IsDestroyer)
			{
				_submarineDamage = _submarineDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
				foreach (KeyValuePair<TileGO, int> item6 in _submarineDamage)
				{
					if (item6.Value == 0)
					{
						break;
					}
					foreach (Tile item7 in Utils.GetAllTilesInRadiusAround(item6.Key.tile, p_unitGO.unit.Range))
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item7.tileGO))
						{
							return item7.tileGO;
						}
					}
				}
			}
			if (!p_unitGO.unit.IsCarrier)
			{
				_carrierDamage = _carrierDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
				foreach (KeyValuePair<TileGO, int> item8 in _carrierDamage)
				{
					if (item8.Value == 0)
					{
						break;
					}
					foreach (Tile item9 in Utils.GetAllTilesInRadiusAround(item8.Key.tile, p_unitGO.unit.Range))
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item9.tileGO))
						{
							return item9.tileGO;
						}
					}
				}
			}
			if (p_unitGO.unit.IsSubmarine)
			{
				_landingCraftsDamage = _landingCraftsDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
				foreach (KeyValuePair<TileGO, int> item10 in _landingCraftsDamage)
				{
					if (item10.Value == 0)
					{
						break;
					}
					foreach (Tile item11 in Utils.GetAllTilesInRadiusAround(item10.Key.tile, p_unitGO.unit.Range))
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item11.tileGO))
						{
							return item11.tileGO;
						}
					}
				}
				_enemyVPsByDistance = (from x in _enemyVPsByDistance
					orderby x.Key.tile.enumType == TileTypes.SEA descending, x.Value
					select x).ToDictionary((KeyValuePair<TileGO, float> x) => x.Key, (KeyValuePair<TileGO, float> x) => x.Value);
			}
			if (!p_unitGO.unit.IsCarrier && !p_unitGO.unit.IsConvoy)
			{
				_ = p_unitGO.unit.IsDestroyer;
				bool isSubmarine = p_unitGO.unit.IsSubmarine;
				_dicGroundDamage = _dicGroundDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
				foreach (KeyValuePair<TileGO, int> item12 in _dicGroundDamage)
				{
					if (item12.Value == 0)
					{
						break;
					}
					if (isSubmarine && item12.Key.groundUnitGO != null && item12.Key.groundUnitGO.unit.IsDestroyer && item12.Key.groundUnitGO.unit.GetHPPercent(p_forDamagePurposes: false) > 0.5f)
					{
						continue;
					}
					if (item12.Key.groundUnitGO.unit.Type != "Boat" && (float)item12.Value / (float)item12.Key.groundUnitGO.unit.CurrHP < 0.1f)
					{
						if (!(tileGO == null))
						{
							continue;
						}
						foreach (Tile item13 in Utils.GetAllTilesInRadiusAround(item12.Key.tile, p_unitGO.unit.Range))
						{
							if (p_unitGO.listOfpotentialTiles.Contains(item13.tileGO))
							{
								tileGO = item13.tileGO;
								break;
							}
						}
						continue;
					}
					foreach (Tile item14 in Utils.GetAllTilesInRadiusAround(item12.Key.tile, p_unitGO.unit.Range))
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item14.tileGO))
						{
							return item14.tileGO;
						}
					}
				}
				foreach (TileGO listOfSonarSignal in _listOfSonarSignals)
				{
					if (listOfSonarSignal.tile.GroundUnit == null || !listOfSonarSignal.tile.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						continue;
					}
					foreach (Tile item15 in Utils.GetAllTilesInRadiusAround(listOfSonarSignal.tile, p_unitGO.unit.Range))
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item15.tileGO) && item15.GroundUnit == null)
						{
							return item15.tileGO;
						}
					}
				}
				_friendliesToEscort = _friendliesToEscort.OrderByDescending((TileGO x) => x.groundUnitGO.unit.IsCarrier).ToList();
				foreach (TileGO item16 in _friendliesToEscort)
				{
					if (isRecon)
					{
						int num8 = 0;
						int num9 = 0;
						TileGO tileGO2 = null;
						foreach (Tile neighbour4 in item16.tile.neighbours)
						{
							if (neighbour4.enumType != TileTypes.SEA || neighbour4.GroundUnit == null)
							{
								continue;
							}
							if (neighbour4.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								if (neighbour4.GroundUnit.IsSubmarine)
								{
									num9++;
									if (num9 >= 1)
									{
										tileGO2 = null;
										break;
									}
								}
								else if (neighbour4.GroundUnit.IsDestroyer)
								{
									num8++;
									if (num8 >= 2)
									{
										tileGO2 = null;
										break;
									}
								}
							}
							tileGO2 = neighbour4.tileGO;
						}
						if (tileGO2 != null)
						{
							return tileGO2;
						}
						continue;
					}
					foreach (Tile neighbour5 in item16.tile.neighbours)
					{
						if (neighbour5.enumType == TileTypes.SEA && neighbour5.tileGO.groundUnitGO == null && p_unitGO.listOfpotentialTiles.Contains(neighbour5.tileGO))
						{
							return neighbour5.tileGO;
						}
					}
				}
			}
			if (_enemyVPsByDistance.Count > 0)
			{
				_enemyVPsByDistance = _enemyVPsByDistance.OrderByDescending((KeyValuePair<TileGO, float> x) => x.Key.tile.enumType == TileTypes.SEA && x.Key.tile.Capturable).ToDictionary((KeyValuePair<TileGO, float> x) => x.Key, (KeyValuePair<TileGO, float> x) => x.Value);
				for (int num10 = 0; num10 < 2; num10++)
				{
					foreach (KeyValuePair<TileGO, float> item17 in _enemyVPsByDistance)
					{
						TileGO tileGO3 = null;
						bool flag9 = false;
						if (item17.Key.tile.unreachable || item17.Key.tile.isFrozen)
						{
							continue;
						}
						if (num10 == 0)
						{
							if (item17.Key.tile.Capturable && item17.Key.tile.enumType == TileTypes.SEA)
							{
								if (!item17.Key.tile.isVisible || item17.Key.groundUnitGO == null)
								{
									_VPGOfromNeighbourGoal = item17.Key;
									return item17.Key;
								}
							}
							else
							{
								if (item17.Key.tile.enumType != TileTypes.HARBOUR || item17.Key.tile.GroundUnit == null)
								{
									continue;
								}
								foreach (Tile item18 in Utils.GetAllTilesInRadiusAround(item17.Key.tile, p_unitGO.unit.Range))
								{
									if (item18.isFrozen)
									{
										continue;
									}
									if (item18.isVisible)
									{
										if (item18.tileGO.groundUnitGO != null && item18.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
										{
											tileGO3 = item18.tileGO;
										}
									}
									else if (item18.enumType == TileTypes.SEA)
									{
										flag9 = true;
										tileGO3 = item18.tileGO;
									}
									if ((tileGO3 != null) & flag9)
									{
										_VPGOfromNeighbourGoal = item17.Key;
										return tileGO3;
									}
								}
							}
						}
						else
						{
							if (item17.Key.tile.GroundUnit == null)
							{
								continue;
							}
							foreach (Tile item19 in Utils.GetAllTilesInRadiusAround(item17.Key.tile, p_unitGO.unit.Range))
							{
								if (item19.unreachable || item19.isFrozen)
								{
									continue;
								}
								if (item19.isVisible)
								{
									if ((item17.Key.tile.enumType == TileTypes.SEA || item19.enumType == TileTypes.SEA) && item19.tileGO.groundUnitGO == null)
									{
										flag9 = true;
										tileGO3 = item17.Key;
									}
									else if (item19.tileGO.groundUnitGO != null && item19.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
									{
										tileGO3 = item19.tileGO;
									}
								}
								else if (item17.Key.tile.enumType == TileTypes.SEA)
								{
									flag9 = true;
									tileGO3 = item17.Key;
								}
								else if (item19.enumType == TileTypes.SEA)
								{
									flag9 = true;
									tileGO3 = item19.tileGO;
								}
								if ((tileGO3 != null) & flag9)
								{
									_VPGOfromNeighbourGoal = item17.Key;
									return tileGO3;
								}
							}
						}
					}
				}
				return _enemyVPsByDistance.ElementAt(0).Key;
			}
			if (tileGO != null)
			{
				return tileGO;
			}
		}
		else if (!p_unitGO.unit.IsParatrooper && p_unitGO.unit.Type == "Plane")
		{
			if (p_unitGO.unit.FilterType == "FilterFighter")
			{
				int num11 = 0;
				int num12 = 0;
				int num13 = 0;
				int num14 = 0;
				_dicOfUnitsWeCanKill.Clear();
				_dicBomberDamage.Clear();
				_dicUnescortedBomberDamage.Clear();
				_dicFighterDamage.Clear();
				_dicGroundDamage.Clear();
				_dicBuildingsToProtect.Clear();
				_friendliesToEscort.Clear();
				_paratroopersToEscort.Clear();
				_friendlyBombers.Clear();
				_radarSignals.Clear();
				foreach (Tile item20 in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_unitGO.unit.CurrMP))
				{
					if (item20.unreachable || (item20.hasTerrainOwnerPlayer && item20.terrainOwner.noRelationWithCurrentPlayer))
					{
						continue;
					}
					if (MapGO.radarDetections.Contains(item20.tileGO) && item20.AirUnit != null && item20.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						_radarSignals.Add(item20.tileGO);
					}
					if (!item20.isVisible)
					{
						continue;
					}
					if (item20.enumType != TileTypes.SEA && item20.IsVictoryPoint && item20.MoneyProductionPerTurn > 5 && item20.NumberOfTurnsAtLowProd == 0 && item20.ownerPlayer != null && item20.ownerPlayer.isAlliedWithCurrentPlayer)
					{
						_dicBuildingsToProtect.Add(item20.tileGO, item20.MoneyProductionPerTurn);
					}
					if (item20.tileGO.airUnitGO != null)
					{
						bool flag10 = item20.tileGO.airUnitGO.unit.FilterType == "FilterFighter";
						if (item20.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
						{
							if (item20.tileGO.airUnitGO.unit.FilterType == "FilterBomber")
							{
								num12++;
								num13 += item20.tileGO.airUnitGO.unit.CurrHP;
							}
							p_unitGO.unit.GetPotentialDamage(item20.tileGO.airUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
							if (CanPlaneAttackSafely(p_unitGO, item20, p_unitGO.unit.FinalDamage))
							{
								if (flag10)
								{
									_dicFighterDamage.Add(item20.tileGO, p_unitGO.unit.FinalDamage);
								}
								else
								{
									_dicBomberDamage.Add(item20.tileGO, p_unitGO.unit.FinalDamage);
									if (!IsPlaneEscorted(item20))
									{
										_dicUnescortedBomberDamage.Add(item20.tileGO, p_unitGO.unit.FinalDamage);
									}
								}
								if (item20.tileGO.airUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0 && !_dicOfUnitsWeCanKill.ContainsKey(item20.tileGO.airUnitGO))
								{
									_dicOfUnitsWeCanKill.Add(item20.tileGO.airUnitGO, item20.tileGO.airUnitGO.unit.Cost);
								}
							}
						}
						else if (item20.tileGO.airUnitGO.unit.MaxMP > 0 && item20.tileGO.airUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
						{
							if (flag10)
							{
								if (!item20.tileGO.airUnitGO.unit.HasAttacked)
								{
									num11++;
									num14 += Mathf.RoundToInt((float)item20.tileGO.airUnitGO.unit.AirDamage * item20.tileGO.airUnitGO.unit.GetHPPercent(p_forDamagePurposes: true));
								}
							}
							else if (item20.tileGO.airUnitGO.unit.IsParatrooper)
							{
								_paratroopersToEscort.Add(item20.tileGO);
							}
							else
							{
								_friendlyBombers.Add(item20.tileGO);
							}
						}
					}
					if (!(item20.tileGO.groundUnitGO != null))
					{
						continue;
					}
					if (item20.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						if (item20.tileGO.groundUnitGO.unit.IsMine)
						{
							continue;
						}
						p_unitGO.unit.GetPotentialDamage(item20.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
						if (CanPlaneAttackSafely(p_unitGO, item20, p_unitGO.unit.FinalDamage))
						{
							_dicGroundDamage.Add(item20.tileGO, p_unitGO.unit.FinalDamage);
							if (item20.tileGO.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0 && !_dicOfUnitsWeCanKill.ContainsKey(item20.tileGO.groundUnitGO))
							{
								_dicOfUnitsWeCanKill.Add(item20.tileGO.groundUnitGO, item20.tileGO.groundUnitGO.unit.Cost);
							}
						}
					}
					else if ((item20.tileGO.groundUnitGO.unit.IsCarrier || item20.tileGO.groundUnitGO.unit.IsConvoy) && item20.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
					{
						_friendliesToEscort.Add(item20.tileGO);
					}
				}
				if (p_unitGO.unit.HasAttacked)
				{
					List<Tile> list;
					if (p_unitGO.unit.HasMoved)
					{
						list = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 1);
					}
					else
					{
						list = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_unitGO.unit.CurrMP);
						list = list.OrderByDescending((Tile x) => Vector2.Distance(x.tileGO.transform.position, p_unitGO.tileGO.transform.position)).ToList();
					}
					for (byte b2 = 0; b2 <= 4; b2++)
					{
						TileGO tileGO4 = null;
						int num15 = 0;
						foreach (Tile item21 in list)
						{
							if ((item21.tileGO.airUnitGO != null && item21.tileGO != p_unitGO.tileGO) || item21.unreachable || (item21.terrainOwner != null && item21.terrainOwner.noRelationWithCurrentPlayer))
							{
								continue;
							}
							switch (b2)
							{
							case 0:
							{
								if (item21.GroundUnit != null && !item21.GroundUnit.IsMine && item21.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									break;
								}
								bool flag14 = false;
								foreach (Tile item22 in Utils.GetAllTilesInRadiusAround(item21, 1))
								{
									if (item22.isVisible)
									{
										if (item22.AirUnit != null && item22.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item22.AirUnit.FilterType == "FilterFighter" && item22.AirUnit.CurrHP > item22.AirUnit.MaxHP / 2)
										{
											flag14 = true;
											break;
										}
										if (item22.GroundUnit != null && item22.tileGO != p_unitGO.tileGO && item22.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item22.GroundUnit.FilterType == "FilterAA" && item22.GroundUnit.CurrHP > item22.GroundUnit.MaxHP / 2)
										{
											flag14 = true;
											break;
										}
									}
								}
								if (flag14)
								{
									break;
								}
								int num17 = 0;
								bool flag15 = false;
								foreach (Tile item23 in Utils.GetAllTilesInRadiusAround(item21, 1))
								{
									if (item23.GroundUnit != null && !item23.GroundUnit.IsMine && item23.GroundUnit.FilterType != "FilterAA" && item23.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
									{
										num17++;
										flag15 = true;
									}
									if (item23.AirUnit != null && item23.AirUnit.FilterType != "FilterFighter" && item23.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
									{
										num17++;
									}
								}
								if (num17 != 0 && (num17 > num15 || ((num17 == num15) & flag15)))
								{
									num15 = num17;
									tileGO4 = item21.tileGO;
								}
								break;
							}
							case 1:
							{
								bool flag12 = false;
								foreach (Tile item24 in Utils.GetAllTilesInRadiusAround(item21, 1))
								{
									if (item24.isVisible)
									{
										if (item24.AirUnit != null && item24.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item24.AirUnit.FilterType == "FilterFighter" && item24.AirUnit.CurrHP > item24.AirUnit.MaxHP / 2)
										{
											flag12 = true;
											break;
										}
										if (item24.GroundUnit != null && item24.tileGO != p_unitGO.tileGO && item24.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item24.GroundUnit.FilterType == "FilterAA" && item24.GroundUnit.CurrHP > item24.GroundUnit.MaxHP / 2)
										{
											flag12 = true;
											break;
										}
									}
								}
								if (flag12)
								{
									break;
								}
								int num16 = 0;
								bool flag13 = false;
								foreach (Tile item25 in Utils.GetAllTilesInRadiusAround(item21, 1))
								{
									if (item25.GroundUnit != null && !item25.GroundUnit.IsMine && item25.GroundUnit.FilterType != "FilterAA" && item25.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
									{
										num16++;
										flag13 = true;
									}
									if (item25.AirUnit != null && item25.AirUnit.FilterType != "FilterFighter" && item25.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
									{
										num16++;
									}
								}
								if (num16 != 0 && (num16 > num15 || ((num16 == num15) & flag13)))
								{
									num15 = num16;
									tileGO4 = item21.tileGO;
								}
								break;
							}
							case 2:
							{
								bool flag11 = false;
								foreach (Tile item26 in Utils.GetAllTilesInRadiusAround(item21, 1))
								{
									if (item26.isVisible)
									{
										if (item26.AirUnit != null && item26.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item26.AirUnit.FilterType == "FilterFighter" && item26.AirUnit.CurrHP > item26.AirUnit.MaxHP / 2)
										{
											flag11 = true;
											break;
										}
										if (item26.GroundUnit != null && item26.tileGO != p_unitGO.tileGO && item26.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item26.GroundUnit.FilterType == "FilterAA" && item26.GroundUnit.CurrHP > item26.GroundUnit.MaxHP / 2)
										{
											flag11 = true;
											break;
										}
									}
								}
								if (!flag11 && (!item21.IsVictoryPoint || (item21.IsVictoryPoint && item21.ownerPlayer.isAlliedWithCurrentPlayer)) && (item21.tileGO.groundUnitGO == null || item21.tileGO.groundUnitGO.unit.IsMine || item21.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer))
								{
									return item21.tileGO;
								}
								break;
							}
							case 3:
								if ((!item21.IsVictoryPoint || (item21.IsVictoryPoint && item21.ownerPlayer.isAlliedWithCurrentPlayer)) && (item21.tileGO.groundUnitGO == null || item21.tileGO.groundUnitGO.unit.IsMine || item21.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer))
								{
									return item21.tileGO;
								}
								break;
							default:
								if (item21.tileGO.groundUnitGO == null || item21.tileGO.groundUnitGO.unit.IsMine || item21.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									return item21.tileGO;
								}
								break;
							}
						}
						if (b2 <= 1 && tileGO4 != null)
						{
							return tileGO4;
						}
					}
					foreach (Tile item27 in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 3))
					{
						if (item27.tileGO.airUnitGO == null && (item27.terrainOwner == null || !item27.terrainOwner.noRelationWithCurrentPlayer))
						{
							return item27.tileGO;
						}
					}
				}
				_dicOfUnitsWeCanKill = _dicOfUnitsWeCanKill.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Key.unit.Cost).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				foreach (KeyValuePair<UnitGO, int> item28 in _dicOfUnitsWeCanKill)
				{
					if (item28.Key.unit.Type == "Plane")
					{
						foreach (Tile neighbour6 in item28.Key.tileGO.tile.neighbours)
						{
							if (neighbour6.tileGO.airUnitGO == null && (neighbour6.terrainOwner == null || !neighbour6.terrainOwner.noRelationWithCurrentPlayer))
							{
								return neighbour6.tileGO;
							}
						}
					}
					else if (item28.Key.tileGO.airUnitGO == null)
					{
						return item28.Key.tileGO;
					}
				}
				bool flag16 = p_unitGO.unit.IsRecon;
				if (!flag16)
				{
					if (num13 >= num14)
					{
						flag16 = false;
					}
					if (num12 >= num11)
					{
						flag16 = true;
					}
				}
				if (flag16)
				{
					_dicBuildingsToProtect = _dicBuildingsToProtect.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
					foreach (KeyValuePair<TileGO, int> item29 in _dicBomberDamage)
					{
						if (item29.Key.airUnitGO.unit.CurrAmmo <= 0 || !(item29.Key.airUnitGO.unit.FilterType == "FilterBomber"))
						{
							continue;
						}
						foreach (KeyValuePair<TileGO, int> item30 in _dicBuildingsToProtect)
						{
							if (item30.Key.airUnitGO == null)
							{
								if (num12 > num11)
								{
									return item30.Key;
								}
								if (item30.Key.tile.MoneyProductionPerTurn >= 30)
								{
									return item30.Key;
								}
							}
						}
					}
				}
				if (!p_unitGO.unit.IsRecon)
				{
					_dicUnescortedBomberDamage = _dicUnescortedBomberDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
					foreach (KeyValuePair<TileGO, int> item31 in _dicUnescortedBomberDamage)
					{
						if (item31.Value == 0)
						{
							break;
						}
						foreach (Tile neighbour7 in item31.Key.tile.neighbours)
						{
							if (neighbour7.tileGO.airUnitGO == null && (neighbour7.terrainOwner == null || !neighbour7.terrainOwner.noRelationWithCurrentPlayer))
							{
								return neighbour7.tileGO;
							}
						}
					}
					if (_areWeWinning)
					{
						_dicFighterDamage = _dicFighterDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
						foreach (KeyValuePair<TileGO, int> item32 in _dicFighterDamage)
						{
							if (item32.Value == 0)
							{
								break;
							}
							foreach (Tile neighbour8 in item32.Key.tile.neighbours)
							{
								if (neighbour8.tileGO.airUnitGO == null && (neighbour8.terrainOwner == null || !neighbour8.terrainOwner.noRelationWithCurrentPlayer))
								{
									return neighbour8.tileGO;
								}
							}
						}
						_dicBomberDamage = _dicBomberDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
						foreach (KeyValuePair<TileGO, int> item33 in _dicBomberDamage)
						{
							if (item33.Value == 0)
							{
								break;
							}
							foreach (Tile neighbour9 in item33.Key.tile.neighbours)
							{
								if (neighbour9.tileGO.airUnitGO == null && (neighbour9.terrainOwner == null || !neighbour9.terrainOwner.noRelationWithCurrentPlayer))
								{
									return neighbour9.tileGO;
								}
							}
						}
					}
					else
					{
						_dicBomberDamage = _dicBomberDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
						foreach (KeyValuePair<TileGO, int> item34 in _dicBomberDamage)
						{
							if (item34.Value == 0)
							{
								break;
							}
							foreach (Tile neighbour10 in item34.Key.tile.neighbours)
							{
								if (neighbour10.tileGO.airUnitGO == null && (neighbour10.terrainOwner == null || !neighbour10.terrainOwner.noRelationWithCurrentPlayer))
								{
									return neighbour10.tileGO;
								}
							}
						}
						_dicFighterDamage = _dicFighterDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
						foreach (KeyValuePair<TileGO, int> item35 in _dicFighterDamage)
						{
							if (item35.Value == 0)
							{
								break;
							}
							foreach (Tile neighbour11 in item35.Key.tile.neighbours)
							{
								if (neighbour11.tileGO.airUnitGO == null && (neighbour11.terrainOwner == null || !neighbour11.terrainOwner.noRelationWithCurrentPlayer))
								{
									return neighbour11.tileGO;
								}
							}
						}
					}
					foreach (TileGO item36 in _paratroopersToEscort)
					{
						TileGO tileGO5 = null;
						int num18 = 0;
						foreach (Tile neighbour12 in item36.tile.neighbours)
						{
							if (neighbour12.tileGO.airUnitGO == null)
							{
								if (neighbour12.terrainOwner == null || !neighbour12.terrainOwner.noRelationWithCurrentPlayer)
								{
									tileGO5 = neighbour12.tileGO;
								}
							}
							else if (!neighbour12.tileGO.airUnitGO.unit.IsRecon && neighbour12.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && neighbour12.tileGO.airUnitGO.unit.OwnerPlayer != null && neighbour12.tileGO.airUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								num18++;
							}
						}
						if (num18 == 0 && tileGO5 != null && (tileGO5.tile.ownerPlayer == null || !tileGO5.tile.ownerPlayer.noRelationWithCurrentPlayer))
						{
							return tileGO5;
						}
					}
					_dicGroundDamage = _dicGroundDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
					if (_dicGroundDamage.Count > 0 && _dicGroundDamage.ElementAt(0).Value >= 5)
					{
						foreach (KeyValuePair<TileGO, int> item37 in _dicGroundDamage)
						{
							if (item37.Key.airUnitGO == null)
							{
								return item37.Key;
							}
						}
					}
					foreach (TileGO item38 in _friendliesToEscort)
					{
						TileGO tileGO6 = null;
						int num19 = 0;
						foreach (Tile neighbour13 in item38.tile.neighbours)
						{
							if (neighbour13.tileGO == item38)
							{
								continue;
							}
							if (neighbour13.tileGO.airUnitGO == null)
							{
								if (neighbour13.terrainOwner == null || !neighbour13.terrainOwner.noRelationWithCurrentPlayer)
								{
									tileGO6 = neighbour13.tileGO;
								}
							}
							else if (!neighbour13.tileGO.airUnitGO.unit.IsRecon && neighbour13.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && neighbour13.tileGO.airUnitGO.unit.OwnerPlayer != null && neighbour13.tileGO.airUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								num19++;
							}
						}
						if (num19 <= 2 && tileGO6 != null && (tileGO6.tile.ownerPlayer == null || !tileGO6.tile.ownerPlayer.noRelationWithCurrentPlayer))
						{
							return tileGO6;
						}
					}
					foreach (TileGO friendlyBomber in _friendlyBombers)
					{
						TileGO tileGO7 = null;
						int num20 = 0;
						foreach (Tile neighbour14 in friendlyBomber.tile.neighbours)
						{
							if (neighbour14.tileGO.airUnitGO == null)
							{
								if (neighbour14.terrainOwner == null || !neighbour14.terrainOwner.noRelationWithCurrentPlayer)
								{
									tileGO7 = neighbour14.tileGO;
								}
							}
							else if (!neighbour14.tileGO.airUnitGO.unit.IsRecon && neighbour14.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && neighbour14.tileGO.airUnitGO.unit.OwnerPlayer != null && neighbour14.tileGO.airUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								num20++;
							}
						}
						if (num20 <= 1 && tileGO7 != null && (tileGO7.tile.ownerPlayer == null || !tileGO7.tile.ownerPlayer.noRelationWithCurrentPlayer))
						{
							return tileGO7;
						}
					}
				}
				foreach (TileGO radarSignal in _radarSignals)
				{
					if (radarSignal.tile.AirUnit == null || !radarSignal.tile.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						continue;
					}
					foreach (Tile neighbour15 in radarSignal.tile.neighbours)
					{
						if ((!neighbour15.isVisible || neighbour15.AirUnit == null) && (radarSignal.tile.terrainOwner == null || !radarSignal.tile.terrainOwner.noRelationWithCurrentPlayer))
						{
							return radarSignal;
						}
					}
				}
				if (p_unitGO.unit.IsDefensive)
				{
					return null;
				}
				if (!p_unitGO.unit.IsRecon)
				{
					foreach (KeyValuePair<TileGO, float> item39 in _enemyVPsByDistance)
					{
						foreach (Tile item40 in Utils.GetAllTilesInRadiusAround(item39.Key.tile, 1))
						{
							if (!item40.isVisible)
							{
								continue;
							}
							if (item40.tileGO.airUnitGO != null && item40.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
							{
								foreach (Tile item41 in item40.neighbours.OrderBy((Tile x) => x.IsVictoryPoint).ToList())
								{
									if (item41.tileGO.airUnitGO == null && (item41.terrainOwner == null || !item41.terrainOwner.noRelationWithCurrentPlayer))
									{
										return item41.tileGO;
									}
								}
								return item39.Key;
							}
							if (item40.tileGO.groundUnitGO != null && item40.tileGO.airUnitGO == null && item40.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && !item40.tileGO.groundUnitGO.unit.IsMine)
							{
								p_unitGO.unit.GetPotentialDamage(item40.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
								if (CanPlaneAttackSafely(p_unitGO, item40, p_unitGO.unit.FinalDamage))
								{
									return item40.tileGO;
								}
							}
						}
					}
				}
			}
			else
			{
				_dicOfUnitsWeCanKill.Clear();
				_dicGroundDamage.Clear();
				_carrierDamage.Clear();
				_dicBbuildingDamage.Clear();
				foreach (Tile item42 in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_unitGO.unit.CurrMP))
				{
					if (item42.unreachable || (item42.hasTerrainOwnerPlayer && item42.terrainOwner.noRelationWithCurrentPlayer))
					{
						continue;
					}
					if (item42.enumType != TileTypes.SEA && item42.Capturable && item42.MoneyProductionPerTurn > 0 && item42.NumberOfTurnsAtLowProd == 0 && !p_unitGO.unit.IsRecon && p_unitGO.unit.FilterType == "FilterBomber" && (!item42.isVisible || (item42.isVisible && item42.tileGO.airUnitGO == null)) && item42.ownerPlayer != null && item42.ownerPlayer.isAtWarWithCurrentPlayer && CanPlaneAttackSafely(p_unitGO, item42, item42.MoneyProductionPerTurn))
					{
						_dicBbuildingDamage.Add(item42.tileGO, item42.MoneyProductionPerTurn);
					}
					if (!item42.isVisible || item42.tileGO.airUnitGO != null || item42.tileGO.groundUnitGO == null || item42.tileGO.groundUnitGO.unit.IsMine || item42.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer || !item42.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						continue;
					}
					p_unitGO.unit.GetPotentialDamage(item42.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
					if (CanPlaneAttackSafely(p_unitGO, item42, p_unitGO.unit.FinalDamage))
					{
						if (item42.tileGO.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0 && !_dicOfUnitsWeCanKill.ContainsKey(item42.tileGO.groundUnitGO))
						{
							_dicOfUnitsWeCanKill.Add(item42.tileGO.groundUnitGO, item42.tileGO.groundUnitGO.unit.Cost);
						}
						if (item42.tileGO.groundUnitGO.unit.IsCarrier)
						{
							_carrierDamage.Add(item42.tileGO, p_unitGO.unit.FinalDamage);
						}
						else
						{
							_dicGroundDamage.Add(item42.tileGO, p_unitGO.unit.FinalDamage);
						}
					}
				}
				if (p_unitGO.unit.HasAttacked)
				{
					List<Tile> list2;
					if (p_unitGO.unit.HasMoved)
					{
						list2 = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 1);
					}
					else
					{
						list2 = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_unitGO.unit.CurrMP);
						list2 = list2.OrderByDescending((Tile x) => Vector2.Distance(x.tileGO.transform.position, p_unitGO.tileGO.transform.position)).ToList();
					}
					for (byte b3 = 0; b3 <= 3; b3++)
					{
						foreach (Tile item43 in list2)
						{
							if ((item43.tileGO.airUnitGO != null && item43.tileGO != p_unitGO.tileGO) || item43.unreachable || (item43.terrainOwner != null && item43.terrainOwner.noRelationWithCurrentPlayer))
							{
								continue;
							}
							switch (b3)
							{
							case 0:
							{
								if (item43.GroundUnit != null && !item43.GroundUnit.IsMine && item43.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									break;
								}
								bool flag18 = false;
								foreach (Tile item44 in Utils.GetAllTilesInRadiusAround(item43, 1))
								{
									if (item44.isVisible)
									{
										if (item44.AirUnit != null && item44.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item44.AirUnit.FilterType == "FilterFighter" && item44.AirUnit.CurrHP > item44.AirUnit.MaxHP / 2)
										{
											flag18 = true;
											break;
										}
										if (item44.GroundUnit != null && item44.tileGO != p_unitGO.tileGO && item44.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item44.GroundUnit.FilterType == "FilterAA" && item44.GroundUnit.CurrHP > item44.GroundUnit.MaxHP / 2)
										{
											flag18 = true;
											break;
										}
									}
								}
								if (!flag18 && item43.tileGO.groundUnitGO != null && item43.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer && !item43.tileGO.groundUnitGO.unit.IsMine)
								{
									return item43.tileGO;
								}
								break;
							}
							case 1:
							{
								if (item43.GroundUnit != null && !item43.GroundUnit.IsMine && item43.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									break;
								}
								bool flag17 = false;
								foreach (Tile item45 in Utils.GetAllTilesInRadiusAround(item43, 1))
								{
									if (item45.isVisible)
									{
										if (item45.AirUnit != null && item45.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item45.AirUnit.FilterType == "FilterFighter" && item45.AirUnit.CurrHP > item45.AirUnit.MaxHP / 2)
										{
											flag17 = true;
											break;
										}
										if (item45.GroundUnit != null && item45.tileGO != p_unitGO.tileGO && item45.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item45.GroundUnit.FilterType == "FilterAA" && item45.GroundUnit.CurrHP > item45.GroundUnit.MaxHP / 2)
										{
											flag17 = true;
											break;
										}
									}
								}
								if (!flag17)
								{
									return item43.tileGO;
								}
								break;
							}
							case 2:
							{
								bool flag19 = false;
								foreach (Tile item46 in Utils.GetAllTilesInRadiusAround(item43, 1))
								{
									if (item46.isVisible)
									{
										if (item46.AirUnit != null && item46.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item46.AirUnit.FilterType == "FilterFighter" && item46.AirUnit.CurrHP > item46.AirUnit.MaxHP / 2)
										{
											flag19 = true;
											break;
										}
										if (item46.GroundUnit != null && item46.tileGO != p_unitGO.tileGO && item46.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item46.GroundUnit.FilterType == "FilterAA" && item46.GroundUnit.CurrHP > item46.GroundUnit.MaxHP / 2)
										{
											flag19 = true;
											break;
										}
									}
								}
								if (!flag19)
								{
									return item43.tileGO;
								}
								break;
							}
							default:
								if (item43.GroundUnit == null || item43.GroundUnit.IsMine || !item43.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									return item43.tileGO;
								}
								break;
							}
						}
					}
					foreach (Tile item47 in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 3))
					{
						if (item47.tileGO.airUnitGO == null && (item47.terrainOwner == null || item47.terrainOwner.isAtWarWithCurrentPlayer || item47.terrainOwner.isAlliedWithCurrentPlayer))
						{
							return item47.tileGO;
						}
					}
				}
				_dicOfUnitsWeCanKill = _dicOfUnitsWeCanKill.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Key.unit.Cost).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				if (_dicOfUnitsWeCanKill.Count > 0)
				{
					return _dicOfUnitsWeCanKill.ElementAt(0).Key.tileGO;
				}
				if (_dicBbuildingDamage.Count > 0)
				{
					_dicBbuildingDamage = _dicBbuildingDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
					foreach (KeyValuePair<TileGO, int> item48 in _dicBbuildingDamage)
					{
						if (item48.Value != 0 && (!item48.Key.tile.isVisible || !(item48.Key.airUnitGO != null)) && _enemyVPsByDistance.ContainsKey(item48.Key))
						{
							return item48.Key;
						}
					}
				}
				_carrierDamage = _carrierDamage.OrderByDescending((KeyValuePair<TileGO, int> x) => x.Value).ToDictionary((KeyValuePair<TileGO, int> x) => x.Key, (KeyValuePair<TileGO, int> x) => x.Value);
				foreach (KeyValuePair<TileGO, int> item49 in _carrierDamage)
				{
					if (item49.Value > 0 && item49.Key.airUnitGO == null)
					{
						return item49.Key;
					}
				}
				bool flag20 = false;
				foreach (KeyValuePair<UnitGO, int> item50 in _dicOfDamagePerUnit)
				{
					if (item50.Key.tileGO.tile.IsVictoryPoint)
					{
						foreach (Tile neighbour16 in item50.Key.tileGO.tile.neighbours)
						{
							if (neighbour16.GroundUnit != null && neighbour16.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								flag20 = true;
								break;
							}
						}
					}
					if (flag20)
					{
						break;
					}
				}
				if (flag20)
				{
					_dicOfDamagePerUnit = (from x in _dicOfDamagePerUnit
						orderby x.Key.tileGO.tile.IsVictoryPoint descending, x.Value descending
						select x).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				}
				else
				{
					_dicOfDamagePerUnit = _dicOfDamagePerUnit.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Value).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				}
				foreach (KeyValuePair<TileGO, int> item51 in _dicGroundDamage)
				{
					if (item51.Value > 0 && item51.Key.airUnitGO == null)
					{
						return item51.Key;
					}
				}
				if (p_unitGO.unit.IsDefensive)
				{
					return null;
				}
				for (int num21 = 0; num21 <= 1; num21++)
				{
					foreach (KeyValuePair<TileGO, float> item52 in _enemyVPsByDistance)
					{
						foreach (Tile item53 in Utils.GetAllTilesInRadiusAround(item52.Key.tile, 1))
						{
							if ((num21 == 0 && !item53.isVisible) || (item53.isVisible && item53.tileGO.airUnitGO != null))
							{
								continue;
							}
							if (item53.IsVictoryPoint && item53.enumType != TileTypes.SEA && p_unitGO.unit.FilterType == "FilterBomber" && item53.MoneyProductionPerTurn > 0 && item53.NumberOfTurnsAtLowProd == 0 && item53.ownerPlayer.isAtWarWithCurrentPlayer)
							{
								return item53.tileGO;
							}
							if (item53.isVisible && item53.tileGO.airUnitGO != null && item53.tileGO.groundUnitGO != null && item53.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
							{
								p_unitGO.unit.GetPotentialDamage(item53.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
								if (CanPlaneAttackSafely(p_unitGO, item53, p_unitGO.unit.FinalDamage))
								{
									return item53.tileGO;
								}
							}
						}
					}
				}
			}
			if ((p_unitGO.unit.IsRecon || p_unitGO.unit.FilterType == "FilterFighter") && _enemyVPsByDistance.Count >= 1)
			{
				TileGO key = _enemyVPsByDistance.ElementAt(UnityEngine.Random.Range(0, Mathf.Clamp(_enemyVPsByDistance.Count, 1, 5))).Key;
				foreach (Tile item54 in key.tile.neighbours.OrderBy((Tile x) => x.IsVictoryPoint).ToList())
				{
					if ((item54.isVisible && (!item54.isVisible || !(item54.tileGO.airUnitGO == null))) || (item54.terrainOwner != null && item54.terrainOwner.noRelationWithCurrentPlayer))
					{
						continue;
					}
					if (p_unitGO.unit.IsRecon)
					{
						bool flag21 = false;
						foreach (Tile item55 in Utils.GetAllTilesInRadiusAround(key.tile, 1))
						{
							if (item55.AirUnit != null && item55.AirUnit.IsRecon && item55.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								flag21 = true;
								break;
							}
						}
						if (!flag21)
						{
							return item54.tileGO;
						}
						continue;
					}
					return item54.tileGO;
				}
			}
			foreach (KeyValuePair<TileGO, float> item56 in _enemyVPsByDistance)
			{
				if (!item56.Key.tile.isVisible)
				{
					return item56.Key;
				}
			}
		}
		else
		{
			bool flag22 = p_unitGO.unit.Type == "Tank";
			bool flag23 = p_unitGO.unit.Range > 1 && p_unitGO.unit.FilterType == "FilterArtillery";
			_dicOfUnitsWeCanKill.Clear();
			_dicOfDamagePerUnit.Clear();
			if (p_unitGO.unit.IsKillGoal)
			{
				TileGO furthestWalkableTileFromEnemy2 = GetFurthestWalkableTileFromEnemy(p_unitGO, 10);
				if (furthestWalkableTileFromEnemy2 != null)
				{
					_ignoreOrder = true;
					return furthestWalkableTileFromEnemy2;
				}
			}
			if (p_unitGO.unit.IsDropable)
			{
				List<Tile> list3 = new List<Tile>();
				foreach (TileGO listOfpotentialTile in p_unitGO.listOfpotentialTiles)
				{
					if (listOfpotentialTile.tile.enumType == TileTypes.SEA || listOfpotentialTile.tile.enumType == TileTypes.RIVER || (listOfpotentialTile.tile.enumType == TileTypes.MOUNTAIN && !p_unitGO.unit.IsMountaineer))
					{
						continue;
					}
					if (_enemyVPsByDistance.ContainsKey(listOfpotentialTile))
					{
						list3.Add(listOfpotentialTile.tile);
						if (!(listOfpotentialTile.groundUnitGO != null) && !(listOfpotentialTile.airUnitGO != null))
						{
							return listOfpotentialTile;
						}
						continue;
					}
					foreach (Tile neighbour17 in listOfpotentialTile.tile.neighbours)
					{
						if (!neighbour17.isVisible || neighbour17.tileGO.groundUnitGO == null || neighbour17.tileGO.groundUnitGO.unit.IsMine || !neighbour17.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
						{
							continue;
						}
						bool flag24 = false;
						foreach (Tile neighbour18 in neighbour17.neighbours)
						{
							if (neighbour18.isVisible && !(neighbour18.tileGO.groundUnitGO != null) && !(neighbour18.tileGO.airUnitGO != null) && neighbour18.enumType != TileTypes.SEA && neighbour18.enumType != TileTypes.RIVER && (neighbour18.enumType != TileTypes.MOUNTAIN || p_unitGO.unit.IsMountaineer))
							{
								flag24 = true;
								break;
							}
						}
						if (!flag24)
						{
							continue;
						}
						p_unitGO.unit.IsDropable = false;
						p_unitGO.unit.GetPotentialDamage(neighbour17.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
						p_unitGO.unit.IsDropable = true;
						if (neighbour17.tileGO.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0)
						{
							if (!_dicOfUnitsWeCanKill.ContainsKey(neighbour17.tileGO.groundUnitGO))
							{
								_dicOfUnitsWeCanKill.Add(neighbour17.tileGO.groundUnitGO, p_unitGO.unit.FinalDamage);
							}
						}
						else if (!_dicOfDamagePerUnit.ContainsKey(neighbour17.tileGO.groundUnitGO))
						{
							_dicOfDamagePerUnit.Add(neighbour17.tileGO.groundUnitGO, p_unitGO.unit.FinalDamage);
						}
					}
				}
				_dicOfUnitsWeCanKill = _dicOfUnitsWeCanKill.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Key.unit.Cost).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				foreach (KeyValuePair<UnitGO, int> item57 in _dicOfUnitsWeCanKill)
				{
					foreach (Tile item58 in flag22 ? item57.Key.tileGO.tile.neighbours.OrderBy((Tile x) => x.DamageModificator).ToList() : item57.Key.tileGO.tile.neighbours.OrderByDescending((Tile x) => x.DamageModificator).ToList())
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item58.tileGO) && !item58.unreachable && item58.isVisible && !(item58.tileGO.groundUnitGO != null) && !(item58.tileGO.airUnitGO != null) && item58.enumType != TileTypes.SEA && item58.enumType != TileTypes.RIVER && (item58.enumType != TileTypes.MOUNTAIN || p_unitGO.unit.IsMountaineer))
						{
							return item58.tileGO;
						}
					}
				}
				_dicOfDamagePerUnit = _dicOfDamagePerUnit.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Value).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				foreach (KeyValuePair<UnitGO, int> item59 in _dicOfDamagePerUnit)
				{
					if (item59.Value == 0)
					{
						break;
					}
					foreach (Tile item60 in flag22 ? item59.Key.tileGO.tile.neighbours.OrderBy((Tile x) => x.DamageModificator).ToList() : item59.Key.tileGO.tile.neighbours.OrderByDescending((Tile x) => x.DamageModificator).ToList())
					{
						if (p_unitGO.listOfpotentialTiles.Contains(item60.tileGO) && !item60.unreachable && item60.isVisible && !(item60.tileGO.groundUnitGO != null) && !(item60.tileGO.airUnitGO != null) && item60.enumType != TileTypes.SEA && item60.enumType != TileTypes.RIVER && (item60.enumType != TileTypes.MOUNTAIN || p_unitGO.unit.IsMountaineer))
						{
							return item60.tileGO;
						}
					}
				}
				if (p_unitGO.unit.IsDefensive)
				{
					return null;
				}
				Dictionary<Tile, float> dictionary = new Dictionary<Tile, float>();
				foreach (Tile item61 in list3)
				{
					foreach (Tile neighbour19 in item61.neighbours)
					{
						if (!neighbour19.unreachable && !(neighbour19.tileGO.groundUnitGO != null) && !(neighbour19.tileGO.airUnitGO != null) && neighbour19.enumType != TileTypes.SEA && neighbour19.enumType != TileTypes.RIVER && (neighbour19.enumType != TileTypes.MOUNTAIN || p_unitGO.unit.IsMountaineer) && !dictionary.ContainsKey(neighbour19))
						{
							dictionary.Add(neighbour19, Vector2.Distance(item61.tileGO.transform.position, p_unitGO.tileGO.transform.position));
						}
					}
				}
				if (dictionary.Count > 0)
				{
					dictionary = (from x in dictionary
						orderby x.Value, x.Key.isVisible
						select x).ToDictionary((KeyValuePair<Tile, float> x) => x.Key, (KeyValuePair<Tile, float> x) => x.Value);
					return dictionary.ElementAt(0).Key.tileGO;
				}
			}
			else
			{
				if (!p_unitGO.unit.IsCommander && !p_unitGO.unit.IsSupplyDepot)
				{
					foreach (TileGO listOfpotentialTile2 in p_unitGO.listOfpotentialTiles)
					{
						if (listOfpotentialTile2.tile.Capturable && listOfpotentialTile2.tile.IsVictoryPoint && listOfpotentialTile2.tile.enumType != TileTypes.SEA && listOfpotentialTile2.groundUnitGO == null && listOfpotentialTile2.tile.ownerPlayer.isAlliedWithCurrentPlayer && ((listOfpotentialTile2.tile.PrimaryObjective && p_unitGO.tileGO.tile.PrimaryObjective) || !p_unitGO.tileGO.tile.PrimaryObjective) && listOfpotentialTile2.tile.MoneyProductionPerTurn > p_unitGO.tileGO.tile.MoneyProductionPerTurn && AreThereEnemiesAround(listOfpotentialTile2, 3, p_includeAir: true, p_useFogOfWar: true))
						{
							return listOfpotentialTile2;
						}
					}
					if (!flag23)
					{
						foreach (TileGO item62 in p_unitGO.listOfpotentialTiles.OrderByDescending((TileGO x) => x.tile.MoneyProductionPerTurn).ToList())
						{
							if (item62.tile.isVisible && item62.tile.enumType != TileTypes.SEA && item62.tile.tileGO.groundUnitGO == null && _enemyVPsByDistance.ContainsKey(item62))
							{
								if (p_unitGO.tileGO.tile.IsVictoryPoint && !p_unitGO.tileGO.tile.PrimaryObjective)
								{
									_ = p_unitGO.tileGO.tile.MoneyProductionPerTurn;
									_ = item62.tile.MoneyProductionPerTurn;
									return item62;
								}
								return item62;
							}
						}
					}
				}
				if (p_unitGO.tileGO.tile.Capturable && (p_unitGO.tileGO.tile.IsVictoryPoint || p_unitGO.tileGO.tile.IsWW1TrenchTile()) && (AreThereEnemiesAround(p_unitGO.tileGO, 2, p_includeAir: false, p_useFogOfWar: true) || (!p_unitGO.unit.IsParatrooper && !p_unitGO.unit.IsCommander && !p_unitGO.unit.IsSupplyDepot && !p_unitGO.unit.IsOnBoat && !p_unitGO.unit.IsArmouredTrain && !p_unitGO.unit.IsEngineers && p_unitGO.unit.FilterType != "FilterArtillery" && p_unitGO.unit.Type != "Tank")))
				{
					if (!p_unitGO.tileGO.tile.IsVictoryPoint)
					{
						return null;
					}
					if (p_unitGO.tileGO.tile.ownerPlayer.IsComputer)
					{
						return null;
					}
				}
				float hPPercent2 = p_unitGO.unit.GetHPPercent(p_forDamagePurposes: false);
				if (hPPercent2 <= 0.5f || p_unitGO.unit.IsCommander || p_unitGO.unit.IsSupplyDepot || (flag23 && !p_unitGO.unit.IsArmouredTrain))
				{
					int p_radius = ((p_unitGO.unit.IsCommander || p_unitGO.unit.IsSupplyDepot) ? 3 : ((!(hPPercent2 <= 0.5f)) ? 1 : 2));
					float num22 = 0f;
					float num23 = 0f;
					int num24 = 0;
					foreach (Tile item63 in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_radius))
					{
						if (item63.tileGO.groundUnitGO != null && !item63.tileGO.groundUnitGO.unit.IsMine && item63.tileGO.groundUnitGO.unit.OwnerPlayer != null && item63.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
						{
							num24++;
							num22 += item63.tileGO.transform.position.x;
							num23 += item63.tileGO.transform.position.y;
						}
						if (p_unitGO.unit.IsCommander && item63.tileGO.airUnitGO != null && !item63.tileGO.airUnitGO.unit.IsRecon && item63.tileGO.airUnitGO.unit.OwnerPlayer != null && item63.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
						{
							num24++;
							num22 += item63.tileGO.transform.position.x;
							num23 += item63.tileGO.transform.position.y;
						}
					}
					if (num24 > 0)
					{
						num22 /= (float)num24;
						num23 /= (float)num24;
						Vector2 a = new Vector2(num22, num23);
						TileGO tileGO8 = null;
						float num25 = 0f;
						for (int num26 = 1; num26 <= 2; num26++)
						{
							foreach (TileGO listOfpotentialTile3 in p_unitGO.listOfpotentialTiles)
							{
								if ((!listOfpotentialTile3.tile.isVisible && listOfpotentialTile3.tile.IsVictoryPoint && !listOfpotentialTile3.tile.ownerPlayer.isAlliedWithCurrentPlayer) || (num26 == 1 && AreThereEnemiesAround(listOfpotentialTile3, 1, p_includeAir: false, p_useFogOfWar: false)))
								{
									continue;
								}
								if (tileGO8 == null)
								{
									tileGO8 = listOfpotentialTile3;
									num25 = Vector2.Distance(a, listOfpotentialTile3.transform.position);
									continue;
								}
								float num27 = Vector2.Distance(a, listOfpotentialTile3.transform.position);
								if (num27 > num25)
								{
									tileGO8 = listOfpotentialTile3;
									num25 = num27;
								}
							}
							if (tileGO8 != null)
							{
								_ignoreOrder = true;
								return tileGO8;
							}
						}
					}
				}
				if (p_unitGO.unit.IsOnBoat)
				{
					List<TileGO> list4 = new List<TileGO>();
					foreach (KeyValuePair<TileGO, float> item64 in _enemyVPsByDistance)
					{
						if (item64.Key.tile.enumType == TileTypes.SEA)
						{
							continue;
						}
						for (byte b4 = 0; b4 <= 2; b4++)
						{
							list4.Clear();
							foreach (Tile item65 in (from x in Utils.GetAllTilesInRadiusAround(item64.Key.tile, b4)
								orderby x.enumType != TileTypes.SEA
								select x).ToList())
							{
								if (IsLandingOnDifferentLandmass(item65, item64.Key.tile) || item65.unreachable || (item65.isVisible && (item65.tileGO.groundUnitGO != null || item65.tileGO.groundUnitGO == p_unitGO)) || (item65.enumType == TileTypes.RIVER && !p_unitGO.unit.CanMoveOnRivers()) || (item65.enumType == TileTypes.MOUNTAIN && !p_unitGO.unit.CanMoveOverMountains()) || (item65.enumType == TileTypes.MARSH && !p_unitGO.unit.CanMoveOnMarshes()) || (flag23 && AreThereEnemiesAround(item65.tileGO, 1, p_includeAir: false, p_useFogOfWar: false)) || (item65.terrainOwner != null && item65.terrainOwner != item64.Key.tile.ownerPlayer && item65.terrainOwner.noRelationWithCurrentPlayer && !item65.terrainOwner.canAIattackPlayer))
								{
									continue;
								}
								if (flag23)
								{
									if (Vector2.Distance(item64.Key.transform.position, item65.tileGO.transform.position) > 6f)
									{
										list4.Add(item65.tileGO);
									}
								}
								else if (Vector2.Distance(item64.Key.transform.position, item65.tileGO.transform.position) <= 5.2f * (float)(int)p_unitGO.unit.Range)
								{
									list4.Add(item65.tileGO);
								}
							}
							if (list4.Count != 0)
							{
								_VPGOfromNeighbourGoal = item64.Key;
								return list4.OrderBy((TileGO x) => Vector2.Distance(x.transform.position, p_unitGO.tileGO.transform.position)).ElementAt(0);
							}
						}
					}
				}
				else
				{
					short num28 = -1;
					if (p_unitGO.unit.CurrAmmo == 0 && p_unitGO.unit.MaxAmmo > 0)
					{
						num28 = p_unitGO.unit.CurrAmmo;
						p_unitGO.unit.CurrAmmo = 1;
					}
					if (!p_unitGO.unit.IsCommander && !p_unitGO.unit.IsSupplyDepot)
					{
						foreach (TileGO listOfpotentialTile4 in p_unitGO.listOfpotentialTiles)
						{
							foreach (Tile item66 in Utils.GetAllTilesInRadiusAround(listOfpotentialTile4.tile, p_unitGO.unit.Range))
							{
								if (item66.unreachable || !item66.isVisible)
								{
									continue;
								}
								if (p_unitGO.unit.FilterType == "FilterAA" && item66.tileGO.airUnitGO != null && item66.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									p_unitGO.unit.GetPotentialDamage(item66.tileGO.airUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
									if (item66.tileGO.airUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0)
									{
										if (!_dicOfUnitsWeCanKill.ContainsKey(item66.tileGO.airUnitGO))
										{
											_dicOfUnitsWeCanKill.Add(item66.tileGO.airUnitGO, p_unitGO.unit.FinalDamage);
										}
									}
									else if (!_dicOfDamagePerUnit.ContainsKey(item66.tileGO.airUnitGO))
									{
										_dicOfDamagePerUnit.Add(item66.tileGO.airUnitGO, p_unitGO.unit.FinalDamage);
									}
								}
								if (!(item66.tileGO.groundUnitGO != null) || item66.tileGO.groundUnitGO.unit.IsMine || !item66.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									continue;
								}
								p_unitGO.unit.GetPotentialDamage(item66.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
								if (item66.tileGO.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0)
								{
									if (!_dicOfUnitsWeCanKill.ContainsKey(item66.tileGO.groundUnitGO))
									{
										_dicOfUnitsWeCanKill.Add(item66.tileGO.groundUnitGO, p_unitGO.unit.FinalDamage);
									}
								}
								else if (!_dicOfDamagePerUnit.ContainsKey(item66.tileGO.groundUnitGO))
								{
									_dicOfDamagePerUnit.Add(item66.tileGO.groundUnitGO, p_unitGO.unit.FinalDamage);
								}
							}
						}
					}
					if (num28 >= 0)
					{
						p_unitGO.unit.CurrAmmo = num28;
					}
					_dicOfUnitsWeCanKill = _dicOfUnitsWeCanKill.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Key.unit.Cost).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
					foreach (KeyValuePair<UnitGO, int> item67 in _dicOfUnitsWeCanKill)
					{
						if (flag23)
						{
							foreach (Tile neighbour20 in p_unitGO.tileGO.tile.neighbours)
							{
								if (p_unitGO.listOfpotentialTiles.Contains(neighbour20.tileGO) && Vector2.Distance(neighbour20.tileGO.transform.position, item67.Key.tileGO.transform.position) / 5.2f <= (float)(int)p_unitGO.unit.Range)
								{
									_ignoreOrder = true;
									return neighbour20.tileGO;
								}
							}
							return item67.Key.tileGO;
						}
						foreach (Tile item68 in flag22 ? (from x in item67.Key.tileGO.tile.neighbours
							orderby x.DamageModificator, x == p_unitGO.tileGO.tile descending
							select x).ToList() : (from x in item67.Key.tileGO.tile.neighbours
							orderby x.DamageModificator descending, x == p_unitGO.tileGO.tile descending
							select x).ToList())
						{
							if (item68 == p_unitGO.tileGO.tile || p_unitGO.listOfpotentialTiles.Contains(item68.tileGO))
							{
								if (p_unitGO.tileGO.tile == item68)
								{
									return null;
								}
								return item68.tileGO;
							}
						}
					}
					if (p_unitGO.unit.IsPartisans)
					{
						foreach (TileGO listOfpotentialTile5 in p_unitGO.listOfpotentialTiles)
						{
							if (listOfpotentialTile5.tile.Bridge && !listOfpotentialTile5.tile.IsBridgeDestroyed && listOfpotentialTile5.groundUnitGO == null && listOfpotentialTile5.tile.terrainOwner != null && listOfpotentialTile5.tile.terrainOwner.isAtWarWithCurrentPlayer)
							{
								return listOfpotentialTile5;
							}
						}
					}
					if (p_unitGO.unit.FilterType == "FilterAA")
					{
						if (!ShouldUnitLeaveDefensiveMode(p_unitGO))
						{
							return null;
						}
						p_unitGO.unit.IsDefensive = false;
						for (int num29 = 0; num29 <= 1; num29++)
						{
							int num30 = 0;
							TileGO tileGO9 = null;
							foreach (TileGO item69 in p_unitGO.listOfpotentialTiles.Concat(new List<TileGO> { p_unitGO.tileGO }))
							{
								int num31 = 0;
								int num32 = 0;
								bool flag25 = false;
								foreach (Tile neighbour21 in item69.tile.neighbours)
								{
									if (neighbour21.AirUnit != null && neighbour21.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer && neighbour21.AirUnit.FilterType != "FilterFighter")
									{
										num31++;
									}
									if (neighbour21.GroundUnit != null && neighbour21.GroundUnit != p_unitGO.unit)
									{
										if (neighbour21.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
										{
											if (neighbour21.GroundUnit.FilterType != "FilterAA")
											{
												num31++;
											}
										}
										else
										{
											flag25 = true;
										}
									}
									foreach (Tile neighbour22 in neighbour21.neighbours)
									{
										if (neighbour22.GroundUnit != null && neighbour22.GroundUnit != p_unitGO.unit && neighbour22.GroundUnit.FilterType == "FilterAA" && neighbour22.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
										{
											num32++;
										}
									}
								}
								if (num29 == 0)
								{
									if (flag25)
									{
										continue;
									}
								}
								else if (num31 == 0)
								{
									if (num32 == 0)
									{
										continue;
									}
									num31 += num32;
								}
								if (num31 > num30)
								{
									num30 = num31;
									tileGO9 = item69;
								}
							}
							if (num30 > 0 && tileGO9 != null)
							{
								return tileGO9;
							}
						}
					}
					if (ShouldTakeDefensivePosition(p_unitGO, out var o_tileGO))
					{
						return o_tileGO;
					}
					if (CanDealDamage(p_unitGO) && !p_unitGO.unit.shouldMoveOutNow)
					{
						return null;
					}
					_dicOfDamagePerUnit = _dicOfDamagePerUnit.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Value).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
					bool flag26 = p_unitGO.unit.FilterType == "FilterArtillery";
					foreach (KeyValuePair<UnitGO, int> item70 in _dicOfDamagePerUnit)
					{
						if (!((item70.Value > 0) | flag26))
						{
							continue;
						}
						if (flag23)
						{
							foreach (Tile neighbour23 in p_unitGO.tileGO.tile.neighbours)
							{
								if (p_unitGO.listOfpotentialTiles.Contains(neighbour23.tileGO) && Vector2.Distance(neighbour23.tileGO.transform.position, item70.Key.tileGO.transform.position) / 5.2f <= (float)(int)p_unitGO.unit.Range)
								{
									_ignoreOrder = true;
									return neighbour23.tileGO;
								}
							}
							return item70.Key.tileGO;
						}
						foreach (Tile item71 in flag22 ? item70.Key.tileGO.tile.neighbours.OrderBy((Tile x) => x.DamageModificator).ToList() : item70.Key.tileGO.tile.neighbours.OrderByDescending((Tile x) => x.DamageModificator).ToList())
						{
							if (p_unitGO.listOfpotentialTiles.Contains(item71.tileGO))
							{
								return item71.tileGO;
							}
						}
					}
				}
				if (!ShouldUnitLeaveDefensiveMode(p_unitGO))
				{
					return null;
				}
				p_unitGO.unit.IsDefensive = false;
			}
			foreach (KeyValuePair<TileGO, float> item72 in _enemyVPsByDistance)
			{
				if (item72.Key.tile.enumType == TileTypes.SEA)
				{
					continue;
				}
				for (byte b5 = 0; b5 <= 2; b5++)
				{
					foreach (Tile item73 in from neighborTile in Utils.GetAllTilesInRadiusAround(item72.Key.tile, b5)
						orderby Vector2.Distance(p_unitGO.tileGO.transform.position, neighborTile.tileGO.transform.position)
						select neighborTile)
					{
						if ((!p_unitGO.unit.IsOnBoat || !IsLandingOnDifferentLandmass(item73, item72.Key.tile)) && !item73.unreachable && (!item73.IsVictoryPoint || item73.isVisible) && item73.enumType != TileTypes.SEA && (!item73.isVisible || (!(item73.tileGO.groundUnitGO != null) && !(item73.tileGO.groundUnitGO == p_unitGO))) && (item73.enumType != TileTypes.RIVER || p_unitGO.unit.CanMoveOnRivers()) && (item73.enumType != TileTypes.MOUNTAIN || p_unitGO.unit.CanMoveOverMountains()) && (item73.enumType != TileTypes.MARSH || p_unitGO.unit.CanMoveOnMarshes()) && (!flag23 || !AreThereEnemiesAround(item73.tileGO, 1, p_includeAir: false, p_useFogOfWar: false)) && (item73.terrainOwner == null || item73.terrainOwner == item72.Key.tile.ownerPlayer || !item73.terrainOwner.noRelationWithCurrentPlayer || item73.terrainOwner.canAIattackPlayer))
						{
							_VPGOfromNeighbourGoal = item72.Key;
							return item73.tileGO;
						}
					}
				}
			}
		}
		return null;
	}

	private void GetPathToGoal(UnitGO p_unitGO, RunTimes p_runTime)
	{
		int count = TurnManager.currPlayer.PlayersAtWar.Count;
		bool flag = _hasCompleteReachabilityMap && _reachabilityPlayersAtWarCount != count;
		bool flag2 = (_pathingCount == 0) | flag;
		if (flag2)
		{
			_reachableTilesForCurrentPathing.Clear();
			_hasCompleteReachabilityMap = false;
		}
		_goal = p_unitGO.goal.tile;
		_source = p_unitGO.tileGO.tile;
		bool hasCompleteReachabilityMap = _hasCompleteReachabilityMap;
		bool flag3 = hasCompleteReachabilityMap && _reachableTilesForCurrentPathing.Contains(_goal);
		bool flag4 = hasCompleteReachabilityMap && !flag3;
		if (!flag4)
		{
			for (short num = 0; num < _mapSizeX; num++)
			{
				for (short num2 = 0; num2 < _mapSizeY; num2++)
				{
					_tiles[num, num2].SetupForAStar();
					if (flag2)
					{
						_tiles[num, num2].unreachable = false;
					}
				}
			}
		}
		p_unitGO.AIpathToVP.Clear();
		Unit unit = p_unitGO.unit;
		Tile tile = null;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = MapGO.radarDetections.Count > 0;
		bool flag8 = MapGO.sonarDetections.Count > 0;
		byte maxMP = p_unitGO.unit.MaxMP;
		bool flag9 = unit.Type == "Plane";
		bool flag10 = unit.Type == "Boat";
		bool flag11 = unit.FilterType == "FilterBoat";
		bool flag12 = unit.Type == "Tank" || unit.IsMechanized;
		bool flag13 = unit.Type == "Tank" || unit.IsMotorized || unit.IsMechanized;
		bool flag14 = unit.IsArmouredTrain || unit.IsTrain;
		bool flag15 = unit.CanMoveOnRivers();
		bool flag16 = unit.CanMoveOverMountains();
		bool flag17 = unit.CanMoveOnMarshes();
		bool flag18 = flag10 || (!flag9 && _canPerformLandings);
		bool flag19 = !flag4;
		if (flag11)
		{
			if (unit.IsPatrolBoat)
			{
				if (_source.isFrozen || _goal.isFrozen || (_source.riverAreaIndex != _goal.riverAreaIndex && _source.seaAreaIndex != _goal.seaAreaIndex))
				{
					flag19 = false;
				}
			}
			else if (_goal.isFrozen || _source.isFrozen || _source.seaAreaIndex != _goal.seaAreaIndex)
			{
				flag19 = false;
			}
		}
		else if (!flag9 && !_canPerformLandings && _source.landAreaIndex != _goal.landAreaIndex && !_source.isFrozen && !_goal.isFrozen)
		{
			flag19 = false;
		}
		if (_goal.unreachable)
		{
			flag19 = false;
		}
		if (flag19 && !flag3)
		{
			_reachableTilesForCurrentPathing.Clear();
			_reachableTilesForCurrentPathing.Add(_source);
			_hasCompleteReachabilityMap = false;
			Queue<Tile> queue = new Queue<Tile>();
			queue.Enqueue(_source);
			int num3 = 1;
			bool flag20 = false;
			while (num3 > 0)
			{
				tile = queue.Dequeue();
				num3--;
				for (byte b = 0; b < tile.amountOfNeighbours; b++)
				{
					Tile tile2 = tile.neighbours[b];
					if (tile2.visited)
					{
						continue;
					}
					if (!flag9 && !flag10 && !flag14 && tile2.IsBridgeDestroyed && !tile2.isFrozen && !unit.IsPontoonBridge)
					{
						tile2.visited = true;
						continue;
					}
					if (!flag9)
					{
						switch (tile2.enumType)
						{
						case TileTypes.SEA:
							if (flag11)
							{
								if (tile2.isFrozen)
								{
									continue;
								}
								if (tile.enumType == TileTypes.RIVER && !flag15)
								{
									tile2.visited = true;
									continue;
								}
							}
							else
							{
								if (tile2.isFrozen || (tile2.Bridge && !tile2.IsBridgeDestroyed))
								{
									break;
								}
								if (!flag18)
								{
									continue;
								}
								if (tile.enumType == TileTypes.SEA)
								{
									break;
								}
								if (tile.enumType == TileTypes.RIVER)
								{
									if (!flag15)
									{
										tile2.visited = true;
										continue;
									}
								}
								else if (tile.enumType != TileTypes.HARBOUR || !tile.hasTerrainOwnerPlayer || !tile.terrainOwner.isAlliedWithCurrentPlayer)
								{
									continue;
								}
							}
							break;
						case TileTypes.MARSH:
							if (!flag17)
							{
								tile2.visited = true;
								continue;
							}
							break;
						case TileTypes.MOUNTAIN:
							if (!flag16)
							{
								tile2.visited = true;
								continue;
							}
							break;
						case TileTypes.RIVER:
							if (flag15 & flag11)
							{
								if (tile2.isFrozen)
								{
									tile2.visited = true;
									continue;
								}
							}
							else if (!flag15 && !tile2.isFrozen && (!tile2.Bridge || tile2.IsBridgeDestroyed) && (tile2.GroundUnit == null || !tile2.GroundUnit.IsPontoonBridge || !tile2.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer))
							{
								tile2.visited = true;
								continue;
							}
							break;
						}
					}
					tile2.visited = true;
					if (!tile2.hasTerrainOwnerPlayer || !tile2.terrainOwner.noRelationWithCurrentPlayer)
					{
						_reachableTilesForCurrentPathing.Add(tile2);
						if (tile2 == _goal)
						{
							flag20 = true;
							num3 = 0;
							break;
						}
						queue.Enqueue(tile2);
						num3++;
					}
				}
			}
			if (!flag20)
			{
				flag19 = false;
				_hasCompleteReachabilityMap = true;
				_reachabilityPlayersAtWarCount = count;
			}
		}
		if (flag19)
		{
			int num4 = 1;
			PriorityQueue<Tile> priorityQueue = new PriorityQueue<Tile>();
			Dictionary<Tile, Tile> dictionary = new Dictionary<Tile, Tile>();
			Dictionary<Tile, int> dictionary2 = new Dictionary<Tile, int>();
			Vector3 position = _goal.tileGO.transform.position;
			priorityQueue.Enqueue(_source, 0f);
			dictionary2[_source] = 0;
			while (num4 > 0)
			{
				tile = priorityQueue.Dequeue();
				num4--;
				if (tile == _goal)
				{
					flag5 = true;
					break;
				}
				bool flag21 = ((_canPerformLandings && tile.IsVictoryPoint && tile.enumType == TileTypes.HARBOUR && tile.hasOwnerPlayer && tile.ownerPlayer.isAlliedWithCurrentPlayer) ? true : false);
				for (byte b2 = 0; b2 < tile.amountOfNeighbours; b2++)
				{
					int num5 = 0;
					Tile tile3 = tile.neighbours[b2];
					if (tile3.hasTerrainOwnerPlayer && tile3.terrainOwner.noRelationWithCurrentPlayer)
					{
						continue;
					}
					if (flag9)
					{
						num5++;
						if (tile3.isVisible)
						{
							if (tile3.AirUnit != null)
							{
								if (tile3.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									num5 += 5;
								}
								else if (!tile3.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									continue;
								}
							}
						}
						else if (flag7 && (tile3.isRADARtile || MapGO.radarDetections.Contains(tile3.tileGO)))
						{
							num5 += 10;
							tile3.isRADARtile = true;
						}
					}
					else if (flag11 || (tile.enumType == TileTypes.SEA && !tile.Bridge) || (flag21 && tile3.enumType == TileTypes.SEA))
					{
						if (tile3.isVisible && tile3.GroundUnit != null)
						{
							if (tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
							{
								num5 += 10;
							}
							else
							{
								if (!tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									continue;
								}
								if (flag11)
								{
									num5 += 5;
								}
								else
								{
									num5 = ((tile.enumType != TileTypes.SEA || tile3.enumType == TileTypes.SEA) ? (num5 + maxMP) : (num5 + maxMP * 2));
								}
							}
						}
						num5 += tile3.MPCost;
						if (flag11)
						{
							if (tile3.isFrozen)
							{
								continue;
							}
							if (unit.IsPatrolBoat)
							{
								if (!tile3.Bridge && tile3.enumType != TileTypes.RIVER && tile3.enumType != TileTypes.CITY && tile3.enumType != TileTypes.HARBOUR && tile3.enumType != TileTypes.SEA)
								{
									continue;
								}
							}
							else if (tile3.enumType != TileTypes.SEA && tile3.enumType != TileTypes.HARBOUR)
							{
								continue;
							}
						}
						else
						{
							if (flag21)
							{
								num5 += 50;
								if (tile.GroundUnit != null && tile.GroundUnit != unit)
								{
									num5 += maxMP * 2;
								}
							}
							if (tile3.enumType != TileTypes.SEA || tile3.isFrozen)
							{
								if (tile3.enumType == TileTypes.MOUNTAIN)
								{
									if (!flag16)
									{
										continue;
									}
								}
								else if (tile3.enumType == TileTypes.RIVER)
								{
									if (!flag15)
									{
										continue;
									}
								}
								else if (tile3.enumType == TileTypes.MARSH && !flag17)
								{
									continue;
								}
							}
						}
						if (flag8 && (tile3.isSonarTile || MapGO.sonarDetections.Contains(tile3.tileGO)))
						{
							tile3.isSonarTile = true;
							continue;
						}
					}
					else if (flag14)
					{
						if (tile3.IsBridgeDestroyed)
						{
							continue;
						}
						if (tile3.isVisible && tile3.GroundUnit != null)
						{
							if (tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
							{
								num5 += 10;
							}
							else
							{
								if (!tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									continue;
								}
								num5++;
							}
						}
						num5 = ((!tile3.Railroad && tile3.enumType != TileTypes.CITY && tile3.enumType != TileTypes.HARBOUR && tile3.enumType != TileTypes.AIRPORT && tile3.enumType != TileTypes.TRENCH && tile3.enumType != TileTypes.FACTORY && tile3.enumType != TileTypes.REFINERY) ? (num5 + 100) : (num5 + tile3.MPCost));
					}
					else
					{
						if (tile3.IsBridgeDestroyed && !tile3.isFrozen && !unit.IsPontoonBridge)
						{
							flag6 = true;
							continue;
						}
						num5 = ((!flag12 || !tile3.isRoadOrPlain) ? (num5 + tile3.MPCost) : (num5 + Mathf.Clamp(tile3.MPCost / 2, 1, tile3.MPCost)));
						if (tile3.enumType == TileTypes.MOUNTAIN)
						{
							if (!flag16)
							{
								continue;
							}
						}
						else if (tile3.enumType == TileTypes.MARSH)
						{
							if (!flag17)
							{
								continue;
							}
						}
						else if (tile3.enumType == TileTypes.RIVER && !tile3.isFrozen)
						{
							if (flag15)
							{
								if (tile3.GroundUnit == null)
								{
									num5 = unit.CurrMP;
								}
								else if (tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									num5 += 5;
								}
								else
								{
									if (!tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
									{
										continue;
									}
									num5++;
								}
							}
							else
							{
								if (tile3.GroundUnit == null || !tile3.GroundUnit.IsPontoonBridge || !tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									flag6 = true;
									continue;
								}
								num5 = 0;
							}
						}
						else if (tile3.enumType == TileTypes.SEA)
						{
							if (tile3.isFrozen)
							{
								if (flag12)
								{
									num5 += 15;
								}
							}
							else if (tile.Bridge)
							{
								if (!tile3.Bridge)
								{
									continue;
								}
							}
							else if (tile.enumType != TileTypes.SEA && (!tile3.Bridge || tile3.IsBridgeDestroyed))
							{
								continue;
							}
						}
						if (tile3.isVisible && tile3.GroundUnit != null)
						{
							if (tile3.enumType == TileTypes.SEA && !tile3.isFrozen)
							{
								if (tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									num5++;
								}
								else
								{
									if (!tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
									{
										continue;
									}
									num5 += 5;
								}
							}
							else if (tile3.GroundUnit.IsArmourObstacle & flag13)
							{
								if (tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
								{
									num5 += 5;
								}
								else if (!tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									continue;
								}
							}
							else if (tile3.enumType != TileTypes.RIVER || (tile3.enumType == TileTypes.RIVER && (flag15 || tile3.isFrozen)))
							{
								if (tile3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									num5 = ((p_runTime != RunTimes.SECOND) ? (num5 + 1) : (num5 + 10));
								}
								else
								{
									if (!tile3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
									{
										continue;
									}
									num5 = ((p_runTime != RunTimes.SECOND) ? (num5 + 5) : (num5 + 10));
								}
							}
						}
					}
					int num6 = dictionary2[tile] + num5;
					if (!dictionary2.ContainsKey(tile3) || num6 < dictionary2[tile3])
					{
						dictionary2[tile3] = num6;
						float p_priority = (float)num6 + Vector2.Distance(position, tile3.tileGO.transform.position);
						priorityQueue.Enqueue(tile3, p_priority);
						dictionary[tile3] = tile;
						num4++;
					}
				}
			}
			if (flag5)
			{
				_bestPath.Clear();
				while (dictionary.ContainsKey(tile))
				{
					_bestPath.Add(tile);
					tile = dictionary[tile];
				}
				_bestPath.Reverse();
				if ((_bestPath.Count > 100) & flag10)
				{
					return;
				}
				for (int i = 0; i < _bestPath.Count; i++)
				{
					p_unitGO.AIpathToVP.Add(_bestPath[i].tileGO);
					if (_bestPath[i].GroundUnit != null && _bestPath[i].GroundUnit.IsPontoonBridge && _bestPath[i].GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
					{
						_bestPath[i].GroundUnit.OthersNeedUsAsBridge = true;
						_bestPath[i].GroundUnit.OthersNeedUsAsBridgeNotArtificially = true;
					}
				}
				if (p_runTime != RunTimes.FIRST || flag9 || flag10 || flag14)
				{
					return;
				}
				int num7 = 0;
				bool flag22 = false;
				bool flag23 = false;
				TileTypes tileTypes = TileTypes.NONE;
				foreach (Tile item in _bestPath)
				{
					if (item.enumType == TileTypes.SEA && tileTypes == TileTypes.HARBOUR && !item.isFrozen)
					{
						flag23 = true;
					}
					num7 = ((!flag12 || !item.isRoadOrPlain) ? (num7 + item.MPCost) : (num7 + Mathf.Clamp(item.MPCost / 2, 1, item.MPCost)));
					if (num7 > p_unitGO.unit.CurrMP)
					{
						break;
					}
					flag22 = ((item.tileGO.groundUnitGO != null && item.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer) ? true : false);
					tileTypes = item.enumType;
				}
				if (!flag23 & flag22)
				{
					GetPathToGoal(p_unitGO, RunTimes.SECOND);
				}
				return;
			}
		}
		if (!flag5)
		{
			if (flag6)
			{
				_engineersNeeded = true;
			}
			_pathingCount++;
			_goal.unreachable = true;
			if (_VPGOfromNeighbourGoal != null)
			{
				_VPGOfromNeighbourGoal.tile.unreachable = true;
			}
			_lastUnitIDusingUnreachable = p_unitGO.unit.ID;
			if (_pathingCount < 15 && _pathingCount < _enemyVPsGO.Count)
			{
				Move(p_unitGO);
			}
		}
	}

	private static bool IsLandingOnDifferentLandmass(Tile p_landingTile, Tile p_objectiveTile)
	{
		if (p_objectiveTile == null || p_landingTile.enumType == TileTypes.SEA || p_landingTile.enumType == TileTypes.HARBOUR)
		{
			return false;
		}
		if (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter)
		{
			return false;
		}
		if (p_landingTile.landAreaIndex.HasValue && p_objectiveTile.landAreaIndex.HasValue)
		{
			return p_landingTile.landAreaIndex != p_objectiveTile.landAreaIndex;
		}
		return false;
	}

	private void GetClickedTile(UnitGO p_unitGO)
	{
		if (p_unitGO.unit.Type == "Boat")
		{
			bool flag = p_unitGO.unit.GetHPPercent(p_forDamagePurposes: false) < 0.9f;
			bool flag2 = p_unitGO.unit.MaxAmmo > 0 && p_unitGO.unit.CurrAmmo == 0;
			bool flag3 = (float)p_unitGO.unit.CurrAutonomy < (float)p_unitGO.unit.MaxAutonomy * 0.15f;
			if ((flag | flag2 | flag3) && MapGO.IsThereACertainTileAround(p_unitGO.tileGO, "Harbour"))
			{
				_tileGOForMovement = p_unitGO.tileGO;
				return;
			}
		}
		int num = 0;
		GameObject gameObject = null;
		TileTypes tileTypes = TileTypes.NONE;
		bool flag4 = false;
		bool flag5 = p_unitGO.unit.Type == "Tank" || p_unitGO.unit.IsMechanized;
		TileGO tileGO = null;
		TileGO tileGO2 = null;
		foreach (TileGO item in p_unitGO.AIpathToVP)
		{
			if (p_unitGO.unit.Type == "Plane")
			{
				num++;
			}
			else
			{
				if (item.tile.enumType == TileTypes.SEA && tileTypes == TileTypes.HARBOUR)
				{
					flag4 = true;
					tileGO2 = tileGO;
				}
				tileTypes = item.tile.enumType;
				if (p_unitGO.unit.IsOnBoat && item.tile.enumType != TileTypes.SEA)
				{
					num += p_unitGO.unit.CurrMP;
				}
				else
				{
					num = ((!flag5 || !item.tile.isRoadOrPlain) ? (num + item.tile.MPCost) : (num + Mathf.Clamp(item.tile.MPCost / 2, 1, item.tile.MPCost)));
				}
			}
			if (num > p_unitGO.unit.CurrMP)
			{
				break;
			}
			gameObject = item.gameObject;
			tileGO = item;
		}
		TileGO tileGO3 = null;
		if (flag4)
		{
			foreach (Tile neighbour in p_unitGO.tileGO.tile.neighbours)
			{
				if (neighbour.tileGO == tileGO2)
				{
					tileGO3 = tileGO2;
					break;
				}
			}
		}
		if (gameObject == null)
		{
			return;
		}
		Dictionary<TileGO, int> dictionary = new Dictionary<TileGO, int>();
		int num2 = 0;
		foreach (TileGO item2 in p_unitGO.AIpathToVP)
		{
			if (!dictionary.ContainsKey(item2))
			{
				dictionary[item2] = num2;
			}
			num2++;
			if (item2.gameObject == gameObject)
			{
				break;
			}
		}
		Dictionary<TileGO, float> dictionary2 = new Dictionary<TileGO, float>();
		Tile p_objectiveTile = ((_VPGOfromNeighbourGoal != null) ? _VPGOfromNeighbourGoal.tile : p_unitGO.goal?.tile);
		foreach (TileGO listOfpotentialTile in p_unitGO.listOfpotentialTiles)
		{
			if (p_unitGO.unit.IsOnBoat && IsLandingOnDifferentLandmass(listOfpotentialTile.tile, p_objectiveTile))
			{
				continue;
			}
			if (!dictionary2.ContainsKey(listOfpotentialTile))
			{
				if (dictionary.TryGetValue(listOfpotentialTile, out var value))
				{
					dictionary2.Add(listOfpotentialTile, -value);
				}
				else
				{
					dictionary2.Add(listOfpotentialTile, Vector2.Distance(gameObject.transform.position, listOfpotentialTile.transform.position));
				}
			}
			if (flag4 && listOfpotentialTile.tile.enumType == TileTypes.HARBOUR)
			{
				tileGO3 = listOfpotentialTile;
				break;
			}
		}
		if (dictionary2.Count == 0)
		{
			return;
		}
		dictionary2 = dictionary2.OrderBy((KeyValuePair<TileGO, float> x) => x.Value).ToDictionary((KeyValuePair<TileGO, float> x) => x.Key, (KeyValuePair<TileGO, float> x) => x.Value);
		if (flag4 && tileGO3 != null && !p_unitGO.unit.IsOnBoat)
		{
			_tileGOForMovement = tileGO3;
		}
		else if (p_unitGO.unit.IsCommander || (p_unitGO.unit.Range > 1 && p_unitGO.unit.FilterType == "FilterArtillery") || p_unitGO.unit.FilterType == "FilterAA")
		{
			foreach (KeyValuePair<TileGO, float> item3 in dictionary2)
			{
				if ((!item3.Key.tile.isVisible && p_unitGO.unit.Type != "Tank") || (p_unitGO.unit.FilterType != "FilterAA" && AreThereEnemiesAround(item3.Key, 1, p_includeAir: false, p_useFogOfWar: false)) || (!item3.Key.tile.isVisible && item3.Key.tile.IsVictoryPoint && item3.Key.tile.ownerPlayer.isAtWarWithCurrentPlayer))
				{
					continue;
				}
				bool flag6 = false;
				foreach (Tile neighbour2 in item3.Key.tile.neighbours)
				{
					if (!neighbour2.isVisible && neighbour2.IsVictoryPoint && neighbour2.ownerPlayer.isAtWarWithCurrentPlayer)
					{
						flag6 = true;
						break;
					}
				}
				if (!flag6)
				{
					_tileGOForMovement = item3.Key;
					break;
				}
			}
			if (_tileGOForMovement == null)
			{
				return;
			}
		}
		else
		{
			_tileGOForMovement = dictionary2.ElementAt(0).Key;
			if (p_unitGO.unit.FilterType == "FilterBoat" && _tileGOForMovement.tile.enumType == TileTypes.HARBOUR)
			{
				TileGO tileGO4 = null;
				foreach (KeyValuePair<TileGO, float> item4 in dictionary2)
				{
					if (item4.Key.tile.enumType != TileTypes.HARBOUR)
					{
						tileGO4 = item4.Key;
						break;
					}
				}
				_tileGOForMovement = ((tileGO4 != null) ? tileGO4 : p_unitGO.tileGO);
			}
		}
		if (_tileGOForMovement.isInFogOfWar && p_unitGO.tileGO.isInFogOfWar)
		{
			PlayerSettings.Instance.IsQuickMovement = true;
		}
		p_unitGO.gameObject.SetActive(value: true);
	}

	private bool ShouldSendUnitBackToReserve(UnitGO p_unitGO)
	{
		if (p_unitGO.unit.MaxMP == 0)
		{
			return false;
		}
		if (p_unitGO.unit.IsDefensive)
		{
			return false;
		}
		if (p_unitGO.unit.IsMissile)
		{
			return false;
		}
		if (p_unitGO.tileGO.tile.IsVictoryPoint)
		{
			return false;
		}
		if (playerNeedsToRedeployItsUnitsNow == TurnManager.currPlayer)
		{
			return true;
		}
		if (p_unitGO.unit.Type == "Boat")
		{
			return false;
		}
		if (p_unitGO.goal == p_unitGO.tileGO)
		{
			return false;
		}
		foreach (Tile neighbour in p_unitGO.tileGO.tile.neighbours)
		{
			if (neighbour.terrainOwner != p_unitGO.unit.OwnerPlayer && !p_unitGO.unit.OwnerPlayer.IsAlliedWith(neighbour.terrainOwner))
			{
				return false;
			}
		}
		if (p_unitGO.goal == null)
		{
			if (_enemyVPsByDistance.Count == 0 && TurnManager.currPlayer.PlayersAtWar.Count == 0)
			{
				return true;
			}
			return false;
		}
		if (p_unitGO.unit.TotalDistance < p_unitGO.unit.MaxMP && GameData.Instance.map.turnCount > 1)
		{
			return false;
		}
		bool flag = p_unitGO.unit.Type == "Plane";
		if (flag && _goingForRepair)
		{
			return false;
		}
		if (!p_unitGO.unit.CanBeRedeployed())
		{
			return false;
		}
		if (p_unitGO.AIpathToVP.Count == 0)
		{
			return false;
		}
		int num = 0;
		if (flag)
		{
			num = Utils.GetHeuristicDistance(p_unitGO.tileGO.tile, p_unitGO.goal.tile);
		}
		else
		{
			foreach (TileGO item in p_unitGO.AIpathToVP)
			{
				num += item.tile.MPCost;
			}
		}
		int turnsForMPCost = GetTurnsForMPCost(num, p_unitGO.unit.MaxMP);
		if (turnsForMPCost < 10)
		{
			return false;
		}
		if (!TryGetClosestRedeployTileToGoal(p_unitGO, out var _, out var o_distanceToGoal))
		{
			return false;
		}
		float num2 = (float)num / (float)Utils.GetHeuristicDistance(p_unitGO.tileGO.tile, p_unitGO.goal.tile);
		int p_MPCost = Mathf.CeilToInt((float)o_distanceToGoal * num2);
		int num3 = GetRedeployDelayInTurns(p_unitGO.unit) + GetTurnsForMPCost(p_MPCost, p_unitGO.unit.MaxMP);
		if (turnsForMPCost - num3 < 3)
		{
			return false;
		}
		if ((float)num3 > (float)turnsForMPCost * 0.75f)
		{
			return false;
		}
		return true;
	}

	private bool TryGetClosestRedeployTileToGoal(UnitGO p_unitGO, out TileGO o_spawnTileGO, out int o_distanceToGoal)
	{
		o_spawnTileGO = null;
		o_distanceToGoal = int.MaxValue;
		Tile tile = p_unitGO.goal.tile;
		bool flag = p_unitGO.unit.Type != "Plane";
		byte turnToProduce = p_unitGO.unit.TurnToProduce;
		p_unitGO.unit.IsReserve = true;
		p_unitGO.unit.TurnToProduce = 0;
		try
		{
			reservePanel.PrepareSpawningZone(p_unitGO.unit);
		}
		finally
		{
			p_unitGO.unit.IsReserve = false;
			p_unitGO.unit.TurnToProduce = turnToProduce;
		}
		foreach (TileGO listOfSpawnableTile in MapGO.listOfSpawnableTiles)
		{
			if (!flag || (listOfSpawnableTile.tile.enumType != TileTypes.SEA && listOfSpawnableTile.tile.landAreaIndex == tile.landAreaIndex))
			{
				int heuristicDistance = Utils.GetHeuristicDistance(listOfSpawnableTile.tile, tile);
				if (heuristicDistance < o_distanceToGoal)
				{
					o_spawnTileGO = listOfSpawnableTile;
					o_distanceToGoal = heuristicDistance;
				}
			}
		}
		return o_spawnTileGO != null;
	}

	private int GetRedeployDelayInTurns(Unit p_unit)
	{
		if (_productionTakesTimeON)
		{
			return Mathf.Max(1, p_unit.GetRedeploymentTime());
		}
		return 1;
	}

	private static int GetTurnsForMPCost(int p_MPCost, int p_maxMP)
	{
		return (p_MPCost + p_maxMP - 1) / p_maxMP;
	}

	private bool ShouldWeAttackPlayer(string p_country)
	{
		if (!GameData.Instance.TryFindPlayerByCountry(p_country, out var o_player))
		{
			return false;
		}
		if (o_player.isAtWarWithCurrentPlayer)
		{
			return true;
		}
		if (o_player.isAlliedWithCurrentPlayer)
		{
			return false;
		}
		if (TurnManager.currPlayer.HasTruceWith(o_player))
		{
			return false;
		}
		if (_isHistoricalFocusON)
		{
			if (_currPlayerCountry == "Nazi Germany")
			{
				switch (p_country)
				{
				case "Denmark":
					if (_currDate < new DateTime(1940, 4, 1))
					{
						return false;
					}
					break;
				case "Norway":
					if (_currDate < new DateTime(1940, 5, 1))
					{
						return false;
					}
					break;
				case "Netherlands":
					if (_currDate < new DateTime(1940, 5, 9))
					{
						return false;
					}
					break;
				case "France":
				case "Belgium":
				case "Luxembourg":
					if (_currDate < new DateTime(1940, 6, 1))
					{
						return false;
					}
					break;
				case "Great Britain":
				case "Canada":
					if (_currDate < new DateTime(1941, 4, 2))
					{
						return false;
					}
					break;
				case "Yugoslavia":
				case "Greece":
					if (_currDate < new DateTime(1941, 4, 1))
					{
						return false;
					}
					break;
				case "Soviet Union":
				{
					if (_currDate < new DateTime(1941, 6, 22))
					{
						return false;
					}
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					foreach (KeyValuePair<Player, int> item in _dicOfPlayersAndTheirIncome)
					{
						if (item.Key.isAlliedWithCurrentPlayer)
						{
							num += item.Value;
						}
						else if (item.Key.isAtWarWithCurrentPlayer)
						{
							num2 += item.Value;
						}
						else if (item.Key.Country == p_country)
						{
							num3 += item.Value;
						}
					}
					if (num > num3)
					{
						return num > num2;
					}
					return false;
				}
				case "USA":
					if (_currDate < new DateTime(1941, 12, 11))
					{
						return false;
					}
					break;
				case "Vichy France":
					if (_currDate < new DateTime(1942, 12, 1))
					{
						return false;
					}
					break;
				default:
					return false;
				}
			}
			else if (_currPlayerCountry == "Soviet Union")
			{
				switch (p_country)
				{
				case "Poland":
					if (_currDate < new DateTime(1939, 9, 17))
					{
						return false;
					}
					break;
				case "Finland":
					if (_currDate < new DateTime(1939, 11, 30))
					{
						return false;
					}
					if (_currDate >= new DateTime(1940, 3, 13) && _currDate <= new DateTime(1941, 6, 1))
					{
						return false;
					}
					break;
				case "Lithuania":
				case "Estonia":
				case "Latvia":
					if (_currDate < new DateTime(1940, 6, 1))
					{
						return false;
					}
					break;
				case "Japan":
				case "Manchukuo":
					if (_currDate < new DateTime(1945, 8, 7) || _currDate >= new DateTime(1960, 8, 7))
					{
						return false;
					}
					break;
				case "Iran":
					return _currDate >= new DateTime(1941, 8, 25);
				default:
					return false;
				}
			}
			else if (_currPlayerCountry == "Italy")
			{
				switch (p_country)
				{
				case "Yugoslavia":
					return _currDate >= new DateTime(1941, 4, 1);
				case "Greece":
					return _currDate >= new DateTime(1940, 10, 28);
				case "Soviet Union":
				{
					if (GameData.Instance.TryFindPlayerByCountry("Nazi Germany", out var o_player2) && o_player2.isAlliedWithCurrentPlayer && o_player2.Faction == TurnManager.currPlayer.Faction && o_player2.IsAtWarWith(o_player))
					{
						return true;
					}
					return false;
				}
				case "Vichy France":
					break;
				default:
					return false;
				}
				if (_currDate < new DateTime(1942, 12, 1))
				{
					return false;
				}
			}
			else
			{
				if (!(_currPlayerCountry == "Japan"))
				{
					if (_currPlayerCountry == "Great Britain")
					{
						if (p_country == "Iraq")
						{
							return _currDate > new DateTime(1941, 5, 2);
						}
						if (p_country == "Vichy France")
						{
							foreach (string alliedPlayer in TurnManager.currPlayer.AlliedPlayers)
							{
								if (GameData.Instance.TryFindPlayerByName(alliedPlayer, out var o_player3) && o_player3.IsAtWarWith(o_player))
								{
									return true;
								}
							}
							return _currDate >= new DateTime(1944, 6, 1);
						}
						if (p_country == "Iran")
						{
							_ = _currDate < new DateTime(1941, 8, 25);
							return false;
						}
						if (p_country == "Vichy France")
						{
							_ = _currDate < new DateTime(1942, 12, 1);
							return false;
						}
						return false;
					}
					if (_currPlayerCountry == "USA")
					{
						if (p_country == "Vichy France")
						{
							foreach (string alliedPlayer2 in TurnManager.currPlayer.AlliedPlayers)
							{
								if (GameData.Instance.TryFindPlayerByName(alliedPlayer2, out var o_player4) && o_player4.IsAtWarWith(o_player))
								{
									return true;
								}
							}
							return _currDate >= new DateTime(1942, 11, 1);
						}
						return false;
					}
					if (GameData.Instance.TryFindPlayerByCountry("Nazi Germany", out var o_player5) && o_player5.isAlliedWithCurrentPlayer && o_player5.Faction == TurnManager.currPlayer.Faction && o_player5.IsAtWarWith(o_player))
					{
						return true;
					}
					return false;
				}
				switch (p_country)
				{
				case "Soviet Union":
					return false;
				case "USA":
				case "Great Britain":
				case "Thailand":
				case "British Raj":
				case "Australia":
				case "New Zealand":
				case "Netherlands":
				case "Philippines":
				case "Mongolia":
				case "France":
				case "Portugal":
					break;
				default:
					return false;
				}
				if (_currDate < new DateTime(1941, 12, 8))
				{
					return false;
				}
			}
		}
		int num4 = 0;
		int num5 = 0;
		foreach (KeyValuePair<Player, int> item2 in _dicOfPlayersAndTheirIncome)
		{
			if (item2.Key.isAlliedWithCurrentPlayer)
			{
				num4 += item2.Value;
			}
			else if (item2.Key.Country == p_country || item2.Key.isAtWarWithCurrentPlayer)
			{
				num5 += item2.Value;
			}
		}
		return (float)num4 >= 1.5f * (float)num5;
	}

	private void DeclareWarOnCountry(UnitGO p_unitGO, TileGO p_tileGO)
	{
		Player player = p_tileGO.tile.ownerPlayer;
		if (player == null)
		{
			player = p_tileGO.tile.terrainOwner;
			if (player == null)
			{
				return;
			}
		}
		if (p_unitGO.unit.FilterType == "FilterBoat" || p_unitGO.unit.FilterType == "FilterBuilding" || player.isAtWarWithCurrentPlayer || !ShouldWeAttackPlayer(player.Country))
		{
			return;
		}
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Declare war"));
		UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, player.Country, LocalizationManager.Translate("relationship-war declaration"));
		if (player.Faction == "Neutral")
		{
			if (player.Ideology == TurnManager.currPlayer.Ideology && player.Ideology != "Neutral" && (TurnManager.currPlayer.Faction == "Axis" || TurnManager.currPlayer.Faction == "Allies" || TurnManager.currPlayer.Faction == "Communists"))
			{
				player.ChangeFaction(player.Country, p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (TurnManager.currPlayer.Faction != "Communists" && player.Ideology == "Communism")
			{
				player.ChangeFaction("Communists", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (TurnManager.currPlayer.Faction == "Axis")
			{
				player.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (TurnManager.currPlayer.Faction == "Allies")
			{
				player.ChangeFaction("Axis", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (player.Ideology == "Fascism" || player.Ideology == "National Socialism")
			{
				player.ChangeFaction("Axis", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (player.Ideology == "Democracy")
			{
				player.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else if (player.Ideology == "Communism")
			{
				player.ChangeFaction("Communists", p_getNewAllianceWars: true, p_sendRPC: true);
			}
			else
			{
				player.ChangeFaction(player.Country, p_getNewAllianceWars: true, p_sendRPC: true);
			}
		}
		TurnManager.currPlayer.AddWarWith(player, p_actualDirectWarDeclaration: true, p_sendRPC: true);
		MultiplayerManager.RPC("RPC_WarDeclaration", TurnManager.currPlayer.Name, player.Name);
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
	}

	private bool CanDealDamage(UnitGO p_unitGO)
	{
		if (p_unitGO.unit.IsCommander || p_unitGO.unit.IsMine || p_unitGO.unit.IsSupplyDepot)
		{
			return false;
		}
		if (p_unitGO.unit.IsParatrooper && p_unitGO.unit.IsDropable)
		{
			return false;
		}
		int p_radius = p_unitGO.unit.Range;
		if (p_unitGO.unit.IsOnBoat)
		{
			p_radius = 1;
		}
		bool flag = p_unitGO.unit.FilterType == "FilterArtillery";
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_radius))
		{
			if (item.isVisible && item.tileGO.groundUnitGO != null && !item.tileGO.groundUnitGO.unit.IsMine && item.tileGO.groundUnitGO.unit.OwnerPlayer != null && item.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				p_unitGO.unit.GetPotentialDamage(item.tileGO.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				if ((p_unitGO.unit.FinalDamage > 0) | flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CheckForAttack(UnitGO p_unitGO)
	{
		_tileGOForAttack = null;
		if (p_unitGO == null || p_unitGO.unit.HasAttacked || p_unitGO.unit.ActionPoints <= 0 || p_unitGO.unit.IsReserve || p_unitGO.unit.CurrAmmo == 0 || p_unitGO.unit.IsSupplyShip || p_unitGO.unit.Range <= 0)
		{
			return;
		}
		p_unitGO.ShowPotentialTargets();
		_dicOfDamagePerUnit.Clear();
		_listOfUnitsWeCanKill.Clear();
		foreach (GameObject item in MapGO.listOfTargetedUnit)
		{
			TileGO component = item.GetComponent<TileGO>();
			if ((p_unitGO.unit.FilterType == "FilterAA" || p_unitGO.unit.FilterType == "FilterFighter" || p_unitGO.unit.FilterType == "FilterCAS") && component.airUnitGO != null && component.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				int potentialDamage = p_unitGO.unit.GetPotentialDamage(component.airUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				if (p_unitGO.unit.Type != "Plane" || CanPlaneAttackSafely(p_unitGO, component.tile, potentialDamage))
				{
					if (!_dicOfDamagePerUnit.ContainsKey(component.airUnitGO))
					{
						_dicOfDamagePerUnit.Add(component.airUnitGO, potentialDamage);
					}
					if (component.airUnitGO.unit.CurrHP - potentialDamage <= 0)
					{
						_listOfUnitsWeCanKill.Add(component.airUnitGO);
					}
				}
			}
			if (p_unitGO.unit.Type == "Plane")
			{
				if (!(p_unitGO.tileGO.gameObject == item))
				{
					continue;
				}
				if (component.tile.MoneyProductionPerTurn > 0 && component.tile.NumberOfTurnsAtLowProd == 0 && (p_unitGO.unit.IsMissile || (!p_unitGO.unit.IsRecon && p_unitGO.unit.FilterType == "FilterBomber")) && p_unitGO.tileGO.groundUnitGO == null)
				{
					_tileGOForAttack = component;
				}
				if (!(component.groundUnitGO != null) || component.groundUnitGO.unit.IsMine || !component.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					continue;
				}
				int potentialDamage2 = p_unitGO.unit.GetPotentialDamage(component.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				if (CanPlaneAttackSafely(p_unitGO, component.tile, potentialDamage2))
				{
					if (!_dicOfDamagePerUnit.ContainsKey(component.groundUnitGO))
					{
						_dicOfDamagePerUnit.Add(component.groundUnitGO, potentialDamage2);
					}
					if (component.groundUnitGO.unit.CurrHP - potentialDamage2 <= 0)
					{
						_listOfUnitsWeCanKill.Add(component.groundUnitGO);
					}
				}
			}
			else if (component.groundUnitGO != null && !component.groundUnitGO.unit.IsMine && component.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				int potentialDamage3 = p_unitGO.unit.GetPotentialDamage(component.groundUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				component.groundUnitGO.unit.GetPotentialDamage(p_unitGO, p_isRetaliation: true, p_isSupportiveFire: false);
				if (!_dicOfDamagePerUnit.ContainsKey(component.groundUnitGO))
				{
					_dicOfDamagePerUnit.Add(component.groundUnitGO, potentialDamage3);
				}
				if (component.groundUnitGO.unit.CurrHP - p_unitGO.unit.FinalDamage <= 0)
				{
					_listOfUnitsWeCanKill.Add(component.groundUnitGO);
				}
			}
		}
		if (_dicOfDamagePerUnit.Count == 0)
		{
			return;
		}
		if (_listOfUnitsWeCanKill.Count > 0)
		{
			_listOfUnitsWeCanKill.OrderByDescending((UnitGO x) => x.unit.Cost);
			_tileGOForAttack = _listOfUnitsWeCanKill.ElementAt(0).tileGO;
			p_unitGO.targetedUnitGO = _listOfUnitsWeCanKill.ElementAt(0);
		}
		else
		{
			if (p_unitGO.unit.FilterType == "FilterArtillery" || p_unitGO.unit.FilterType == "FilterBoat" || p_unitGO.unit.FilterType == "FilterCAS" || p_unitGO.unit.FilterType == "FilterBomber")
			{
				bool flag = false;
				foreach (KeyValuePair<UnitGO, int> item2 in _dicOfDamagePerUnit)
				{
					if (item2.Key.tileGO.tile.IsVictoryPoint)
					{
						foreach (Tile neighbour in item2.Key.tileGO.tile.neighbours)
						{
							if (neighbour.GroundUnit != null && neighbour.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					_dicOfDamagePerUnit = (from x in _dicOfDamagePerUnit
						orderby x.Key.tileGO.tile.IsVictoryPoint descending, x.Value descending
						select x).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				}
				else
				{
					_dicOfDamagePerUnit = _dicOfDamagePerUnit.OrderByDescending((KeyValuePair<UnitGO, int> x) => x.Value).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				}
			}
			else
			{
				_dicOfDamagePerUnit = (from x in _dicOfDamagePerUnit
					orderby x.Value descending, x.Key.unit.FinalDamage
					select x).ToDictionary((KeyValuePair<UnitGO, int> x) => x.Key, (KeyValuePair<UnitGO, int> x) => x.Value);
				if (_dicOfDamagePerUnit.ElementAt(0).Value == 0)
				{
					return;
				}
			}
			TileGO tileGOForAttack = null;
			UnitGO targetedUnitGO = null;
			float num = float.NegativeInfinity;
			foreach (KeyValuePair<UnitGO, int> item3 in _dicOfDamagePerUnit)
			{
				UnitGO key = item3.Key;
				TileGO tileGO = key.tileGO;
				float attackRiskRewardRatio = GetAttackRiskRewardRatio(p_unitGO, key);
				if (!(attackRiskRewardRatio < 0f) && attackRiskRewardRatio > num)
				{
					num = attackRiskRewardRatio;
					tileGOForAttack = tileGO;
					targetedUnitGO = key;
				}
			}
			_tileGOForAttack = tileGOForAttack;
			p_unitGO.targetedUnitGO = targetedUnitGO;
			if (_tileGOForAttack == null || p_unitGO.targetedUnitGO == null)
			{
				return;
			}
		}
		DoResupply(p_unitGO, p_ignoreHP: true);
		p_unitGO.unit.GetPotentialDamage(p_unitGO.targetedUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
		p_unitGO.targetedUnitGO.unit.GetPotentialDamage(p_unitGO, p_isRetaliation: true, p_isSupportiveFire: false);
	}

	private void SendMoneyToAllies()
	{
		if (_isMinor || _currentTurnCount <= 1 || TurnManager.currPlayer.AlliedPlayers.Count == 0)
		{
			return;
		}
		CalculateIncomeForAllPlayers();
		int num = 0;
		int num2 = Mathf.Max(0, TurnManager.currPlayer.MoneyAmount - _dicOfPlayersAndTheirIncome[TurnManager.currPlayer]);
		if (num2 <= 0)
		{
			return;
		}
		if (num2 > TurnManager.currPlayer.baseIncome)
		{
			num2 = TurnManager.currPlayer.baseIncome;
		}
		foreach (KeyValuePair<Player, int> item in _dicOfPlayersAndTheirIncome)
		{
			if (item.Key.isAlliedWithCurrentPlayer && !((float)item.Key.MoneyAmount >= (float)item.Value * 0.5f))
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		int num3 = num2 / num;
		if (num3 < 10)
		{
			return;
		}
		foreach (KeyValuePair<Player, int> item2 in _dicOfPlayersAndTheirIncome)
		{
			if (item2.Key.isAlliedWithCurrentPlayer && !((float)item2.Key.MoneyAmount >= (float)item2.Value * 0.5f))
			{
				int num4 = Mathf.Clamp(num3, 0, _dicOfPlayersAndTheirIncome[item2.Key]);
				new Notification(NotificationTypes.MONEY_TRANSFER, item2.Key.Name, string.Empty, num4, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
				item2.Key.AddMoney(num4, p_sendRPC: true);
				TurnManager.currPlayer.RemoveMoney(num4, p_sendRPC: true);
			}
		}
	}

	private void CalculateIncomeForAllPlayers()
	{
		_baseCoreTerritory = 0;
		_currentCoreTerritory = 0;
		_coreTerritoryPercent = 0f;
		_numberOfRivers = 0;
		_numberOfSeaTiles = 0;
		_numberOfDesertTiles = 0;
		_numberOfForestTiles = 0;
		_numberOfHillsAndMountains = 0;
		_dicOfPlayersAndTheirIncome.Clear();
		_our_production = 0;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.CalculateTurnIncome();
			_dicOfPlayersAndTheirIncome.Add(listOfPlayer, listOfPlayer.baseIncome);
			if (listOfPlayer == TurnManager.currPlayer)
			{
				_our_production = listOfPlayer.turnIncome;
			}
		}
		for (short num = 0; num < _mapSizeX; num++)
		{
			for (short num2 = 0; num2 < _mapSizeY; num2++)
			{
				if (_tiles[num, num2].enumSeason == TileSeason.DESERT)
				{
					_numberOfDesertTiles++;
				}
				if (_tiles[num, num2].enumType == TileTypes.FOREST)
				{
					_numberOfForestTiles++;
				}
				else if (_tiles[num, num2].enumType == TileTypes.HILL || _tiles[num, num2].enumType == TileTypes.MOUNTAIN)
				{
					_numberOfHillsAndMountains++;
				}
				else if (_tiles[num, num2].enumType == TileTypes.SEA)
				{
					_numberOfSeaTiles++;
				}
				else if (_tiles[num, num2].enumType == TileTypes.RIVER)
				{
					_numberOfRivers++;
				}
				if (_tiles[num, num2].ownerPlayer != null && _tiles[num, num2].OriginalCountryOwner == _currPlayerCountry)
				{
					_baseCoreTerritory++;
					if (_tiles[num, num2].ownerPlayer.Country == _currPlayerCountry)
					{
						_currentCoreTerritory++;
					}
				}
			}
		}
		_coreTerritoryPercent = (float)_currentCoreTerritory / (float)_baseCoreTerritory;
		_forestPercentage = (float)_numberOfForestTiles / (float)_totalMapSize;
		_seaTilesPercentage = (float)_numberOfSeaTiles / (float)_totalMapSize;
		_desertTilesPercentage = (float)_numberOfDesertTiles / (float)_totalMapSize;
		_hillAndMountainsPercentage = (float)_numberOfHillsAndMountains / (float)_totalMapSize;
		_forestPercentage *= 100f;
		_seaTilesPercentage *= 100f;
		_desertTilesPercentage *= 100f;
		_hillAndMountainsPercentage *= 100f;
	}

	private void PickPolicies()
	{
		if (!PlayerSettings.Instance.Policies || TurnManager.currPlayer.HighCommandPoints <= 0 || (TurnManager.currPlayer.HighCommandPoints <= 35 && UnityEngine.Random.Range(0, 100) <= 85) || (TurnManager.currPlayer.HighCommandPoints <= 50 && UnityEngine.Random.Range(0, 100) <= 70) || (TurnManager.currPlayer.HighCommandPoints <= 75 && UnityEngine.Random.Range(0, 100) <= 50) || (TurnManager.currPlayer.HighCommandPoints <= 100 && UnityEngine.Random.Range(0, 100) <= 25))
		{
			return;
		}
		Policies.InitializePolicies();
		List<Policy> list = new List<Policy>();
		if (PlayerSettings.Instance.TechTree)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Military_scientists_1));
			list.Add(Policies.GetPolicy(Policies.Type.Military_scientists_2));
			if (!_isHistoricalFocusON)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Better_scientists));
			}
		}
		if (_hasHeavyProductionMeans)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Industrial_overtime));
		}
		if (_manpowerSettingON && _currentAvailableManpower <= 15000 && GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0 && GameData.Instance.map.NumberOfDaysThatGoesByPerTurn * _currentTurnCount / 31 >= 6)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Aggressive_propaganda));
		}
		if (_numberOfOurCommanders > 0 && _upkeepSettingON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Commanders_development_program));
		}
		if (PlayerSettings.Instance.Partisans)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Resistance_suppresion));
		}
		if (_manpowerSettingON || PlayerSettings.Instance.TechTree)
		{
			if (!_manpowerSettingON || _currentAvailableManpower <= 30000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Total_mobilization));
			}
			else if (!_manpowerSettingON || _currentAvailableManpower <= 50000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Partial_mobilization));
			}
			else if (!_manpowerSettingON || _currentAvailableManpower <= 100000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Early_mobilization));
			}
		}
		list.Add(Policies.GetPolicy(Policies.Type.Urban_tactics));
		if (_hillAndMountainsPercentage >= 5f || _currPlayerCountry == "Greece" || _currPlayerCountry == "Yugoslavia" || _currPlayerCountry == "Turkey")
		{
			list.Add(Policies.GetPolicy(Policies.Type.Highlands_experts));
		}
		if (_forestPercentage >= 5f)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Forest_tactics));
		}
		if (_canPerformLandings && _numberOfOurLandingCrafts > 0)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Landing_expert));
		}
		if (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter && _currDate.Month >= 11)
		{
			Policy policy = Policies.GetPolicy(Policies.Type.Winter_soldiers);
			if (_currPlayerCountry == "Finland" || _currPlayerCountry == "Norway" || _currPlayerCountry == "Sweden")
			{
				policy.HighCommandPointsCost = 0;
			}
			list.Add(policy);
		}
		if (_desertTilesPercentage >= 10f && (_currPlayerCountry == "Great Britain" || _currPlayerCountry == "Italy"))
		{
			list.Add(Policies.GetPolicy(Policies.Type.Desert_rats));
		}
		if (_currPlayerCountry == "Nazi Germany")
		{
			if (_numberOfOurSubmarines >= 3)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Wolfpack));
			}
		}
		else if (_currPlayerCountry == "Japan")
		{
			if (!_areWeWinning)
			{
				list.Add(Policies.GetPolicy(Policies.Type.No_step_back));
			}
		}
		else if (_currPlayerCountry == "China")
		{
			list.Add(Policies.GetPolicy(Policies.Type.Scorched_earth));
		}
		else if (_currPlayerCountry == "Soviet Union")
		{
			list.Add(Policies.GetPolicy(Policies.Type.Human_wave));
			if (!_areWeWinning)
			{
				list.Add(Policies.GetPolicy(Policies.Type.No_step_back));
				list.Add(Policies.GetPolicy(Policies.Type.Scorched_earth));
			}
		}
		if (!_isHistoricalFocusON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Human_wave));
			list.Add(Policies.GetPolicy(Policies.Type.No_step_back));
			list.Add(Policies.GetPolicy(Policies.Type.Scorched_earth));
			if (_numberOfOurSubmarines >= 3)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Wolfpack));
			}
		}
		if (_isMinor)
		{
			list.Add(Policies.GetPolicy(Policies.Type.ImportFocus));
		}
		if (_hasInfantry)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Infantry_focus));
			list.Add(Policies.GetPolicy(Policies.Type.Recruitment_boost_infantry));
		}
		if (_hasTanks && _hasHeavyProductionMeans)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Armoured_focus));
			list.Add(Policies.GetPolicy(Policies.Type.Recruitment_boost_armoured));
		}
		if (_hasAirport && (_hasFighters || _hasCAS || _hasStrategicBombers))
		{
			list.Add(Policies.GetPolicy(Policies.Type.Air_focus));
			list.Add(Policies.GetPolicy(Policies.Type.Recruitment_boost_air));
		}
		if (_hasHarbour && _hasShips)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Naval_focus));
			list.Add(Policies.GetPolicy(Policies.Type.Recruitment_boost_naval));
		}
		if (_hasArtillery && _hasHeavyProductionMeans)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Guns_focus));
			list.Add(Policies.GetPolicy(Policies.Type.Recruitment_boost_guns));
		}
		if (_manpowerSettingON)
		{
			if (_currentAvailableManpower <= 50000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Conscription3));
			}
			else if (_currentAvailableManpower <= 100000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Conscription2));
			}
			else if (_currentAvailableManpower <= 150000)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Conscription1));
			}
		}
		if (!_areWeWinning)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Improved_hospitals));
		}
		if (_numberOfOurRadars >= 3 && _numberOfEnemyPlanes > 1)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Improved_radars));
		}
		if (_numberOfEnemyHeavyBombers > 0)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Minimized_destruction));
			list.Add(Policies.GetPolicy(Policies.Type.Efficient_workers));
		}
		if (PlayerSettings.Instance.Partisans)
		{
			if (TurnManager.currPlayer.Ideology == "Fascism" || TurnManager.currPlayer.Ideology == "National Socialism" || TurnManager.currPlayer.Ideology == "Communism")
			{
				list.Add(Policies.GetPolicy(Policies.Type.Balanced_Approach));
				list.Add(Policies.GetPolicy(Policies.Type.Aggressive_Approach));
				list.Add(Policies.GetPolicy(Policies.Type.Secret_Police));
				list.Add(Policies.GetPolicy(Policies.Type.Forced_labor));
			}
			else
			{
				list.Add(Policies.GetPolicy(Policies.Type.Balanced_Approach));
			}
		}
		if (!_areWeWinning)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Dug_deep));
		}
		if (!_areWeWinning)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Camouflage));
		}
		if (!_areWeWinning && _numberOfOurTanks >= 3 && _currYear >= 1943)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Armoured_skirts));
		}
		if (!_areWeWinning && _numberOfEnemyTanks >= 5 && _currYear >= 1942)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Magnetic_grenades));
		}
		if (!_areWeWinning && _numberOfEnemyTanks >= 5 && _numberOfOurAntitanks >= 3)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Improved_AP_shells));
		}
		if (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter && _currDate.Month >= 11)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Winter_trained));
		}
		if (!_isHistoricalFocusON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Improved_HQ));
		}
		list.Add(Policies.GetPolicy(Policies.Type.Cheap_supplies));
		if (!PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Organized_supplyLines));
		}
		if (_numberOfOurArtillery >= 3)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Artillery_barrage));
		}
		if (_areWeWinning && _numberOfOurTanks >= 5 && _numberOfOurBombers > 1)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Blitzkrieg));
		}
		if (_numberOfOurFighters >= 3)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Air_supremacy));
		}
		if (_numberOfOurPlanes >= 5)
		{
			list.Add(Policies.GetPolicy(Policies.Type.All_weather));
		}
		if (_numberOfOurShips >= 5)
		{
			if (_isNavalPower)
			{
				list.Add(Policies.GetPolicy(Policies.Type.Shore_bombardment));
			}
			if (_isHistoricalFocusON)
			{
				if (_isNavalMinor || _isNavalPower)
				{
					list.Add(Policies.GetPolicy(Policies.Type.Fleet_in_being));
				}
			}
			else
			{
				list.Add(Policies.GetPolicy(Policies.Type.Fleet_in_being));
			}
		}
		if (!_areWeWinning)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Organized_defenses));
		}
		if (!_areWeWinning && _numberOfOurAA >= 5 && _currYear >= 1900 && _upkeepSettingON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Superior_bombing_protection));
		}
		if (!_isHistoricalFocusON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Lessons_of_war));
		}
		if (!_isHistoricalFocusON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.War_experience));
		}
		if (!_isHistoricalFocusON)
		{
			list.Add(Policies.GetPolicy(Policies.Type.Strategic_Flexibility));
		}
		List<Policy> list2 = new List<Policy>();
		foreach (Policy item in list)
		{
			if (item != null && !list2.Contains(item))
			{
				list2.Add(item);
			}
		}
		if (list2.Count != 0)
		{
			_randNumber = UnityEngine.Random.Range(0, list2.Count);
			if (list2[_randNumber].HighCommandPointsCost <= TurnManager.currPlayer.HighCommandPoints)
			{
				Policy p_policy = list2.ElementAt(_randNumber);
				TurnManager.currPlayer.PurchasePolicy(p_policy, p_sendRPC: true);
			}
		}
	}

	private void ResearchTechnologies()
	{
		if (!PlayerSettings.Instance.TechTree || TurnManager.currPlayer.NumberOfAvailableResearchPoints <= 0)
		{
			return;
		}
		List<Unit> list = new List<Unit>();
		foreach (Unit item in from x in MapGO.instance.listOfAllUnits
			orderby x.ProductionYear, x.ProductionMonth
			select x)
		{
			if (item.IsCommander || item.Country != _currPlayerCountry || (item.IsPrototype && (_isHistoricalFocusON || PlayerSettings.Instance.PrototypesDisabled)) || TurnManager.currPlayer.ListOfUnitsResearched.Contains(item.Name))
			{
				continue;
			}
			bool flag = false;
			foreach (string key in TurnManager.currPlayer.CurrentUnitsResearch.Keys)
			{
				if (key == item.Name)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int num = 0;
		while (TurnManager.currPlayer.NumberOfAvailableResearchPoints > 0 && list.Count != 0)
		{
			if (num <= list.Count - 1)
			{
				Unit p_unit = list.ElementAt(num);
				TurnManager.currPlayer.StartResearch(p_unit, p_sendRPC: true);
				list.RemoveAt(num);
			}
			num++;
			if (num > 500)
			{
				break;
			}
		}
	}

	private void CountEnemyUnits(Player p_playerReference)
	{
		_enemyHasTanks = false;
		_enemyHasPlanes = false;
		_enemyHasShips = false;
		_numberOfEnemyAT = 0;
		_numberOfEnemyLandMines = 0;
		_numberOfEnemyTanks = 0;
		_numberOfEnemyShips = 0;
		_numberOfEnemySubmarines = 0;
		_numberOfEnemyPlanes = 0;
		_numberOfEnemyFighters = 0;
		_numberOfEnemyHeavyBombers = 0;
		_numberOfEnemyTorpedoBombers = 0;
		_numberOfAlliedFighters = 0;
		if (p_playerReference.PlayersAtWar.Count == 0)
		{
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!listOfPlayer.IsAtWarWith(p_playerReference))
			{
				continue;
			}
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				if (listOfUnit.MaxMP > 0 && !listOfUnit.IsOnBoat && !listOfUnit.IsParatrooper)
				{
					if (listOfUnit.Type == "Plane")
					{
						_enemyHasPlanes = true;
					}
					else if (listOfUnit.Type == "Tank")
					{
						_enemyHasTanks = true;
					}
					else if (listOfUnit.FilterType == "FilterBoat")
					{
						_enemyHasShips = true;
					}
				}
			}
		}
		for (short num = 0; num < _mapSizeX; num++)
		{
			for (short num2 = 0; num2 < _mapSizeY; num2++)
			{
				if (_tiles[num, num2].isVisible)
				{
					if (_tiles[num, num2].tileGO.airUnitGO != null && _tiles[num, num2].tileGO.airUnitGO.unit.OwnerPlayer != null && _tiles[num, num2].tileGO.airUnitGO.unit.MaxMP > 0 && !_tiles[num, num2].tileGO.airUnitGO.unit.IsParatrooper && !_tiles[num, num2].tileGO.airUnitGO.unit.IsRecon && !_tiles[num, num2].tileGO.airUnitGO.unit.IsSupplyShip)
					{
						if (_tiles[num, num2].tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
						{
							_numberOfEnemyPlanes++;
							if (_tiles[num, num2].tileGO.airUnitGO.unit.FilterType == "FilterFighter")
							{
								_numberOfEnemyFighters++;
							}
							else
							{
								if (_tiles[num, num2].tileGO.airUnitGO.unit.FilterType == "FilterBomber")
								{
									_numberOfEnemyHeavyBombers++;
								}
								if (_tiles[num, num2].tileGO.airUnitGO.unit.HasTorpedo)
								{
									_numberOfEnemyTorpedoBombers++;
								}
							}
						}
						else if (_tiles[num, num2].tileGO.airUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer && _tiles[num, num2].tileGO.airUnitGO.unit.FilterType == "FilterFighter")
						{
							_numberOfAlliedFighters++;
						}
					}
					if (_tiles[num, num2].tileGO.groundUnitGO != null && _tiles[num, num2].tileGO.groundUnitGO.unit.OwnerPlayer != null && _tiles[num, num2].tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
					{
						if (_tiles[num, num2].tileGO.groundUnitGO.unit.FilterType == "FilterBoat")
						{
							_numberOfEnemyShips++;
							if (_tiles[num, num2].tileGO.groundUnitGO.unit.IsSubmarine)
							{
								_numberOfEnemySubmarines++;
							}
						}
						else
						{
							if (_tiles[num, num2].tileGO.groundUnitGO.unit.IsMine)
							{
								_numberOfEnemyLandMines++;
							}
							if (_tiles[num, num2].tileGO.groundUnitGO.unit.Type == "Tank" && _tiles[num, num2].tileGO.groundUnitGO.unit.MaxMP > 0)
							{
								_numberOfEnemyTanks++;
							}
							if (_tiles[num, num2].tileGO.groundUnitGO.unit.FilterType == "FilterAntitank")
							{
								_numberOfEnemyAT++;
							}
						}
					}
				}
				else
				{
					if (!_enemyHasPlanes && _tiles[num, num2].tileGO.airUnitGO != null && _tiles[num, num2].tileGO.airUnitGO.unit.Type == "Plane" && _tiles[num, num2].tileGO.airUnitGO.unit.MaxMP > 0 && !_tiles[num, num2].tileGO.airUnitGO.unit.IsParatrooper)
					{
						_enemyHasPlanes = true;
					}
					if (!_enemyHasTanks && _tiles[num, num2].tileGO.groundUnitGO != null && _tiles[num, num2].tileGO.groundUnitGO.unit.Type == "Tank" && _tiles[num, num2].tileGO.groundUnitGO.unit.MaxMP > 0 && _tiles[num, num2].tileGO.groundUnitGO.unit.Armour > 0)
					{
						_enemyHasTanks = true;
					}
					if (!_enemyHasShips && _tiles[num, num2].tileGO.groundUnitGO != null && _tiles[num, num2].tileGO.groundUnitGO.unit.FilterType == "FilterBoat" && _tiles[num, num2].tileGO.groundUnitGO.unit.MaxMP > 0)
					{
						_enemyHasShips = true;
					}
				}
				if (_numberOfEnemyPlanes > 0)
				{
					_enemyHasPlanes = true;
				}
				if (_numberOfEnemyTanks > 0)
				{
					_enemyHasTanks = true;
				}
				if (_numberOfEnemyShips > 0)
				{
					_enemyHasShips = true;
				}
			}
		}
	}

	private void CountOwnUnits(Player p_currentPlayer)
	{
		_ownsEngineers = false;
		_numberOfOurShips = 0;
		_numberOfOurLandingCrafts = 0;
		_numberOfOurDestroyers = 0;
		_numberOfOurFighters = 0;
		_numberOfOurBombers = 0;
		_numberOfOurAntitanks = 0;
		_numberOfOurATinfantry = 0;
		_numberOfOurRadars = 0;
		_numberOfOurLaunchSites = 0;
		_numberOfOurDeployedLaunchSites = 0;
		_numberOfOurCommanders = 0;
		_numberOfOurArmouredTrains = 0;
		_numberOfOurPlanes = 0;
		_numberOfOurCarriers = 0;
		_numberOfOurAA = 0;
		_numberOfOurActiveAA = 0;
		_numberOfOurSubmarines = 0;
		_numberOfOurArtillery = 0;
		_numberOfOurInfantry = 0;
		_numberOfOurEngineers = 0;
		_numberOfOurTanks = 0;
		_ourPlanesToGroundUnitsRatio = 0f;
		foreach (Unit listOfUnit in p_currentPlayer.ListOfUnits)
		{
			if (listOfUnit.IsDead || listOfUnit.CurrHP == 0)
			{
				continue;
			}
			if (listOfUnit.IsCommander)
			{
				_numberOfOurCommanders++;
			}
			if (listOfUnit.IsRADAR)
			{
				_numberOfOurRadars++;
			}
			if (listOfUnit.IsLaunchSite)
			{
				_numberOfOurLaunchSites++;
			}
			if (listOfUnit.IsLaunchSite && !listOfUnit.IsReserve && listOfUnit.unitGO != null)
			{
				_numberOfOurDeployedLaunchSites++;
			}
			if (listOfUnit.IsEngineers)
			{
				_ownsEngineers = true;
				_numberOfOurEngineers++;
			}
			if (listOfUnit.FilterType == "FilterBoat")
			{
				_numberOfOurShips++;
				if (listOfUnit.IsDestroyer)
				{
					_numberOfOurDestroyers++;
				}
				if (listOfUnit.IsSubmarine)
				{
					_numberOfOurSubmarines++;
				}
				if (listOfUnit.IsCarrier)
				{
					_numberOfOurCarriers++;
					_numberOfOurShips--;
				}
				continue;
			}
			if (listOfUnit.FilterType == "FilterInfantry")
			{
				if (!listOfUnit.IsCommander && !listOfUnit.GetNameToLower().Contains("garrison"))
				{
					if (listOfUnit.IsReserve || ((bool)listOfUnit.unitGO && !listOfUnit.unitGO.tileGO.tile.IsVictoryPoint))
					{
						_numberOfOurInfantry++;
					}
					if (listOfUnit.Name.Contains("AT infantry"))
					{
						_numberOfOurATinfantry++;
					}
				}
			}
			else if (listOfUnit.FilterType == "FilterFighter" && !listOfUnit.IsRecon)
			{
				_numberOfOurFighters++;
			}
			else if (listOfUnit.FilterType == "FilterCAS" && !listOfUnit.IsRecon)
			{
				_numberOfOurBombers++;
			}
			else if (listOfUnit.FilterType == "FilterBomber" && !listOfUnit.IsRecon)
			{
				_numberOfOurBombers++;
			}
			else if (listOfUnit.FilterType == "FilterArtillery")
			{
				_numberOfOurArtillery++;
			}
			else if (listOfUnit.FilterType == "FilterAA")
			{
				_numberOfOurAA++;
				if (!listOfUnit.IsReserve && listOfUnit.unitGO != null && listOfUnit.unitGO.tileGO != null && !listOfUnit.unitGO.tileGO.tile.IsVictoryPoint)
				{
					_numberOfOurActiveAA++;
				}
			}
			else if (listOfUnit.FilterType == "FilterAntitank")
			{
				_numberOfOurAntitanks++;
			}
			if (listOfUnit.MaxMP > 0)
			{
				if (listOfUnit.FilterType == "FilterTank")
				{
					_numberOfOurTanks++;
				}
				if (listOfUnit.Type == "Plane" && !listOfUnit.IsDropable)
				{
					_numberOfOurPlanes++;
				}
			}
			if (listOfUnit.IsOnBoat)
			{
				_numberOfOurLandingCrafts++;
			}
			if (listOfUnit.IsArmouredTrain)
			{
				_numberOfOurArmouredTrains++;
			}
		}
		float num = (float)_numberOfOurInfantry + (float)_numberOfOurTanks + (float)_numberOfOurArtillery + (float)_numberOfOurShips + (float)_numberOfOurAA + (float)_numberOfOurAntitanks;
		if (num <= 0f)
		{
			num = _numberOfOurPlanes;
		}
		_ourPlanesToGroundUnitsRatio = (float)_numberOfOurPlanes / num;
	}

	private void PurchaseUnits(Player p_player, bool p_initializeDatabase, bool p_borderGuardingPurposes, string p_filterOverwrite = null)
	{
		_purchaseUnitsLoopCounter++;
		if (_purchaseUnitsLoopCounter > _maxUnitPurchaseAmount * 2)
		{
			return;
		}
		if (_manpowerSettingON)
		{
			if (_currentAvailableManpower <= 0)
			{
				return;
			}
			if (PlayerSettings.Instance.FiniteManpower)
			{
				int fieldedManpower = p_player.GetFieldedManpower();
				int manpowerGrowth = p_player.GetManpowerGrowth();
				float num = (float)_currentAvailableManpower / (float)fieldedManpower;
				if (_currentAvailableManpower <= manpowerGrowth || (num < 0.15f && (p_player.ManpowerMax < 1000000 || p_player.MoneyAmount < p_player.baseIncome || _currentAvailableManpower <= manpowerGrowth * 2)))
				{
					return;
				}
			}
		}
		if (_upkeepSettingON && GetUpkeepToBaseIncomeRatio(p_player) >= GetUpkeepThresholdForAcceptingNewUnits(p_player))
		{
			return;
		}
		if (p_initializeDatabase)
		{
			InitializePurchasableUnitsDatabase(p_player);
		}
		if (_purchasableUnits.Count == 0)
		{
			return;
		}
		bool flag = _hasAA && _enemyHasPlanes && _numberOfOurInfantry > 0 && _numberOfOurActiveAA * 3 < _numberOfOurInfantry;
		bool flag2 = _hasAT && _enemyHasTanks && _numberOfOurInfantry > 0 && _numberOfOurAntitanks * 3 < _numberOfOurInfantry;
		bool flag3 = _hasArtillery && _numberOfOurInfantry > 0 && _numberOfOurArtillery * 4 < _numberOfOurInfantry;
		bool flag4 = _hasTanks && _numberOfOurInfantry > 0 && _numberOfOurTanks * 4 < _numberOfOurInfantry && (_numberOfEnemyAT < _numberOfOurTanks || _numberOfEnemyAT == 0);
		bool flag5 = _hasHarbour && _hasShips && (_numberOfEnemyTorpedoBombers == 0 || _numberOfAlliedFighters >= _numberOfEnemyTorpedoBombers) && (_numberOfOurShips == 0 || _numberOfEnemyShips == 0 || _numberOfOurShips < _numberOfEnemyShips * (_isNavalPower ? 5 : 2));
		bool flag6 = ShouldWeBuy("FilterFighter");
		bool flag7 = ShouldWeBuy("FilterCAS");
		bool flag8 = ShouldWeBuy("FilterBomber");
		string text = p_filterOverwrite;
		_randNumber = UnityEngine.Random.Range(0, 100);
		if (!string.IsNullOrEmpty(text))
		{
			text = p_filterOverwrite;
		}
		else if (_currYear >= 1900 && _currYear <= 1950)
		{
			if (_randNumber <= 5)
			{
				if (flag)
				{
					text = "FilterAA";
				}
				else
				{
					text = ((!flag6) ? "FilterInfantry" : "FilterFighter");
				}
			}
			else if (_randNumber <= 10)
			{
				if (flag2)
				{
					text = "FilterAntitank";
				}
				else
				{
					text = ((!flag4) ? "FilterInfantry" : "FilterTank");
				}
			}
			else if (_randNumber <= 20)
			{
				if (flag3)
				{
					text = "FilterArtillery";
				}
				else if (flag7)
				{
					text = "FilterCAS";
				}
				else if (flag8)
				{
					text = "FilterBomber";
				}
				else
				{
					text = ((UnityEngine.Random.Range(0, 2) != 0) ? "FilterTank" : "FilterInfantry");
				}
			}
			else if (_randNumber <= 25)
			{
				if (flag6)
				{
					if (_areWeWinning)
					{
						if (_isMinor)
						{
							text = ((UnityEngine.Random.Range(0, 5) != 0) ? "FilterInfantry" : "FilterFighter");
						}
						else
						{
							text = "FilterFighter";
						}
					}
					else if (_numberOfAlliedFighters > 0)
					{
						if (_numberOfAlliedFighters * 4 >= _numberOfEnemyFighters)
						{
							if (_isMinor)
							{
								text = ((UnityEngine.Random.Range(0, 5) != 0) ? "FilterInfantry" : "FilterFighter");
							}
							else
							{
								text = "FilterFighter";
							}
						}
						else if (UnityEngine.Random.Range(0, 2) != 0)
						{
							text = ((!flag) ? "FilterInfantry" : "FilterAA");
						}
						else
						{
							text = "FilterFighter";
						}
					}
					else if (_isMinor)
					{
						if (UnityEngine.Random.Range(0, 5) != 0)
						{
							text = ((!flag) ? "FilterInfantry" : "FilterAA");
						}
						else
						{
							text = "FilterFighter";
						}
					}
					else
					{
						text = "FilterFighter";
					}
				}
				else
				{
					text = "FilterInfantry";
				}
			}
			else if (_randNumber <= 30)
			{
				if ((!_areWeWinning & flag) && _numberOfAlliedFighters <= _numberOfEnemyFighters)
				{
					text = "FilterAA";
				}
				else if (!flag7 || (!_areWeWinning && (_numberOfAlliedFighters <= _numberOfEnemyFighters || _numberOfAlliedFighters <= 0 || _numberOfEnemyFighters <= 0)))
				{
					text = ((!flag3 || UnityEngine.Random.Range(0, 3) != 0) ? "FilterInfantry" : "FilterArtillery");
				}
				else if (!_isMinor)
				{
					text = "FilterCAS";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 4);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterCAS";
					}
				}
			}
			else if (_randNumber <= 35)
			{
				if ((!_areWeWinning & flag) && _numberOfAlliedFighters <= _numberOfEnemyFighters)
				{
					text = "FilterAA";
				}
				else if (!flag8 || (!_areWeWinning && (_numberOfAlliedFighters <= _numberOfEnemyFighters || _numberOfAlliedFighters <= 0 || _numberOfEnemyFighters <= 0)))
				{
					text = ((!flag3 || UnityEngine.Random.Range(0, 3) != 0) ? "FilterInfantry" : "FilterArtillery");
				}
				else if (!_isMinor)
				{
					text = "FilterBomber";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 4);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterBomber";
					}
				}
			}
			else if (_randNumber <= 40)
			{
				text = ((!_ownsEngineers) ? "FilterInfantry" : "FilterBuilding");
			}
			else if (_randNumber <= 50)
			{
				if (!flag4)
				{
					text = ((!flag3 || UnityEngine.Random.Range(0, 3) != 0) ? "FilterInfantry" : "FilterArtillery");
				}
				else if (!_isMinor)
				{
					text = "FilterTank";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 15);
					text = ((_randNumber != 0) ? "FilterInfantry" : "FilterTank");
				}
			}
			else if (_randNumber <= 70 - ((!_isNavalPower) ? 15 : 0))
			{
				if (!flag5)
				{
					if (flag3)
					{
						text = "FilterArtillery";
					}
					else if (flag7)
					{
						text = "FilterCAS";
					}
					else if (flag8)
					{
						text = "FilterBomber";
					}
					else
					{
						text = ((!flag4) ? "FilterInfantry" : "FilterTank");
					}
				}
				else if (!_isMinor)
				{
					text = "FilterBoat";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 3);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterBoat";
					}
				}
			}
			else
			{
				text = "FilterInfantry";
			}
		}
		else if (_currYear > 1950)
		{
			if (_randNumber <= 35)
			{
				text = "FilterInfantry";
			}
			else if (_randNumber <= 50)
			{
				if (!flag4)
				{
					text = ((UnityEngine.Random.Range(0, 2) != 0) ? "FilterArtillery" : "FilterInfantry");
				}
				else if (!_isMinor)
				{
					text = "FilterTank";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 15);
					text = ((_randNumber != 0) ? "FilterInfantry" : "FilterTank");
				}
			}
			else if (_randNumber <= 60 - ((!_isNavalPower) ? 5 : 0))
			{
				if (!flag5)
				{
					if (flag3)
					{
						text = "FilterArtillery";
					}
					else if (flag7)
					{
						text = "FilterCAS";
					}
					else if (flag8)
					{
						text = "FilterBomber";
					}
					else
					{
						text = ((!flag4) ? "FilterInfantry" : "FilterTank");
					}
				}
				else if (!_isMinor)
				{
					text = "FilterBoat";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 3);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterBoat";
					}
				}
			}
			else if (_randNumber <= 65)
			{
				if (flag3)
				{
					text = "FilterArtillery";
				}
				else if (flag7)
				{
					text = "FilterCAS";
				}
				else if (flag8)
				{
					text = "FilterBomber";
				}
				else
				{
					text = ((UnityEngine.Random.Range(0, 2) != 0) ? "FilterTank" : "FilterInfantry");
				}
			}
			else if (_randNumber <= 70)
			{
				if (flag)
				{
					text = "FilterAA";
				}
				else
				{
					text = ((!flag6) ? "FilterInfantry" : "FilterFighter");
				}
			}
			else if (_randNumber <= 75)
			{
				if (flag2)
				{
					text = "FilterAntitank";
				}
				else
				{
					text = ((!flag4) ? "FilterInfantry" : "FilterTank");
				}
			}
			else if (_randNumber <= 80)
			{
				if ((!_areWeWinning & flag) && _numberOfAlliedFighters <= _numberOfEnemyFighters)
				{
					text = "FilterAA";
				}
				else if (!flag7 || (!_areWeWinning && (_numberOfAlliedFighters <= _numberOfEnemyFighters || _numberOfAlliedFighters <= 0 || _numberOfEnemyFighters <= 0)))
				{
					text = ((UnityEngine.Random.Range(0, 2) != 0) ? "FilterArtillery" : "FilterInfantry");
				}
				else if (!_isMinor)
				{
					text = "FilterCAS";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 4);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterCAS";
					}
				}
			}
			else if (_randNumber <= 85)
			{
				if ((!_areWeWinning & flag) && _numberOfAlliedFighters <= _numberOfEnemyFighters)
				{
					text = "FilterAA";
				}
				else if (!flag8 || (!_areWeWinning && (_numberOfAlliedFighters <= _numberOfEnemyFighters || _numberOfAlliedFighters <= 0 || _numberOfEnemyFighters <= 0)))
				{
					text = ((UnityEngine.Random.Range(0, 2) != 0) ? "FilterArtillery" : "FilterInfantry");
				}
				else if (!_isMinor)
				{
					text = "FilterBomber";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 4);
					if (_randNumber != 0)
					{
						text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
					}
					else
					{
						text = "FilterBomber";
					}
				}
			}
			else if (_randNumber <= 90)
			{
				if (!flag6)
				{
					text = "FilterInfantry";
				}
				else if (_areWeWinning)
				{
					if (!_isMinor)
					{
						text = "FilterFighter";
					}
					else
					{
						_randNumber = UnityEngine.Random.Range(0, 4);
						text = ((_randNumber != 0) ? "FilterInfantry" : "FilterFighter");
					}
				}
				else if (_numberOfAlliedFighters > 0)
				{
					if (_numberOfAlliedFighters * 2 < _numberOfEnemyFighters)
					{
						text = ((!flag) ? "FilterInfantry" : "FilterAA");
					}
					else if (!_isMinor)
					{
						text = "FilterFighter";
					}
					else
					{
						_randNumber = UnityEngine.Random.Range(0, 4);
						text = ((_randNumber != 0) ? "FilterInfantry" : "FilterFighter");
					}
				}
				else if (_numberOfAlliedFighters > 0 && _numberOfEnemyFighters >= _numberOfAlliedFighters * 3)
				{
					text = ((!flag) ? "FilterInfantry" : "FilterAA");
				}
				else if (!_isMinor)
				{
					text = "FilterFighter";
				}
				else
				{
					_randNumber = UnityEngine.Random.Range(0, 4);
					text = ((_randNumber != 0) ? "FilterInfantry" : "FilterFighter");
				}
			}
			else if (_randNumber <= 95)
			{
				text = ((!_ownsEngineers) ? "FilterInfantry" : "FilterBuilding");
			}
		}
		else
		{
			flag3 = _hasArtillery && _numberOfOurInfantry > 0 && _numberOfOurArtillery * 5 < _numberOfOurInfantry;
			flag4 = _hasTanks && _numberOfOurInfantry > 0 && _numberOfOurTanks * 4 < _numberOfOurInfantry;
			if (_randNumber <= 60)
			{
				text = "FilterInfantry";
			}
			else if (_randNumber <= 75)
			{
				text = ((!flag4) ? "FilterInfantry" : "FilterTank");
			}
			else if (_randNumber <= 85)
			{
				if (flag5)
				{
					text = "FilterBoat";
				}
				else if (flag3)
				{
					text = "FilterArtillery";
				}
				else
				{
					text = ((!flag4) ? "FilterInfantry" : "FilterTank");
				}
			}
			else if (_randNumber <= 90)
			{
				text = ((!_ownsEngineers) ? "FilterInfantry" : "FilterBuilding");
			}
			else
			{
				text = ((!flag3) ? "FilterInfantry" : "FilterArtillery");
			}
		}
		Unit unit = null;
		if (supplyDepotNeeded && _hasSupplyDepots && _numberOfOurEngineers == 0)
		{
			_engineersNeeded = true;
		}
		if (_engineersNeeded && _hasEngineers && (_numberOfOurEngineers == 0 || _numberOfOurEngineers + 1 <= _numberOfOurInfantry / 10))
		{
			text = "Engineers";
			foreach (Unit purchasableUnit in _purchasableUnits)
			{
				if (purchasableUnit.IsEngineers)
				{
					unit = purchasableUnit;
					break;
				}
			}
		}
		else if (supplyDepotNeeded && _hasSupplyDepots && _ownsEngineers && _numberOfOurEngineers > 0 && !_boughtSupplyDepotThisTurn)
		{
			text = "SupplyDepot";
			foreach (Unit purchasableUnit2 in _purchasableUnits)
			{
				if (purchasableUnit2.IsSupplyDepot)
				{
					unit = purchasableUnit2;
					break;
				}
			}
		}
		else
		{
			short num2 = 0;
			while (unit == null && num2 < 50)
			{
				foreach (Unit purchasableUnit3 in _purchasableUnits)
				{
					if (p_player.MoneyAmount - purchasableUnit3.Cost < 0 || (purchasableUnit3.IsCommander && _boughtCommanderThisTurn) || (purchasableUnit3.IsArmouredTrain && _boughtArmouredTrainThisTurn) || (purchasableUnit3.IsSupplyDepot && (_boughtSupplyDepotThisTurn || !supplyDepotNeeded)) || purchasableUnit3.FilterType != text)
					{
						continue;
					}
					_randNumber = -1;
					if (purchasableUnit3.AIPurchaseChanceOverride > 0)
					{
						_randNumber = UnityEngine.Random.Range(0, purchasableUnit3.AIPurchaseChanceOverride);
					}
					else
					{
						if (purchasableUnit3.IsCommander)
						{
							_randNumber = UnityEngine.Random.Range(0, 500);
						}
						else if (purchasableUnit3.IsArmouredTrain)
						{
							_randNumber = UnityEngine.Random.Range(0, 100);
						}
						else if (purchasableUnit3.Country != _currPlayerCountry)
						{
							if (purchasableUnit3.FilterType == "FilterBoat")
							{
								if (purchasableUnit3.IsCarrier || purchasableUnit3.Name.Contains("attle"))
								{
									_randNumber = 100;
								}
								else
								{
									_randNumber = UnityEngine.Random.Range(0, 30);
								}
							}
							else if (_currYear >= 1941)
							{
								if (_currPlayerCountry == "Poland" || _currPlayerCountry == "France" || _currPlayerCountry == "Norway" || _currPlayerCountry == "Belgium" || _currPlayerCountry == "Luxembourg")
								{
									_randNumber = UnityEngine.Random.Range(0, 3);
								}
								else
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
							}
							else
							{
								_randNumber = UnityEngine.Random.Range(0, 15);
							}
						}
						else if (purchasableUnit3.IsPrototype)
						{
							_randNumber = UnityEngine.Random.Range(0, 20);
						}
						else if (purchasableUnit3.FilterType == "FilterBoat")
						{
							if (_numberOfOurDestroyers <= _numberOfEnemySubmarines * 2)
							{
								if (purchasableUnit3.GetNameToLower().Contains("aux"))
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
								else if (purchasableUnit3.IsDestroyer)
								{
									_randNumber = 0;
								}
								else
								{
									_randNumber = 100;
								}
							}
							else
							{
								if (_isHistoricalFocusON && !purchasableUnit3.IsSubmarine && UnityEngine.Random.Range(0, 2) != 0 && _currPlayerCountry == "Nazi Germany")
								{
									continue;
								}
								if (purchasableUnit3.IsConvoy)
								{
									_randNumber = UnityEngine.Random.Range(0, 50);
								}
								else if (purchasableUnit3.IsCarrier)
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("aux"))
								{
									_randNumber = UnityEngine.Random.Range(0, 30);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("nought"))
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("attle"))
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("heavy"))
								{
									_randNumber = UnityEngine.Random.Range(0, 8);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("light"))
								{
									_randNumber = UnityEngine.Random.Range(0, 5);
								}
								else if (purchasableUnit3.GetNameToLower().Contains("coastal"))
								{
									_randNumber = UnityEngine.Random.Range(0, 150);
								}
							}
						}
						else if (purchasableUnit3.FilterType == "FilterArtillery")
						{
							if (purchasableUnit3.MaxMP > 1)
							{
								_randNumber = UnityEngine.Random.Range(0, 20);
							}
							else if (purchasableUnit3.SoftDamage >= 30 || purchasableUnit3.Range >= 4)
							{
								_randNumber = UnityEngine.Random.Range(0, 20);
							}
						}
						else if (purchasableUnit3.FilterType == "FilterAntitank")
						{
							if (purchasableUnit3.MaxMP > 1 && purchasableUnit3.Type == "Tank")
							{
								_randNumber = UnityEngine.Random.Range(0, 10);
							}
						}
						else if (purchasableUnit3.FilterType == "FilterAA")
						{
							if (purchasableUnit3.MaxMP > 1 && purchasableUnit3.Type == "Tank")
							{
								_randNumber = UnityEngine.Random.Range(0, 10);
							}
						}
						else if (purchasableUnit3.CoastalBattery || purchasableUnit3.GetNameToLower().Contains("bunker"))
						{
							_randNumber = UnityEngine.Random.Range(0, 20);
						}
						else if (purchasableUnit3.IsPontoonBridge && purchasableUnit3.MaxMP == 0)
						{
							_randNumber = 0;
						}
						else if (purchasableUnit3.IsLaunchSite && purchasableUnit3.MaxMP == 0)
						{
							_randNumber = 0;
						}
						else if (purchasableUnit3.IsArmourObstacle && purchasableUnit3.MaxMP == 0)
						{
							_randNumber = UnityEngine.Random.Range(0, 30);
						}
						else if (purchasableUnit3.IsParatrooper)
						{
							if (_isEuropeScenario || _isWorldScenario)
							{
								if (_currPlayerCountry == "Nazi Germany")
								{
									if (_currYear < 1940)
									{
										continue;
									}
								}
								else if (_currPlayerCountry == "Soviet Union")
								{
									if (_currYear < 1941)
									{
										continue;
									}
								}
								else if (_currDate < new DateTime(1943, 6, 1))
								{
									continue;
								}
							}
							if (_isHistoricalFocusON)
							{
								if (_difficultyLevel == 4)
								{
									_randNumber = UnityEngine.Random.Range(0, 10);
								}
								else if (_difficultyLevel == 3)
								{
									_randNumber = UnityEngine.Random.Range(0, 12);
								}
								else if (_difficultyLevel == 2)
								{
									_randNumber = UnityEngine.Random.Range(0, 16);
								}
								else if (_difficultyLevel <= 1)
								{
									_randNumber = UnityEngine.Random.Range(0, 20);
								}
							}
							else if (_difficultyLevel == 4)
							{
								_randNumber = UnityEngine.Random.Range(0, 5);
							}
							else if (_difficultyLevel == 3)
							{
								_randNumber = UnityEngine.Random.Range(0, 6);
							}
							else if (_difficultyLevel == 2)
							{
								_randNumber = UnityEngine.Random.Range(0, 8);
							}
							else if (_difficultyLevel <= 1)
							{
								_randNumber = UnityEngine.Random.Range(0, 10);
							}
						}
						else if (_hasBarrageBalloons && purchasableUnit3.MaxMP == 0 && purchasableUnit3.Name.Contains("allon"))
						{
							if (_numberOfEnemyHeavyBombers == 0)
							{
								continue;
							}
							_randNumber = 0;
						}
						else if (purchasableUnit3.IsEngineers)
						{
							if (_numberOfOurEngineers > 0 && _numberOfOurEngineers + 1 > _numberOfOurInfantry / 10)
							{
								continue;
							}
							_randNumber = UnityEngine.Random.Range(0, 5);
						}
						else if (purchasableUnit3.Type == "Plane")
						{
							if (purchasableUnit3.IsRecon)
							{
								if (_numberOfEnemyPlanes >= _numberOfOurPlanes)
								{
									continue;
								}
								_randNumber = UnityEngine.Random.Range(0, 50);
							}
						}
						else if (purchasableUnit3.Name == "Conscript" && _difficultyLevel >= 2)
						{
							_randNumber = UnityEngine.Random.Range(0, 20);
						}
						else if (purchasableUnit3.IsCavalry)
						{
							if (_currYear > 1939)
							{
								_randNumber = UnityEngine.Random.Range(0, 50);
							}
							else if (_currYear > 1930)
							{
								_randNumber = UnityEngine.Random.Range(0, 25);
							}
							else
							{
								_randNumber = UnityEngine.Random.Range(0, 15);
							}
						}
						else if (purchasableUnit3.GetNameToLower().Contains("NKVD"))
						{
							_randNumber = UnityEngine.Random.Range(0, 16);
						}
						else if (purchasableUnit3.Name == "Rangers" || purchasableUnit3.GetNameToLower().Contains("commando"))
						{
							_randNumber = UnityEngine.Random.Range(0, 15);
						}
						else if (purchasableUnit3.GetNameToLower().Contains("marines"))
						{
							if ((_isEuropeScenario || _isWorldScenario) && _currPlayerCountry == "Great Britain" && _currDate < new DateTime(1944, 6, 1))
							{
								continue;
							}
							_randNumber = UnityEngine.Random.Range(0, 15);
						}
						else if (purchasableUnit3.HasFlamethrower)
						{
							_randNumber = UnityEngine.Random.Range(0, 15);
						}
						else if (purchasableUnit3.Name.Contains("AT infantry"))
						{
							if (_numberOfOurAntitanks >= _numberOfEnemyTanks * 2 || _numberOfOurATinfantry >= _numberOfEnemyTanks * 2)
							{
								continue;
							}
							if (_hasATinfantry43 && !purchasableUnit3.Name.Contains("43"))
							{
								_randNumber = 100;
							}
							else
							{
								_randNumber = UnityEngine.Random.Range(0, 5);
							}
						}
						if (_randNumber == -1 && purchasableUnit3.MaxMP > 0)
						{
							switch (_difficultyLevel)
							{
							case 1:
								_randNumber = UnityEngine.Random.Range(0, 5);
								break;
							default:
								_randNumber = UnityEngine.Random.Range(0, 4);
								break;
							case 3:
								_randNumber = UnityEngine.Random.Range(0, 3);
								break;
							case 4:
								_randNumber = UnityEngine.Random.Range(0, 2);
								break;
							}
						}
					}
					if (_randNumber == 0)
					{
						if (!purchasableUnit3.IsCarrier || _numberOfOurCarriers == 0)
						{
							unit = purchasableUnit3;
							break;
						}
						if (_numberOfOurShips / (_numberOfOurCarriers + 1) >= 5)
						{
							unit = purchasableUnit3;
							break;
						}
					}
				}
				num2++;
			}
		}
		if (unit == null)
		{
			if (_randNumber == 0)
			{
				PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes);
			}
			else if (text == "FilterAA")
			{
				PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes, text);
			}
			else
			{
				PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes);
			}
			return;
		}
		if (unit.MaxMP == 1 && unit.CanBeHorseTowed() && p_player.MoneyAmount - (unit.Cost + Database.horseCost) < 0)
		{
			PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes);
			return;
		}
		if (unit.MaxPurchaseTime > 0 && p_player.HasReachedMaxPurchaseLimit(unit))
		{
			PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes);
			return;
		}
		Unit unit2 = unit.Clone(p_generateNewID: true);
		unit2.OwnerPlayer = p_player;
		unit2.OwnerName = p_player.Name;
		unit2.XP = 0;
		unit2.Lvl = 1;
		unit2.IsReserve = true;
		unit2.CustomName = null;
		if (unit2.FilterType != "FilterBuilding")
		{
			if (!unit2.IsCommander && unit2.MaxMP > 0)
			{
				if (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter && unit2.Type != "Plane" && unit2.Type != "Boat" && !unit2.IsCommander)
				{
					if (_currPlayerCountry == "Soviet Union" || _currPlayerCountry == "Finland")
					{
						unit2.IsWinterSpecialized = true;
					}
					else if (unit2.Cost > 150 && p_player.MoneyAmount - (unit2.Cost + Database.winterCost) >= 0 && UnityEngine.Random.Range(0, 6) == 0)
					{
						unit2.ApplyModification(Database.winterTrainedOption);
					}
				}
				if (_hillAndMountainsPercentage >= 1.5f && unit2.FilterType == "FilterInfantry" && !unit2.IsMine && !unit2.IsSupplyDepot && unit2.MaxMP > 0 && !unit2.IsCommander && !unit2.IsCavalry && p_player.MoneyAmount - (unit2.Cost + Database.mountaineerCost) >= 0)
				{
					if (_hillAndMountainsPercentage >= 50f)
					{
						_randNumber = UnityEngine.Random.Range(0, 1);
					}
					else if (_hillAndMountainsPercentage >= 30f)
					{
						_randNumber = UnityEngine.Random.Range(0, 3);
					}
					else if (_hillAndMountainsPercentage >= 10f)
					{
						_randNumber = UnityEngine.Random.Range(0, 4);
					}
					else if (_hillAndMountainsPercentage >= 5f)
					{
						_randNumber = UnityEngine.Random.Range(0, 5);
					}
					else
					{
						_randNumber = UnityEngine.Random.Range(0, 6);
					}
					if (_randNumber == 0)
					{
						unit2.ApplyModification(Database.mountaineersOption);
						if (p_player.HasPolicy(Policies.Type.Highlands_experts))
						{
							unit2.Cost += 25;
						}
					}
				}
				_randNumber = 0;
				if (unit2.Armour == 0 && (unit2.Type == "Infantry" || (unit2.MaxMP == 1 && (unit2.Type == "Artillery" || unit2.Type == "AA"))))
				{
					if (unit2.Type == "Infantry")
					{
						if (!p_borderGuardingPurposes && (!_upkeepSettingON || (_upkeepSettingON && GetUpkeepToBaseIncomeRatio(p_player) <= 0.5f)) && !unit2.GetNameToLower().Contains("militia") && !unit2.GetNameToLower().Contains("conscript") && !unit2.GetNameToLower().Contains("volkssturm"))
						{
							if (p_player.MoneyAmount - (unit2.Cost + Database.mechanizeCost) >= 0)
							{
								int num3;
								if (_difficultyLevel == 4)
								{
									num3 = 4;
								}
								else if (_difficultyLevel == 3)
								{
									num3 = 5;
								}
								else
								{
									num3 = ((_difficultyLevel != 2) ? 8 : 7);
								}
								if (_currYear >= 1945)
								{
									num3--;
								}
								if (_currYear >= 1944)
								{
									num3--;
								}
								if (!_areWeWinning)
								{
									num3 += 2;
								}
								if (num3 < 0)
								{
									num3 = 2;
								}
								if (UnityEngine.Random.Range(0, num3) == 0)
								{
									_randNumber = 2;
								}
							}
							if (_randNumber == 0 && p_player.MoneyAmount - (unit2.Cost + Database.motorizeCost) >= 0)
							{
								int num4;
								if (_difficultyLevel == 4)
								{
									num4 = 4;
								}
								else if (_difficultyLevel == 3)
								{
									num4 = 5;
								}
								else
								{
									num4 = ((_difficultyLevel != 2) ? 8 : 7);
								}
								if (_currYear >= 1945)
								{
									num4--;
								}
								if (_currYear >= 1944)
								{
									num4--;
								}
								if (!_areWeWinning)
								{
									num4 += 2;
								}
								if (UnityEngine.Random.Range(0, num4) == 0)
								{
									_randNumber = 1;
								}
							}
						}
					}
					else if (_currYear < 1914)
					{
						_randNumber = 0;
					}
					else if (unit2.HardDamage >= 100 || (unit2.SoftDamage >= 30 && unit2.Range > 1))
					{
						if (p_player.MoneyAmount - (unit2.Cost + Database.mechanizeCost) >= 0 && UnityEngine.Random.Range(0, 10) == 0)
						{
							_randNumber = 2;
						}
						else
						{
							_randNumber = 1;
						}
					}
					else if (p_player.MoneyAmount - (unit2.Cost + Database.motorizeCost) >= 0 && UnityEngine.Random.Range(0, 5) == 0)
					{
						_randNumber = 1;
					}
					else
					{
						_randNumber = 0;
					}
					switch (_randNumber)
					{
					case 0:
						if (p_player.MoneyAmount - (unit2.Cost + Database.horseCost) >= 0 && unit2.CanBeHorseTowed())
						{
							unit2.ApplyModification(Database.horseTowedOption);
						}
						break;
					case 1:
						if (p_player.MoneyAmount - (unit2.Cost + Database.motorizeCost) >= 0 && unit2.CanBeMotorized())
						{
							unit2.ApplyModification(Database.motorizedOption);
						}
						break;
					case 2:
						if (p_player.MoneyAmount - (unit2.Cost + Database.mechanizeCost) >= 0 && unit2.CanBeMechanized())
						{
							unit2.ApplyModification(Database.mechanizedOption);
						}
						break;
					}
					if (!unit2.IsHorsed && !unit2.IsMotorized && !unit2.IsMechanized && unit2.Type != "Infantry")
					{
						PurchaseUnits(p_player, p_initializeDatabase: false, p_borderGuardingPurposes);
						return;
					}
				}
				if (unit2.Type == "Plane")
				{
					if (unit2.CanCarryABomb && p_player.MoneyAmount - (unit2.Cost + Database.AtomBombCost) >= 0 && _currDate > new DateTime(1945, 8, 1))
					{
						unit2.ApplyModification(Database.atomBombOption);
					}
					if (unit2.CanCarryBombs && (unit2.FilterType != "FilterFighter" || _numberOfAlliedFighters > _numberOfEnemyFighters) && p_player.MoneyAmount - (unit2.Cost + Database.bombCost) >= 0)
					{
						if (_difficultyLevel == 4)
						{
							_randNumber = UnityEngine.Random.Range(0, 2);
						}
						else if (_difficultyLevel == 3)
						{
							_randNumber = UnityEngine.Random.Range(0, 3);
						}
						else
						{
							_randNumber = UnityEngine.Random.Range(0, 4);
						}
						if (_randNumber == 0)
						{
							unit2.ApplyModification(Database.bombsOption);
						}
					}
					if (unit2.CanCarryTorpedo && _numberOfEnemyShips >= 3 && p_player.MoneyAmount - (unit2.Cost + Database.torpedoCost) >= 0)
					{
						unit2.ApplyModification(Database.torpedoOption);
					}
					if (unit2.CanCarryDropTank && _totalMapSize >= 3000 && p_player.MoneyAmount - (unit2.Cost + Database.dropTankCost) >= 0 && UnityEngine.Random.Range(0, 6) == 0)
					{
						unit2.ApplyModification(Database.dropTankOption);
					}
					if (unit2.CanCarryRockets && (unit2.FilterType != "FilterFighter" || _numberOfAlliedFighters > _numberOfEnemyFighters) && p_player.MoneyAmount - (unit2.Cost + Database.rocketsCost) >= 0)
					{
						if (_difficultyLevel == 4)
						{
							_randNumber = UnityEngine.Random.Range(0, 2);
						}
						else if (_difficultyLevel == 3)
						{
							_randNumber = UnityEngine.Random.Range(0, 3);
						}
						else
						{
							_randNumber = UnityEngine.Random.Range(0, 4);
						}
						if (_randNumber == 0)
						{
							unit2.ApplyModification(Database.rocketsOption);
						}
					}
				}
				if (_currYear >= 1935 && _currYear <= 1945 && p_player.MoneyAmount - (unit2.Cost + Database.politicalUnitCost) >= 0 && UnityEngine.Random.Range(0, 40) == 0 && (_currPlayerCountry == "Nazi Germany" || _currPlayerCountry == "Soviet Union") && unit2.BaseMaxMP > 1 && unit2.Type != "Plane" && unit2.Type != "Boat" && !unit2.IsCommander && !unit2.Name.Contains("AT") && !unit2.Name.Contains("ilitia") && !unit2.Name.Contains("cript") && !unit2.Name.Contains("olk"))
				{
					unit2.ApplyModification(Database.politicalOption);
				}
			}
			if (unit2.Lvl <= 1 && p_player.HasPolicy(Policies.Type.Army_training))
			{
				byte lvl = unit2.Lvl;
				while (unit2.Lvl < lvl + 1)
				{
					unit2.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
			}
		}
		if (p_player.MoneyAmount - unit2.Cost < 0)
		{
			return;
		}
		unit2.GenerateNewID();
		if (unit2.IsCommander)
		{
			if (unit2.Type != "Boat")
			{
				unit2.IsWinterSpecialized = true;
			}
			_boughtCommanderThisTurn = true;
		}
		if (unit2.IsParatrooper)
		{
			unit2.ApplyModification(Database.planeTransportOption);
		}
		unit2.CurrHP = unit2.MaxHP;
		unit2.CurrMP = unit2.MaxMP;
		unit2.CurrAutonomy = unit2.MaxAutonomy;
		if (unit2.IsEliteUnit)
		{
			unit2.Morale = Unit.MORALE_ELITE;
		}
		else
		{
			unit2.Morale = Unit.MORALE_DEFAULT;
		}
		if (unit2.IsEngineers)
		{
			_ownsEngineers = true;
			_engineersNeeded = false;
		}
		if (unit2.IsArmouredTrain)
		{
			_boughtArmouredTrainThisTurn = true;
		}
		if (unit2.IsSupplyDepot)
		{
			_boughtSupplyDepotThisTurn = true;
		}
		_currentUpkeepCost += unit2.GetUpkeepCost(p_getReserveCost: false);
		_currentAvailableManpower -= unit2.manpowerCost;
		p_player.PurchaseUnit(unit2, p_sendRPC: true);
		_purchasedUnits++;
		CountOwnUnits(p_player);
	}

	private float GetUpkeepThresholdForAcceptingNewUnits(Player p_player)
	{
		if (p_player.MoneyAmount >= p_player.baseIncome * 10)
		{
			return 0.95f;
		}
		if (p_player.MoneyAmount >= p_player.baseIncome * 5)
		{
			return 0.9f;
		}
		if (p_player.MoneyAmount >= p_player.baseIncome * 4)
		{
			return 0.85f;
		}
		if (p_player.MoneyAmount >= p_player.baseIncome * 3)
		{
			return 0.8f;
		}
		if (p_player.MoneyAmount >= p_player.baseIncome * 2)
		{
			return 0.75f;
		}
		if ((float)p_player.MoneyAmount >= (float)p_player.baseIncome * 1.5f)
		{
			return 0.7f;
		}
		if (p_player.MoneyAmount > p_player.baseIncome)
		{
			return 0.65f;
		}
		return 0.6f;
	}

	private void InitializePurchasableUnitsDatabase(Player p_player)
	{
		_hasEngineers = false;
		_hasInfantry = false;
		_hasTanks = false;
		_hasArtillery = false;
		_hasAT = false;
		_hasAA = false;
		_hasShips = false;
		_hasFighters = false;
		_hasCAS = false;
		_hasStrategicBombers = false;
		_hasBarrageBalloons = false;
		_hasSupplyDepots = false;
		_hasATinfantry43 = false;
		_purchasableUnits = new List<Unit>();
		p_player.CalculateTurnIncome();
		foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
		{
			Unit unit = listOfAllUnit.Clone(p_generateNewID: true);
			unit.OwnerName = p_player.Name;
			unit.OwnerPlayer = p_player;
			if (!IsUnitEligibleForPurchase(unit, p_player))
			{
				continue;
			}
			if (unit.IsEngineers)
			{
				_hasEngineers = true;
			}
			if (unit.IsSupplyDepot)
			{
				_hasSupplyDepots = true;
			}
			if (unit.MaxMP == 0 && unit.GetNameToLower().Contains("alloon"))
			{
				_hasBarrageBalloons = true;
			}
			if (unit.Type == "Infantry")
			{
				_hasInfantry = true;
				if (unit.Name == "AT infantry 43")
				{
					_hasATinfantry43 = true;
				}
			}
			if (!_hasTanks && unit.FilterType == "FilterTank")
			{
				_hasTanks = true;
			}
			else if (!_hasArtillery && unit.FilterType == "FilterArtillery")
			{
				_hasArtillery = true;
			}
			else if (!_hasAT && unit.FilterType == "FilterAntitank")
			{
				_hasAT = true;
			}
			else if (!_hasAA && unit.FilterType == "FilterAA")
			{
				_hasAA = true;
			}
			else if (!_hasShips && unit.FilterType == "FilterBoat")
			{
				_hasShips = true;
			}
			else if (!_hasFighters && unit.FilterType == "FilterFighter")
			{
				_hasFighters = true;
			}
			else if (!_hasCAS && unit.FilterType == "FilterCAS")
			{
				_hasCAS = true;
			}
			else if (!_hasStrategicBombers && unit.FilterType == "FilterBomber")
			{
				_hasStrategicBombers = true;
			}
			if (unit.Country != _currPlayerCountry && !p_player.HasPolicy(Policies.Type.ImportFocus))
			{
				unit.Cost += (int)Math.Round((float)unit.Cost * 0.35f, MidpointRounding.ToEven);
			}
			_purchasableUnits.Add(unit);
		}
		_purchasableUnits = _purchasableUnits.OrderByDescending((Unit x) => x.Cost).ToList();
	}

	private bool CanPurchaseUnitsFromPlayer(Player p_player)
	{
		if (!p_player.isAlliedWithCurrentPlayer)
		{
			return false;
		}
		if (_isHistoricalFocusON && p_player.Ideology != p_player.Ideology)
		{
			return false;
		}
		if (_isMajor)
		{
			return false;
		}
		if (!p_player.IsMajor())
		{
			return false;
		}
		return true;
	}

	private bool IsUnitEligibleForPurchase(Unit p_unit, Player p_player)
	{
		if (!p_unit.IsAvailable(p_player))
		{
			return false;
		}
		if (p_unit.TileBuilding)
		{
			return false;
		}
		if (_currYear > p_unit.EndUseYear)
		{
			return false;
		}
		if (p_unit.IsMine || p_unit.IsSupplyShip || p_unit.IsPartisans)
		{
			return false;
		}
		if (p_unit.IsConvoy)
		{
			if (!_hasHarbour)
			{
				return false;
			}
			if (p_player.AlliedPlayers.Count <= 1)
			{
				return false;
			}
			if (_numberOfOurShips <= 10)
			{
				return false;
			}
			if (PlayerSettings.Instance.ISHistoricalFocus && p_player.Faction == "Axis")
			{
				return false;
			}
		}
		if (p_unit.IsMissile && _numberOfOurDeployedLaunchSites == 0)
		{
			return false;
		}
		if (!_ownsEngineers && p_unit.BuiltByEngineers)
		{
			return false;
		}
		if (p_unit.IsPrototype)
		{
			if (_isHistoricalFocusON)
			{
				return false;
			}
			if (PlayerSettings.Instance.PrototypesDisabled)
			{
				return false;
			}
		}
		if (p_unit.IsCavalry && _currYear >= 1941)
		{
			return false;
		}
		if (p_unit.IsParatrooper)
		{
			if (_numberOfOurFighters == 0)
			{
				return false;
			}
			if (_numberOfEnemyFighters > _numberOfAlliedFighters)
			{
				return false;
			}
			if (_currYear < 1940 && _isHistoricalFocusON)
			{
				return false;
			}
			if (_numberOfEnemyLandVPs <= 0)
			{
				return false;
			}
		}
		if (p_player.Faction == "Neutral")
		{
			if (p_unit.IsCommander)
			{
				return false;
			}
			if (p_unit.Type != "Infantry")
			{
				return false;
			}
		}
		if (p_unit.MaxPurchaseTime > 0 && p_player.HasReachedMaxPurchaseLimit(p_unit))
		{
			return false;
		}
		if (!_hasAirport && p_unit.Type == "Plane")
		{
			return false;
		}
		if (!_hasHarbour && (p_unit.FilterType == "FilterBoat" || p_unit.CoastalBattery))
		{
			return false;
		}
		if (p_unit.CanBeRefueledOnCarrier && _numberOfOurCarriers == 0 && (p_unit.Country == "Nazi Germany" || p_unit.Country == "France"))
		{
			return false;
		}
		if (p_unit.NoLendLease && p_unit.Country != _currPlayerCountry)
		{
			return false;
		}
		if (_numberOfEnemyLandVPs <= 0 && p_player.PlayersAtWar.Count > 0)
		{
			if (!p_unit.CanCarryTorpedo && !p_unit.IsRecon && p_unit.FilterType == "FilterBomber")
			{
				return false;
			}
			if (p_unit.Type != "Boat" && p_unit.Type != "Plane" && p_unit.FilterType != "FilterBuilding")
			{
				return false;
			}
		}
		if (!_hasHeavyProductionMeans && p_unit.IsConsideredHeavyEquipment())
		{
			return false;
		}
		if (p_unit.FilterType == "FilterInfantry")
		{
			if (p_unit.GetNameToLower().Contains("garrison"))
			{
				return false;
			}
			if (_seaTilesPercentage < 10f && p_unit.GetNameToLower().Contains("marines"))
			{
				return false;
			}
			if (_coreTerritoryPercent > 0.6f)
			{
				if (p_unit.GetNameToLower().Contains("militia") || p_unit.GetNameToLower().Contains("volkssturm"))
				{
					return false;
				}
				if (p_unit.GetNameToLower().Contains("conscript") && _currYear >= 1943)
				{
					return false;
				}
			}
			if (p_unit.Name.Contains("AT infantry"))
			{
				if (_numberOfEnemyTanks == 0)
				{
					return false;
				}
				if (_numberOfOurAntitanks >= _numberOfEnemyTanks * 2)
				{
					return false;
				}
				if (_numberOfOurATinfantry >= _numberOfEnemyTanks)
				{
					return false;
				}
				if (_hasATinfantry43 && !p_unit.Name.Contains("43"))
				{
					return false;
				}
			}
		}
		else if (p_unit.IsMineFlail && _numberOfEnemyLandMines <= 0)
		{
			return false;
		}
		if (p_unit.MaxMP == 0)
		{
			if (p_unit.IsArmourObstacle)
			{
				if (_areWeWinning)
				{
					return false;
				}
				if (_numberOfEnemyTanks == 0)
				{
					return false;
				}
			}
			if (p_unit.IsPontoonBridge && _numberOfRivers == 0)
			{
				return false;
			}
			if (p_unit.IsRADAR && _numberOfOurRadars > 0 && p_player.ListOfUnits.Count / _numberOfOurRadars + 1 < 25)
			{
				return false;
			}
			if (p_unit.IsLaunchSite && _numberOfOurLaunchSites > 0 && p_player.ListOfUnits.Count / _numberOfOurLaunchSites + 1 < 25)
			{
				return false;
			}
			if (_areWeWinning && p_unit.GetNameToLower().Contains("bunker"))
			{
				return false;
			}
		}
		if (p_unit.IsArmouredTrain && ((_numberOfOurArmouredTrains > 0 && p_player.ListOfUnits.Count / _numberOfOurArmouredTrains + 1 < 50) || (_numberOfOurArmouredTrains == 0 && p_player.ListOfUnits.Count < 50)))
		{
			return false;
		}
		if (!p_unit.IsDestroyer && _numberOfOurDestroyers < _numberOfEnemySubmarines && p_unit.FilterType == "FilterBoat")
		{
			return false;
		}
		if (p_unit.IsCommander)
		{
			if (_isCampaign)
			{
				return false;
			}
			if (_boughtCommanderThisTurn)
			{
				return false;
			}
			if ((_numberOfOurCommanders == 0 && p_player.ListOfUnits.Count < 30) || (_numberOfOurCommanders > 0 && p_player.ListOfUnits.Count / _numberOfOurCommanders + 1 < 30))
			{
				return false;
			}
			if (string.IsNullOrEmpty(p_unit.CommanderTargets))
			{
				return false;
			}
			if (p_unit.CommanderTargets.Contains("FilterInfantry"))
			{
				if (_numberOfOurInfantry < 10)
				{
					return false;
				}
			}
			else
			{
				if (_numberOfOurTanks < 5 && p_unit.CommanderTargets.Contains("FilterTank"))
				{
					return false;
				}
				if (_numberOfOurFighters < 5 && p_unit.CommanderTargets.Contains("FilterFighter"))
				{
					return false;
				}
				if (_numberOfOurShips < 5 && p_unit.CommanderTargets.Contains("FilterBoat"))
				{
					return false;
				}
				if (_numberOfOurArtillery < 5 && p_unit.CommanderTargets.Contains("FilterArtillery"))
				{
					return false;
				}
				if (_numberOfOurAntitanks < 5 && p_unit.CommanderTargets.Contains("FilterAntitank"))
				{
					return false;
				}
				if (_numberOfOurAA < 5 && p_unit.CommanderTargets.Contains("FilterAA"))
				{
					return false;
				}
				if (_numberOfOurBombers < 5 && (p_unit.CommanderTargets.Contains("FilterCAS") || p_unit.CommanderTargets.Contains("FilterBomber")))
				{
					return false;
				}
			}
		}
		if ((!GameData.Instance.map.IsWinter || !GameData.Instance.map.AllowSnowInWinter) && p_unit.Name == "NKL-26")
		{
			return false;
		}
		p_unit.CalculateAndSetUnitPrice();
		if (p_unit.IsCommander && PlayerSettings.Instance.Policies)
		{
			if (p_player.HighCommandPoints - p_unit.Cost <= 0)
			{
				return false;
			}
		}
		else if (p_player.MoneyAmount - p_unit.Cost <= 0)
		{
			return false;
		}
		DateTime dateTime = new DateTime(p_unit.ProductionYear, p_unit.ProductionMonth, 1);
		if (PlayerSettings.Instance.TechTree)
		{
			if (!p_player.ListOfUnitsResearched.Contains(p_unit.Name))
			{
				return false;
			}
		}
		else if (_currDate < dateTime)
		{
			return false;
		}
		if (!_isMinor && p_unit.MaxMP > 0 && _currYear >= 1935 && _currYear <= 1946 && !p_unit.IsCommander && p_unit.FilterType != "FilterBoat")
		{
			if (dateTime.Year < 1939)
			{
				int year = Mathf.Clamp(dateTime.Year, 1939, 1947);
				dateTime = new DateTime(year, dateTime.Month, dateTime.Day);
			}
			float num = (float)_currDate.Subtract(dateTime).Days / 365f;
			if (num > 0f)
			{
				float num2 = 2f;
				if (p_unit.FilterType == "FilterInfantry")
				{
					num2 = 10f;
				}
				else if (p_unit.FilterType == "FilterArtillery" && p_unit.Type != "Tank")
				{
					num2 = 10f;
				}
				else if (p_unit.FilterType == "FilterAntitank" && p_unit.Type != "Tank")
				{
					if (dateTime.Year < 1939)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1940)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1943)
					{
						num2 = 5f;
					}
				}
				else if (p_unit.FilterType == "FilterAA" && p_unit.Type != "Tank")
				{
					num2 = ((dateTime.Year >= 1939) ? 10f : 3f);
				}
				else if (p_unit.IsRecon)
				{
					if (dateTime.Year <= 1939)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1940)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1943)
					{
						num2 = 3f;
					}
				}
				else if (p_unit.FilterType == "FilterTank" || p_unit.FilterType == "FilterAntitank" || p_unit.FilterType == "FilterFighter" || p_unit.FilterType == "FilterAA" || p_unit.IsSubmarine)
				{
					if (dateTime.Year <= 1939)
					{
						num2 = 2.5f;
					}
					else if (dateTime.Year <= 1940)
					{
						num2 = 2.5f;
					}
					else if (dateTime.Year <= 1943)
					{
						num2 = 3f;
					}
				}
				else if (p_unit.FilterType == "FilterBomber" || p_unit.FilterType == "FilterCAS")
				{
					if (dateTime.Year <= 1939)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1940)
					{
						num2 = 3f;
					}
					else if (dateTime.Year <= 1943)
					{
						num2 = 3f;
					}
				}
				if (num > num2)
				{
					return false;
				}
			}
		}
		if (p_unit.Country != _currPlayerCountry)
		{
			if (_isMajor)
			{
				return false;
			}
			if (!GameData.Instance.TryFindPlayerByCountry(p_unit.Country, out var o_player))
			{
				return false;
			}
			if (!CanPurchaseUnitsFromPlayer(o_player))
			{
				return false;
			}
		}
		if (_upkeepSettingON && p_player.turnIncome - p_unit.GetUpkeepCost(p_getReserveCost: false) * 5 < 0)
		{
			return false;
		}
		p_unit.CalculateAndSetManpowerCost();
		if (_manpowerSettingON && _currentAvailableManpower - p_unit.manpowerCost < 0)
		{
			return false;
		}
		return true;
	}

	private bool ShouldWeBuy(string p_filterName)
	{
		switch (p_filterName)
		{
		case "FilterCAS":
		{
			if (!_hasAirport)
			{
				return false;
			}
			if (!_hasCAS)
			{
				return false;
			}
			if (_numberOfOurFighters <= 0)
			{
				return false;
			}
			if (_hasFighters && _numberOfOurBombers > _numberOfOurFighters)
			{
				return false;
			}
			if (_numberOfOurFighters < _numberOfEnemyFighters)
			{
				return false;
			}
			if (_isMinor && _ourPlanesToGroundUnitsRatio >= 0.1f)
			{
				return false;
			}
			if (_ourPlanesToGroundUnitsRatio >= 0.2f)
			{
				return false;
			}
			int num2 = 0;
			if (_hasArtillery)
			{
				num2 += _numberOfOurArtillery;
			}
			if (_hasShips)
			{
				num2 += _numberOfOurShips;
			}
			if (num2 == 0)
			{
				num2 = _numberOfOurFighters;
			}
			return _numberOfOurBombers < num2;
		}
		case "FilterBomber":
		{
			if (!_hasAirport)
			{
				return false;
			}
			if (!_hasStrategicBombers)
			{
				return false;
			}
			if (_numberOfOurFighters <= 0)
			{
				return false;
			}
			if (_hasFighters && _numberOfOurBombers > _numberOfOurFighters)
			{
				return false;
			}
			if (_numberOfOurFighters < _numberOfEnemyFighters)
			{
				return false;
			}
			if (_isMinor && _ourPlanesToGroundUnitsRatio >= 0.1f)
			{
				return false;
			}
			if (_ourPlanesToGroundUnitsRatio >= 0.2f)
			{
				return false;
			}
			int num = 0;
			if (_hasArtillery)
			{
				num += _numberOfOurArtillery;
			}
			if (_hasShips)
			{
				num += _numberOfOurShips;
			}
			if (num == 0)
			{
				num = _numberOfOurFighters;
			}
			if (_numberOfOurBombers >= num)
			{
				if (_hasBarrageBalloons)
				{
					return _numberOfEnemyPlanes > _numberOfOurPlanes;
				}
				return false;
			}
			return true;
		}
		case "FilterFighter":
			if (!_hasAirport)
			{
				return false;
			}
			if (!_hasFighters)
			{
				return false;
			}
			if (_numberOfOurFighters == 0)
			{
				return true;
			}
			if (_numberOfEnemyFighters > 0 && _numberOfAlliedFighters <= _numberOfEnemyFighters * 2)
			{
				return true;
			}
			if (_isMinor && _ourPlanesToGroundUnitsRatio >= 0.1f)
			{
				return false;
			}
			if (_ourPlanesToGroundUnitsRatio >= 0.2f)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}

	private void DeployUnits(Player p_player, bool p_forBorderGuarding)
	{
		if ((!p_forBorderGuarding && p_player.PlayersAtWar.Count == 0) || !p_player.AreThereUnitsReadyForDeployment())
		{
			return;
		}
		_currentUpkeepCost = p_player.CalculateTotalUnitsUpkeep(p_applyReserveModifier: true);
		foreach (Unit listOfUnit in p_player.ListOfUnits)
		{
			if (!listOfUnit.IsReserve || (_productionTakesTimeON && listOfUnit.TurnToProduce > 0))
			{
				continue;
			}
			if (listOfUnit.BuiltByEngineers || listOfUnit.IsMissile)
			{
				reservePanel.PrepareSpawningZone(listOfUnit);
				if (MapGO.listOfSpawnableTiles.Count == 0)
				{
					continue;
				}
				foreach (TileGO listOfSpawnableTile in MapGO.listOfSpawnableTiles)
				{
					bool flag = true;
					if (listOfUnit.IsSupplyDepot)
					{
						foreach (Tile neighbour in listOfSpawnableTile.tile.neighbours)
						{
							if (neighbour.GroundUnit != null && neighbour.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						listOfSpawnableTile.SpawnUnit(listOfUnit, p_playSound: false, p_sendRPC: true);
						_currentUpkeepCost += listOfUnit.GetUpkeepCost(p_getReserveCost: false);
						break;
					}
				}
			}
			else
			{
				if (!listOfUnit.IsCatapultPlane && !listOfUnit.CanBeRefueledOnCarrier)
				{
					continue;
				}
				if (ShouldDisbandUnit(listOfUnit))
				{
					DisbandUnit(listOfUnit);
					continue;
				}
				foreach (Unit listOfUnit2 in p_player.ListOfUnits)
				{
					if (!listOfUnit2.IsReserve && !listOfUnit2.IsDead && listOfUnit2.CurrHP != 0 && ((listOfUnit2.IsCarrier && listOfUnit.CanBeRefueledOnCarrier) || (listOfUnit2.CanCatapultPlanes && listOfUnit.IsCatapultPlane)) && (bool)listOfUnit2.unitGO && (bool)listOfUnit2.unitGO.tileGO && listOfUnit2.unitGO.tileGO.airUnitGO == null)
					{
						MapGO.listOfSpawnableTiles.Add(listOfUnit2.unitGO.tileGO);
						listOfUnit2.unitGO.tileGO.SpawnUnit(listOfUnit, p_playSound: false, p_sendRPC: true);
						_currentUpkeepCost += listOfUnit.GetUpkeepCost(p_getReserveCost: false);
						break;
					}
				}
			}
		}
		if (p_forBorderGuarding && p_player.PlayersAtWar.Count == 0)
		{
			List<BorderData> borders = p_player.GetBorders(GameData.Instance.map);
			if (borders.Count == 0)
			{
				return;
			}
			borders.Sort((BorderData a, BorderData borderData) =>
			{
				int dangerLevel2 = a.OtherPlayer.GetDangerLevel(p_player);
				int dangerLevel3 = borderData.OtherPlayer.GetDangerLevel(p_player);
				return (dangerLevel2 != dangerLevel3) ? dangerLevel3.CompareTo(dangerLevel2) : borderData.OtherPlayer.ListOfUnits.Count.CompareTo(a.OtherPlayer.ListOfUnits.Count);
			});
			List<Tile> list = new List<Tile>();
			Dictionary<Tile, int> dictionary = new Dictionary<Tile, int>();
			for (int num = 0; num < borders.Count; num++)
			{
				foreach (Tile myTile in borders[num].MyTiles)
				{
					if (myTile.enumType != TileTypes.RIVER && myTile.enumType != TileTypes.MOUNTAIN && myTile.enumType != TileTypes.SEA)
					{
						list.Add(myTile);
						dictionary[myTile] = num;
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			int[] array = new int[borders.Count];
			int[] array2 = new int[borders.Count];
			for (int num2 = 0; num2 < borders.Count; num2++)
			{
				int dangerLevel = borders[num2].OtherPlayer.GetDangerLevel(p_player);
				int num3;
				if (dangerLevel >= 3)
				{
					num3 = 2;
				}
				else
				{
					num3 = ((dangerLevel >= 1) ? 3 : 4);
				}
				int num4 = 0;
				foreach (Tile myTile2 in borders[num2].MyTiles)
				{
					if (myTile2.enumType != TileTypes.RIVER && myTile2.enumType != TileTypes.MOUNTAIN && myTile2.enumType != TileTypes.SEA)
					{
						num4++;
					}
				}
				array[num2] = Mathf.Max(1, Mathf.CeilToInt((float)num4 / (float)num3));
			}
			HashSet<Tile> hashSet = new HashSet<Tile>(list);
			foreach (Tile item in list)
			{
				int num5 = dictionary[item];
				if (array2[num5] >= array[num5])
				{
					continue;
				}
				if (item.GroundUnit != null && item.GroundUnit.OwnerPlayer == p_player && item.GroundUnit.MaxMP > 0)
				{
					array2[num5]++;
					continue;
				}
				for (int num6 = 0; num6 < item.neighbours.Count; num6++)
				{
					Tile tile = item.neighbours[num6];
					if (tile != null && tile.GroundUnit != null && tile.GroundUnit.OwnerPlayer == p_player && tile.GroundUnit.MaxMP > 0 && !hashSet.Contains(tile))
					{
						array2[num5]++;
						break;
					}
				}
			}
			p_player.ListOfUnits = (from x in p_player.ListOfUnits
				orderby x.Type == "Infantry" descending, x.Cost descending
				select x).ToList();
			{
				foreach (Unit listOfUnit3 in p_player.ListOfUnits)
				{
					if (!p_player.AreThereUnitsReadyForDeployment())
					{
						break;
					}
					if (!listOfUnit3.IsReserve || listOfUnit3.MaxMP == 0 || listOfUnit3.IsCommander || listOfUnit3.IsArmouredTrain || listOfUnit3.CurrHP < listOfUnit3.MaxHP || (_productionTakesTimeON && listOfUnit3.TurnToProduce > 0) || listOfUnit3.Type == "Plane" || listOfUnit3.Type == "Boat")
					{
						continue;
					}
					reservePanel.PrepareSpawningZone(listOfUnit3);
					if (MapGO.listOfSpawnableTiles.Count == 0)
					{
						continue;
					}
					TileGO tileGO = null;
					int num7 = int.MaxValue;
					for (int num8 = 0; num8 < borders.Count; num8++)
					{
						if (array2[num8] >= array[num8])
						{
							continue;
						}
						foreach (TileGO listOfSpawnableTile2 in MapGO.listOfSpawnableTiles)
						{
							foreach (Tile myTile3 in borders[num8].MyTiles)
							{
								if (myTile3.enumType != TileTypes.RIVER && myTile3.enumType != TileTypes.MOUNTAIN && myTile3.enumType != TileTypes.SEA)
								{
									int heuristicDistance = Utils.GetHeuristicDistance(listOfSpawnableTile2.tile, myTile3);
									if (heuristicDistance < num7)
									{
										num7 = heuristicDistance;
										tileGO = listOfSpawnableTile2;
									}
								}
							}
						}
						if (tileGO != null)
						{
							tileGO.SpawnUnit(listOfUnit3, p_playSound: false, p_sendRPC: true);
							_currentUpkeepCost += listOfUnit3.GetUpkeepCost(p_getReserveCost: false);
							array2[num8]++;
							break;
						}
					}
				}
				return;
			}
		}
		List<TileGO> list2 = new List<TileGO>();
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.ownerPlayer != null && victoryPoint.tile.OwnerName == p_player.Name)
			{
				list2.Add(victoryPoint);
			}
		}
		_VPdangerLevels.Clear();
		foreach (TileGO item2 in list2)
		{
			VPDangerData vPDangerData = new VPDangerData();
			foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(item2.tile, 3))
			{
				if (!item3.isVisible)
				{
					continue;
				}
				if (item3.tileGO.groundUnitGO != null && item3.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && item3.tileGO.groundUnitGO.unit.MaxMP > 0)
				{
					if (item3.tileGO.groundUnitGO.unit.FilterType == "FilterBoat")
					{
						vPDangerData.dangerValue++;
					}
					else if (item3.tileGO.groundUnitGO.unit.IsCommander)
					{
						vPDangerData.dangerValue++;
					}
					else if (item3.tileGO.groundUnitGO.unit.Type == "Artillery" || item3.tileGO.groundUnitGO.unit.Type == "AA")
					{
						vPDangerData.dangerValue += 2;
					}
					else if (item3.tileGO.groundUnitGO.unit.Type == "Tank")
					{
						vPDangerData.dangerValue += 4;
					}
					else
					{
						vPDangerData.dangerValue += 5;
					}
					if (item3.tileGO.groundUnitGO.unit.IsEliteUnit)
					{
						vPDangerData.dangerValue++;
					}
					if (item3.tileGO.groundUnitGO.unit.IsPoliticalUnit)
					{
						vPDangerData.dangerValue++;
					}
					if (item3.tileGO.groundUnitGO.unit.Type == "Infantry" || item3.tileGO.groundUnitGO.unit.Type == "Tank")
					{
						vPDangerData.enemyToMatch++;
					}
				}
				if (!(item3.tileGO.airUnitGO != null) || !item3.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer || item3.tileGO.airUnitGO.unit.MaxMP <= 0)
				{
					continue;
				}
				if (item3.tileGO.airUnitGO.unit.IsParatrooper)
				{
					vPDangerData.dangerValue += 3;
					if (item3.tileGO.airUnitGO.unit.IsEliteUnit)
					{
						vPDangerData.dangerValue++;
					}
					if (item3.tileGO.airUnitGO.unit.IsPoliticalUnit)
					{
						vPDangerData.dangerValue++;
					}
				}
				else
				{
					vPDangerData.dangerValue++;
				}
			}
			_VPdangerLevels.Add(item2, vPDangerData);
		}
		_VPdangerLevels = (from x in _VPdangerLevels
			orderby x.Key.tile.PrimaryObjective descending, x.Value.dangerValue descending
			select x).ToDictionary((KeyValuePair<TileGO, VPDangerData> x) => x.Key, (KeyValuePair<TileGO, VPDangerData> x) => x.Value);
		p_player.ListOfUnits = (from x in p_player.ListOfUnits
			orderby x.Type == "Infantry" descending, x.FilterType == "FilterAA" descending, x.FilterType == "FilterArtillery" descending, x.FilterType == "FilterAntitank" descending, x.Cost descending
			select x).ToList();
		Dictionary<Unit, HashSet<TileGO>> dictionary2 = new Dictionary<Unit, HashSet<TileGO>>();
		foreach (Unit listOfUnit4 in p_player.ListOfUnits)
		{
			if (listOfUnit4.IsReserve && (!_productionTakesTimeON || listOfUnit4.TurnToProduce <= 0))
			{
				reservePanel.PrepareSpawningZone(listOfUnit4);
				if (MapGO.listOfSpawnableTiles.Count > 0)
				{
					dictionary2[listOfUnit4] = new HashSet<TileGO>(MapGO.listOfSpawnableTiles);
				}
			}
		}
		for (byte b = 0; b < 2; b++)
		{
			foreach (KeyValuePair<TileGO, VPDangerData> vPdangerLevel in _VPdangerLevels)
			{
				if (b == 0 || vPdangerLevel.Value.dangerValue == 0)
				{
					if (vPdangerLevel.Key.groundUnitGO != null)
					{
						continue;
					}
					bool flag2 = false;
					foreach (Tile item4 in Utils.GetAllTilesInRadiusAround(vPdangerLevel.Key.tile, 10))
					{
						if ((item4.terrainOwner != null && item4.terrainOwner.isAtWarWithCurrentPlayer) || (item4.GroundUnit != null && item4.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer) || (item4.AirUnit != null && item4.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						continue;
					}
				}
				vPdangerLevel.Value.matchingUnitsDeployed = 0;
				foreach (Tile item5 in Utils.GetAllTilesInRadiusAround(vPdangerLevel.Key.tile, 3))
				{
					if ((!item5.IsVictoryPoint || item5 == vPdangerLevel.Key.tile) && item5.GroundUnit != null && (item5.GroundUnit.Type == "Infantry" || item5.GroundUnit.Type == "Tank") && item5.GroundUnit.OwnerName == TurnManager.currPlayer.Name)
					{
						vPdangerLevel.Value.matchingUnitsDeployed++;
					}
				}
				bool flag3 = false;
				foreach (Unit listOfUnit5 in p_player.ListOfUnits)
				{
					if (!listOfUnit5.IsReserve || listOfUnit5.MaxMP == 0 || listOfUnit5.CurrHP < listOfUnit5.MaxHP || listOfUnit5.IsCommander || listOfUnit5.IsArmouredTrain || (_productionTakesTimeON && listOfUnit5.TurnToProduce > 0))
					{
						continue;
					}
					if (vPdangerLevel.Value.enemyToMatch > 0 && vPdangerLevel.Value.matchingUnitsDeployed >= vPdangerLevel.Value.enemyToMatch)
					{
						break;
					}
					if (ShouldDisbandUnit(listOfUnit5))
					{
						DisbandUnit(listOfUnit5);
					}
					else
					{
						if (!dictionary2.TryGetValue(listOfUnit5, out var value) || value.Count == 0)
						{
							continue;
						}
						List<Tile> list3;
						if (listOfUnit5.Type == "Plane")
						{
							list3 = (from x in Utils.GetAllTilesInRadiusAround(vPdangerLevel.Key.tile, 1)
								orderby x.IsVictoryPoint descending, x.GroundUnit == null || x.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer descending
								select x).ToList();
						}
						else
						{
							list3 = ((!(listOfUnit5.Type == "Tank")) ? (from x in Utils.GetAllTilesInRadiusAround(vPdangerLevel.Key.tile, 1)
								orderby x.IsVictoryPoint descending, x.DamageModificator descending
								select x).ToList() : (from x in Utils.GetAllTilesInRadiusAround(vPdangerLevel.Key.tile, 1)
								orderby x.IsVictoryPoint descending, x.DamageModificator
								select x).ToList());
						}
						foreach (Tile item6 in list3)
						{
							if (!value.Contains(item6.tileGO))
							{
								continue;
							}
							bool flag4 = false;
							if (listOfUnit5.Type == "Plane" && listOfUnit5.GetNameToLower().Contains("balloon"))
							{
								if (item6.Capturable && item6.tileGO.tile.MoneyProductionPerTurn >= 15 && item6.tileGO.tile.NumberOfTurnsAtLowProd == 0)
								{
									flag4 = true;
								}
							}
							else if (listOfUnit5.FilterType == "FilterAA")
							{
								if (IsThereSpecificEnemyUnitAround(item6, 2, "Plane") && !IsThereSimilarUnitAround(item6, 1, listOfUnit5))
								{
									flag4 = true;
								}
							}
							else if (listOfUnit5.FilterType == "FilterAntitank" || (listOfUnit5.Type == "Infantry" && listOfUnit5.Name.Contains("AT")))
							{
								if (IsThereSpecificEnemyUnitAround(item6, 2, "Tank", listOfUnit5.HardDamage) && !IsThereSimilarUnitAround(item6, 1, listOfUnit5))
								{
									flag4 = true;
								}
							}
							else if (listOfUnit5.FilterType == "FilterArtillery")
							{
								if (!AreThereEnemiesAround(item6.tileGO, 1, p_includeAir: false, p_useFogOfWar: true) || item6.IsVictoryPoint)
								{
									flag4 = true;
								}
							}
							else if (listOfUnit5.Type == "Tank")
							{
								if (!IsThereSimilarUnitAround(item6, 1, listOfUnit5) && (!IsThereSpecificEnemyUnitAround(item6, 2, "Tank") || IsThereSpecificEnemyUnitAround(item6, 2, "Tank", listOfUnit5.HardDamage)))
								{
									flag4 = true;
								}
							}
							else
							{
								flag4 = true;
							}
							if (!flag4)
							{
								continue;
							}
							flag3 = true;
							if (listOfUnit5.Type == "Infantry" || listOfUnit5.Type == "Tank")
							{
								vPdangerLevel.Value.matchingUnitsDeployed++;
							}
							MapGO.listOfSpawnableTiles.Clear();
							MapGO.listOfSpawnableTiles.Add(item6.tileGO);
							item6.tileGO.SpawnUnit(listOfUnit5, p_playSound: false, p_sendRPC: true);
							_currentUpkeepCost += listOfUnit5.GetUpkeepCost(p_getReserveCost: false);
							dictionary2.Remove(listOfUnit5);
							foreach (HashSet<TileGO> value5 in dictionary2.Values)
							{
								value5.Remove(item6.tileGO);
							}
							break;
						}
						if ((b == 0) & flag3)
						{
							break;
						}
					}
				}
				if (!p_player.AreThereUnitsReadyForDeployment())
				{
					break;
				}
			}
		}
		Dictionary<TileGO, int> dictionary3 = new Dictionary<TileGO, int>();
		foreach (TileGO item7 in list2)
		{
			int num9 = int.MaxValue;
			foreach (TileGO item8 in _enemyVPsGO)
			{
				int num10 = Utils.GetHeuristicDistance(item7.tile, item8.tile);
				if (item8.tile.enumType == TileTypes.SEA)
				{
					num10 *= 10;
				}
				if (num10 < num9)
				{
					num9 = num10;
				}
			}
			dictionary3.Add(item7, num9);
		}
		List<TileGO> list4 = (from x in dictionary3
			orderby x.Value
			select x.Key).ToList();
		int num11 = 6;
		for (int num12 = 0; num12 < num11; num12++)
		{
			if (!p_player.AreThereUnitsReadyForDeployment())
			{
				break;
			}
			for (int num13 = 0; num13 < list4.Count; num13++)
			{
				bool flag5 = false;
				foreach (Unit listOfUnit6 in p_player.ListOfUnits)
				{
					if (!listOfUnit6.IsReserve || listOfUnit6.BuiltByEngineers || (_productionTakesTimeON && listOfUnit6.TurnToProduce > 0))
					{
						continue;
					}
					if (listOfUnit6.IsCommander)
					{
						if (num12 < num11 - 2)
						{
							continue;
						}
						bool flag6 = false;
						foreach (UnitGO item9 in MapGO.listOfAllGeneralsOnTheMap)
						{
							if (item9.unit.OwnerPlayer.isAlliedWithCurrentPlayer && Utils.IsTileInsideRadiusSimplified(list4[num13].tile, item9.tileGO.tile, 15))
							{
								flag6 = true;
								break;
							}
						}
						if (flag6 || (num12 < num11 - 1 && _VPdangerLevels.TryGetValue(list4[num13], out var value2) && value2.dangerValue > 0))
						{
							continue;
						}
					}
					if (ShouldDisbandUnit(listOfUnit6))
					{
						DisbandUnit(listOfUnit6);
					}
					else
					{
						if ((listOfUnit6.CurrHP < listOfUnit6.MaxHP && (num12 < num11 - 2 || (num12 < num11 - 1 && _VPdangerLevels.TryGetValue(list4[num13], out var value3) && value3.dangerValue > 0))) || !dictionary2.TryGetValue(listOfUnit6, out var value4) || value4.Count == 0)
						{
							continue;
						}
						if (listOfUnit6.MaxMP == 0)
						{
							bool flag7 = false;
							foreach (Tile item10 in Utils.GetAllTilesInRadiusAround(list4[num13].tile, 2))
							{
								if (item10.tileGO.groundUnitGO != null && item10.tileGO.groundUnitGO.unit.MaxMP == 0)
								{
									flag7 = true;
									break;
								}
							}
							if (flag7)
							{
								continue;
							}
						}
						else if (num12 <= 1)
						{
							bool flag8 = false;
							foreach (Tile item11 in Utils.GetAllTilesInRadiusAround(list4[num13].tile, 1))
							{
								if (!item11.IsVictoryPoint && item11.tileGO.groundUnitGO != null && item11.tileGO.groundUnitGO.unit.FilterType == listOfUnit6.FilterType && item11.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)
								{
									flag8 = true;
									break;
								}
							}
							if (flag8)
							{
								continue;
							}
						}
						bool flag9 = false;
						foreach (Tile item12 in Utils.GetAllTilesInRadiusAround(list4[num13].tile, 1))
						{
							if (!value4.Contains(item12.tileGO))
							{
								continue;
							}
							if (listOfUnit6.MaxMP > 0)
							{
								if (listOfUnit6.FilterType == "FilterArtillery" && AreThereEnemiesAround(item12.tileGO, 1, p_includeAir: false, p_useFogOfWar: true) && !item12.IsVictoryPoint)
								{
									continue;
								}
								flag9 = true;
							}
							else if (listOfUnit6.Type == "Plane" && listOfUnit6.GetNameToLower().Contains("balloon"))
							{
								if (item12.Capturable && item12.tileGO.tile.MoneyProductionPerTurn > 5 && item12.tileGO.tile.NumberOfTurnsAtLowProd == 0)
								{
									flag9 = true;
								}
							}
							else if (!item12.tileGO.tile.IsVictoryPoint)
							{
								if (listOfUnit6.IsArmourObstacle)
								{
									if (item12.tileGO.tile.enumType == TileTypes.ROAD || item12.tileGO.tile.Bridge)
									{
										flag9 = true;
									}
								}
								else
								{
									flag9 = true;
								}
							}
							if (!flag9)
							{
								continue;
							}
							MapGO.listOfSpawnableTiles.Clear();
							MapGO.listOfSpawnableTiles.Add(item12.tileGO);
							item12.tileGO.SpawnUnit(listOfUnit6, p_playSound: false, p_sendRPC: true);
							_currentUpkeepCost += listOfUnit6.GetUpkeepCost(p_getReserveCost: false);
							dictionary2.Remove(listOfUnit6);
							foreach (HashSet<TileGO> value6 in dictionary2.Values)
							{
								value6.Remove(item12.tileGO);
							}
							flag5 = true;
							break;
						}
						if (flag5)
						{
							break;
						}
					}
				}
			}
		}
		foreach (Unit listOfUnit7 in p_player.ListOfUnits)
		{
			if (listOfUnit7.IsReserve && listOfUnit7.Lvl < 3 && (!_productionTakesTimeON || listOfUnit7.TurnToProduce == 0))
			{
				DisbandUnit(listOfUnit7);
			}
		}
	}

	private void GatherEnemyVictoryPoints(Player p_player)
	{
		needToRefreshEnemyVPs = false;
		_numberOfEnemyVPs = 0;
		_numberOfEnemyLandVPs = 0;
		_hasAirport = false;
		_hasHarbour = false;
		_hasHeavyProductionMeans = false;
		_enemyVPsGO.Clear();
		_ourLandMassesIndexes.Clear();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!hashSet.Contains(listOfPlayer.Country))
			{
				if (ShouldWeAttackPlayer(listOfPlayer.Country))
				{
					hashSet.Add(listOfPlayer.Country);
					listOfPlayer.canAIattackPlayer = true;
				}
				else
				{
					listOfPlayer.canAIattackPlayer = false;
				}
			}
		}
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.ownerPlayer == null || victoryPoint.tile.ownerPlayer.Country == null)
			{
				continue;
			}
			if (victoryPoint.tile.OwnerName == p_player.Name)
			{
				if (victoryPoint.tile.landAreaIndex.HasValue && !_ourLandMassesIndexes.Contains(victoryPoint.tile.landAreaIndex.Value))
				{
					_ourLandMassesIndexes.Add(victoryPoint.tile.landAreaIndex.Value);
				}
				if (_hasAirport && _hasHarbour && _hasHeavyProductionMeans)
				{
					continue;
				}
				if (victoryPoint.tile.IsReinforcementTile)
				{
					_hasAirport = true;
					_hasHeavyProductionMeans = true;
					if (victoryPoint.tile.enumType == TileTypes.SEA)
					{
						_hasHarbour = true;
					}
					continue;
				}
				if (!_hasAirport && victoryPoint.tile.enumType == TileTypes.AIRPORT)
				{
					_hasAirport = true;
				}
				if (!_hasHarbour && victoryPoint.tile.enumType == TileTypes.HARBOUR && !victoryPoint.tile.Name.Contains("Small"))
				{
					_hasHarbour = true;
				}
				if (!_hasHeavyProductionMeans && (victoryPoint.tile.Name.ToLower().Contains("city") || victoryPoint.tile.Type == "Factory"))
				{
					_hasHeavyProductionMeans = true;
				}
			}
			else if (victoryPoint.tile.Capturable && hashSet.Contains(victoryPoint.tile.ownerPlayer.Country))
			{
				_numberOfEnemyVPs++;
				if (victoryPoint.tile.enumType != TileTypes.SEA)
				{
					_numberOfEnemyLandVPs++;
				}
				_enemyVPsGO.Add(victoryPoint);
			}
		}
	}

	private void CheckWinningWar(Player p_player)
	{
		_enemies_production = 0;
		_alliance_production = 0;
		if (p_player.PlayersAtWar.Count == 0)
		{
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.CalculateTurnIncome();
			if (listOfPlayer.isAtWarWithCurrentPlayer)
			{
				_enemies_production += listOfPlayer.baseIncome;
			}
			else if (listOfPlayer.isAlliedWithCurrentPlayer)
			{
				_alliance_production += listOfPlayer.baseIncome;
			}
		}
		if ((float)_enemies_production == 0f)
		{
			_enemies_production = 1;
		}
		_warIncomeRatio = (float)_alliance_production / (float)_enemies_production;
		_areWeWinning = _warIncomeRatio >= 0.85f;
	}

	private void GoToWar()
	{
		if (!_isHistoricalFocusON || MapGO.currentPlayerVPs.Count == 0)
		{
			return;
		}
		_dicPlayersIntact.Clear();
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			Tile tile = victoryPoint.tile;
			if (tile.Capturable && tile.ownerPlayer != null && tile.ownerPlayer.Country != null)
			{
				if (!_dicPlayersIntact.ContainsKey(tile.ownerPlayer))
				{
					_dicPlayersIntact.Add(tile.ownerPlayer, value: true);
				}
				if (tile.OriginalCountryOwner != tile.ownerPlayer.Country && tile.ownerPlayer.Country != _currPlayerCountry && GameData.Instance.TryFindPlayerByCountry(tile.OriginalCountryOwner, out var o_player))
				{
					_dicPlayersIntact[o_player] = false;
				}
			}
		}
		Player o_player2 = null;
		foreach (KeyValuePair<Player, bool> item in _dicPlayersIntact)
		{
			if (item.Value || item.Key.PlayersAtWar.Count == 0)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			foreach (TileGO victoryPoint2 in MapGO.victoryPoints)
			{
				if (victoryPoint2.tile.OriginalCountryOwner == item.Key.Country && victoryPoint2.tile.OwnerName != TurnManager.currPlayer.Name)
				{
					GameData.Instance.TryFindPlayerByName(victoryPoint2.tile.OwnerName, out o_player2);
					if (o_player2 != null && !o_player2.IsComputer)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (TurnManager.currPlayer.Faction == "Neutral")
			{
				if (_isPacificScenario)
				{
					if (_currPlayerCountry == "USA")
					{
						if (item.Key.Country == "Philippines")
						{
							flag = true;
							string text = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
							UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text);
							TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
							MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text);
						}
					}
					else if (_currPlayerCountry == "Great Britain" || _currPlayerCountry == "British Raj")
					{
						if (item.Key.Country == "Great Britain" || item.Key.Country == "British Raj")
						{
							flag = true;
							string text2 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
							UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text2);
							TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
							MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text2);
						}
					}
					else if (_currPlayerCountry == "Great Britain" || _currPlayerCountry == "Netherlands" || _currPlayerCountry == "France")
					{
						if (item.Key.Country == "Great Britain" || item.Key.Country == "Netherlands" || item.Key.Country == "France")
						{
							flag = true;
							string text3 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
							UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text3);
							TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
							MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text3);
						}
					}
					else if ((_currPlayerCountry == "Great Britain" || _currPlayerCountry == "USA" || _currPlayerCountry == "New Zealand" || _currPlayerCountry == "Australia") && (item.Key.Country == "Great Britain" || item.Key.Country == "USA" || item.Key.Country == "New Zealand" || item.Key.Country == "Australia"))
					{
						flag = true;
						string text4 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
						UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text4);
						TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
						MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text4);
					}
				}
				if (_currPlayerCountry == "France" || _currPlayerCountry == "Great Britain")
				{
					if ((item.Key.Country == "Netherlands" || item.Key.Country == "Belgium" || item.Key.Country == "Luxembourg" || item.Key.Country == "Norway" || item.Key.Country == "Greece" || item.Key.Country == "France") && (flag2 || item.Key.Country == "Greece" || item.Key.Country == "France"))
					{
						flag = true;
						string text5 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
						UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text5);
						TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
						MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text5);
					}
				}
				else if (_currPlayerCountry == "Belgium")
				{
					if (item.Key.Country == "Netherlands" && flag2)
					{
						flag = true;
						string text6 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
						UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text6);
						TurnManager.currPlayer.ChangeFaction("Allies", p_getNewAllianceWars: true, p_sendRPC: true);
						MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text6);
					}
				}
				else if (_currPlayerCountry == "Soviet Union" && (item.Key.Country == "Estonia" || item.Key.Country == "Latvia") && flag2)
				{
					flag = true;
					string text7 = LocalizationManager.Translate("relationship-claim tolerance");
					UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text7);
					TurnManager.currPlayer.ChangeFaction("Communists", p_getNewAllianceWars: true, p_sendRPC: true);
					MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text7);
				}
			}
			else if (_currPlayerCountry == "Nazi Germany")
			{
				if (item.Key.Country == "Sweden" && flag2 && item.Key.noRelationWithCurrentPlayer)
				{
					flag = true;
					string text8 = LocalizationManager.Translate("relationship-claim tolerance");
					UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text8);
					TurnManager.currPlayer.AddWarWith(o_player2, p_actualDirectWarDeclaration: true, p_sendRPC: true);
					TurnManager.currPlayer.AddAllianceWith(item.Key, p_sendRPC: true);
					MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text8);
				}
			}
			else if (_currPlayerCountry == "Great Britain" && item.Key.Country == "Norway" && flag2 && !item.Key.isAtWarWithCurrentPlayer)
			{
				bool flag3 = false;
				if (!o_player2.isAtWarWithCurrentPlayer)
				{
					flag3 = true;
					TurnManager.currPlayer.AddWarWith(o_player2, p_actualDirectWarDeclaration: true, p_sendRPC: true);
				}
				if (!item.Key.isAlliedWithCurrentPlayer)
				{
					flag3 = true;
					TurnManager.currPlayer.AddAllianceWith(item.Key, p_sendRPC: true);
				}
				if (flag3)
				{
					flag = true;
					string text9 = LocalizationManager.Translate("relationship-support 1") + LocalizationManager.GetLocalizedCountry(item.Key.Country) + LocalizationManager.Translate("relationship-support 2");
					UIManager.ShowRelationshipChangeMessage(_currPlayerCountry, item.Key.Country, text9);
					MultiplayerManager.RPC("RPC_ShowRelationshipWindow", _currPlayerCountry, item.Key.Country, text9);
				}
			}
			if (flag)
			{
				needToRefreshEnemyVPs = true;
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Declare war"));
				MultiplayerManager.RPC("RPC_PlaySound", (short)0, (short)0, "Declare war", "UI");
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
				break;
			}
		}
	}

	private void UpgradeDowngradeUnits()
	{
		if (!_manpowerSettingON || TurnManager.currPlayer.PlayersAtWar.Count == 0)
		{
			return;
		}
		Unit unit = null;
		Unit unit2 = null;
		Unit unit3 = null;
		foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
		{
			if (!(listOfAllUnit.Country != _currPlayerCountry) && _currYear <= listOfAllUnit.EndUseYear && _currYear >= listOfAllUnit.ProductionYear && !listOfAllUnit.IsCommander && !(listOfAllUnit.FilterType != "FilterInfantry") && !listOfAllUnit.IsParatrooper)
			{
				listOfAllUnit.CalculateAndSetUnitPrice();
				listOfAllUnit.CalculateAndSetManpowerCost();
				if (listOfAllUnit.GetNameToLower().Contains("garrison"))
				{
					unit = listOfAllUnit;
				}
				else if (listOfAllUnit.GetNameToLower().Contains("light infantry"))
				{
					unit2 = listOfAllUnit;
				}
				else if (listOfAllUnit.GetNameToLower().Contains("heavy infantry"))
				{
					unit3 = listOfAllUnit;
				}
			}
		}
		if (unit2 == null || unit3 == null)
		{
			List<Unit> list = new List<Unit>();
			foreach (Unit listOfAllUnit2 in MapGO.instance.listOfAllUnits)
			{
				if (!(listOfAllUnit2.Country != _currPlayerCountry) && _currYear <= listOfAllUnit2.EndUseYear && _currYear >= listOfAllUnit2.ProductionYear && !listOfAllUnit2.IsCommander && !(listOfAllUnit2.FilterType != "FilterInfantry") && !listOfAllUnit2.IsParatrooper && !listOfAllUnit2.IsEngineers && !listOfAllUnit2.IsPontoonBridge && !listOfAllUnit2.GetNameToLower().Contains("garrison"))
				{
					list.Add(listOfAllUnit2);
				}
			}
			if (list.Count != 0)
			{
				if (unit2 == null)
				{
					unit2 = list.OrderBy((Unit x) => x.SoftDamage).ElementAt(0);
				}
				if (unit3 == null)
				{
					unit3 = list.OrderByDescending((Unit x) => x.SoftDamage).ElementAt(0);
				}
			}
		}
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsReserve || listOfUnit.IsParatrooper || listOfUnit.IsCommander || listOfUnit.ActionPoints != 2 || listOfUnit.MaxMP == 0 || listOfUnit.Cost >= 300 || !listOfUnit.unitGO || !listOfUnit.unitGO.tileGO || !listOfUnit.unitGO.tileGO.tile.IsVictoryPoint || listOfUnit.unitGO.tileGO.tile.OwnerName != TurnManager.currPlayer.Name || listOfUnit.unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || !string.IsNullOrEmpty(listOfUnit.ForcedCustomName) || listOfUnit.FilterType != "FilterInfantry" || (listOfUnit.unitGO.tileGO.tile.enumType != TileTypes.CITY && listOfUnit.unitGO.tileGO.tile.enumType != TileTypes.FACTORY && listOfUnit.unitGO.tileGO.tile.enumType != TileTypes.HARBOUR))
			{
				continue;
			}
			bool flag = false;
			byte p_radius = 4;
			if (listOfUnit.unitGO.tileGO.tile.PrimaryObjective)
			{
				p_radius = 6;
			}
			else if (listOfUnit.unitGO.tileGO.tile.SecondaryObjective)
			{
				p_radius = 5;
			}
			foreach (Tile item in Utils.GetAllTilesInRadiusAround(listOfUnit.unitGO.tileGO.tile, p_radius))
			{
				if (item.ownerPlayer != null && !item.ownerPlayer.isAlliedWithCurrentPlayer)
				{
					flag = true;
					break;
				}
				if (item.GroundUnit != null && item.GroundUnit.OwnerPlayer != null && item.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					flag = true;
					break;
				}
				if (item.AirUnit != null && item.AirUnit.OwnerPlayer != null && item.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer && item.AirUnit.IsParatrooper)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = listOfUnit.GetNameToLower().Contains("garrison");
			if ((!flag & flag2) || (flag && !flag2))
			{
				continue;
			}
			bool flag3 = false;
			if (flag2)
			{
				if (unit3 != null && listOfUnit.unitGO.tileGO.tile.PrimaryObjective && TurnManager.currPlayer.MoneyAmount - (unit3.Cost - listOfUnit.Cost) > 0 && TurnManager.currPlayer.GetAvailableManpower() - (unit3.manpowerCost - listOfUnit.manpowerCost) > 0)
				{
					listOfUnit.Name = unit3.Name;
					listOfUnit.Type = unit3.Type;
					listOfUnit.BaseType = (string.IsNullOrEmpty(unit3.BaseType) ? unit3.Type : unit3.BaseType);
					listOfUnit.MaxMP = unit3.MaxMP;
					listOfUnit.CurrMP = unit3.MaxMP;
					listOfUnit.BaseMaxMP = unit3.MaxMP;
					if (listOfUnit.CurrHP >= listOfUnit.MaxHP)
					{
						listOfUnit.CurrHP = unit3.MaxHP;
					}
					listOfUnit.MaxHP = unit3.MaxHP;
					listOfUnit.BaseMaxHP = unit3.MaxHP;
					if (listOfUnit.CurrAmmo >= listOfUnit.MaxAmmo)
					{
						listOfUnit.CurrAmmo = unit3.MaxAmmo;
					}
					listOfUnit.MaxAmmo = unit3.MaxAmmo;
					listOfUnit.BaseMaxAmmo = unit3.BaseMaxAmmo;
					if (listOfUnit.CurrAutonomy >= listOfUnit.MaxAutonomy)
					{
						listOfUnit.CurrAutonomy = unit3.MaxAutonomy;
					}
					listOfUnit.MaxAutonomy = unit3.MaxAutonomy;
					listOfUnit.BaseMaxAutonomy = unit3.MaxAutonomy;
					listOfUnit.SoftDamage = unit3.SoftDamage;
					listOfUnit.BaseSoftDamage = unit3.SoftDamage;
					listOfUnit.HardDamage = unit3.HardDamage;
					listOfUnit.BaseHardDamage = unit3.HardDamage;
					listOfUnit.AirDamage = unit3.AirDamage;
					listOfUnit.BaseAirDamage = unit3.AirDamage;
					int num = unit3.Cost - listOfUnit.Cost;
					if (num > 0)
					{
						TurnManager.currPlayer.RemoveMoney(num, p_sendRPC: false);
					}
					else if (num < 0)
					{
						TurnManager.currPlayer.AddMoney(-num, p_sendRPC: false);
					}
					listOfUnit.Cost = unit3.Cost;
					TurnManager.currPlayer.AddManpower(listOfUnit.manpowerCost - unit3.manpowerCost, p_doMarginCheck: false, p_sendRPC: false);
					listOfUnit.manpowerCost = unit3.manpowerCost;
					if (listOfUnit.CurrHP > listOfUnit.MaxHP)
					{
						listOfUnit.CurrHP = listOfUnit.MaxHP;
					}
					if (listOfUnit.CurrAutonomy > listOfUnit.MaxAutonomy)
					{
						listOfUnit.CurrAutonomy = listOfUnit.MaxAutonomy;
					}
					if (listOfUnit.CurrMP > listOfUnit.MaxMP)
					{
						listOfUnit.CurrMP = listOfUnit.MaxMP;
					}
					if (listOfUnit.CurrAmmo > listOfUnit.MaxAmmo)
					{
						listOfUnit.CurrAmmo = listOfUnit.MaxAmmo;
					}
					listOfUnit.IsPoliticalUnit = false;
					listOfUnit.IsEliteUnit = false;
					listOfUnit.IsCavalry = false;
					listOfUnit.IsEngineers = unit3.IsEngineers;
					listOfUnit.IsRecon = unit3.IsRecon;
					flag3 = true;
				}
				else if (unit2 != null && TurnManager.currPlayer.MoneyAmount - (unit2.Cost - listOfUnit.Cost) > 0 && TurnManager.currPlayer.GetAvailableManpower() - (unit2.manpowerCost - listOfUnit.manpowerCost) > 0)
				{
					listOfUnit.Name = unit2.Name;
					listOfUnit.Type = unit2.Type;
					listOfUnit.BaseType = (string.IsNullOrEmpty(unit2.BaseType) ? unit2.Type : unit2.BaseType);
					listOfUnit.MaxMP = unit2.MaxMP;
					listOfUnit.CurrMP = unit2.MaxMP;
					listOfUnit.BaseMaxMP = unit2.MaxMP;
					if (listOfUnit.CurrHP >= listOfUnit.MaxHP)
					{
						listOfUnit.CurrHP = unit2.MaxHP;
					}
					listOfUnit.MaxHP = unit2.MaxHP;
					listOfUnit.BaseMaxHP = unit2.MaxHP;
					if (listOfUnit.CurrAmmo >= listOfUnit.MaxAmmo)
					{
						listOfUnit.CurrAmmo = unit2.MaxAmmo;
					}
					listOfUnit.MaxAmmo = unit2.MaxAmmo;
					listOfUnit.BaseMaxAmmo = unit2.BaseMaxAmmo;
					if (listOfUnit.CurrAutonomy >= listOfUnit.MaxAutonomy)
					{
						listOfUnit.CurrAutonomy = unit2.MaxAutonomy;
					}
					listOfUnit.MaxAutonomy = unit2.MaxAutonomy;
					listOfUnit.BaseMaxAutonomy = unit2.MaxAutonomy;
					listOfUnit.SoftDamage = unit2.SoftDamage;
					listOfUnit.BaseSoftDamage = unit2.SoftDamage;
					listOfUnit.HardDamage = unit2.HardDamage;
					listOfUnit.BaseHardDamage = unit2.HardDamage;
					listOfUnit.AirDamage = unit2.AirDamage;
					listOfUnit.BaseAirDamage = unit2.AirDamage;
					int num2 = unit2.Cost - listOfUnit.Cost;
					if (num2 > 0)
					{
						TurnManager.currPlayer.RemoveMoney(num2, p_sendRPC: false);
					}
					else if (num2 < 0)
					{
						TurnManager.currPlayer.AddMoney(-num2, p_sendRPC: false);
					}
					listOfUnit.Cost = unit2.Cost;
					TurnManager.currPlayer.AddManpower(listOfUnit.manpowerCost - unit2.manpowerCost, p_doMarginCheck: false, p_sendRPC: false);
					listOfUnit.manpowerCost = unit2.manpowerCost;
					if (listOfUnit.CurrHP > listOfUnit.MaxHP)
					{
						listOfUnit.CurrHP = listOfUnit.MaxHP;
					}
					if (listOfUnit.CurrAutonomy > listOfUnit.MaxAutonomy)
					{
						listOfUnit.CurrAutonomy = listOfUnit.MaxAutonomy;
					}
					if (listOfUnit.CurrMP > listOfUnit.MaxMP)
					{
						listOfUnit.CurrMP = listOfUnit.MaxMP;
					}
					if (listOfUnit.CurrAmmo > listOfUnit.MaxAmmo)
					{
						listOfUnit.CurrAmmo = listOfUnit.MaxAmmo;
					}
					listOfUnit.IsPoliticalUnit = false;
					listOfUnit.IsEliteUnit = false;
					listOfUnit.IsCavalry = false;
					listOfUnit.IsEngineers = unit2.IsEngineers;
					listOfUnit.IsRecon = unit2.IsRecon;
					flag3 = true;
				}
			}
			else if (unit != null)
			{
				listOfUnit.Name = unit.Name;
				listOfUnit.Type = unit.Type;
				listOfUnit.BaseType = (string.IsNullOrEmpty(unit.BaseType) ? unit.Type : unit.BaseType);
				listOfUnit.MaxMP = unit.MaxMP;
				listOfUnit.CurrMP = unit.MaxMP;
				listOfUnit.BaseMaxMP = unit.MaxMP;
				if (listOfUnit.CurrHP >= listOfUnit.MaxHP)
				{
					listOfUnit.CurrHP = unit.MaxHP;
				}
				listOfUnit.MaxHP = unit.MaxHP;
				listOfUnit.BaseMaxHP = unit.MaxHP;
				if (listOfUnit.CurrAmmo >= listOfUnit.MaxAmmo)
				{
					listOfUnit.CurrAmmo = unit.MaxAmmo;
				}
				listOfUnit.MaxAmmo = unit.MaxAmmo;
				listOfUnit.BaseMaxAmmo = unit.BaseMaxAmmo;
				if (listOfUnit.CurrAutonomy >= listOfUnit.MaxAutonomy)
				{
					listOfUnit.CurrAutonomy = unit.MaxAutonomy;
				}
				listOfUnit.MaxAutonomy = unit.MaxAutonomy;
				listOfUnit.BaseMaxAutonomy = unit.MaxAutonomy;
				listOfUnit.SoftDamage = unit.SoftDamage;
				listOfUnit.BaseSoftDamage = unit.SoftDamage;
				listOfUnit.HardDamage = unit.HardDamage;
				listOfUnit.BaseHardDamage = unit.HardDamage;
				listOfUnit.AirDamage = unit.AirDamage;
				listOfUnit.BaseAirDamage = unit.AirDamage;
				int num3 = unit.Cost - listOfUnit.Cost;
				if (num3 > 0)
				{
					TurnManager.currPlayer.RemoveMoney(num3, p_sendRPC: false);
				}
				else if (num3 < 0)
				{
					TurnManager.currPlayer.AddMoney(-num3, p_sendRPC: false);
				}
				listOfUnit.Cost = unit.Cost;
				TurnManager.currPlayer.AddManpower(listOfUnit.manpowerCost - unit.manpowerCost, p_doMarginCheck: false, p_sendRPC: false);
				listOfUnit.manpowerCost = unit.manpowerCost;
				if (listOfUnit.CurrHP > listOfUnit.MaxHP)
				{
					listOfUnit.CurrHP = listOfUnit.MaxHP;
				}
				if (listOfUnit.CurrAutonomy > listOfUnit.MaxAutonomy)
				{
					listOfUnit.CurrAutonomy = listOfUnit.MaxAutonomy;
				}
				if (listOfUnit.CurrMP > listOfUnit.MaxMP)
				{
					listOfUnit.CurrMP = listOfUnit.MaxMP;
				}
				if (listOfUnit.CurrAmmo > listOfUnit.MaxAmmo)
				{
					listOfUnit.CurrAmmo = listOfUnit.MaxAmmo;
				}
				listOfUnit.IsPoliticalUnit = false;
				listOfUnit.IsEliteUnit = false;
				listOfUnit.IsCavalry = false;
				listOfUnit.IsEngineers = unit.IsEngineers;
				listOfUnit.IsRecon = unit.IsRecon;
				flag3 = true;
			}
			if (flag3)
			{
				listOfUnit.CacheUnitNameToLower();
				listOfUnit.CalculateAndSetManpowerCost();
				listOfUnit.CalculateAndSetUnitPrice();
				listOfUnit.unitGO.SetSprite();
				TurnManager.currPlayer.RemoveUnitCategoryID(listOfUnit);
				TurnManager.currPlayer.AssignUnitCategoryID(listOfUnit);
				listOfUnit.SetUnitCustomName();
				MultiplayerManager.RPC("RPC_SyncUnit", listOfUnit, true);
			}
		}
	}

	private int GetSupportiveFireDamage(UnitGO p_attackingUnitGO, UnitGO p_defendingUnitGO)
	{
		if (p_attackingUnitGO.unit.FilterType == "FilterBoat")
		{
			return 0;
		}
		if (p_attackingUnitGO.unit.FilterType == "FilterArtillery")
		{
			return 0;
		}
		int num = 0;
		bool flag = p_attackingUnitGO.unit.Type == "Tank";
		bool flag2 = p_attackingUnitGO.unit.Type == "Plane";
		foreach (Tile neighbour in p_defendingUnitGO.tileGO.tile.neighbours)
		{
			if (flag2 && neighbour.tileGO.airUnitGO != null && neighbour.tileGO.airUnitGO.unit.CurrAmmo > 0 && neighbour.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && neighbour.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && p_defendingUnitGO.unit.FilterType != "FilterFighter")
			{
				num += neighbour.tileGO.airUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
			}
			if (!(neighbour.tileGO.groundUnitGO != null) || neighbour.tileGO.groundUnitGO.unit.CurrAmmo <= 0 || !neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				continue;
			}
			if (flag2)
			{
				if (neighbour.tileGO.groundUnitGO.unit.FilterType == "FilterAA" || (neighbour.tileGO.groundUnitGO.unit.FilterType == "FilterBoat" && (neighbour.tileGO.groundUnitGO.unit.IsDestroyer || neighbour.tileGO.groundUnitGO.unit.GetNameToLower().Contains("cruiser"))))
				{
					num += neighbour.tileGO.groundUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
				}
			}
			else if (neighbour.tileGO.groundUnitGO.unit.FilterType == "FilterArtillery")
			{
				num += neighbour.tileGO.groundUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
			}
			else if (flag && neighbour.tileGO.groundUnitGO.unit.FilterType == "FilterAntitank")
			{
				num += neighbour.tileGO.groundUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
			}
		}
		return num;
	}

	private bool IsPlaneEscorted(Tile p_tile)
	{
		if (p_tile.AirUnit == null)
		{
			return false;
		}
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (neighbour.AirUnit != null && neighbour.AirUnit.FilterType == "FilterFighter" && !neighbour.AirUnit.IsRecon && p_tile.AirUnit.OwnerPlayer.IsAlliedWith(neighbour.AirUnit.OwnerPlayer))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanPlaneAttackSafely(UnitGO p_unitGO, Tile p_tile, int p_ownDamageDealt)
	{
		if (p_unitGO.unit.IsMissile)
		{
			return true;
		}
		int num = 0;
		int num2 = 0;
		if (p_tile.isVisible)
		{
			if ((bool)p_tile.tileGO.groundUnitGO && p_tile.tileGO.groundUnitGO.unit.CurrAmmo > 0 && (p_tile.tileGO.groundUnitGO.unit.FilterType == "FilterAA" || p_tile.tileGO.groundUnitGO.unit.FilterType == "FilterBoat") && p_tile.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				num += Mathf.RoundToInt((float)p_tile.tileGO.groundUnitGO.unit.AirDamage * p_tile.tileGO.groundUnitGO.unit.GetHPPercent(p_forDamagePurposes: true));
			}
			if (!TurnManager.isBadWeather && (bool)p_tile.tileGO.airUnitGO && p_tile.tileGO.airUnitGO.unit.CurrAmmo > 0 && p_tile.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && p_tile.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				num += Mathf.RoundToInt((float)p_tile.tileGO.airUnitGO.unit.AirDamage * p_tile.tileGO.airUnitGO.unit.GetHPPercent(p_forDamagePurposes: true));
			}
		}
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (!TurnManager.isBadWeather && neighbour.isVisible)
			{
				if ((bool)neighbour.tileGO.groundUnitGO && !PlayerSettings.Instance.NoAAsupportDuringAirFight && neighbour.tileGO.groundUnitGO.unit.CurrAmmo > 0 && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && (neighbour.tileGO.groundUnitGO.unit.FilterType == "FilterAA" || neighbour.tileGO.groundUnitGO.unit.IsDestroyer || neighbour.tileGO.groundUnitGO.unit.GetNameToLower().Contains("cruiser")))
				{
					num2 += Mathf.RoundToInt((float)neighbour.tileGO.groundUnitGO.unit.AirDamage * neighbour.tileGO.groundUnitGO.unit.GetHPPercent(p_forDamagePurposes: true));
				}
				if ((bool)neighbour.tileGO.airUnitGO && neighbour.tileGO.airUnitGO.unit.CurrAmmo > 0 && neighbour.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer && neighbour.tileGO.airUnitGO.unit.FilterType == "FilterFighter")
				{
					num2 += Mathf.RoundToInt((float)neighbour.tileGO.airUnitGO.unit.AirDamage * neighbour.tileGO.airUnitGO.unit.GetHPPercent(p_forDamagePurposes: true));
				}
			}
		}
		num += num2;
		if (num <= 0)
		{
			return true;
		}
		if (num >= p_unitGO.unit.CurrHP)
		{
			return false;
		}
		float num3 = 0.5f;
		if (num > 0)
		{
			num3 = ((num2 <= 0) ? 0.3f : 0.2f);
		}
		if (_areWeWinning)
		{
			num3 += 0.15f;
		}
		if (p_tile.IsVictoryPoint)
		{
			num3 += 0.15f;
		}
		float num4 = (float)p_ownDamageDealt / (float)num;
		float num5 = (float)num / (float)p_unitGO.unit.CurrHP;
		return num4 * num3 - num5 * (1f - num3) >= 0f;
	}

	private bool AreThereEnemiesAround(TileGO p_tileGO, int p_radius, bool p_includeAir, bool p_useFogOfWar)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tileGO.tile, p_radius))
		{
			if (!p_useFogOfWar || item.isVisible)
			{
				if (item.tileGO.groundUnitGO != null && (!item.tileGO.groundUnitGO.unit.IsMine || item.tileGO.groundUnitGO.unit.MaxMP > 0) && item.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					return true;
				}
				if (p_includeAir && item.tileGO.airUnitGO != null && item.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsThereSimilarUnitAround(Tile p_tile, int p_radius, Unit p_unit)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.isVisible && !(item.tileGO.groundUnitGO == null) && item.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer && !(item.tileGO.groundUnitGO.unit.FilterType != p_unit.FilterType))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsThereSpecificEnemyUnitAround(Tile p_tile, int p_radius, string p_type, int p_damageValue = -1)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (!item.isVisible)
			{
				continue;
			}
			if (item.tileGO.groundUnitGO != null && item.tileGO.groundUnitGO.unit.Type == p_type && item.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				if (p_damageValue == -1)
				{
					return true;
				}
				return p_damageValue - item.tileGO.groundUnitGO.unit.GetArmour() > 0;
			}
			if (item.tileGO.airUnitGO != null && item.tileGO.airUnitGO.unit.Type == p_type && item.tileGO.airUnitGO.unit.OwnerPlayer.isAtWarWithCurrentPlayer)
			{
				if (p_damageValue == -1)
				{
					return true;
				}
				return p_damageValue - item.tileGO.airUnitGO.unit.GetArmour() > 0;
			}
		}
		return false;
	}

	private bool ShouldTakeDefensivePosition(UnitGO p_unitGO, out TileGO o_tileGO)
	{
		o_tileGO = null;
		if (_areWeWinning)
		{
			return false;
		}
		if (p_unitGO.unit.Type == "Boat")
		{
			return false;
		}
		if (p_unitGO.unit.Type == "Plane")
		{
			return false;
		}
		if (p_unitGO.unit.Type == "Artillery" && p_unitGO.unit.FilterType == "FilterArtillery")
		{
			return false;
		}
		if (p_unitGO.listOfpotentialTiles.Count == 0)
		{
			return false;
		}
		Tile tile = p_unitGO.tileGO.tile;
		_ = tile.tileGO.transform.position;
		List<Tile> allTilesInRadiusAround = Utils.GetAllTilesInRadiusAround(tile, 3);
		int num = 0;
		int num2 = 0;
		foreach (Tile item in allTilesInRadiusAround)
		{
			if (!item.isVisible)
			{
				continue;
			}
			if (item.GroundUnit != null && !item.GroundUnit.IsMine && !item.GroundUnit.IsCommander && (item.GroundUnit.SoftDamage > 0 || item.GroundUnit.HardDamage > 0 || item.GroundUnit.AirDamage > 0))
			{
				if (item.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
				{
					num++;
				}
				if (!item.IsVictoryPoint && item.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					num2++;
				}
			}
			if (item.AirUnit != null && !item.AirUnit.IsMissile && !item.AirUnit.IsCommander && (item.AirUnit.SoftDamage > 0 || item.AirUnit.HardDamage > 0 || item.AirUnit.AirDamage > 0))
			{
				if (item.AirUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
				{
					num++;
				}
				if (item.AirUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					num2++;
				}
			}
		}
		if (num >= num2)
		{
			return false;
		}
		allTilesInRadiusAround = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 3);
		List<RetreatTile> list = new List<RetreatTile>();
		RetreatTile retreatTile = null;
		foreach (TileGO item2 in new List<TileGO>(p_unitGO.listOfpotentialTiles) { p_unitGO.tileGO })
		{
			RetreatTile retreatTile2 = new RetreatTile
			{
				tile = item2.tile,
				terrainModifier = item2.tile.DamageModificator
			};
			foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(item2.tile, 1))
			{
				if (item3.GroundUnit == null || item3.GroundUnit.IsMine || item3.GroundUnit.FilterType == "FilterBoat" || (item3.GroundUnit.MaxMP == 0 && item3.GroundUnit.SoftDamage == 0 && item3.GroundUnit.HardDamage == 0 && item3.GroundUnit.AirDamage == 0))
				{
					continue;
				}
				if (item3.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer)
				{
					if (!item3.GroundUnit.IsCommander && item3.GroundUnit != p_unitGO.unit)
					{
						retreatTile2.alliesAround++;
					}
				}
				else if (item3.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer)
				{
					retreatTile2.enemiesAround++;
				}
			}
			int num3 = 0;
			Vector3 b = default;
			foreach (Tile item4 in allTilesInRadiusAround)
			{
				if (item4.isVisible && item4.GroundUnit != null && !item4.GroundUnit.IsCommander && !item4.GroundUnit.IsMine && !(item4.GroundUnit.FilterType == "FilterBoat") && item4.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && (item4.GroundUnit.MaxMP != 0 || item4.GroundUnit.SoftDamage != 0 || item4.GroundUnit.HardDamage != 0 || item4.GroundUnit.AirDamage != 0))
				{
					b += item4.tileGO.transform.position;
					num3++;
				}
			}
			b /= (float)num3;
			retreatTile2.distanceToAverageEnemiesPosition = Vector3.Distance(item2.transform.position, b);
			if (item2 == p_unitGO.tileGO)
			{
				retreatTile = retreatTile2;
			}
			list.Add(retreatTile2);
		}
		if (list.Count == 0)
		{
			return false;
		}
		bool flag = p_unitGO.unit.Type != "Tank";
		list = ((!flag) ? (from x in list
			orderby x.terrainModifier, x.distanceToAverageEnemiesPosition
			select x).ToList() : (from x in list
			orderby x.terrainModifier descending, x.distanceToAverageEnemiesPosition
			select x).ToList());
		for (int num4 = 1; num4 <= 6; num4++)
		{
			foreach (RetreatTile item5 in list)
			{
				if (retreatTile != null && flag && item5.terrainModifier == 0 && p_unitGO.tileGO.tile.DamageModificator > 0 && (retreatTile.enemiesAround > 0 || item5.alliesAround == 0 || item5.enemiesAround == 0))
				{
					continue;
				}
				switch (num4)
				{
				case 1:
					if (item5.alliesAround + 1 != item5.enemiesAround)
					{
						break;
					}
					o_tileGO = item5.tile.tileGO;
					goto end_IL_0687;
				case 2:
					if (item5.alliesAround * 2 + 1 != item5.enemiesAround)
					{
						break;
					}
					o_tileGO = item5.tile.tileGO;
					goto end_IL_0687;
				case 5:
					if (item5.enemiesAround > 0 && item5.alliesAround == item5.enemiesAround)
					{
						o_tileGO = item5.tile.tileGO;
						goto end_IL_0687;
					}
					break;
				default:
					_ = 6;
					break;
				case 3:
				case 4:
					break;
				}
				continue;
				end_IL_0687:
				break;
			}
			if (o_tileGO != null)
			{
				break;
			}
		}
		if (o_tileGO == null)
		{
			o_tileGO = list.ElementAt(0).tile.tileGO;
		}
		if (o_tileGO == p_unitGO.tileGO)
		{
			o_tileGO = null;
		}
		return true;
	}

	private List<Unit> SortUnitsForBestStrategicPlayOrder(List<Unit> p_units)
	{
		Dictionary<Unit, float> dictionary = new Dictionary<Unit, float>();
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		foreach (Unit p_unit in p_units)
		{
			num2 = float.MaxValue;
			bool flag = p_unit.FilterType == "FilterBoat";
			if (!p_unit.IsReserve && (bool)p_unit.unitGO)
			{
				foreach (TileGO item in _enemyVPsGO)
				{
					if (flag || item.tile.enumType != TileTypes.SEA)
					{
						num = Vector2.Distance(item.transform.position, p_unit.unitGO.tileGO.transform.position);
						if (num < num2)
						{
							num2 = num;
						}
						if (num2 < 6f)
						{
							break;
						}
					}
				}
				if (p_unit.unitGO.tileGO.tile.enumType == TileTypes.HARBOUR)
				{
					num2 = 0f;
				}
				else if (ShouldLeaveTileForSomeoneElse(p_unit.unitGO, p_unit.unitGO.tileGO, RunTimes.FIRST))
				{
					num2 = 5f;
				}
			}
			if (!dictionary.ContainsKey(p_unit))
			{
				dictionary.Add(p_unit, num2);
			}
		}
		return (from x in dictionary
			orderby x.Value, x.Key.GetHPPercent(p_forDamagePurposes: false)
			select x).ToDictionary((KeyValuePair<Unit, float> x) => x.Key, (KeyValuePair<Unit, float> x) => x.Value).Keys.ToList();
	}

	private bool ShouldLeaveTileForSomeoneElse(UnitGO p_unitGO, TileGO p_tileGO, RunTimes p_runTime)
	{
		if (p_unitGO.unit.IsDefensive)
		{
			return false;
		}
		if (p_unitGO.unit.HasMoved)
		{
			return false;
		}
		if (p_unitGO.unit.Range > 1)
		{
			return false;
		}
		if (p_unitGO.unit.MaxMP == 0)
		{
			return false;
		}
		if (p_unitGO.unit.Type == "Plane")
		{
			return false;
		}
		if (p_unitGO.unit.FilterType == "FilterBoat")
		{
			return false;
		}
		float num = 0f;
		List<Unit> list = new List<Unit>();
		foreach (Tile neighbour in p_tileGO.tile.neighbours)
		{
			if (neighbour.GroundUnit != null && neighbour.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && !(neighbour.GroundUnit.Type == "Boat") && !neighbour.GroundUnit.IsMine)
			{
				list.Add(neighbour.GroundUnit);
				p_unitGO.unit.GetPotentialDamage(neighbour.GroundUnit.unitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				float num2 = (float)p_unitGO.unit.FinalDamage / (float)neighbour.GroundUnit.CurrHP;
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		if (num >= 1f)
		{
			return false;
		}
		if (list.Count == 0)
		{
			return false;
		}
		bool flag = false;
		float num3 = 0f;
		List<Tile> allTilesInRadiusAround = Utils.GetAllTilesInRadiusAround(p_tileGO.tile, 3);
		p_tileGO.groundUnitGO = null;
		p_tileGO.tile.GroundUnit = null;
		Unit unit = null;
		foreach (Tile item in allTilesInRadiusAround)
		{
			if (item.tileGO == p_tileGO || item.IsVictoryPoint || item.GroundUnit == null || !item.GroundUnit.OwnerPlayer.isAlliedWithCurrentPlayer || !item.GroundUnit.OwnerPlayer.IsComputer || (p_runTime == RunTimes.FIRST && item.GroundUnit.willReplaceUnitMovingOut1) || (p_runTime == RunTimes.SECOND && item.GroundUnit.willReplaceUnitMovingOut2) || item.GroundUnit.IsDefensive || item.GroundUnit.HasMoved || item.GroundUnit.HasAttacked || item.GroundUnit.IsCommander || item.GroundUnit.MaxMP == 0 || item.GroundUnit.Range > 1 || item.GroundUnit.GetHPPercent(p_forDamagePurposes: false) < 0.6f || item.GroundUnit.Type == "Boat" || item.GroundUnit.FilterType == "FilterAA")
			{
				continue;
			}
			bool flag2 = false;
			foreach (Tile neighbour2 in item.neighbours)
			{
				if (neighbour2.GroundUnit != null && neighbour2.GroundUnit.OwnerPlayer.isAtWarWithCurrentPlayer && !neighbour2.GroundUnit.IsMine && !(neighbour2.GroundUnit.Type == "Boat"))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				continue;
			}
			TileGO tileGO = item.GroundUnit.unitGO.tileGO;
			foreach (Unit item2 in list)
			{
				p_tileGO.groundUnitGO = item.GroundUnit.unitGO;
				p_tileGO.tile.GroundUnit = item.GroundUnit;
				item.GroundUnit.unitGO.tileGO = p_tileGO;
				item.GroundUnit.GetPotentialDamage(item2.unitGO, p_isRetaliation: false, p_isSupportiveFire: false);
				item.GroundUnit.unitGO.tileGO = tileGO;
				float num2 = (float)item.GroundUnit.FinalDamage / (float)item2.CurrHP;
				if (!(num2 >= num))
				{
					continue;
				}
				p_tileGO.groundUnitGO = null;
				p_tileGO.tile.GroundUnit = null;
				item.GroundUnit.unitGO.ShowPotentialPaths();
				if (!item.GroundUnit.unitGO.listOfpotentialTiles.Contains(p_tileGO))
				{
					continue;
				}
				flag = true;
				if (!(num2 > num3))
				{
					continue;
				}
				if (p_runTime == RunTimes.SECOND && p_tileGO.tile.neighbours.Contains(item))
				{
					if (unit != null)
					{
						unit.shouldPerformDirectSwap = false;
						unit.swapeeUnitID = 0;
					}
					item.GroundUnit.shouldPerformDirectSwap = true;
					item.GroundUnit.swapeeUnitID = p_unitGO.unit.ID;
					p_unitGO.unit.shouldPerformDirectSwap = true;
					p_unitGO.unit.swapeeUnitID = item.GroundUnit.ID;
				}
				num3 = num2;
				unit = item.GroundUnit;
			}
		}
		p_tileGO.groundUnitGO = p_unitGO;
		p_tileGO.tile.GroundUnit = p_unitGO.unit;
		if (flag && unit != null)
		{
			if (p_runTime == RunTimes.FIRST)
			{
				unit.willReplaceUnitMovingOut1 = true;
			}
			else
			{
				if (unit.shouldPerformDirectSwap)
				{
					p_unitGO.unit.shouldPerformDirectSwap = true;
				}
				unit.willReplaceUnitMovingOut2 = true;
			}
		}
		return flag;
	}

	private bool ShouldUnitLeaveDefensiveMode(UnitGO p_unitGO)
	{
		if (!p_unitGO.unit.IsDefensive)
		{
			return true;
		}
		if (p_unitGO.tileGO.tile.enumType == TileTypes.HARBOUR)
		{
			return true;
		}
		if (p_unitGO.tileGO.tile.IsVictoryPoint && !p_unitGO.tileGO.tile.ownerPlayer.IsComputer)
		{
			return true;
		}
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, 3))
		{
			if (item.terrainOwner != null && item.terrainOwner.isAtWarWithCurrentPlayer)
			{
				return true;
			}
		}
		return false;
	}

	private void TransferVPs()
	{
		if (!_isHistoricalFocusON || (_currPlayerCountry != "Slovakia" && _currPlayerCountry != "Hungary" && _currPlayerCountry != "Italy" && _currPlayerCountry != "Romania" && _currPlayerCountry != "Croatia" && _currPlayerCountry != "Bulgaria") || !GameData.Instance.TryFindPlayerByCountry("Nazi Germany", out var o_player) || !o_player.isAlliedWithCurrentPlayer)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>
		{
			"Soviet Union", "Poland", "Belgium", "Netherlands", "Luxembourg", "Norway", "Denmark", "France", "Estonia", "Latvia",
			"Lithuania", "Great Britain"
		};
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.ownerPlayer != TurnManager.currPlayer || (_currPlayerCountry == "Croatia" && victoryPoint.tile.OriginalCountryOwner == "Yugoslavia") || (_currPlayerCountry == "Italy" && (victoryPoint.tile.OriginalCountryOwner == "France" || victoryPoint.tile.OriginalCountryOwner == "Great Britain")))
			{
				continue;
			}
			if (_currPlayerCountry == "Romania" && victoryPoint.tile.OriginalCountryOwner == "Soviet Union")
			{
				int num = int.MaxValue;
				foreach (TileGO victoryPoint2 in MapGO.victoryPoints)
				{
					if (victoryPoint2.tile.OriginalCountryOwner == "Romania")
					{
						num = Mathf.Min(num, Utils.GetHeuristicDistance(victoryPoint.tile, victoryPoint2.tile));
						if (num <= 5)
						{
							break;
						}
					}
				}
				if ((float)num <= 5f)
				{
					continue;
				}
			}
			if (hashSet.Contains(victoryPoint.tile.OriginalCountryOwner))
			{
				TilesManagerGO.instance.TransferTile(victoryPoint, o_player);
			}
			else if (victoryPoint.tile.OriginalCountryOwner == "Yugoslavia" && _currPlayerCountry != "Italy")
			{
				TilesManagerGO.instance.TransferTile(victoryPoint, o_player);
			}
			else if (victoryPoint.tile.OriginalCountryOwner == "Greece" && _currPlayerCountry != "Italy")
			{
				TilesManagerGO.instance.TransferTile(victoryPoint, o_player);
			}
		}
	}

	private float GetUpkeepToBaseIncomeRatio(Player p_player)
	{
		if (p_player.baseIncome == 0)
		{
			return (float)_currentUpkeepCost / (float)_currentUpkeepCost;
		}
		return (float)_currentUpkeepCost / (float)p_player.baseIncome;
	}

	private bool ShouldDisbandUnit(Unit p_unit)
	{
		if (p_unit.Lvl >= 3)
		{
			return false;
		}
		if (p_unit.Hero != null)
		{
			return false;
		}
		if (TurnManager.currPlayer.MoneyAmount < TurnManager.currPlayer.turnIncome)
		{
			return true;
		}
		if (_upkeepSettingON && GetUpkeepToBaseIncomeRatio(TurnManager.currPlayer) > GetUpkeepThresholdForAcceptingNewUnits(TurnManager.currPlayer))
		{
			return true;
		}
		return false;
	}

	private void DisbandUnit(Unit p_unit)
	{
		p_unit.CurrHP = 0;
		p_unit.IsDead = true;
		p_unit.IsReserve = false;
		TurnManager.currPlayer.AddManpower(p_unit.manpowerCost, p_doMarginCheck: false, p_sendRPC: false);
		TurnManager.currPlayer.AddMoney(p_unit.Cost, p_sendRPC: true);
	}

	private TileGO GetFurthestWalkableTileFromEnemy(UnitGO p_unitGO, int p_checkRadius)
	{
		Vector2[] array = Utils.GetAllTilesInRadiusAround(p_unitGO.tileGO.tile, p_checkRadius).Where(IsEnemyOnTile).Select((Func<Tile, Vector2>)((Tile t) => t.tileGO.transform.position))
			.ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		for (int num = 0; num < 2; num++)
		{
			float num2 = -1f;
			TileGO tileGO = null;
			foreach (TileGO listOfpotentialTile in p_unitGO.listOfpotentialTiles)
			{
				if (!IsTileFreeFor(p_unitGO, listOfpotentialTile) || (num == 0 && AreThereEnemiesAround(listOfpotentialTile, 1, p_includeAir: false, p_useFogOfWar: false)))
				{
					continue;
				}
				Vector2 vector = listOfpotentialTile.transform.position;
				float num3 = float.PositiveInfinity;
				Vector2[] array2 = array;
				for (int num4 = 0; num4 < array2.Length; num4++)
				{
					float sqrMagnitude = (array2[num4] - vector).sqrMagnitude;
					if (sqrMagnitude < num3)
					{
						num3 = sqrMagnitude;
					}
				}
				if (num3 > num2)
				{
					num2 = num3;
					tileGO = listOfpotentialTile;
				}
			}
			if (tileGO != null)
			{
				return tileGO;
			}
		}
		return null;
	}

	private bool IsEnemyOnTile(Tile p_tile)
	{
		UnitGO groundUnitGO = p_tile.tileGO.groundUnitGO;
		bool num = (object)groundUnitGO != null && groundUnitGO.unit.OwnerPlayer?.isAtWarWithCurrentPlayer == true;
		UnitGO airUnitGO = p_tile.tileGO.airUnitGO;
		bool flag = (object)airUnitGO != null && airUnitGO.unit.OwnerPlayer?.isAtWarWithCurrentPlayer == true;
		return num | flag;
	}

	private bool IsTileFreeFor(UnitGO p_unitGO, TileGO p_tileGO)
	{
		if (p_unitGO.unit.Type == "Plane")
		{
			return p_tileGO.airUnitGO == null;
		}
		return p_tileGO.groundUnitGO == null;
	}

	private float GetAttackRiskRewardRatio(UnitGO p_attackingUnitGO, UnitGO p_enemyUnitGO)
	{
		Tile tile = p_enemyUnitGO.tileGO.tile;
		float num = 0.6f;
		num += Mathf.Clamp((_warIncomeRatio - 1f) * 0.5f, -0.2f, 0.2f);
		num = Mathf.Clamp01(num);
		if (p_attackingUnitGO.unit.IsOnBoat)
		{
			num += 0.25f;
		}
		if (tile.IsVictoryPoint)
		{
			num += 0.15f;
			if (_areWeWinning || tile.OriginalCountryOwner == _currPlayerCountry)
			{
				num += 0.15f;
			}
		}
		if (tile.Bridge)
		{
			num += 0.1f;
			int num2 = 0;
			foreach (Tile neighbour in p_attackingUnitGO.tileGO.tile.neighbours)
			{
				if (neighbour.GroundUnit != null && neighbour.GroundUnit.MaxMP > 0 && p_attackingUnitGO.unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
				{
					num2++;
				}
			}
			num += 0.05f * (float)num2 * (_areWeWinning ? 2f : 1f);
		}
		int num3 = 0;
		foreach (Tile neighbour2 in tile.neighbours)
		{
			if (neighbour2.GroundUnit != null && neighbour2.GroundUnit.MaxMP > 0 && neighbour2.GroundUnit != p_attackingUnitGO.unit && p_attackingUnitGO.unit.OwnerPlayer.IsAlliedWith(neighbour2.GroundUnit.OwnerPlayer))
			{
				num3++;
			}
		}
		num += 0.025f * (float)num3 * (_areWeWinning ? 2f : 1f);
		if (p_attackingUnitGO.tileGO.tile.IsVictoryPoint || p_attackingUnitGO.tileGO.tile.enumType == TileTypes.TRENCH)
		{
			if (p_attackingUnitGO.unit.Type == "Plane")
			{
				if (p_attackingUnitGO.unit.FilterType == "FilterBomber" && p_attackingUnitGO.tileGO.tile.NumberOfTurnsAtLowProd > 0)
				{
					num = 0.25f;
				}
			}
			else
			{
				num -= 0.1f;
				if (p_attackingUnitGO.tileGO.tile.PrimaryObjective)
				{
					num -= 0.1f;
					if (p_attackingUnitGO.tileGO.tile.OriginalCountryOwner == _currPlayerCountry)
					{
						num -= 0.2f;
					}
				}
			}
		}
		int potentialDamage = p_attackingUnitGO.unit.GetPotentialDamage(p_enemyUnitGO, p_isRetaliation: false, p_isSupportiveFire: false);
		int potentialDamage2 = p_enemyUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: true, p_isSupportiveFire: false);
		int supportiveFireDamage = GetSupportiveFireDamage(p_attackingUnitGO, p_enemyUnitGO);
		float num4 = (float)potentialDamage / (float)p_enemyUnitGO.unit.CurrHP;
		float num5 = ((float)potentialDamage2 + (float)supportiveFireDamage) / (float)p_attackingUnitGO.unit.CurrHP;
		if (num5 >= 1f && p_attackingUnitGO.tileGO.tile.IsVictoryPoint && p_attackingUnitGO.unit.Type != "Plane")
		{
			return -1f;
		}
		float num6 = (float)p_attackingUnitGO.unit.Cost * num5;
		float num7 = (float)p_enemyUnitGO.unit.Cost * num4;
		if (tile.IsVictoryPoint && tile.NumberOfTurnsAtLowProd <= 0 && !p_attackingUnitGO.unit.IsRecon && p_attackingUnitGO.unit.FilterType == "FilterBomber")
		{
			num7 += (float)tile.MoneyProductionPerTurn;
		}
		float num8 = num4 * num - num5 * (1f - num);
		float num9 = Mathf.Max(1f, (float)p_attackingUnitGO.unit.Cost + (float)p_enemyUnitGO.unit.Cost);
		float num10 = (num7 - num6) / num9;
		float num11 = Mathf.Clamp01(1f - _warIncomeRatio);
		float num12 = 0.3f + 0.15f * (num11 - 0.5f);
		return (1f - num12) * num8 + num12 * num10;
	}

	private void OnDestroy()
	{
		PlayerSettings.Instance.IsQuickMovement = _backupQuickmovementRule;
	}
}
