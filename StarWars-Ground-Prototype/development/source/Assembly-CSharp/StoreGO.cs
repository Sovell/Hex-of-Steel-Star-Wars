using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoreGO : MonoBehaviour
{
	public static StoreGO instance;

	public bool upgradeMode;

	public Unit selectedUnit;

	[Header("Prefabs")]
	public UnitStoreSlotGO unitSlotPrefab;

	public GameObject basketSlotPrefab;

	public GameObject basketSlotCorePrefab;

	public GameObject turnToProduceBox;

	public GameObject upkeepBox;

	public TextMeshProUGUI HQPoints_text;

	public TextMeshProUGUI productionYear;

	public TextMeshProUGUI HP;

	public TextMeshProUGUI HPcomparison;

	public TextMeshProUGUI armour;

	public TextMeshProUGUI armourComparison;

	public TextMeshProUGUI softDamage;

	public TextMeshProUGUI softDamageComparison;

	public TextMeshProUGUI hardDamage;

	public TextMeshProUGUI hardDamageComparison;

	public TextMeshProUGUI airDamage;

	public TextMeshProUGUI airDamageComparison;

	public TextMeshProUGUI fuel;

	public TextMeshProUGUI fuelComparison;

	public TextMeshProUGUI ammo;

	public TextMeshProUGUI ammoComparison;

	public TextMeshProUGUI MP;

	public TextMeshProUGUI MPComparison;

	public TextMeshProUGUI Range;

	public TextMeshProUGUI rangeComparison;

	public TextMeshProUGUI Visibility;

	public TextMeshProUGUI visibilityComparison;

	public TextMeshProUGUI turnProduction;

	public TextMeshProUGUI turnProductionComparison;

	public TextMeshProUGUI upkeep;

	public TextMeshProUGUI upkeepComparison;

	public Transform parentForBasket_Transform;

	public List<Unit> unitsBasket;

	public TextMeshProUGUI basketTotalPrice_Text;

	public Transform parentForUnitsSlot_Transform;

	public string filterType;

	public TMP_Dropdown filterCountry_Dropdown;

	public Sprite unselectedHorse_Sprite;

	public Sprite selectedHorse_Sprite;

	public Sprite unselectedMotorized_Sprite;

	public Sprite selectedMotorized_Sprite;

	public Sprite unselectedMechanized_Sprite;

	public Sprite selectedMechanized_Sprite;

	public Sprite unselectedElite_Sprite;

	public Sprite selectedElite_Sprite;

	public Sprite unselectedDropTank_Sprite;

	public Sprite selectedDropTank_Sprite;

	public Sprite unselectedBomb_Sprite;

	public Sprite selectedBomb_Sprite;

	public Sprite unselectedRocket_Sprite;

	public Sprite selectedRocket_Sprite;

	public Sprite unselectedTorpedo_Sprite;

	public Sprite selectedTorpedo_Sprite;

	public Sprite unselectedMountaineer_Sprite;

	public Sprite selectedMountaineer_Sprite;

	public Sprite unselectedWinterSpecialized_Sprite;

	public Sprite selectedWinterSpecialized_Sprite;

	public Sprite unselectedABomb_Sprite;

	public Sprite selectedABomb_Sprite;

	public Sprite unselectedSS_Sprite;

	public Sprite selectedSS_Sprite;

	public Sprite unselectedCensoredSS_Sprite;

	public Sprite selectedCensoredSS_Sprite;

	public Sprite unselectedGuards_Sprite;

	public Sprite selectedGuards_Sprite;

	public GameObject coreUnit_Button;

	private List<Unit> _allUnits;

	private List<Unit> _unitsWeAreAllowedToBuy;

	private List<UnitStoreSlotGO> _unitStoreSlotGOs;

	[Header("Upgrade")]
	public List<GameObject> selectedCategories;

	public List<Image> categoriesImages;

	public UnitStoreSlotGO upgradeFromUnit;

	public UnitStoreSlotGO upgradeIntoUnit;

	public Button buy_button;

	public TextMeshProUGUI buy_text;

	[Header("DEBUG / INFOS")]
	[SerializeField]
	private string _activeCountryDropdownValue;

	[SerializeField]
	private List<string> _availableCountries;

	[SerializeField]
	private UnitStoreSlotGO _selectedStoreSlot;

	private Unit _unitToUpgrade;

	private EventActionSlotGO _eventActionSlotGO;

	private byte _productionTimeOnStoreOpening;

	private Player _lastPlayerOpenedStore;

	private void Awake()
	{
		instance = this;
		_availableCountries = new List<string>();
		if (PlayerSettings.Instance.CensoredGermany)
		{
			unselectedSS_Sprite = unselectedCensoredSS_Sprite;
			selectedSS_Sprite = selectedCensoredSS_Sprite;
		}
		_allUnits = Utils.GetAllUnits();
		_unitStoreSlotGOs = new List<UnitStoreSlotGO>();
		HQPoints_text.gameObject.SetActive(!MainMenu.isEditor && PlayerSettings.Instance.Policies);
		turnToProduceBox.SetActive(PlayerSettings.Instance.ProductionTakesTime);
		upkeepBox.SetActive(PlayerSettings.Instance.Upkeep);
	}

	private void OnEnable()
	{
		UIManager.isUIOpen = true;
		selectedUnit = null;
		_selectedStoreSlot = null;
		_productionTimeOnStoreOpening = 0;
		_allUnits = Utils.GetAllUnits();
		if (TurnManager.currPlayer != _lastPlayerOpenedStore)
		{
			EmptyBasket();
		}
		else
		{
			CalculateBasketTotalPrice();
		}
		_lastPlayerOpenedStore = TurnManager.currPlayer;
		if (filterType == string.Empty)
		{
			filterType = "FilterInfantry";
		}
		EmptyAllStatsText();
		_availableCountries.Clear();
		filterCountry_Dropdown.onValueChanged.RemoveAllListeners();
		filterCountry_Dropdown.options.Clear();
		_availableCountries.Add(TurnManager.currPlayer.Country);
		filterCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(TurnManager.currPlayer.Country).Replace(".", "")));
		if (upgradeMode && _unitToUpgrade != null)
		{
			_productionTimeOnStoreOpening = _unitToUpgrade.TurnToProduce;
		}
		if (MainMenu.isEditor)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.Name != TurnManager.currPlayer.Name && !string.IsNullOrEmpty(listOfPlayer.Country))
				{
					_availableCountries.Add(listOfPlayer.Country);
					filterCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(listOfPlayer.Country).Replace(".", "")));
				}
			}
		}
		else
		{
			foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
			{
				if (!TurnManager.currPlayer.PlayersAtWar.Contains(listOfPlayer2.Name) && (TurnManager.currPlayer.AlliedPlayers.Contains(listOfPlayer2.Name) || listOfPlayer2.Faction == TurnManager.currPlayer.Faction || TurnManager.currPlayer.Faction == "Neutral" || listOfPlayer2.Faction == "Neutral") && listOfPlayer2.Name != TurnManager.currPlayer.Name && !string.IsNullOrEmpty(listOfPlayer2.Country))
				{
					_availableCountries.Add(listOfPlayer2.Country);
					filterCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(listOfPlayer2.Country).Replace(".", "")));
				}
			}
		}
		filterCountry_Dropdown.value = 0;
		filterCountry_Dropdown.RefreshShownValue();
		filterCountry_Dropdown.onValueChanged.AddListener(delegate
		{
			SelectCountryFilter(filterCountry_Dropdown);
		});
		_activeCountryDropdownValue = _availableCountries.ElementAt(0);
		SetupUnitsHeader();
		ManageUpgradeUI();
		PopulateAllUnitSlots();
		if (upgradeMode)
		{
			ShowSelectedUnitStats(_unitToUpgrade);
		}
		if (SteamAPI.IsSteamRunning())
		{
			SteamFriends.SetRichPresence("status", "Buying units");
		}
		if (!MainMenu.isEditor)
		{
			UIManager.instance.RefreshCoreUnits();
			UIManager.instance.RefreshManpower();
			UIManager.instance.HandleMenuStacking(gameObject);
			HQPoints_text.text = LocalizationManager.Translate("txt-HQ points") + TurnManager.currPlayer.HighCommandPoints + " / " + (TurnManager.currPlayer.HasPolicy(Policies.Type.Bigger_command_centers) ? "200" : "100");
		}
	}

	private void OnDisable()
	{
		upgradeMode = false;
		_unitToUpgrade = null;
		_eventActionSlotGO = null;
		selectedUnit = null;
		UIManager.isUIOpen = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CloseStorePanel();
		}
		if (GameData.Instance.map.IsCampaign && selectedUnit != null)
		{
			coreUnit_Button.SetActive(value: true);
			coreUnit_Button.GetComponentInChildren<TextMeshProUGUI>().text = (selectedUnit.IsCoreUnit ? LocalizationManager.Translate("btn-remove core unit") : LocalizationManager.Translate("btn-make core unit"));
		}
		else
		{
			coreUnit_Button.SetActive(value: false);
		}
	}

	public void OpenPanelInUpgradeMode(Unit p_unit)
	{
		upgradeMode = true;
		_eventActionSlotGO = null;
		_unitToUpgrade = p_unit;
		gameObject.SetActive(value: true);
	}

	public void OpenPanelInEventMode(EventActionSlotGO p_eventActionSlotGO)
	{
		upgradeMode = false;
		_unitToUpgrade = null;
		_eventActionSlotGO = p_eventActionSlotGO;
		gameObject.SetActive(value: true);
	}

	public void OpenPanelInPurchaseMode()
	{
		upgradeMode = false;
		_unitToUpgrade = null;
		_eventActionSlotGO = null;
		gameObject.SetActive(value: true);
	}

	public Unit GetUnitToUpgrade()
	{
		return _unitToUpgrade;
	}

	private void SetupUnitsHeader()
	{
		string activeCountryDropdownValue = _activeCountryDropdownValue;
		foreach (Image categoriesImage in categoriesImages)
		{
			if (GameData.Instance.map.Date.Year > 1920 && GameData.Instance.map.Date.Year < 1950)
			{
				switch (activeCountryDropdownValue)
				{
				case "Nazi Germany":
				case "Italy":
				case "Soviet Union":
				case "France":
				case "USA":
				case "Great Britain":
				case "Japan":
				case "Hungary":
				case "Romania":
				case "Finland":
				case "Poland":
				case "Spain":
					categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/WWII/" + activeCountryDropdownValue + "/" + categoriesImage.name);
					break;
				default:
					categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/WWII/Generic/" + categoriesImage.name);
					break;
				}
			}
			else if (GameData.Instance.map.Date.Year < 800)
			{
				categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/Antiquity/" + categoriesImage.name);
			}
			else if (GameData.Instance.map.Date.Year >= 800 && GameData.Instance.map.Date.Year < 1600)
			{
				categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/Medieval/" + categoriesImage.name);
			}
			else if (GameData.Instance.map.Date.Year > 1900 && GameData.Instance.map.Date.Year < 1939)
			{
				categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/WWI/" + categoriesImage.name);
			}
			else if (GameData.Instance.map.Date.Year > 1950 && GameData.Instance.map.Date.Year < 2100)
			{
				categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/Modern/" + categoriesImage.name);
			}
			else if (GameData.Instance.map.Date.Year >= 1500 && GameData.Instance.map.Date.Year < 1870)
			{
				categoriesImage.sprite = Resources.Load<Sprite>("UI/Store headers/Napoleon/" + categoriesImage.name);
			}
			categoriesImage.enabled = categoriesImage.sprite != null;
		}
	}

	private void ManageUpgradeUI()
	{
		if (MainMenu.isEditor)
		{
			return;
		}
		buy_button.onClick.RemoveAllListeners();
		buy_button.onClick.AddListener(upgradeMode ? new UnityAction(UpgradeUnit) : new UnityAction(BuyAll));
		if (upgradeMode)
		{
			filterType = _unitToUpgrade.FilterType;
			SetUnitPrice(_unitToUpgrade);
			foreach (GameObject selectedCategory in selectedCategories)
			{
				selectedCategory.SetActive(selectedCategory.name.Contains(filterType));
				selectedCategory.transform.parent.GetComponent<Button>().interactable = selectedCategory.name.Contains(filterType);
			}
			foreach (Image categoriesImage in categoriesImages)
			{
				categoriesImage.color = (categoriesImage.name.Contains(filterType) ? MainMenu.blackColor : Color.white);
			}
			RefreshUpgradeFromIntoUI();
			RefreshUpgradePrice();
		}
		else
		{
			foreach (GameObject selectedCategory2 in selectedCategories)
			{
				selectedCategory2.SetActive(selectedCategory2.name.Contains(filterType));
				selectedCategory2.transform.parent.GetComponent<Button>().interactable = true;
			}
			foreach (Image categoriesImage2 in categoriesImages)
			{
				categoriesImage2.color = (categoriesImage2.name.Contains(filterType) ? MainMenu.blackColor : Color.white);
			}
			CalculateBasketTotalPrice();
		}
		filterCountry_Dropdown.gameObject.SetActive(value: true);
		upgradeFromUnit.gameObject.SetActive(upgradeMode);
		upgradeIntoUnit.gameObject.SetActive(upgradeMode);
		parentForBasket_Transform.transform.parent.gameObject.SetActive(!upgradeMode);
		RefreshBuyButtonText();
		RefreshUpgradeFromIntoUI();
	}

	private void RefreshBuyButtonText()
	{
		if (MainMenu.isEditor)
		{
			return;
		}
		if (upgradeMode)
		{
			if (selectedUnit != null)
			{
				if (selectedUnit.Name == _unitToUpgrade.Name)
				{
					buy_text.text = LocalizationManager.Translate("btn-update unit");
				}
				else if (selectedUnit.Cost < _unitToUpgrade.Cost)
				{
					buy_text.text = LocalizationManager.Translate("btn-downgrade unit");
				}
				else
				{
					buy_text.text = LocalizationManager.Translate("btn-upgrade unit");
				}
			}
			else
			{
				buy_text.text = LocalizationManager.Translate("btn-upgrade unit");
			}
		}
		else
		{
			buy_text.text = LocalizationManager.Translate("btn-buy all units");
		}
	}

	private void RefreshUpgradePrice()
	{
		if (upgradeMode && !MainMenu.isEditor && selectedUnit != null && _unitToUpgrade != null)
		{
			SetUnitPrice(_unitToUpgrade);
			_unitToUpgrade.CalculateAndSetManpowerCost();
			int num = selectedUnit.Cost - _unitToUpgrade.Cost;
			if (TurnManager.currPlayer.HasPolicy(Policies.Type.Efficient_modernization) && num != 0)
			{
				num = (int)((float)num * 0.75f);
			}
			basketTotalPrice_Text.text = ((num < 0) ? ("+" + num * -1) : (num * -1).ToString());
			if (num > TurnManager.currPlayer.MoneyAmount)
			{
				basketTotalPrice_Text.color = Color.red;
			}
			else if (num < 0)
			{
				basketTotalPrice_Text.color = new Color(0.1f, 0.71f, 0.1f);
			}
			else
			{
				basketTotalPrice_Text.color = MainMenu.greyColor;
			}
		}
	}

	private void RefreshUpgradeFromIntoUI()
	{
		if (upgradeMode && !MainMenu.isEditor)
		{
			SetUnitPrice(_unitToUpgrade);
			_unitToUpgrade.CalculateAndSetManpowerCost();
			upgradeFromUnit.unit = _unitToUpgrade;
			upgradeFromUnit.SetUpSlot(p_isUpgradeMode: true);
			ShowSelectedUnitStats(upgradeFromUnit.unit);
			if (selectedUnit != null)
			{
				upgradeIntoUnit.unit = selectedUnit;
			}
			else
			{
				Unit unit = new Unit();
				unit.Name = LocalizationManager.Translate("txt-none");
				unit.Cost = 0;
				unit.Country = TurnManager.currPlayer.Country;
				upgradeIntoUnit.unit = unit;
			}
			upgradeIntoUnit.SetUpSlot(p_isUpgradeMode: true);
		}
	}

	public void SelectFilter(string p_filter)
	{
		SoundManager.instance.PlayUIClickSound();
		selectedUnit = null;
		if (TurnManager.currPlayer == null)
		{
			return;
		}
		filterType = p_filter;
		foreach (GameObject selectedCategory in selectedCategories)
		{
			selectedCategory.SetActive(selectedCategory.name.Contains(filterType));
		}
		foreach (Image categoriesImage in categoriesImages)
		{
			categoriesImage.color = (categoriesImage.name.Contains(filterType) ? MainMenu.blackColor : Color.white);
		}
		PopulateAllUnitSlots();
		EmptyAllStatsText();
	}

	private void PopulateAllUnitSlots()
	{
		EmptyAllUnitSlots();
		if (TurnManager.currPlayer == null)
		{
			return;
		}
		_unitsWeAreAllowedToBuy = new List<Unit>();
		foreach (Unit allUnit in _allUnits)
		{
			if (allUnit.FilterType != filterType || !allUnit.IsAvailable(TurnManager.currPlayer) || (allUnit.IsCommander && (upgradeMode || (GameData.Instance.map.IsCampaign && !MainMenu.isEditor))))
			{
				continue;
			}
			if (MainMenu.isEditor)
			{
				if (allUnit.Country != _activeCountryDropdownValue)
				{
					continue;
				}
			}
			else if (upgradeMode)
			{
				if ((allUnit.Name != _unitToUpgrade.Name || allUnit.Country != _unitToUpgrade.Country) && (allUnit.IsPartisans || (allUnit.IsPrototype && PlayerSettings.Instance.PrototypesDisabled) || allUnit.Country != _activeCountryDropdownValue || (allUnit.NoLendLease && allUnit.Country != TurnManager.currPlayer.Country) || (allUnit.IsArmouredTrain && !_unitToUpgrade.IsArmouredTrain) || (!allUnit.IsArmouredTrain && _unitToUpgrade.IsArmouredTrain)))
				{
					continue;
				}
			}
			else if (allUnit.IsPartisans || (allUnit.IsPrototype && PlayerSettings.Instance.PrototypesDisabled) || allUnit.Country != _activeCountryDropdownValue || (allUnit.NoLendLease && allUnit.Country != TurnManager.currPlayer.Country))
			{
				continue;
			}
			if (allUnit.ProductionMonth == 0)
			{
				allUnit.ProductionMonth = 1;
			}
			DateTime dateTime;
			try
			{
				dateTime = new DateTime(allUnit.ProductionYear, allUnit.ProductionMonth, 1);
			}
			catch
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-unit production date") + allUnit.Name);
				dateTime = new DateTime(GameData.Instance.map.Date.Year, 1, 1);
			}
			if (upgradeMode && allUnit.Name == _unitToUpgrade.Name && allUnit.Country == _unitToUpgrade.Country)
			{
				_unitsWeAreAllowedToBuy.Add(allUnit);
			}
			else if (!MainMenu.isEditor && PlayerSettings.Instance.TechTree && !GameData.Instance.map.IsCampaign)
			{
				Player o_player = TurnManager.currPlayer;
				if (allUnit.Country == _activeCountryDropdownValue)
				{
					GameData.Instance.TryFindPlayerByCountry(_activeCountryDropdownValue, out o_player);
				}
				else if (!GameData.Instance.TryFindPlayerByCountry(allUnit.Country, out o_player))
				{
					o_player = TurnManager.currPlayer;
				}
				if (o_player.ListOfUnitsResearched.Contains(allUnit.Name) && GameData.Instance.map.Date.Year <= allUnit.EndUseYear)
				{
					_unitsWeAreAllowedToBuy.Add(allUnit);
				}
			}
			else if (dateTime <= GameData.Instance.map.Date && GameData.Instance.map.Date.Year <= allUnit.EndUseYear)
			{
				_unitsWeAreAllowedToBuy.Add(allUnit);
			}
		}
		List<Unit> list = new List<Unit>();
		foreach (Unit item2 in _unitsWeAreAllowedToBuy)
		{
			if (!upgradeMode || !(_unitToUpgrade.FilterType == "FilterBoat") || ((!_unitToUpgrade.IsCarrier || item2.IsCarrier) && (!_unitToUpgrade.IsDestroyer || item2.IsDestroyer) && (!_unitToUpgrade.IsSubmarine || item2.IsSubmarine) && (!_unitToUpgrade.IsPatrolBoat || item2.IsPatrolBoat || item2.IsDestroyer) && (!_unitToUpgrade.GetNameToLower().Contains("light cruiser") || item2.GetNameToLower().Contains("light cruiser")) && (!_unitToUpgrade.GetNameToLower().Contains("heavy cruiser") || item2.GetNameToLower().Contains("heavy cruiser")) && (!_unitToUpgrade.GetNameToLower().Contains("battleship") || item2.GetNameToLower().Contains("battleship"))))
			{
				Unit unit = item2.Clone(p_generateNewID: true);
				unit.OwnerPlayer = TurnManager.currPlayer;
				unit.OwnerName = TurnManager.currPlayer.Name;
				unit.Lvl = 1;
				unit.XP = 0;
				unit.CustomName = null;
				unit.IsReserve = true;
				unit.IsCoreUnit = upgradeMode && _unitToUpgrade.IsCoreUnit;
				unit.IsEliteUnit = upgradeMode && _unitToUpgrade.IsEliteUnit;
				unit.TampMP = 0;
				unit.TampMaxMP = unit.BaseMaxMP;
				unit.TampMaxAutonomy = unit.BaseMaxAutonomy;
				unit.TampAutonomy = unit.BaseMaxAutonomy;
				unit.SetDefaultProductionTime();
				SetUnitPrice(unit);
				unit.CalculateAndSetManpowerCost();
				list.Add(unit);
			}
		}
		list = (from o in list
			orderby o.IsCommander, o.Cost
			select o).ToList();
		if (_unitStoreSlotGOs.Count < list.Count)
		{
			for (int num = _unitStoreSlotGOs.Count; num < list.Count; num++)
			{
				UnitStoreSlotGO item = UnityEngine.Object.Instantiate(unitSlotPrefab, parentForUnitsSlot_Transform);
				_unitStoreSlotGOs.Add(item);
			}
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			Unit unit2 = list[num2];
			UnitStoreSlotGO storeSlotGO = _unitStoreSlotGOs[num2];
			_unitStoreSlotGOs[num2].ResetOptions();
			_unitStoreSlotGOs[num2].unit = unit2;
			_unitStoreSlotGOs[num2].SetUpSlot(upgradeMode);
			_unitStoreSlotGOs[num2].slot_button.onClick.AddListener(() =>
			{
				SelectUnit(unit2, storeSlotGO);
			});
			_unitStoreSlotGOs[num2].buy_button.onClick.AddListener(() =>
			{
				AddToBasket(unit2);
			});
			_unitStoreSlotGOs[num2].buyManpower_button.onClick.AddListener(() =>
			{
				AddToBasket(unit2);
			});
			_unitStoreSlotGOs[num2].elite_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Veteran");
			});
			_unitStoreSlotGOs[num2].winter_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Winter");
			});
			_unitStoreSlotGOs[num2].mountaineer_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Mountaineer");
			});
			_unitStoreSlotGOs[num2].horse_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Horse");
			});
			_unitStoreSlotGOs[num2].mechanize_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Mechanized");
			});
			_unitStoreSlotGOs[num2].truck_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Motorized");
			});
			_unitStoreSlotGOs[num2].atomBomb_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Atom bomb");
			});
			_unitStoreSlotGOs[num2].dropTank_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Drop tank");
			});
			_unitStoreSlotGOs[num2].bomb_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Bombs");
			});
			_unitStoreSlotGOs[num2].rocket_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Rockets");
			});
			_unitStoreSlotGOs[num2].torpedo_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Torpedo");
			});
			_unitStoreSlotGOs[num2].ideologicalUnit_button.onClick.AddListener(() =>
			{
				SetOptionUnit(storeSlotGO, "Political");
			});
			_unitStoreSlotGOs[num2].gameObject.SetActive(value: true);
		}
		Canvas.ForceUpdateCanvases();
	}

	public void SetUnitPrice(Unit p_unit)
	{
		p_unit.CalculateAndSetUnitPrice();
		if (p_unit.IsEliteUnit && !upgradeMode)
		{
			p_unit.Cost += p_unit.GetVeterancyOptionCost();
		}
		if ((!p_unit.IsCommander || !PlayerSettings.Instance.Policies) && !TurnManager.currPlayer.HasPolicy(Policies.Type.ImportFocus) && p_unit.Country != TurnManager.currPlayer.Country)
		{
			if (upgradeMode)
			{
				if (_unitToUpgrade == null || p_unit.Name != _unitToUpgrade.Name)
				{
					p_unit.Cost += (int)Math.Round((float)p_unit.Cost * 0.35f, MidpointRounding.ToEven);
				}
			}
			else
			{
				p_unit.Cost += (int)Math.Round((float)p_unit.Cost * 0.35f, MidpointRounding.ToEven);
			}
		}
		if (p_unit.IsWinterSpecialized && TurnManager.currPlayer.Country != "Soviet Union" && TurnManager.currPlayer.Country != "Finland")
		{
			p_unit.Cost += Database.winterCost;
		}
		if (p_unit.ProgressiveCost == 0)
		{
			return;
		}
		TurnManager.currPlayer.PurchasedUnits.TryGetValue(p_unit.Name, out var value);
		int num = value;
		foreach (Unit item in unitsBasket)
		{
			if (item.Name == p_unit.Name)
			{
				num++;
			}
		}
		p_unit.Cost += num * p_unit.ProgressiveCost;
	}

	public void RemoveUnitFromBasket(Unit p_unit)
	{
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Unfield unit"));
		unitsBasket.Remove(p_unit);
		PopulateBasketSlots();
		CalculateBasketTotalPrice();
		PopulateAllUnitSlots();
	}

	private void AddToBasket(Unit p_unit)
	{
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		if (p_unit.IsCommander)
		{
			foreach (Unit item in unitsBasket)
			{
				if (item.Name == p_unit.Name)
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-unit already in basket"));
					return;
				}
			}
		}
		if (!MainMenu.isEditor && p_unit.MaxPurchaseTime > 0)
		{
			int num = 0;
			foreach (KeyValuePair<string, int> purchasedUnit in TurnManager.currPlayer.PurchasedUnits)
			{
				if (purchasedUnit.Key == p_unit.Name)
				{
					num = purchasedUnit.Value;
					break;
				}
			}
			int num2 = 0;
			foreach (Unit item2 in unitsBasket)
			{
				if (item2.MaxPurchaseTime > 0 && item2.Name == p_unit.Name)
				{
					num2++;
					if (num2 + num >= item2.MaxPurchaseTime)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("error-unit purchase limit reached") + "(x" + p_unit.MaxPurchaseTime + ").");
						return;
					}
				}
			}
			foreach (KeyValuePair<string, int> purchasedUnit2 in TurnManager.currPlayer.PurchasedUnits)
			{
				if (purchasedUnit2.Key == p_unit.Name && (purchasedUnit2.Value >= p_unit.MaxPurchaseTime || num2 >= p_unit.MaxPurchaseTime))
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-unit purchase limit reached") + "(x" + p_unit.MaxPurchaseTime + ").");
					return;
				}
			}
		}
		Unit unit = p_unit.Clone(p_generateNewID: true);
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Army_training) && unit.FilterType != "FilterBuilding" && unit.Lvl <= 1)
		{
			byte lvl = unit.Lvl;
			while (unit.Lvl < lvl + 1)
			{
				unit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
			}
		}
		if (unit.IsCommander)
		{
			unit.IsWinterSpecialized = true;
		}
		if (unit.IsParatrooper)
		{
			unit.ApplyModification(Database.planeTransportOption);
		}
		unit.CurrHP = unit.MaxHP;
		unit.CurrMP = unit.MaxMP;
		unit.CurrAutonomy = unit.MaxAutonomy;
		if (unit.IsEliteUnit || unit.IsPoliticalUnit)
		{
			unit.Morale = Unit.MORALE_ELITE;
		}
		else
		{
			unit.Morale = Unit.MORALE_DEFAULT;
		}
		if (unit.MaxMP == 0)
		{
			unit.AsleepState = AsleepStates.SLEEP;
		}
		else
		{
			unit.AsleepState = AsleepStates.NONE;
		}
		unitsBasket.Add(unit);
		CalculateBasketTotalPrice();
		PopulateAllUnitSlots();
		PopulateBasketSlots();
		selectedUnit = null;
		if (MainMenu.isEditor)
		{
			UIManager.HideToolTip();
		}
		else
		{
			UIManager.instance.HideTooltip();
		}
		if (MainMenu.isEditor && _eventActionSlotGO != null)
		{
			_eventActionSlotGO.GetEventAction().Unit = p_unit;
			_eventActionSlotGO.OnBoughtUnit();
			CloseStorePanel();
		}
	}

	public void DuplicateBasketUnit(Unit p_unit)
	{
		Unit p_unit2 = p_unit.Clone(p_generateNewID: true);
		if (p_unit.ProgressiveCost != 0)
		{
			TurnManager.currPlayer.PurchasedUnits.TryGetValue(p_unit.Name, out var value);
			int num = value;
			foreach (Unit item in unitsBasket)
			{
				if (item.Name == p_unit.Name)
				{
					num++;
				}
			}
			p_unit.Cost += num * p_unit.ProgressiveCost;
		}
		AddToBasket(p_unit2);
	}

	public void BuyAll()
	{
		if (unitsBasket.Count == 0)
		{
			return;
		}
		if (!MainMenu.isEditor)
		{
			if (TurnManager.currPlayer.MoneyAmount - int.Parse(basketTotalPrice_Text.text) < 0)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-purchase not enough money"));
				return;
			}
			int num = 0;
			int num2 = 0;
			foreach (Unit item in unitsBasket)
			{
				if (PlayerSettings.Instance.Manpower)
				{
					num += item.manpowerCost;
					if (TurnManager.currPlayer.GetAvailableManpower() - num < 0)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("error-purchase not enough manpower"));
						return;
					}
				}
				if (PlayerSettings.Instance.Policies && item.IsCommander)
				{
					num2 += item.Cost;
					if (TurnManager.currPlayer.HighCommandPoints - num2 < 0)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("error-purchase not enough hq points"));
						return;
					}
				}
			}
		}
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Purchase"));
		foreach (Unit item2 in unitsBasket)
		{
			if (GameData.Instance.map.IsCampaign && item2.IsCoreUnit && TurnManager.currPlayer.CountCurrCoreUnits() >= Utils.GetMaxCoreUnitsNumber(TurnManager.currPlayer))
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-exceeding core unit limit"));
				continue;
			}
			if (item2.MaxPurchaseTime > 0)
			{
				foreach (KeyValuePair<string, int> purchasedUnit in TurnManager.currPlayer.PurchasedUnits)
				{
					if (purchasedUnit.Key == item2.Name && purchasedUnit.Value >= item2.MaxPurchaseTime)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("error-unit purchase limit reached") + "(x" + item2.MaxPurchaseTime + ").");
					}
				}
			}
			CheckAchievements(item2);
			TurnManager.currPlayer.PurchaseUnit(item2, p_sendRPC: true);
		}
		EmptyBasket();
		PopulateAllUnitSlots();
		EmptyAllStatsText();
		if (!MainMenu.isEditor)
		{
			UIManager.instance.RefreshCoreUnits();
			UIManager.instance.RefreshManpower();
			UIManager.instance.RefreshMoneyAmountUI();
			UIManager.instance.RefreshIncomePerTurnUI();
			HQPoints_text.text = LocalizationManager.Translate("txt-HQ points") + TurnManager.currPlayer.HighCommandPoints + " / " + (TurnManager.currPlayer.HasPolicy(Policies.Type.Bigger_command_centers) ? "200" : "100");
		}
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsReserve && (listOfUnit.TurnToProduce == 0 || !PlayerSettings.Instance.ProductionTakesTime || MainMenu.isEditor))
			{
				if (!ReservePanelGO.instance.reservePanel.activeSelf)
				{
					ReservePanelGO.instance.ManageReserveMenu();
				}
				else
				{
					ReservePanelGO.instance.GenerateButtonsList();
				}
				break;
			}
		}
		CloseStorePanel();
		if (ReservePanelGO.instance.reservePanel.activeSelf)
		{
			ReservePanelGO.instance.GenerateButtonsList();
		}
		ReservePanelGO.instance.RefreshDeployNotification();
		if (!MainMenu.isEditor)
		{
			TutorialManager.PerformAction(TutorialActions.PURCHASE_UNIT);
		}
	}

	public void CalculateBasketTotalPrice()
	{
		int num = 0;
		foreach (Unit item in unitsBasket)
		{
			if (!item.IsCommander || !PlayerSettings.Instance.Policies)
			{
				num += item.Cost;
			}
		}
		basketTotalPrice_Text.text = num.ToString();
		if (!MainMenu.isEditor && num > TurnManager.currPlayer.MoneyAmount)
		{
			basketTotalPrice_Text.color = Color.red;
		}
		else
		{
			basketTotalPrice_Text.color = MainMenu.greyColor;
		}
	}

	private void SelectCountryFilter(TMP_Dropdown p_dropdown)
	{
		SoundManager.instance.PlayUIClickSound();
		_activeCountryDropdownValue = _availableCountries.ElementAt(p_dropdown.value);
		SetupUnitsHeader();
		PopulateAllUnitSlots();
	}

	public void SetOptionUnit(UnitStoreSlotGO p_unitStoreSlotGO, string p_optionName)
	{
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		switch (p_optionName)
		{
		case "Veteran":
			if (p_unitStoreSlotGO.unit.IsEliteUnit)
			{
				p_unitStoreSlotGO.unit.Cost -= p_unitStoreSlotGO.unit.veterancyOptionCost;
				p_unitStoreSlotGO.unit.XP = 0;
				p_unitStoreSlotGO.unit.Lvl = 1;
				p_unitStoreSlotGO.unit.AddXP(0, p_canAddHero: false, p_showXPUI: false);
				p_unitStoreSlotGO.unit.CurrHP = p_unitStoreSlotGO.unit.MaxHP;
				p_unitStoreSlotGO.elite_button.GetComponent<Image>().sprite = unselectedElite_Sprite;
			}
			else
			{
				p_unitStoreSlotGO.unit.SetVeterancyOptionCost(p_unitStoreSlotGO.unit.GetVeterancyOptionCost());
				p_unitStoreSlotGO.unit.Cost += p_unitStoreSlotGO.unit.veterancyOptionCost;
				while (p_unitStoreSlotGO.unit.Lvl < 3)
				{
					p_unitStoreSlotGO.unit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
				p_unitStoreSlotGO.unit.CurrHP = p_unitStoreSlotGO.unit.MaxHP;
				p_unitStoreSlotGO.elite_button.GetComponent<Image>().sprite = selectedElite_Sprite;
			}
			p_unitStoreSlotGO.unit.IsEliteUnit = !p_unitStoreSlotGO.unit.IsEliteUnit;
			break;
		case "Political":
			if (p_unitStoreSlotGO.unit.IsPoliticalUnit)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.politicalOption);
				if (p_unitStoreSlotGO.unit.Country == "Nazi Germany")
				{
					p_unitStoreSlotGO.ideologicalUnit_button.GetComponent<Image>().sprite = unselectedSS_Sprite;
				}
				else if (p_unitStoreSlotGO.unit.Country == "Soviet Union")
				{
					p_unitStoreSlotGO.ideologicalUnit_button.GetComponent<Image>().sprite = unselectedGuards_Sprite;
				}
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.politicalOption);
				if (p_unitStoreSlotGO.unit.Country == "Nazi Germany")
				{
					p_unitStoreSlotGO.ideologicalUnit_button.GetComponent<Image>().sprite = selectedSS_Sprite;
				}
				else if (p_unitStoreSlotGO.unit.Country == "Soviet Union")
				{
					p_unitStoreSlotGO.ideologicalUnit_button.GetComponent<Image>().sprite = selectedGuards_Sprite;
				}
			}
			break;
		case "Drop tank":
			if (p_unitStoreSlotGO.unit.HasDropTank)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.dropTankOption);
				p_unitStoreSlotGO.dropTank_button.GetComponent<Image>().sprite = unselectedDropTank_Sprite;
				break;
			}
			if (!CheckAccessories(p_unitStoreSlotGO.unit))
			{
				return;
			}
			p_unitStoreSlotGO.unit.ApplyModification(Database.dropTankOption);
			p_unitStoreSlotGO.dropTank_button.GetComponent<Image>().sprite = selectedDropTank_Sprite;
			break;
		case "Atom bomb":
			if (p_unitStoreSlotGO.unit.HasABomb)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.atomBombOption);
				p_unitStoreSlotGO.atomBomb_button.GetComponent<Image>().sprite = unselectedABomb_Sprite;
				break;
			}
			if (!CheckAccessories(p_unitStoreSlotGO.unit))
			{
				return;
			}
			p_unitStoreSlotGO.unit.ApplyModification(Database.atomBombOption);
			p_unitStoreSlotGO.atomBomb_button.GetComponent<Image>().sprite = selectedABomb_Sprite;
			break;
		case "Bombs":
			if (p_unitStoreSlotGO.unit.HasBombs)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.bombsOption);
				p_unitStoreSlotGO.bomb_button.GetComponent<Image>().sprite = unselectedBomb_Sprite;
				break;
			}
			if (!CheckAccessories(p_unitStoreSlotGO.unit))
			{
				return;
			}
			p_unitStoreSlotGO.unit.ApplyModification(Database.bombsOption);
			p_unitStoreSlotGO.bomb_button.GetComponent<Image>().sprite = selectedBomb_Sprite;
			break;
		case "Rockets":
			if (p_unitStoreSlotGO.unit.HasRockets)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.rocketsOption);
				p_unitStoreSlotGO.rocket_button.GetComponent<Image>().sprite = unselectedRocket_Sprite;
				break;
			}
			if (!CheckAccessories(p_unitStoreSlotGO.unit))
			{
				return;
			}
			p_unitStoreSlotGO.unit.ApplyModification(Database.rocketsOption);
			p_unitStoreSlotGO.rocket_button.GetComponent<Image>().sprite = selectedRocket_Sprite;
			break;
		case "Torpedo":
			if (p_unitStoreSlotGO.unit.HasTorpedo)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.torpedoOption);
				p_unitStoreSlotGO.torpedo_button.GetComponent<Image>().sprite = unselectedTorpedo_Sprite;
				break;
			}
			if (!CheckAccessories(p_unitStoreSlotGO.unit))
			{
				return;
			}
			p_unitStoreSlotGO.unit.ApplyModification(Database.torpedoOption);
			p_unitStoreSlotGO.torpedo_button.GetComponent<Image>().sprite = selectedTorpedo_Sprite;
			break;
		case "Mountaineer":
			if (p_unitStoreSlotGO.unit.IsMountaineer)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.mountaineersOption);
				p_unitStoreSlotGO.mountaineer_button.GetComponent<Image>().sprite = unselectedMountaineer_Sprite;
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.mountaineersOption);
				p_unitStoreSlotGO.mountaineer_button.GetComponent<Image>().sprite = selectedMountaineer_Sprite;
			}
			break;
		case "Winter":
			if (p_unitStoreSlotGO.unit.IsWinterSpecialized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.winterTrainedOption);
				p_unitStoreSlotGO.winter_button.GetComponent<Image>().sprite = unselectedWinterSpecialized_Sprite;
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.winterTrainedOption);
				p_unitStoreSlotGO.winter_button.GetComponent<Image>().sprite = selectedWinterSpecialized_Sprite;
			}
			break;
		case "Horse":
			if (p_unitStoreSlotGO.unit.IsMechanized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.mechanizedOption);
			}
			else if (p_unitStoreSlotGO.unit.IsMotorized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.motorizedOption);
			}
			if (p_unitStoreSlotGO.unit.IsHorsed)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.horseTowedOption);
				p_unitStoreSlotGO.unit.MaxMP = p_unitStoreSlotGO.unit.BaseMaxMP;
				p_unitStoreSlotGO.unit.MaxAutonomy = p_unitStoreSlotGO.unit.BaseMaxAutonomy;
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.horseTowedOption);
			}
			p_unitStoreSlotGO.horse_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsHorsed ? selectedHorse_Sprite : unselectedHorse_Sprite);
			p_unitStoreSlotGO.truck_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMotorized ? selectedMotorized_Sprite : unselectedMotorized_Sprite);
			p_unitStoreSlotGO.mechanize_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMechanized ? selectedMechanized_Sprite : unselectedMechanized_Sprite);
			break;
		case "Motorized":
			if (p_unitStoreSlotGO.unit.IsMechanized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.mechanizedOption);
			}
			else if (p_unitStoreSlotGO.unit.IsHorsed)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.horseTowedOption);
			}
			if (p_unitStoreSlotGO.unit.IsMotorized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.motorizedOption);
				p_unitStoreSlotGO.unit.MaxMP = p_unitStoreSlotGO.unit.BaseMaxMP;
				p_unitStoreSlotGO.unit.MaxAutonomy = p_unitStoreSlotGO.unit.BaseMaxAutonomy;
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.motorizedOption);
			}
			p_unitStoreSlotGO.horse_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsHorsed ? selectedHorse_Sprite : unselectedHorse_Sprite);
			p_unitStoreSlotGO.truck_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMotorized ? selectedMotorized_Sprite : unselectedMotorized_Sprite);
			p_unitStoreSlotGO.mechanize_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMechanized ? selectedMechanized_Sprite : unselectedMechanized_Sprite);
			break;
		case "Mechanized":
			if (p_unitStoreSlotGO.unit.IsMotorized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.motorizedOption);
			}
			else if (p_unitStoreSlotGO.unit.IsHorsed)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.horseTowedOption);
			}
			if (p_unitStoreSlotGO.unit.IsMechanized)
			{
				p_unitStoreSlotGO.unit.RevertModification(Database.mechanizedOption);
			}
			else
			{
				p_unitStoreSlotGO.unit.ApplyModification(Database.mechanizedOption);
			}
			if (!p_unitStoreSlotGO.unit.IsMechanized && !p_unitStoreSlotGO.unit.IsMotorized && !p_unitStoreSlotGO.unit.IsHorsed)
			{
				p_unitStoreSlotGO.unit.MaxMP = p_unitStoreSlotGO.unit.BaseMaxMP;
				p_unitStoreSlotGO.unit.MaxAutonomy = p_unitStoreSlotGO.unit.BaseMaxAutonomy;
			}
			p_unitStoreSlotGO.horse_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsHorsed ? selectedHorse_Sprite : unselectedHorse_Sprite);
			p_unitStoreSlotGO.truck_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMotorized ? selectedMotorized_Sprite : unselectedMotorized_Sprite);
			p_unitStoreSlotGO.mechanize_button.GetComponent<Image>().sprite = (p_unitStoreSlotGO.unit.IsMechanized ? selectedMechanized_Sprite : unselectedMechanized_Sprite);
			break;
		}
		p_unitStoreSlotGO.unit.SetDefaultProductionTime();
		SetUnitPrice(p_unitStoreSlotGO.unit);
		p_unitStoreSlotGO.unit.CalculateAndSetManpowerCost();
		p_unitStoreSlotGO.RefreshValues();
		SelectUnit(p_unitStoreSlotGO.unit, p_unitStoreSlotGO);
	}

	private void UpgradeUnit()
	{
		if (_unitToUpgrade.IsTrain)
		{
			return;
		}
		if (MainMenu.isEditor)
		{
			UIManager.HideToolTip();
		}
		else
		{
			UIManager.instance.HideTooltip();
		}
		SoundManager.instance.PlayUIClickSound();
		if (selectedUnit == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no upgrade into"));
			return;
		}
		if (selectedUnit.MaxPurchaseTime > 0)
		{
			foreach (KeyValuePair<string, int> purchasedUnit in TurnManager.currPlayer.PurchasedUnits)
			{
				if (purchasedUnit.Key == selectedUnit.Name && purchasedUnit.Value >= selectedUnit.MaxPurchaseTime && purchasedUnit.Key != _unitToUpgrade.Name)
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-unit purchase limit reached") + "(x" + selectedUnit.MaxPurchaseTime + ").");
					return;
				}
			}
		}
		SetUnitPrice(_unitToUpgrade);
		_unitToUpgrade.CalculateAndSetManpowerCost();
		Unit unit = _unitToUpgrade.Clone(p_generateNewID: false);
		int cost = _unitToUpgrade.Cost;
		int num = selectedUnit.Cost - _unitToUpgrade.Cost;
		int num2 = selectedUnit.manpowerCost - _unitToUpgrade.manpowerCost;
		if (TurnManager.currPlayer.HasPolicy(Policies.Type.Efficient_modernization) && num != 0)
		{
			num = (int)((float)num * 0.75f);
		}
		if (num > TurnManager.currPlayer.MoneyAmount)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-upgrade not enough money"));
			return;
		}
		if (PlayerSettings.Instance.Manpower && num2 > TurnManager.currPlayer.GetAvailableManpower())
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-upgrade not enough manpower"));
			return;
		}
		if (!_unitToUpgrade.IsReserve)
		{
			if (_unitToUpgrade.Type == "Plane")
			{
				if (!MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Airport") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Reinforcement"))
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-cannot upgrade plane"));
					return;
				}
			}
			else if (selectedUnit.FilterType == "FilterBoat")
			{
				if (!MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Harbour") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Reinforcement"))
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-cannot upgrade ship"));
					return;
				}
			}
			else
			{
				if (!MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "City") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Town") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Village") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Harbour") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Factory") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Reinforcement"))
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-cannot upgrade ship"));
					return;
				}
				if (selectedUnit.IsConsideredHeavyEquipment())
				{
					if (!MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "City") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Factory") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Reinforcement"))
					{
						UIManager.ShowMessage(LocalizationManager.Translate("info-cannot upgrade heavy"));
						return;
					}
				}
				else if (!selectedUnit.Name.Contains("Garrison") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "City") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Factory") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Reinforcement") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "Town") && !MapGO.IsThereACertainTileAround(_unitToUpgrade.unitGO.tileGO, "BigHarbour"))
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-cannot upgrade regular"));
					return;
				}
			}
		}
		if (selectedUnit.IsCoreUnit && !_unitToUpgrade.IsCoreUnit && TurnManager.currPlayer.CountCurrCoreUnits() >= Utils.GetMaxCoreUnitsNumber(TurnManager.currPlayer))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-exceeding core unit limit"));
			return;
		}
		bool flag = selectedUnit.Name != _unitToUpgrade.Name;
		string p_text;
		if (flag)
		{
			p_text = ((selectedUnit.Cost <= _unitToUpgrade.Cost) ? (LocalizationManager.Translate("history-downgraded to") + selectedUnit.Name) : (LocalizationManager.Translate("history-upgraded to") + selectedUnit.Name));
		}
		else if (_unitToUpgrade.Cost == selectedUnit.Cost)
		{
			p_text = LocalizationManager.Translate("history-updated");
		}
		else
		{
			p_text = ((selectedUnit.Cost <= _unitToUpgrade.Cost) ? LocalizationManager.Translate("history-downgraded") : LocalizationManager.Translate("history-upgraded"));
		}
		_unitToUpgrade.AddToHistory(p_text, p_sendRPC: false);
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Start research"));
		if (TurnManager.currPlayer.PurchasedUnits.ContainsKey(selectedUnit.Name))
		{
			TurnManager.currPlayer.PurchasedUnits[selectedUnit.Name]++;
		}
		else
		{
			TurnManager.currPlayer.PurchasedUnits.Add(selectedUnit.Name, 1);
		}
		if (TurnManager.currPlayer.PurchasedUnits.ContainsKey(_unitToUpgrade.Name))
		{
			TurnManager.currPlayer.PurchasedUnits[_unitToUpgrade.Name]--;
		}
		bool flag2 = _unitToUpgrade.GetUnitCategory() != selectedUnit.GetUnitCategory();
		if (flag2)
		{
			TurnManager.currPlayer.RemoveUnit(_unitToUpgrade, p_sendRPC: false);
		}
		Unit unit2 = new Unit();
		unit2.Name = _unitToUpgrade.Name;
		unit2.Country = _unitToUpgrade.Country;
		unit2.Cost = _unitToUpgrade.Cost;
		unit2.OwnerPlayer = _unitToUpgrade.OwnerPlayer;
		Utils.UpdateUnitStatsOnTurnOne(unit2, _allUnits);
		SetUnitPrice(unit2);
		Unit unit3 = new Unit();
		unit3.Name = selectedUnit.Name;
		unit3.Country = selectedUnit.Country;
		unit3.Cost = selectedUnit.Cost;
		unit3.OwnerPlayer = selectedUnit.OwnerPlayer;
		Utils.UpdateUnitStatsOnTurnOne(unit3, _allUnits);
		SetUnitPrice(unit3);
		_unitToUpgrade.Name = selectedUnit.Name;
		_unitToUpgrade.Country = selectedUnit.Country;
		_unitToUpgrade.BaseType = (string.IsNullOrEmpty(selectedUnit.BaseType) ? selectedUnit.Type : selectedUnit.BaseType);
		_unitToUpgrade.FilterType = selectedUnit.FilterType;
		_unitToUpgrade.Armour = selectedUnit.Armour;
		_unitToUpgrade.BaseArmour = selectedUnit.BaseArmour;
		_unitToUpgrade.SoftDamage = selectedUnit.SoftDamage;
		_unitToUpgrade.HardDamage = selectedUnit.HardDamage;
		_unitToUpgrade.AirDamage = selectedUnit.AirDamage;
		_unitToUpgrade.AttackSound = selectedUnit.AttackSound;
		_unitToUpgrade.MovementSound = selectedUnit.MovementSound;
		_unitToUpgrade.AllowedRotation = selectedUnit.AllowedRotation;
		_unitToUpgrade.BaseAirDamage = selectedUnit.BaseAirDamage;
		_unitToUpgrade.BaseSoftDamage = selectedUnit.BaseSoftDamage;
		_unitToUpgrade.BaseHardDamage = selectedUnit.BaseHardDamage;
		_unitToUpgrade.IsPrototype = selectedUnit.IsPrototype;
		_unitToUpgrade.Range = selectedUnit.Range;
		_unitToUpgrade.BaseRange = selectedUnit.BaseRange;
		_unitToUpgrade.Visibility = selectedUnit.Visibility;
		_unitToUpgrade.BaseVisibility = selectedUnit.BaseVisibility;
		_unitToUpgrade.MaxAmmo = selectedUnit.MaxAmmo;
		_unitToUpgrade.CurrAmmo = selectedUnit.MaxAmmo;
		_unitToUpgrade.BaseMaxAmmo = selectedUnit.BaseMaxAmmo;
		_unitToUpgrade.CustomCost = selectedUnit.CustomCost;
		_unitToUpgrade.MaxAutonomy = selectedUnit.MaxAutonomy;
		_unitToUpgrade.BaseMaxAutonomy = selectedUnit.BaseMaxAutonomy;
		_unitToUpgrade.MaxMP = selectedUnit.MaxMP;
		_unitToUpgrade.BaseMaxMP = selectedUnit.BaseMaxMP;
		if (_unitToUpgrade.Hero != null)
		{
			_unitToUpgrade.SoftDamage += _unitToUpgrade.Hero.SoftDamageBonus;
			_unitToUpgrade.BaseSoftDamage += _unitToUpgrade.Hero.SoftDamageBonus;
			_unitToUpgrade.HardDamage += _unitToUpgrade.Hero.HardDamageBonus;
			_unitToUpgrade.BaseHardDamage += _unitToUpgrade.Hero.HardDamageBonus;
			_unitToUpgrade.AirDamage += _unitToUpgrade.Hero.AirDamageBonus;
			_unitToUpgrade.BaseAirDamage += _unitToUpgrade.Hero.AirDamageBonus;
			_unitToUpgrade.Visibility += _unitToUpgrade.Hero.VisibilityBonus;
			_unitToUpgrade.BaseVisibility += _unitToUpgrade.Hero.VisibilityBonus;
			_unitToUpgrade.MaxMP += _unitToUpgrade.Hero.MPBonus;
			_unitToUpgrade.BaseMaxMP += _unitToUpgrade.Hero.MPBonus;
		}
		float num3 = (float)_unitToUpgrade.CurrHP / (float)_unitToUpgrade.MaxHP;
		_unitToUpgrade.MaxHP = selectedUnit.MaxHP;
		_unitToUpgrade.BaseMaxHP = selectedUnit.BaseMaxHP;
		_unitToUpgrade.AddXP(0, p_canAddHero: false, p_showXPUI: false);
		_unitToUpgrade.CurrHP = Mathf.Clamp((int)((float)_unitToUpgrade.MaxHP * num3), 0, _unitToUpgrade.MaxHP);
		if (_unitToUpgrade.CurrMP > _unitToUpgrade.MaxMP)
		{
			_unitToUpgrade.CurrMP = _unitToUpgrade.MaxMP;
		}
		if (_unitToUpgrade.CurrAutonomy > _unitToUpgrade.MaxAutonomy)
		{
			_unitToUpgrade.CurrAutonomy = _unitToUpgrade.MaxAutonomy;
		}
		_unitToUpgrade.IsMotorized = selectedUnit.IsMotorized;
		_unitToUpgrade.IsMechanized = selectedUnit.IsMechanized;
		_unitToUpgrade.IsHorsed = selectedUnit.IsHorsed;
		_unitToUpgrade.CanCarryABomb = selectedUnit.CanCarryABomb;
		_unitToUpgrade.CanCarryBombs = selectedUnit.CanCarryBombs;
		_unitToUpgrade.CanCarryDropTank = selectedUnit.CanCarryDropTank;
		_unitToUpgrade.CanCarryRockets = selectedUnit.CanCarryRockets;
		_unitToUpgrade.CanCarryTorpedo = selectedUnit.CanCarryTorpedo;
		_unitToUpgrade.CanBeRefueledOnCarrier = selectedUnit.CanBeRefueledOnCarrier;
		_unitToUpgrade.HasSonar = selectedUnit.HasSonar;
		_unitToUpgrade.IsRADAR = selectedUnit.IsRADAR;
		_unitToUpgrade.HasDropTank = selectedUnit.HasDropTank;
		_unitToUpgrade.HasBombs = selectedUnit.HasBombs;
		_unitToUpgrade.HasRockets = selectedUnit.HasRockets;
		_unitToUpgrade.HasTorpedo = selectedUnit.HasTorpedo;
		_unitToUpgrade.IsArmouredTrain = selectedUnit.IsArmouredTrain;
		_unitToUpgrade.HasABomb = selectedUnit.HasABomb;
		_unitToUpgrade.HasFlamethrower = selectedUnit.HasFlamethrower;
		_unitToUpgrade.IsParatrooper = selectedUnit.IsParatrooper;
		_unitToUpgrade.IsAmphibious = selectedUnit.IsAmphibious;
		_unitToUpgrade.CoastalBattery = selectedUnit.CoastalBattery;
		_unitToUpgrade.IsMine = selectedUnit.IsMine;
		_unitToUpgrade.NoLendLease = selectedUnit.NoLendLease;
		_unitToUpgrade.NoCapture = selectedUnit.NoCapture;
		_unitToUpgrade.PermanentDeath = selectedUnit.PermanentDeath;
		_unitToUpgrade.CanAttackShips = selectedUnit.CanAttackShips;
		_unitToUpgrade.ImmuneToWinterDamage = selectedUnit.ImmuneToWinterDamage;
		_unitToUpgrade.IsMountaineer = selectedUnit.IsMountaineer;
		_unitToUpgrade.IsWinterSpecialized = selectedUnit.IsWinterSpecialized;
		_unitToUpgrade.IsPoliticalUnit = selectedUnit.IsPoliticalUnit;
		_unitToUpgrade.IsSupplyDepot = selectedUnit.IsSupplyDepot;
		_unitToUpgrade.IsPartisans = selectedUnit.IsPartisans;
		_unitToUpgrade.IsLaunchSite = selectedUnit.IsLaunchSite;
		_unitToUpgrade.IsMissile = selectedUnit.IsMissile;
		_unitToUpgrade.IsRecon = selectedUnit.IsRecon;
		_unitToUpgrade.IsCavalry = selectedUnit.IsCavalry;
		_unitToUpgrade.IsConvoy = selectedUnit.IsConvoy;
		_unitToUpgrade.IsSupplyShip = selectedUnit.IsSupplyShip;
		_unitToUpgrade.IsDestroyer = selectedUnit.IsDestroyer;
		_unitToUpgrade.IsPatrolBoat = selectedUnit.IsPatrolBoat;
		_unitToUpgrade.IsSubmarine = selectedUnit.IsSubmarine;
		_unitToUpgrade.IsCarrier = selectedUnit.IsCarrier;
		_unitToUpgrade.IsCatapultPlane = selectedUnit.IsCatapultPlane;
		_unitToUpgrade.CanCatapultPlanes = selectedUnit.CanCatapultPlanes;
		_unitToUpgrade.IsEngineers = selectedUnit.IsEngineers;
		_unitToUpgrade.IsMineFlail = selectedUnit.IsMineFlail;
		_unitToUpgrade.BuiltByEngineers = selectedUnit.BuiltByEngineers;
		_unitToUpgrade.IsPontoonBridge = selectedUnit.IsPontoonBridge;
		_unitToUpgrade.manpowerCost = selectedUnit.manpowerCost;
		_unitToUpgrade.MoveOrAttack = selectedUnit.MoveOrAttack;
		_unitToUpgrade.IsCoreUnit = selectedUnit.IsCoreUnit;
		bool flag3 = _unitToUpgrade.CurrHP == _unitToUpgrade.MaxHP;
		if (_unitToUpgrade.IsEliteUnit)
		{
			_unitToUpgrade.IsEliteUnit = selectedUnit.IsEliteUnit;
			if (_unitToUpgrade.IsEliteUnit)
			{
				while (_unitToUpgrade.Lvl < 3)
				{
					_unitToUpgrade.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
			}
			if (flag3)
			{
				_unitToUpgrade.CurrHP = _unitToUpgrade.MaxHP;
			}
		}
		else
		{
			_unitToUpgrade.IsEliteUnit = selectedUnit.IsEliteUnit;
			if (_unitToUpgrade.IsEliteUnit)
			{
				while (_unitToUpgrade.Lvl < 3)
				{
					_unitToUpgrade.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
				if (flag3)
				{
					_unitToUpgrade.CurrHP = _unitToUpgrade.MaxHP;
				}
			}
		}
		if (flag && _unitToUpgrade.Lvl > 1)
		{
			byte b = (byte)Mathf.Clamp(_unitToUpgrade.Lvl - 1, 1, _unitToUpgrade.Lvl);
			while (_unitToUpgrade.Lvl > b)
			{
				_unitToUpgrade.AddXP(-1, p_canAddHero: false, p_showXPUI: false);
			}
		}
		TurnManager.currPlayer.RemoveManpower(num2, p_sendRPC: false);
		TurnManager.currPlayer.RemoveMoney(num, p_sendRPC: true);
		if (_unitToUpgrade.Country != TurnManager.currPlayer.Country)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.Country == _unitToUpgrade.Country)
				{
					new Notification(NotificationTypes.UNIT_PURCHASE, listOfPlayer.Name, 0f, 0f, _unitToUpgrade.Name, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
					if (_unitToUpgrade.Country != TurnManager.currPlayer.Country)
					{
						SteamManager.UnlockAchievement("Parasite");
					}
					break;
				}
			}
		}
		_unitToUpgrade.Cost = selectedUnit.Cost;
		_unitToUpgrade.HasAttacked = true;
		_unitToUpgrade.ActionPoints = 0;
		_unitToUpgrade.HasMoved = true;
		_unitToUpgrade.CurrMP = _unitToUpgrade.MaxMP;
		if (_unitToUpgrade.IsHorsed)
		{
			_unitToUpgrade.MaxAutonomy = 80;
			_unitToUpgrade.CurrAutonomy = (short)Mathf.Clamp(_unitToUpgrade.MaxAutonomy + _unitToUpgrade.MaxMP, 0, _unitToUpgrade.MaxAutonomy);
		}
		else if (_unitToUpgrade.IsMotorized)
		{
			_unitToUpgrade.MaxAutonomy = 50;
			_unitToUpgrade.CurrAutonomy = (short)Mathf.Clamp(_unitToUpgrade.MaxAutonomy + _unitToUpgrade.MaxMP, 0, _unitToUpgrade.MaxAutonomy);
		}
		else if (_unitToUpgrade.IsMechanized)
		{
			_unitToUpgrade.MaxAutonomy = 30;
			_unitToUpgrade.CurrAutonomy = (short)Mathf.Clamp(_unitToUpgrade.MaxAutonomy + _unitToUpgrade.MaxMP, 0, _unitToUpgrade.MaxAutonomy);
		}
		else
		{
			_unitToUpgrade.CurrAutonomy = (short)Mathf.Clamp(_unitToUpgrade.MaxAutonomy + _unitToUpgrade.MaxMP, 0, _unitToUpgrade.MaxAutonomy);
		}
		if (!flag)
		{
			_unitToUpgrade.CurrAmmo = (short)Mathf.Clamp(unit.CurrAmmo, 0, _unitToUpgrade.MaxAmmo);
			_unitToUpgrade.CurrAutonomy = (short)Mathf.Clamp(unit.CurrAutonomy, 0, _unitToUpgrade.MaxAutonomy);
		}
		MapGO.ClearTargetedUnits();
		MapGO.instance.DestroyPotentialTilesAboveMap();
		bool flag4 = flag;
		if (flag4)
		{
			_unitToUpgrade.IsReserve = true;
			_unitToUpgrade.Waypoints = new List<int[]>();
			int cost2 = _unitToUpgrade.Cost;
			_unitToUpgrade.Cost = cost;
			_unitToUpgrade.TurnToProduce = selectedUnit.GetUpgradeProductionTime(unit2, _productionTimeOnStoreOpening);
			_unitToUpgrade.Cost = cost2;
		}
		if (flag2)
		{
			TurnManager.currPlayer.AddUnit(_unitToUpgrade, p_sendRPC: false);
		}
		if (_unitToUpgrade.unitGO != null)
		{
			if (flag4)
			{
				UnitGO unitGO = _unitToUpgrade.unitGO;
				MapGO.instance.UnselectUnit();
				if (unitGO.unit.Type == "Plane")
				{
					if (unitGO.tileGO.groundUnitGO != null)
					{
						unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
						unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
					}
					unitGO.tileGO.tile.AirUnit = null;
					unitGO.tileGO.airUnitGO = null;
				}
				else
				{
					if (unitGO.tileGO.airUnitGO != null)
					{
						unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
						unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
					}
					unitGO.tileGO.tile.GroundUnit = null;
					unitGO.tileGO.groundUnitGO = null;
				}
				MultiplayerManager.Instance.RunRPC("RPC_DespawnUnit", "O", unitGO.unit.ID, unitGO.unit.OwnerName);
				UnityEngine.Object.Destroy(unitGO.gameObject);
			}
			else
			{
				_unitToUpgrade.Type = selectedUnit.Type;
				if (_unitToUpgrade.IsCoreUnit)
				{
					_unitToUpgrade.unitGO.horse_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.mechanized_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.motorized_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.dropTank_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.bomb_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.ABomb_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.rocket_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.torpedo_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					_unitToUpgrade.unitGO.mountaineer_GO.GetComponent<SpriteRenderer>().color = MainMenu.blackColor;
					Transform[] componentsInChildren = _unitToUpgrade.unitGO.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform in componentsInChildren)
					{
						if (transform.gameObject.name == "Sub container")
						{
							transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Store/Box core");
						}
						if (transform.gameObject.name == "Box PV")
						{
							transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Store/HP box core");
						}
					}
				}
				else
				{
					_unitToUpgrade.unitGO.horse_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.mechanized_GO.GetComponent<SpriteRenderer>().color = Color.white;
					_unitToUpgrade.unitGO.motorized_GO.GetComponent<SpriteRenderer>().color = Color.white;
					_unitToUpgrade.unitGO.dropTank_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.bomb_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.ABomb_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.rocket_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.torpedo_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					_unitToUpgrade.unitGO.mountaineer_GO.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
					Transform[] componentsInChildren = _unitToUpgrade.unitGO.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform2 in componentsInChildren)
					{
						if (transform2.gameObject.name == "Sub container")
						{
							transform2.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Store/Box normal");
						}
						if (transform2.gameObject.name == "Box PV")
						{
							transform2.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Store/HP box normal");
						}
					}
				}
				_unitToUpgrade.unitGO.canUndo = false;
				_unitToUpgrade.unitGO.SetSprite();
				_unitToUpgrade.unitGO.ShowPotentialPaths();
				_unitToUpgrade.unitGO.UpdateSelectedUnitStats();
			}
		}
		_unitToUpgrade.Type = selectedUnit.Type;
		_unitToUpgrade.IsDropable = _unitToUpgrade.IsParatrooper && _unitToUpgrade.Type == "Plane";
		if (_unitToUpgrade.IsDropable)
		{
			_unitToUpgrade.MaxMP = 8;
			_unitToUpgrade.CurrMP = _unitToUpgrade.MaxMP;
			_unitToUpgrade.MaxAutonomy = 40;
			_unitToUpgrade.CurrAutonomy = _unitToUpgrade.MaxAutonomy;
		}
		_unitToUpgrade.CustomName = null;
		_unitToUpgrade.SetUnitCustomName();
		MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", _unitToUpgrade, true);
		CloseStorePanel();
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshMoneyAmountUI();
		UIManager.instance.RefreshIncomePerTurnUI();
		if (flag4)
		{
			ReservePanelGO.instance.GenerateButtonsList();
			ReservePanelGO.instance.RefreshDeployNotification();
		}
		TutorialManager.PerformAction(TutorialActions.UPGRADE_UNIT);
	}

	public void CloseStorePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	private void EmptyAllStatsText()
	{
		productionYear.text = LocalizationManager.Translate("txt-used from") + "???";
		armour.text = "0";
		softDamage.text = "0";
		hardDamage.text = "0";
		airDamage.text = "0";
		ammo.text = "0";
		fuel.text = "0";
		HP.text = "0";
		MP.text = "0";
		Range.text = "0";
		Visibility.text = "0";
		turnProduction.text = "0";
		upkeep.text = "0";
		EmptyComparisonStats();
	}

	public void EmptyComparisonStats()
	{
		armourComparison.text = string.Empty;
		softDamageComparison.text = string.Empty;
		hardDamageComparison.text = string.Empty;
		airDamageComparison.text = string.Empty;
		ammoComparison.text = string.Empty;
		fuelComparison.text = string.Empty;
		HPcomparison.text = string.Empty;
		MPComparison.text = string.Empty;
		rangeComparison.text = string.Empty;
		visibilityComparison.text = string.Empty;
		turnProductionComparison.text = string.Empty;
		upkeepComparison.text = string.Empty;
	}

	public void PopulateBasketSlots()
	{
		EmptyBasketSlots();
		foreach (Unit unit in unitsBasket)
		{
			GameObject gameObject = ((!unit.IsCoreUnit) ? UnityEngine.Object.Instantiate(basketSlotPrefab, parentForBasket_Transform) : UnityEngine.Object.Instantiate(basketSlotCorePrefab, parentForBasket_Transform));
			gameObject.name = unit.ID.ToString();
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Unit name Text")
				{
					transform.gameObject.GetComponent<TextMeshProUGUI>().text = unit.Name;
					transform.transform.parent.gameObject.GetComponent<Button>().onClick.AddListener(() =>
					{
						DuplicateBasketUnit(unit);
					});
				}
				if (transform.name == "Unit Basket Slot Quantity Text")
				{
					transform.gameObject.GetComponent<TextMeshProUGUI>().text = "1";
				}
				if (transform.name == "Unit Basket Slot Price Text")
				{
					transform.gameObject.GetComponent<TextMeshProUGUI>().text = unit.Cost.ToString();
				}
				if (transform.name == "Remove Button")
				{
					transform.gameObject.GetComponent<Button>().onClick.AddListener(() =>
					{
						RemoveUnitFromBasket(unit);
					});
				}
			}
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void SetCoreUnit()
	{
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		if (selectedUnit.IsMine || selectedUnit.IsMissile || selectedUnit.MaxMP == 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot be core unit"));
			return;
		}
		if (selectedUnit.IsCoreUnit)
		{
			selectedUnit.RevertModification(Database.coreOption);
			UIManager.ShowMessage(LocalizationManager.Translate("info-no longer core unit"));
		}
		else
		{
			selectedUnit.ApplyModification(Database.coreOption);
			UIManager.ShowMessage(LocalizationManager.Translate("info-now a core unit"));
		}
		selectedUnit.SetDefaultProductionTime();
		SetUnitPrice(selectedUnit);
		selectedUnit.CalculateAndSetManpowerCost();
		_selectedStoreSlot.RefreshValues();
		SelectUnit(selectedUnit, _selectedStoreSlot);
		if (upgradeMode)
		{
			upgradeIntoUnit.SetUpSlot(p_isUpgradeMode: true);
		}
	}

	public void ShowSelectedUnitStats(Unit p_unit)
	{
		productionYear.text = LocalizationManager.Translate("txt-used from");
		if (p_unit.ProductionMonth != 0)
		{
			productionYear.text += LocalizationManager.GetLocalizedShortDate(new DateTime(p_unit.ProductionYear, p_unit.ProductionMonth, 1));
		}
		else
		{
			productionYear.text += p_unit.ProductionYear;
		}
		armour.text = p_unit.Armour.ToString();
		softDamage.text = p_unit.SoftDamage.ToString();
		hardDamage.text = p_unit.HardDamage.ToString();
		airDamage.text = p_unit.AirDamage.ToString();
		ammo.text = p_unit.MaxAmmo.ToString();
		HP.text = p_unit.MaxHP.ToString();
		fuel.text = p_unit.MaxAutonomy.ToString();
		MP.text = p_unit.MaxMP.ToString();
		Range.text = p_unit.Range.ToString();
		Visibility.text = p_unit.Visibility.ToString();
		turnProduction.text = p_unit.TurnToProduce.ToString();
		upkeep.text = p_unit.Clone(p_generateNewID: true).GetUpkeepCost(p_getReserveCost: false).ToString();
		if (!MainMenu.isEditor)
		{
			HPcomparison.text = "";
			armourComparison.text = "";
			fuelComparison.text = "";
			ammoComparison.text = "";
			softDamageComparison.text = "";
			hardDamageComparison.text = "";
			airDamageComparison.text = "";
			rangeComparison.text = "";
			visibilityComparison.text = "";
			MPComparison.text = "";
			turnProductionComparison.text = "";
			upkeepComparison.text = "";
		}
	}

	private bool CheckAccessories(Unit unit)
	{
		int num = 0;
		if (unit.HasDropTank)
		{
			num++;
		}
		if (unit.HasTorpedo)
		{
			num++;
		}
		if (unit.HasABomb)
		{
			num++;
		}
		if (unit.HasRockets)
		{
			num++;
		}
		if (unit.HasBombs)
		{
			num++;
		}
		if (num == 2)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-planes two options max"));
			return false;
		}
		return true;
	}

	private void SelectUnit(Unit p_unit, UnitStoreSlotGO p_unitStoreSlotGO)
	{
		selectedUnit = p_unit;
		_selectedStoreSlot = p_unitStoreSlotGO;
		EventSystem.current.SetSelectedGameObject(_selectedStoreSlot.gameObject);
		ShowSelectedUnitStats(selectedUnit);
		RefreshUpgradeFromIntoUI();
		RefreshBuyButtonText();
		RefreshUpgradePrice();
	}

	private void EmptyBasket()
	{
		unitsBasket = new List<Unit>();
		basketTotalPrice_Text.text = string.Empty;
		EmptyBasketSlots();
	}

	public void EmptyAllUnitSlots()
	{
		foreach (UnitStoreSlotGO unitStoreSlotGO in _unitStoreSlotGOs)
		{
			unitStoreSlotGO.gameObject.SetActive(value: false);
		}
	}

	public void EmptyBasketSlots()
	{
		foreach (Transform item in parentForBasket_Transform)
		{
			if (item != parentForBasket_Transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void CheckAchievements(Unit unit)
	{
		if (MainMenu.isEditor)
		{
			return;
		}
		if (unit.Name == "Volkssturm")
		{
			SteamManager.UnlockAchievement("Desperate war");
		}
		if (unit.IsHorsed)
		{
			SteamManager.UnlockAchievement("HorseIt");
		}
		if (unit.IsMotorized)
		{
			SteamManager.UnlockAchievement("MotorizeIt");
		}
		if (unit.IsMechanized)
		{
			SteamManager.UnlockAchievement("MechanizeIt");
		}
		if (unit.HasDropTank)
		{
			SteamManager.UnlockAchievement("DropTank");
		}
		if (unit.HasRockets)
		{
			SteamManager.UnlockAchievement("Rockets");
		}
		if (unit.HasTorpedo)
		{
			SteamManager.UnlockAchievement("Torpedo");
		}
		if (unit.HasFlamethrower)
		{
			SteamManager.UnlockAchievement("Flamethrower");
		}
		if (unit.Name == "V1")
		{
			SteamManager.UnlockAchievement("V1");
		}
		int num = 0;
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (listOfUnit.IsRADAR && unit.FilterType == "FilterBuilding")
			{
				num++;
				if (num >= 10)
				{
					SteamManager.UnlockAchievement("Sauron's eye");
					break;
				}
			}
		}
	}
}
