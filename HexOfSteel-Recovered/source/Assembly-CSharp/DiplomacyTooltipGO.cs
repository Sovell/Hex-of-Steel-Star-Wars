using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiplomacyTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private GameObject _tooltip;

	private RectTransform _canvas_RectTransform;

	private Player _player;

	private bool _rebuildCanvas;

	private bool _rebuiltCanvas;

	private void Start()
	{
		_tooltip = DiplomacyGO.instance.diplomacyTooltip.gameObject;
		_canvas_RectTransform = _tooltip.transform.parent.gameObject.GetComponent<RectTransform>();
		GameData.Instance.TryFindPlayerByName(transform.parent.gameObject.name, out _player);
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

	private void LateUpdate()
	{
		if (_rebuiltCanvas)
		{
			_rebuildCanvas = false;
			_rebuiltCanvas = false;
		}
		if (_rebuildCanvas)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltip.GetComponent<RectTransform>());
			_rebuiltCanvas = true;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_tooltip.SetActive(value: true);
		DiplomacyGO.instance.tooltip_country_text.text = LocalizationManager.GetLocalizedCountry(_player.Country).Replace(".", "");
		Transform[] componentsInChildren = DiplomacyGO.instance.tooltip_atWar_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != DiplomacyGO.instance.tooltip_atWar_parent)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		componentsInChildren = DiplomacyGO.instance.tooltip_alliances_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform2 in componentsInChildren)
		{
			if (transform2 != DiplomacyGO.instance.tooltip_alliances_parent)
			{
				Object.Destroy(transform2.gameObject);
			}
		}
		componentsInChildren = DiplomacyGO.instance.tooltip_truces_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform3 in componentsInChildren)
		{
			if (transform3 != DiplomacyGO.instance.tooltip_truces_parent)
			{
				Object.Destroy(transform3.gameObject);
			}
		}
		foreach (string item in _player.PlayersAtWar)
		{
			if (GameData.Instance.TryFindPlayerByName(item, out var o_player))
			{
				GameObject gameObject = Object.Instantiate(DiplomacyGO.instance.tooltipCountryPrefab, DiplomacyGO.instance.tooltip_atWar_parent);
				gameObject.transform.GetComponentInChildren<Image>().sprite = Utils.GetFlagSprite(o_player.Country);
				gameObject.transform.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(o_player.Country).Replace(".", "");
				gameObject.SetActive(value: true);
			}
		}
		foreach (string alliedPlayer in _player.AlliedPlayers)
		{
			if (!(alliedPlayer == _player.Name) && GameData.Instance.TryFindPlayerByName(alliedPlayer, out var o_player2))
			{
				GameObject gameObject = Object.Instantiate(DiplomacyGO.instance.tooltipCountryPrefab, DiplomacyGO.instance.tooltip_alliances_parent);
				gameObject.transform.GetComponentInChildren<Image>().sprite = Utils.GetFlagSprite(o_player2.Country);
				gameObject.transform.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(o_player2.Country).Replace(".", "");
				gameObject.SetActive(value: true);
			}
		}
		foreach (KeyValuePair<string, int> truce in _player.Truces)
		{
			if (!(truce.Key == _player.Name) && GameData.Instance.TryFindPlayerByName(truce.Key, out var o_player3))
			{
				GameObject gameObject = Object.Instantiate(DiplomacyGO.instance.tooltipCountryPrefab, DiplomacyGO.instance.tooltip_truces_parent);
				gameObject.transform.GetComponentInChildren<Image>().sprite = Utils.GetFlagSprite(o_player3.Country);
				gameObject.transform.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(o_player3.Country).Replace(".", "") + " : " + truce.Value + " " + LocalizationManager.Translate("txt-turns");
				gameObject.SetActive(value: true);
			}
		}
		_rebuildCanvas = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_tooltip.SetActive(value: false);
	}
}
