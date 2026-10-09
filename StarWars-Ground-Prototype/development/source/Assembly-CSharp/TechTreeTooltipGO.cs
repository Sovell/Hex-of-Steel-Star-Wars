using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TechTreeTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private RectTransform _canvas_RectTransform;

	private GameObject _tooltip;

	private Unit _unitBeingResearched;

	private int _researchTime;

	private void Awake()
	{
		_tooltip = ResearchMenu.instance.techTreeTooltip;
		_canvas_RectTransform = _tooltip.transform.parent.gameObject.GetComponent<RectTransform>();
	}

	private void Start()
	{
		foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
		{
			if (gameObject.name == listOfAllUnit.Name && TurnManager.currPlayer.Country == listOfAllUnit.Country)
			{
				_unitBeingResearched = listOfAllUnit;
				float researchRate = TurnManager.instance.GetResearchRate(TurnManager.currPlayer, listOfAllUnit);
				_researchTime = (int)Math.Round(1f / researchRate, 1, MidpointRounding.ToEven);
				if (_researchTime < 1)
				{
					_researchTime = 1;
				}
				break;
			}
		}
	}

	private void Update()
	{
		if (_tooltip.activeSelf)
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(_tooltip.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = _tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.width > _canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltip.GetComponent<RectTransform>().rect.width;
				_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.height > _canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltip.GetComponent<RectTransform>().rect.height;
				_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			_tooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_tooltip.SetActive(value: true);
		_unitBeingResearched.CalculateAndSetUnitPrice();
		Transform[] componentsInChildren = _tooltip.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "Name")
			{
				transform.GetComponent<TextMeshProUGUI>().text = _unitBeingResearched.Name;
				Canvas.ForceUpdateCanvases();
			}
			if (transform.name == "Research time")
			{
				if (!TurnManager.currPlayer.ListOfUnitsResearched.Contains(_unitBeingResearched.Name))
				{
					transform.gameObject.SetActive(value: true);
					transform.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Translate("txt-turns to research") + _researchTime;
				}
				else
				{
					transform.gameObject.SetActive(value: false);
				}
			}
			if (transform.name == "Cost")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.Cost.ToString();
			}
			if (transform.name == "Upkeep")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.GetUpkeepCost(p_getReserveCost: false).ToString();
			}
			if (transform.name == "Soft damage")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.SoftDamage.ToString();
			}
			if (transform.name == "Visibility")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.Visibility.ToString();
			}
			if (transform.name == "Hard damage")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.HardDamage.ToString();
			}
			if (transform.name == "Air damage")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.AirDamage.ToString();
			}
			if (transform.name == "Armour")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.Armour.ToString();
			}
			if (transform.name == "Fuel")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.MaxAutonomy.ToString();
			}
			if (transform.name == "Ammo")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.MaxAmmo.ToString();
			}
			if (transform.name == "Range")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.Range.ToString();
			}
			if (transform.name == "Movement points")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.MaxMP.ToString();
			}
			if (transform.name == "HP")
			{
				transform.GetComponentInChildren<TextMeshProUGUI>().text = _unitBeingResearched.MaxHP.ToString();
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((bool)_tooltip)
		{
			_tooltip.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if ((bool)_tooltip)
		{
			_tooltip.SetActive(value: false);
		}
	}
}
