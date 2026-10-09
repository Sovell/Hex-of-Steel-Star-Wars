using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStoreSlotGO : MonoBehaviour
{
	public Button slot_button;

	public Button buy_button;

	public Button buyManpower_button;

	public Image limitReached_image;

	public Image tooExpensive_image;

	public Image background_image;

	public Image representation_image;

	public Image HQcost_image;

	public Image HQcostManpower_image;

	public Image cost_image;

	public Image costManpower_image;

	public Button elite_button;

	public Button horse_button;

	public Button truck_button;

	public Button mechanize_button;

	public Button bomb_button;

	public Button rocket_button;

	public Button torpedo_button;

	public Button dropTank_button;

	public Button atomBomb_button;

	public Button mountaineer_button;

	public Button winter_button;

	public Button ideologicalUnit_button;

	public GameObject recon_option;

	public GameObject flamethrower_option;

	public GameObject seaplane_option;

	public GameObject navalAircraft_option;

	public GameObject droppable_option;

	public GameObject permanentDeath_option;

	public GameObject prototype_option;

	public GameObject carrier_option;

	public GameObject radar_option;

	public GameObject sonar_option;

	public TextMeshProUGUI unitName_text;

	public TextMeshProUGUI displayName_text;

	public TextMeshProUGUI price_text;

	public TextMeshProUGUI priceManpower_text;

	public TextMeshProUGUI manpowerCost_text;

	public TextMeshProUGUI unitPurchaseLimit_text;

	public TextMeshProUGUI unitPurchaseLimitAmount_text;

	public Unit unit;

	public void ResetOptions()
	{
		slot_button.onClick.RemoveAllListeners();
		buy_button.onClick.RemoveAllListeners();
		buyManpower_button.onClick.RemoveAllListeners();
		elite_button.gameObject.SetActive(value: true);
		mountaineer_button.gameObject.SetActive(value: true);
		winter_button.gameObject.SetActive(value: true);
		horse_button.gameObject.SetActive(value: true);
		truck_button.gameObject.SetActive(value: true);
		mechanize_button.gameObject.SetActive(value: true);
		dropTank_button.gameObject.SetActive(value: true);
		bomb_button.gameObject.SetActive(value: true);
		atomBomb_button.gameObject.SetActive(value: true);
		torpedo_button.gameObject.SetActive(value: true);
		rocket_button.gameObject.SetActive(value: true);
		recon_option.SetActive(value: true);
		flamethrower_option.SetActive(value: true);
		seaplane_option.SetActive(value: true);
		navalAircraft_option.SetActive(value: true);
		droppable_option.SetActive(value: true);
		permanentDeath_option.SetActive(value: true);
		prototype_option.SetActive(value: true);
		carrier_option.SetActive(value: true);
		radar_option.SetActive(value: true);
		sonar_option.SetActive(value: true);
		displayName_text.gameObject.SetActive(value: false);
		ideologicalUnit_button.gameObject.SetActive(value: true);
		unitPurchaseLimitAmount_text.gameObject.SetActive(value: true);
		elite_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedElite_Sprite;
		elite_button.onClick.RemoveAllListeners();
		horse_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedHorse_Sprite;
		horse_button.onClick.RemoveAllListeners();
		truck_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedMotorized_Sprite;
		truck_button.onClick.RemoveAllListeners();
		mechanize_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedMechanized_Sprite;
		mechanize_button.onClick.RemoveAllListeners();
		bomb_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedBomb_Sprite;
		bomb_button.onClick.RemoveAllListeners();
		rocket_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedRocket_Sprite;
		rocket_button.onClick.RemoveAllListeners();
		torpedo_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedTorpedo_Sprite;
		torpedo_button.onClick.RemoveAllListeners();
		dropTank_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedDropTank_Sprite;
		dropTank_button.onClick.RemoveAllListeners();
		atomBomb_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedABomb_Sprite;
		atomBomb_button.onClick.RemoveAllListeners();
		mountaineer_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedMountaineer_Sprite;
		mountaineer_button.onClick.RemoveAllListeners();
		winter_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedWinterSpecialized_Sprite;
		winter_button.onClick.RemoveAllListeners();
		ideologicalUnit_button.onClick.RemoveAllListeners();
		if (limitReached_image.gameObject.activeSelf)
		{
			limitReached_image.gameObject.SetActive(value: false);
		}
	}

	public void SetUpSlot(bool p_isUpgradeMode)
	{
		name = unit.Name;
		if (PlayerSettings.Instance.Manpower)
		{
			buy_button.gameObject.SetActive(value: false);
		}
		else
		{
			buyManpower_button.gameObject.SetActive(value: false);
		}
		buy_button.interactable = !StoreGO.instance.upgradeMode;
		buyManpower_button.interactable = !StoreGO.instance.upgradeMode;
		if (unit.IsCommander || unit.MaxMP <= 1 || (unit.Type == "Plane" && !unit.IsParatrooper) || unit.Type == "Boat")
		{
			ideologicalUnit_button.gameObject.SetActive(value: false);
		}
		else if (unit.Country == "Nazi Germany" && TurnManager.currPlayer.Country == "Nazi Germany")
		{
			ideologicalUnit_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedSS_Sprite;
		}
		else if (unit.Country == "Soviet Union" && TurnManager.currPlayer.Country == "Soviet Union")
		{
			ideologicalUnit_button.GetComponent<Image>().sprite = StoreGO.instance.unselectedGuards_Sprite;
		}
		else
		{
			ideologicalUnit_button.gameObject.SetActive(value: false);
		}
		unitName_text.text = Unit.TryTranslateUnitName(unit.Name);
		price_text.text = unit.Cost.ToString();
		priceManpower_text.text = unit.Cost.ToString();
		manpowerCost_text.text = unit.GetFormattedManpowerCost();
		representation_image.sprite = Utils.GetUnitSprite(unit);
		background_image.sprite = Utils.GetUnitBackground(unit, TurnManager.currPlayer.Country);
		HQcostManpower_image.gameObject.SetActive(unit.IsCommander && PlayerSettings.Instance.Policies);
		HQcost_image.gameObject.SetActive(unit.IsCommander && PlayerSettings.Instance.Policies);
		costManpower_image.gameObject.SetActive(!HQcostManpower_image.gameObject.activeSelf);
		cost_image.gameObject.SetActive(!HQcost_image.gameObject.activeSelf);
		displayName_text.text = unit.GetCounterDisplayName();
		displayName_text.gameObject.SetActive(!string.IsNullOrEmpty(displayName_text.text));
		horse_button.gameObject.SetActive(!string.IsNullOrEmpty(unit.Type) && unit.CanBeHorseTowed());
		truck_button.gameObject.SetActive(!string.IsNullOrEmpty(unit.Type) && unit.CanBeMotorized());
		mechanize_button.gameObject.SetActive(!string.IsNullOrEmpty(unit.Type) && unit.CanBeMechanized());
		if (unit.MaxPurchaseTime > 0)
		{
			unitPurchaseLimitAmount_text.gameObject.SetActive(value: true);
			int num = 0;
			if (TurnManager.currPlayer.PurchasedUnits.ContainsKey(unit.Name))
			{
				num += TurnManager.currPlayer.PurchasedUnits[unit.Name];
			}
			foreach (Unit item in StoreGO.instance.unitsBasket)
			{
				if (item.MaxPurchaseTime > 0 && item.Name == unit.Name)
				{
					num++;
				}
			}
			unitPurchaseLimitAmount_text.text = num + "/" + unit.MaxPurchaseTime;
		}
		else
		{
			unitPurchaseLimitAmount_text.gameObject.SetActive(value: false);
		}
		if ((unit.IsMissile || unit.IsArmouredTrain || unit.MaxMP == 0 || unit.IsCommander || unit.IsConvoy) | p_isUpgradeMode)
		{
			elite_button.gameObject.SetActive(value: false);
		}
		if (unit.MaxMP == 0 || unit.Type == "Boat" || unit.IsCommander || (unit.Type == "Plane" && !unit.IsParatrooper))
		{
			winter_button.gameObject.SetActive(value: false);
		}
		if (unit.MaxMP == 0 || unit.IsCommander || unit.IsCavalry)
		{
			mountaineer_button.gameObject.SetActive(value: false);
		}
		else if (unit.FilterType != "FilterInfantry")
		{
			if (unit.FilterType == "FilterArtillery")
			{
				if (unit.SoftDamage >= 15)
				{
					mountaineer_button.gameObject.SetActive(value: false);
				}
			}
			else if (unit.FilterType == "FilterAntitank")
			{
				if (unit.HardDamage >= 60)
				{
					mountaineer_button.gameObject.SetActive(value: false);
				}
			}
			else
			{
				mountaineer_button.gameObject.SetActive(value: false);
			}
		}
		if (!unit.CanCarryABomb)
		{
			atomBomb_button.gameObject.SetActive(value: false);
		}
		if (!unit.CanCarryDropTank)
		{
			dropTank_button.gameObject.SetActive(value: false);
		}
		if (!unit.CanCarryBombs)
		{
			bomb_button.gameObject.SetActive(value: false);
		}
		if (!IsRocketAvailable())
		{
			rocket_button.gameObject.SetActive(value: false);
		}
		if (!unit.CanCarryTorpedo)
		{
			torpedo_button.gameObject.SetActive(value: false);
		}
		if (!unit.IsRecon)
		{
			recon_option.SetActive(value: false);
		}
		if (!unit.HasFlamethrower)
		{
			flamethrower_option.SetActive(value: false);
		}
		if (!unit.CanBeRefueledOnCarrier)
		{
			navalAircraft_option.SetActive(value: false);
		}
		if (!unit.IsParatrooper)
		{
			droppable_option.SetActive(value: false);
		}
		if (!unit.PermanentDeath)
		{
			permanentDeath_option.SetActive(value: false);
		}
		if (!unit.IsPrototype)
		{
			prototype_option.SetActive(value: false);
		}
		if (!unit.IsCarrier)
		{
			carrier_option.SetActive(value: false);
		}
		if (!unit.IsRADAR)
		{
			radar_option.SetActive(value: false);
		}
		if (!unit.HasSonar)
		{
			sonar_option.SetActive(value: false);
		}
		if (!unit.IsCatapultPlane && !unit.CanCatapultPlanes)
		{
			seaplane_option.SetActive(value: false);
		}
		SetUpUpgradeSlot();
		RefreshMaxPurchase(p_isUpgradeMode);
		if (!StoreGO.instance.upgradeMode && winter_button.gameObject.activeSelf && (TurnManager.currPlayer.Country == "Finland" || unit.ImmuneToWinterDamage))
		{
			unit.IsWinterSpecialized = true;
			winter_button.GetComponent<Image>().sprite = StoreGO.instance.selectedWinterSpecialized_Sprite;
		}
		ColorBlock colors = slot_button.colors;
		if (!StoreGO.instance.upgradeMode && !MainMenu.isEditor)
		{
			bool flag = ((!unit.IsCommander || !PlayerSettings.Instance.Policies) ? (unit.Cost <= TurnManager.currPlayer.MoneyAmount) : (unit.Cost <= TurnManager.currPlayer.HighCommandPoints));
			tooExpensive_image.gameObject.SetActive(!flag);
			buy_button.interactable = flag;
			buyManpower_button.interactable = flag;
			if (flag)
			{
				representation_image.color = Color.white;
				background_image.color = Color.white;
				colors.normalColor = MainMenu.greyColor;
				colors.highlightedColor = MainMenu.yellowColor;
				colors.selectedColor = MainMenu.yellowColor;
				colors.pressedColor = MainMenu.greyColor;
			}
			else
			{
				representation_image.color = new Color(0.7f, 0.7f, 0.7f, 1f);
				background_image.color = new Color(0.7f, 0.7f, 0.7f, 1f);
				colors.normalColor = new Color(0.6792453f, 0.05000966f, 0f, 1f);
				colors.highlightedColor = new Color(1f, 0f, 0f, 1f);
				colors.selectedColor = new Color(0.6792453f, 0.05000966f, 0f, 1f);
				colors.pressedColor = new Color(0.6792453f, 0.05000966f, 0f, 1f);
			}
		}
		else
		{
			representation_image.color = Color.white;
			background_image.color = Color.white;
			colors.normalColor = MainMenu.greyColor;
			colors.highlightedColor = MainMenu.yellowColor;
			colors.selectedColor = MainMenu.yellowColor;
			colors.pressedColor = MainMenu.greyColor;
			buy_button.interactable = !p_isUpgradeMode;
			buyManpower_button.interactable = !p_isUpgradeMode;
			tooExpensive_image.gameObject.SetActive(value: false);
		}
		slot_button.colors = colors;
	}

	public void RefreshMaxPurchase(bool p_isUpgradeMode)
	{
		limitReached_image.gameObject.SetActive(value: false);
		unitPurchaseLimit_text.text = unit.MaxPurchaseTime + "/" + unit.MaxPurchaseTime;
		if (unit.MaxPurchaseTime > 0)
		{
			int num = 0;
			foreach (Unit item in StoreGO.instance.unitsBasket)
			{
				if (item.MaxPurchaseTime > 0 && item.Name == unit.Name)
				{
					num++;
					if (num >= unit.MaxPurchaseTime)
					{
						limitReached_image.gameObject.SetActive(value: true);
						return;
					}
				}
			}
			if (TurnManager.currPlayer.PurchasedUnits.TryGetValue(unit.Name, out var value) && value >= unit.MaxPurchaseTime && (!p_isUpgradeMode || (p_isUpgradeMode && StoreGO.instance.GetUnitToUpgrade().Name != unit.Name)))
			{
				limitReached_image.gameObject.SetActive(value: true);
			}
		}
		if (unit.ProgressiveCost != 0)
		{
			StoreGO.instance.SetUnitPrice(unit);
			RefreshValues();
		}
	}

	public void RefreshValues()
	{
		price_text.text = unit.Cost.ToString();
		priceManpower_text.text = unit.Cost.ToString();
		manpowerCost_text.text = unit.GetFormattedManpowerCost();
	}

	private void SetUpUpgradeSlot()
	{
		if (StoreGO.instance.upgradeMode && (unit == StoreGO.instance.GetUnitToUpgrade() || unit == StoreGO.instance.selectedUnit))
		{
			elite_button.gameObject.SetActive(unit.IsEliteUnit);
			mountaineer_button.gameObject.SetActive(unit.IsMountaineer);
			winter_button.gameObject.SetActive(unit.IsWinterSpecialized);
			horse_button.gameObject.SetActive(unit.IsHorsed);
			truck_button.gameObject.SetActive(unit.IsMotorized);
			mechanize_button.gameObject.SetActive(unit.IsMechanized);
			dropTank_button.gameObject.SetActive(unit.HasDropTank);
			bomb_button.gameObject.SetActive(unit.HasBombs);
			atomBomb_button.gameObject.SetActive(unit.HasABomb);
			torpedo_button.gameObject.SetActive(unit.HasTorpedo);
			rocket_button.gameObject.SetActive(unit.HasRockets);
			recon_option.SetActive(unit.IsRecon);
			droppable_option.SetActive(unit.IsParatrooper);
			permanentDeath_option.SetActive(unit.PermanentDeath);
			flamethrower_option.SetActive(unit.HasFlamethrower);
			ideologicalUnit_button.gameObject.SetActive(unit.IsPoliticalUnit);
			elite_button.GetComponent<Image>().sprite = (unit.IsEliteUnit ? StoreGO.instance.selectedElite_Sprite : StoreGO.instance.unselectedElite_Sprite);
			mountaineer_button.GetComponent<Image>().sprite = (unit.IsMountaineer ? StoreGO.instance.selectedMountaineer_Sprite : StoreGO.instance.unselectedMountaineer_Sprite);
			winter_button.GetComponent<Image>().sprite = (unit.IsWinterSpecialized ? StoreGO.instance.selectedWinterSpecialized_Sprite : StoreGO.instance.unselectedWinterSpecialized_Sprite);
			horse_button.GetComponent<Image>().sprite = (unit.IsHorsed ? StoreGO.instance.selectedHorse_Sprite : StoreGO.instance.unselectedHorse_Sprite);
			truck_button.GetComponent<Image>().sprite = (unit.IsMotorized ? StoreGO.instance.selectedMotorized_Sprite : StoreGO.instance.unselectedMotorized_Sprite);
			mechanize_button.GetComponent<Image>().sprite = (unit.IsMechanized ? StoreGO.instance.selectedMechanized_Sprite : StoreGO.instance.unselectedMechanized_Sprite);
			dropTank_button.GetComponent<Image>().sprite = (unit.HasDropTank ? StoreGO.instance.selectedDropTank_Sprite : StoreGO.instance.unselectedDropTank_Sprite);
			rocket_button.GetComponent<Image>().sprite = (unit.HasRockets ? StoreGO.instance.selectedRocket_Sprite : StoreGO.instance.unselectedRocket_Sprite);
			torpedo_button.GetComponent<Image>().sprite = (unit.HasTorpedo ? StoreGO.instance.selectedTorpedo_Sprite : StoreGO.instance.unselectedTorpedo_Sprite);
			bomb_button.GetComponent<Image>().sprite = (unit.HasBombs ? StoreGO.instance.selectedBomb_Sprite : StoreGO.instance.unselectedBomb_Sprite);
			atomBomb_button.GetComponent<Image>().sprite = (unit.HasABomb ? StoreGO.instance.selectedABomb_Sprite : StoreGO.instance.unselectedABomb_Sprite);
			if (unit.Country == "Nazi Germany")
			{
				ideologicalUnit_button.GetComponent<Image>().sprite = (unit.IsPoliticalUnit ? StoreGO.instance.selectedSS_Sprite : StoreGO.instance.unselectedSS_Sprite);
			}
			else if (unit.Country == "Soviet Union")
			{
				ideologicalUnit_button.GetComponent<Image>().sprite = (unit.IsPoliticalUnit ? StoreGO.instance.selectedGuards_Sprite : StoreGO.instance.unselectedGuards_Sprite);
			}
		}
	}

	private bool IsRocketAvailable()
	{
		if (!unit.CanCarryRockets)
		{
			return false;
		}
		int year = GameData.Instance.map.Date.Year;
		string country = TurnManager.currPlayer.Country;
		if (country == "Soviet Union")
		{
			return year >= 1942;
		}
		if (country == "Nazi Germany")
		{
			return year >= 1943;
		}
		return year >= 1944;
	}
}
