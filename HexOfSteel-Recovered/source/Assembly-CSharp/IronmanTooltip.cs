using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IronmanTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	[SerializeField]
	private GameObject _tooltip;

	[SerializeField]
	private RectTransform _canvas_RectTransform;

	[SerializeField]
	private Sprite _valid_sprite;

	[SerializeField]
	private Sprite _invalid_sprite;

	[SerializeField]
	private Color _valid_color;

	[SerializeField]
	private Color _invalid_color;

	[SerializeField]
	private Image _fow_image;

	[SerializeField]
	private Image _zoc_image;

	[SerializeField]
	private Image _historicalFocus_image;

	[SerializeField]
	private Image _upkeep_image;

	[SerializeField]
	private Image _supplies_image;

	[SerializeField]
	private Image _production_image;

	[SerializeField]
	private Image _infiniteAmmoFuel_image;

	[SerializeField]
	private Image _difficulty_image;

	[SerializeField]
	private Image _mods_image;

	[SerializeField]
	private Image _officialScenario_image;

	[SerializeField]
	private Image _weather_image;

	[SerializeField]
	private Image _scenarioSetup_image;

	private RectTransform _tooltipRectTransform;

	private void Awake()
	{
		_tooltipRectTransform = _tooltip.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (_tooltip.activeSelf)
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(_tooltip.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = _tooltipRectTransform.anchoredPosition;
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.width > _canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltip.GetComponent<RectTransform>().rect.width;
				_tooltipRectTransform.anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.height > _canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltip.GetComponent<RectTransform>().rect.height;
				_tooltipRectTransform.anchoredPosition = anchoredPosition;
			}
			_tooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		RefreshValidations();
		_tooltip.SetActive(value: true);
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRectTransform);
		Canvas.ForceUpdateCanvases();
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		_tooltip.SetActive(value: false);
	}

	private void RefreshValidations()
	{
		GameData.Instance.IsIronManModeON(out var o_ironManOFFReasons);
		UpdateImage(_fow_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.FOW));
		UpdateImage(_zoc_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.ZOC));
		UpdateImage(_upkeep_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.UPKEEP));
		UpdateImage(_historicalFocus_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.HISTORICAL_FOCUS));
		UpdateImage(_supplies_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.REALISTIC_SUPPLIES));
		UpdateImage(_production_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.PRODUCTION_TAKES_TIME));
		UpdateImage(_infiniteAmmoFuel_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.INFINITE_AMMO_FUEL));
		UpdateImage(_difficulty_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.DIFFICULTY));
		UpdateImage(_mods_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.MODS));
		UpdateImage(_officialScenario_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.OFFICIAL_MAP));
		UpdateImage(_weather_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.WEATHER));
		UpdateImage(_scenarioSetup_image, !o_ironManOFFReasons.Contains(InvalidIronManReasons.SCENARIO_SETUP));
	}

	private void UpdateImage(Image p_image, bool p_isValid)
	{
		p_image.sprite = (p_isValid ? _valid_sprite : _invalid_sprite);
		p_image.color = (p_isValid ? _valid_color : _invalid_color);
	}

	private void OnDisable()
	{
		_tooltip.SetActive(value: false);
	}
}
