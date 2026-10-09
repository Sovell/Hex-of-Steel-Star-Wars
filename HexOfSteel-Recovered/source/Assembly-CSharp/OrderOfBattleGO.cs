using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderOfBattleGO : MonoBehaviour
{
	public static OrderOfBattleGO instance;

	public string filterType = "All";

	[SerializeField]
	private AnimationCurve _selectionAnimationCurve;

	public Animator animator;

	public GameObject OOBPanel;

	public UnitOrderOfBattleSlotGO slotPrefab;

	public Transform parent;

	public GameObject transferPanel;

	public GameObject transferPlayerPrefab;

	public Toggle autoSupplies_toggle;

	public Toggle sortByLowHealth_toggle;

	public Toggle sortByCoreUnits_toggle;

	public Toggle sortByUpkeepCost_toggle;

	public TMP_Dropdown filter_dropdown;

	public Scrollbar scrollbar;

	public TMP_InputField unitName_inputfield;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private RectTransform _filtersRect;

	[SerializeField]
	private float _slotSpacing = 10f;

	private List<Unit> _filteredUnits = new List<Unit>();

	private Dictionary<int, UnitOrderOfBattleSlotGO> _activeSlots = new Dictionary<int, UnitOrderOfBattleSlotGO>();

	private Stack<UnitOrderOfBattleSlotGO> _pool = new Stack<UnitOrderOfBattleSlotGO>();

	private RectTransform _contentRect;

	private RectTransform _viewportRect;

	private float _slotHeight;

	private float _filtersHeight;

	private int _currentTopIndex = -1;

	private int _currentBottomIndex = -1;

	private bool _ignoreHighlightUnit;

	private void Awake()
	{
		instance = this;
		_contentRect = _scrollRect.content;
		_viewportRect = _scrollRect.viewport;
		_slotHeight = ((RectTransform)slotPrefab.transform).rect.height + _slotSpacing;
		filter_dropdown.ClearOptions();
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-all")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-infantry")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-tanks")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-antitanks")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-artillery")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-antiair")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-fighters")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-light bombers")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-heavy bombers")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-ships")));
		filter_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("oob-buildings")));
		filter_dropdown.RefreshShownValue();
		_scrollRect.onValueChanged.AddListener(OnScrollChanged);
	}

	private void Start()
	{
		sortByCoreUnits_toggle.gameObject.SetActive(GameData.Instance.map.IsCampaign);
		sortByUpkeepCost_toggle.gameObject.SetActive(PlayerSettings.Instance.Upkeep);
	}

	private void Update()
	{
		if (transferPanel.activeSelf)
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
	}

	public void ManageOOB()
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer.IsComputer && !MapGO.IsAIOnly)
		{
			return;
		}
		if (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsLocalClientPlayer())
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-not your turn"));
			return;
		}
		transferPanel.SetActive(value: false);
		if (OOBPanel.activeSelf)
		{
			StartCoroutine(CloseOOB());
			return;
		}
		autoSupplies_toggle.SetIsOnWithoutNotify(TurnManager.currPlayer.OOBautoSupplies);
		OOBPanel.SetActive(value: true);
		animator.Play("ShowOOB");
		GenerateAllSlots();
		ShowSelectedUnitInOOB();
		TutorialManager.PerformAction(TutorialActions.OPEN_ORDER_OF_BATTLE_MENU);
	}

	public void GenerateAllSlots()
	{
		RecycleAllSlots();
		BuildFilteredUnitList();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_filtersRect);
		_filtersHeight = _filtersRect.rect.height - _filtersRect.anchoredPosition.y + _slotSpacing;
		_contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, _filtersHeight + (float)_filteredUnits.Count * _slotHeight);
		_currentTopIndex = -1;
		_currentBottomIndex = -1;
		UpdateVisibleSlots(p_forceRefresh: true);
	}

	private void BuildFilteredUnitList()
	{
		switch (filter_dropdown.value)
		{
		default:
			filterType = "All";
			break;
		case 1:
			filterType = "FilterInfantry";
			break;
		case 2:
			filterType = "FilterTank";
			break;
		case 3:
			filterType = "FilterAntitank";
			break;
		case 4:
			filterType = "FilterArtillery";
			break;
		case 5:
			filterType = "FilterAA";
			break;
		case 6:
			filterType = "FilterFighter";
			break;
		case 7:
			filterType = "FilterCAS";
			break;
		case 8:
			filterType = "FilterBomber";
			break;
		case 9:
			filterType = "FilterBoat";
			break;
		case 10:
			filterType = "FilterBuilding";
			break;
		}
		if (sortByLowHealth_toggle.isOn)
		{
			TurnManager.currPlayer.ListOfUnits = TurnManager.currPlayer.ListOfUnits.OrderBy((Unit x) => (float)x.CurrHP / (float)x.MaxHP).ToList();
		}
		else if (sortByUpkeepCost_toggle.isOn)
		{
			TurnManager.currPlayer.ListOfUnits = TurnManager.currPlayer.ListOfUnits.OrderByDescending((Unit x) => x.GetUpkeepCost(p_getReserveCost: false)).ToList();
		}
		else
		{
			TurnManager.currPlayer.ListOfUnits = TurnManager.currPlayer.ListOfUnits.OrderByDescending((Unit x) => x.IsCommander).ThenByDescending((Unit o) => o.Cost).ThenByDescending((Unit o) => o.XP)
				.ToList();
		}
		_filteredUnits.Clear();
		string text = unitName_inputfield.text;
		for (int num = 0; num < TurnManager.currPlayer.ListOfUnits.Count; num++)
		{
			Unit unit = TurnManager.currPlayer.ListOfUnits[num];
			if ((!sortByCoreUnits_toggle.isOn || unit.IsCoreUnit) && (!(filterType != "All") || !(unit.FilterType != filterType)) && !unit.IsDead && !unit.IsReserve && unit.OwnerPlayer != null && (bool)unit.unitGO && (string.IsNullOrEmpty(text) || unit.GetUnitName().Contains(text, StringComparison.OrdinalIgnoreCase)))
			{
				_filteredUnits.Add(unit);
			}
		}
	}

	private void OnScrollChanged(Vector2 p_scrollPosition)
	{
		if (OOBPanel.activeSelf)
		{
			UpdateVisibleSlots(p_forceRefresh: false);
		}
	}

	private void UpdateVisibleSlots(bool p_forceRefresh)
	{
		if (_filteredUnits.Count == 0)
		{
			if (_currentTopIndex != -1)
			{
				RecycleAllSlots();
			}
			return;
		}
		float num = _contentRect.anchoredPosition.y - _filtersHeight;
		float height = _viewportRect.rect.height;
		float num2 = 1f / _slotHeight;
		int num3 = Mathf.Max(0, (int)(num * num2));
		int num4 = Mathf.Min(_filteredUnits.Count - 1, (int)((num + height) * num2));
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (!p_forceRefresh && num3 == _currentTopIndex && num4 == _currentBottomIndex)
		{
			return;
		}
		if (_currentTopIndex == -1)
		{
			p_forceRefresh = true;
			_currentTopIndex = num3;
			_currentBottomIndex = num4;
			for (int i = num3; i <= num4; i++)
			{
				_activeSlots[i] = GetOrCreateSlot();
			}
		}
		else
		{
			if (num4 < _currentTopIndex || num3 > _currentBottomIndex)
			{
				p_forceRefresh = true;
				RecycleAllSlots();
				for (int j = num3; j <= num4; j++)
				{
					_activeSlots[j] = GetOrCreateSlot();
				}
			}
			else
			{
				if (num3 > _currentTopIndex)
				{
					for (int k = _currentTopIndex; k < num3; k++)
					{
						RecycleSlotAtIndex(k);
					}
				}
				if (num4 < _currentBottomIndex)
				{
					for (int l = num4 + 1; l <= _currentBottomIndex; l++)
					{
						RecycleSlotAtIndex(l);
					}
				}
				if (num3 < _currentTopIndex)
				{
					for (int m = num3; m < _currentTopIndex; m++)
					{
						_activeSlots[m] = GetOrCreateSlot();
						if (!p_forceRefresh)
						{
							BindSlot(_activeSlots[m], m);
						}
					}
				}
				if (num4 > _currentBottomIndex)
				{
					for (int n = _currentBottomIndex + 1; n <= num4; n++)
					{
						_activeSlots[n] = GetOrCreateSlot();
						if (!p_forceRefresh)
						{
							BindSlot(_activeSlots[n], n);
						}
					}
				}
			}
			_currentTopIndex = num3;
			_currentBottomIndex = num4;
		}
		if (p_forceRefresh)
		{
			for (int num5 = _currentTopIndex; num5 <= _currentBottomIndex; num5++)
			{
				BindSlot(_activeSlots[num5], num5);
			}
		}
	}

	private void BindSlot(UnitOrderOfBattleSlotGO p_slot, int p_index)
	{
		Unit unit = _filteredUnits[p_index];
		p_slot.ResetSlot();
		p_slot.unit = unit;
		p_slot.autoSupplies_toggle.onValueChanged.AddListener(delegate
		{
			ManageAutoSupplies(unit);
		});
		p_slot.delete_button.onClick.AddListener(() =>
		{
			DisbandUnit(unit);
		});
		p_slot.redeploy_button.onClick.AddListener(() =>
		{
			RedeployUnit(unit);
		});
		p_slot.transfer_button.onClick.AddListener(() =>
		{
			ManageTransferPanel(unit);
		});
		p_slot.slot_button.onClick.AddListener(() =>
		{
			SelectUnit(unit);
		});
		p_slot.SetUpSlot();
		RectTransform rectTransform = (RectTransform)p_slot.transform;
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f - (_filtersHeight + (float)p_index * _slotHeight));
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ((RectTransform)slotPrefab.transform).rect.height);
		p_slot.gameObject.SetActive(value: true);
	}

	private UnitOrderOfBattleSlotGO GetOrCreateSlot()
	{
		UnitOrderOfBattleSlotGO unitOrderOfBattleSlotGO;
		if (_pool.Count > 0)
		{
			unitOrderOfBattleSlotGO = _pool.Pop();
		}
		else
		{
			unitOrderOfBattleSlotGO = UnityEngine.Object.Instantiate(slotPrefab, _contentRect);
			unitOrderOfBattleSlotGO.gameObject.SetActive(value: false);
		}
		return unitOrderOfBattleSlotGO;
	}

	private void RecycleSlotAtIndex(int p_index)
	{
		if (_activeSlots.TryGetValue(p_index, out var value))
		{
			value.ResetSlot();
			value.gameObject.SetActive(value: false);
			_pool.Push(value);
			_activeSlots.Remove(p_index);
		}
	}

	private void RecycleAllSlots()
	{
		foreach (UnitOrderOfBattleSlotGO value in _activeSlots.Values)
		{
			value.ResetSlot();
			value.gameObject.SetActive(value: false);
			_pool.Push(value);
		}
		_activeSlots.Clear();
		_currentTopIndex = -1;
		_currentBottomIndex = -1;
	}

	public void ShowSelectedUnitInOOB()
	{
		if (_ignoreHighlightUnit || !MapGO.selectedUnitGO || !OOBPanel.activeSelf)
		{
			return;
		}
		int selectedID = MapGO.selectedUnitGO.unit.ID;
		int num = _filteredUnits.FindIndex((Unit u) => u.ID == selectedID);
		if (num < 0)
		{
			scrollbar.value = 1f;
			return;
		}
		float value = _filtersHeight + (float)num * _slotHeight;
		float num2 = _contentRect.sizeDelta.y - _viewportRect.rect.height;
		if (num2 > 0f)
		{
			_contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, Mathf.Clamp(value, 0f, num2));
		}
		UpdateVisibleSlots(p_forceRefresh: false);
		if (_activeSlots.TryGetValue(num, out var value2))
		{
			StartCoroutine(CR_HighlightUnit(value2.gameObject));
		}
	}

	private IEnumerator CR_HighlightUnit(GameObject p_gameObject)
	{
		p_gameObject.transform.localScale = Vector3.one;
		float timer = 0f;
		float animationTime = 0.17f;
		while (timer <= animationTime)
		{
			timer += Time.deltaTime;
			p_gameObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, timer / animationTime);
			yield return null;
		}
		timer = 0f;
		while (timer <= animationTime)
		{
			timer += Time.deltaTime;
			p_gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, timer / animationTime);
			yield return null;
		}
	}

	public void SelectUnit(Unit unit)
	{
		SoundManager.instance.PlayUIClickSound();
		transferPanel.SetActive(value: false);
		if (!unit.OwnerPlayer.IsComputer)
		{
			if (!unit.unitGO.gameObject.activeSelf)
			{
				UIManager.showAirUnit = !UIManager.showAirUnit;
				UIManager.instance.ShowHideAirGroundUnit(doesSound: false);
			}
			_ignoreHighlightUnit = true;
			unit.unitGO.SelectUnit(p_playAnimation: true);
			UIManager.instance.StartCoroutine(UIManager.instance.HighlighTile(unit.unitGO.transform.position.x, unit.unitGO.transform.position.y));
		}
		StartCoroutine(LerpFromTo(Camera.main.transform.position, new Vector3(unit.unitGO.gameObject.transform.position.x, unit.unitGO.gameObject.transform.position.y, Camera.main.transform.position.z), 0.3f));
		_ignoreHighlightUnit = false;
	}

	public void DisbandUnit(Unit p_unit)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Tile neighbour in p_unit.unitGO.tileGO.tile.neighbours)
		{
			if (neighbour.tileGO.groundUnitGO != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.Faction != p_unit.OwnerPlayer.Faction)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-disband unit enemies around"));
				return;
			}
			if (neighbour.tileGO.airUnitGO != null && neighbour.tileGO.airUnitGO.unit.OwnerPlayer != null && neighbour.tileGO.airUnitGO.unit.OwnerPlayer.Faction != p_unit.OwnerPlayer.Faction)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-disband unit enemies around"));
				return;
			}
		}
		if (MapGO.selectedUnitGO == p_unit.unitGO)
		{
			MapGO.instance.UnselectUnit();
		}
		if (p_unit.Type == "Plane")
		{
			if (p_unit.unitGO.tileGO.groundUnitGO != null)
			{
				p_unit.unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				p_unit.unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			}
			p_unit.unitGO.tileGO.tile.AirUnit = null;
			UnityEngine.Object.Destroy(p_unit.unitGO.tileGO.airUnitGO.gameObject);
		}
		else
		{
			if (p_unit.unitGO.tileGO.airUnitGO != null)
			{
				p_unit.unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
				p_unit.unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			}
			p_unit.unitGO.tileGO.tile.GroundUnit = null;
			UnityEngine.Object.Destroy(p_unit.unitGO.tileGO.groundUnitGO.gameObject);
		}
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Unfield unit"));
		float num = (float)p_unit.CurrHP / (float)p_unit.MaxHP;
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Infantry_focus) && p_unit.FilterType == "FilterInfantry")
		{
			p_unit.Cost -= (int)Math.Round((float)p_unit.Cost * 0.25f, MidpointRounding.ToEven);
		}
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Armoured_focus) && p_unit.Type == "Tank")
		{
			p_unit.Cost -= (int)Math.Round((float)p_unit.Cost * 0.25f, MidpointRounding.ToEven);
		}
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Air_focus) && p_unit.Type == "Plane")
		{
			p_unit.Cost -= (int)Math.Round((float)p_unit.Cost * 0.25f, MidpointRounding.ToEven);
		}
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Guns_focus) && (p_unit.Type == "Artillery" || p_unit.Type == "AA"))
		{
			p_unit.Cost -= (int)Math.Round((float)p_unit.Cost * 0.25f, MidpointRounding.ToEven);
		}
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Naval_focus) && p_unit.Type == "Boat")
		{
			p_unit.Cost -= (int)Math.Round((float)p_unit.Cost * 0.25f, MidpointRounding.ToEven);
		}
		TurnManager.currPlayer.AddMoney((int)Math.Round((float)p_unit.Cost * num / 2f, MidpointRounding.AwayFromZero), p_sendRPC: true);
		TurnManager.currPlayer.AddManpower((int)Math.Round((float)p_unit.manpowerCost * num, MidpointRounding.AwayFromZero), p_doMarginCheck: false, p_sendRPC: true);
		MultiplayerManager.Instance.RunRPC("RPC_KillUnit", "O", p_unit.ID, p_unit.OwnerName, false, false);
		if (p_unit.IsCommander)
		{
			MapGO.listOfAllGeneralsOnTheMap.Remove(p_unit.unitGO);
		}
		if (TurnManager.currPlayer.PurchasedUnits.ContainsKey(p_unit.Name) && TurnManager.currPlayer.PurchasedUnits[p_unit.Name] > 0)
		{
			TurnManager.currPlayer.PurchasedUnits[p_unit.Name]--;
		}
		TurnManager.currPlayer.RemoveUnit(p_unit, p_sendRPC: true);
		GenerateAllSlots();
		if (UIManager.instance.coreUnitsPanel.activeSelf)
		{
			UIManager.instance.GenerateCoreUnitsSlots();
		}
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		MapGO.instance.ClearCommanderTiles();
		TutorialManager.PerformAction(TutorialActions.DISBAND_UNIT);
	}

	public void RedeployUnit(Unit p_unit)
	{
		if (!(p_unit.unitGO == null) && !p_unit.unitGO.isMoving && !p_unit.OwnerPlayer.IsComputer)
		{
			SoundManager.instance.PlayUIClickSound();
			int redeployCost = p_unit.GetRedeployCost();
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation unit redeployment") + redeployCost, p_unit, ConfirmRedeployUnit);
		}
	}

	private void ConfirmRedeployUnit(Unit p_unit)
	{
		if (!(MapGO.selectedUnitGO == null) && !MapGO.selectedUnitGO.isMoving && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Unfield unit"));
			p_unit.RedeployUnit(p_removeMoney: true, p_sendRPC: true);
			GenerateAllSlots();
		}
	}

	public void ManageAutoSupplies(Unit p_unit)
	{
		SoundManager.instance.PlayUIClickSound();
		p_unit.AutoSupplies = !p_unit.AutoSupplies;
	}

	public void RefreshOrderSlots_Toggle()
	{
		SoundManager.instance.PlayUIClickSound();
		GenerateAllSlots();
	}

	public void ManageAutoSuppliesForAll_Toggle(Toggle p_toggle)
	{
		SoundManager.instance.PlayUIClickSound();
		TurnManager.currPlayer.OOBautoSupplies = p_toggle.isOn;
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (filterType == "All" || listOfUnit.FilterType == filterType)
			{
				listOfUnit.AutoSupplies = TurnManager.currPlayer.OOBautoSupplies;
			}
		}
		GenerateAllSlots();
		MultiplayerManager.RPC("RPC_SyncPlayerData", TurnManager.currPlayer.Name, MultiplayerManager.SyncActions.PLAYER_AUTOMATIC_SUPPLIES, p_toggle.isOn);
	}

	public void ShowAllPlayersWeCanGiveUnitTo()
	{
		SoundManager.instance.PlayUIClickSound();
		transferPanel.SetActive(!transferPanel.activeSelf);
	}

	public void ManageTransferPanel(Unit p_unit)
	{
		SoundManager.instance.PlayUIClickSound();
		Transform[] componentsInChildren = transferPanel.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != transferPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
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
				transferPanel.SetActive(value: true);
			}
		}
		if (Input.mousePosition.y > (float)(Screen.height / 2))
		{
			transferPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
		}
		else
		{
			transferPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
		}
		transferPanel.transform.position = new Vector3(Input.mousePosition.x + 50f, Input.mousePosition.y);
	}

	public void TransferUnit(Unit p_unit, Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (p_unit.ActionPoints < 2)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-transfer unit no aps"));
			return;
		}
		if (p_unit.IsCoreUnit && p_player.CountCurrCoreUnits() >= Utils.GetMaxCoreUnitsNumber(p_player))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-transfer core unit"));
			return;
		}
		transferPanel.SetActive(value: false);
		if (MapGO.selectedUnitGO != null && p_unit.ID == MapGO.selectedUnitGO.unit.ID)
		{
			MapGO.instance.UnselectUnit();
		}
		new Notification(NotificationTypes.UNIT_TRANSFER, p_player.Name, p_unit.unitGO.transform.position.x, p_unit.unitGO.transform.position.y, p_unit.Name, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
		p_unit.Waypoints = new List<int[]>();
		if (!TurnManager.currPlayer.IsAlliedWith(p_player) && !TurnManager.currPlayer.IsAtWarWith(p_player))
		{
			p_unit.IsReserve = true;
			p_unit.SetRedeploymentTime();
			if (p_unit.Type == "Plane")
			{
				if (p_unit.unitGO.tileGO.groundUnitGO != null)
				{
					p_unit.unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
					p_unit.unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
				}
				p_unit.unitGO.tileGO.airUnitGO = null;
				p_unit.unitGO.tileGO.tile.AirUnit = null;
			}
			else
			{
				if (p_unit.unitGO.tileGO.airUnitGO != null)
				{
					p_unit.unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
					p_unit.unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
				}
				p_unit.unitGO.tileGO.groundUnitGO = null;
				p_unit.unitGO.tileGO.tile.GroundUnit = null;
			}
			if (p_unit.IsCommander)
			{
				MapGO.listOfAllGeneralsOnTheMap.Remove(p_unit.unitGO);
			}
			UnityEngine.Object.Destroy(p_unit.unitGO.gameObject);
		}
		MultiplayerManager.RPC("RPC_TransferUnit", p_unit, p_player.Name);
		TurnManager.currPlayer.RemoveUnit(p_unit, p_sendRPC: false);
		p_player.AddUnit(p_unit, p_sendRPC: false);
		if (p_unit.unitGO != null)
		{
			p_unit.unitGO.SetSprite();
		}
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		if (!TurnManager.currPlayer.IsComputer)
		{
			if (p_unit.IsPrototype)
			{
				SteamManager.UnlockAchievement("Sharing secrets");
			}
			SteamManager.UnlockAchievement("Generous benefactor");
		}
		GenerateAllSlots();
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		TutorialManager.PerformAction(TutorialActions.TRANSFER_UNIT);
	}

	private IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			Camera.main.transform.position = Vector3.Lerp(pos1, pos2, t / duration);
			yield return 0;
		}
		Camera.main.transform.position = new Vector3(pos2.x, pos2.y, -80f);
	}

	public IEnumerator CloseOOB()
	{
		animator.Play("HideOOB");
		yield return new WaitForSeconds(1f);
		RecycleAllSlots();
		OOBPanel.SetActive(value: false);
	}
}
