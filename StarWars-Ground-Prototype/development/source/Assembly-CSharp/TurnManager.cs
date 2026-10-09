using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
	public static TurnManager instance;

	public static bool isBadWeather;

	public static Player currPlayer;

	public static Player humanPlayer;

	public AnimationCurve researchRate;

	[Header("Text")]
	public TextMeshProUGUI turnNumber;

	public TextMeshProUGUI Date_Text;

	[Header("Sounds")]
	public AudioClip snowSound;

	public AudioClip rainSound;

	[Header("Weather")]
	public FadeTileGO fadeTileGO_prefab;

	[Header("Multiplayer")]
	public GameObject startmyturn_button;

	public GameObject endTurn_Panel;

	public TMP_InputField password_inputfield;

	public TextMeshProUGUI nextHumanPlayer_text;

	public Image nextHumanPlayer_image;

	[Header("Other")]
	public GameObject units;

	public OrderOfBattleGO OrderOfBattleGO;

	public GameObject rightSideUI_panel;

	public Animator sablierQuiTourne_Animator;

	private Coroutine _CR_changingSeason;

	private bool _isRealtimeMultiplayer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		ShowTurnNumberAndDate();
		ManageNotifications();
		DoMapFogOfWar();
		MainMenu.isLoadedGame = false;
		ReservePanelGO.instance.RefreshDeployNotification();
		_isRealtimeMultiplayer = MultiplayerManager.IsRealtimeMultiplayer();
		if (_isRealtimeMultiplayer && !currPlayer.IsLocalClientPlayer())
		{
			rightSideUI_panel.SetActive(value: false);
		}
		UIManager.instance.RefreshTechTreeNotification();
	}

	public void ManageWeather()
	{
		if (GameData.Instance.map.AllowSnowInWinter)
		{
			GameData.Instance.map.SetIsWinterBasedOnCurrentDate();
			if (_CR_changingSeason != null)
			{
				StopCoroutine(_CR_changingSeason);
			}
			_CR_changingSeason = StartCoroutine(CR_ChangeSeason());
		}
		else
		{
			GameData.Instance.map.IsWinter = false;
		}
		WeatherPredictionData currentTurnWeatherPredictionData = GameData.Instance.map.GetCurrentTurnWeatherPredictionData();
		if (!PlayerSettings.Instance.IsWeather || GameData.Instance.map.DisableWeather)
		{
			currentTurnWeatherPredictionData.predictionAccuracy = 100f;
			currentTurnWeatherPredictionData.predictedWeather = WeatherType.CLEAR;
			currentTurnWeatherPredictionData.actualWeather = WeatherType.CLEAR;
		}
		if (PlayerSettings.Instance.IsWeather && !GameData.Instance.map.DisableWeather)
		{
			if (currentTurnWeatherPredictionData.actualWeather == WeatherType.CLEAR)
			{
				isBadWeather = false;
				SoundManager.instance.weather_Source.Stop();
			}
			else
			{
				isBadWeather = true;
				SoundManager.instance.weather_Source.clip = (GameData.Instance.map.IsWinter ? snowSound : rainSound);
				SoundManager.instance.weather_Source.Play();
			}
		}
		else
		{
			isBadWeather = false;
		}
		RefreshTurnWeatherUI();
	}

	private void RefreshTurnWeatherUI()
	{
		WeatherPredictionData currentTurnWeatherPredictionData = GameData.Instance.map.GetCurrentTurnWeatherPredictionData();
		if (!PlayerSettings.Instance.IsWeather || GameData.Instance.map.DisableWeather)
		{
			currentTurnWeatherPredictionData.predictionAccuracy = 100f;
			currentTurnWeatherPredictionData.predictedWeather = WeatherType.CLEAR;
			currentTurnWeatherPredictionData.actualWeather = WeatherType.CLEAR;
		}
		UIManager.instance.weather_image.sprite = UIManager.instance.GetWeatherSprite(currentTurnWeatherPredictionData);
		UIManager.instance.weather_Text.text = string.Empty;
	}

	private IEnumerator CR_ChangeSeason()
	{
		HashSet<TileGO> listOfTilesThatWeNeedToChange = new HashSet<TileGO>();
		bool isWinter = GameData.Instance.map.IsWinter;
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (short num = 0; num < sizeX; num++)
		{
			for (short num2 = 0; num2 < sizeY; num2++)
			{
				Tile tile = tilesTable[num, num2];
				if (tile.SeasonNeverChanges || tile.isJungleTile || tile.enumSeason == TileSeason.DESERT)
				{
					continue;
				}
				if (tile.isCoastLineTile)
				{
					bool flag = false;
					foreach (Tile neighbour in tile.neighbours)
					{
						if (neighbour.SeasonNeverChanges || neighbour.isJungleTile || neighbour.enumSeason == TileSeason.DESERT)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (isWinter)
				{
					if (tile.enumSeason != TileSeason.WINTER)
					{
						listOfTilesThatWeNeedToChange.Add(tile.tileGO);
					}
				}
				else if (tile.enumSeason == TileSeason.WINTER)
				{
					listOfTilesThatWeNeedToChange.Add(tile.tileGO);
				}
			}
		}
		if (listOfTilesThatWeNeedToChange.Count == 0)
		{
			yield break;
		}
		bool winter = GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter;
		if (PlayerSettings.Instance.InstantSeasonChange && !MultiplayerManager.IsRealtimeMultiplayer())
		{
			foreach (TileGO item in listOfTilesThatWeNeedToChange)
			{
				item.tile.Season = (winter ? "Winter" : "Summer");
				item.tile.isFrozen = winter && item.tile.CanFreezeInWinter;
				Utils.UpdateTileProperties(item.tile, Utils.GetOriginalTile(item.tile), p_nonDestructive: false);
				item.SetSprite();
				item.tile.SetRuntimeOptimizations();
			}
			yield break;
		}
		int numberOfTilesPerFrame = Mathf.Clamp(GameData.Instance.map.TilesTable.Length / 1500, 1, 50);
		while (listOfTilesThatWeNeedToChange.Count > 0)
		{
			yield return null;
			for (int i = 0; i < numberOfTilesPerFrame; i++)
			{
				TileGO tileGO = listOfTilesThatWeNeedToChange.ElementAt(UnityEngine.Random.Range(0, listOfTilesThatWeNeedToChange.Count));
				tileGO.tile.Season = (winter ? "Winter" : "Summer");
				tileGO.tile.isFrozen = winter && tileGO.tile.CanFreezeInWinter;
				Utils.UpdateTileProperties(tileGO.tile, Utils.GetOriginalTile(tileGO.tile), p_nonDestructive: false);
				if (CameraGO.instance.cam.orthographicSize < 150f && Vector2.Distance(tileGO.transform.position, CameraGO.instance.transform.position) < 150f)
				{
					UnityEngine.Object.Instantiate(fadeTileGO_prefab).StartFade(tileGO);
				}
				tileGO.SetSprite();
				tileGO.tile.SetRuntimeOptimizations();
				listOfTilesThatWeNeedToChange.Remove(tileGO);
				if (listOfTilesThatWeNeedToChange.Count == 0)
				{
					break;
				}
			}
		}
	}

	public void ButtonNextTurn()
	{
		if (!AI.isPlaying && !UIManager.isUIOpen && (!(MapGO.selectedUnitGO != null) || !MapGO.selectedUnitGO.isMoving))
		{
			SoundManager.instance.PlayUIClickSound();
			if (MultiplayerManager.IsRealtimeMultiplayer() && !currPlayer.IsLocalClientPlayer())
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-not player turn"));
			}
			else if (MainMenu.isDemo && GameData.Instance.map.turnCount >= 20 && !MultiplayerManager.IsRealtimeMultiplayer() && !GameData.Instance.map.IsTutorial)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-demo turn limit"));
				GameData.Instance.SaveAsSaveFile(GameData.Instance.map.ScenarioName);
			}
			else if (PlayerSettings.Instance.TechTree && !currPlayer.HasResearchedAllUnits() && currPlayer.NumberOfAvailableResearchPoints > 0)
			{
				UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-end turn research available"), NextTurnConfirmation);
			}
			else if (PlayerSettings.Instance.Policies && ((currPlayer.HighCommandPoints == 100 && !currPlayer.HasPolicy(Policies.Type.Bigger_command_centers)) || (currPlayer.HighCommandPoints == 200 && currPlayer.HasPolicy(Policies.Type.Bigger_command_centers))))
			{
				UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-end turn HQ points"), NextTurnConfirmation);
			}
			else if (currPlayer.AreThereUnitsReadyForDeployment())
			{
				UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-end turn units available"), NextTurnConfirmation);
			}
			else
			{
				NextTurnConfirmation();
			}
		}
	}

	private void NextTurnConfirmation()
	{
		StartCoroutine(CR_MoveUnitsWaypoints());
	}

	public void NextTurn(bool p_sendRPC)
	{
		TutorialManager.PerformAction(TutorialActions.END_TURN);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayer", currPlayer);
			MultiplayerManager.RPC("RPC_SyncNotifications", GameData.Instance.map.Notifications);
			MultiplayerManager.RPC("RPC_EndTurn");
		}
		if (UIManager.instance.coreUnitsPanel.activeSelf)
		{
			UIManager.instance.ManageCoreUnitsPanel();
		}
		if (UIManager.instance.objectives_Panel.activeSelf)
		{
			UIManager.instance.objectives_Panel.SetActive(value: false);
		}
		if (TilesManagerGO.isOpen)
		{
			TilesManagerGO.instance.DisableTilesManagerPanel();
		}
		if ((bool)StoreGO.instance && StoreGO.instance.gameObject.activeSelf)
		{
			StoreGO.instance.CloseStorePanel();
		}
		if (ReservePanelGO.instance.reservePanel.activeSelf)
		{
			ReservePanelGO.instance.reservePanel.SetActive(value: false);
		}
		UIManager.instance.HideTooltip();
		UIManager.instance.ShowHideAllUnits(p_show: true);
		MapGO.instance.UnselectUnit();
		MapGO.numberOfCapturedMajorVPByFaction = 0;
		if (!PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
		{
			int sizeX = GameData.Instance.map.SizeX;
			int sizeY = GameData.Instance.map.SizeY;
			Tile[,] tilesTable = GameData.Instance.map.TilesTable;
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					Tile tile = tilesTable[i, j];
					if (tile.AirUnit != null && !tile.AirUnit.HasMoved && (tile.AirUnit.OwnerPlayer == null || !(tile.AirUnit.OwnerPlayer.Country != currPlayer.Country)))
					{
						if (GameData.Instance.map.Date.Year >= 1900 && !MapGO.IsThereACertainTileAround(tile.tileGO, "Airport") && tile.AirUnit.CurrAutonomy > 0)
						{
							tile.AirUnit.CurrAutonomy--;
						}
						if (tile.AirUnit.CurrAutonomy < 0)
						{
							tile.AirUnit.CurrAutonomy = 0;
						}
					}
				}
			}
		}
		if (!currPlayer.IsComputer)
		{
			ManageAutoResupply(p_endOfTurn: true);
		}
		if (currPlayer.NumberOfPartisansLost > 0)
		{
			currPlayer.NumberOfPartisansLost--;
		}
		SpawnPartisans(currPlayer);
		if (OrderOfBattleGO.instance.OOBPanel.activeSelf)
		{
			OrderOfBattleGO.StartCoroutine(OrderOfBattleGO.CloseOOB());
		}
		currPlayer.BackupTurnStatsAndReset();
		currPlayer.AddTurnStats();
		currPlayer.ClearedFOW.Clear();
		StartCoroutine(CR_RotateHourglass());
		SetNextPlayerPlaying();
		DrawingManager.instance.SpawnLinesForPlayer(currPlayer);
		ProcessPlayerResearch(currPlayer);
		ManageNotifications();
		if (currPlayer.IsPlayingFirst)
		{
			GameData.Instance.map.turnCount++;
			foreach (TileGO victoryPoint in MapGO.victoryPoints)
			{
				if (victoryPoint.tile.NumberOfTurnsSinceVPGotCaptured + 1 < 255)
				{
					victoryPoint.tile.NumberOfTurnsSinceVPGotCaptured = (byte)Mathf.Clamp((byte)(victoryPoint.tile.NumberOfTurnsSinceVPGotCaptured + 1), 0, 255);
				}
			}
			if (GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0)
			{
				GameData.Instance.map.Date = GameData.Instance.map.Date.AddDays(GameData.Instance.map.NumberOfDaysThatGoesByPerTurn);
			}
			ManageWeather();
			ShowTurnNumberAndDate();
			ProcessTruces();
			ProcessEvents();
			if (PlayerSettings.Instance.Policies)
			{
				Policies.InitializePolicies();
			}
		}
		foreach (Unit listOfUnit in currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsReserve && listOfUnit.TurnToProduce - 1 >= 0)
			{
				listOfUnit.TurnToProduce--;
			}
			else
			{
				if (listOfUnit.IsReserve || !listOfUnit.unitGO)
				{
					continue;
				}
				listOfUnit.NextTurn();
				if (listOfUnit.NumberOfTurnsUnderWater >= 4)
				{
					listOfUnit.IsSubmerged = false;
				}
				bool flag = false;
				TileGO tileGO = listOfUnit.unitGO.tileGO;
				if (!tileGO)
				{
					continue;
				}
				if (tileGO.tile.CanFreezeInWinter && listOfUnit.Type != "Plane")
				{
					if (GameData.Instance.map.IsWinter)
					{
						if (listOfUnit.IsOnBoat)
						{
							listOfUnit.EmbarkDisembarkBoat();
							listOfUnit.unitGO.SetSprite();
						}
						else if (listOfUnit.FilterType == "FilterBoat")
						{
							flag = true;
						}
					}
					else if (!listOfUnit.IsOnBoat && !tileGO.tile.Bridge && tileGO.tile.enumType != TileTypes.RIVER && listOfUnit.FilterType != "FilterBoat")
					{
						flag = true;
					}
				}
				if (!flag && (listOfUnit.unitGO.tileGO.tile.enumType == TileTypes.SEA || (listOfUnit.unitGO.tileGO.tile.terrainOwner == null && listOfUnit.OwnerPlayer.Faction != "Neutral") || (listOfUnit.unitGO.tileGO.tile.terrainOwner == null && listOfUnit.OwnerPlayer.Faction == "Neutral" && !listOfUnit.OwnerPlayer.IsComputer) || (listOfUnit.unitGO.tileGO.tile.terrainOwner != null && (listOfUnit.unitGO.tileGO.tile.terrainOwner.Name == listOfUnit.OwnerPlayer.Name || listOfUnit.unitGO.tileGO.tile.terrainOwner.Country == "Neutral"))))
				{
					continue;
				}
				if (listOfUnit.OwnerPlayer.IsComputer && listOfUnit.OwnerPlayer.Faction == "Neutral" && tileGO.tile.terrainOwner == null)
				{
					flag = true;
				}
				else if (tileGO.tile.terrainOwner != null && !tileGO.tile.terrainOwner.IsAlliedWith(listOfUnit.OwnerPlayer) && !tileGO.tile.terrainOwner.IsAtWarWith(listOfUnit.OwnerPlayer))
				{
					flag = true;
				}
				if (flag)
				{
					listOfUnit.IsReserve = true;
					listOfUnit.TurnToProduce = 2;
					if (listOfUnit.Type == "Plane")
					{
						listOfUnit.unitGO.tileGO.airUnitGO = null;
						listOfUnit.unitGO.tileGO.tile.AirUnit = null;
					}
					else
					{
						listOfUnit.unitGO.tileGO.groundUnitGO = null;
						listOfUnit.unitGO.tileGO.tile.GroundUnit = null;
					}
					UnityEngine.Object.Destroy(listOfUnit.unitGO.gameObject);
				}
			}
		}
		if (!PlayerSettings.Instance.SkipEndTurnSummary)
		{
			UIManager.instance.ShowPreviousTurnSummary(makeClickSound: false);
		}
		MapGO.numberOfCapturedMajorVPByFaction = 0;
		MapGO.currentPlayerVPs = new List<TileGO>();
		ResetRadarSpottings();
		DoMapFogOfWar();
		if (isBadWeather)
		{
			foreach (Unit listOfUnit2 in currPlayer.ListOfUnits)
			{
				if (listOfUnit2.IsReserve)
				{
					continue;
				}
				if (listOfUnit2.CurrAutonomy > 0 && (bool)listOfUnit2.unitGO && (bool)listOfUnit2.unitGO.tileGO && !listOfUnit2.unitGO.tileGO.tile.IsVictoryPoint)
				{
					if (listOfUnit2.Type == "Plane")
					{
						if (GameData.Instance.map.Date.Year >= 1900 && !MapGO.IsThereACertainTileAround(listOfUnit2.unitGO.tileGO, "Airport"))
						{
							listOfUnit2.CurrAutonomy--;
						}
					}
					else if (listOfUnit2.Type == "Boat")
					{
						if (!MapGO.IsThereACertainTileAround(listOfUnit2.unitGO.tileGO, "Harbour"))
						{
							listOfUnit2.CurrAutonomy--;
						}
					}
					else
					{
						listOfUnit2.CurrAutonomy--;
					}
				}
				if (listOfUnit2.CurrMP > 0 && listOfUnit2.FilterType != "FilterInfantry" && (GameData.Instance.map.Date.Year >= 1900 || listOfUnit2.Type != "Plane"))
				{
					listOfUnit2.CurrMP--;
				}
				if (!listOfUnit2.IsEliteUnit || listOfUnit2.Lvl == 1)
				{
					listOfUnit2.AddMorale(-5);
				}
			}
		}
		if (!currPlayer.IsComputer)
		{
			ManageAutoResupply(p_endOfTurn: false);
		}
		if (currPlayer.IsPlayingFirst)
		{
			GameData.Instance.map.timePlayed += MapGO.timeElapsedSinceGameStarted.ElapsedMilliseconds;
			MapGO.timeElapsedSinceGameStarted.Restart();
			if (PlayerSettings.Instance.AutosavesAmount > 0 && PlayerSettings.Instance.AutosavesFrequency > 0 && GameData.Instance.map.turnCount % PlayerSettings.Instance.AutosavesFrequency == 0)
			{
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					GameData.Instance.SaveAsSaveFile(LocalizationManager.Translate("txt-autosave multiplayer") + " " + PlayerSettings.Instance.GetNextAutosaveNumber());
				}
				else if (!GameData.Instance.isPBEM)
				{
					GameData.Instance.SaveAsSaveFile(LocalizationManager.Translate("txt-autosave") + " " + PlayerSettings.Instance.GetNextAutosaveNumber());
				}
			}
			MapGO.onSaveLoadedHumanPlayer = null;
		}
		UIManager.instance.RefreshMoneyAmountUI();
		UIManager.instance.RefreshIncomePerTurnUI();
		UIManager.instance.RefreshManpower();
		MapGO.CheckIfEndOfScenario();
		SteamManager.SetRichPresenceStatus(SteamStatuses.DEFAULT);
		ReservePanelGO.instance.RefreshDeployNotification();
		if (GameData.Instance.map.IsCampaign)
		{
			return;
		}
		List<Player> list = new List<Player>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!listOfPlayer.IsComputer)
			{
				list.Add(listOfPlayer);
			}
		}
		if (list.Count <= 1 || _isRealtimeMultiplayer || currPlayer.IsComputer || currPlayer == MapGO.onSaveLoadedHumanPlayer || (!GameData.Instance.isPBEM && !GameData.Instance.offlinePBEM))
		{
			return;
		}
		if (GameData.Instance.isPBEM)
		{
			if (!currPlayer.IsLocalClientPlayer())
			{
				ShowHumanEndTurnPanel();
				startmyturn_button.SetActive(value: false);
				MapGO.onSaveLoadedHumanPlayer = null;
				GameData.Instance.endOfTurnPanelOpen = false;
				Server.Instance.GameService.EndTurn(GameData.Instance, CancellationToken.None);
			}
		}
		else if (GameData.Instance.offlinePBEM)
		{
			ShowHumanEndTurnPanel();
			GameData.Instance.SaveAsSaveFile(currPlayer.Country + "_" + GameData.Instance.map.turnCount);
			Process.Start(PrototypePaths.DataPath + "/Saves");
		}
	}

	public void DiplomacyChanged(bool p_refreshBorders)
	{
		MapGO.radarDetections = new HashSet<TileGO>();
		MapGO.sonarDetections = new HashSet<TileGO>();
		MapGO.PlaceFogOfWarEverywhere();
		MapGO.RemoveFogOfWarAroundVictoryPoints();
		MapGO.RemoveFogOfWarAroundUnits();
		MapGO.ProcessFogOfWar();
		if (p_refreshBorders)
		{
			MapGO.instance.DrawBordersOnMap();
		}
	}

	private void ManageAutoResupply(bool p_endOfTurn)
	{
		foreach (Unit item in currPlayer.ListOfUnits.OrderByDescending((Unit x) => x.Cost).ToList())
		{
			if (currPlayer.MoneyAmount <= 0)
			{
				break;
			}
			if (!item.IsReserve && item.AutoSupplies && !(item.unitGO == null))
			{
				int moneyAmount = currPlayer.MoneyAmount;
				item.unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
				if (p_endOfTurn)
				{
					item.Heal();
				}
				item.ResupplyAmmoAndFuel();
				if (currPlayer.MoneyAmount != moneyAmount)
				{
					item.unitGO.UpdateCounter();
				}
			}
		}
	}

	public void ShowHumanEndTurnPanel()
	{
		UIManager.isUIOpen = true;
		GameData.Instance.endOfTurnPanelOpen = true;
		endTurn_Panel.SetActive(value: true);
		password_inputfield.SetTextWithoutNotify(string.Empty);
		nextHumanPlayer_text.text = LocalizationManager.Translate("txt-next human player playing") + LocalizationManager.GetLocalizedCountry(currPlayer.Country);
		nextHumanPlayer_image.sprite = Utils.GetFlagSprite(currPlayer.Country);
		password_inputfield.gameObject.SetActive(!GameData.Instance.isPBEM);
	}

	private IEnumerator CR_MoveUnitsWaypoints()
	{
		bool quickMovementBackup = PlayerSettings.Instance.IsQuickMovement;
		PlayerSettings.Instance.IsQuickMovement = true;
		for (int i = 0; i < currPlayer.ListOfUnits.Count; i++)
		{
			if (currPlayer.ListOfUnits[i].ActionPoints > 0 && currPlayer.ListOfUnits[i].NumberOfTimesTheUnitMoved < 2 && currPlayer.ListOfUnits[i].CurrMP != 0 && currPlayer.ListOfUnits[i].Waypoints != null && currPlayer.ListOfUnits[i].Waypoints.Count != 0 && (!currPlayer.ListOfUnits[i].IsRecon || !currPlayer.ListOfUnits[i].HasMoved || !currPlayer.ListOfUnits[i].IsOnBoat) && (bool)currPlayer.ListOfUnits[i].unitGO)
			{
				currPlayer.ListOfUnits[i].unitGO.MoveToWayPoint();
				while (currPlayer.ListOfUnits[i].unitGO.isMoving)
				{
					yield return null;
				}
			}
		}
		PlayerSettings.Instance.IsQuickMovement = quickMovementBackup;
		NextTurn(p_sendRPC: true);
	}

	public float GetResearchRate(Player p_player, Unit p_unit)
	{
		if (p_unit == null)
		{
			return 0f;
		}
		if (p_player == null)
		{
			return 0f;
		}
		float num = 0.2f;
		float num2 = 0f;
		float num3 = ((GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0) ? ((float)GameData.Instance.map.NumberOfDaysThatGoesByPerTurn / 7f) : 0.1f);
		int monthDifference = Utils.GetMonthDifference(new DateTime(p_unit.ProductionYear, p_unit.ProductionMonth, 1), GameData.Instance.map.Date);
		if (monthDifference > 0)
		{
			int num4 = 72;
			float time = Mathf.Clamp01((float)monthDifference / (float)num4);
			float num5 = researchRate.Evaluate(time);
			num2 = num * num5;
			num2 = num - num2;
			num2 *= -1f;
		}
		else if (monthDifference < 0)
		{
			num2 = Mathf.Abs((float)monthDifference / 100f);
		}
		float num6 = Mathf.Clamp((num + num2) * num3, 0.001f, 1f);
		if (p_unit.FilterType == "FilterBoat")
		{
			num6 /= 1.5f;
		}
		if (p_unit.IsPrototype)
		{
			num6 /= 1.5f;
		}
		if (p_player.HasPolicy(Policies.Type.Early_mobilization))
		{
			num6 -= num6 * 0.05f;
		}
		else if (p_player.HasPolicy(Policies.Type.Partial_mobilization))
		{
			num6 -= num6 * 0.1f;
		}
		else if (p_player.HasPolicy(Policies.Type.War_economy))
		{
			num6 -= num6 * 0.15f;
		}
		else if (p_player.HasPolicy(Policies.Type.Total_mobilization))
		{
			num6 -= num6 * 0.25f;
		}
		if (p_player.HasPolicy(Policies.Type.Better_scientists))
		{
			num6 += num6 * 0.3f;
		}
		return num6;
	}

	private void ProcessPlayerResearch(Player p_player)
	{
		if (PlayerSettings.Instance.TechTree)
		{
			for (int i = 0; i < p_player.CurrentUnitsResearch.Count; i++)
			{
				Unit unit = null;
				foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
				{
					if (listOfAllUnit.Country == p_player.Country && listOfAllUnit.Name == p_player.CurrentUnitsResearch.ElementAt(i).Key)
					{
						unit = listOfAllUnit;
						break;
					}
				}
				if (unit == null)
				{
					continue;
				}
				float num = GetResearchRate(p_player, unit);
				p_player.CurrentUnitsResearch[unit.Name] += num;
				if (!(p_player.CurrentUnitsResearch[unit.Name] < 1f) && !p_player.ListOfUnitsResearched.Contains(unit.Name))
				{
					p_player.ListOfUnitsResearched.Add(unit.Name);
					p_player.NumberOfAvailableResearchPoints++;
					new Notification(NotificationTypes.RESEARCH_COMPLETE, p_player.Name, 0f, 0f, unit.Name, p_isImportant: true).UnitResearchedFilterType = unit.FilterType;
					p_player.CurrentUnitsResearch.Remove(unit.Name);
					if (!p_player.IsComputer && !MultiplayerManager.IsRealtimeMultiplayer())
					{
						SteamManager.UnlockAchievement("Complete first research");
					}
				}
			}
		}
		UIManager.instance.RefreshTechTreeNotification();
	}

	private void ProcessTruces()
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			for (int i = 0; i < listOfPlayer.Truces.Count; i++)
			{
				string key = listOfPlayer.Truces.ElementAt(i).Key;
				listOfPlayer.Truces[key]--;
				if (listOfPlayer.Truces[key] <= 0 && GameData.Instance.TryFindPlayerByName(key, out var o_player))
				{
					listOfPlayer.RemoveTruceWith(o_player, p_sendRPC: false);
				}
			}
		}
	}

	private void ProcessEvents()
	{
		if (MultiplayerManager.IsRealtimeMultiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		List<Event> list = new List<Event>();
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (@event.FullyProcessed())
			{
				list.Add(@event);
			}
			else if ((@event.CheckType == CheckTypes.DATE || @event.CheckType == CheckTypes.TURN_COUNT) && (!@event.HistoricalFocusOnly || PlayerSettings.Instance.ISHistoricalFocus) && (@event.CheckType != CheckTypes.TURN_COUNT || GameData.Instance.map.turnCount >= @event.CheckTurnCount) && (@event.CheckType != CheckTypes.DATE || !(GameData.Instance.map.Date < @event.CheckDate)))
			{
				ProcessSingleEvent(@event);
				if (@event.FullyProcessed())
				{
					list.Add(@event);
				}
			}
		}
		foreach (Event item in list)
		{
			for (int i = 0; i < GameData.Instance.map.Events.Count; i++)
			{
				if (GameData.Instance.map.Events[i].ID == item.ID)
				{
					GameData.Instance.map.Events.RemoveAt(i);
					break;
				}
			}
		}
		MultiplayerManager.RPC("RPC_SyncEvents", GameData.Instance.map.Events);
		MultiplayerManager.RPC("RPC_SyncNotifications", GameData.Instance.map.Notifications);
	}

	public void ProcessSingleEvent(Event p_event, EventData p_eventData = null)
	{
		if ((MultiplayerManager.IsRealtimeMultiplayer() && !PhotonNetwork.IsMasterClient) || (p_event.HistoricalFocusOnly && !PlayerSettings.Instance.ISHistoricalFocus) || (p_event.CheckType == CheckTypes.TURN_COUNT && GameData.Instance.map.turnCount < p_event.CheckTurnCount) || (p_event.CheckType == CheckTypes.DATE && GameData.Instance.map.Date < p_event.CheckDate) || p_event.DontCheckBeforeTurnCount > GameData.Instance.map.turnCount)
		{
			return;
		}
		p_event.RemoveExecution();
		if (p_event.Chance != 100)
		{
			if (p_event.Chance == 0)
			{
				return;
			}
			int num = UnityEngine.Random.Range(0, 101);
			if (num == 0 || 100 - num > p_event.Chance)
			{
				return;
			}
		}
		if (GameData.Instance.TryFindPlayerByCountry(p_event.MasterPlayer, out var o_player) && ((p_event.HumansOnly && o_player.IsComputer) || (p_event.AIonly && !o_player.IsComputer)))
		{
			return;
		}
		foreach (EventAction eventAction in p_event.EventActions)
		{
			if (eventAction.Processed)
			{
				if (p_event.ExecutionsLeft <= 0)
				{
					continue;
				}
				eventAction.ResetProcess();
			}
			Player o_player3;
			if (p_event.AIonly)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player2) && !o_player2.IsComputer)
				{
					continue;
				}
			}
			else if (p_event.HumansOnly && GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out o_player3) && o_player3.IsComputer)
			{
				continue;
			}
			if (eventAction.Type == ActionTypes.SHOW_MESSAGE)
			{
				if (!GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player4) || eventAction.TargetPlayerName1 == currPlayer.Name || (humanPlayer != null && eventAction.TargetPlayerName1 == humanPlayer.Name) || (o_player4 != null && !o_player4.IsComputer && !p_event.AIonly))
				{
					UIManager.instance.ShowEventMessageWindow(eventAction.MessageText);
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.ADD_MONEY)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player5))
				{
					if (p_event.CheckType == CheckTypes.TURN_COUNT || p_event.CheckType == CheckTypes.DATE)
					{
						o_player5.EventMoneyToProcess += eventAction.MoneyAmount;
					}
					else
					{
						o_player5.AddMoney(eventAction.MoneyAmount, p_sendRPC: true);
					}
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.ADD_HQ_POINTS)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player6))
				{
					o_player6.AddHQPoints(eventAction.HQPointsAmount, p_sendRPC: true);
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.ADD_FIXED_INCOME)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player7))
				{
					o_player7.FixedIncome += eventAction.MoneyAmount;
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.SET_FIXED_INCOME)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player8))
				{
					o_player8.FixedIncome = eventAction.MoneyAmount;
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.SET_FIXED_INCOME_PERCENT)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player9))
				{
					o_player9.FixedIncome *= (int)((float)eventAction.MoneyAmount / 100f);
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.SET_FIXED_INCOME_FROM_BASE_INCOME_PERCENT)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player10))
				{
					o_player10.CalculateTurnIncome();
					int num2 = o_player10.baseIncome;
					if (o_player10.FixedIncome != 0)
					{
						num2 -= o_player10.FixedIncome;
					}
					o_player10.FixedIncome = (int)((float)num2 * ((float)eventAction.MoneyAmount / 100f));
					o_player10.CalculateTurnIncome();
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.RESET_INCOME)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player11))
				{
					if (p_event.CheckType == CheckTypes.TURN_COUNT || p_event.CheckType == CheckTypes.DATE)
					{
						o_player11.EventMoneyToProcess -= o_player11.CalculateTurnIncome();
					}
					if (_isRealtimeMultiplayer)
					{
						o_player11.RemoveMoney(o_player11.MoneyAmount, p_sendRPC: true);
					}
					else
					{
						o_player11.MoneyAmount = 0;
					}
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.SET_TILE)
			{
				if (GameData.Instance.map.FindTile(eventAction.PosX, eventAction.PosY, out var o_tile))
				{
					Tile tile = new Tile();
					tile.Name = eventAction.TileName;
					tile.Season = eventAction.TileSeason;
					if (tile.Season == "Summer" && (o_tile.Season == "Winter" || (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter && !o_tile.SeasonNeverChanges)))
					{
						tile.Season = "Winter";
						tile.enumSeason = TileSeason.WINTER;
					}
					o_tile.Name = tile.Name;
					Utils.UpdateTileProperties(o_tile, Utils.GetOriginalTile(tile), p_nonDestructive: false);
					o_tile.SetRuntimeOptimizations();
					o_tile.tileGO.SetSprite();
					eventAction.Process();
				}
				MultiplayerManager.RPC("RPC_SyncTile", o_tile.GetCoord(), o_tile, eventAction.Type);
			}
			else if (eventAction.Type == ActionTypes.ADD_PLAYER)
			{
				Player player = AddPlayer(eventAction.TargetCountryName, eventAction.TargetFactionName, p_AIcontrolled: true, eventAction.PlayerIndex);
				if (!string.IsNullOrEmpty(eventAction.TargetIdeology))
				{
					player.Ideology = eventAction.TargetIdeology;
				}
				MultiplayerManager.RPC("RPC_AddPlayer", player, eventAction.PlayerIndex);
			}
			else if (eventAction.Type == ActionTypes.REMOVE_PLAYER)
			{
				for (byte b = 0; b < GameData.Instance.listOfPlayers.Count; b++)
				{
					if (GameData.Instance.listOfPlayers[b].Name == eventAction.TargetPlayerName1)
					{
						GameData.Instance.listOfPlayers.RemoveAt(b);
						MultiplayerManager.RPC("RPC_RemovePlayer", eventAction.TargetPlayerName1);
						break;
					}
				}
			}
			else if (eventAction.Type == ActionTypes.DECLARE_WAR)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player12) && (o_player12.IsComputer || (!o_player12.IsComputer && p_event.ForceHumanApplyEvent)) && GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player13) && GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName2, out var o_player14))
				{
					UIManager.ShowRelationshipChangeMessage(o_player13.Country, o_player14.Country, LocalizationManager.Translate("relationship-war declaration"));
					if (o_player14.Faction == "Neutral")
					{
						if (o_player13.Faction == "Axis")
						{
							o_player14.Faction = "Allies";
						}
						else if (o_player13.Faction == "Allies")
						{
							o_player14.Faction = "Axis";
						}
						else if (o_player14.Ideology == "Fascism")
						{
							o_player14.Faction = "Axis";
						}
						else if (o_player14.Ideology == "Democracy" || o_player14.Ideology == "Neutral")
						{
							o_player14.Faction = "Allies";
						}
						else if (o_player14.Ideology == "Communism")
						{
							o_player14.Faction = "Communists";
						}
					}
					o_player13.AddWarWith(o_player14, p_actualDirectWarDeclaration: true, p_sendRPC: true);
					eventAction.Process();
					MultiplayerManager.RPC("RPC_WarDeclaration", o_player13.Name, o_player14.Name);
					DiplomacyChanged(p_refreshBorders: false);
				}
			}
			else if (eventAction.Type == ActionTypes.MAKE_PEACE)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player15) && GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName2, out var o_player16) && (o_player15.IsComputer || (!o_player15.IsComputer && p_event.ForceHumanApplyEvent)))
				{
					o_player15.RemoveWarWith(o_player16, p_sendRPC: true);
					eventAction.Process();
					UIManager.ShowRelationshipChangeMessage(o_player15.Country, o_player16.Country, LocalizationManager.Translate("relationship-made peace"));
					MultiplayerManager.RPC("RPC_SignedPeace", o_player15.Name, o_player16.Name);
				}
			}
			else if (eventAction.Type == ActionTypes.MAKE_PEACE_FACTION)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player17) && (o_player17.IsComputer || (!o_player17.IsComputer && p_event.ForceHumanApplyEvent)))
				{
					foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
					{
						if (listOfPlayer.Faction == eventAction.TargetFactionName)
						{
							o_player17.RemoveWarWith(listOfPlayer, p_sendRPC: true);
							UIManager.ShowRelationshipChangeMessage(o_player17.Country, listOfPlayer.Country, LocalizationManager.Translate("relationship-made peace"));
							MultiplayerManager.RPC("RPC_SignedPeace", o_player17.Name, listOfPlayer.Name);
						}
					}
					eventAction.Process();
				}
			}
			else if (eventAction.Type == ActionTypes.MAKE_ALLIANCE)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player18) && GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName2, out var o_player19) && (o_player18.IsComputer || (!o_player18.IsComputer && p_event.ForceHumanApplyEvent)))
				{
					o_player18.AddAllianceWith(o_player19, p_sendRPC: true);
					eventAction.Process();
					DiplomacyChanged(p_refreshBorders: false);
					UIManager.ShowRelationshipChangeMessage(o_player18.Country, o_player19.Country, LocalizationManager.Translate("relationship-made alliance"));
					MultiplayerManager.RPC("RPC_AllianceMade", o_player18.Name, o_player19.Name);
				}
			}
			else if (eventAction.Type == ActionTypes.FACTION_CHANGE)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player20))
				{
					if ((!o_player20.IsComputer && !p_event.ForceHumanApplyEvent) || o_player20.Faction == eventAction.TargetFactionName)
					{
						continue;
					}
					if (!MapGO.IsAIOnly && humanPlayer.IsAtWarWith(o_player20))
					{
						bool flag = false;
						if (GameData.Instance.map.IsOfficial)
						{
							if (eventAction.TargetFactionName == "Neutral")
							{
								if (humanPlayer.Country != "Finland" && o_player20.Country != "Soviet Union")
								{
									flag = true;
								}
								if (humanPlayer.Country == "Nazi Germany" && o_player20.Country == "Soviet Union")
								{
									flag = true;
								}
							}
							else if (eventAction.TargetFactionName == "Winter War")
							{
								flag = true;
							}
							else if (eventAction.TargetFactionName == "Baltic States")
							{
								flag = true;
							}
						}
						else if (eventAction.TargetFactionName == "Neutral")
						{
							flag = true;
						}
						if (flag)
						{
							eventAction.Process();
							break;
						}
					}
					o_player20.ChangeFaction(eventAction.TargetFactionName, p_getNewAllianceWars: true, p_sendRPC: true);
					eventAction.Process();
					UIManager.ShowRelationshipChangeMessage(o_player20.Country, "", LocalizationManager.Translate("relationship-joined faction") + LocalizationManager.GetLocalizedFaction(eventAction.TargetFactionName));
					DiplomacyChanged(p_refreshBorders: false);
					MultiplayerManager.Instance.RunRPC("RPC_ShowRelationshipWindow", "O", o_player20.Country, "", LocalizationManager.Translate("relationship-joined faction") + eventAction.TargetFactionName);
				}
			}
			else if (eventAction.Type == ActionTypes.ADD_UNIT_RESERVE)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player21))
				{
					Unit unit = ((eventAction.Unit == null) ? Utils.GenerateNewUnit(eventAction.UnitName, eventAction.UnitCountry) : eventAction.Unit.Clone(p_generateNewID: true));
					if (!Utils.IsUnitRegistered(unit))
					{
						UnityEngine.Debug.LogError("Unit not registered: " + unit.Name + " " + unit.Country);
						eventAction.Process();
						continue;
					}
					Utils.PatchUnit(unit);
					if (unit.MaxPurchaseTime > 0)
					{
						if (unit.IsCommander)
						{
							unit.IsWinterSpecialized = true;
						}
						if (unit.MaxPurchaseTime > 0)
						{
							if (o_player21.HasReachedMaxPurchaseLimit(unit))
							{
								eventAction.Process();
								continue;
							}
							if (o_player21.PurchasedUnits.ContainsKey(unit.Name))
							{
								o_player21.PurchasedUnits[unit.Name]++;
							}
							else
							{
								o_player21.PurchasedUnits.Add(unit.Name, 1);
							}
						}
					}
					unit.IsReserve = true;
					if (eventAction.UnitLevel > 1)
					{
						while (unit.Lvl < eventAction.UnitLevel)
						{
							unit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
						}
					}
					o_player21.AddUnit(unit, p_sendRPC: true);
					eventAction.Process();
					foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
					{
						if (listOfPlayer2.Name == eventAction.TargetPlayerName1)
						{
							new Notification(NotificationTypes.REINFORCEMENTS_PLAYER, listOfPlayer2.Name, 0f, 0f, string.Empty, p_isImportant: false);
						}
						else if (listOfPlayer2.IsAlliedWith(unit.OwnerPlayer))
						{
							new Notification(NotificationTypes.REINFORCEMENTS_ALLIES, listOfPlayer2.Name, 0f, 0f, string.Empty, p_isImportant: false);
						}
						else if (listOfPlayer2.IsAtWarWith(unit.OwnerPlayer))
						{
							new Notification(NotificationTypes.REINFORCEMENTS_ENEMIES, listOfPlayer2.Name, 0f, 0f, string.Empty, p_isImportant: true);
						}
					}
					ReservePanelGO.instance.RefreshDeployNotification();
				}
			}
			else if (eventAction.Type == ActionTypes.SPAWN_UNIT)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player22))
				{
					Unit unit2 = ((eventAction.Unit == null) ? Utils.GenerateNewUnit(eventAction.UnitName, eventAction.UnitCountry) : eventAction.Unit.Clone(p_generateNewID: true));
					if (!Utils.IsUnitRegistered(unit2))
					{
						UnityEngine.Debug.LogError("Unit not registered: " + unit2.Name + " " + unit2.Country);
						eventAction.Process();
						continue;
					}
					Utils.PatchUnit(unit2);
					if (unit2.MaxPurchaseTime > 0)
					{
						if (unit2.IsCommander)
						{
							unit2.IsWinterSpecialized = true;
						}
						if (unit2.MaxPurchaseTime > 0)
						{
							if (o_player22.HasReachedMaxPurchaseLimit(unit2))
							{
								eventAction.Process();
								continue;
							}
							if (o_player22.PurchasedUnits.ContainsKey(unit2.Name))
							{
								o_player22.PurchasedUnits[unit2.Name]++;
							}
							else
							{
								o_player22.PurchasedUnits.Add(unit2.Name, 1);
							}
						}
					}
					if (eventAction.UnitLevel > 1)
					{
						while (unit2.Lvl < eventAction.UnitLevel)
						{
							unit2.AddXP(1, p_canAddHero: false, p_showXPUI: false);
						}
					}
					if ((unit2.Type != "Plane" && GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].tileGO.groundUnitGO != null) || (unit2.Type == "Plane" && GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].tileGO.airUnitGO != null) || (unit2.Type == "Boat" && GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].enumType != TileTypes.SEA))
					{
						continue;
					}
					o_player22.AddUnit(unit2, p_sendRPC: true);
					MapGO.listOfSpawnableTiles.Add(GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].tileGO);
					GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].tileGO.SpawnUnit(unit2, p_playSound: false, p_sendRPC: true);
					eventAction.Process();
					Vector2 vector = GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY].tileGO.transform.position;
					foreach (Player listOfPlayer3 in GameData.Instance.listOfPlayers)
					{
						if (listOfPlayer3.Name == eventAction.TargetPlayerName1)
						{
							new Notification(NotificationTypes.REINFORCEMENTS_PLAYER, listOfPlayer3.Name, vector.x, vector.y, string.Empty, p_isImportant: false);
						}
						else if (listOfPlayer3.IsAlliedWith(unit2.OwnerPlayer))
						{
							new Notification(NotificationTypes.REINFORCEMENTS_ALLIES, listOfPlayer3.Name, vector.x, vector.y, string.Empty, p_isImportant: false);
						}
						else if (listOfPlayer3.IsAtWarWith(unit2.OwnerPlayer))
						{
							new Notification(NotificationTypes.REINFORCEMENTS_ENEMIES, listOfPlayer3.Name, vector.x, vector.y, string.Empty, p_isImportant: true);
						}
					}
				}
			}
			else if (eventAction.Type == ActionTypes.COUNTRY_CHANGE)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player23))
				{
					string country = o_player23.Country;
					o_player23.Country = eventAction.TargetCountryName;
					eventAction.Process();
					new Notification(NotificationTypes.COUNTRY_CHANGED, o_player23.Name, 0f, 0f, o_player23.Country, p_isImportant: true);
					UIManager.ShowRelationshipChangeMessage(country, eventAction.TargetCountryName, LocalizationManager.Translate("notif-country change") + LocalizationManager.GetLocalizedCountry(eventAction.TargetCountryName));
					for (int i = 0; i < GameData.Instance.map.SizeX; i++)
					{
						for (int j = 0; j < GameData.Instance.map.SizeY; j++)
						{
							if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OwnerName == o_player23.Name)
							{
								GameData.Instance.map.TilesTable[i, j].tileGO.SetVPFlag();
							}
						}
					}
					MapGO.instance.DrawBordersOnMap();
					foreach (Unit listOfUnit in o_player23.ListOfUnits)
					{
						if ((bool)listOfUnit.unitGO)
						{
							listOfUnit.unitGO.SetSprite();
						}
					}
					MultiplayerManager.RPC("RPC_CountryChange", o_player23.Name, o_player23.Country);
				}
			}
			else if (eventAction.Type == ActionTypes.DESTROY_TILE)
			{
				int sizeX = GameData.Instance.map.SizeX;
				int sizeY = GameData.Instance.map.SizeY;
				for (int k = 0; k < sizeX; k++)
				{
					int num3 = 0;
					while (num3 < sizeY)
					{
						if (k != eventAction.PosX || num3 != eventAction.PosY)
						{
							num3++;
							continue;
						}
						goto IL_13d0;
					}
					continue;
					IL_13d0:
					if (GameData.Instance.map.TilesTable[k, num3].IsDestroyable())
					{
						GameData.Instance.map.TilesTable[k, num3].DestroyTile(p_sendRPC: true);
					}
					eventAction.Process();
					break;
				}
			}
			else if (eventAction.Type == ActionTypes.CAPITULATION)
			{
				bool flag2 = true;
				EventData eventData = null;
				if (p_eventData == null)
				{
					flag2 = false;
					GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player24);
					eventData = new EventData(null, o_player24);
				}
				else
				{
					eventData = p_eventData.Clone();
				}
				eventData.playerLosing.NumberOfPartisansLost = 25;
				UIManager.ShowRelationshipChangeMessage(eventData.playerLosing.Country, "", LocalizationManager.GetLocalizedCountry(eventData.playerLosing.Country) + " " + LocalizationManager.Translate("relationship-capitulated"));
				if (!flag2)
				{
					eventData.playerLosing.Country = "Neutral";
				}
				MultiplayerManager.Instance.RunRPC("RPC_SyncPlayer", "O", eventData.playerLosing);
				bool flag3 = true;
				if (PlayerSettings.Instance.ISHistoricalFocus && eventData.playerLosing.Country == "Italy" && eventData.playerCapturing.Faction == "Allies")
				{
					flag3 = false;
					eventData.playerLosing.MoneyAmount = 0;
					eventData.playerLosing.DestroyAndResetAllUnits(_isRealtimeMultiplayer ? null : eventData.playerCapturing);
					eventData.playerLosing.ChangeFaction(eventData.playerCapturing.Faction, p_getNewAllianceWars: true, p_sendRPC: true);
				}
				else if (PlayerSettings.Instance.ISHistoricalFocus && eventData.playerLosing.Country == "Romania" && eventData.playerCapturing.Faction == "Communists")
				{
					flag3 = false;
					eventData.playerLosing.MoneyAmount = 0;
					eventData.playerLosing.ChangeFaction(eventData.playerCapturing.Faction, p_getNewAllianceWars: true, p_sendRPC: true);
				}
				if (flag3)
				{
					int sizeX2 = GameData.Instance.map.SizeX;
					int sizeY2 = GameData.Instance.map.SizeY;
					Tile[,] tilesTable = GameData.Instance.map.TilesTable;
					GameData.Instance.TryFindPlayerByCountry("Nazi Germany", out var o_player25);
					for (int l = 0; l < sizeX2; l++)
					{
						for (int m = 0; m < sizeY2; m++)
						{
							if (!tilesTable[l, m].IsVictoryPoint || !(tilesTable[l, m].OwnerName == eventAction.TargetPlayerName1))
							{
								continue;
							}
							if (flag2)
							{
								Player o_player26;
								if (m >= 70 && o_player25 != null && eventData.playerLosing.Country == "Vichy France" && eventData.playerCapturing.Faction == "Allies")
								{
									tilesTable[l, m].OwnerName = o_player25.Name;
									tilesTable[l, m].ownerPlayer = o_player25;
								}
								else if (GameData.Instance.TryFindPlayerByCountry(tilesTable[l, m].OriginalCountryOwner, out o_player26))
								{
									if (o_player26.IsAtWarWith(eventData.playerCapturing))
									{
										tilesTable[l, m].OwnerName = eventData.playerCapturing.Name;
										tilesTable[l, m].ownerPlayer = eventData.playerCapturing;
									}
									else
									{
										tilesTable[l, m].OwnerName = o_player26.Name;
										tilesTable[l, m].ownerPlayer = o_player26;
									}
								}
								else
								{
									tilesTable[l, m].OwnerName = eventData.playerCapturing.Name;
									tilesTable[l, m].ownerPlayer = eventData.playerCapturing;
								}
							}
							tilesTable[l, m].NumberOfTurnsSinceVPGotCaptured = 0;
							tilesTable[l, m].tileGO.SetVPFlag();
							MultiplayerManager.RPC("RPC_SyncTile", tilesTable[l, m].GetCoord(), tilesTable[l, m], eventAction.Type);
						}
					}
					if (PlayerSettings.Instance.ISHistoricalFocus && eventData.playerLosing.Country == "Vichy France" && eventData.playerCapturing.Faction == "Allies")
					{
						eventData.playerLosing.ChangeFaction("Neutral", p_getNewAllianceWars: true, p_sendRPC: true);
					}
					else
					{
						ProcessCountryCreationOnCapitulation(eventData);
					}
					eventData.playerLosing.MoneyAmount = 0;
					eventData.playerLosing.DestroyAndResetAllUnits(_isRealtimeMultiplayer ? null : eventData.playerCapturing);
				}
				eventAction.Process();
				DiplomacyChanged(p_refreshBorders: true);
				MapGO.CheckIfEndOfScenario();
				UIManager.instance.UpdateVPtext();
				MultiplayerManager.Instance.RunRPC("RPC_SyncPlayer", "O", eventData.playerLosing);
			}
			else if (eventAction.Type == ActionTypes.CHANGE_VP_OWNERSHIP)
			{
				GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player27);
				int sizeX3 = GameData.Instance.map.SizeX;
				int sizeY3 = GameData.Instance.map.SizeY;
				EventData eventData2 = null;
				for (int n = 0; n < sizeX3; n++)
				{
					int num4 = 0;
					while (num4 < sizeY3)
					{
						if (n != eventAction.PosX || num4 != eventAction.PosY || !GameData.Instance.map.TilesTable[n, num4].IsVictoryPoint)
						{
							num4++;
							continue;
						}
						goto IL_1988;
					}
					continue;
					IL_1988:
					if (p_eventData == null)
					{
						GameData.Instance.TryFindPlayerByName(GameData.Instance.map.TilesTable[n, num4].OwnerName, out var o_player28);
						eventData2 = new EventData(o_player27, o_player28);
					}
					else
					{
						eventData2 = p_eventData.Clone();
						eventData2.playerCapturing = o_player27;
					}
					if (eventAction.OriginalOwnerOnly)
					{
						if (eventData2.playerLosing.Country != GameData.Instance.map.TilesTable[n, num4].OriginalCountryOwner && eventData2.playerCapturing.Country != GameData.Instance.map.TilesTable[n, num4].OriginalCountryOwner)
						{
							eventAction.Process();
							break;
						}
						if (p_event.CheckType == CheckTypes.DATE && eventData2.playerLosing.IsAtWarWith(eventData2.playerCapturing))
						{
							eventAction.Process();
							break;
						}
					}
					GameData.Instance.map.TilesTable[n, num4].OwnerName = eventData2.playerCapturing.Name;
					GameData.Instance.map.TilesTable[n, num4].NumberOfTurnsSinceVPGotCaptured = 0;
					GameData.Instance.map.TilesTable[n, num4].tileGO.SetVPFlag();
					Vector3 position = GameData.Instance.map.TilesTable[n, num4].tileGO.transform.position;
					new Notification(NotificationTypes.TERRITORY_LOSS, eventData2.playerLosing.Name, position.x, position.y, string.Empty, p_isImportant: true);
					new Notification(NotificationTypes.TERRITORY_GAIN, eventData2.playerCapturing.Name, position.x, position.y, string.Empty, p_isImportant: true);
					if (GameData.Instance.map.TilesTable[n, num4].GroundUnit != null)
					{
						GameData.Instance.map.TilesTable[n, num4].GroundUnit.IsReserve = true;
						GameData.Instance.map.TilesTable[n, num4].GroundUnit.TurnToProduce = 2;
						if ((bool)GameData.Instance.map.TilesTable[n, num4].tileGO.groundUnitGO)
						{
							UnityEngine.Object.Destroy(GameData.Instance.map.TilesTable[n, num4].tileGO.groundUnitGO.gameObject);
						}
						MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", GameData.Instance.map.TilesTable[n, num4].GroundUnit, false);
						MultiplayerManager.Instance.RunRPC("RPC_DespawnUnit", "O", GameData.Instance.map.TilesTable[n, num4].GroundUnit.ID, GameData.Instance.map.TilesTable[n, num4].GroundUnit.OwnerName);
						GameData.Instance.map.TilesTable[n, num4].GroundUnit.unitGO = null;
						GameData.Instance.map.TilesTable[n, num4].GroundUnit = null;
					}
					if (GameData.Instance.map.TilesTable[n, num4].AirUnit != null)
					{
						GameData.Instance.map.TilesTable[n, num4].AirUnit.IsReserve = true;
						GameData.Instance.map.TilesTable[n, num4].AirUnit.TurnToProduce = 2;
						if ((bool)GameData.Instance.map.TilesTable[n, num4].tileGO.airUnitGO)
						{
							UnityEngine.Object.Destroy(GameData.Instance.map.TilesTable[n, num4].tileGO.airUnitGO.gameObject);
						}
						MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", GameData.Instance.map.TilesTable[n, num4].AirUnit, false);
						MultiplayerManager.Instance.RunRPC("RPC_DespawnUnit", "O", GameData.Instance.map.TilesTable[n, num4].AirUnit.ID, GameData.Instance.map.TilesTable[n, num4].AirUnit.OwnerName);
						GameData.Instance.map.TilesTable[n, num4].AirUnit.unitGO = null;
						GameData.Instance.map.TilesTable[n, num4].AirUnit = null;
					}
					eventAction.Process();
					MultiplayerManager.RPC("RPC_SyncTile", GameData.Instance.map.TilesTable[n, num4].GetCoord(), GameData.Instance.map.TilesTable[n, num4], eventAction.Type);
					DiplomacyChanged(p_refreshBorders: true);
					MapGO.CheckIfEndOfScenario();
					UIManager.instance.UpdateVPtext();
					break;
				}
			}
			else if (eventAction.Type == ActionTypes.ADD_VP)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var o_player29) && GameData.Instance.map.FindTile(eventAction.PosX, eventAction.PosY, out var o_tile2))
				{
					o_tile2.IsVictoryPoint = true;
					o_tile2.ownerPlayer = o_player29;
					o_tile2.OwnerName = o_player29.Name;
					o_tile2.terrainOwner = o_player29;
					o_tile2.NumberOfTurnsSinceVPGotCaptured = 0;
					o_tile2.tileGO.SetVPFlag();
					MapGO.instance.UpdateVpsList();
					DiplomacyChanged(p_refreshBorders: true);
					MapGO.CheckIfEndOfScenario();
					UIManager.instance.UpdateVPtext();
					eventAction.Process();
					MultiplayerManager.RPC("RPC_SyncTile", o_tile2.GetCoord(), o_tile2, eventAction.Type);
				}
			}
			else if (eventAction.Type == ActionTypes.REMOVE_VP)
			{
				if (GameData.Instance.TryFindPlayerByName(eventAction.TargetPlayerName1, out var _) && GameData.Instance.map.FindTile(eventAction.PosX, eventAction.PosY, out var o_tile3))
				{
					o_tile3.IsVictoryPoint = false;
					o_tile3.SecondaryObjective = false;
					o_tile3.PrimaryObjective = false;
					o_tile3.ownerPlayer = null;
					o_tile3.OwnerName = null;
					if (o_tile3.tileGO.flagGO != null)
					{
						UnityEngine.Object.Destroy(o_tile3.tileGO.flagGO);
					}
					MapGO.instance.UpdateVpsList();
					DiplomacyChanged(p_refreshBorders: true);
					MapGO.CheckIfEndOfScenario();
					UIManager.instance.UpdateVPtext();
					eventAction.Process();
					MultiplayerManager.RPC("RPC_SyncTile", o_tile3.GetCoord(), o_tile3, eventAction.Type);
				}
			}
			else if (eventAction.Type == ActionTypes.SET_TILE_NAME)
			{
				Tile tile2 = GameData.Instance.map.TilesTable[eventAction.PosX, eventAction.PosY];
				if (tile2 != null)
				{
					tile2.CityName = eventAction.CityName;
					if (string.IsNullOrEmpty(tile2.CityName))
					{
						UnityEngine.Object.Destroy(tile2.tileGO.cityNameGO);
					}
					else
					{
						tile2.tileGO.SetCityName();
					}
					eventAction.Process();
					MultiplayerManager.RPC("RPC_SyncTile", tile2.GetCoord(), tile2, eventAction.Type);
				}
			}
			if (p_event.ExecutionsLeft > 0)
			{
				eventAction.ResetProcess();
			}
		}
	}

	private Player AddPlayer(string p_country, string p_faction, bool p_AIcontrolled, int p_insertionIndex)
	{
		Player player = new Player(p_country + " (New) " + UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		player.Country = p_country;
		player.IsComputer = p_AIcontrolled;
		player.AlliedPlayers = new HashSet<string>();
		player.PlayersAtWar = new HashSet<string>();
		player.CheckForMissingUnitsCategories();
		player.SetDefaultResearchPointsAmount(MapGO.instance.listOfAllUnits);
		player.ChangeFaction(p_faction, p_getNewAllianceWars: false, p_sendRPC: true);
		player.SetupIdeology();
		GameData.Instance.listOfPlayers.Insert(p_insertionIndex, player);
		return player;
	}

	private void ProcessCountryCreationOnCapitulation(EventData p_eventData)
	{
		CreateVichyFrance(p_eventData.playerCapturing, p_eventData.playerLosing);
		CreateCroatia(p_eventData.playerCapturing, p_eventData.playerLosing);
		MolotovRibbentropPact(p_eventData.playerCapturing, p_eventData.playerLosing);
	}

	private void CreateVichyFrance(Player p_playerCapturing, Player p_playerLosing)
	{
		if (_isRealtimeMultiplayer || !PlayerSettings.Instance.ISHistoricalFocus || p_playerLosing.Country != "France" || (GameData.Instance.map.SizeX != 300 && GameData.Instance.map.SizeY != 220) || (!string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && !GameData.Instance.map.ScenarioName.ToLower().Contains("europe")) || !GameData.Instance.TryFindPlayerByCountry("Nazi Germany", out var o_player) || !p_playerCapturing.IsAlliedWith(o_player) || GameData.Instance.TryFindPlayerByCountry("Vichy France", out var _))
		{
			return;
		}
		p_playerLosing.ChangeFaction("Neutral", p_getNewAllianceWars: false, p_sendRPC: true);
		Player player = AddPlayer("Vichy France", "Axis", p_AIcontrolled: true, 7);
		Event obj = new Event(GameData.Instance.map.GetNewEventID());
		obj.Name = "Capitulation : Vichy France";
		obj.MasterPlayer = player.Country;
		obj.Group = player.Country;
		obj.Chance = 100;
		obj.ExecutionsLeft = 1;
		obj.AIonly = true;
		obj.HistoricalFocusOnly = true;
		obj.SetCheckType(CheckTypes.VPS_LOSSES);
		obj.VPsToBeLost.Add(new Coordinates(32, 37));
		obj.VPsToBeLost.Add(new Coordinates(35, 39));
		obj.VPsToBeLost.Add(new Coordinates(41, 42));
		obj.VPsToBeLost.Add(new Coordinates(43, 43));
		EventAction eventAction = new EventAction();
		eventAction.Type = ActionTypes.CAPITULATION;
		eventAction.TargetPlayerName1 = player.Name;
		obj.EventActions.Add(eventAction);
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.PosY > 101 || victoryPoint.tile.OriginalCountryOwner != p_playerLosing.Country || (victoryPoint.tile.PosX == 116 && victoryPoint.tile.PosY == 100) || (victoryPoint.tile.PosX == 94 && victoryPoint.tile.PosY == 101) || (victoryPoint.tile.PosX == 94 && victoryPoint.tile.PosY == 98) || (victoryPoint.tile.PosX == 94 && victoryPoint.tile.PosY == 94) || (victoryPoint.tile.PosX == 93 && victoryPoint.tile.PosY == 94) || (victoryPoint.tile.PosX == 94 && victoryPoint.tile.PosY == 93) || (victoryPoint.tile.PosX == 88 && victoryPoint.tile.PosY == 88))
			{
				continue;
			}
			victoryPoint.tile.OwnerName = player.Name;
			victoryPoint.tile.ownerPlayer = player;
			victoryPoint.tile.terrainOwner = player;
			if (obj.VPsToBeLost.Contains(new Coordinates(victoryPoint.tile.PosX, victoryPoint.tile.PosY)))
			{
				victoryPoint.tile.OriginalCountryOwner = player.Country;
			}
			victoryPoint.SetVPFlag();
			if (victoryPoint.tile.GroundUnit != null && victoryPoint.tile.GroundUnit.OwnerName == p_playerLosing.Name)
			{
				player.ListOfUnits.Add(victoryPoint.tile.GroundUnit);
				p_playerLosing.ListOfUnits.Remove(victoryPoint.tile.GroundUnit);
				victoryPoint.tile.GroundUnit.OwnerName = player.Name;
				victoryPoint.tile.GroundUnit.OwnerPlayer = player;
				if ((bool)victoryPoint.tile.GroundUnit.unitGO)
				{
					victoryPoint.tile.GroundUnit.unitGO.SetSprite();
				}
			}
		}
		player.SetAvailableManpower(player.CalculateMaxManpower() - player.GetFieldedManpower());
		StartCoroutine(CR_AddEventAsync(obj));
	}

	private IEnumerator CR_AddEventAsync(Event p_event)
	{
		yield return new WaitForSeconds(1f);
		GameData.Instance.map.Events.Add(p_event);
	}

	private void CreateCroatia(Player p_playerCapturing, Player p_playerLosing)
	{
		if (_isRealtimeMultiplayer || !PlayerSettings.Instance.ISHistoricalFocus || p_playerLosing.Country != "Yugoslavia" || p_playerCapturing.Faction != "Axis" || (GameData.Instance.map.SizeX != 300 && GameData.Instance.map.SizeY != 220) || (!string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && !GameData.Instance.map.ScenarioName.ToLower().Contains("europe")) || GameData.Instance.TryFindPlayerByCountry("Croatia", out var _))
		{
			return;
		}
		Player player = AddPlayer("Croatia", "Axis", p_AIcontrolled: true, 8);
		player.FixedIncome = 100;
		HashSet<(int, int)> hashSet = new HashSet<(int, int)>
		{
			(148, 91),
			(149, 92),
			(150, 89),
			(146, 84),
			(150, 84),
			(151, 84),
			(152, 84),
			(155, 87),
			(157, 89),
			(158, 83),
			(158, 84),
			(157, 81),
			(156, 80),
			(163, 82),
			(155, 78),
			(160, 78),
			(157, 74),
			(158, 74)
		};
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.PosY <= 93 && hashSet.Contains((victoryPoint.tile.PosX, victoryPoint.tile.PosY)) && !(victoryPoint.tile.OriginalCountryOwner != p_playerLosing.Country))
			{
				victoryPoint.tile.OwnerName = player.Name;
				victoryPoint.tile.ownerPlayer = player;
				victoryPoint.tile.terrainOwner = player;
				victoryPoint.SetVPFlag();
			}
		}
		player.SetAvailableManpower(player.CalculateMaxManpower() - player.GetFieldedManpower());
	}

	private void MolotovRibbentropPact(Player p_playerCapturing, Player p_playerLosing)
	{
		if (_isRealtimeMultiplayer || !PlayerSettings.Instance.ISHistoricalFocus || p_playerLosing.Country != "Poland" || (p_playerCapturing.Faction != "Axis" && p_playerCapturing.Faction != "Communists") || (GameData.Instance.map.SizeX != 300 && GameData.Instance.map.SizeY != 220) || (!string.IsNullOrEmpty(GameData.Instance.map.ScenarioName) && !GameData.Instance.map.ScenarioName.ToLower().Contains("europe")))
		{
			return;
		}
		HashSet<(int, int)> hashSet = new HashSet<(int, int)>
		{
			(182, 132),
			(181, 132),
			(182, 131),
			(176, 128),
			(177, 128),
			(181, 129),
			(174, 125),
			(174, 124),
			(182, 125),
			(183, 126),
			(176, 121),
			(176, 120),
			(183, 121),
			(183, 120),
			(179, 116),
			(180, 115),
			(183, 115),
			(177, 112),
			(176, 111),
			(177, 111),
			(182, 110),
			(174, 108),
			(179, 107)
		};
		if (!GameData.Instance.TryFindPlayerByCountry("Soviet Union", out var o_player))
		{
			return;
		}
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (!(victoryPoint.tile.OriginalCountryOwner != p_playerLosing.Country) && hashSet.Contains((victoryPoint.tile.PosX, victoryPoint.tile.PosY)))
			{
				victoryPoint.tile.OwnerName = o_player.Name;
				victoryPoint.tile.ownerPlayer = o_player;
				victoryPoint.tile.terrainOwner = o_player;
				victoryPoint.SetVPFlag();
			}
		}
	}

	private void SpawnPartisans(Player p_player)
	{
		if (!PlayerSettings.Instance.Partisans || p_player.PlayersAtWar.Count == 0 || (p_player.Country == "Italy" && GameData.Instance.map.IsCampaign && GameData.Instance.map.IsOfficial) || (MultiplayerManager.IsRealtimeMultiplayer() && !PhotonNetwork.IsMasterClient))
		{
			return;
		}
		bool flag = false;
		Unit unit = new Unit();
		foreach (Unit allUnit in Utils.GetAllUnits())
		{
			if (allUnit.IsPartisans && !(allUnit.Country != p_player.Country) && allUnit.ProductionYear <= GameData.Instance.map.Date.Year && allUnit.EndUseYear > GameData.Instance.map.Date.Year && (allUnit.MaxPurchaseTime <= 0 || !p_player.HasReachedMaxPurchaseLimit(allUnit)))
			{
				flag = true;
				unit = allUnit;
				if (UnityEngine.Random.Range(0, 3) == 0)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < GameData.Instance.map.SizeX; i++)
		{
			for (int j = 0; j < GameData.Instance.map.SizeY; j++)
			{
				if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OriginalCountryOwner == p_player.Country && GameData.Instance.map.TilesTable[i, j].ownerPlayer != null && p_player.IsAtWarWith(GameData.Instance.map.TilesTable[i, j].ownerPlayer) && !(GameData.Instance.map.TilesTable[i, j].ownerPlayer.Faction == "Neutral"))
				{
					num4++;
					if (GameData.Instance.map.TilesTable[i, j].GroundUnit != null)
					{
						num3++;
					}
					float num5 = 0f;
					if (GameData.Instance.map.TilesTable[i, j].Name.Contains("SmallHarbour"))
					{
						num5 += 0.5f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].enumType == TileTypes.HARBOUR)
					{
						num5 += 1.5f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].Name.ToLower().Contains("village"))
					{
						num5 += 0.5f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].Name.ToLower().Contains("town"))
					{
						num5 += 1.5f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].Name.ToLower().Contains("city"))
					{
						num5 += 5f;
					}
					float num6;
					if (GameData.Instance.map.TilesTable[i, j].ownerPlayer.HasPolicy(Policies.Type.Balanced_Approach))
					{
						num6 = 0.85f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].ownerPlayer.HasPolicy(Policies.Type.Aggressive_Approach))
					{
						num6 = 0.7f;
					}
					else if (GameData.Instance.map.TilesTable[i, j].ownerPlayer.HasPolicy(Policies.Type.Secret_Police))
					{
						num6 = 0.5f;
					}
					else
					{
						num6 = ((!GameData.Instance.map.TilesTable[i, j].ownerPlayer.HasPolicy(Policies.Type.Forced_labor)) ? 1f : 1.3f);
					}
					if (GameData.Instance.map.TilesTable[i, j].ownerPlayer.HasPolicy(Policies.Type.Resistance_suppresion))
					{
						num6 = 0.5f;
					}
					num += num5 * num6;
					if ((float)(int)GameData.Instance.map.TilesTable[i, j].NumberOfTurnsSinceVPGotCaptured > num2)
					{
						num2 = (int)GameData.Instance.map.TilesTable[i, j].NumberOfTurnsSinceVPGotCaptured;
					}
					list.Add(GameData.Instance.map.TilesTable[i, j]);
				}
			}
		}
		num += Mathf.Clamp(num2, 0f, 30f);
		num -= (float)(p_player.NumberOfPartisansLost * 5);
		for (int k = 0; k < num4 && k <= num3; k++)
		{
			num -= (float)UnityEngine.Random.Range(0, 2);
		}
		if (num <= 0f || (float)Utils.GetRandomNumber(100) > num)
		{
			return;
		}
		List<Tile> list2 = new List<Tile>();
		foreach (Tile item in list)
		{
			if (item.NumberOfTurnsSinceVPGotCaptured < 5)
			{
				continue;
			}
			foreach (Tile item2 in Utils.GetAllTilesInRadiusAround(item, 3))
			{
				if (item2.enumType != TileTypes.SEA && item2.enumType != TileTypes.RIVER && item2.enumType != TileTypes.MOUNTAIN && item2.tileGO.groundUnitGO == null && item2.terrainOwner != null && p_player.IsAtWarWith(item2.terrainOwner))
				{
					list2.Add(item2);
				}
			}
		}
		if (list2.Count == 0)
		{
			return;
		}
		int value = (int)Math.Round(num / 10f, MidpointRounding.AwayFromZero);
		int num7 = list.Count / 5;
		if (num7 <= 0 && list.Count > 0)
		{
			num7 = 1;
		}
		num7 = Mathf.Min(num7, list.Count);
		value = Mathf.Clamp(value, 1, num7);
		int num8 = 0;
		foreach (Unit listOfUnit in p_player.ListOfUnits)
		{
			if (listOfUnit.IsPartisans && !listOfUnit.IsReserve && listOfUnit.unitGO != null)
			{
				num8++;
			}
		}
		if (num8 > 0)
		{
			value = Mathf.Min(value, num8);
		}
		for (int l = 0; l < value; l++)
		{
			if (list2.Count == 0)
			{
				break;
			}
			int index = UnityEngine.Random.Range(0, list2.Count);
			TileGO tileGO = list2.ElementAt(index).tileGO;
			Unit unit2 = unit.Clone(p_generateNewID: true);
			unit2.Lvl = 1;
			unit2.XP = 0;
			unit2.CalculateAndSetUnitPrice();
			unit2.CalculateAndSetUpkeepValue();
			p_player.AddUnit(unit2, p_sendRPC: true);
			if (p_player.PurchasedUnits.ContainsKey(unit2.Name))
			{
				p_player.PurchasedUnits[unit2.Name]++;
			}
			else
			{
				p_player.PurchasedUnits.Add(unit2.Name, 1);
			}
			MapGO.listOfSpawnableTiles.Add(tileGO);
			tileGO.SpawnUnit(unit2, p_playSound: false, p_sendRPC: true);
			list2.RemoveAt(index);
		}
	}

	public void ResetRadarSpottings()
	{
		foreach (TileGO radarDetection in MapGO.radarDetections)
		{
			radarDetection.ClearUnitSpotted();
		}
		foreach (TileGO sonarDetection in MapGO.sonarDetections)
		{
			sonarDetection.ClearUnitSpotted();
		}
		MapGO.radarDetections.Clear();
		MapGO.sonarDetections.Clear();
	}

	private void SetNextPlayerPlaying()
	{
		GameData.Instance.map.BackUpPlayerPlaying = currPlayer.Name;
		for (int i = 0; i < GameData.Instance.listOfPlayers.Count; i++)
		{
			if (GameData.Instance.listOfPlayers[i].IsPlaying)
			{
				GameData.Instance.listOfPlayers[i].IsPlaying = false;
				GameData.Instance.listOfPlayers[(i + 1) % GameData.Instance.listOfPlayers.Count].IsPlaying = true;
				currPlayer = GameData.Instance.listOfPlayers[(i + 1) % GameData.Instance.listOfPlayers.Count];
				break;
			}
		}
		if (currPlayer.IsComputer)
		{
			AI.instance.startTurn = true;
		}
		else
		{
			AI.instance.startTurn = false;
			if (_isRealtimeMultiplayer && GameData.Instance.TryFindLocalPlayer(out var o_localPlayer))
			{
				humanPlayer = o_localPlayer;
			}
			else
			{
				humanPlayer = currPlayer;
			}
		}
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshCurrentPlayerFlag();
		if (_isRealtimeMultiplayer)
		{
			rightSideUI_panel.SetActive(currPlayer.IsLocalClientPlayer());
		}
	}

	public void ScenarioStartAnimateHourglass()
	{
		StartCoroutine(CR_RotateHourglass());
	}

	private IEnumerator CR_RotateHourglass()
	{
		if (currPlayer.IsComputer)
		{
			sablierQuiTourne_Animator.enabled = true;
			while (currPlayer.IsComputer)
			{
				sablierQuiTourne_Animator.Play("Rotating Sablier");
				yield return new WaitForSeconds(1f);
			}
			sablierQuiTourne_Animator.enabled = false;
			sablierQuiTourne_Animator.gameObject.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
		}
	}

	public void CloseEndTurnPanel()
	{
		if (string.IsNullOrWhiteSpace(currPlayer.LocalPassword))
		{
			currPlayer.LocalPassword = password_inputfield.text;
		}
		else if (currPlayer.LocalPassword != password_inputfield.text && !GameData.Instance.isPBEM)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password"));
			return;
		}
		endTurn_Panel.SetActive(value: false);
		UIManager.isUIOpen = false;
		GameData.Instance.endOfTurnPanelOpen = false;
	}

	private void ShowTurnNumberAndDate()
	{
		if (GameData.Instance.map.NumberOfTurnsToWin > 0)
		{
			turnNumber.text = GameData.Instance.map.turnCount + " / " + GameData.Instance.map.NumberOfTurnsToWin;
		}
		else
		{
			turnNumber.text = GameData.Instance.map.turnCount.ToString();
		}
		Date_Text.text = LocalizationManager.GetLocalizedDate(GameData.Instance.map.Date);
	}

	private void ManageNotifications()
	{
		NotificationButtonGO[] componentsInChildren = NotificationManager.instance.parent.GetComponentsInChildren<NotificationButtonGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		if (!MainMenu.isLoadedGame)
		{
			List<Notification> list = new List<Notification>();
			foreach (Notification notification in GameData.Instance.map.Notifications)
			{
				if (notification.HasBeenShown)
				{
					list.Add(notification);
				}
			}
			foreach (Notification item in list)
			{
				GameData.Instance.map.Notifications.Remove(item);
			}
		}
		bool flag = !currPlayer.IsComputer;
		if (MultiplayerManager.IsRealtimeMultiplayer() && !currPlayer.IsLocalClientPlayer())
		{
			flag = false;
		}
		foreach (Notification notification2 in GameData.Instance.map.Notifications)
		{
			if (notification2.OwnerName == currPlayer.Name)
			{
				notification2.HasBeenShown = true;
				if (flag)
				{
					NotificationManager.instance.CreateNewUInotification(notification2);
				}
			}
		}
	}

	private void DoMapFogOfWar()
	{
		MapGO.PlaceFogOfWarEverywhere();
		MapGO.RemoveFogOfWarAroundVictoryPoints();
		MapGO.ProcessFogOfWar();
		MapGO.RemoveFogOfWarAroundUnits();
		if (MainMenu.isLoadedGame)
		{
			MapGO.RemovePlayerSavedFOW();
		}
		ManageMoneyAndDestroyedTiles();
		MapGO.instance.DrawBordersOnMap();
	}

	private void ManageMoneyAndDestroyedTiles()
	{
		if (GameData.Instance.map.turnCount > 1 && !MainMenu.isLoadedGame)
		{
			short num = (short)((currPlayer.PlayersAtWar.Count != 0) ? 5 : 3);
			if (currPlayer.HasPolicy(Policies.Type.Commanders_development_program))
			{
				num += 3;
			}
			if (currPlayer.HasPolicy(Policies.Type.Military_scientists_1))
			{
				num--;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Military_scientists_1, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Military_scientists_2))
			{
				num -= 2;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Military_scientists_2, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Aggressive_propaganda))
			{
				num--;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Aggressive_propaganda, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Resistance_suppresion))
			{
				num -= 2;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Resistance_suppresion, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Army_training))
			{
				num -= 2;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Army_training, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Bigger_command_centers))
			{
				num--;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Bigger_command_centers, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Mutual_Support_Pact))
			{
				num -= 3;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Mutual_Support_Pact, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Strategic_Flexibility))
			{
				num--;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Strategic_Flexibility, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.War_economy))
			{
				num--;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.War_economy, p_sendRPC: false);
				}
			}
			if (currPlayer.HasPolicy(Policies.Type.Total_mobilization))
			{
				num -= 2;
				if (num < 0)
				{
					currPlayer.RemovePolicy(Policies.Type.Total_mobilization, p_sendRPC: false);
				}
			}
			num = (short)Mathf.Clamp(num, 0, 32767);
			currPlayer.AddHQPoints(num, p_sendRPC: false);
			currPlayer.AddNewTurnManpower();
			int p_amount = currPlayer.CalculateTurnIncome() + currPlayer.EventMoneyToProcess;
			currPlayer.AddMoney(p_amount, p_sendRPC: false);
			currPlayer.EventMoneyToProcess = 0;
		}
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (!victoryPoint.tile.IsVictoryPoint || !(victoryPoint.tile.OwnerName == currPlayer.Name))
			{
				continue;
			}
			MapGO.currentPlayerVPs.Add(victoryPoint);
			if (victoryPoint.tile.NumberOfTurnsAtLowProd <= 0 || MainMenu.isLoadedGame)
			{
				continue;
			}
			victoryPoint.tile.NumberOfTurnsAtLowProd--;
			if (victoryPoint.tile.NumberOfTurnsAtLowProd > 0 && victoryPoint.tile.ownerPlayer != null && victoryPoint.tile.ownerPlayer.HasPolicy(Policies.Type.Efficient_workers))
			{
				victoryPoint.tile.NumberOfTurnsAtLowProd--;
			}
			if (victoryPoint.tile.NumberOfTurnsAtLowProd <= 0)
			{
				victoryPoint.tile.NumberOfTurnsAtLowProd = 0;
				if (victoryPoint.tile.Name.Contains("destroyed"))
				{
					victoryPoint.tile.Name = victoryPoint.tile.Name.Substring(0, victoryPoint.tile.Name.Length - 10);
				}
				victoryPoint.SetSprite();
			}
		}
		MapGO.RecountCapturedMajorVPs();
		UIManager.instance.UpdateVPtext();
	}
}
