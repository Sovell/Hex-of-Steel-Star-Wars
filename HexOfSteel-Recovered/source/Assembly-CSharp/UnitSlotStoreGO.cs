using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSlotStoreGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	public UnitStoreSlotGO unitStoreSlotGO;

	public GameObject messagePopupPrefab;

	private GameObject _deploymentTooltip;

	private RectTransform _canvas_RectTransform;

	private void Start()
	{
		_canvas_RectTransform = StoreGO.instance.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (!(_deploymentTooltip == null))
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(_deploymentTooltip.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = _deploymentTooltip.transform.GetComponent<RectTransform>().anchoredPosition;
			if (anchoredPosition.x + _deploymentTooltip.GetComponent<RectTransform>().rect.width > _canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.width - _deploymentTooltip.GetComponent<RectTransform>().rect.width;
				_deploymentTooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + _deploymentTooltip.GetComponent<RectTransform>().rect.height > _canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.height - _deploymentTooltip.GetComponent<RectTransform>().rect.height;
				_deploymentTooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			_deploymentTooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Unit unit = unitStoreSlotGO.unit;
		_deploymentTooltip = Object.Instantiate(messagePopupPrefab, _canvas_RectTransform.transform);
		Unit unit2 = ((!StoreGO.instance.upgradeMode) ? StoreGO.instance.selectedUnit : StoreGO.instance.GetUnitToUpgrade());
		if (unit2 != null)
		{
			if (unit.MaxHP < unit2.MaxHP)
			{
				StoreGO.instance.HPcomparison.color = MainMenu.redColor;
			}
			else if (unit.MaxHP == unit2.MaxHP)
			{
				StoreGO.instance.HPcomparison.color = MainMenu.greyColor;
			}
			else if (unit.MaxHP > unit2.MaxHP)
			{
				StoreGO.instance.HPcomparison.color = MainMenu.greenColor;
			}
			if (unit.Armour < unit2.Armour)
			{
				StoreGO.instance.armourComparison.color = MainMenu.redColor;
			}
			else if (unit.Armour == unit2.Armour)
			{
				StoreGO.instance.armourComparison.color = MainMenu.greyColor;
			}
			else if (unit.Armour > unit2.Armour)
			{
				StoreGO.instance.armourComparison.color = MainMenu.greenColor;
			}
			if (unit.MaxAmmo < unit2.MaxAmmo)
			{
				StoreGO.instance.ammoComparison.color = MainMenu.redColor;
			}
			else if (unit.MaxAmmo == unit2.MaxAmmo)
			{
				StoreGO.instance.ammoComparison.color = MainMenu.greyColor;
			}
			else if (unit.MaxAmmo > unit2.MaxAmmo)
			{
				StoreGO.instance.ammoComparison.color = MainMenu.greenColor;
			}
			if (unit.MaxAutonomy < unit2.MaxAutonomy)
			{
				StoreGO.instance.fuelComparison.color = MainMenu.redColor;
			}
			else if (unit.MaxAutonomy == unit2.MaxAutonomy)
			{
				StoreGO.instance.fuelComparison.color = MainMenu.greyColor;
			}
			else if (unit.MaxAutonomy > unit2.MaxAutonomy)
			{
				StoreGO.instance.fuelComparison.color = MainMenu.greenColor;
			}
			if (unit.Range < unit2.Range)
			{
				StoreGO.instance.rangeComparison.color = MainMenu.redColor;
			}
			else if (unit.Range == unit2.Range)
			{
				StoreGO.instance.rangeComparison.color = MainMenu.greyColor;
			}
			else if (unit.Range > unit2.Range)
			{
				StoreGO.instance.rangeComparison.color = MainMenu.greenColor;
			}
			if (unit.Visibility < unit2.Visibility)
			{
				StoreGO.instance.visibilityComparison.color = MainMenu.redColor;
			}
			else if (unit.Visibility == unit2.Visibility)
			{
				StoreGO.instance.visibilityComparison.color = MainMenu.greyColor;
			}
			else if (unit.Visibility > unit2.Visibility)
			{
				StoreGO.instance.visibilityComparison.color = MainMenu.greenColor;
			}
			if (unit.MaxMP < unit2.MaxMP)
			{
				StoreGO.instance.MPComparison.color = MainMenu.redColor;
			}
			else if (unit.MaxMP == unit2.MaxMP)
			{
				StoreGO.instance.MPComparison.color = MainMenu.greyColor;
			}
			else if (unit.MaxMP > unit2.MaxMP)
			{
				StoreGO.instance.MPComparison.color = MainMenu.greenColor;
			}
			if (unit.SoftDamage < unit2.SoftDamage)
			{
				StoreGO.instance.softDamageComparison.color = MainMenu.redColor;
			}
			else if (unit.SoftDamage == unit2.SoftDamage)
			{
				StoreGO.instance.softDamageComparison.color = MainMenu.greyColor;
			}
			else if (unit.SoftDamage > unit2.SoftDamage)
			{
				StoreGO.instance.softDamageComparison.color = MainMenu.greenColor;
			}
			if (unit.HardDamage < unit2.HardDamage)
			{
				StoreGO.instance.hardDamageComparison.color = MainMenu.redColor;
			}
			else if (unit.HardDamage == unit2.HardDamage)
			{
				StoreGO.instance.hardDamageComparison.color = MainMenu.greyColor;
			}
			else if (unit.HardDamage > unit2.HardDamage)
			{
				StoreGO.instance.hardDamageComparison.color = MainMenu.greenColor;
			}
			if (unit.AirDamage < unit2.AirDamage)
			{
				StoreGO.instance.airDamageComparison.color = MainMenu.redColor;
			}
			else if (unit.AirDamage == unit2.AirDamage)
			{
				StoreGO.instance.airDamageComparison.color = MainMenu.greyColor;
			}
			else if (unit.AirDamage > unit2.AirDamage)
			{
				StoreGO.instance.airDamageComparison.color = MainMenu.greenColor;
			}
			if (StoreGO.instance.upgradeMode)
			{
				if (unit.GetUpgradeProductionTime(unit2, unit2.TurnToProduce) > unit2.TurnToProduce)
				{
					StoreGO.instance.turnProductionComparison.color = MainMenu.redColor;
				}
				else if (unit.GetUpgradeProductionTime(unit2, unit2.TurnToProduce) == unit2.TurnToProduce)
				{
					StoreGO.instance.turnProductionComparison.color = MainMenu.greyColor;
				}
				else if (unit.GetUpgradeProductionTime(unit2, unit2.TurnToProduce) < unit2.TurnToProduce)
				{
					StoreGO.instance.turnProductionComparison.color = MainMenu.greenColor;
				}
			}
			else if (unit.TurnToProduce > unit2.TurnToProduce)
			{
				StoreGO.instance.turnProductionComparison.color = MainMenu.redColor;
			}
			else if (unit.TurnToProduce == unit2.TurnToProduce)
			{
				StoreGO.instance.turnProductionComparison.color = MainMenu.greyColor;
			}
			else if (unit.TurnToProduce < unit2.TurnToProduce)
			{
				StoreGO.instance.turnProductionComparison.color = MainMenu.greenColor;
			}
			int upkeepCost = unit.Clone(p_generateNewID: true).GetUpkeepCost(p_getReserveCost: false);
			int upkeepCost2 = unit2.Clone(p_generateNewID: true).GetUpkeepCost(p_getReserveCost: false);
			if (upkeepCost > upkeepCost2)
			{
				StoreGO.instance.upkeepComparison.color = MainMenu.redColor;
			}
			else if (upkeepCost == upkeepCost2)
			{
				StoreGO.instance.upkeepComparison.color = MainMenu.greyColor;
			}
			else if (upkeepCost < upkeepCost2)
			{
				StoreGO.instance.upkeepComparison.color = MainMenu.greenColor;
			}
			if (unit2.Name != unit.Name || StoreGO.instance.upgradeMode)
			{
				StoreGO.instance.HPcomparison.text = unit.MaxHP.ToString();
				StoreGO.instance.armourComparison.text = unit.Armour.ToString();
				StoreGO.instance.fuelComparison.text = unit.MaxAutonomy.ToString();
				StoreGO.instance.ammoComparison.text = unit.MaxAmmo.ToString();
				StoreGO.instance.softDamageComparison.text = unit.SoftDamage.ToString();
				StoreGO.instance.hardDamageComparison.text = unit.HardDamage.ToString();
				StoreGO.instance.airDamageComparison.text = unit.AirDamage.ToString();
				StoreGO.instance.rangeComparison.text = unit.Range.ToString();
				StoreGO.instance.visibilityComparison.text = unit.Visibility.ToString();
				StoreGO.instance.MPComparison.text = unit.MaxMP.ToString();
				StoreGO.instance.upkeepComparison.text = upkeepCost.ToString();
				if (StoreGO.instance.upgradeMode)
				{
					StoreGO.instance.turnProductionComparison.text = unit.GetUpgradeProductionTime(unit2, unit2.TurnToProduce).ToString();
				}
				else
				{
					StoreGO.instance.turnProductionComparison.text = unit.TurnToProduce.ToString();
				}
			}
			else
			{
				StoreGO.instance.EmptyComparisonStats();
			}
		}
		TextMeshProUGUI componentInChildren = _deploymentTooltip.GetComponentInChildren<TextMeshProUGUI>();
		string informationRules = unit.GetInformationRules();
		componentInChildren.text = string.Concat(str2: unit.GetDeploymentRule(), str0: informationRules, str1: string.IsNullOrEmpty(informationRules) ? "" : "\n\n");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Object.Destroy(_deploymentTooltip);
		StoreGO.instance.EmptyComparisonStats();
	}

	private void OnDisable()
	{
		if ((bool)_deploymentTooltip)
		{
			Object.Destroy(_deploymentTooltip);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_deploymentTooltip)
		{
			Object.Destroy(_deploymentTooltip);
		}
	}
}
