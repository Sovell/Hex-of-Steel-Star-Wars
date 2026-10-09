using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class IncomeTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	[SerializeField]
	private GameObject upkeepPart;

	[SerializeField]
	private TextMeshProUGUI baseIncome_text;

	[SerializeField]
	private TextMeshProUGUI noWar_text;

	[SerializeField]
	private TextMeshProUGUI destroyedCost_text;

	[SerializeField]
	private TextMeshProUGUI suppliesCost_text;

	[SerializeField]
	private TextMeshProUGUI upkeepCost_text;

	[SerializeField]
	private TextMeshProUGUI finalIncomeValue_text;

	[SerializeField]
	private TextMeshProUGUI infantryUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI antitankUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI antiairUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI artilleryUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI tanksUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI fightersUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI CASbombersUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI heavyBombersUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI shipsUpkeep_text;

	[SerializeField]
	private TextMeshProUGUI buildingsUpkeep_text;

	private GameObject _tooltip;

	private RectTransform _canvas_RectTransform;

	private void Start()
	{
		_tooltip = UIManager.instance.money_tooltip;
		_canvas_RectTransform = _tooltip.transform.parent.gameObject.GetComponent<RectTransform>();
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
		upkeepPart.SetActive(PlayerSettings.Instance.Upkeep);
		suppliesCost_text.gameObject.SetActive(TurnManager.currPlayer.OOBautoSupplies);
		upkeepCost_text.gameObject.SetActive(PlayerSettings.Instance.Upkeep);
		bool isAIOnly = MapGO.IsAIOnly;
		bool isComputer = TurnManager.currPlayer.IsComputer;
		bool flag = TurnManager.currPlayer.PlayersAtWar.Count > 0;
		_tooltip.SetActive(!isComputer || isAIOnly);
		TurnManager.currPlayer.CalculateTurnIncome();
		if (TurnManager.currPlayer.baseIncome > 0)
		{
			baseIncome_text.text = LocalizationManager.Translate("txt-base income") + "+" + TurnManager.currPlayer.baseIncome;
		}
		else
		{
			baseIncome_text.text = LocalizationManager.Translate("txt-base income") + TurnManager.currPlayer.baseIncome;
		}
		noWar_text.gameObject.SetActive(!flag);
		if (!flag)
		{
			noWar_text.text = LocalizationManager.Translate("txt-no war income penalty") + " " + -TurnManager.currPlayer.baseIncome / 2;
		}
		destroyedCost_text.gameObject.SetActive(TurnManager.currPlayer.tilesDestroyedIncomeLoss != 0);
		destroyedCost_text.text = LocalizationManager.Translate("txt-destroyed tiles") + TurnManager.currPlayer.tilesDestroyedIncomeLoss * -1;
		suppliesCost_text.text = LocalizationManager.Translate("txt-supplies cost") + TurnManager.currPlayer.GetTotalSuppliesCost(p_autoSuppliesOnly: true) * -1;
		upkeepCost_text.text = LocalizationManager.Translate("txt-units upkeep") + TurnManager.currPlayer.upkeepCost * -1;
		Dictionary<string, int> dictionary = new Dictionary<string, int>
		{
			{ "FilterInfantry", 0 },
			{ "FilterAntitank", 0 },
			{ "FilterAA", 0 },
			{ "FilterArtillery", 0 },
			{ "FilterTank", 0 },
			{ "FilterFighter", 0 },
			{ "FilterCAS", 0 },
			{ "FilterBomber", 0 },
			{ "FilterBoat", 0 },
			{ "FilterBuilding", 0 }
		};
		foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
		{
			if (!dictionary.ContainsKey(listOfUnit.FilterType))
			{
				dictionary.Add(listOfUnit.FilterType, listOfUnit.GetUpkeepCost(p_getReserveCost: true));
			}
			else
			{
				dictionary[listOfUnit.FilterType] += listOfUnit.GetUpkeepCost(p_getReserveCost: true);
			}
		}
		if (TurnManager.currPlayer.turnIncome > 0)
		{
			finalIncomeValue_text.text = LocalizationManager.Translate("txt-current income") + "+" + TurnManager.currPlayer.turnIncome;
		}
		else
		{
			finalIncomeValue_text.text = LocalizationManager.Translate("txt-current income") + TurnManager.currPlayer.turnIncome;
		}
		foreach (string item in dictionary.Keys.ToList())
		{
			dictionary[item] *= -1;
			if (TurnManager.currPlayer.PlayersAtWar.Count == 0)
			{
				dictionary[item] /= 2;
			}
		}
		infantryUpkeep_text.text = dictionary["FilterInfantry"].ToString();
		antitankUpkeep_text.text = dictionary["FilterAntitank"].ToString();
		antiairUpkeep_text.text = dictionary["FilterAA"].ToString();
		artilleryUpkeep_text.text = dictionary["FilterArtillery"].ToString();
		tanksUpkeep_text.text = dictionary["FilterTank"].ToString();
		fightersUpkeep_text.text = dictionary["FilterFighter"].ToString();
		CASbombersUpkeep_text.text = dictionary["FilterCAS"].ToString();
		heavyBombersUpkeep_text.text = dictionary["FilterBomber"].ToString();
		shipsUpkeep_text.text = dictionary["FilterBoat"].ToString();
		buildingsUpkeep_text.text = dictionary["FilterBuilding"].ToString();
		_tooltip.GetComponent<RectTransform>().ForceUpdateRectTransforms();
		Canvas.ForceUpdateCanvases();
		TutorialManager.PerformAction(TutorialActions.OPEN_UPKEEP_WINDOW);
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
