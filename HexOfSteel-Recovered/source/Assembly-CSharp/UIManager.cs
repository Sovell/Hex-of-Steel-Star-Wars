using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager instance;

	public static bool isNATO;

	public static bool showAirUnit;

	public static bool isUIOpen;

	public static string nextMapChoice;

	public static float LeftClickTime;

	public static GameObject activeConfirmationWindow;

	[Header("Panels")]
	public Transform UI_parent;

	public Transform BOTTOM_parent;

	public GameObject diplomacy_panel;

	public GameObject store_Panel;

	public GameObject inGameMenu_Panel;

	public GameObject end_Panel;

	public GameObject saveLoad_Panel;

	public GameObject potentialDamagePanel;

	public GameObject techTree_panel;

	public GameObject reservePanel;

	public GameObject coreUnitsPanel;

	public GameObject OOB_panel;

	public GameObject Policies_panel;

	public GameObject entireUI;

	[Header("Canvases")]
	public Canvas mainCanvas;

	[Header("Time speed")]
	[SerializeField]
	private GameObject timeSpeedWindow;

	[SerializeField]
	private TextMeshProUGUI timeSpeed_text;

	[Header("Previous turn summary")]
	public GameObject previousTurnSummary_panel;

	public Transform previousTurnSummary_parent;

	[Header("Multiplayer")]
	public GameObject chat_panel;

	public GameObject chat_Notification;

	public GameObject message_prefab;

	public Transform chat_parent;

	public TMP_InputField chat_inputfield;

	[Header("Buildings")]
	public GameObject buildingIconPrefab;

	[Header("Start scenario panel")]
	public TextMeshProUGUI startResumeButton_text;

	public TextMeshProUGUI scenarioName_text;

	public TMP_InputField scenarioDescription_inputField;

	[Header("Stats")]
	public UnitStatsPanelUI stats_Panel;

	public GameObject heroStats_Panel;

	public TMP_InputField unitName_InputField;

	[Header("UI Top")]
	[SerializeField]
	private TextMeshProUGUI coreUnit_Text;

	[SerializeField]
	private TextMeshProUGUI coreUnit2_Text;

	public TextMeshProUGUI playerMoneyAmount_Text;

	public TextMeshProUGUI playerIncomePerTurn_text;

	public TextMeshProUGUI numberOfVP_Text;

	public TextMeshProUGUI manpower_Text;

	[SerializeField]
	private GameObject CoreUnitsBox_parent;

	public GameObject manpower_parent;

	public GameObject research_Notification;

	public GameObject manpower_tooltip;

	public GameObject money_tooltip;

	private List<GameObject> VPindicators;

	[Header("Objectives panel")]
	public GameObject objectives_Panel;

	public TextMeshProUGUI objectiveScenarioName_Text;

	public TextMeshProUGUI objectiveScenarioDescription_Text;

	[Header("Tile info")]
	public Image tileInfoOwnerCountry_Image;

	public Image tileInfoRepresentation_Image;

	public Image frozenTile_Image;

	public TextMeshProUGUI tileType_Text;

	public TextMeshProUGUI tileMPCost_Text;

	public TextMeshProUGUI tileDamageModifier_Text;

	public TextMeshProUGUI tileCoordinates_Text;

	public TextMeshProUGUI tileProduction_text;

	[Header("Weather")]
	public TextMeshProUGUI weather_Text;

	public Image weather_image;

	public Sprite clearWeather;

	public Sprite badWeather_rain;

	public Sprite badWeather_snow;

	public Sprite unpredictableWeather;

	[Header("UI right")]
	public Image twoUnitsIndicator_image;

	public Button upgrade_button;

	public Sprite twoUnitsIndicatorGroundGreen_sprite;

	public Sprite twoUnitsIndicatorAirGreen_sprite;

	[Header("UI left")]
	public Image sleeping_Button;

	public Button showDropZone_Button;

	public Button embarkDisembark_Button;

	public Button resupplyHP_Button;

	public Button resupplyFuel_Button;

	public Button resupplyAmmo_Button;

	public Button resupplyBoth_Button;

	public Button redeploy_Button;

	public Button blowRepairBridge_Button;

	public Button diveSurface_Button;

	public Button undo_Button;

	public GameObject leftUI_GO;

	public GameObject moreStats_Panel;

	public Sprite sleeping_SPrite;

	public Sprite skip_sprite;

	public Sprite awake_SPrite;

	public Sprite dive_sprite;

	public Sprite surface_sprite;

	[Header("UI Bottom")]
	public SelectedUnitUI selectedUnitUI;

	public HoverUnitUI hoverUnitUI;

	public SelectedUnitExtraStatsUI SelectedUnitExtraStatsUI;

	[Header("Sound")]
	public GameObject sound_Panel;

	public bool isSoundPanelOpen;

	[Header("End of scenario")]
	public TextMeshProUGUI victoryDefeat_Text;

	public TextMeshProUGUI endOfScenario_Text;

	public TextMeshProUGUI timePlayed_Text;

	public Button nextScenarioChoice1_Button;

	public Button nextScenarioChoice2_Button;

	public Button closeEndOfGameMenu_button;

	public GameObject mainMenu_Button;

	public GameObject continue_Button;

	public GameObject endOfScenarioPrefab;

	public Transform endOfScenarioParent;

	[Header("Others")]
	public Image currentPlayerFlag_image;

	public Image pause_image;

	public Button quitToMapEditorButton;

	public Button midGameStatisticsButton;

	public GameObject supplyLine;

	public GameObject unitMovePath;

	public GameObject highlightRingPrefab;

	public GameObject resupplyCost_Panel;

	public NewMinimap minimap;

	public Transform unitsParent;

	public Transform coreUnitsParent;

	public Transform waypointsParent;

	public GameObject coreUnitPrefab;

	public LastTurnSummaryUI lastTurnSummarySlot_prefab;

	private int _count;

	private bool _ascending;

	private bool _isShowingAllWaypoints;

	private bool _showBuildingsIcons;

	public GameObject tooltip;

	private bool _quittingToMainMenu;

	public static void HideToolTip()
	{
		if ((bool)GameObject.Find("Tooltip"))
		{
			GameObject.Find("Tooltip").SetActive(value: false);
		}
	}

	public static GameObject ShowConfirmationWindow(string p_text, Action p_action)
	{
		return ShowConfirmationWindow(p_text, null, (object _) =>
		{
			p_action?.Invoke();
		});
	}

	public static GameObject ShowConfirmationWindow<T>(string p_text, T p_payload, Action<T> p_onYes)
	{
		UnityEngine.Object.Destroy(activeConfirmationWindow);
		Transform parent = null;
		switch (SceneManager.GetActiveScene().buildIndex)
		{
		case 0:
			parent = GameObject.Find("UI").transform;
			break;
		case 1:
			parent = instance.UI_parent;
			break;
		case 2:
			parent = GameObject.Find("UI").transform;
			break;
		}
		activeConfirmationWindow = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Confirmation Window"), parent);
		RectTransform component = activeConfirmationWindow.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.one;
		component.offsetMax = Vector2.one;
		if (activeConfirmationWindow.TryGetComponent<ConfirmationWindowGO>(out var component2))
		{
			component2.description_text.text = p_text;
			component2.yes_button.onClick.AddListener(() =>
			{
				try
				{
					p_onYes?.Invoke(p_payload);
				}
				finally
				{
					CloseConfirmationWindow(activeConfirmationWindow);
				}
			});
			component2.no_button.onClick.AddListener(() =>
			{
				CloseConfirmationWindow(activeConfirmationWindow);
			});
		}
		return activeConfirmationWindow;
	}

	public static void CloseConfirmationWindow(GameObject p_confirmationWindow)
	{
		if (!(p_confirmationWindow == null))
		{
			SoundManager.instance.PlayUIClickSound();
			UnityEngine.Object.Destroy(p_confirmationWindow);
		}
	}

	private void Awake()
	{
		instance = this;
		showAirUnit = false;
		isSoundPanelOpen = false;
	}

	private void Start()
	{
		instance.RefreshCurrentPlayerFlag();
		if (PlayerSettings.Instance.Manpower)
		{
			CoreUnitsBox_parent.SetActive(value: false);
		}
		else
		{
			manpower_parent.SetActive(value: false);
			CoreUnitsBox_parent.SetActive(value: true);
			coreUnit_Text.gameObject.SetActive(value: false);
		}
		if (!GameData.Instance.map.IsCampaign)
		{
			CoreUnitsBox_parent.SetActive(value: false);
			coreUnit_Text.gameObject.SetActive(value: false);
			coreUnit2_Text.gameObject.SetActive(value: false);
		}
		if (TurnManager.currPlayer.IsComputer)
		{
			TurnManager.currPlayer.IsPlaying = true;
		}
		if (GameData.Instance.isPBEM)
		{
			try
			{
				if (!GameData.Instance.messages.ElementAt(GameData.Instance.messages.Count() - 1).Contains(PlayerSettings.Instance.Username))
				{
					chat_Notification.SetActive(value: true);
				}
			}
			catch
			{
			}
		}
		ShowPreviousTurnSummary(makeClickSound: false);
		RefreshCoreUnits();
		VPindicators = new List<GameObject>();
		moreStats_Panel.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
		sound_Panel.GetComponent<Animator>().keepAnimatorStateOnDisable = true;
		scenarioName_text.text = GameData.Instance.map.ScenarioName;
		string text = string.Empty;
		if (GameData.Instance.map.IsOfficial && PlayerSettings.Instance.Language != GameLanguages.EN)
		{
			string text2 = LocalizationManager.Translate("txt-warning scenario description") + "\n\n-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n\n";
			if (TurnManager.humanPlayer != null)
			{
				text = TurnManager.humanPlayer.ScenarioDescription;
			}
			else if (TurnManager.currPlayer != null)
			{
				text = TurnManager.currPlayer.ScenarioDescription;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = GameData.Instance.map.ScenarioDescription;
			}
			text = text2 + text;
		}
		else
		{
			if (TurnManager.humanPlayer != null)
			{
				text = TurnManager.humanPlayer.ScenarioDescription;
			}
			else if (TurnManager.currPlayer != null)
			{
				text = TurnManager.currPlayer.ScenarioDescription;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = GameData.Instance.map.ScenarioDescription;
			}
		}
		scenarioDescription_inputField.SetTextWithoutNotify(text);
		objectiveScenarioName_Text.text = GameData.Instance.map.ScenarioName;
		if (GameData.Instance.map.turnCount > 1)
		{
			startResumeButton_text.text = LocalizationManager.Translate("btn-resume");
		}
		else
		{
			startResumeButton_text.text = LocalizationManager.Translate("btn-start");
		}
		quitToMapEditorButton.gameObject.SetActive(GameData.Instance.isFromMapEditor);
	}

	public void RefreshCurrentPlayerFlag()
	{
		currentPlayerFlag_image.sprite = Utils.GetFlagSprite(TurnManager.currPlayer.Country);
	}

	public Sprite GetWeatherSprite(WeatherPredictionData p_weatherPredictionData)
	{
		if (p_weatherPredictionData.predictionAccuracy <= 0f)
		{
			return instance.unpredictableWeather;
		}
		if (p_weatherPredictionData.predictedWeather == WeatherType.BAD)
		{
			if (GameData.Instance.map.IsDateWinter(p_weatherPredictionData.date))
			{
				return instance.badWeather_snow;
			}
			return instance.badWeather_rain;
		}
		return instance.clearWeather;
	}

	public void ShowEventMessageWindow(string p_text)
	{
		if (!string.IsNullOrEmpty(p_text))
		{
			GameObject messageWindow = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Message window prefab"), UI_parent);
			messageWindow.GetComponentInChildren<TextMeshProUGUI>().text = p_text;
			messageWindow.transform.Find("close Button").GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				CloseConfirmationWindow(messageWindow);
			});
		}
	}

	public void ShowPreviousTurnSummary(bool makeClickSound)
	{
		if (GameData.Instance.map.turnCount == 1 && TurnManager.currPlayer.IsPlayingFirst)
		{
			return;
		}
		if (makeClickSound)
		{
			SoundManager.instance.PlayUIClickSound();
		}
		Transform[] componentsInChildren = previousTurnSummary_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != previousTurnSummary_parent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		if (previousTurnSummary_panel.activeSelf)
		{
			previousTurnSummary_panel.SetActive(value: false);
		}
		else
		{
			if (TurnManager.currPlayer.IsComputer)
			{
				return;
			}
			previousTurnSummary_panel.SetActive(value: true);
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				UnityEngine.Object.Instantiate(lastTurnSummarySlot_prefab, previousTurnSummary_parent).Setup(listOfPlayer);
			}
		}
	}

	public void SortEndTurnSummary(string p_dataType)
	{
		SoundManager.instance.PlayUIClickSound();
		List<LastTurnSummaryUI> list = previousTurnSummary_parent.GetComponentsInChildren<LastTurnSummaryUI>(includeInactive: true).ToList();
		Transform transform = new GameObject().transform;
		foreach (LastTurnSummaryUI item in list)
		{
			item.transform.SetParent(transform);
		}
		switch (p_dataType)
		{
		case "Country name":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.countryName).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.countryName).ToList());
			break;
		case "Units destroyed":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsDestroyed).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsDestroyed).ToList());
			break;
		case "Units lost":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsLost).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsLost).ToList());
			break;
		case "Units built":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsBuilt).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.unitsBuilt).ToList());
			break;
		case "Money spent":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.moneySpent).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.moneySpent).ToList());
			break;
		case "Money earned":
			list = ((!_ascending) ? list.OrderByDescending((LastTurnSummaryUI x) => x.lastTurnSummaryData.moneyEarned).ToList() : list.OrderBy((LastTurnSummaryUI x) => x.lastTurnSummaryData.moneyEarned).ToList());
			break;
		}
		foreach (LastTurnSummaryUI item2 in list)
		{
			item2.transform.SetParent(previousTurnSummary_parent);
		}
		_ascending = !_ascending;
		UnityEngine.Object.Destroy(transform.gameObject);
	}

	private void Update()
	{
		bool flag = EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _);
		if (Input.GetMouseButton(0))
		{
			LeftClickTime += Time.deltaTime;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			LeftClickTime = 0f;
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.UnselectUnit) && MapGO.selectedUnitGO != null && TurnManager.currPlayer.CanControlUnitsOf(MapGO.selectedUnitGO.unit.OwnerPlayer) && (PlayerSettings.Instance.UnselectUnit != KeyCode.Mouse0 || !Utils.IsPointerOverUIObject()))
		{
			MapGO.instance.UnselectUnit();
		}
		if (flag)
		{
			if (Input.GetKeyDown(PlayerSettings.Instance.Quickload) && !PhotonNetwork.InRoom)
			{
				try
				{
					FileInfo mostRecentSaveFile = Utils.GetMostRecentSaveFile();
					if (mostRecentSaveFile != null)
					{
						GameData.Instance.Load(mostRecentSaveFile.FullName);
						if (MainMenu.isDemo && GameData.Instance.map.turnCount > 20)
						{
							ShowMessage(LocalizationManager.Translate("info-demo no loading"));
							return;
						}
						MainMenu.isLoadedGame = true;
						MainMenu.selectedMap = mostRecentSaveFile;
						SceneManager.LoadScene("Game");
						return;
					}
				}
				catch (Exception ex)
				{
					ShowMessage(ex.Message);
				}
			}
			if (!TurnManager.currPlayer.IsComputer)
			{
				if (Input.GetKeyDown(PlayerSettings.Instance.Disengage))
				{
					MapGO.ClearTargetedUnits();
				}
				if (Input.GetKey(PlayerSettings.Instance.Waypoints))
				{
					ShowAllWaypoints();
				}
				if (Input.GetKeyUp(PlayerSettings.Instance.Waypoints))
				{
					if ((bool)MapGO.selectedUnitGO)
					{
						ShowWaypoints(MapGO.selectedUnitGO.unit);
					}
					else
					{
						_isShowingAllWaypoints = false;
						DeleteAllWaypoints();
					}
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.TimeSpeedUp) && !PhotonNetwork.InRoom)
			{
				float timeScale = Time.timeScale;
				if (Time.timeScale == 0f)
				{
					PauseUnpause();
				}
				else
				{
					Time.timeScale = Mathf.Clamp(Time.timeScale + 1f, 0f, 5f);
				}
				if (Time.timeScale != timeScale)
				{
					CancelInvoke("HideSpeedWindow");
					Invoke("HideSpeedWindow", 1.5f);
					timeSpeedWindow.SetActive(value: true);
					timeSpeed_text.text = LocalizationManager.Translate("txt-speed") + ": x" + Time.timeScale;
				}
			}
			else if (Input.GetKeyDown(PlayerSettings.Instance.TimeSlowDown) && !PhotonNetwork.InRoom)
			{
				float timeScale2 = Time.timeScale;
				if (Time.timeScale == 1f)
				{
					PauseUnpause();
				}
				Time.timeScale = Mathf.Clamp(Time.timeScale - 1f, 0f, 5f);
				if (Time.timeScale != timeScale2)
				{
					CancelInvoke("HideSpeedWindow");
					Invoke("HideSpeedWindow", 1.5f);
					timeSpeedWindow.SetActive(value: true);
					timeSpeed_text.text = LocalizationManager.Translate("txt-speed") + ": x" + Time.timeScale;
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.PauseGame) && !PhotonNetwork.InRoom)
			{
				PauseUnpause();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ManageMinimap))
			{
				ManageMinimap();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ShowHideUI))
			{
				entireUI.SetActive(!entireUI.activeSelf);
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.EndTurn))
			{
				TurnManager.instance.ButtonNextTurn();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ManageInGameMenu();
			HideTooltip();
			CloseConfirmationWindow(activeConfirmationWindow);
			if (chat_panel.activeSelf)
			{
				chat_panel.SetActive(value: false);
			}
			else
			{
				if (MapGO.instance.startScenario_Panel.activeSelf)
				{
					MapGO.instance.StartScenario();
				}
				if (stats_Panel.gameObject.activeSelf)
				{
					stats_Panel.gameObject.SetActive(value: false);
				}
				if (saveLoad_Panel.activeSelf)
				{
					saveLoad_Panel.SetActive(value: false);
				}
				if (heroStats_Panel.activeSelf)
				{
					heroStats_Panel.SetActive(value: false);
				}
				if (diplomacy_panel.activeSelf)
				{
					diplomacy_panel.SetActive(value: false);
				}
				if (previousTurnSummary_panel.activeSelf)
				{
					previousTurnSummary_panel.SetActive(value: false);
				}
				if (techTree_panel.activeSelf)
				{
					techTree_panel.SetActive(value: false);
				}
				if (OOB_panel.activeSelf)
				{
					OrderOfBattleGO.instance.StartCoroutine(OrderOfBattleGO.instance.CloseOOB());
				}
				if (Policies_panel.activeSelf)
				{
					Policies_panel.SetActive(value: false);
				}
				if (reservePanel.activeSelf)
				{
					ReservePanelGO.instance.CloseReservePanel();
				}
				if (TilesManagerGO.isOpen)
				{
					TilesManagerGO.instance.DisableTilesManagerPanel();
				}
			}
			isUIOpen = false;
		}
		else if (Input.GetKeyDown(KeyCode.Return) && activeConfirmationWindow != null)
		{
			Button[] componentsInChildren = activeConfirmationWindow.GetComponentsInChildren<Button>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].name.ToLower().Contains("yes"))
				{
					componentsInChildren[i].onClick?.Invoke();
				}
			}
		}
		if (MultiplayerChat.isTyping && Input.GetKeyDown(KeyCode.Return))
		{
			SendMessage();
		}
		if (MapGO.isOver)
		{
			return;
		}
		if (flag)
		{
			if (activeConfirmationWindow == null)
			{
				if (Input.GetKeyDown(PlayerSettings.Instance.ToggleStore))
				{
					if (store_Panel.activeSelf)
					{
						StoreGO.instance.CloseStorePanel();
					}
					else
					{
						OpenStorePanel();
					}
				}
				if (Input.GetKeyDown(PlayerSettings.Instance.ToggleReserve))
				{
					ReservePanelGO.instance.ManageReserveMenu();
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ShowBuilding))
			{
				ShowHideAllIconsForBuildings();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.FocusOnSelectedUnit) && !TurnManager.currPlayer.IsComputer && MapGO.selectedUnitGO != null)
			{
				StartCoroutine(CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y, Camera.main.transform.position.z), 0.2f));
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.Objectives))
			{
				ManageObjectivesPanel();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ToggleOrderOfBattle))
			{
				OrderOfBattleGO.instance.ManageOOB();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.SwitchNATOGraphics))
			{
				SwitchNATOrepresentation();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.Undo) && !TurnManager.currPlayer.IsComputer && MapGO.selectedUnitGO != null && undo_Button.interactable && MapGO.selectedUnitGO.canUndo)
			{
				Undo();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.Quicksave))
			{
				QuickSave();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.HideAllUnits))
			{
				SHowHideAllUnitsButton();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ToggleSuppliesMap))
			{
				ToggleSupplyMap(TurnManager.currPlayer);
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ResupplyManpower) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer && !MapGO.selectedUnitGO.isMoving)
			{
				MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
				ResupplyHP();
				if ((bool)instance.supplyLine)
				{
					UnityEngine.Object.Destroy(instance.supplyLine);
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ResupplyAmmo) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer && !MapGO.selectedUnitGO.isMoving)
			{
				MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
				ResupplyAmmo();
				if ((bool)instance.supplyLine)
				{
					UnityEngine.Object.Destroy(instance.supplyLine);
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ResupplyFuel) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer && !MapGO.selectedUnitGO.isMoving)
			{
				MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
				ResupplyFuel();
				if ((bool)instance.supplyLine)
				{
					UnityEngine.Object.Destroy(instance.supplyLine);
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ResupplyAmmoAndFuel) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer && !MapGO.selectedUnitGO.isMoving)
			{
				MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
				ResupplyAmmoAndFuel();
				if ((bool)instance.supplyLine)
				{
					UnityEngine.Object.Destroy(instance.supplyLine);
				}
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.PutUnitToSleep) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer && !MapGO.selectedUnitGO.isMoving)
			{
				ManageUnitSleep();
			}
			else if (Input.GetKeyDown(PlayerSettings.Instance.SkipUnit) && MapGO.selectedUnitGO != null)
			{
				if (MapGO.selectedUnitGO.unit.AsleepState != AsleepStates.SKIP)
				{
					MapGO.selectedUnitGO.unit.AsleepState = AsleepStates.SKIP;
				}
				else
				{
					MapGO.selectedUnitGO.unit.AsleepState = AsleepStates.NONE;
				}
				MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", MapGO.selectedUnitGO.unit, false);
				MapGO.selectedUnitGO.UpdateCounter();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.NextUnit) || Input.GetKeyDown(PlayerSettings.Instance.NextUnitGround) || Input.GetKeyDown(PlayerSettings.Instance.NextUnitAir) || Input.GetKeyDown(PlayerSettings.Instance.NextUnitNaval))
			{
				NextUnit();
			}
			else if (Input.GetKeyDown(PlayerSettings.Instance.PreviousUnit))
			{
				PrevUnit();
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.ShowHideGroundAirUnit))
			{
				ShowHideAirGroundUnit(doesSound: true);
			}
			if (Input.GetKeyDown(PlayerSettings.Instance.Disband) && MapGO.selectedUnitGO != null && !TurnManager.currPlayer.IsComputer)
			{
				OrderOfBattleGO.instance.DisbandUnit(MapGO.selectedUnitGO.unit);
			}
		}
		if (MapGO.selectedUnitGO != null && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			diveSurface_Button.gameObject.SetActive(MapGO.selectedUnitGO.unit.IsSubmarine);
			showDropZone_Button.gameObject.SetActive(MapGO.selectedUnitGO.unit.IsDropable);
			blowRepairBridge_Button.gameObject.SetActive(MapGO.selectedUnitGO.unit.IsEngineers || MapGO.selectedUnitGO.unit.IsPartisans || MapGO.selectedUnitGO.unit.IsParatrooper);
			embarkDisembark_Button.gameObject.SetActive(MapGO.selectedUnitGO.unit.FilterType != "FilterFighter" && MapGO.selectedUnitGO.unit.FilterType != "FilterCAS" && MapGO.selectedUnitGO.unit.FilterType != "FilterBomber" && (MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.AIRPORT || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR || MapGO.selectedUnitGO.HasAvailableRailroadNearby()));
			if (MapGO.selectedUnitGO.unit.AsleepState == AsleepStates.NONE)
			{
				sleeping_Button.sprite = skip_sprite;
			}
			else if (MapGO.selectedUnitGO.unit.AsleepState == AsleepStates.SKIP)
			{
				sleeping_Button.sprite = sleeping_SPrite;
			}
			else
			{
				sleeping_Button.sprite = awake_SPrite;
			}
			upgrade_button.interactable = MapGO.selectedUnitGO.unit.CanBeUpgraded();
			if (MapGO.selectedUnitGO.canUndo && MapGO.selectedUnitGO.prevTile != null && !MapGO.selectedUnitGO.isMoving)
			{
				if (MapGO.selectedUnitGO.unit.Type != "Plane" && MapGO.selectedUnitGO.prevTile.groundUnitGO == null)
				{
					undo_Button.interactable = true;
				}
				else if (MapGO.selectedUnitGO.unit.Type == "Plane" && MapGO.selectedUnitGO.prevTile.airUnitGO == null)
				{
					undo_Button.interactable = true;
				}
				else
				{
					undo_Button.interactable = false;
				}
			}
			else
			{
				undo_Button.interactable = false;
			}
			if (!MapGO.selectedUnitGO.unit.IsDropable || TurnManager.isBadWeather || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.SEA || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.RIVER || (MapGO.selectedUnitGO.tileGO.groundUnitGO != null && (!MapGO.selectedUnitGO.tileGO.groundUnitGO.unit.IsMine || MapGO.selectedUnitGO.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithCurrentPlayer)))
			{
				showDropZone_Button.interactable = false;
			}
			else
			{
				showDropZone_Button.interactable = true;
			}
			if (MapGO.selectedUnitGO.tileGO.tile.Bridge && MapGO.selectedUnitGO.unit.ActionPoints > 0 && (MapGO.selectedUnitGO.unit.IsEngineers || (MapGO.selectedUnitGO.unit.IsPartisans && !MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed) || (MapGO.selectedUnitGO.unit.IsParatrooper && !MapGO.selectedUnitGO.unit.IsDropable && !MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed)))
			{
				blowRepairBridge_Button.interactable = true;
			}
			else
			{
				blowRepairBridge_Button.interactable = false;
			}
			if (MapGO.selectedUnitGO.unit.IsSubmarine && MapGO.selectedUnitGO.unit.ActionPoints > 0)
			{
				float num = (float)MapGO.selectedUnitGO.unit.CurrHP / (float)MapGO.selectedUnitGO.unit.MaxHP;
				if ((MapGO.selectedUnitGO.unit.NumberOfTurnsUnderWater < 3 || (MapGO.selectedUnitGO.unit.NumberOfTurnsUnderWater >= 3 && MapGO.selectedUnitGO.unit.IsSubmerged)) && (num > 0.15f || MapGO.selectedUnitGO.unit.IsSubmerged))
				{
					diveSurface_Button.interactable = true;
				}
				else
				{
					diveSurface_Button.interactable = false;
				}
				diveSurface_Button.targetGraphic.GetComponent<Image>().sprite = (MapGO.selectedUnitGO.unit.IsSubmerged ? surface_sprite : dive_sprite);
			}
			else
			{
				diveSurface_Button.interactable = false;
			}
			if (MapGO.selectedUnitGO.unit.Type == "Plane")
			{
				if (!MapGO.selectedUnitGO.unit.CanBeRefueledOnCarrier && !MapGO.selectedUnitGO.unit.IsCatapultPlane)
				{
					if (MapGO.selectedUnitGO.unit.IsParatrooper)
					{
						if (MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.AIRPORT && MapGO.selectedUnitGO.tileGO.tile.NumberOfTurnsAtLowProd == 0)
						{
							embarkDisembark_Button.interactable = true;
						}
						else
						{
							embarkDisembark_Button.interactable = false;
						}
					}
					else
					{
						embarkDisembark_Button.interactable = false;
					}
					if (MapGO.selectedUnitGO.unit.ActionPoints > 0 && !MapGO.selectedUnitGO.unit.IsMissile && (MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Airport") || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Reinforcement")))
					{
						RefreshResupplyButtons(p_canResupply: true);
					}
					else
					{
						RefreshResupplyButtons(p_canResupply: false);
					}
				}
				else if (MapGO.selectedUnitGO.unit.CanBeRefueledOnCarrier)
				{
					bool flag2 = false;
					foreach (Tile item in Utils.GetAllTilesInRadiusAround(MapGO.selectedUnitGO.tileGO.tile, 1))
					{
						if (!(item.Type != "Sea") && item.GroundUnit != null && item.GroundUnit.IsCarrier && item.GroundUnit.OwnerPlayer != null && item.GroundUnit.OwnerPlayer.Faction == MapGO.selectedUnitGO.unit.OwnerPlayer.Faction)
						{
							flag2 = true;
						}
					}
					if (MapGO.selectedUnitGO.unit.ActionPoints > 0 && (flag2 || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Airport") || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Reinforcement")))
					{
						RefreshResupplyButtons(p_canResupply: true);
					}
					else
					{
						RefreshResupplyButtons(p_canResupply: false);
					}
					embarkDisembark_Button.interactable = false;
				}
				else if (MapGO.selectedUnitGO.unit.IsCatapultPlane)
				{
					bool flag3 = false;
					if (MapGO.selectedUnitGO.tileGO.tile.GroundUnit != null && MapGO.selectedUnitGO.tileGO.tile.GroundUnit.CanCatapultPlanes && MapGO.selectedUnitGO.tileGO.tile.GroundUnit.OwnerPlayer != null && MapGO.selectedUnitGO.tileGO.tile.GroundUnit.OwnerPlayer.IsAlliedWith(MapGO.selectedUnitGO.unit.OwnerPlayer))
					{
						flag3 = true;
					}
					if (MapGO.selectedUnitGO.unit.ActionPoints > 0 && (flag3 || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Airport") || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Reinforcement")))
					{
						RefreshResupplyButtons(p_canResupply: true);
					}
					else
					{
						RefreshResupplyButtons(p_canResupply: false);
					}
				}
			}
			else if (MapGO.selectedUnitGO.unit.Type == "Boat")
			{
				if ((MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Harbour") || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Reinforcement") || (MapGO.selectedUnitGO.unit.IsPatrolBoat && (MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "City") || MapGO.IsThereACertainTileAround(MapGO.selectedUnitGO.tileGO, "Town")))) && !MapGO.selectedUnitGO.unit.IsSubmerged && MapGO.selectedUnitGO.unit.ActionPoints > 0)
				{
					RefreshResupplyButtons(p_canResupply: true);
					if (MapGO.selectedUnitGO.unit.IsOnBoat && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR && MapGO.selectedUnitGO.tileGO.tile.NumberOfTurnsAtLowProd == 0)
					{
						embarkDisembark_Button.interactable = true;
					}
					else
					{
						embarkDisembark_Button.interactable = false;
					}
				}
				else
				{
					RefreshResupplyButtons(p_canResupply: false);
					if (MapGO.selectedUnitGO.unit.IsOnBoat && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR && MapGO.selectedUnitGO.tileGO.tile.NumberOfTurnsAtLowProd == 0)
					{
						embarkDisembark_Button.interactable = true;
					}
					else
					{
						embarkDisembark_Button.interactable = false;
					}
				}
			}
			else if (MapGO.selectedUnitGO.unit.IsTrain)
			{
				RefreshResupplyButtons(p_canResupply: false);
				if (MapGO.selectedUnitGO.unit.MaxMP > 0 && ((MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.AIRPORT && MapGO.selectedUnitGO.unit.IsParatrooper) || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.CITY || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.FACTORY || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.REFINERY || MapGO.selectedUnitGO.HasAvailableRailroadNearby()))
				{
					if (!MapGO.selectedUnitGO.unit.IsArmouredTrain && MapGO.selectedUnitGO.unit.FilterType != "FilterBuilding" && MapGO.selectedUnitGO.tileGO.tile.NumberOfTurnsAtLowProd == 0)
					{
						embarkDisembark_Button.interactable = true;
					}
					else
					{
						embarkDisembark_Button.interactable = false;
					}
				}
				else
				{
					embarkDisembark_Button.interactable = false;
				}
			}
			else
			{
				if (MapGO.selectedUnitGO.unit.MaxMP > 0 && ((MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.AIRPORT && MapGO.selectedUnitGO.unit.IsParatrooper) || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.CITY || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.FACTORY || MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.REFINERY || MapGO.selectedUnitGO.HasAvailableRailroadNearby()))
				{
					if (!MapGO.selectedUnitGO.unit.IsArmouredTrain && MapGO.selectedUnitGO.unit.FilterType != "FilterBuilding" && MapGO.selectedUnitGO.tileGO.tile.NumberOfTurnsAtLowProd == 0)
					{
						embarkDisembark_Button.interactable = true;
					}
					else
					{
						embarkDisembark_Button.interactable = false;
					}
				}
				else
				{
					embarkDisembark_Button.interactable = false;
				}
				if (MapGO.selectedUnitGO.unit.ActionPoints > 0)
				{
					RefreshResupplyButtons(p_canResupply: true);
				}
				else
				{
					RefreshResupplyButtons(p_canResupply: false);
				}
			}
			redeploy_Button.interactable = MapGO.selectedUnitGO.unit.CanBeRedeployed();
		}
		else
		{
			undo_Button.interactable = false;
			upgrade_button.interactable = false;
		}
	}

	private void RefreshResupplyButtons(bool p_canResupply)
	{
		Unit unit = MapGO.selectedUnitGO.unit;
		bool flag = p_canResupply && !MapGO.selectedUnitGO.isMoving && !unit.IsDead && unit.ActionPoints >= 2 && unit.SupplyPercentage > 0f;
		resupplyHP_Button.interactable = flag && unit.CurrHP < unit.MaxHP && unit.OwnerPlayer.MoneyAmount >= unit.GetHealCost() && (!PlayerSettings.Instance.Manpower || !PlayerSettings.Instance.FiniteManpower || unit.OwnerPlayer.ManpowerAvailable >= unit.GetResupplyManpowerCost(unit.GetHealAmount()));
		resupplyAmmo_Button.interactable = flag && unit.CurrAmmo != unit.MaxAmmo && !unit.HasResuppliedAmmo && unit.OwnerPlayer.MoneyAmount >= unit.GetResupplyAmmoCost();
		resupplyFuel_Button.interactable = flag && unit.MaxMP > 0 && unit.CurrAutonomy != unit.MaxAutonomy && !unit.HasResuppliedFuel && unit.OwnerPlayer.MoneyAmount >= unit.GetResupplyFuelCost();
		resupplyBoth_Button.interactable = resupplyAmmo_Button.interactable && resupplyFuel_Button.interactable && unit.OwnerPlayer.MoneyAmount >= unit.GetResupplyAmmoAndFuelCost();
	}

	private void HideSpeedWindow()
	{
		timeSpeedWindow.SetActive(value: false);
	}

	public void HideTooltip()
	{
		tooltip.SetActive(value: false);
	}

	public void RefreshTechTreeNotification()
	{
		if (PlayerSettings.Instance.TechTree && TurnManager.currPlayer != null && !TurnManager.currPlayer.IsComputer && TurnManager.currPlayer.NumberOfAvailableResearchPoints > 0)
		{
			if (MultiplayerManager.IsRealtimeMultiplayer())
			{
				if (TurnManager.currPlayer.IsLocalClientPlayer() && !TurnManager.currPlayer.HasResearchedAllUnits())
				{
					research_Notification.SetActive(value: true);
				}
				else
				{
					research_Notification.SetActive(value: false);
				}
			}
			else if (!TurnManager.currPlayer.HasResearchedAllUnits())
			{
				research_Notification.SetActive(value: true);
			}
		}
		else
		{
			research_Notification.SetActive(value: false);
		}
	}

	public void ShowAllWaypoints()
	{
		if (TurnManager.currPlayer.IsComputer || _isShowingAllWaypoints)
		{
			return;
		}
		_isShowingAllWaypoints = true;
		instance.DeleteAllWaypoints();
		for (int i = 0; i < TurnManager.currPlayer.ListOfUnits.Count; i++)
		{
			if (TurnManager.currPlayer.ListOfUnits[i].Waypoints == null)
			{
				TurnManager.currPlayer.ListOfUnits[i].Waypoints = new List<int[]>();
			}
			else
			{
				if (TurnManager.currPlayer.ListOfUnits[i].Waypoints.Count == 0 || TurnManager.currPlayer.ListOfUnits[i].IsReserve || !TurnManager.currPlayer.ListOfUnits[i].unitGO)
				{
					continue;
				}
				for (int j = 0; j < TurnManager.currPlayer.ListOfUnits[i].Waypoints.Count; j++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Waypoint"), waypointsParent);
					gameObject.GetComponentInChildren<TextMeshPro>().text = (j + 1).ToString();
					gameObject.transform.position = GameData.Instance.map.TilesTable[TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][0], TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][1]].tileGO.transform.position;
					gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -6f);
					LineRenderer component = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Line"), waypointsParent).GetComponent<LineRenderer>();
					component.startColor = Color.white;
					component.endColor = Color.white;
					if (j == 0)
					{
						component.transform.position = TurnManager.currPlayer.ListOfUnits[i].unitGO.tileGO.transform.position;
						component.SetPosition(0, TurnManager.currPlayer.ListOfUnits[i].unitGO.tileGO.transform.position);
						component.SetPosition(1, GameData.Instance.map.TilesTable[TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][0], TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][1]].tileGO.transform.position);
					}
					else
					{
						component.transform.position = GameData.Instance.map.TilesTable[TurnManager.currPlayer.ListOfUnits[i].Waypoints[j - 1][0], TurnManager.currPlayer.ListOfUnits[i].Waypoints[j - 1][1]].tileGO.transform.position;
						component.SetPosition(0, GameData.Instance.map.TilesTable[TurnManager.currPlayer.ListOfUnits[i].Waypoints[j - 1][0], TurnManager.currPlayer.ListOfUnits[i].Waypoints[j - 1][1]].tileGO.transform.position);
						component.SetPosition(1, GameData.Instance.map.TilesTable[TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][0], TurnManager.currPlayer.ListOfUnits[i].Waypoints[j][1]].tileGO.transform.position);
					}
				}
			}
		}
	}

	public void ShowWaypoints(Unit p_unit)
	{
		if (p_unit.Waypoints == null)
		{
			p_unit.Waypoints = new List<int[]>();
		}
		instance.DeleteAllWaypoints();
		_isShowingAllWaypoints = false;
		if (p_unit.Waypoints.Count == 0 || p_unit.IsReserve || !p_unit.unitGO)
		{
			return;
		}
		for (int i = 0; i < p_unit.Waypoints.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Waypoint"), waypointsParent);
			gameObject.GetComponentInChildren<TextMeshPro>().text = (i + 1).ToString();
			gameObject.transform.position = GameData.Instance.map.TilesTable[p_unit.Waypoints[i][0], p_unit.Waypoints[i][1]].tileGO.transform.position;
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -6f);
			LineRenderer component = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Line"), waypointsParent).GetComponent<LineRenderer>();
			component.startColor = Color.white;
			component.endColor = Color.white;
			if (i == 0)
			{
				component.transform.position = p_unit.unitGO.tileGO.transform.position;
				component.SetPosition(0, p_unit.unitGO.tileGO.transform.position);
				component.SetPosition(1, GameData.Instance.map.TilesTable[p_unit.Waypoints[i][0], p_unit.Waypoints[i][1]].tileGO.transform.position);
			}
			else
			{
				component.transform.position = GameData.Instance.map.TilesTable[p_unit.Waypoints[i - 1][0], p_unit.Waypoints[i - 1][1]].tileGO.transform.position;
				component.SetPosition(0, GameData.Instance.map.TilesTable[p_unit.Waypoints[i - 1][0], p_unit.Waypoints[i - 1][1]].tileGO.transform.position);
				component.SetPosition(1, GameData.Instance.map.TilesTable[p_unit.Waypoints[i][0], p_unit.Waypoints[i][1]].tileGO.transform.position);
			}
		}
	}

	public void DeleteAllWaypoints()
	{
		Transform[] componentsInChildren = waypointsParent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != waypointsParent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private IEnumerator ShowHidePausePanel()
	{
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			yield break;
		}
		float duration = 0.13f;
		Color startColor = new Color(1f, 1f, 1f, 0f);
		Color endColor = new Color(1f, 1f, 1f, 1f);
		if (Time.timeScale != 0f)
		{
			pause_image.gameObject.SetActive(value: true);
			for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
			{
				float t2 = t / duration;
				pause_image.color = Color.Lerp(startColor, endColor, t2);
				yield return null;
			}
			pause_image.color = endColor;
		}
		else
		{
			for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
			{
				float t3 = t / duration;
				pause_image.color = Color.Lerp(endColor, startColor, t3);
				yield return null;
			}
			pause_image.color = startColor;
			pause_image.gameObject.SetActive(value: false);
		}
	}

	public void SHowHideAllUnitsButton()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowHideAllUnits(!unitsParent.gameObject.activeSelf);
	}

	public void ShowHideAllUnits(bool p_show)
	{
		if (!TurnManager.currPlayer.IsComputer && (!(MapGO.selectedUnitGO != null) || !MapGO.selectedUnitGO.isMoving) && unitsParent.gameObject.activeSelf != p_show)
		{
			unitsParent.gameObject.SetActive(p_show);
		}
	}

	public void RefreshCoreUnits()
	{
		if (!MainMenu.isEditor && GameData.Instance.map.IsCampaign && TurnManager.humanPlayer != null)
		{
			coreUnit_Text.text = TurnManager.humanPlayer.CountCurrCoreUnits() + " / " + Utils.GetMaxCoreUnitsNumber(TurnManager.humanPlayer);
			coreUnit2_Text.text = LocalizationManager.Translate("txt-core units") + TurnManager.humanPlayer.CountCurrCoreUnits() + " / " + Utils.GetMaxCoreUnitsNumber(TurnManager.humanPlayer);
		}
	}

	public void ShowHideAllIconsForBuildings()
	{
		_showBuildingsIcons = !_showBuildingsIcons;
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		float num = 0f;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				if (tilesTable[i, j].Bridge || (tilesTable[i, j].IsVictoryPoint && (tilesTable[i, j].enumType == TileTypes.AIRPORT || tilesTable[i, j].enumType == TileTypes.HARBOUR || tilesTable[i, j].enumType == TileTypes.FACTORY || tilesTable[i, j].enumType == TileTypes.REFINERY || tilesTable[i, j].enumType == TileTypes.CITY)))
				{
					if (tilesTable[i, j].tileGO.buildingIcon == null)
					{
						tilesTable[i, j].tileGO.buildingIcon = UnityEngine.Object.Instantiate(buildingIconPrefab, tilesTable[i, j].tileGO.gameObject.transform);
						tilesTable[i, j].tileGO.buildingIcon.transform.position = new Vector3(tilesTable[i, j].tileGO.transform.position.x, tilesTable[i, j].tileGO.transform.position.y + 3f, -10f - num);
						tilesTable[i, j].tileGO.buildingIcon.SetActive(value: false);
					}
					string text;
					if (tilesTable[i, j].Bridge)
					{
						text = ((!tilesTable[i, j].IsBridgeDestroyed) ? "Bridge" : "Bridge destroyed");
					}
					else if (tilesTable[i, j].NumberOfTurnsAtLowProd != 0)
					{
						text = ((tilesTable[i, j].enumType == TileTypes.CITY) ? tilesTable[i, j].Name : (tilesTable[i, j].Type + " destroyed"));
					}
					else if (tilesTable[i, j].enumType == TileTypes.CITY)
					{
						text = tilesTable[i, j].Name;
					}
					else if (tilesTable[i, j].enumType != TileTypes.HARBOUR)
					{
						text = tilesTable[i, j].Type;
					}
					else
					{
						text = ((!tilesTable[i, j].Name.ToLower().Contains("small")) ? "Big Harbour" : "Small Harbour");
					}
					tilesTable[i, j].tileGO.buildingIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/Buildings icons/" + text);
					tilesTable[i, j].tileGO.buildingIcon.SetActive(_showBuildingsIcons);
					num -= 0.001f;
				}
			}
		}
	}

	public void Undo()
	{
		SoundManager.instance.PlayUIClickSound();
		MapGO.selectedUnitGO.unit.CurrMP = MapGO.selectedUnitGO.prevMP;
		MapGO.selectedUnitGO.unit.CurrHP = MapGO.selectedUnitGO.prevHP;
		MapGO.selectedUnitGO.unit.CurrAutonomy = MapGO.selectedUnitGO.prevAutonomy;
		MapGO.selectedUnitGO.unit.HasMoved = false;
		MapGO.selectedUnitGO.unit.AddRemoveActionPoints(1);
		MapGO.selectedUnitGO.unit.HasAttacked = MapGO.selectedUnitGO.backupHasAttacked;
		MapGO.selectedUnitGO.unit.NumberOfTimesTheUnitMoved = MapGO.selectedUnitGO.backupHowManyTimesUnitMoved;
		MapGO.selectedUnitGO.unit.NumberOfTilesMovedDuringCurrentTurn = MapGO.selectedUnitGO.backupHowManyTimesTilesTheUnitMoved;
		MapGO.selectedUnitGO.unit.EntrenchmentLevel = MapGO.selectedUnitGO.backupEntrenchmentLevel;
		MultiplayerManager.RPC("RPC_MoveUnit", MapGO.selectedUnitGO.unit, new List<Tile.Coordinates>
		{
			MapGO.selectedUnitGO.tileGO.tile.GetCoord(),
			MapGO.selectedUnitGO.prevTile.tile.GetCoord()
		});
		if (MapGO.selectedUnitGO.tileGO.groundUnitGO != null && MapGO.selectedUnitGO.tileGO.airUnitGO != null)
		{
			if (MapGO.selectedUnitGO.unit.Type == "Plane")
			{
				MapGO.selectedUnitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				MapGO.selectedUnitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			}
			else
			{
				MapGO.selectedUnitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
				MapGO.selectedUnitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
				MapGO.selectedUnitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: true);
			}
		}
		if (MapGO.selectedUnitGO.unit.Type == "Plane")
		{
			MapGO.selectedUnitGO.tileGO.tile.AirUnit = null;
			MapGO.selectedUnitGO.tileGO.airUnitGO = null;
			MapGO.selectedUnitGO.tileGO = MapGO.selectedUnitGO.prevTile;
			MapGO.selectedUnitGO.tileGO.airUnitGO = MapGO.selectedUnitGO;
			MapGO.selectedUnitGO.tileGO.tile.AirUnit = MapGO.selectedUnitGO.unit;
		}
		else
		{
			MapGO.selectedUnitGO.tileGO.tile.GroundUnit = null;
			MapGO.selectedUnitGO.tileGO.groundUnitGO = null;
			MapGO.selectedUnitGO.tileGO = MapGO.selectedUnitGO.prevTile;
			MapGO.selectedUnitGO.tileGO.groundUnitGO = MapGO.selectedUnitGO;
			MapGO.selectedUnitGO.tileGO.tile.GroundUnit = MapGO.selectedUnitGO.unit;
		}
		if (PlayerSettings.Instance.IsFogOfWar)
		{
			foreach (TileGO listOfNewlyDiscoveredTile in MapGO.selectedUnitGO.listOfNewlyDiscoveredTiles)
			{
				listOfNewlyDiscoveredTile.SetFogOfWar();
				listOfNewlyDiscoveredTile.tile.isVisible = false;
			}
		}
		MapGO.selectedUnitGO.transform.position = new Vector3(MapGO.selectedUnitGO.tileGO.transform.position.x, MapGO.selectedUnitGO.tileGO.transform.position.y, MapGO.selectedUnitGO.transform.position.z);
		MapGO.selectedUnitGO.SelectUnit(p_playAnimation: true);
	}

	public void PauseUnpause()
	{
		StartCoroutine(ShowHidePausePanel());
		if (Time.timeScale != 0f)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = 1f;
		}
	}

	public void ManageMinimap()
	{
		SoundManager.instance.PlayUIClickSound();
		MapGO.instance.TogglePoliticalMap();
	}

	public void ToggleSupplyMap(Player p_player = null)
	{
		if (SupplyMapOverlay.instance != null)
		{
			SoundManager.instance.PlayUIClickSound();
			SupplyMapOverlay.instance.ToggleSupplyMap(p_player);
		}
	}

	public void RenameUnit()
	{
		MapGO.selectedUnitGO.unit.ForcedCustomName = unitName_InputField.text;
		if (string.IsNullOrEmpty(MapGO.selectedUnitGO.unit.ForcedCustomName))
		{
			unitName_InputField.text = MapGO.selectedUnitGO.unit.GetUnitName();
		}
	}

	public void FinishedRenamingUnit()
	{
		EventSystem.current.SetSelectedGameObject(null);
	}

	public static void SwitchNATOrepresentation()
	{
		SoundManager.instance.PlayUIClickSound();
		SteamManager.UnlockAchievement("NATO");
		isNATO = !isNATO;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				if ((bool)listOfUnit.unitGO)
				{
					listOfUnit.unitGO.SetSprite();
				}
			}
		}
	}

	public void BlowUpRepairBridge()
	{
		if (!MapGO.selectedUnitGO.tileGO.tile.Bridge || MapGO.selectedUnitGO.unit.ActionPoints <= 0 || (!MapGO.selectedUnitGO.unit.IsEngineers && !MapGO.selectedUnitGO.unit.IsParatrooper && !MapGO.selectedUnitGO.unit.IsPartisans) || (!MapGO.selectedUnitGO.unit.IsEngineers && MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed))
		{
			return;
		}
		SoundManager.instance.PlayUIClickSound();
		MapGO.selectedUnitGO.unit.AddRemoveActionPoints(-1);
		MapGO.selectedUnitGO.ManageUnitInfoContainer();
		if (!MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound("Explosion1"));
			MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed = true;
			MapGO.selectedUnitGO.tileGO.tile.Name += " destroyed";
			if (!MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
			{
				SteamManager.UnlockAchievement("Bob the destroyer");
			}
		}
		else if (MapGO.selectedUnitGO.unit.IsEngineers)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Deploy building"));
			MapGO.selectedUnitGO.tileGO.tile.IsBridgeDestroyed = false;
			MapGO.selectedUnitGO.tileGO.tile.Name = MapGO.selectedUnitGO.tileGO.tile.Name.Replace(" destroyed", string.Empty);
			if (!MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
			{
				SteamManager.UnlockAchievement("Bob the builder");
			}
			TutorialManager.PerformAction(TutorialActions.REPAIR_BRIDGE);
		}
		MapGO.selectedUnitGO.tileGO.SetSprite();
		MapGO.selectedUnitGO.canUndo = false;
		MultiplayerManager.RPC("RPC_SyncTile", MapGO.selectedUnitGO.tileGO.tile.GetCoord(), MapGO.selectedUnitGO.tileGO.tile);
		MapGO.ClearTargetedUnits();
	}

	public void DiveGoToSurface()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!(MapGO.selectedUnitGO == null))
		{
			MapGO.selectedUnitGO.DiveUndiveSubmarine(p_sendRPC: true);
		}
	}

	public void ShowHideAirGroundUnit(bool doesSound)
	{
		if (doesSound)
		{
			SoundManager.instance.PlayUIClickSound();
		}
		if (potentialDamagePanel.activeSelf)
		{
			potentialDamagePanel.SetActive(value: false);
		}
		MapGO.ClearTargetedUnits();
		if (doesSound)
		{
			showAirUnit = !showAirUnit;
			if (showAirUnit)
			{
				twoUnitsIndicator_image.sprite = twoUnitsIndicatorAirGreen_sprite;
			}
			else
			{
				twoUnitsIndicator_image.sprite = twoUnitsIndicatorGroundGreen_sprite;
			}
		}
		else if (MapGO.selectedUnitGO != null)
		{
			if (MapGO.selectedUnitGO.unit.Type == "Plane")
			{
				showAirUnit = true;
			}
			else
			{
				showAirUnit = false;
			}
		}
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		int num = ((MapGO.selectedUnitGO == null) ? (-1) : MapGO.selectedUnitGO.tileGO.tile.PosX);
		int num2 = ((MapGO.selectedUnitGO == null) ? (-1) : MapGO.selectedUnitGO.tileGO.tile.PosY);
		for (short num3 = 0; num3 < sizeX; num3++)
		{
			for (short num4 = 0; num4 < sizeY; num4++)
			{
				Tile tile = tilesTable[num3, num4];
				if (tile.AirUnit != null && tile.GroundUnit != null)
				{
					UnitGO unitGO = tile.AirUnit.unitGO;
					UnitGO unitGO2 = tile.GroundUnit.unitGO;
					if (showAirUnit)
					{
						if (tile.PosX != num || tile.PosY != num2)
						{
							if ((bool)unitGO)
							{
								unitGO.gameObject.SetActive(value: true);
							}
							if ((bool)unitGO2)
							{
								unitGO2.gameObject.SetActive(value: false);
							}
						}
					}
					else if (tile.PosX != num || tile.PosY != num2)
					{
						if ((bool)unitGO2 && unitGO2.unit.IsMine)
						{
							if (MapGO.IsAIOnly || !TurnManager.humanPlayer.IsAlliedWith(unitGO2.unit.OwnerPlayer))
							{
								if ((bool)unitGO)
								{
									unitGO.gameObject.SetActive(value: true);
								}
								unitGO2.gameObject.SetActive(value: false);
							}
							else
							{
								if ((bool)unitGO2)
								{
									unitGO2.gameObject.SetActive(value: true);
								}
								if ((bool)unitGO)
								{
									unitGO.gameObject.SetActive(value: false);
								}
							}
						}
						else
						{
							if ((bool)unitGO2)
							{
								unitGO2.gameObject.SetActive(value: true);
							}
							if ((bool)unitGO)
							{
								unitGO.gameObject.SetActive(value: false);
							}
						}
					}
					if ((bool)unitGO && unitGO.gameObject.activeSelf)
					{
						unitGO.ManageTwoUnitsIndicator();
					}
					else if ((bool)unitGO2 && unitGO2.gameObject.activeSelf)
					{
						unitGO2.ManageTwoUnitsIndicator();
					}
				}
			}
		}
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.ShowPotentialTargets();
		}
	}

	public static void ReceivedNewHero(UnitGO p_unitGO)
	{
		if (!(p_unitGO == null))
		{
			p_unitGO.OpenStatsMenu();
			ShowMessage(p_unitGO.unit.Name + LocalizationManager.Translate("info-unit received hero"));
		}
	}

	public void ManageDiplomacyPanel()
	{
		if (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsLocalClientPlayer())
		{
			ShowMessage(LocalizationManager.Translate("error-not your turn"));
			return;
		}
		SoundManager.instance.PlayUIClickSound();
		if (!TurnManager.currPlayer.IsComputer || MapGO.IsAIOnly)
		{
			diplomacy_panel.SetActive(!diplomacy_panel.activeSelf);
		}
	}

	public void HandleMenuStacking(GameObject p_menuOpen)
	{
		if (!(p_menuOpen == store_Panel))
		{
			if (diplomacy_panel.activeSelf && p_menuOpen != diplomacy_panel)
			{
				diplomacy_panel.SetActive(value: false);
			}
			if (techTree_panel.activeSelf && p_menuOpen != techTree_panel)
			{
				techTree_panel.SetActive(value: false);
			}
			if (store_Panel.activeSelf && p_menuOpen != store_Panel)
			{
				store_Panel.SetActive(value: false);
			}
			if (Policies_panel.activeSelf && p_menuOpen != Policies_panel)
			{
				Policies_panel.SetActive(value: false);
			}
		}
	}

	public void ManageUnitSleep()
	{
		SoundManager.instance.PlayUIClickSound();
		if (MapGO.selectedUnitGO.unit.AsleepState == AsleepStates.NONE)
		{
			MapGO.selectedUnitGO.unit.AsleepState = AsleepStates.SKIP;
		}
		else if (MapGO.selectedUnitGO.unit.AsleepState == AsleepStates.SKIP)
		{
			MapGO.selectedUnitGO.unit.AsleepState = AsleepStates.SLEEP;
		}
		else if (MapGO.selectedUnitGO.unit.AsleepState == AsleepStates.SLEEP)
		{
			MapGO.selectedUnitGO.unit.AsleepState = AsleepStates.NONE;
		}
		MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", MapGO.selectedUnitGO.unit, false);
		MapGO.selectedUnitGO.UpdateCounter();
	}

	public void OpenUnitStatsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		MapGO.selectedUnitGO.OpenStatsMenu();
	}

	public void ManageObjectivesPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		objectives_Panel.SetActive(!objectives_Panel.activeSelf);
		objectiveScenarioDescription_Text.text = "";
		foreach (GameObject vPindicator in VPindicators)
		{
			UnityEngine.Object.Destroy(vPindicator);
		}
		VPindicators = new List<GameObject>();
		Player referencePlayer = ((TurnManager.humanPlayer != null) ? TurnManager.humanPlayer : TurnManager.currPlayer);
		if (!objectives_Panel.activeSelf)
		{
			return;
		}
		if (GameData.Instance.map.listOfVPThatNeedsToBeHeld.Count > 0)
		{
			bool flag = referencePlayer.Faction != GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(0).Value;
			foreach (TileGO victoryPoint in MapGO.victoryPoints)
			{
				bool flag2 = false;
				foreach (KeyValuePair<Tile, string> item in GameData.Instance.map.listOfVPThatNeedsToBeHeld)
				{
					if (item.Key.PosX == victoryPoint.tile.PosX && item.Key.PosY == victoryPoint.tile.PosY && (!flag || (flag && item.Value != referencePlayer.Faction)))
					{
						SpawnVPindicator(victoryPoint.transform.position, majorVP: true);
						flag2 = true;
					}
				}
				if (!flag2 && victoryPoint.tile.PrimaryObjective && victoryPoint.tile.ownerPlayer != null && (victoryPoint.tile.ownerPlayer.Faction == "Neutral" || victoryPoint.tile.ownerPlayer.Faction != referencePlayer.Faction))
				{
					SpawnVPindicator(victoryPoint.transform.position, victoryPoint.tile.PrimaryObjective);
				}
			}
		}
		else
		{
			foreach (TileGO victoryPoint2 in MapGO.victoryPoints)
			{
				if ((victoryPoint2.tile.PrimaryObjective || victoryPoint2.tile.SecondaryObjective) && victoryPoint2.tile.ownerPlayer != null && (victoryPoint2.tile.ownerPlayer.Faction == "Neutral" || victoryPoint2.tile.ownerPlayer.Faction != referencePlayer.Faction))
				{
					SpawnVPindicator(victoryPoint2.transform.position, victoryPoint2.tile.PrimaryObjective);
				}
			}
		}
		if (MapGO.HasReachedTurnLimit() != GameStates.NOT_SETUP)
		{
			TextMeshProUGUI textMeshProUGUI = objectiveScenarioDescription_Text;
			textMeshProUGUI.text = textMeshProUGUI.text + LocalizationManager.Translate("txt-do either before turn") + (GameData.Instance.map.NumberOfTurnsToWin + 1) + "\n";
		}
		if (GameData.Instance.map.NumberOfDamageToWin != 0)
		{
			if (GameData.Instance.map.DamageToWinFaction == TurnManager.currPlayer.Faction || GameData.Instance.map.DamageToWinFaction == "Both")
			{
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer))
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-deal damage") + o_localPlayer.TotalDamageDealt + " / " + GameData.Instance.map.NumberOfDamageToWin;
					}
				}
				else if (MapGO.IsAIOnly)
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-deal damage") + TurnManager.currPlayer.TotalDamageDealt + " / " + GameData.Instance.map.NumberOfDamageToWin;
				}
				else if (TurnManager.humanPlayer != null)
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-deal damage") + TurnManager.humanPlayer.TotalDamageDealt + " / " + GameData.Instance.map.NumberOfDamageToWin;
				}
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-recieve little damage") + " (<" + GameData.Instance.map.NumberOfDamageToWin + ")";
			}
		}
		if (GameData.Instance.map.NumberOfTilesToBombToWin != 0)
		{
			if (GameData.Instance.map.TilesToBombFaction == TurnManager.currPlayer.Faction || GameData.Instance.map.TilesToBombFaction == "Both")
			{
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer2))
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-bomb tiles") + o_localPlayer2.TotalTilesBombed + " / " + GameData.Instance.map.NumberOfTilesToBombToWin;
					}
				}
				else if (MapGO.IsAIOnly)
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-bomb tiles") + TurnManager.currPlayer.TotalTilesBombed + " / " + GameData.Instance.map.NumberOfTilesToBombToWin;
				}
				else if (TurnManager.humanPlayer != null)
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-bomb tiles") + TurnManager.humanPlayer.TotalTilesBombed + " / " + GameData.Instance.map.NumberOfTilesToBombToWin;
				}
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-protect tiles") + " (" + GameData.Instance.map.NumberOfTilesToBombToWin + ")";
			}
		}
		if (MapGO.CheckEndOfScenarioHoldingVPs() != GameStates.NOT_SETUP)
		{
			if (GameData.Instance.map.listOfVPThatNeedsToBeHeld.ElementAt(0).Value == referencePlayer.Faction)
			{
				TextMeshProUGUI textMeshProUGUI3 = objectiveScenarioDescription_Text;
				textMeshProUGUI3.text = textMeshProUGUI3.text + "\n" + LocalizationManager.Translate("txt-defend tiles");
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI4 = objectiveScenarioDescription_Text;
				textMeshProUGUI4.text = textMeshProUGUI4.text + "\n" + LocalizationManager.Translate("txt-capture tiles");
			}
			foreach (KeyValuePair<Tile, string> item2 in GameData.Instance.map.listOfVPThatNeedsToBeHeld)
			{
				if (!string.IsNullOrEmpty(item2.Key.CityName))
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + item2.Key.CityName + "  (" + item2.Key.PosX + ", " + item2.Key.PosY + ")";
				}
				else
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + item2.Key.Type + "  (" + item2.Key.PosX + ", " + item2.Key.PosY + ")";
				}
			}
		}
		if (MapGO.CheckEndOfScenarioHoldingAllMajorVPs() != GameStates.NOT_SETUP)
		{
			IEnumerable<TileGO> source = MapGO.victoryPoints.Where((TileGO p_vpGO) => p_vpGO.tile.PrimaryObjective && p_vpGO.tile.ownerPlayer != null && (p_vpGO.tile.ownerPlayer.Faction == "Neutral" || p_vpGO.tile.ownerPlayer.Faction != referencePlayer.Faction));
			if (source.Any() && source.All((TileGO p_vpGO) => p_vpGO.tile.Capturable))
			{
				if (objectiveScenarioDescription_Text.text.Count() > 0)
				{
					TextMeshProUGUI textMeshProUGUI5 = objectiveScenarioDescription_Text;
					textMeshProUGUI5.text = textMeshProUGUI5.text + "\n\n" + LocalizationManager.Translate("txt-or");
					TextMeshProUGUI textMeshProUGUI6 = objectiveScenarioDescription_Text;
					textMeshProUGUI6.text = textMeshProUGUI6.text + "\n\n" + LocalizationManager.Translate("txt-capture all major vps");
				}
				else
				{
					objectiveScenarioDescription_Text.text += LocalizationManager.Translate("txt-capture all major vps");
				}
			}
		}
		else if (MapGO.CheckEndOfScenarioHoldingAllVPs() != GameStates.NOT_SETUP)
		{
			IEnumerable<TileGO> source2 = MapGO.victoryPoints.Where((TileGO p_vpGO) => p_vpGO.tile.ownerPlayer != null && (p_vpGO.tile.ownerPlayer.Faction == "Neutral" || p_vpGO.tile.ownerPlayer.Faction != referencePlayer.Faction));
			if (source2.Any() && source2.All((TileGO p_vpGO) => p_vpGO.tile.Capturable))
			{
				if (objectiveScenarioDescription_Text.text.Count() > 0)
				{
					TextMeshProUGUI textMeshProUGUI7 = objectiveScenarioDescription_Text;
					textMeshProUGUI7.text = textMeshProUGUI7.text + "\n\n" + LocalizationManager.Translate("txt-or");
					TextMeshProUGUI textMeshProUGUI8 = objectiveScenarioDescription_Text;
					textMeshProUGUI8.text = textMeshProUGUI8.text + "\n\n" + LocalizationManager.Translate("txt-capture all vps");
				}
				else
				{
					objectiveScenarioDescription_Text.text += LocalizationManager.Translate("txt-capture all vps");
				}
			}
		}
		if (MapGO.CheckEndOfScenarioTargetsUnits() != GameStates.NOT_SETUP)
		{
			bool flag3 = false;
			string text = "";
			foreach (KeyValuePair<Unit, bool> item3 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
			{
				if (text != "" && text != item3.Key.OwnerPlayer.Faction)
				{
					flag3 = true;
				}
				text = item3.Key.OwnerPlayer.Faction;
			}
			if (objectiveScenarioDescription_Text.text.Count() > 0)
			{
				TextMeshProUGUI textMeshProUGUI9 = objectiveScenarioDescription_Text;
				textMeshProUGUI9.text = textMeshProUGUI9.text + "\n\n" + LocalizationManager.Translate("txt-or");
			}
			if (!flag3)
			{
				if (GameData.Instance.map.listOfUnitsThatNeedToBeKilled.ElementAt(0).Key.OwnerPlayer.Faction == TurnManager.currPlayer.Faction)
				{
					TextMeshProUGUI textMeshProUGUI10 = objectiveScenarioDescription_Text;
					textMeshProUGUI10.text = textMeshProUGUI10.text + "\n\n" + LocalizationManager.Translate("txt-protect units");
				}
				else
				{
					TextMeshProUGUI textMeshProUGUI11 = objectiveScenarioDescription_Text;
					textMeshProUGUI11.text = textMeshProUGUI11.text + "\n\n" + LocalizationManager.Translate("txt-destroy units");
				}
				foreach (KeyValuePair<Unit, bool> item4 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
				{
					if (!item4.Value)
					{
						TextMeshProUGUI textMeshProUGUI12 = objectiveScenarioDescription_Text;
						textMeshProUGUI12.text = textMeshProUGUI12.text + "\n- " + item4.Key.Name;
					}
				}
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI13 = objectiveScenarioDescription_Text;
				textMeshProUGUI13.text = textMeshProUGUI13.text + "\n\n" + LocalizationManager.Translate("txt-protect units");
				foreach (KeyValuePair<Unit, bool> item5 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
				{
					if (!item5.Value && item5.Key.OwnerPlayer.Faction == TurnManager.currPlayer.Faction)
					{
						TextMeshProUGUI textMeshProUGUI14 = objectiveScenarioDescription_Text;
						textMeshProUGUI14.text = textMeshProUGUI14.text + "\n- " + item5.Key.Name;
					}
				}
				TextMeshProUGUI textMeshProUGUI15 = objectiveScenarioDescription_Text;
				textMeshProUGUI15.text = textMeshProUGUI15.text + "\n\n" + LocalizationManager.Translate("txt-destroy units");
				foreach (KeyValuePair<Unit, bool> item6 in GameData.Instance.map.listOfUnitsThatNeedToBeKilled)
				{
					if (!item6.Value && item6.Key.OwnerPlayer.Faction != TurnManager.currPlayer.Faction)
					{
						TextMeshProUGUI textMeshProUGUI16 = objectiveScenarioDescription_Text;
						textMeshProUGUI16.text = textMeshProUGUI16.text + "\n- " + item6.Key.Name;
					}
				}
			}
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if ((@event.Group != referencePlayer.Country && @event.MasterPlayer != referencePlayer.Country) || (@event.HistoricalFocusOnly && !PlayerSettings.Instance.ISHistoricalFocus))
			{
				continue;
			}
			bool flag4 = false;
			foreach (EventAction eventAction in @event.EventActions)
			{
				if (eventAction.Type == ActionTypes.CAPITULATION && !(eventAction.TargetPlayerName1 != referencePlayer.Name))
				{
					flag4 = true;
					break;
				}
			}
			if (!flag4)
			{
				continue;
			}
			if (@event.CheckType == CheckTypes.VP_LOSS)
			{
				TextMeshProUGUI textMeshProUGUI17 = objectiveScenarioDescription_Text;
				textMeshProUGUI17.text = textMeshProUGUI17.text + "\n\n" + LocalizationManager.Translate("txt-capitulation warning all");
				Tile tile = GameData.Instance.map.TilesTable[@event.CheckTileCoordinateX, @event.CheckTileCoordinateY];
				if (!string.IsNullOrEmpty(tile.CityName))
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + tile.CityName + "  (" + tile.PosX + ", " + tile.PosY + ")";
				}
				else
				{
					TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + LocalizationManager.Translate(tile.Type) + "  (" + tile.PosX + ", " + tile.PosY + ")";
				}
			}
			else if (@event.CheckType == CheckTypes.VPS_LOSSES)
			{
				TextMeshProUGUI textMeshProUGUI18 = objectiveScenarioDescription_Text;
				textMeshProUGUI18.text = textMeshProUGUI18.text + "\n\n" + LocalizationManager.Translate("txt-capitulation warning all");
				foreach (Coordinates item7 in @event.VPsToBeLost)
				{
					Tile tile2 = GameData.Instance.map.TilesTable[item7.posX, item7.posY];
					if (!string.IsNullOrEmpty(tile2.CityName))
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + tile2.CityName + "  (" + tile2.PosX + ", " + tile2.PosY + ")";
					}
					else
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + LocalizationManager.Translate(tile2.Type) + "  (" + tile2.PosX + ", " + tile2.PosY + ")";
					}
				}
			}
			else
			{
				if (@event.CheckType != CheckTypes.VPS_LOSSES_ONE_OF)
				{
					continue;
				}
				TextMeshProUGUI textMeshProUGUI19 = objectiveScenarioDescription_Text;
				textMeshProUGUI19.text = textMeshProUGUI19.text + "\n\n" + LocalizationManager.Translate("txt-capitulation warning any");
				foreach (Coordinates item8 in @event.VPsToBeLost)
				{
					Tile tile3 = GameData.Instance.map.TilesTable[item8.posX, item8.posY];
					if (!string.IsNullOrEmpty(tile3.CityName))
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + tile3.CityName + "  (" + tile3.PosX + ", " + tile3.PosY + ")";
					}
					else
					{
						TextMeshProUGUI textMeshProUGUI2 = objectiveScenarioDescription_Text;
						textMeshProUGUI2.text = textMeshProUGUI2.text + "\n- " + LocalizationManager.Translate(tile3.Type) + "  (" + tile3.PosX + ", " + tile3.PosY + ")";
					}
				}
			}
		}
		TutorialManager.PerformAction(TutorialActions.OPEN_OBJECTIVE_WINDOW);
	}

	private void SpawnVPindicator(Vector3 position, bool majorVP)
	{
		string text = (majorVP ? "Major VP indicator" : "Secondary VP indicator");
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/" + text));
		position.z = -20f;
		gameObject.transform.position = position;
		VPindicators.Add(gameObject);
	}

	public void ManageMoreStatsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (moreStats_Panel.transform.position.x < 0f)
		{
			moreStats_Panel.GetComponent<Animator>().Play("OPEN more stats panel");
		}
		else
		{
			moreStats_Panel.GetComponent<Animator>().Play("CLOSE more stats panel");
		}
	}

	public void UpdateVPtext()
	{
		numberOfVP_Text.text = MapGO.numberOfCapturedMajorVPByFaction + " / " + MapGO.majorVPsAmount;
	}

	public void DropParatroopers()
	{
		SoundManager.instance.PlayUIClickSound();
		MapGO.selectedUnitGO.unit.HasMoved = true;
		MapGO.selectedUnitGO.canUndo = false;
		MapGO.selectedUnitGO.DropParatrooper(p_sendRPC: true);
		TutorialManager.PerformAction(TutorialActions.DROP_PARATROOPERS);
	}

	public void ManageSoundPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!isSoundPanelOpen)
		{
			sound_Panel.GetComponent<Animator>().Play("OPEN sound panel");
			isSoundPanelOpen = true;
		}
		else
		{
			sound_Panel.GetComponent<Animator>().Play("CLOSE sound panel");
			isSoundPanelOpen = false;
		}
	}

	public void ShowStatsMidGame()
	{
		timePlayed_Text.text = string.Empty;
		endOfScenario_Text.text = string.Empty;
		victoryDefeat_Text.text = string.Empty;
		ManageInGameMenu();
		PauseUnpause();
		closeEndOfGameMenu_button.gameObject.SetActive(value: true);
		nextScenarioChoice1_Button.gameObject.SetActive(value: false);
		nextScenarioChoice2_Button.gameObject.SetActive(value: false);
		GenerateEndOfGameStatistics(string.Empty);
		end_Panel.SetActive(value: true);
		mainMenu_Button.SetActive(value: false);
		continue_Button.SetActive(value: false);
		isUIOpen = true;
	}

	public void CloseEndOfGameStatsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		end_Panel.SetActive(value: false);
		isUIOpen = false;
		if (Time.timeScale == 0f)
		{
			PauseUnpause();
		}
	}

	public void GenerateEndOfGameStatistics(string filter)
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.TotalUnitsLost == 0)
			{
				listOfPlayer.ratio = listOfPlayer.TotalUnitsDestroyed;
			}
			else
			{
				listOfPlayer.ratio = Utils.Truncate((float)listOfPlayer.TotalUnitsDestroyed / (float)listOfPlayer.TotalUnitsLost, 2);
			}
		}
		List<Player> list = GameData.Instance.listOfPlayers.OrderBy((Player x) => x.Country).ToList();
		switch (filter)
		{
		case "Country":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.Country).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.Country).ToList());
			break;
		case "Units destroyed":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.TotalUnitsDestroyed).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.TotalUnitsDestroyed).ToList());
			break;
		case "Units lost":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.TotalUnitsLost).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.TotalUnitsLost).ToList());
			break;
		case "Units built":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.TotalUnitsBought).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.TotalUnitsBought).ToList());
			break;
		case "Units ratio":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.ratio).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.ratio).ToList());
			break;
		case "Money spent":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.TotalMoneySpent).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.TotalMoneySpent).ToList());
			break;
		case "Money earned":
			list = ((!_ascending) ? GameData.Instance.listOfPlayers.OrderByDescending((Player x) => x.TotalMoneyEarned).ToList() : GameData.Instance.listOfPlayers.OrderBy((Player x) => x.TotalMoneyEarned).ToList());
			break;
		}
		Transform[] componentsInChildren = endOfScenarioParent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != endOfScenarioParent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		foreach (Player item in list)
		{
			componentsInChildren = UnityEngine.Object.Instantiate(endOfScenarioPrefab, endOfScenarioParent).GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform2 in componentsInChildren)
			{
				if (transform2.name == "Country")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(item.Country);
				}
				if (transform2.name == "Units destroyed")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.TotalUnitsDestroyed.ToString();
					if (item.TotalUnitsDestroyed > 0)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.greenColor;
					}
				}
				if (transform2.name == "Units lost")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.TotalUnitsLost.ToString();
					if (item.TotalUnitsLost > 0)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.redColor;
					}
				}
				if (transform2.name == "Units built")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.TotalUnitsBought.ToString();
					if (item.TotalUnitsBought > 0)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.yellowColor;
					}
				}
				if (transform2.name == "Units ratio")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.ratio.ToString();
					if (item.ratio >= 1f)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.greenColor;
					}
					else if (item.ratio == 0f)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.greyColor;
					}
					else if (item.ratio < 1f)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.redColor;
					}
				}
				if (transform2.name == "Money spent")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.TotalMoneySpent.ToString();
					if (item.TotalMoneySpent > 0)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.redColor;
					}
				}
				if (transform2.name == "Money earned")
				{
					transform2.GetComponent<TextMeshProUGUI>().text = item.TotalMoneyEarned.ToString();
					if (item.TotalMoneyEarned > 0)
					{
						transform2.GetComponent<TextMeshProUGUI>().color = MainMenu.greenColor;
					}
				}
			}
		}
		_ascending = !_ascending;
	}

	public void ContinuePlayingGameAfterEnd()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.gameFinishedButContinue = true;
		end_Panel.SetActive(value: false);
		pause_image.gameObject.SetActive(value: false);
		Time.timeScale = 1f;
		MapGO.isOver = false;
	}

	public void EndTheGame(GameOutcomes p_outcome)
	{
		if (MapGO.isOver)
		{
			return;
		}
		GameData.Instance.gameFinishedButContinue = true;
		MapGO.isOver = true;
		end_Panel.SetActive(value: true);
		mainMenu_Button.SetActive(value: true);
		quitToMapEditorButton.gameObject.SetActive(value: false);
		continue_Button.SetActive(!MultiplayerManager.IsRealtimeMultiplayer() && !GameData.Instance.isPBEM && !GameData.Instance.map.IsCampaign);
		closeEndOfGameMenu_button.gameObject.SetActive(value: false);
		nextScenarioChoice1_Button.gameObject.SetActive(value: false);
		nextScenarioChoice2_Button.gameObject.SetActive(value: false);
		GenerateEndOfGameStatistics("");
		end_Panel.GetComponent<Animator>().Play("SmoothTransitionToEndPanel");
		GameData.Instance.map.timePlayed += MapGO.timeElapsedSinceGameStarted.ElapsedMilliseconds;
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(GameData.Instance.map.timePlayed);
		int num = timeSpan.Days * 24 + timeSpan.Hours;
		string text;
		if (num > 0)
		{
			text = $"{num}h {timeSpan.Minutes:D2}min {timeSpan.Seconds:D2}s";
		}
		else
		{
			text = ((timeSpan.Minutes <= 0) ? $"{timeSpan.Seconds:D2} seconds" : $"{timeSpan.Minutes:D2}min {timeSpan.Seconds:D2}s");
		}
		timePlayed_Text.text = LocalizationManager.Translate("txt-time complete scenario") + text + " (" + GameData.Instance.map.turnCount + " " + LocalizationManager.Translate("txt-turns") + ")";
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			switch (p_outcome)
			{
			case GameOutcomes.VICTORY:
				listOfPlayer.GameOutcome = (listOfPlayer.isAlliedWithCurrentPlayer ? GameOutcomes.VICTORY : GameOutcomes.DEFEAT);
				break;
			case GameOutcomes.DEFEAT:
				listOfPlayer.GameOutcome = ((!listOfPlayer.isAlliedWithCurrentPlayer) ? GameOutcomes.VICTORY : GameOutcomes.DEFEAT);
				break;
			}
		}
		if (GameData.Instance.isPBEM)
		{
			Server.Instance.GameService.EndGame(GameData.Instance, CancellationToken.None);
		}
		switch (p_outcome)
		{
		case GameOutcomes.VICTORY:
		{
			GameData.Instance.TryFindPlayerByCountry(GameData.Instance.map.OriginalCampaignCountry, out var o_player);
			CheckAchievements();
			if (GameData.Instance.isPBEM || MultiplayerManager.IsRealtimeMultiplayer())
			{
				SteamManager.instance.AddPlayerStat("Won games", 1);
			}
			victoryDefeat_Text.text = LocalizationManager.Translate("txt-victory");
			string text3 = string.Empty;
			if (TurnManager.humanPlayer != null)
			{
				text3 = TurnManager.humanPlayer.ScenarioVictoryText;
			}
			else if (TurnManager.currPlayer != null)
			{
				text3 = TurnManager.currPlayer.ScenarioVictoryText;
			}
			if (string.IsNullOrEmpty(text3))
			{
				text3 = GameData.Instance.map.ScenarioVictoryText;
			}
			endOfScenario_Text.text = text3;
			if (GameData.Instance.map.IsCampaign)
			{
				int num2 = 0;
				foreach (TileGO victoryPoint in MapGO.victoryPoints)
				{
					if (!victoryPoint.tile.SecondaryObjective)
					{
						continue;
					}
					Debug.Log(victoryPoint.tile.PosX + ", " + victoryPoint.tile.PosY + " - Owner: " + ((victoryPoint.tile.ownerPlayer != null) ? victoryPoint.tile.ownerPlayer.Country : "None"), victoryPoint);
					if (!(victoryPoint.tile.ownerPlayer.Faction == o_player.Faction))
					{
						continue;
					}
					num2++;
					if (GameData.Instance.map.XPAwarded)
					{
						continue;
					}
					foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
					{
						if (!listOfPlayer2.IsComputer && GameData.Instance.map.OriginalCampaignCountry == listOfPlayer2.Country)
						{
							listOfPlayer2.XP += GameData.Instance.map.XPperSecondaryVP;
						}
					}
				}
				if (GameData.Instance.map.NextMap2 != "")
				{
					TextMeshProUGUI textMeshProUGUI = endOfScenario_Text;
					textMeshProUGUI.text = textMeshProUGUI.text + "\n" + LocalizationManager.Translate("txt-choice campaign");
				}
				endOfScenario_Text.text += "\n \n ---------------------------------------------------------------------------------------------- \n";
				TextMeshProUGUI textMeshProUGUI2;
				if (num2 > 0)
				{
					textMeshProUGUI2 = endOfScenario_Text;
					textMeshProUGUI2.text = textMeshProUGUI2.text + LocalizationManager.Translate("txt-you captured") + num2 + LocalizationManager.Translate("txt-X secondary objectives") + GameData.Instance.map.XPperSecondaryVP * num2 + " " + LocalizationManager.Translate("txt-xp");
				}
				bool flag = false;
				foreach (Player listOfPlayer3 in GameData.Instance.listOfPlayers)
				{
					if (!listOfPlayer3.IsComputer && GameData.Instance.map.OriginalCampaignCountry == listOfPlayer3.Country && listOfPlayer3.TotalUnitsLost == 0)
					{
						if (!GameData.Instance.map.XPAwarded)
						{
							listOfPlayer3.XP += 10;
						}
						flag = true;
					}
				}
				if (flag)
				{
					TextMeshProUGUI textMeshProUGUI3 = endOfScenario_Text;
					textMeshProUGUI3.text = textMeshProUGUI3.text + "\n" + LocalizationManager.Translate("txt-no units loss");
				}
				flag = false;
				foreach (Player listOfPlayer4 in GameData.Instance.listOfPlayers)
				{
					if (!listOfPlayer4.IsComputer && GameData.Instance.map.OriginalCampaignCountry == listOfPlayer4.Country && listOfPlayer4.TotalUnitsLost <= 5)
					{
						if (!GameData.Instance.map.XPAwarded)
						{
							listOfPlayer4.XP += 5;
						}
						flag = true;
					}
				}
				if (flag)
				{
					TextMeshProUGUI textMeshProUGUI4 = endOfScenario_Text;
					textMeshProUGUI4.text = textMeshProUGUI4.text + "\n" + LocalizationManager.Translate("txt-lost 5 units or less");
				}
				flag = false;
				if (PlayerSettings.Instance.DifficultyLevel >= 3)
				{
					foreach (Player listOfPlayer5 in GameData.Instance.listOfPlayers)
					{
						if (listOfPlayer5.IsComputer || !(GameData.Instance.map.OriginalCampaignCountry == listOfPlayer5.Country))
						{
							continue;
						}
						flag = true;
						if (!GameData.Instance.map.XPAwarded)
						{
							if (PlayerSettings.Instance.DifficultyLevel == 3)
							{
								listOfPlayer5.XP += 5;
							}
							else if (PlayerSettings.Instance.DifficultyLevel == 4)
							{
								listOfPlayer5.XP += 10;
							}
						}
					}
				}
				if (flag)
				{
					TextMeshProUGUI textMeshProUGUI5 = endOfScenario_Text;
					textMeshProUGUI5.text = textMeshProUGUI5.text + "\n" + LocalizationManager.Translate("txt-won in hard");
				}
				flag = false;
				if (GameData.Instance.map.turnCount <= 30)
				{
					foreach (Player listOfPlayer6 in GameData.Instance.listOfPlayers)
					{
						if (!listOfPlayer6.IsComputer && GameData.Instance.map.OriginalCampaignCountry == listOfPlayer6.Country)
						{
							if (!GameData.Instance.map.XPAwarded)
							{
								listOfPlayer6.XP += 5;
							}
							flag = true;
						}
					}
				}
				if (flag)
				{
					TextMeshProUGUI textMeshProUGUI6 = endOfScenario_Text;
					textMeshProUGUI6.text = textMeshProUGUI6.text + "\n" + LocalizationManager.Translate("txt-won in under 30 turns");
				}
				int num3 = 20;
				int num4 = 250 / (num3 - 1);
				foreach (Player listOfPlayer7 in GameData.Instance.listOfPlayers)
				{
					if (!listOfPlayer7.IsComputer && GameData.Instance.map.OriginalCampaignCountry == listOfPlayer7.Country)
					{
						if (!GameData.Instance.map.XPAwarded)
						{
							listOfPlayer7.XP += GameData.Instance.map.XPPerScenario;
						}
						int num5 = 1;
						int num6 = num4;
						int num7 = 0;
						while (listOfPlayer7.XP >= num7 + num6 && num5 < num3)
						{
							num7 += num6;
							num5++;
							num6 = num4;
						}
						listOfPlayer7.XPNeededForNextRank = num7 + num6;
						listOfPlayer7.Rank = Mathf.Clamp(num5, 1, num3);
					}
				}
				GameData.Instance.map.XPAwarded = true;
				textMeshProUGUI2 = endOfScenario_Text;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-scenario completion") + GameData.Instance.map.XPPerScenario + " " + LocalizationManager.Translate("txt-xp");
				TextMeshProUGUI textMeshProUGUI7 = endOfScenario_Text;
				textMeshProUGUI7.text = textMeshProUGUI7.text + "\n\n" + LocalizationManager.Translate("txt-commander rank overview") + o_player.Rank;
				textMeshProUGUI2 = endOfScenario_Text;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-commander xp overview") + o_player.XP + " " + LocalizationManager.Translate("txt-xp");
				textMeshProUGUI2 = endOfScenario_Text;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "\n" + LocalizationManager.Translate("txt-next rank progress") + o_player.XP + " / " + o_player.XPNeededForNextRank + " " + LocalizationManager.Translate("txt-xp");
				if (PlayerSettings.Instance.ShowTooltips)
				{
					TextMeshProUGUI textMeshProUGUI8 = endOfScenario_Text;
					textMeshProUGUI8.text = textMeshProUGUI8.text + "\n\n" + LocalizationManager.Translate("txt-campaign core units tip");
				}
				if (GameData.Instance.map.NextMap == "" && GameData.Instance.map.NextMap2 == "" && GameData.Instance.map.NextMapDefeat == "" && GameData.Instance.map.NextMap2Defeat == "")
				{
					TextMeshProUGUI textMeshProUGUI9 = endOfScenario_Text;
					textMeshProUGUI9.text = textMeshProUGUI9.text + "\n\n" + LocalizationManager.Translate("txt-final campaign scenario");
				}
			}
			string fileName = GameData.Instance.map.FileName;
			if (MapGO.IsAIOnly || p_outcome != GameOutcomes.VICTORY || TurnManager.humanPlayer == null || string.IsNullOrEmpty(fileName))
			{
				break;
			}
			if (PlayerSettings.Instance.CompletedScenarioz.ContainsKey(fileName))
			{
				if (TurnManager.humanPlayer.Faction == "Axis")
				{
					if (!PlayerSettings.Instance.CompletedScenarioz[fileName].Contains("Axis"))
					{
						PlayerSettings.Instance.CompletedScenarioz[fileName] += "Axis";
					}
				}
				else if ((TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists") && !PlayerSettings.Instance.CompletedScenarioz[fileName].Contains("Allies"))
				{
					PlayerSettings.Instance.CompletedScenarioz[fileName] += "Allies";
				}
			}
			else if (TurnManager.humanPlayer.Faction == "Axis")
			{
				PlayerSettings.Instance.CompletedScenarioz.Add(fileName, "Axis");
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				PlayerSettings.Instance.CompletedScenarioz.Add(fileName, "Allies");
			}
			break;
		}
		case GameOutcomes.DEFEAT:
		{
			victoryDefeat_Text.text = LocalizationManager.Translate("txt-defeat");
			string text2 = string.Empty;
			if (TurnManager.humanPlayer != null)
			{
				text2 = TurnManager.humanPlayer.ScenarioDefeatText;
			}
			else if (TurnManager.currPlayer != null)
			{
				text2 = TurnManager.currPlayer.ScenarioDefeatText;
			}
			if (string.IsNullOrEmpty(text2))
			{
				text2 = GameData.Instance.map.ScenarioDefeatText;
			}
			endOfScenario_Text.text = text2;
			if (GameData.Instance.isPBEM || MultiplayerManager.IsRealtimeMultiplayer())
			{
				SteamManager.instance.AddPlayerStat("Lost games", 1);
			}
			break;
		}
		}
		ManageNextScenarioButtons(p_outcome);
		SteamManager.SetRichPresenceStatus(SteamStatuses.END_GAME_STATISTICS);
	}

	private void ManageNextScenarioButtons(GameOutcomes p_gameOutcome)
	{
		if (!GameData.Instance.map.IsCampaign || GameData.Instance.isPBEM || MultiplayerManager.IsRealtimeMultiplayer())
		{
			return;
		}
		switch (p_gameOutcome)
		{
		case GameOutcomes.VICTORY:
			if (!string.IsNullOrWhiteSpace(GameData.Instance.map.NextMap))
			{
				nextScenarioChoice1_Button.onClick.AddListener(() =>
				{
					LoadNextScenario(GameData.Instance.map.NextMap);
				});
				nextScenarioChoice1_Button.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedScenario(GameData.Instance.map.NextMap);
				nextScenarioChoice1_Button.gameObject.SetActive(value: true);
			}
			if (!string.IsNullOrWhiteSpace(GameData.Instance.map.NextMap2))
			{
				nextScenarioChoice2_Button.onClick.AddListener(() =>
				{
					LoadNextScenario(GameData.Instance.map.NextMap2);
				});
				nextScenarioChoice2_Button.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedScenario(GameData.Instance.map.NextMap2);
				nextScenarioChoice2_Button.gameObject.SetActive(value: true);
			}
			break;
		case GameOutcomes.DEFEAT:
			if (!string.IsNullOrWhiteSpace(GameData.Instance.map.NextMapDefeat))
			{
				nextScenarioChoice1_Button.onClick.AddListener(() =>
				{
					LoadNextScenario(GameData.Instance.map.NextMapDefeat);
				});
				nextScenarioChoice1_Button.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedScenario(GameData.Instance.map.NextMapDefeat);
				nextScenarioChoice1_Button.gameObject.SetActive(value: true);
			}
			if (!string.IsNullOrWhiteSpace(GameData.Instance.map.NextMap2Defeat))
			{
				nextScenarioChoice2_Button.onClick.AddListener(() =>
				{
					LoadNextScenario(GameData.Instance.map.NextMap2Defeat);
				});
				nextScenarioChoice2_Button.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedScenario(GameData.Instance.map.NextMap2Defeat);
				nextScenarioChoice2_Button.gameObject.SetActive(value: true);
			}
			break;
		}
	}

	private void LoadNextScenario(string p_campaignScenarioName)
	{
		MainMenu.isLoadedGame = false;
		MainMenu.listOfPlayersToBeSavedBetweenCampaign = new List<Player>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsComputer)
			{
				continue;
			}
			foreach (Unit listOfUnit in listOfPlayer.ListOfUnits)
			{
				listOfUnit.IsReserve = true;
				listOfUnit.NextScenario();
			}
			listOfPlayer.TotalMoneyEarned = 0;
			listOfPlayer.TotalMoneySpent = 0;
			listOfPlayer.TotalUnitsBought = 0;
			listOfPlayer.TotalUnitsDestroyed = 0;
			listOfPlayer.TotalUnitsLost = 0;
			MainMenu.listOfPlayersToBeSavedBetweenCampaign.Add(listOfPlayer);
		}
		string originalCampaignCountry = GameData.Instance.map.OriginalCampaignCountry;
		if (GameData.Instance.map.IsOfficial)
		{
			foreach (FileInfo listOfAllCampaign in MainMenu.listOfAllCampaigns)
			{
				if (Path.GetFileNameWithoutExtension(listOfAllCampaign.FullName) == p_campaignScenarioName && ((GameData.Instance.map.OriginalCampaignCountry != null && listOfAllCampaign.FullName.Contains(GameData.Instance.map.OriginalCampaignCountry)) || listOfAllCampaign.FullName.Contains(TurnManager.humanPlayer.Country)))
				{
					try
					{
						GameData.Instance.Load(listOfAllCampaign.FullName);
						GameData.Instance.map.OriginalCampaignCountry = originalCampaignCountry;
						MainMenu.selectedMap = listOfAllCampaign;
						MapGO.INeedToMakeABackupOfTheCampaign = true;
						TurnManager.currPlayer = null;
						TurnManager.humanPlayer = null;
						SceneManager.LoadScene("Game");
					}
					catch (Exception ex)
					{
						ShowMessage(ex.Message);
					}
					break;
				}
			}
		}
		else
		{
			foreach (FileInfo item in MainMenu.listOfAllMapsQuickGame)
			{
				if (Path.GetFileNameWithoutExtension(item.FullName) == p_campaignScenarioName)
				{
					try
					{
						GameData.Instance.Load(item.FullName);
						GameData.Instance.map.OriginalCampaignCountry = originalCampaignCountry;
						MainMenu.selectedMap = item;
						MapGO.INeedToMakeABackupOfTheCampaign = true;
						TurnManager.currPlayer = null;
						TurnManager.humanPlayer = null;
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					}
					catch (Exception ex2)
					{
						ShowMessage(ex2.Message);
					}
					break;
				}
			}
		}
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer2.IsComputer)
			{
				if (PlayerSettings.Instance.DifficultyLevel == 1 && listOfPlayer2.IncomeModifier < 5)
				{
					listOfPlayer2.IncomeModifier = 5;
				}
				else if (PlayerSettings.Instance.DifficultyLevel == 2 && listOfPlayer2.IncomeModifier < 10)
				{
					listOfPlayer2.IncomeModifier = 10;
				}
				else if (PlayerSettings.Instance.DifficultyLevel == 3 && listOfPlayer2.IncomeModifier < 20)
				{
					listOfPlayer2.IncomeModifier = 20;
				}
				else if (PlayerSettings.Instance.DifficultyLevel == 4 && listOfPlayer2.IncomeModifier < 30)
				{
					listOfPlayer2.IncomeModifier = 30;
				}
			}
		}
	}

	public void ManageInGameMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		if (inGameMenu_Panel.activeSelf)
		{
			DisableInGameMenu();
		}
		else if (!stats_Panel.gameObject.activeSelf && !saveLoad_Panel.activeSelf && !sound_Panel.activeSelf && !store_Panel.activeSelf && !diplomacy_panel.activeSelf && !chat_panel.activeSelf && !techTree_panel.activeSelf && !reservePanel.activeSelf && !previousTurnSummary_panel.activeSelf && !OOB_panel.activeSelf && !Policies_panel.activeSelf && !MapGO.instance.startScenario_Panel.activeSelf && !TilesManagerGO.isOpen && activeConfirmationWindow == null)
		{
			EnableInGameMenu();
		}
	}

	private void EnableInGameMenu()
	{
		if (!pause_image.gameObject.activeSelf)
		{
			StartCoroutine(ShowHidePausePanel());
		}
		isUIOpen = true;
		inGameMenu_Panel.SetActive(value: true);
		sound_Panel.SetActive(value: true);
		if (!MultiplayerManager.IsRealtimeMultiplayer())
		{
			Time.timeScale = 0f;
		}
	}

	private void DisableInGameMenu()
	{
		if (pause_image.gameObject.activeSelf)
		{
			StartCoroutine(ShowHidePausePanel());
		}
		isUIOpen = false;
		inGameMenu_Panel.SetActive(value: false);
		sound_Panel.SetActive(value: false);
		diplomacy_panel.SetActive(value: false);
		Time.timeScale = 1f;
	}

	public void OpenUpgradePanel_Button()
	{
		if (!TurnManager.currPlayer.IsComputer)
		{
			SoundManager.instance.PlayUIClickSound();
			if (MapGO.selectedUnitGO == null)
			{
				ShowMessage(LocalizationManager.Translate("error-select unit first"));
			}
			else
			{
				store_Panel.GetComponent<StoreGO>().OpenPanelInUpgradeMode(MapGO.selectedUnitGO.unit);
			}
		}
	}

	public void OpenUpgradePanel(Unit p_unit)
	{
		if (!TurnManager.currPlayer.IsComputer)
		{
			store_Panel.GetComponent<StoreGO>().OpenPanelInUpgradeMode(p_unit);
		}
	}

	public void OpenStorePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!TurnManager.currPlayer.IsComputer)
		{
			if (GameData.Instance.map.DisablePurchases)
			{
				ShowMessage(LocalizationManager.Translate("info-disabled purchases"));
			}
			else
			{
				store_Panel.GetComponent<StoreGO>().OpenPanelInPurchaseMode();
			}
		}
	}

	public void ManageStatsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (stats_Panel.gameObject.activeSelf)
		{
			stats_Panel.gameObject.SetActive(value: false);
			isUIOpen = false;
		}
		else
		{
			stats_Panel.gameObject.SetActive(value: true);
			isUIOpen = true;
		}
	}

	public void ManageChatBox()
	{
		SoundManager.instance.PlayUIClickSound();
		chat_panel.SetActive(!chat_panel.activeSelf);
		chat_Notification.SetActive(value: false);
		if (chat_panel.activeSelf)
		{
			MultiplayerChat.instance.RefreshConnectedUsers();
		}
		if (GameData.Instance.messages == null)
		{
			GameData.Instance.messages = new List<string>();
		}
		if (!GameData.Instance.isPBEM || GameData.Instance.messages.Count <= 0)
		{
			return;
		}
		Transform[] componentsInChildren = chat_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != chat_parent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		foreach (string message in GameData.Instance.messages)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(message_prefab, chat_parent);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = message;
			gameObject.transform.localScale = Vector3.one;
		}
	}

	public void SendMessage()
	{
		SoundManager.instance.PlayUIClickSound();
		if (GameData.Instance.messages == null)
		{
			GameData.Instance.messages = new List<string>();
		}
		if (!string.IsNullOrWhiteSpace(chat_inputfield.text))
		{
			if (GameData.Instance.isPBEM)
			{
				string text = PlayerSettings.Instance.Username + ": " + chat_inputfield.text;
				GameData.Instance.messages.Add(text);
				GameObject gameObject = UnityEngine.Object.Instantiate(message_prefab, chat_parent);
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
				gameObject.transform.localScale = Vector3.one;
				chat_inputfield.SetTextWithoutNotify(string.Empty);
			}
			else
			{
				MultiplayerChat.instance.SendMessage_button();
			}
		}
	}

	public void Forfeit()
	{
		instance.sound_Panel.SetActive(value: false);
		instance.inGameMenu_Panel.SetActive(value: false);
		EndTheGame(GameOutcomes.DEFEAT);
		Time.timeScale = 1f;
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			PhotonNetwork.LeaveRoom();
		}
		if (GameData.Instance.isPBEM)
		{
			if (GameData.Instance.map.turnCount < 5)
			{
				ShowMessage(LocalizationManager.Translate("error-cannot surrender pbem"));
			}
			else
			{
				Server.Instance.GameService.Forfeit(GameData.Instance.sessionID, CancellationToken.None);
			}
		}
	}

	public static GameObject ShowMessage(string p_text, float p_timeOverride = 2f)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/MessageBox"));
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = p_text;
		if (p_timeOverride <= 1f)
		{
			UnityEngine.Object.Destroy(gameObject, p_timeOverride);
		}
		else if (PlayerSettings.Instance.AutomaticallyHidePopups)
		{
			UnityEngine.Object.Destroy(gameObject, p_timeOverride);
		}
		return gameObject;
	}

	public static void ShowRelationshipChangeMessage(string country1, string country2, string text)
	{
		GameObject messBox = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Relationship change message"), instance.UI_parent);
		messBox.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text;
		messBox.GetComponentInChildren<Button>().onClick.AddListener(() =>
		{
			CloseRelationShipMessage(messBox);
		});
		messBox.transform.Find("Player1").GetComponent<Image>().sprite = Utils.GetFlagSprite(country1);
		if (!string.IsNullOrEmpty(country2))
		{
			messBox.transform.Find("Player2").GetComponent<Image>().sprite = Utils.GetFlagSprite(country2);
		}
		else
		{
			messBox.transform.Find("Player2").gameObject.SetActive(value: false);
		}
	}

	public static void CloseRelationShipMessage(GameObject relationshipWindow)
	{
		SoundManager.instance.PlayUIClickSound();
		UnityEngine.Object.Destroy(relationshipWindow);
	}

	public void EmbarkDisembark()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MapGO.selectedUnitGO.isMoving)
		{
			if (MapGO.selectedUnitGO.unit.IsParatrooper && MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.AIRPORT)
			{
				MapGO.selectedUnitGO.EmbarkDisembarkPlane();
			}
			else if (MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR && MapGO.selectedUnitGO.unit.Type != "Plane" && !MapGO.selectedUnitGO.unit.IsTrain && !MapGO.selectedUnitGO.unit.IsOnBoat)
			{
				MapGO.selectedUnitGO.EmbarkDisembarkBoat(p_sendRPC: true);
			}
			else if (MapGO.selectedUnitGO.tileGO.tile.enumType == TileTypes.HARBOUR && MapGO.selectedUnitGO.unit.Type != "Plane" && MapGO.selectedUnitGO.unit.IsOnBoat)
			{
				MapGO.selectedUnitGO.EmbarkDisembarkBoat(p_sendRPC: true);
				MapGO.selectedUnitGO.EmbarkDisembarkTrain(p_sendRPC: true);
			}
			else
			{
				MapGO.selectedUnitGO.EmbarkDisembarkTrain(p_sendRPC: true);
			}
			MapGO.selectedUnitGO.SelectUnit(p_playAnimation: false);
			if (MapGO.selectedUnitGO.unit.IsTrain)
			{
				TutorialManager.PerformAction(TutorialActions.EMBARK_IN_TRAIN);
			}
			else if (MapGO.selectedUnitGO.unit.Type == "Plane")
			{
				TutorialManager.PerformAction(TutorialActions.EMBARK_IN_PLANE);
			}
			else if (MapGO.selectedUnitGO.unit.Type == "Boat")
			{
				TutorialManager.PerformAction(TutorialActions.EMBARK_IN_BOAT);
			}
		}
	}

	public void QuickSave()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.timePlayed += MapGO.timeElapsedSinceGameStarted.ElapsedMilliseconds;
		MapGO.timeElapsedSinceGameStarted.Restart();
		string text = Clean(LocalizationManager.Translate("txt-quicksave"));
		string text2 = Clean(GameData.Instance.map.ScenarioName);
		string text3 = Clean(LocalizationManager.Translate("txt-turn"));
		string text4 = $"{DateTime.Now.Day:D2}-{DateTime.Now.Month:D2}-{DateTime.Now.Year:D4}  {DateTime.Now.Hour:D2}h{DateTime.Now.Minute:D2}";
		string s = $"{text} - {text2} - {text3} {GameData.Instance.map.turnCount} ({text4})";
		s = Clean(s);
		GameData.Instance.SaveAsSaveFile(s);
		ShowMessage(LocalizationManager.Translate("info-quicksaved"));
		string Clean(string text5)
		{
			if (string.IsNullOrEmpty(text5))
			{
				return string.Empty;
			}
			char[] array = Path.GetInvalidFileNameChars().Concat(new char[3] { ':', '/', '.' }).ToArray();
			foreach (char c in array)
			{
				text5 = text5.Replace(c.ToString(), string.Empty);
			}
			return text5;
		}
	}

	public void ResupplyHP()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MapGO.selectedUnitGO.isMoving)
		{
			MapGO.selectedUnitGO.unit.Heal();
			MapGO.selectedUnitGO.SelectUnit(p_playAnimation: false);
			resupplyCost_Panel.SetActive(value: false);
			TutorialManager.PerformAction(TutorialActions.RESUPPLY_UNIT);
		}
	}

	public void ResupplyAmmo()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MapGO.selectedUnitGO.isMoving)
		{
			int moneyAmount = TurnManager.currPlayer.MoneyAmount;
			MapGO.selectedUnitGO.unit.ResupplyAmmo();
			if (TurnManager.currPlayer.MoneyAmount < moneyAmount)
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply ammo"));
			}
			MapGO.selectedUnitGO.SelectUnit(p_playAnimation: false);
			resupplyCost_Panel.SetActive(value: false);
			TutorialManager.PerformAction(TutorialActions.RESUPPLY_UNIT);
		}
	}

	public void ResupplyFuel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MapGO.selectedUnitGO.isMoving)
		{
			int moneyAmount = TurnManager.currPlayer.MoneyAmount;
			MapGO.selectedUnitGO.unit.ResupplyFuel();
			if (TurnManager.currPlayer.MoneyAmount < moneyAmount && MapGO.selectedUnitGO.unit.MaxMP > 0 && (MapGO.selectedUnitGO.unit.IsMechanized || MapGO.selectedUnitGO.unit.IsMotorized || (MapGO.selectedUnitGO.unit.FilterType != "FilterInfantry" && MapGO.selectedUnitGO.unit.FilterType != "FilterArtillery")))
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply fuel"));
			}
			MapGO.selectedUnitGO.SelectUnit(p_playAnimation: false);
			resupplyCost_Panel.SetActive(value: false);
			TutorialManager.PerformAction(TutorialActions.RESUPPLY_UNIT);
		}
	}

	public void ResupplyAmmoAndFuel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (MapGO.selectedUnitGO.isMoving)
		{
			return;
		}
		int moneyAmount = TurnManager.currPlayer.MoneyAmount;
		MapGO.selectedUnitGO.unit.ResupplyAmmoAndFuel();
		if (TurnManager.currPlayer.MoneyAmount < moneyAmount)
		{
			if (MapGO.selectedUnitGO.unit.IsMechanized || MapGO.selectedUnitGO.unit.IsMotorized || (MapGO.selectedUnitGO.unit.FilterType != "FilterInfantry" && MapGO.selectedUnitGO.unit.FilterType != "FilterArtillery" && MapGO.selectedUnitGO.unit.FilterType != "FilterAntitank"))
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply fuel"));
			}
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply ammo"));
		}
		MapGO.selectedUnitGO.SelectUnit(p_playAnimation: false);
		resupplyCost_Panel.SetActive(value: false);
		TutorialManager.PerformAction(TutorialActions.RESUPPLY_UNIT);
	}

	public void SendSelectedUnitBackToReserve()
	{
		if (!(MapGO.selectedUnitGO == null))
		{
			OrderOfBattleGO.instance.RedeployUnit(MapGO.selectedUnitGO.unit);
		}
	}

	public void DisbandSelectedUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!(MapGO.selectedUnitGO == null) && !MapGO.selectedUnitGO.isMoving && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			ShowConfirmationWindow(LocalizationManager.Translate("info-confirm disband unit"), ConfirmDisbandUnit);
		}
	}

	private void ConfirmDisbandUnit()
	{
		if (!(MapGO.selectedUnitGO == null) && !MapGO.selectedUnitGO.isMoving && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			OrderOfBattleGO.instance.DisbandUnit(MapGO.selectedUnitGO.unit);
		}
	}

	public void NextUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer.IsComputer)
		{
			return;
		}
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.UpdateCounter();
			if (MapGO.selectedUnitGO.isMoving)
			{
				return;
			}
		}
		List<Unit> list = new List<Unit>();
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsReserve || listOfUnit.AsleepState != AsleepStates.NONE || listOfUnit.ActionPoints == 0 || listOfUnit.HasMoved || listOfUnit.IsMine || listOfUnit.IsSupplyDepot || listOfUnit.IsArmourObstacle)
			{
				continue;
			}
			if (OrderOfBattleGO.instance.OOBPanel.activeSelf && OrderOfBattleGO.instance.filterType != "All")
			{
				if (listOfUnit.FilterType == OrderOfBattleGO.instance.filterType)
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitAir))
			{
				if (listOfUnit.Type == "Plane")
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitNaval))
			{
				if (listOfUnit.Type == "Boat")
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitGround))
			{
				if (listOfUnit.Type != "Plane" && listOfUnit.Type != "Boat")
				{
					list.Add(listOfUnit);
				}
			}
			else
			{
				list.Add(listOfUnit);
			}
		}
		if (list.Count == 0)
		{
			ShowMessage(LocalizationManager.Translate("info-no unit to loop through"), 1f);
			return;
		}
		_count++;
		if (_count >= list.Count)
		{
			_count = 0;
			ShowMessage(LocalizationManager.Translate("info-looped through all"), 1f);
		}
		if ((bool)instance.supplyLine)
		{
			UnityEngine.Object.Destroy(instance.supplyLine);
		}
		foreach (Unit item in list)
		{
			if (item.ID == list[_count].ID && item.unitGO != null)
			{
				if (!item.unitGO.gameObject.activeSelf)
				{
					showAirUnit = !showAirUnit;
					ShowHideAirGroundUnit(doesSound: false);
				}
				MapGO.selectedUnitGO = item.unitGO;
				MapGO.selectedUnitGO.SelectUnit(p_playAnimation: true);
				StartCoroutine(CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y, Camera.main.transform.position.z), 0.3f));
				StartCoroutine(HighlighTile(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y));
				break;
			}
		}
	}

	public void PrevUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer.IsComputer)
		{
			return;
		}
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.UpdateCounter();
			if (MapGO.selectedUnitGO.isMoving)
			{
				return;
			}
		}
		List<Unit> list = new List<Unit>();
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsReserve || listOfUnit.AsleepState != AsleepStates.NONE || listOfUnit.ActionPoints == 0 || listOfUnit.HasMoved || listOfUnit.IsMine || listOfUnit.IsSupplyDepot || listOfUnit.IsArmourObstacle)
			{
				continue;
			}
			if (OrderOfBattleGO.instance.OOBPanel.activeSelf && OrderOfBattleGO.instance.filterType != "All")
			{
				if (listOfUnit.FilterType == OrderOfBattleGO.instance.filterType)
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitAir))
			{
				if (listOfUnit.Type == "Plane")
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitNaval))
			{
				if (listOfUnit.Type == "Boat")
				{
					list.Add(listOfUnit);
				}
			}
			else if (Input.GetKey(PlayerSettings.Instance.NextUnitGround))
			{
				if (listOfUnit.Type != "Plane" && listOfUnit.Type != "Boat")
				{
					list.Add(listOfUnit);
				}
			}
			else
			{
				list.Add(listOfUnit);
			}
		}
		if (list.Count == 0)
		{
			ShowMessage(LocalizationManager.Translate("info-no unit to loop through"), 1f);
			return;
		}
		_count--;
		if (_count < 0)
		{
			_count = list.Count - 1;
			ShowMessage(LocalizationManager.Translate("info-looped through all"), 1f);
		}
		if ((bool)instance.supplyLine)
		{
			UnityEngine.Object.Destroy(instance.supplyLine);
		}
		foreach (Unit item in list)
		{
			if (item.ID == list[_count].ID && item.unitGO != null)
			{
				if (!item.unitGO.gameObject.activeSelf)
				{
					showAirUnit = !showAirUnit;
					ShowHideAirGroundUnit(doesSound: false);
				}
				MapGO.selectedUnitGO = item.unitGO;
				MapGO.selectedUnitGO.SelectUnit(p_playAnimation: true);
				StartCoroutine(CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y, Camera.main.transform.position.z), 0.3f));
				StartCoroutine(HighlighTile(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y));
				break;
			}
		}
	}

	public IEnumerator HighlighTile(float p_worldPosX, float p_worldPosY)
	{
		yield return new WaitForSeconds(0.3f);
		GameObject gameObject = UnityEngine.Object.Instantiate(highlightRingPrefab);
		gameObject.transform.position = new Vector3(p_worldPosX, p_worldPosY, -0.48f);
		StartCoroutine(ChangeRingOpacity(gameObject));
	}

	private IEnumerator ChangeRingOpacity(GameObject instantiatedRing)
	{
		if (instantiatedRing == null)
		{
			yield break;
		}
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t2 = t / 0.5f;
			if (instantiatedRing != null)
			{
				instantiatedRing.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
			}
			yield return null;
		}
		if (instantiatedRing != null)
		{
			instantiatedRing.GetComponent<SpriteRenderer>().color = Color.white;
		}
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t3 = t / 0.5f;
			if (instantiatedRing != null)
			{
				instantiatedRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t3);
			}
			yield return null;
		}
		if (instantiatedRing != null)
		{
			instantiatedRing.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
		}
		UnityEngine.Object.Destroy(instantiatedRing);
	}

	public void CenterCameraOnUnit()
	{
		StartCoroutine(CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y, Camera.main.transform.position.z), 0.2f));
		StartCoroutine(HighlighTile(MapGO.selectedUnitGO.transform.position.x, MapGO.selectedUnitGO.transform.position.y));
	}

	public IEnumerator CenterCameraOnUnitCoroutine(Vector3 pos1, Vector3 pos2, float duration)
	{
		Vector3 pos2WithCorrectedZ = new Vector3(pos2.x, pos2.y, pos1.z);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			Camera.main.transform.position = Vector3.Lerp(pos1, pos2WithCorrectedZ, t / duration);
			yield return 0;
		}
		Camera.main.transform.position = new Vector3(pos2.x, pos2.y, -80f);
	}

	public void ManageCoreUnitsPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!TurnManager.currPlayer.IsComputer)
		{
			coreUnitsPanel.SetActive(!coreUnitsPanel.activeSelf);
			if (coreUnitsPanel.activeSelf)
			{
				GenerateCoreUnitsSlots();
			}
			else
			{
				ClearAllCoreUnitsSlots();
			}
		}
	}

	private void ClearAllCoreUnitsSlots()
	{
		Transform[] componentsInChildren = coreUnitsParent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != coreUnitsParent && transform.name != "Core units text")
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	public void GenerateCoreUnitsSlots()
	{
		ClearAllCoreUnitsSlots();
		int num = 0;
		TurnManager.currPlayer.ListOfUnits = (from x in TurnManager.currPlayer.ListOfUnits
			orderby x.Cost descending, x.XP
			select x).ToList();
		foreach (Unit unit in TurnManager.currPlayer.ListOfUnits)
		{
			if (unit.IsCoreUnit && !unit.IsReserve)
			{
				num++;
				GameObject gameObject = UnityEngine.Object.Instantiate(coreUnitPrefab, coreUnitsParent);
				gameObject.name = unit.ID.ToString();
				gameObject.transform.Find("Unit background").GetComponent<Image>().sprite = Utils.GetUnitBackground(unit, null);
				gameObject.transform.Find("Unit background").transform.Find("Unit representation").GetComponent<Image>().sprite = Utils.GetUnitSprite(unit);
				if (unit.CustomName == null || unit.CustomName == "")
				{
					gameObject.transform.Find("Unit name").GetComponent<Text>().text = unit.Name;
				}
				else
				{
					gameObject.transform.Find("Unit name").GetComponent<Text>().text = unit.CustomName;
				}
				gameObject.GetComponent<Button>().onClick.AddListener(() =>
				{
					CenterCameraOnCoreUnit(unit.ID);
				});
				if (coreUnitPrefab.GetComponent<RectTransform>().rect.height * (float)num < (float)Screen.height - coreUnitPrefab.GetComponent<RectTransform>().rect.height)
				{
					coreUnitsPanel.GetComponent<ContentSizeFitter>().enabled = true;
					LayoutRebuilder.ForceRebuildLayoutImmediate(coreUnitsParent.GetComponent<RectTransform>());
					Canvas.ForceUpdateCanvases();
				}
				else
				{
					coreUnitsPanel.GetComponent<ContentSizeFitter>().enabled = false;
				}
			}
		}
	}

	private void CenterCameraOnCoreUnit(int unit_ID)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.ID == unit_ID)
			{
				listOfUnit.unitGO.SelectUnit(p_playAnimation: true);
				CenterCameraOnUnit();
				break;
			}
		}
	}

	public void RefreshMoneyAmountUI()
	{
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer) && o_localPlayer.isAlliedWithCurrentPlayer)
			{
				playerMoneyAmount_Text.text = TurnManager.currPlayer.MoneyAmount.ToString();
				coreUnit_Text.gameObject.SetActive(value: true);
			}
			else
			{
				playerMoneyAmount_Text.text = "???";
				coreUnit_Text.gameObject.SetActive(value: false);
			}
		}
		else if (MapGO.IsAIOnly || TurnManager.humanPlayer == null || (TurnManager.currPlayer.Faction == TurnManager.humanPlayer.Faction && (!TurnManager.currPlayer.IsComputer || TurnManager.currPlayer.Faction != "Neutral")))
		{
			playerMoneyAmount_Text.text = TurnManager.currPlayer.MoneyAmount.ToString();
			coreUnit_Text.gameObject.SetActive(value: true);
		}
		else
		{
			playerMoneyAmount_Text.text = "???";
			coreUnit_Text.gameObject.SetActive(value: true);
		}
	}

	public void RefreshIncomePerTurnUI()
	{
		TurnManager.currPlayer.CalculateTurnIncome();
		int turnIncome = TurnManager.currPlayer.turnIncome;
		if (turnIncome >= 0)
		{
			playerIncomePerTurn_text.text = "(+" + turnIncome + ")";
		}
		else if (turnIncome < 0)
		{
			playerIncomePerTurn_text.text = "(" + turnIncome + ")";
		}
	}

	public void RefreshManpower()
	{
		if (MainMenu.isEditor || !PlayerSettings.Instance.Manpower)
		{
			return;
		}
		int availableManpower = TurnManager.currPlayer.GetAvailableManpower();
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			if (GameData.Instance.TryFindLocalPlayer(out var o_localPlayer) && o_localPlayer.isAlliedWithCurrentPlayer)
			{
				manpower_Text.text = Utils.GetManpowerForUI(availableManpower);
			}
			else
			{
				manpower_Text.text = "???";
			}
		}
		else if (MapGO.IsAIOnly || TurnManager.humanPlayer == null || (TurnManager.currPlayer.Faction == TurnManager.humanPlayer.Faction && (!TurnManager.currPlayer.IsComputer || TurnManager.currPlayer.Faction != "Neutral")))
		{
			manpower_Text.text = Utils.GetManpowerForUI(availableManpower);
		}
		else
		{
			manpower_Text.text = "???";
		}
	}

	private void CheckAchievements()
	{
		if (!GameData.Instance.IsIronManModeON(out var _))
		{
			return;
		}
		if (GameData.Instance.map.IsCampaign)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (!listOfPlayer.IsComputer)
				{
					if (listOfPlayer.Rank >= 5)
					{
						SteamManager.UnlockAchievement("Commander 5");
					}
					if (listOfPlayer.Rank >= 10)
					{
						SteamManager.UnlockAchievement("Commander 10");
					}
					if (listOfPlayer.Rank >= 15)
					{
						SteamManager.UnlockAchievement("Commander 15");
					}
				}
			}
			return;
		}
		string text = string.Empty;
		if (GameData.Instance.map.IsTutorial)
		{
			SteamManager.UnlockAchievement("Tutorial");
		}
		else if (GameData.Instance.map.ScenarioName == "Spanish Civil War : 17th July 1936")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Spanish Civil War Axis");
				text = "Spanish Civil War Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Spanish Civil War Allies");
				text = "Spanish Civil War Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Winter War : 30th November 1939")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Winter War Axis");
				text = "Winter War Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Winter War Allies");
				text = "Winter War Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Continuation War : 25th June 1941")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Continuation War 1941 Axis");
				text = "Continuation War 1941 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Continuation War 1941 Allies");
				text = "Continuation War 1941 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Continuation War : 10th June 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Continuation War 1944 Axis");
				text = "Continuation War 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Continuation War 1944 Allies");
				text = "Continuation War 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Fall Gelb : 10th May 1940")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("France 1940 Axis");
				text = "France 1940 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("France 1940 Allies");
				text = "France 1940 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Invasion of Greece : 6th April 1941")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				text = "Greece 1941 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				text = "Greece 1941 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Stalingrad 42")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Stalingrad 1942 Axis");
				text = "Stalingrad 1942 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Stalingrad 1942 Allies");
				text = "Stalingrad 1942 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Stalingrad 43")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Stalingrad 1943 Axis");
				text = "Stalingrad 1943 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Stalingrad 1943 Allies");
				text = "Stalingrad 1943 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Poland : 1st September 1939")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Poland 1939 Axis");
				text = "Poland 1939 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Poland 1939 Allies");
				text = "Poland 1939 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Poland : 12th January 1945")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Poland 1945 Axis");
				text = "Poland 1945 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Poland 1945 Allies");
				text = "Poland 1945 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Eastern Front: 22nd June 1941")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Eastern Front 1941 Axis");
				text = "Eastern Front 1941 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Eastern Front 1941 Allies");
				text = "Eastern Front 1941 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Eastern Front: 28th July 1942")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Eastern Front 1942 Axis");
				text = "Eastern Front 1942 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Eastern Front 1942 Allies");
				text = "Eastern Front 1942 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Eastern Front: 5th July 1943")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Eastern Front 1943 Axis");
				text = "Eastern Front 1943 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Eastern Front 1943 Allies");
				text = "Eastern Front 1943 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Eastern Front: 22nd June 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Eastern Front 1944 Axis");
				text = "Eastern Front 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Eastern Front 1944 Allies");
				text = "Eastern Front 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Final months of the Afrikakorps" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("Afrikakorps Allies");
			text = "Afrikakorps Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "Final months of the Afrikakorps" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("Afrikakorps Axis");
			text = "Afrikakorps Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "North Africa 1942" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("North Africa 1942 Allies");
			text = "North Africa 1942 Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "North Africa 1942" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("North Africa 1942 Axis");
			text = "North Africa 1942 Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : July 1937" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("Pacific 1937 Axis");
			text = "Pacific 1937 Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : July 1937" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("Pacific 1937 Allies");
			text = "Pacific 1937 Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : December 1941" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("Pacific 1941 Axis");
			text = "Pacific 1941 Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : December 1941" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("Pacific 1941 Allies");
			text = "Pacific 1941 Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : June 1942" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("Pacific 1942 Axis");
			text = "Pacific 1942 Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : June 1942" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("Pacific 1942 Allies");
			text = "Pacific 1942 Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : August 1945" && TurnManager.humanPlayer.Faction == "Axis")
		{
			SteamManager.UnlockAchievement("Pacific 1945 Axis");
			text = "Pacific 1945 Axis";
		}
		else if (GameData.Instance.map.ScenarioName == "Pacific theatre : August 1945" && TurnManager.humanPlayer.Faction == "Allies")
		{
			SteamManager.UnlockAchievement("Pacific 1945 Allies");
			text = "Pacific 1945 Allies";
		}
		else if (GameData.Instance.map.ScenarioName == "Europe: 1st September 1939")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1939 Axis");
				text = "Europe 1939 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1939 Allies");
				text = "Europe 1939 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe: 10th May 1940")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1940 Axis");
				text = "Europe 1940 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1940 Allies");
				text = "Europe 1940 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe : 22nd June 1941")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1941 Axis");
				text = "Europe 1941 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1941 Allies");
				text = "Europe 1941 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe : 8th November 1942")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1942 Axis");
				text = "Europe 1942 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1942 Allies");
				text = "Europe 1942 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe : 5th July 1943")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1943 Axis");
				text = "Europe 1943 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1943 Allies");
				text = "Europe 1943 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe : 6th June 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1944 Axis");
				text = "Europe 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1944 Allies");
				text = "Europe 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Europe : 1st March 1945")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Europe 1945 Axis");
				text = "Europe 1945 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Europe 1945 Allies");
				text = "Europe 1945 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Crimean campaign : 18th October 1941")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				text = "Crimea 1941 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				text = "Crimea 1941 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Crimean offensive : 8th April 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				text = "Crimea 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				text = "Crimea 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Operation Husky : 9th July 1943")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Italy 1943 Axis");
				text = "Italy 1943 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Italy 1943 Allies");
				text = "Italy 1943 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Operation Overlord : 6th June 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Overlord 1944 Axis");
				text = "Overlord 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Overlord 1944 Allies");
				text = "Overlord 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Battle of Anzio : 21st January 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Anzio 1944 Axis");
				text = "Anzio 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Anzio 1944 Allies");
				text = "Anzio 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Ardennes : 16th December 1944")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Bulge 1944 Axis");
				text = "Bulge 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Bulge 1944 Allies");
				text = "Bulge 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Battle of Courland '44")
		{
			Debug.Log("Hi");
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				Debug.Log("Oh");
				SteamManager.UnlockAchievement("Courland 1944 Axis");
				text = "Courland 1944 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Courland 1944 Allies");
				text = "Courland 1944 Allies";
			}
		}
		else if (GameData.Instance.map.ScenarioName == "Battle of Berlin : 16th April 1945" || GameData.Instance.map.ScenarioName == "Battle of Berlin : 11th April 1945")
		{
			if (TurnManager.humanPlayer.Faction == "Axis")
			{
				SteamManager.UnlockAchievement("Berlin 1945 Axis");
				text = "Berlin 1945 Axis";
			}
			else if (TurnManager.humanPlayer.Faction == "Allies" || TurnManager.humanPlayer.Faction == "Communists")
			{
				SteamManager.UnlockAchievement("Berlin 1945 Allies");
				text = "Berlin 1945 Allies";
			}
		}
		if (!string.IsNullOrEmpty(text) && IsLegitVictory() && (GameData.Instance.map.NumberOfTurnsToWin == 0 || GameData.Instance.map.turnCount <= GameData.Instance.map.NumberOfTurnsToWin))
		{
			SteamManager.instance.SetLeaderboard(GameData.Instance.map.turnCount, text, LeaderboardActions.KEEP_BEST);
		}
	}

	private bool IsLegitVictory()
	{
		if (GameData.Instance.map.turnCount <= 5)
		{
			return false;
		}
		if (TurnManager.humanPlayer != null && TurnManager.humanPlayer.PlayersAtWar.Count == 0)
		{
			return false;
		}
		List<string> list = new List<string>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Faction != "Neutral" && !list.Contains(listOfPlayer.Faction))
			{
				list.Add(listOfPlayer.Faction);
			}
		}
		return list.Count() > 1;
	}

	public void GoToMainMenu()
	{
		if (!_quittingToMainMenu)
		{
			_quittingToMainMenu = true;
			SoundManager.instance.PlayUIClickSound();
			SoundManager.instance.SaveSoundSettings();
			if (MultiplayerManager.IsRealtimeMultiplayer())
			{
				PhotonNetwork.LeaveRoom();
				PhotonNetwork.Disconnect();
			}
			Time.timeScale = 1f;
			MultiplayerChat.instance.Disconnect();
			StartCoroutine(CR_QuitToMainMenu());
		}
	}

	private IEnumerator CR_QuitToMainMenu()
	{
		entireUI.SetActive(value: false);
		yield return null;
		Physics2D.simulationMode = SimulationMode2D.Script;
		yield return null;
		CameraGO.instance.cam.cullingMask = 0;
		CameraGO.instance.cam.orthographicSize = 5f;
		yield return null;
		if (MapGO.instance != null)
		{
			if (MapGO.instance.politicalMap_parent != null)
			{
				yield return DestroyChildrenInBatches(MapGO.instance.politicalMap_parent);
			}
			yield return DestroyChildrenInBatches(MapGO.instance.transform);
		}
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

	public void QuitToMapEditor()
	{
		SoundManager.instance.PlayUIClickSound();
		SoundManager.instance.SaveSoundSettings();
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			PhotonNetwork.LeaveRoom();
			PhotonNetwork.Disconnect();
		}
		Time.timeScale = 1f;
		MultiplayerChat.instance.Disconnect();
		MapEditorMenu.isLoadedMapEditor = true;
		MainMenu.isEditor = true;
		SceneManager.LoadScene("MapEditor");
	}

	public void QuitGame()
	{
		SoundManager.instance.PlayUIClickSound();
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			PhotonNetwork.LeaveRoom();
			PhotonNetwork.Disconnect();
		}
		MultiplayerChat.instance.Disconnect();
		Application.Quit();
	}
}
