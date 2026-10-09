using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitEditor : MonoBehaviour
{
	[Header("Text")]
	public TextMeshProUGUI totalNumberOfUnits_Text;

	[Header("Transform")]
	public Transform parentForListOfUnits;

	[Header("Sprite")]
	public Sprite tickedOption_Image;

	public Sprite untickedOption_Image;

	[Header("Gameobjects")]
	public SimpleSlotUI unitSlotPrefab;

	public GameObject selectedUnitBackground;

	public GameObject commanderTargetsContainer;

	public Button add_Button;

	public Button delete_Button;

	public Button copy_Button;

	public Button paste_Button;

	public Button reset_button;

	public Button resetAll_button;

	public GameObject vertical_scrollbar;

	[Header("Toggles")]
	public Toggle isCommanderFilter_Toggle;

	public Toggle moddedUnitsOnly_Toggle;

	public List<Toggle> commanderTargets_toggles;

	[Header("InputFields")]
	public TMP_InputField research_InputField;

	public TMP_InputField unitName_InputField;

	public TMP_InputField unitProductionYear_InputField;

	public TMP_InputField unitProductionMonth_InputField;

	public TMP_InputField unitEndOfUseYear_InputField;

	public TMP_InputField unitRange_InputField;

	public TMP_InputField unitCost_InputField;

	public TMP_InputField unitManpowerCost_InputField;

	public TMP_InputField unitTurnProduction_InputField;

	public TMP_InputField unitVisibility_InputField;

	public TMP_InputField unitSoftDamage_InputField;

	public TMP_InputField unitHardDamage_InputField;

	public TMP_InputField unitAirDamage_InputField;

	public TMP_InputField unitMaxAmmo_InputField;

	public TMP_InputField unitMaxMP_InputField;

	public TMP_InputField unitMaxAutonomy_InputField;

	public TMP_InputField unitArmour_InputField;

	public TMP_InputField unitMaxHP_InputField;

	public TMP_InputField maxPurchaseTimes_InputField;

	public TMP_InputField upkeepOverride_InputField;

	public TMP_InputField progressiveCost_InputField;

	public TMP_InputField AIpurchaseChanceOverride_InputField;

	[Header("Dropdowns")]
	public TMP_Dropdown mods_Dropdown;

	public TMP_Dropdown filterCountry_Dropdown;

	public TMP_Dropdown filterType_Dropwdown;

	public TMP_Dropdown movementSound_Dropdown;

	public TMP_Dropdown attackSound_Dropdown;

	public TMP_Dropdown unitCountry_Dropdown;

	public TMP_Dropdown unitFilterType_Dropdown;

	public TMP_Dropdown unitType_Dropdown;

	public TMP_Dropdown allowedRotations_Dropdown;

	public TMP_Dropdown availabilityOptions_Dropdown;

	[Header("Image")]
	public Image unitRepresentation_Image;

	public Image unitBackground_Image;

	public Image canCarryDropTank_Image;

	public Image canCarryBombs_Image;

	public Image canCarryTorpedo_Image;

	public Image canCarryRockets_Image;

	public Image canBeRefueledAtCarier_Image;

	public Image hasFlamethrower_Image;

	public Image isSubmarine_Image;

	public Image isArmouredTrain_Image;

	public Image isParatrooper_Image;

	public Image isRecon_Image;

	public Image isDestroyer_Image;

	public Image isCarrier_Image;

	public Image isCatapultPlane_Image;

	public Image canCatapultPlanes_Image;

	public Image isCommander_Image;

	public Image isPrototype_Image;

	public Image isSupplyShip_Image;

	public Image canCarryAbomb_Image;

	public Image isRadar_Image;

	public Image hasSonar_Image;

	public Image isSupplyDepot_Image;

	public Image isPartisans_Image;

	public Image isMissle_Image;

	public Image isLaunchSite_Image;

	public Image isMineFlail_Image;

	public Image isCavalry_Image;

	public Image isPatrolBoat_Image;

	public Image isConvoy_Image;

	public Image builtByEngineers_Image;

	public Image coastalBattery_Image;

	public Image pontoonBridge_Image;

	public Image mine_Image;

	public Image engineers_Image;

	public Image armourObstacle_Image;

	public Image amphibious_Image;

	public Image noCapture_Image;

	public Image noLendLease_Image;

	public Image permanentDeath_Image;

	public Image tileBuilding_Image;

	public Image immuneToWinterDamage_Image;

	public Image canAttackShips_Image;

	public Image moveOrAttack_Image;

	private Unit _selectedUnit;

	private SimpleSlotUI _selectedUnitSlotUI;

	private Unit _copiedUnit;

	private List<Unit> _officialUnitsList;

	private string _prevSelectedUnitCheckSum;

	private string _researchUnitInput;

	private List<AudioClip> _listOfAllMovementSounds;

	private List<AudioClip> _listOfAllAttackSounds;

	private void Awake()
	{
		_listOfAllMovementSounds = new List<AudioClip>();
		_listOfAllAttackSounds = new List<AudioClip>();
		UnityEngine.Object[] array = Resources.LoadAll("Sounds/Moving");
		for (int i = 0; i < array.Length; i++)
		{
			_listOfAllMovementSounds.Add((AudioClip)array[i]);
		}
		UnityEngine.Object[] array2 = Resources.LoadAll("Sounds/Attacking");
		for (int j = 0; j < array2.Length; j++)
		{
			_listOfAllAttackSounds.Add((AudioClip)array2[j]);
		}
	}

	private void OnEnable()
	{
		SetupUIOnEnable();
		SteamManager.SetRichPresenceStatus(SteamStatuses.MODDING);
	}

	private void OnDisable()
	{
		ResetUIOnDisable();
		SteamManager.SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			_selectedUnit = null;
			_selectedUnitSlotUI = null;
			_copiedUnit = null;
			RefreshUnitGraphics();
			movementSound_Dropdown.SetValueWithoutNotify(0);
			attackSound_Dropdown.SetValueWithoutNotify(0);
		}
		delete_Button.gameObject.SetActive(_selectedUnit != null);
		copy_Button.interactable = _selectedUnit != null;
		selectedUnitBackground.SetActive(_selectedUnit != null);
		vertical_scrollbar.SetActive(_selectedUnit != null);
		paste_Button.interactable = _copiedUnit != null && _selectedUnit != null;
		CheckSelectedUnitChanges();
		add_Button.gameObject.SetActive(_selectedUnit != null);
		add_Button.interactable = _selectedUnit != null && !Utils.IsUnitRegistered(_selectedUnit);
		reset_button.gameObject.SetActive(_selectedUnit != null);
		reset_button.interactable = _selectedUnit != null && !IsCurrentUnitIdenticalToOfficialUnit();
		bool interactable = false;
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.ModifiedBaseUnits.Units.Count > 0)
			{
				interactable = true;
				break;
			}
		}
		resetAll_button.interactable = interactable;
	}

	private void SetupUIOnEnable()
	{
		_selectedUnit = null;
		_selectedUnitSlotUI = null;
		_researchUnitInput = "";
		mods_Dropdown.options.Clear();
		mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		movementSound_Dropdown.options.Clear();
		movementSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		attackSound_Dropdown.options.Clear();
		attackSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(activeMod.Name));
		}
		foreach (AudioClip moddedMovementSound in ModsManager.moddedMovementSounds)
		{
			movementSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(moddedMovementSound.name));
		}
		foreach (AudioClip listOfAllMovementSound in _listOfAllMovementSounds)
		{
			movementSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(listOfAllMovementSound.name));
		}
		foreach (AudioClip moddedAttackSound in ModsManager.moddedAttackSounds)
		{
			attackSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(moddedAttackSound.name));
		}
		foreach (AudioClip listOfAllAttackSound in _listOfAllAttackSounds)
		{
			attackSound_Dropdown.options.Add(new TMP_Dropdown.OptionData(listOfAllAttackSound.name));
		}
		filterCountry_Dropdown.options.Clear();
		unitCountry_Dropdown.options.Clear();
		foreach (string country in Database.Countries)
		{
			filterCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
			unitCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
		}
		unitType_Dropdown.options.Clear();
		foreach (string unitType in Database.unitTypes)
		{
			unitType_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(unitType)));
		}
		filterType_Dropwdown.options.Clear();
		foreach (string unitFilter in Database.unitFilters)
		{
			filterType_Dropwdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(unitFilter)));
		}
		unitFilterType_Dropdown.options.Clear();
		foreach (string unitFilter2 in Database.unitFilters)
		{
			unitFilterType_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(unitFilter2)));
		}
		allowedRotations_Dropdown.options.Clear();
		foreach (AllowedRotations value in Enum.GetValues(typeof(AllowedRotations)))
		{
			allowedRotations_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(Unit.GetAuthorizedRotationLocalizationKey(value))));
		}
		availabilityOptions_Dropdown.options.Clear();
		foreach (AvailabilityOptions value2 in Enum.GetValues(typeof(AvailabilityOptions)))
		{
			availabilityOptions_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(Unit.GetAvailabilityOptionLocalizationKey(value2))));
		}
		OfficialUnitsORIGINAL.Load();
		_officialUnitsList = OfficialUnitsORIGINAL.GetInstance().Units;
		mods_Dropdown.RefreshShownValue();
		filterCountry_Dropdown.RefreshShownValue();
		unitCountry_Dropdown.RefreshShownValue();
		movementSound_Dropdown.RefreshShownValue();
		attackSound_Dropdown.RefreshShownValue();
		RefreshUnitList(p_generateSlots: false);
	}

	private void ResetUIOnDisable()
	{
		_selectedUnit = null;
		_selectedUnitSlotUI = null;
		_researchUnitInput = string.Empty;
		research_InputField.SetTextWithoutNotify(string.Empty);
		RefreshUnitGraphics();
		selectedUnitBackground.SetActive(value: false);
		vertical_scrollbar.SetActive(value: false);
		foreach (Transform parentForListOfUnit in parentForListOfUnits)
		{
			if (parentForListOfUnit != parentForListOfUnits)
			{
				UnityEngine.Object.Destroy(parentForListOfUnit.gameObject);
			}
		}
	}

	private bool IsCurrentUnitIdenticalToOfficialUnit()
	{
		string text = JsonConvert.SerializeObject(_selectedUnit);
		foreach (Unit officialUnits in _officialUnitsList)
		{
			if (officialUnits.Country == _selectedUnit.Country && officialUnits.Name == _selectedUnit.Name)
			{
				return text == JsonConvert.SerializeObject(officialUnits);
			}
		}
		return false;
	}

	private void CheckSelectedUnitChanges()
	{
		if (_selectedUnit == null)
		{
			_prevSelectedUnitCheckSum = null;
			return;
		}
		if (ModsManager.selectedMod == null)
		{
			_prevSelectedUnitCheckSum = null;
			return;
		}
		string text = JsonConvert.SerializeObject(_selectedUnit);
		if (_prevSelectedUnitCheckSum != null && _prevSelectedUnitCheckSum != text)
		{
			if (!ModsManager.selectedMod.ModifiedBaseUnits.Units.Contains(_selectedUnit))
			{
				ModsManager.selectedMod.ModifiedBaseUnits.Units.Add(_selectedUnit);
			}
			foreach (Unit unit in OfficialUnitsORIGINAL.GetInstance().Units)
			{
				if (unit.Country == _selectedUnit.Country && unit.Name == _selectedUnit.Name)
				{
					string text2 = JsonConvert.SerializeObject(unit);
					if (text == text2)
					{
						ModsManager.selectedMod.ModifiedBaseUnits.Units.Remove(_selectedUnit);
					}
				}
			}
		}
		_prevSelectedUnitCheckSum = text;
	}

	public void ResetAllUnitsStats()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation reset units changes"), ConfirmResetAllUnitsStats);
	}

	private void ConfirmResetAllUnitsStats()
	{
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			activeMod.ModifiedBaseUnits.Reset();
		}
		ModsManager.SaveUnitsAllMods();
		SetupUIOnEnable();
		if (_selectedUnit != null)
		{
			SelectUnit(_selectedUnit);
		}
	}

	public void ResetUnitStats()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-are you sure war"), ConfirmResetSelectedUnitStats);
	}

	private void ConfirmResetSelectedUnitStats()
	{
		SoundManager.instance.PlayUIClickSound();
		OfficialUnitsORIGINAL.Load();
		_officialUnitsList = OfficialUnitsORIGINAL.GetInstance().Units;
		foreach (Unit officialUnits in _officialUnitsList)
		{
			if (!(officialUnits.Country == _selectedUnit.Country) || !(officialUnits.Name == _selectedUnit.Name))
			{
				continue;
			}
			PropertyInfo[] properties = _selectedUnit.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				PropertyInfo[] properties2 = officialUnits.GetType().GetProperties();
				foreach (PropertyInfo propertyInfo2 in properties2)
				{
					if (propertyInfo.Name == propertyInfo2.Name && propertyInfo.PropertyType == propertyInfo2.PropertyType && propertyInfo2.GetSetMethod(nonPublic: true) != null)
					{
						propertyInfo2.SetValue(_selectedUnit, propertyInfo.GetValue(officialUnits));
						break;
					}
				}
			}
			_selectedUnit.BaseType = _selectedUnit.Type;
			officialUnits.BaseType = officialUnits.Type;
			OfficialUnits.Save();
			ModsManager.SaveUnitsAllMods();
			SetupUIOnEnable();
			_selectedUnit = officialUnits;
			SelectUnit(_selectedUnit);
			break;
		}
	}

	public void SaveAll()
	{
		OfficialUnits.Save();
		ModsManager.SaveUnitsAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-units saved"));
	}

	public void SelectModToEdit()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.selectedMod = ModsManager.FindModByName(mods_Dropdown.options[mods_Dropdown.value].text);
	}

	public void ResearchUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		_researchUnitInput = research_InputField.text;
		RefreshUnitList(p_generateSlots: true);
	}

	public void GenerateUnitList(bool p_generateSlots)
	{
		List<Unit> filteredUnits = GetFilteredUnits(out var o_uniqueUnitsAmount);
		totalNumberOfUnits_Text.text = LocalizationManager.Translate("txt-units shown") + o_uniqueUnitsAmount;
		if (!p_generateSlots)
		{
			return;
		}
		parentForListOfUnits.gameObject.SetActive(value: false);
		foreach (Unit unit in filteredUnits)
		{
			SimpleSlotUI unitSlot = UnityEngine.Object.Instantiate(unitSlotPrefab, parentForListOfUnits);
			unitSlot.name = unit.ID.ToString();
			unitSlot.textComponent.text = unit.Name;
			unitSlot.button.onClick.AddListener(() =>
			{
				SelectUnit(unit, unitSlot);
			});
		}
		parentForListOfUnits.gameObject.SetActive(value: true);
	}

	private bool IsBaseGameUnit(Unit p_unit)
	{
		return OfficialUnits.IsOfficialUnit(p_unit);
	}

	public void SelectUnit(Unit p_unit, SimpleSlotUI p_simpleSlotUI = null)
	{
		_selectedUnit = p_unit;
		_selectedUnitSlotUI = p_simpleSlotUI;
		RefreshUnitGraphics();
		ShowUnitStats();
	}

	public void RefreshUnitGraphics()
	{
		if (_selectedUnit == null)
		{
			unitBackground_Image.sprite = null;
			unitRepresentation_Image.sprite = null;
			unitBackground_Image.color = new Color(1f, 1f, 1f, 0f);
			unitRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		}
		else
		{
			unitBackground_Image.color = Color.white;
			unitRepresentation_Image.color = Color.white;
			unitBackground_Image.sprite = Utils.GetUnitBackground(_selectedUnit, null);
			unitRepresentation_Image.sprite = Utils.GetUnitSprite(_selectedUnit);
		}
	}

	public void RefreshCommanderTargets()
	{
		if (_selectedUnit == null)
		{
			commanderTargetsContainer.SetActive(value: false);
			return;
		}
		commanderTargetsContainer.SetActive(_selectedUnit.IsCommander);
		foreach (Toggle commanderTargets_toggle in commanderTargets_toggles)
		{
			commanderTargets_toggle.SetIsOnWithoutNotify(_selectedUnit.IsCommander && !string.IsNullOrEmpty(_selectedUnit.CommanderTargets) && _selectedUnit.CommanderTargets.Contains(commanderTargets_toggle.name));
		}
	}

	public void ShowUnitStats()
	{
		if (_selectedUnit == null)
		{
			return;
		}
		unitName_InputField.SetTextWithoutNotify(_selectedUnit.Name);
		unitCountry_Dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedUnit.Country, Database.Countries));
		unitCountry_Dropdown.RefreshShownValue();
		unitProductionYear_InputField.SetTextWithoutNotify(_selectedUnit.ProductionYear.ToString());
		unitProductionMonth_InputField.SetTextWithoutNotify(_selectedUnit.ProductionMonth.ToString());
		unitEndOfUseYear_InputField.SetTextWithoutNotify(_selectedUnit.EndUseYear.ToString());
		unitRange_InputField.SetTextWithoutNotify(_selectedUnit.Range.ToString());
		unitCost_InputField.SetTextWithoutNotify(_selectedUnit.CustomCost.ToString());
		unitManpowerCost_InputField.SetTextWithoutNotify(_selectedUnit.CustomManpowerCost.ToString());
		unitTurnProduction_InputField.SetTextWithoutNotify(_selectedUnit.CustomTurnProduction.ToString());
		unitVisibility_InputField.SetTextWithoutNotify(_selectedUnit.Visibility.ToString());
		unitSoftDamage_InputField.SetTextWithoutNotify(_selectedUnit.SoftDamage.ToString());
		unitHardDamage_InputField.SetTextWithoutNotify(_selectedUnit.HardDamage.ToString());
		unitAirDamage_InputField.SetTextWithoutNotify(_selectedUnit.AirDamage.ToString());
		unitMaxAmmo_InputField.SetTextWithoutNotify(_selectedUnit.MaxAmmo.ToString());
		unitMaxMP_InputField.SetTextWithoutNotify(_selectedUnit.MaxMP.ToString());
		unitMaxAutonomy_InputField.SetTextWithoutNotify(_selectedUnit.MaxAutonomy.ToString());
		unitArmour_InputField.SetTextWithoutNotify(_selectedUnit.Armour.ToString());
		unitMaxHP_InputField.SetTextWithoutNotify(_selectedUnit.MaxHP.ToString());
		maxPurchaseTimes_InputField.SetTextWithoutNotify(_selectedUnit.MaxPurchaseTime.ToString());
		upkeepOverride_InputField.SetTextWithoutNotify(_selectedUnit.CustomUpkeepValue.ToString());
		progressiveCost_InputField.SetTextWithoutNotify(_selectedUnit.ProgressiveCost.ToString());
		AIpurchaseChanceOverride_InputField.SetTextWithoutNotify(_selectedUnit.AIPurchaseChanceOverride.ToString());
		unitType_Dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedUnit.Type, Database.unitTypes));
		unitType_Dropdown.RefreshShownValue();
		unitFilterType_Dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedUnit.FilterType, Database.unitFilters));
		unitFilterType_Dropdown.RefreshShownValue();
		allowedRotations_Dropdown.SetValueWithoutNotify((int)_selectedUnit.AllowedRotation);
		allowedRotations_Dropdown.RefreshShownValue();
		availabilityOptions_Dropdown.SetValueWithoutNotify((int)_selectedUnit.AvailabilityOption);
		availabilityOptions_Dropdown.RefreshShownValue();
		RefreshUnitGraphics();
		RefreshCommanderTargets();
		bool flag = false;
		for (int i = 0; i < movementSound_Dropdown.options.Count; i++)
		{
			if (_selectedUnit.MovementSound == movementSound_Dropdown.options[i].text)
			{
				flag = true;
				movementSound_Dropdown.value = i;
			}
		}
		if (!flag)
		{
			movementSound_Dropdown.value = 0;
		}
		movementSound_Dropdown.RefreshShownValue();
		flag = false;
		for (int j = 0; j < attackSound_Dropdown.options.Count; j++)
		{
			if (_selectedUnit.AttackSound == attackSound_Dropdown.options[j].text)
			{
				flag = true;
				attackSound_Dropdown.value = j;
			}
		}
		if (!flag)
		{
			attackSound_Dropdown.value = 0;
		}
		attackSound_Dropdown.RefreshShownValue();
		canCarryDropTank_Image.sprite = (_selectedUnit.CanCarryDropTank ? tickedOption_Image : untickedOption_Image);
		canCarryBombs_Image.sprite = (_selectedUnit.CanCarryBombs ? tickedOption_Image : untickedOption_Image);
		canCarryTorpedo_Image.sprite = (_selectedUnit.CanCarryTorpedo ? tickedOption_Image : untickedOption_Image);
		canCarryRockets_Image.sprite = (_selectedUnit.CanCarryRockets ? tickedOption_Image : untickedOption_Image);
		canBeRefueledAtCarier_Image.sprite = (_selectedUnit.CanBeRefueledOnCarrier ? tickedOption_Image : untickedOption_Image);
		hasFlamethrower_Image.sprite = (_selectedUnit.HasFlamethrower ? tickedOption_Image : untickedOption_Image);
		isSubmarine_Image.sprite = (_selectedUnit.IsSubmarine ? tickedOption_Image : untickedOption_Image);
		isArmouredTrain_Image.sprite = (_selectedUnit.IsArmouredTrain ? tickedOption_Image : untickedOption_Image);
		isParatrooper_Image.sprite = (_selectedUnit.IsParatrooper ? tickedOption_Image : untickedOption_Image);
		isRecon_Image.sprite = (_selectedUnit.IsRecon ? tickedOption_Image : untickedOption_Image);
		isDestroyer_Image.sprite = (_selectedUnit.IsDestroyer ? tickedOption_Image : untickedOption_Image);
		isCarrier_Image.sprite = (_selectedUnit.IsCarrier ? tickedOption_Image : untickedOption_Image);
		isCatapultPlane_Image.sprite = (_selectedUnit.IsCatapultPlane ? tickedOption_Image : untickedOption_Image);
		canCatapultPlanes_Image.sprite = (_selectedUnit.CanCatapultPlanes ? tickedOption_Image : untickedOption_Image);
		isCommander_Image.sprite = (_selectedUnit.IsCommander ? tickedOption_Image : untickedOption_Image);
		isPrototype_Image.sprite = (_selectedUnit.IsPrototype ? tickedOption_Image : untickedOption_Image);
		isSupplyShip_Image.sprite = (_selectedUnit.IsSupplyShip ? tickedOption_Image : untickedOption_Image);
		canCarryAbomb_Image.sprite = (_selectedUnit.CanCarryABomb ? tickedOption_Image : untickedOption_Image);
		isRadar_Image.sprite = (_selectedUnit.IsRADAR ? tickedOption_Image : untickedOption_Image);
		hasSonar_Image.sprite = (_selectedUnit.HasSonar ? tickedOption_Image : untickedOption_Image);
		isSupplyDepot_Image.sprite = (_selectedUnit.IsSupplyDepot ? tickedOption_Image : untickedOption_Image);
		isPartisans_Image.sprite = (_selectedUnit.IsPartisans ? tickedOption_Image : untickedOption_Image);
		isMissle_Image.sprite = (_selectedUnit.IsMissile ? tickedOption_Image : untickedOption_Image);
		isLaunchSite_Image.sprite = (_selectedUnit.IsLaunchSite ? tickedOption_Image : untickedOption_Image);
		isMineFlail_Image.sprite = (_selectedUnit.IsMineFlail ? tickedOption_Image : untickedOption_Image);
		isCavalry_Image.sprite = (_selectedUnit.IsCavalry ? tickedOption_Image : untickedOption_Image);
		isPatrolBoat_Image.sprite = (_selectedUnit.IsPatrolBoat ? tickedOption_Image : untickedOption_Image);
		isConvoy_Image.sprite = (_selectedUnit.IsConvoy ? tickedOption_Image : untickedOption_Image);
		builtByEngineers_Image.sprite = (_selectedUnit.BuiltByEngineers ? tickedOption_Image : untickedOption_Image);
		coastalBattery_Image.sprite = (_selectedUnit.CoastalBattery ? tickedOption_Image : untickedOption_Image);
		pontoonBridge_Image.sprite = (_selectedUnit.IsPontoonBridge ? tickedOption_Image : untickedOption_Image);
		mine_Image.sprite = (_selectedUnit.IsMine ? tickedOption_Image : untickedOption_Image);
		engineers_Image.sprite = (_selectedUnit.IsEngineers ? tickedOption_Image : untickedOption_Image);
		armourObstacle_Image.sprite = (_selectedUnit.IsArmourObstacle ? tickedOption_Image : untickedOption_Image);
		amphibious_Image.sprite = (_selectedUnit.IsAmphibious ? tickedOption_Image : untickedOption_Image);
		noCapture_Image.sprite = (_selectedUnit.NoCapture ? tickedOption_Image : untickedOption_Image);
		noLendLease_Image.sprite = (_selectedUnit.NoLendLease ? tickedOption_Image : untickedOption_Image);
		permanentDeath_Image.sprite = (_selectedUnit.PermanentDeath ? tickedOption_Image : untickedOption_Image);
		tileBuilding_Image.sprite = (_selectedUnit.TileBuilding ? tickedOption_Image : untickedOption_Image);
		immuneToWinterDamage_Image.sprite = (_selectedUnit.ImmuneToWinterDamage ? tickedOption_Image : untickedOption_Image);
		canAttackShips_Image.sprite = (_selectedUnit.CanAttackShips ? tickedOption_Image : untickedOption_Image);
		moveOrAttack_Image.sprite = (_selectedUnit.MoveOrAttack ? tickedOption_Image : untickedOption_Image);
	}

	public void RefreshUnitList(bool p_generateSlots)
	{
		foreach (Transform parentForListOfUnit in parentForListOfUnits)
		{
			if (parentForListOfUnit != parentForListOfUnits)
			{
				UnityEngine.Object.Destroy(parentForListOfUnit.gameObject);
			}
		}
		GenerateUnitList(p_generateSlots);
	}

	public void SetUnitName()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.Name = unitName_InputField.text;
		unitRepresentation_Image.sprite = Utils.GetUnitSprite(_selectedUnit);
		if (_selectedUnitSlotUI != null)
		{
			_selectedUnitSlotUI.textComponent.text = _selectedUnit.Name;
		}
	}

	public void SetUnitSoftAttack()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.SoftDamage = short.Parse(unitSoftDamage_InputField.text);
	}

	public void SetUnitHardAttack()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.HardDamage = short.Parse(unitHardDamage_InputField.text);
	}

	public void SetUnitAirAttack()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.AirDamage = short.Parse(unitAirDamage_InputField.text);
	}

	public void SetUnitArmour()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.Armour = short.Parse(unitArmour_InputField.text);
	}

	public void SetUnitRange()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.Range = byte.Parse(unitRange_InputField.text);
	}

	public void SetUnitCost()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.CustomCost = int.Parse(unitCost_InputField.text);
	}

	public void SetUnitCustomManpowerCost()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.CustomManpowerCost = int.Parse(unitManpowerCost_InputField.text);
	}

	public void SetUnitCustomTurnProduction()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.CustomTurnProduction = byte.Parse(unitTurnProduction_InputField.text);
	}

	public void SetUnitVisibility()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.Visibility = byte.Parse(unitVisibility_InputField.text);
	}

	public void SetUnitMaxMP()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.MaxMP = byte.Parse(unitMaxMP_InputField.text);
	}

	public void SetUnitProgressiveCost()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.ProgressiveCost = int.Parse(progressiveCost_InputField.text);
	}

	public void SetUnitAIPurchaseChanceOverride()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.AIPurchaseChanceOverride = short.Parse(AIpurchaseChanceOverride_InputField.text);
	}

	public void SetUnitMaxHP()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.MaxHP = int.Parse(unitMaxHP_InputField.text);
	}

	public void SetUnitMaxPurchaseTimes()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.MaxPurchaseTime = short.Parse(maxPurchaseTimes_InputField.text);
	}

	public void SetUpkeepOverride()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.CustomUpkeepValue = float.Parse(upkeepOverride_InputField.text);
	}

	public void SetUnitMaxAmmo()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.MaxAmmo = short.Parse(unitMaxAmmo_InputField.text);
	}

	public void SetUnitMaxFuel()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.MaxAutonomy = short.Parse(unitMaxAutonomy_InputField.text);
	}

	public void SetProductionYear()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.ProductionYear = short.Parse(unitProductionYear_InputField.text);
	}

	public void SetProductionMonth()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.ProductionMonth = byte.Parse(unitProductionMonth_InputField.text);
	}

	public void SetEndOfUseYear()
	{
		SoundManager.instance.PlayUIClickSound();
		if (short.TryParse(unitEndOfUseYear_InputField.text, out var _))
		{
			_selectedUnit.EndUseYear = short.Parse(unitEndOfUseYear_InputField.text);
		}
		else
		{
			_selectedUnit.EndUseYear = 9999;
		}
	}

	public void SetUnitCountry()
	{
		if (_selectedUnit != null)
		{
			SoundManager.instance.PlayUIClickSound();
			_selectedUnit.Country = Database.Countries[unitCountry_Dropdown.value];
			RefreshUnitGraphics();
		}
	}

	public void SetUnitMovementSound()
	{
		_selectedUnit.MovementSound = movementSound_Dropdown.options[movementSound_Dropdown.value].text;
		SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(_selectedUnit.MovementSound));
	}

	public void SetUnitAttackSound()
	{
		_selectedUnit.AttackSound = attackSound_Dropdown.options[attackSound_Dropdown.value].text;
		SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(_selectedUnit.AttackSound));
	}

	public void SetUnitType()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.Type = Database.unitTypes[unitType_Dropdown.value];
		_selectedUnit.BaseType = _selectedUnit.Type;
		RefreshUnitGraphics();
	}

	public void SetUnitAllowedFacingDirections()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.AllowedRotation = (AllowedRotations)allowedRotations_Dropdown.value;
	}

	public void SetUnitAvailabilityOption()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.AvailabilityOption = (AvailabilityOptions)availabilityOptions_Dropdown.value;
	}

	public void SetUnitFilterType()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedUnit.FilterType = Database.unitFilters[unitFilterType_Dropdown.value];
		RefreshUnitGraphics();
	}

	public void SetCanCarryDropTank()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCarryDropTank)
		{
			_selectedUnit.CanCarryDropTank = false;
			canCarryDropTank_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCarryDropTank = true;
			canCarryDropTank_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanCarryBombs()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCarryBombs)
		{
			_selectedUnit.CanCarryBombs = false;
			canCarryBombs_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCarryBombs = true;
			canCarryBombs_Image.sprite = tickedOption_Image;
		}
	}

	public void SetArmouredTrain()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsArmouredTrain)
		{
			_selectedUnit.IsArmouredTrain = false;
			isArmouredTrain_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsArmouredTrain = true;
			isArmouredTrain_Image.sprite = tickedOption_Image;
		}
	}

	public void SetMissile()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsMissile)
		{
			_selectedUnit.IsMissile = false;
			isMissle_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsMissile = true;
			isMissle_Image.sprite = tickedOption_Image;
		}
	}

	public void SetLaunchSite()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsLaunchSite)
		{
			_selectedUnit.IsLaunchSite = false;
			isLaunchSite_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsLaunchSite = true;
			isLaunchSite_Image.sprite = tickedOption_Image;
		}
	}

	public void SetMineFlail()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsMineFlail)
		{
			_selectedUnit.IsMineFlail = false;
			isMineFlail_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsMineFlail = true;
			isMineFlail_Image.sprite = tickedOption_Image;
		}
	}

	public void SetMine()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsMine)
		{
			_selectedUnit.IsMine = false;
			mine_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsMine = true;
			mine_Image.sprite = tickedOption_Image;
		}
	}

	public void SetEngineers()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsEngineers)
		{
			_selectedUnit.IsEngineers = false;
			engineers_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsEngineers = true;
			engineers_Image.sprite = tickedOption_Image;
		}
	}

	public void SetArmourObstacle()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsArmourObstacle)
		{
			_selectedUnit.IsArmourObstacle = false;
			armourObstacle_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsArmourObstacle = true;
			armourObstacle_Image.sprite = tickedOption_Image;
		}
	}

	public void SetNoLendLease()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.NoLendLease)
		{
			_selectedUnit.NoLendLease = false;
			noLendLease_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.NoLendLease = true;
			noLendLease_Image.sprite = tickedOption_Image;
		}
	}

	public void SetPermanentDeath()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.PermanentDeath)
		{
			_selectedUnit.PermanentDeath = false;
			permanentDeath_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.PermanentDeath = true;
			permanentDeath_Image.sprite = tickedOption_Image;
		}
	}

	public void SetTileBuilding()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.TileBuilding)
		{
			_selectedUnit.TileBuilding = false;
			tileBuilding_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.TileBuilding = true;
			tileBuilding_Image.sprite = tickedOption_Image;
		}
	}

	public void SetImmuneToWinterDamage()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.ImmuneToWinterDamage)
		{
			_selectedUnit.ImmuneToWinterDamage = false;
			immuneToWinterDamage_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.ImmuneToWinterDamage = true;
			immuneToWinterDamage_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanAttackShips()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanAttackShips)
		{
			_selectedUnit.CanAttackShips = false;
			canAttackShips_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanAttackShips = true;
			canAttackShips_Image.sprite = tickedOption_Image;
		}
	}

	public void SetMoveOrAttack()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.MoveOrAttack)
		{
			_selectedUnit.MoveOrAttack = false;
			moveOrAttack_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.MoveOrAttack = true;
			moveOrAttack_Image.sprite = tickedOption_Image;
		}
	}

	public void SetNoCapture()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.NoCapture)
		{
			_selectedUnit.NoCapture = false;
			noCapture_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.NoCapture = true;
			noCapture_Image.sprite = tickedOption_Image;
		}
	}

	public void SetAmphibious()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsAmphibious)
		{
			_selectedUnit.IsAmphibious = false;
			amphibious_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsAmphibious = true;
			amphibious_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCavalry()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsCavalry)
		{
			_selectedUnit.IsCavalry = false;
			isCavalry_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsCavalry = true;
			isCavalry_Image.sprite = tickedOption_Image;
		}
	}

	public void SetPatrolBoat()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsPatrolBoat)
		{
			_selectedUnit.IsPatrolBoat = false;
			isPatrolBoat_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsPatrolBoat = true;
			isPatrolBoat_Image.sprite = tickedOption_Image;
		}
	}

	public void SetConvoy()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsConvoy)
		{
			_selectedUnit.IsConvoy = false;
			isConvoy_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsConvoy = true;
			isConvoy_Image.sprite = tickedOption_Image;
		}
	}

	public void SetHasSonar()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.HasSonar)
		{
			_selectedUnit.HasSonar = false;
			hasSonar_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.HasSonar = true;
			hasSonar_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsSupplyDepot()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsSupplyDepot)
		{
			_selectedUnit.IsSupplyDepot = false;
			isSupplyDepot_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsSupplyDepot = true;
			isSupplyDepot_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsPartisans()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsPartisans)
		{
			_selectedUnit.IsPartisans = false;
			isPartisans_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsPartisans = true;
			isPartisans_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsParatrooper()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsParatrooper)
		{
			_selectedUnit.IsParatrooper = false;
			isParatrooper_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsParatrooper = true;
			isParatrooper_Image.sprite = tickedOption_Image;
		}
	}

	public void SetRecon()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsRecon)
		{
			_selectedUnit.IsRecon = false;
			isRecon_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsRecon = true;
			isRecon_Image.sprite = tickedOption_Image;
		}
	}

	public void SetDestroyer()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsDestroyer)
		{
			_selectedUnit.IsDestroyer = false;
			isDestroyer_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsDestroyer = true;
			isDestroyer_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCarrier()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsCarrier)
		{
			_selectedUnit.IsCarrier = false;
			isCarrier_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsCarrier = true;
			isCarrier_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCatapultablePlane()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsCatapultPlane)
		{
			_selectedUnit.IsCatapultPlane = false;
			isCatapultPlane_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsCatapultPlane = true;
			isCatapultPlane_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanCatapultPlanes()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCatapultPlanes)
		{
			_selectedUnit.CanCatapultPlanes = false;
			canCatapultPlanes_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCatapultPlanes = true;
			canCatapultPlanes_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsCommander()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsCommander)
		{
			_selectedUnit.IsCommander = false;
			isCommander_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsCommander = true;
			isCommander_Image.sprite = tickedOption_Image;
		}
		commanderTargetsContainer.SetActive(_selectedUnit.IsCommander);
		RefreshUnitGraphics();
		RefreshCommanderTargets();
	}

	public void SetIsPrototype()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsPrototype)
		{
			_selectedUnit.IsPrototype = false;
			isPrototype_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsPrototype = true;
			isPrototype_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsSupplyShip()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsSupplyShip)
		{
			_selectedUnit.IsSupplyShip = false;
			isSupplyShip_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsSupplyShip = true;
			isSupplyShip_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanCarryAbomb()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCarryABomb)
		{
			_selectedUnit.CanCarryABomb = false;
			canCarryAbomb_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCarryABomb = true;
			canCarryAbomb_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsRADAR()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsRADAR)
		{
			_selectedUnit.IsRADAR = false;
			isRadar_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsRADAR = true;
			isRadar_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanCarryTorpedo()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCarryTorpedo)
		{
			_selectedUnit.CanCarryTorpedo = false;
			canCarryTorpedo_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCarryTorpedo = true;
			canCarryTorpedo_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanCarryRockets()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanCarryRockets)
		{
			_selectedUnit.CanCarryRockets = false;
			canCarryRockets_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanCarryRockets = true;
			canCarryRockets_Image.sprite = tickedOption_Image;
		}
	}

	public void SetHasFlamethrower()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.HasFlamethrower)
		{
			_selectedUnit.HasFlamethrower = false;
			hasFlamethrower_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.HasFlamethrower = true;
			hasFlamethrower_Image.sprite = tickedOption_Image;
		}
	}

	public void SetIsSubmarine()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsSubmarine)
		{
			_selectedUnit.IsSubmarine = false;
			isSubmarine_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsSubmarine = true;
			isSubmarine_Image.sprite = tickedOption_Image;
		}
	}

	public void SetBuiltByEngineers()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.BuiltByEngineers)
		{
			_selectedUnit.BuiltByEngineers = false;
			builtByEngineers_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.BuiltByEngineers = true;
			builtByEngineers_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCoastalBattery()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CoastalBattery)
		{
			_selectedUnit.CoastalBattery = false;
			coastalBattery_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CoastalBattery = true;
			coastalBattery_Image.sprite = tickedOption_Image;
		}
	}

	public void SetPontoonBridge()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.IsPontoonBridge)
		{
			_selectedUnit.IsPontoonBridge = false;
			pontoonBridge_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.IsPontoonBridge = true;
			pontoonBridge_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCanBeRefueldAtCarrier()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedUnit.CanBeRefueledOnCarrier)
		{
			_selectedUnit.CanBeRefueledOnCarrier = false;
			canBeRefueledAtCarier_Image.sprite = untickedOption_Image;
		}
		else
		{
			_selectedUnit.CanBeRefueledOnCarrier = true;
			canBeRefueledAtCarier_Image.sprite = tickedOption_Image;
		}
	}

	public void SetCommanderTarget(string p_filterType)
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrEmpty(_selectedUnit.CommanderTargets))
		{
			_selectedUnit.CommanderTargets = p_filterType;
		}
		else if (_selectedUnit.CommanderTargets.Contains(p_filterType))
		{
			_selectedUnit.CommanderTargets = _selectedUnit.CommanderTargets.Replace(p_filterType, "");
		}
		else
		{
			_selectedUnit.CommanderTargets += p_filterType;
		}
	}

	public void CopyUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		_copiedUnit = new Unit();
		PropertyInfo[] properties = _selectedUnit.GetType().GetProperties();
		PropertyInfo[] properties2 = _copiedUnit.GetType().GetProperties();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			PropertyInfo[] array2 = properties2;
			foreach (PropertyInfo propertyInfo2 in array2)
			{
				if (propertyInfo.Name == propertyInfo2.Name && propertyInfo.PropertyType == propertyInfo2.PropertyType && propertyInfo2.GetSetMethod(nonPublic: true) != null)
				{
					propertyInfo2.SetValue(_copiedUnit, propertyInfo.GetValue(_selectedUnit));
					break;
				}
			}
		}
	}

	public void PasteUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		PropertyInfo[] properties = _selectedUnit.GetType().GetProperties();
		PropertyInfo[] properties2 = _copiedUnit.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties2)
		{
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo2 in array)
			{
				if (propertyInfo.Name == propertyInfo2.Name && propertyInfo.PropertyType == propertyInfo2.PropertyType && propertyInfo2.GetSetMethod(nonPublic: true) != null)
				{
					propertyInfo2.SetValue(_selectedUnit, propertyInfo.GetValue(_copiedUnit));
					break;
				}
			}
		}
		_selectedUnit.BaseType = _selectedUnit.Type;
		_selectedUnit.GenerateNewID();
		ShowUnitStats();
	}

	public void CreateNewUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		Unit unit = new Unit();
		unit.GenerateNewID();
		_selectedUnit = unit;
		_selectedUnitSlotUI = null;
		unitRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		unitBackground_Image.color = new Color(1f, 1f, 1f, 0f);
		ShowUnitStats();
	}

	public void AddUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		bool flag = Input.GetKey(KeyCode.LeftShift) && Application.isEditor;
		if (!flag && ModsManager.selectedMod == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no mod selected"));
			return;
		}
		if (string.IsNullOrWhiteSpace(_selectedUnit.FilterType))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-unit filtertype"));
			return;
		}
		if (string.IsNullOrWhiteSpace(_selectedUnit.Type))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-unit type"));
			return;
		}
		if (string.IsNullOrWhiteSpace(_selectedUnit.Country))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-unit country"));
			return;
		}
		if (Utils.IsUnitRegistered(_selectedUnit))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-unit already exist"));
			return;
		}
		_selectedUnit.CurrHP = _selectedUnit.MaxHP;
		_selectedUnit.CurrAmmo = _selectedUnit.MaxAmmo;
		_selectedUnit.CurrAutonomy = _selectedUnit.MaxAutonomy;
		_selectedUnit.CurrMP = _selectedUnit.MaxMP;
		_selectedUnit.BaseType = _selectedUnit.Type;
		if (flag)
		{
			OfficialUnits.GetInstance().Units.Add(_selectedUnit);
		}
		else
		{
			ModsManager.selectedMod.CustomUnits.ListOfCustomUnits.Add(_selectedUnit);
		}
		_selectedUnit = null;
		_selectedUnitSlotUI = null;
		RefreshUnitList(p_generateSlots: true);
		MainMenu.instance.tooltip.SetActive(value: false);
		SteamManager.UnlockAchievement("Frankenstein");
	}

	public void RemoveUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-are you sure war"), ConfirmRemoveUnit);
	}

	private void ConfirmRemoveUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.HideToolTip();
		for (int i = 0; i < OfficialUnits.GetInstance().Units.Count; i++)
		{
			if (_selectedUnit.Name == OfficialUnits.GetInstance().Units[i].Name && _selectedUnit.ID == OfficialUnits.GetInstance().Units[i].ID)
			{
				return;
			}
		}
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			for (int j = 0; j < activeMod.CustomUnits.ListOfCustomUnits.Count; j++)
			{
				if (_selectedUnit.Name == activeMod.CustomUnits.ListOfCustomUnits[j].Name && _selectedUnit.ID == activeMod.CustomUnits.ListOfCustomUnits[j].ID)
				{
					activeMod.CustomUnits.ListOfCustomUnits.RemoveAt(j);
					_selectedUnit = null;
					_selectedUnitSlotUI = null;
					RefreshUnitList(p_generateSlots: true);
					return;
				}
			}
		}
		_selectedUnit = null;
		_selectedUnitSlotUI = null;
		RefreshUnitList(p_generateSlots: true);
		RefreshUnitGraphics();
	}

	private List<Unit> GetFilteredUnits(out int o_uniqueUnitsAmount)
	{
		o_uniqueUnitsAmount = 0;
		List<Unit> list = UnitsFilterHelper.Filter((from u in Utils.GetAllUnits(p_includeRemovedUnits: true)
			orderby u.Name
			select u).ToList(), _researchUnitInput.Trim());
		List<Unit> list2 = new List<Unit>();
		HashSet<string> hashSet = new HashSet<string>();
		List<string> list3 = new List<string>(Database.Countries);
		foreach (Unit item in list)
		{
			if (isCommanderFilter_Toggle.isOn && !item.IsCommander)
			{
				continue;
			}
			bool num = filterType_Dropwdown.value == 0 || Database.unitFilters[filterType_Dropwdown.value] == item.FilterType;
			bool flag = (filterCountry_Dropdown.value == 0) | (list3[filterCountry_Dropdown.value] == item.Country);
			bool flag2 = !moddedUnitsOnly_Toggle.isOn || (moddedUnitsOnly_Toggle.isOn && !IsBaseGameUnit(item));
			if (num && flag && flag2)
			{
				if (!hashSet.Contains(item.Name))
				{
					hashSet.Add(item.Name);
				}
				list2.Add(item);
			}
		}
		o_uniqueUnitsAmount = hashSet.Count();
		return list2;
	}

	public void ImportCSV()
	{
		SoundManager.instance.PlayUIClickSound();
		FileBrowser.SetFilters(true, new FileBrowser.Filter("Files", ".csv"));
		FileBrowser.SetDefaultFilter(".csv");
		FileBrowser.ShowLoadDialog((string[] result) =>
		{
			Success(result);
		}, Cancel, FileBrowser.PickMode.Files, allowMultiSelection: false, PrototypePaths.DataPath + "/MODS/", null, LocalizationManager.Translate("btn-import"), LocalizationManager.Translate("btn-import"));
	}

	public void ExportCSV()
	{
		SoundManager.instance.PlayUIClickSound();
		(string, Func<Unit, object>)[] source = new (string, Func<Unit, object>)[64]
		{
			("Country", (Unit unit) => unit.Country),
			("Name", (Unit unit) => unit.Name),
			("Type", (Unit unit) => unit.Type),
			("FilterType", (Unit unit) => unit.FilterType),
			("MaxHP", (Unit unit) => unit.MaxHP),
			("Armour", (Unit unit) => unit.Armour),
			("Visibility", (Unit unit) => unit.Visibility),
			("Range", (Unit unit) => unit.Range),
			("SoftDamage", (Unit unit) => unit.SoftDamage),
			("HardDamage", (Unit unit) => unit.HardDamage),
			("AirDamage", (Unit unit) => unit.AirDamage),
			("MaxAmmo", (Unit unit) => unit.MaxAmmo),
			("MaxMP", (Unit unit) => unit.MaxMP),
			("MaxAutonomy", (Unit unit) => unit.MaxAutonomy),
			("ProductionYear", (Unit unit) => unit.ProductionYear),
			("ProductionMonth", (Unit unit) => unit.ProductionMonth),
			("EndUseYear", (Unit unit) => unit.EndUseYear),
			("CustomTurnProduction", (Unit unit) => unit.CustomTurnProduction),
			("MaxPurchaseTime", (Unit unit) => unit.MaxPurchaseTime),
			("CustomCost", (Unit unit) => unit.CustomCost),
			("ProgressiveCost", (Unit unit) => unit.ProgressiveCost),
			("CustomUpkeepValue", (Unit unit) => unit.CustomUpkeepValue),
			("CustomManpowerCost", (Unit unit) => unit.CustomManpowerCost),
			("MovementSound", (Unit unit) => unit.MovementSound),
			("AttackSound", (Unit unit) => unit.AttackSound),
			("AllowedRotation", (Unit unit) => unit.AllowedRotation),
			("AvailabilityOption", (Unit unit) => unit.AvailabilityOption),
			("MoveOrAttack", (Unit unit) => unit.MoveOrAttack),
			("IsCommander", (Unit unit) => unit.IsCommander),
			("IsParatrooper", (Unit unit) => unit.IsParatrooper),
			("IsSubmarine", (Unit unit) => unit.IsSubmarine),
			("IsCarrier", (Unit unit) => unit.IsCarrier),
			("IsCatapultPlane", (Unit unit) => unit.IsCatapultPlane),
			("IsDestroyer", (Unit unit) => unit.IsDestroyer),
			("IsPatrolBoat", (Unit unit) => unit.IsPatrolBoat),
			("IsConvoy", (Unit unit) => unit.IsConvoy),
			("IsSupplyShip", (Unit unit) => unit.IsSupplyShip),
			("HasSonar", (Unit unit) => unit.HasSonar),
			("IsRADAR", (Unit unit) => unit.IsRADAR),
			("IsRecon", (Unit unit) => unit.IsRecon),
			("CoastalBattery", (Unit unit) => unit.CoastalBattery),
			("IsPrototype", (Unit unit) => unit.IsPrototype),
			("IsCavalry", (Unit unit) => unit.IsCavalry),
			("IsPartisans", (Unit unit) => unit.IsPartisans),
			("IsEngineers", (Unit unit) => unit.IsEngineers),
			("IsArmouredTrain", (Unit unit) => unit.IsArmouredTrain),
			("IsAmphibious", (Unit unit) => unit.IsAmphibious),
			("IsPontoonBridge", (Unit unit) => unit.IsPontoonBridge),
			("IsArmourObstacle", (Unit unit) => unit.IsArmourObstacle),
			("IsMissile", (Unit unit) => unit.IsMissile),
			("HasFlamethrower", (Unit unit) => unit.HasFlamethrower),
			("CanAttackShips", (Unit unit) => unit.CanAttackShips),
			("CanCarryRockets", (Unit unit) => unit.CanCarryRockets),
			("CanCarryBombs", (Unit unit) => unit.CanCarryBombs),
			("CanCarryDropTank", (Unit unit) => unit.CanCarryDropTank),
			("CanCarryTorpedo", (Unit unit) => unit.CanCarryTorpedo),
			("CanBeRefueledOnCarrier", (Unit unit) => unit.CanBeRefueledOnCarrier),
			("CanCatapultPlanes", (Unit unit) => unit.CanCatapultPlanes),
			("CanLayMines", (Unit unit) => unit.CanLayMines),
			("BuiltByEngineers", (Unit unit) => unit.BuiltByEngineers),
			("PermanentDeath", (Unit unit) => unit.PermanentDeath),
			("ImmuneToWinterDamage", (Unit unit) => unit.ImmuneToWinterDamage),
			("NoCapture", (Unit unit) => unit.NoCapture),
			("NoLendLease", (Unit unit) => unit.NoLendLease)
		};
		List<Unit> list = (from unit in GetFilteredUnits(out var _)
			orderby unit.Country, unit.Name
			select unit).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Join(",", source.Select(((string Header, Func<Unit, object> Get) c) => c.Header)));
		string text = null;
		foreach (Unit u in list)
		{
			if (text != null && u.Country != text)
			{
				stringBuilder.AppendLine();
			}
			text = u.Country;
			stringBuilder.AppendLine(string.Join(",", source.Select(((string Header, Func<Unit, object> Get) c) => Csv(E(c.Get(u))))));
		}
		string text2 = Path.Combine(PrototypePaths.DataPath, "MODS");
		Directory.CreateDirectory(text2);
		File.WriteAllText(Path.Combine(text2, "Units.csv"), stringBuilder.ToString(), Encoding.UTF8);
		Process.Start(text2);
	}

	private static string E(object v)
	{
		string text;
		if (!(v is IFormattable formattable))
		{
			text = v?.ToString();
			if (text == null)
			{
				return "";
			}
		}
		else
		{
			text = formattable.ToString(null, CultureInfo.InvariantCulture);
		}
		return text;
	}

	private static string Csv(string s)
	{
		if (s.IndexOfAny(new char[4] { ',', '"', '\n', '\r' }) >= 0)
		{
			s = "\"" + s.Replace("\"", "\"\"") + "\"";
		}
		return s;
	}

	private void Success(string[] p_values)
	{
		if (p_values == null || p_values[0] == null)
		{
			return;
		}
		string[] array = File.ReadAllLines(p_values[0]);
		if (array.Length < 2)
		{
			return;
		}
		char separator = DetectDelimiter(array[0]);
		string[] array2 = array[0].Split(separator);
		PropertyInfo[] properties = typeof(Unit).GetProperties();
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(separator);
			if (array3.All(string.IsNullOrWhiteSpace))
			{
				continue;
			}
			bool flag = false;
			Unit unit = new Unit();
			List<PropertyInfo> list = new List<PropertyInfo>();
			for (int j = 0; j < array2.Length && j < array3.Length; j++)
			{
				string header = array2[j];
				string text = array3[j];
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				PropertyInfo propertyInfo = properties.FirstOrDefault((PropertyInfo p) => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
				if (propertyInfo == null)
				{
					continue;
				}
				try
				{
					object obj;
					if (propertyInfo.PropertyType.IsEnum)
					{
						obj = ((!byte.TryParse(text, out var result)) ? Enum.Parse(propertyInfo.PropertyType, text, ignoreCase: true) : Enum.ToObject(propertyInfo.PropertyType, result));
					}
					else if (propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(double))
					{
						obj = Convert.ToDouble(text, CultureInfo.InvariantCulture);
						if (propertyInfo.PropertyType == typeof(float))
						{
							obj = (float)(double)obj;
						}
					}
					else
					{
						obj = Convert.ChangeType(text, propertyInfo.PropertyType);
					}
					list.Add(propertyInfo);
					propertyInfo.SetValue(unit, obj);
					flag = true;
				}
				catch
				{
					UIManager.ShowMessage("ERROR: " + propertyInfo.Name + " : " + unit.Name);
				}
			}
			if (string.IsNullOrEmpty(unit.Name) || string.IsNullOrEmpty(unit.Country))
			{
				flag = false;
			}
			unit.BaseType = unit.Type;
			if (!flag)
			{
				continue;
			}
			if (WasUnitAlreadyAdded(unit, out var p_originalUnit))
			{
				foreach (Unit allUnit in Utils.GetAllUnits(p_includeRemovedUnits: true))
				{
					if (!(allUnit.Country == unit.Country) || !(allUnit.Name == unit.Name))
					{
						continue;
					}
					foreach (PropertyInfo item in list)
					{
						object value = item.GetValue(unit);
						item.SetValue(allUnit, value);
					}
					allUnit.BaseType = allUnit.Type;
				}
				if (IsBaseGameUnit(p_originalUnit) && ModsManager.selectedMod != null && !ModsManager.selectedMod.ModifiedBaseUnits.Units.Contains(p_originalUnit))
				{
					ModsManager.selectedMod.ModifiedBaseUnits.Units.Add(p_originalUnit);
				}
			}
			else if (ModsManager.selectedMod == null)
			{
				OfficialUnits.GetInstance().Units.Add(unit);
			}
			else
			{
				ModsManager.selectedMod.CustomUnits.ListOfCustomUnits.Add(unit);
			}
		}
		if (_selectedUnit != null)
		{
			SelectUnit(_selectedUnit);
		}
		RefreshUnitList(p_generateSlots: true);
	}

	private void Cancel()
	{
	}

	private bool WasUnitAlreadyAdded(Unit p_unit, out Unit p_originalUnit)
	{
		p_originalUnit = null;
		foreach (Unit allUnit in Utils.GetAllUnits(p_includeRemovedUnits: true))
		{
			if (allUnit.Country == p_unit.Country && allUnit.Name == p_unit.Name)
			{
				p_originalUnit = allUnit;
				return true;
			}
		}
		return false;
	}

	private char DetectDelimiter(string p_headerLine)
	{
		char[] array = new char[4] { ',', ';', '\t', '|' };
		char result = array[0];
		int num = 0;
		char[] array2 = array;
		foreach (char delimiter in array2)
		{
			int num2 = p_headerLine.Count((char c) => c == delimiter);
			if (num2 > num)
			{
				num = num2;
				result = delimiter;
			}
		}
		return result;
	}
}
