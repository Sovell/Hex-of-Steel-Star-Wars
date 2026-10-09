using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReservePanelGO : MonoBehaviour
{
	public static ReservePanelGO instance;

	public static Unit selectedUnit;

	[InspectorName("Reserve panel")]
	public Transform unitParent;

	public Animator reservePanel_Animator;

	public GameObject parent;

	public GameObject notification;

	public Text notification_text;

	public GameObject unitsInProductionNotification;

	public GameObject reservePanel;

	public UnitReserveSlotGO deploySlotPrefab;

	public GameObject countdownPrefab;

	[InspectorName("Transfer")]
	public GameObject transferPanel;

	public GameObject transferPlayerPrefab;

	private Dictionary<TileGO, int> _countdownsData;

	private List<GameObject> _countdownsGameObjects;

	private List<UnitReserveSlotGO> _deploySlotGOs;

	private void Awake()
	{
		instance = this;
		selectedUnit = null;
		_countdownsData = new Dictionary<TileGO, int>();
		_countdownsGameObjects = new List<GameObject>();
		_deploySlotGOs = new List<UnitReserveSlotGO>();
	}

	private void Start()
	{
		reservePanel.SetActive(value: false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && reservePanel.activeSelf)
		{
			ManageReserveMenu();
			DestroyAllCountdowns();
		}
		if (!MainMenu.isEditor && reservePanel.activeSelf)
		{
			if (Input.GetMouseButton(1))
			{
				transferPanel.SetActive(value: false);
			}
			if (Input.GetMouseButton(0) && !RectTransformUtility.RectangleContainsScreenPoint(transferPanel.GetComponent<RectTransform>(), Input.mousePosition))
			{
				transferPanel.SetActive(value: false);
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			DestroyAllCountdowns();
		}
	}

	private void DestroyAllCountdowns()
	{
		if (_countdownsData.Count > 0)
		{
			for (int i = 0; i < _countdownsGameObjects.Count; i++)
			{
				UnityEngine.Object.Destroy(_countdownsGameObjects[i]);
			}
		}
		_countdownsGameObjects = new List<GameObject>();
		_countdownsData = new Dictionary<TileGO, int>();
	}

	public void SelectNextUnitToDeploy()
	{
		UnitReserveSlotGO[] componentsInChildren = parent.transform.GetComponentsInChildren<UnitReserveSlotGO>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			GetUnit(componentsInChildren.ElementAt(0).unit);
			EventSystem.current.SetSelectedGameObject(componentsInChildren.ElementAt(0).gameObject);
		}
	}

	public void RefreshDeployNotification()
	{
		if (MainMenu.isEditor)
		{
			return;
		}
		if (TurnManager.currPlayer.IsComputer)
		{
			notification.SetActive(value: false);
			unitsInProductionNotification.SetActive(value: false);
			notification_text.text = "";
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (!listOfUnit.IsReserve)
			{
				continue;
			}
			if (PlayerSettings.Instance.ProductionTakesTime)
			{
				if (listOfUnit.TurnToProduce == 0)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
			else
			{
				num++;
			}
		}
		if (num > 0)
		{
			notification_text.text = num.ToString();
			notification.SetActive(value: true);
		}
		else if (num2 > 0)
		{
			unitsInProductionNotification.SetActive(value: true);
			notification.SetActive(value: false);
		}
		else
		{
			notification.SetActive(value: false);
			unitsInProductionNotification.SetActive(value: false);
		}
	}

	public void GetUnit(Unit p_unit)
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MainMenu.isEditor)
		{
			if (MapGO.selectedUnitGO != null)
			{
				MapGO.instance.UnselectUnit();
			}
			DestroyAllCountdowns();
		}
		selectedUnit = p_unit;
		PrepareSpawningZone(p_unit);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
	}

	public void EmptyButtonsList()
	{
		foreach (UnitReserveSlotGO deploySlotGO in _deploySlotGOs)
		{
			deploySlotGO.gameObject.SetActive(value: false);
		}
	}

	public void ManageReserveMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!MainMenu.isEditor)
		{
			transferPanel.SetActive(value: false);
		}
		if (reservePanel.activeSelf)
		{
			CloseReservePanel();
		}
		else
		{
			OpenReservePanel();
		}
	}

	public void OpenReservePanel()
	{
		LeanTween.cancel(reservePanel);
		RectTransform rt = reservePanel.GetComponent<RectTransform>();
		float num = Mathf.Abs(0f - rt.anchoredPosition.y);
		float time = 0.5f * (num / rt.rect.height);
		LeanTween.moveY(rt, 0f, time).setEase(LeanTweenType.easeOutExpo).setOnStart(() =>
		{
			reservePanel.SetActive(value: true);
		})
			.setOnComplete(() =>
			{
				Vector2 anchoredPosition = rt.anchoredPosition;
				rt.anchoredPosition = new Vector2(anchoredPosition.x, 0f);
			});
		GenerateButtonsList();
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		if (!MainMenu.isEditor && OrderOfBattleGO.instance.OOBPanel.activeSelf)
		{
			StartCoroutine(OrderOfBattleGO.instance.CloseOOB());
		}
		TutorialManager.PerformAction(TutorialActions.OPEN_RESERVE_MENU);
	}

	public void CloseReservePanel()
	{
		LeanTween.cancel(reservePanel);
		DestroyAllCountdowns();
		if (!MainMenu.isEditor)
		{
			MapGO.instance.DestroyPotentialTilesAboveMap();
			if (MapGO.selectedUnitGO != null)
			{
				MapGO.selectedUnitGO.RemoveFogOfWarAroundUnit();
			}
		}
		selectedUnit = null;
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		RectTransform rt = reservePanel.GetComponent<RectTransform>();
		float targetY = 0f - rt.rect.height;
		float num = Mathf.Abs(rt.anchoredPosition.y - targetY);
		float time = 0.5f * (num / rt.rect.height);
		LeanTween.moveY(rt, targetY, time).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
		{
			Vector2 anchoredPosition = rt.anchoredPosition;
			rt.anchoredPosition = new Vector2(anchoredPosition.x, targetY);
			reservePanel.SetActive(value: false);
			EmptyButtonsList();
		});
	}

	public void ShowAllPlayersWeCanGiveUnitTo()
	{
		SoundManager.instance.PlayUIClickSound();
		transferPanel.SetActive(!transferPanel.activeSelf);
	}

	public void ManageTransferPanel(Unit p_unit)
	{
		SoundManager.instance.PlayUIClickSound();
		RectTransform component = transferPanel.GetComponent<RectTransform>();
		Transform[] componentsInChildren = transferPanel.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != transferPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		int num = 0;
		foreach (Player player in GameData.Instance.listOfPlayers)
		{
			if (player.Name != TurnManager.currPlayer.Name && !player.isAtWarWithCurrentPlayer)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(transferPlayerPrefab, transferPanel.transform);
				gameObject.GetComponent<Button>().onClick.AddListener(() =>
				{
					TransferUnit(p_unit, player);
				});
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(player.Country);
				gameObject.name = player.Name;
				num++;
				transferPanel.SetActive(value: true);
			}
		}
		if (transferPanel.TryGetComponent<GridLayoutGroup>(out var component2))
		{
			component2.constraintCount = Mathf.Clamp(num / 2, 1, 3);
		}
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(component);
		if (Input.mousePosition.y > (float)(Screen.height / 2))
		{
			component.pivot = new Vector2(0f, 1f);
		}
		else
		{
			component.pivot = new Vector2(0f, 0f);
		}
		transferPanel.transform.position = new Vector3(Input.mousePosition.x + 50f, Input.mousePosition.y);
		Vector3 position = transferPanel.transform.position;
		float num2 = component.rect.width * component.lossyScale.x;
		float num3 = component.rect.height * component.lossyScale.y;
		float num4 = 0f;
		float num5 = Screen.width;
		float num6 = Screen.height;
		float num7 = 0f;
		if (position.x + num2 > num5)
		{
			position.x = num5 - num2;
		}
		if (position.x < num4)
		{
			position.x = num4;
		}
		if (position.y + num3 > num6)
		{
			position.y = num6 - num3;
		}
		if (position.y < num7)
		{
			position.y = num7;
		}
		transferPanel.transform.position = position;
	}

	public void TransferUnit(Unit p_unit, Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (p_unit.IsCoreUnit && p_player.CountCurrCoreUnits() + 1 > Utils.GetMaxCoreUnitsNumber(p_player))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-transfer core unit"));
			return;
		}
		if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO == p_unit.unitGO)
		{
			MapGO.instance.UnselectUnit();
		}
		transferPanel.SetActive(value: false);
		p_unit.IsReserve = true;
		if (p_unit.TurnToProduce <= 1)
		{
			p_unit.SetRedeploymentTime();
		}
		p_unit.Waypoints = new List<int[]>();
		MultiplayerManager.RPC("RPC_TransferUnit", p_unit, p_player.Name);
		TurnManager.currPlayer.RemoveUnit(p_unit, p_sendRPC: false);
		p_player.AddUnit(p_unit, p_sendRPC: false);
		new Notification(NotificationTypes.UNIT_TRANSFER, p_player.Name, 0f, 0f, p_unit.Name, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		if (!TurnManager.currPlayer.IsComputer && SteamAPI.IsSteamRunning())
		{
			if (p_unit.IsPrototype)
			{
				SteamManager.UnlockAchievement("Sharing secrets");
			}
			SteamManager.UnlockAchievement("Generous benefactor");
		}
		selectedUnit = null;
		MapGO.instance.DestroyPotentialTilesAboveMap();
		GenerateButtonsList();
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		RefreshDeployNotification();
		TutorialManager.PerformAction(TutorialActions.TRANSFER_UNIT);
	}

	public void DisbandUnit(Unit p_unit)
	{
		if (TurnManager.currPlayer == null)
		{
			return;
		}
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Unfield unit"));
		if (MainMenu.isEditor)
		{
			if (p_unit.IsKillGoal)
			{
				GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Remove(p_unit);
			}
		}
		else
		{
			if (p_unit.IsKillGoal)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-cannot delete kill goal"));
				return;
			}
			if (p_unit.IsCommander)
			{
				MapGO.listOfAllGeneralsOnTheMap.Remove(p_unit.unitGO);
			}
			float num = (float)p_unit.CurrHP / (float)p_unit.MaxHP;
			if (float.IsNaN(num))
			{
				num = 1f;
			}
			p_unit.CalculateAndSetUnitPrice();
			int num2 = (int)Math.Round((float)p_unit.Cost * num, MidpointRounding.AwayFromZero);
			if (p_unit.NumberOfTimesTheUnitMoved > 0 || (p_unit.XP > 0 && p_unit.Lvl == 1) || p_unit.CurrHP != p_unit.MaxHP)
			{
				num2 /= 2;
			}
			TurnManager.currPlayer.AddManpower((int)Math.Round((float)p_unit.manpowerCost * num, MidpointRounding.AwayFromZero), p_doMarginCheck: false, p_sendRPC: true);
			TurnManager.currPlayer.AddMoney(num2, p_sendRPC: true);
		}
		if (TurnManager.currPlayer.PurchasedUnits.ContainsKey(p_unit.Name) && TurnManager.currPlayer.PurchasedUnits[p_unit.Name] > 0)
		{
			TurnManager.currPlayer.PurchasedUnits[p_unit.Name]--;
		}
		TurnManager.currPlayer.RemoveUnit(p_unit, p_sendRPC: true);
		GenerateButtonsList();
		selectedUnit = null;
		if (!MainMenu.isEditor)
		{
			MapGO.instance.DestroyPotentialTilesAboveMap();
			UIManager.instance.RefreshCoreUnits();
			UIManager.instance.RefreshManpower();
			UIManager.instance.HideTooltip();
			UIManager.instance.RefreshIncomePerTurnUI();
		}
		else
		{
			UIManager.HideToolTip();
		}
		RefreshDeployNotification();
		TutorialManager.PerformAction(TutorialActions.DISBAND_UNIT);
	}

	public void GenerateButtonsList()
	{
		EmptyButtonsList();
		if (TurnManager.currPlayer == null)
		{
			return;
		}
		TurnManager.currPlayer.ListOfUnits = TurnManager.currPlayer.ListOfUnits.OrderBy((Unit x) => x.TurnToProduce).ToList();
		List<Unit> list = new List<Unit>();
		for (int num = 0; num < TurnManager.currPlayer.ListOfUnits.Count; num++)
		{
			if (TurnManager.currPlayer.ListOfUnits[num].IsReserve)
			{
				list.Add(TurnManager.currPlayer.ListOfUnits[num]);
			}
		}
		if (_deploySlotGOs.Count < list.Count)
		{
			for (int num2 = _deploySlotGOs.Count; num2 < list.Count; num2++)
			{
				UnitReserveSlotGO unitReserveSlotGO = UnityEngine.Object.Instantiate(deploySlotPrefab, parent.transform);
				_deploySlotGOs.Add(unitReserveSlotGO);
				unitReserveSlotGO.transform.localScale = Vector3.one;
			}
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			Unit unit = list[num3];
			_deploySlotGOs[num3].unit = list[num3];
			_deploySlotGOs[num3].SetUpSlot();
			_deploySlotGOs[num3].slot_button.onClick.AddListener(() =>
			{
				GetUnit(unit);
			});
			_deploySlotGOs[num3].transfer_button.onClick.AddListener(() =>
			{
				ManageTransferPanel(unit);
			});
			_deploySlotGOs[num3].delete_button.onClick.AddListener(() =>
			{
				DisbandUnit(unit);
			});
			_deploySlotGOs[num3].upgrade_button.onClick.AddListener(() =>
			{
				UpgradeUnit(unit);
			});
			_deploySlotGOs[num3].transform.localScale = Vector3.one;
			_deploySlotGOs[num3].gameObject.SetActive(value: true);
		}
		Canvas.ForceUpdateCanvases();
	}

	private void UpgradeUnit(Unit p_unit)
	{
		UIManager.instance.OpenUpgradePanel(p_unit);
	}

	public void PrepareSpawningZone(Unit p_unit)
	{
		if (MainMenu.isEditor)
		{
			return;
		}
		MapGO.instance.DestroyPotentialTilesAboveMap();
		MapGO.listOfSpawnableTiles = new HashSet<TileGO>();
		_countdownsData = new Dictionary<TileGO, int>();
		if (!MainMenu.isEditor && PlayerSettings.Instance.ProductionTakesTime && p_unit.TurnToProduce > 0)
		{
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.CacheAllianceAndWarForGivenPlayer(TurnManager.currPlayer);
		}
		bool flag = TurnManager.currPlayer.HasPolicy(Policies.Type.Mutual_Support_Pact);
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (p_unit.BuiltByEngineers || p_unit.IsMissile || p_unit.CoastalBattery)
			{
				break;
			}
			if (victoryPoint.tile.ownerPlayer == null)
			{
				continue;
			}
			if (flag)
			{
				if (!victoryPoint.tile.ownerPlayer.isAlliedWithGivenPlayer)
				{
					continue;
				}
			}
			else if (victoryPoint.tile.ownerPlayer.Country != TurnManager.currPlayer.Country)
			{
				continue;
			}
			if ((!string.IsNullOrEmpty(victoryPoint.tile.CityName) && victoryPoint.tile.CityName.Contains("*")) || (PlayerSettings.Instance.ISHistoricalFocus && GameData.Instance.map.IsOfficial && victoryPoint.tile.IsReinforcementTile && victoryPoint.tile.enumType == TileTypes.SEA && TurnManager.currPlayer.IsComputer && p_unit.FilterType != "FilterBoat" && TurnManager.currPlayer.Country != "USA" && TurnManager.currPlayer.Country != "Great Britain" && TurnManager.currPlayer.Country != "Canada" && TurnManager.currPlayer.Country != "Japan") || (TurnManager.currPlayer.IsComputer && AI.instance.IsEuropeWithHistoricalFocus() && victoryPoint.tile.PosY > 67 && victoryPoint.tile.enumType == TileTypes.SEA && GameData.Instance.map.Date < new DateTime(1944, 5, 1) && p_unit.Type != "Boat" && (p_unit.Type != "Plane" || p_unit.IsParatrooper)))
			{
				continue;
			}
			int num = victoryPoint.tile.GetMinimumOccupancyForWork() - victoryPoint.tile.NumberOfTurnsSinceVPGotCaptured;
			int numberOfTurnsAtLowProd = victoryPoint.tile.NumberOfTurnsAtLowProd;
			bool flag2 = false;
			int value = 0;
			if ((numberOfTurnsAtLowProd > 0 || num > 0) && (victoryPoint.tile.enumType == TileTypes.CITY || victoryPoint.tile.enumType == TileTypes.HARBOUR || victoryPoint.tile.enumType == TileTypes.AIRPORT || victoryPoint.tile.enumType == TileTypes.FACTORY))
			{
				flag2 = true;
				value = Mathf.Max(numberOfTurnsAtLowProd, num);
			}
			HashSet<TileGO> hashSet = new HashSet<TileGO>();
			if (p_unit.Type == "Boat")
			{
				if (TurnManager.currPlayer.IsComputer && !victoryPoint.tile.isValidHarbourForAI && (victoryPoint.tile.enumType == TileTypes.SEA || victoryPoint.tile.enumType == TileTypes.HARBOUR))
				{
					continue;
				}
				if (p_unit.IsPatrolBoat && (victoryPoint.tile.IsReinforcementTile || victoryPoint.tile.enumType == TileTypes.FACTORY || victoryPoint.tile.enumType == TileTypes.HARBOUR || victoryPoint.tile.Name.ToLower().Contains("city") || victoryPoint.tile.Name.ToLower().Contains("town")))
				{
					foreach (Tile neighbour in victoryPoint.tile.neighbours)
					{
						if (CanDeployUnitToTile(p_unit, neighbour))
						{
							hashSet.Add(neighbour.tileGO);
						}
					}
				}
				if (!victoryPoint.tile.IsReinforcementTile && (victoryPoint.tile.enumType != TileTypes.HARBOUR || victoryPoint.tile.Name.Contains("SmallHarbour")))
				{
					continue;
				}
				foreach (Tile neighbour2 in victoryPoint.tile.neighbours)
				{
					if (CanDeployUnitToTile(p_unit, neighbour2))
					{
						hashSet.Add(neighbour2.tileGO);
					}
				}
			}
			else if (p_unit.Type == "Plane")
			{
				if (!victoryPoint.tile.IsReinforcementTile && victoryPoint.tile.enumType != TileTypes.AIRPORT && !p_unit.Name.Contains("balloon"))
				{
					continue;
				}
				foreach (Tile neighbour3 in victoryPoint.tile.neighbours)
				{
					if (CanDeployUnitToTile(p_unit, neighbour3))
					{
						hashSet.Add(neighbour3.tileGO);
					}
				}
				if (victoryPoint.airUnitGO == null)
				{
					hashSet.Add(victoryPoint);
				}
			}
			else if (p_unit.IsArmouredTrain)
			{
				if (victoryPoint.tile.enumType != TileTypes.CITY && victoryPoint.tile.enumType != TileTypes.HARBOUR && victoryPoint.tile.enumType != TileTypes.FACTORY)
				{
					continue;
				}
				foreach (Tile neighbour4 in victoryPoint.tile.neighbours)
				{
					if (CanDeployUnitToTile(p_unit, neighbour4))
					{
						hashSet.Add(neighbour4.tileGO);
					}
				}
			}
			else
			{
				if (!TurnManager.currPlayer.IsComputer && p_unit.Name.ToLower().Contains("marines") && (victoryPoint.tile.IsReinforcementTile || victoryPoint.tile.enumType == TileTypes.FACTORY || victoryPoint.tile.enumType == TileTypes.HARBOUR || victoryPoint.tile.Name.ToLower().Contains("city") || victoryPoint.tile.Name.ToLower().Contains("town")))
				{
					foreach (Tile neighbour5 in victoryPoint.tile.neighbours)
					{
						if (neighbour5.tileGO.groundUnitGO == null && neighbour5.enumType == TileTypes.SEA)
						{
							hashSet.Add(neighbour5.tileGO);
						}
					}
				}
				if (!victoryPoint.tile.IsReinforcementTile && (victoryPoint.tile.enumType == TileTypes.SEA || (victoryPoint.tile.enumType != TileTypes.CITY && victoryPoint.tile.enumType != TileTypes.HARBOUR && victoryPoint.tile.enumType != TileTypes.FACTORY)))
				{
					continue;
				}
				if (!GameData.Instance.map.IsCampaign || GameData.Instance.map.turnCount != 1 || !p_unit.IsCoreUnit)
				{
					if (victoryPoint.tile.Name.ToLower().Contains("town") || (victoryPoint.tile.enumType == TileTypes.HARBOUR && !victoryPoint.tile.Name.Contains("SmallHarbour")))
					{
						if (p_unit.IsConsideredHeavyEquipment())
						{
							continue;
						}
					}
					else if ((victoryPoint.tile.Name.ToLower().Contains("village") || victoryPoint.tile.Name == "DesertVillage" || victoryPoint.tile.Name.Contains("SmallHarbour") || victoryPoint.tile.Name == "JungleVillage") && p_unit.Name != "Militia" && p_unit.Name != "Conscript" && p_unit.Name != "Volkssturm" && !p_unit.Name.Contains("Garrison"))
					{
						continue;
					}
					if (victoryPoint.tile.enumType == TileTypes.FACTORY && p_unit.FilterType == "FilterInfantry")
					{
						continue;
					}
				}
				if (IsTileBesieged(victoryPoint.tile, p_unit))
				{
					continue;
				}
				foreach (Tile neighbour6 in victoryPoint.tile.neighbours)
				{
					if (CanDeployUnitToTile(p_unit, neighbour6, victoryPoint.tile.IsReinforcementTile))
					{
						hashSet.Add(neighbour6.tileGO);
					}
				}
				if (victoryPoint.groundUnitGO == null)
				{
					hashSet.Add(victoryPoint);
				}
			}
			if (hashSet.Count <= 0)
			{
				continue;
			}
			if (flag2)
			{
				if (!_countdownsData.ContainsKey(victoryPoint))
				{
					_countdownsData.Add(victoryPoint, value);
				}
				continue;
			}
			foreach (TileGO item in hashSet)
			{
				MapGO.listOfSpawnableTiles.Add(item);
			}
		}
		if (p_unit.IsCatapultPlane)
		{
			foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
			{
				if (listOfUnit.CanCatapultPlanes && !listOfUnit.IsReserve && listOfUnit.unitGO != null && listOfUnit.unitGO.tileGO.airUnitGO == null)
				{
					MapGO.listOfSpawnableTiles.Add(listOfUnit.unitGO.tileGO);
				}
			}
		}
		if (p_unit.CanBeRefueledOnCarrier)
		{
			foreach (Unit listOfUnit2 in TurnManager.currPlayer.ListOfUnits)
			{
				if (!listOfUnit2.IsCarrier || listOfUnit2.IsReserve || !(listOfUnit2.unitGO != null))
				{
					continue;
				}
				foreach (Tile item2 in Utils.GetAllTilesInRadiusAround(listOfUnit2.unitGO.tileGO.tile, 1))
				{
					if (item2.tileGO.airUnitGO == null)
					{
						MapGO.listOfSpawnableTiles.Add(item2.tileGO);
					}
				}
			}
		}
		if (p_unit.IsMissile)
		{
			foreach (Unit listOfUnit3 in TurnManager.currPlayer.ListOfUnits)
			{
				if (!listOfUnit3.IsLaunchSite || listOfUnit3.IsOnBoat || listOfUnit3.IsTrain || listOfUnit3.IsReserve || !(listOfUnit3.unitGO != null))
				{
					continue;
				}
				if (listOfUnit3.unitGO.tileGO.airUnitGO == null)
				{
					MapGO.listOfSpawnableTiles.Add(listOfUnit3.unitGO.tileGO);
				}
				if (!(listOfUnit3.Type == "Plane"))
				{
					continue;
				}
				foreach (Tile neighbour7 in listOfUnit3.unitGO.tileGO.tile.neighbours)
				{
					if (neighbour7.tileGO.airUnitGO == null)
					{
						MapGO.listOfSpawnableTiles.Add(neighbour7.tileGO);
					}
				}
			}
		}
		if (p_unit.BuiltByEngineers)
		{
			if (p_unit.Type == "Boat")
			{
				foreach (Unit listOfUnit4 in TurnManager.currPlayer.ListOfUnits)
				{
					if (listOfUnit4.HasBuilt || listOfUnit4.HasAttacked || listOfUnit4.IsReserve || listOfUnit4.ActionPoints <= 0 || listOfUnit4.unitGO == null || (p_unit.IsMine && !listOfUnit4.IsDestroyer))
					{
						continue;
					}
					foreach (Tile neighbour8 in listOfUnit4.unitGO.tileGO.tile.neighbours)
					{
						if (neighbour8.enumType == TileTypes.SEA && neighbour8.tileGO.groundUnitGO == null && (!neighbour8.IsVictoryPoint || !neighbour8.IsReinforcementTile))
						{
							neighbour8.tileGO.unitGOmakingThisTileAvailableForDeployment = listOfUnit4.unitGO;
							MapGO.listOfSpawnableTiles.Add(neighbour8.tileGO);
						}
					}
				}
			}
			else
			{
				bool flag3 = p_unit.Name.ToLower().Contains("bunker");
				foreach (Unit listOfUnit5 in TurnManager.currPlayer.ListOfUnits)
				{
					if (!listOfUnit5.IsEngineers || listOfUnit5.IsOnBoat || listOfUnit5.IsTrain || listOfUnit5.IsReserve || listOfUnit5.HasAttacked || listOfUnit5.ActionPoints <= 0 || listOfUnit5.HasBuilt || !(listOfUnit5.unitGO != null))
					{
						continue;
					}
					bool flag4 = false;
					foreach (Tile neighbour9 in listOfUnit5.unitGO.tileGO.tile.neighbours)
					{
						if (neighbour9.tileGO.groundUnitGO != null && neighbour9.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithGivenPlayer)
						{
							flag4 = true;
							break;
						}
					}
					if (flag4)
					{
						continue;
					}
					if (TurnManager.currPlayer.IsComputer)
					{
						if (p_unit.IsSupplyDepot)
						{
							bool supplyDepotNeeded = AI.instance.supplyDepotNeeded;
							AI.instance.supplyDepotNeeded = false;
							listOfUnit5.unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
							if (!AI.instance.supplyDepotNeeded)
							{
								AI.instance.supplyDepotNeeded = supplyDepotNeeded;
								continue;
							}
							AI.instance.supplyDepotNeeded = supplyDepotNeeded;
						}
						if (p_unit.IsLaunchSite)
						{
							bool flag5 = false;
							foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(listOfUnit5.unitGO.tileGO.tile, 3))
							{
								if (item3.GroundUnit != null && item3.GroundUnit.IsLaunchSite)
								{
									flag5 = true;
									break;
								}
							}
							if (flag5)
							{
								continue;
							}
						}
					}
					foreach (Tile neighbour10 in listOfUnit5.unitGO.tileGO.tile.neighbours)
					{
						if ((TurnManager.currPlayer.IsComputer && !p_unit.IsSupplyDepot && neighbour10.IsVictoryPoint && (neighbour10.enumType == TileTypes.CITY || neighbour10.enumType == TileTypes.AIRPORT || neighbour10.enumType == TileTypes.FACTORY || neighbour10.enumType == TileTypes.HARBOUR)) || (flag3 && neighbour10.terrainOwner != null && !neighbour10.terrainOwner.isAlliedWithCurrentPlayer))
						{
							continue;
						}
						neighbour10.tileGO.unitGOmakingThisTileAvailableForDeployment = listOfUnit5.unitGO;
						if (neighbour10.enumType == TileTypes.SEA || neighbour10.enumType == TileTypes.MOUNTAIN || (!(neighbour10.tileGO.groundUnitGO == null) && (!p_unit.TileBuilding || !listOfUnit5.OwnerPlayer.IsAlliedWith(neighbour10.tileGO.groundUnitGO.unit.OwnerPlayer))))
						{
							continue;
						}
						if (p_unit.TileBuilding)
						{
							if (!neighbour10.IsVictoryPoint && (neighbour10.terrainOwner == null || p_unit.Name == "Trenches" || neighbour10.terrainOwner.isAlliedWithGivenPlayer) && neighbour10.enumType != TileTypes.RIVER && neighbour10.enumType != TileTypes.SEA && neighbour10.enumType != TileTypes.MOUNTAIN && !neighbour10.Bridge)
							{
								MapGO.listOfSpawnableTiles.Add(neighbour10.tileGO);
							}
						}
						else
						{
							if ((neighbour10.IsVictoryPoint && neighbour10.ownerPlayer.isAtWarWithCurrentPlayer) || (neighbour10.terrainOwner != null && !neighbour10.terrainOwner.isAlliedWithGivenPlayer && !neighbour10.terrainOwner.isAtWarWithGivenPlayer))
							{
								continue;
							}
							if (p_unit.IsPontoonBridge)
							{
								if ((neighbour10.enumType == TileTypes.RIVER || (neighbour10.Bridge && neighbour10.IsBridgeDestroyed)) && (!TurnManager.currPlayer.IsComputer || !neighbour10.isFrozen))
								{
									MapGO.listOfSpawnableTiles.Add(neighbour10.tileGO);
								}
							}
							else if (neighbour10.enumType != TileTypes.RIVER)
							{
								MapGO.listOfSpawnableTiles.Add(neighbour10.tileGO);
							}
						}
					}
				}
			}
		}
		if (p_unit.CoastalBattery)
		{
			Dictionary<short, int> waterBodiesSize = GameData.Instance.map.GetWaterBodiesSize();
			HashSet<short> hashSet2 = new HashSet<short>();
			foreach (KeyValuePair<short, int> item4 in waterBodiesSize)
			{
				if (item4.Value > 10)
				{
					hashSet2.Add(item4.Key);
				}
			}
			Map map = GameData.Instance.map;
			Tile[,] tilesTable = map.TilesTable;
			int sizeX = map.SizeX;
			int sizeY = map.SizeY;
			Player ownerPlayer = p_unit.OwnerPlayer;
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					Tile tile = tilesTable[i, j];
					if (tile.enumType == TileTypes.SEA || tile.terrainOwner != ownerPlayer || tile.tileGO.groundUnitGO != null)
					{
						continue;
					}
					List<Tile> neighbours = tile.neighbours;
					int k = 0;
					for (int count = neighbours.Count; k < count; k++)
					{
						Tile tile2 = neighbours[k];
						if (tile2.enumType == TileTypes.SEA)
						{
							short? seaAreaIndex = tile2.seaAreaIndex;
							if (seaAreaIndex.HasValue && hashSet2.Contains(seaAreaIndex.Value))
							{
								MapGO.listOfSpawnableTiles.Add(tile.tileGO);
								break;
							}
						}
					}
				}
			}
		}
		if (TurnManager.currPlayer.IsComputer)
		{
			return;
		}
		MapGO.listOfPotentialTilesAboveTheMap = new List<TileGO>();
		foreach (TileGO listOfSpawnableTile in MapGO.listOfSpawnableTiles)
		{
			if (!MapGO.listOfPotentialTilesAboveTheMap.Contains(listOfSpawnableTile))
			{
				MapGO.listOfPotentialTilesAboveTheMap.Add(listOfSpawnableTile);
			}
		}
		MapGO.instance.ShowPotentialtileColor(isDeployment: true);
		_countdownsGameObjects = new List<GameObject>();
		foreach (KeyValuePair<TileGO, int> countdownsDatum in _countdownsData)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(countdownPrefab, countdownsDatum.Key.transform);
			gameObject.transform.position = new Vector3(countdownsDatum.Key.transform.position.x + 2f, countdownsDatum.Key.transform.position.y + 2f, -25f);
			gameObject.GetComponentInChildren<TextMeshPro>().text = countdownsDatum.Value.ToString();
			_countdownsGameObjects.Add(gameObject);
		}
	}

	public bool CanDeployUnitToTile(Unit p_unit, Tile p_tile, bool p_sourceIsReinforcementTile = false)
	{
		if (p_unit.Type == "Boat")
		{
			if (p_tile.tileGO.groundUnitGO != null || p_tile.isFrozen)
			{
				return false;
			}
			if (p_unit.IsPatrolBoat)
			{
				if (p_tile.enumType != TileTypes.SEA)
				{
					return p_tile.enumType == TileTypes.RIVER;
				}
				return true;
			}
			return p_tile.enumType == TileTypes.SEA;
		}
		if (p_unit.Type == "Plane")
		{
			if (p_tile.tileGO.airUnitGO == null)
			{
				if (p_tile.terrainOwner != null)
				{
					return p_tile.terrainOwner.isAlliedWithGivenPlayer;
				}
				return true;
			}
			return false;
		}
		if (p_unit.IsArmouredTrain)
		{
			if (p_tile.tileGO.groundUnitGO == null)
			{
				return p_tile.Railroad;
			}
			return false;
		}
		if (p_tile.terrainOwner != null && !p_tile.terrainOwner.isAlliedWithGivenPlayer && !p_tile.terrainOwner.isAtWarWithGivenPlayer)
		{
			return false;
		}
		if (p_tile.enumType == TileTypes.MARSH && (p_unit.FilterType != "FilterInfantry" || p_unit.IsMotorized || p_unit.IsMechanized || p_unit.IsHorsed))
		{
			return false;
		}
		if (p_tile.IsBridgeDestroyed)
		{
			return false;
		}
		if (p_tile.enumType == TileTypes.RIVER)
		{
			return false;
		}
		if (p_tile.enumType == TileTypes.MOUNTAIN)
		{
			return false;
		}
		if (p_tile.tileGO.groundUnitGO != null)
		{
			return false;
		}
		if (!p_sourceIsReinforcementTile && p_tile.enumType == TileTypes.SEA)
		{
			return false;
		}
		return true;
	}

	private bool IsTileBesieged(Tile p_tile, Unit p_unit)
	{
		if (p_tile.IsReinforcementTile)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (neighbour.tileGO.groundUnitGO != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.isAtWarWithGivenPlayer && !neighbour.tileGO.groundUnitGO.unit.IsCommander && !neighbour.tileGO.groundUnitGO.unit.IsTrain && neighbour.tileGO.groundUnitGO.unit.MaxMP > 0)
			{
				num++;
			}
			if (p_unit.Type == "Boat")
			{
				if (neighbour.enumType == TileTypes.SEA && !neighbour.isFrozen)
				{
					num2++;
				}
				else if (neighbour.enumType == TileTypes.RIVER && !neighbour.isFrozen && p_unit.CanMoveOnRivers())
				{
					num2++;
				}
			}
			else if (p_unit.Type == "Plane")
			{
				num2++;
			}
			else if (neighbour.enumType == TileTypes.RIVER)
			{
				if (neighbour.isFrozen || (neighbour.Bridge && neighbour.IsBridgeDestroyed) || p_unit.CanMoveOnRivers())
				{
					num2++;
				}
			}
			else if (neighbour.enumType == TileTypes.MARSH)
			{
				if (p_unit.CanMoveOnMarshes())
				{
					num2++;
				}
			}
			else if (neighbour.enumType == TileTypes.MOUNTAIN)
			{
				if (p_unit.CanMoveOverMountains())
				{
					num2++;
				}
			}
			else if (neighbour.enumType == TileTypes.SEA)
			{
				if (neighbour.isFrozen || (neighbour.Bridge && neighbour.IsBridgeDestroyed))
				{
					num2++;
				}
			}
			else
			{
				num2++;
			}
		}
		return (float)num / (float)num2 >= 0.25f;
	}
}
