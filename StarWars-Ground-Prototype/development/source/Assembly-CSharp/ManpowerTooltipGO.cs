using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManpowerTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public TextMeshProUGUI maxManpower_text;

	public TextMeshProUGUI availableManpower_text;

	public TextMeshProUGUI currentManpower_text;

	public TextMeshProUGUI groundForces_text;

	public TextMeshProUGUI airForces_text;

	public TextMeshProUGUI navalForces_text;

	private GameObject _tooltip;

	private RectTransform _tooltipRectTransform;

	private RectTransform _canvas_RectTransform;

	private void Start()
	{
		_tooltip = UIManager.instance.manpower_tooltip;
		_tooltipRectTransform = _tooltip.GetComponent<RectTransform>();
		_canvas_RectTransform = _tooltip.transform.parent.gameObject.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (_tooltip.activeSelf)
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(_tooltip.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = _tooltipRectTransform.anchoredPosition;
			if (anchoredPosition.x + _tooltipRectTransform.rect.width > _canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltipRectTransform.rect.width;
				_tooltipRectTransform.anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + _tooltipRectTransform.rect.height > _canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltipRectTransform.rect.height;
				_tooltipRectTransform.anchoredPosition = anchoredPosition;
			}
			_tooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		bool isAIOnly = MapGO.IsAIOnly;
		bool isComputer = TurnManager.currPlayer.IsComputer;
		bool flag = TurnManager.currPlayer.PlayersAtWar.Count > 0;
		UpdateManpowerText(maxManpower_text, "txt-max manpower", TurnManager.currPlayer.CalculateMaxManpower());
		if (!flag)
		{
			maxManpower_text.text += LocalizationManager.Translate("txt-manpower no war penalty");
		}
		UpdateManpowerText(currentManpower_text, "txt-fielded manpower", TurnManager.currPlayer.GetFieldedManpower());
		UpdateManpowerText(availableManpower_text, "txt-available manpower", TurnManager.currPlayer.GetAvailableManpower());
		availableManpower_text.text += "  (";
		int manpowerGrowth = TurnManager.currPlayer.GetManpowerGrowth();
		availableManpower_text.text += ((manpowerGrowth >= 0) ? ("+" + Utils.GetManpowerForUI(manpowerGrowth)) : Utils.GetManpowerForUI(manpowerGrowth));
		availableManpower_text.text += ")";
		UpdateManpowerText(groundForces_text, "txt-ground forces", TurnManager.currPlayer.GroundForces);
		UpdateManpowerText(airForces_text, "txt-air forces", TurnManager.currPlayer.AirForces);
		UpdateManpowerText(navalForces_text, "txt-naval forces", TurnManager.currPlayer.NavalForces);
		_tooltip.SetActive(!isComputer || isAIOnly);
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRectTransform);
		Canvas.ForceUpdateCanvases();
		TutorialManager.PerformAction(TutorialActions.OPEN_MANPOWER_WINDOW);
	}

	private void UpdateManpowerText(TextMeshProUGUI p_text, string p_localizationKey, int p_value)
	{
		p_text.text = LocalizationManager.Translate(p_localizationKey) + Utils.GetManpowerForUI(p_value);
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
