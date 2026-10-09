using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TooltipFlagGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	public bool ForceShowTooltip;

	private RectTransform _canvas_RectTransform;

	private RectTransform _tooltipRectTransform;

	private GameObject _tooltip;

	private Image _image;

	private TextMeshProUGUI _tooltip_text;

	private float _timeAboveTooltip;

	private bool _mouseOverTooltip;

	private void Start()
	{
		if (SceneManager.GetActiveScene().name == "Game")
		{
			_tooltip = UIManager.instance.tooltip;
		}
		else if (SceneManager.GetActiveScene().name == "MainMenu")
		{
			_tooltip = MainMenu.instance.tooltip;
		}
		else
		{
			_tooltip = MapEditorGO.instance.tooltip;
		}
		if ((bool)_tooltip)
		{
			_tooltip_text = _tooltip.GetComponentInChildren<TextMeshProUGUI>();
			_tooltipRectTransform = _tooltip.GetComponent<RectTransform>();
			_canvas_RectTransform = _tooltip.transform.parent.gameObject.GetComponent<RectTransform>();
			_image = gameObject.GetComponent<Image>();
		}
	}

	private void Update()
	{
		if (_mouseOverTooltip)
		{
			_timeAboveTooltip += Time.deltaTime;
			if (_timeAboveTooltip >= 0.75f || ForceShowTooltip)
			{
				_tooltip.SetActive(value: true);
			}
		}
		else if (!_tooltip.activeSelf)
		{
			_mouseOverTooltip = false;
			_timeAboveTooltip = 0f;
			return;
		}
		Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
		(_tooltip.transform as RectTransform).pivot = pivot;
		Vector2 anchoredPosition = _tooltipRectTransform.anchoredPosition;
		if (anchoredPosition.x + _tooltipRectTransform.rect.width > _canvas_RectTransform.rect.width)
		{
			anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltipRectTransform.rect.width;
			_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		}
		if (anchoredPosition.x + _tooltipRectTransform.rect.height > _canvas_RectTransform.rect.height)
		{
			anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltipRectTransform.rect.height;
			_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		}
		_tooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if ((PlayerSettings.Instance.ShowTooltips || ForceShowTooltip) && gameObject.activeSelf && (!gameObject.TryGetComponent<Button>(out var component) || component.interactable))
		{
			string localizedCountry = LocalizationManager.GetLocalizedCountry(_image.sprite.name.Replace(" Flag", string.Empty).Replace(".", string.Empty).Replace("_", string.Empty));
			_tooltip_text.text = localizedCountry;
			_mouseOverTooltip = true;
		}
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		_mouseOverTooltip = false;
		_tooltip.SetActive(value: false);
		_timeAboveTooltip = 0f;
	}

	private void OnDisable()
	{
		if (!(_tooltip == null))
		{
			_mouseOverTooltip = false;
			_tooltip.SetActive(value: false);
			_timeAboveTooltip = 0f;
		}
	}
}
