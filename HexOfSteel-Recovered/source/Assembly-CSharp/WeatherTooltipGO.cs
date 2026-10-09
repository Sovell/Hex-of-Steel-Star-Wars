using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeatherTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	[SerializeField]
	private GameObject _tooltip;

	[SerializeField]
	private Transform _parentForSlots;

	[SerializeField]
	private WeatherSlotGO _weatherSlot_prefab;

	private RectTransform _canvas_RectTransform;

	private void Start()
	{
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
		_tooltip.SetActive(value: true);
		WeatherSlotGO[] componentsInChildren = _parentForSlots.GetComponentsInChildren<WeatherSlotGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		WeatherPredictionData currentTurnWeatherPredictionData = GameData.Instance.map.GetCurrentTurnWeatherPredictionData();
		List<WeatherPredictionData> weatherPredictionData = GameData.Instance.map.GetWeatherPredictionData(ForecastType.FULL);
		weatherPredictionData.Insert(0, currentTurnWeatherPredictionData);
		foreach (WeatherPredictionData item in weatherPredictionData)
		{
			if (!PlayerSettings.Instance.IsWeather || GameData.Instance.map.DisableWeather)
			{
				item.actualWeather = WeatherType.CLEAR;
				item.predictedWeather = WeatherType.CLEAR;
				item.predictionAccuracy = 100f;
			}
			Object.Instantiate(_weatherSlot_prefab, _parentForSlots).Setup(item);
		}
		_tooltip.GetComponent<RectTransform>().ForceUpdateRectTransforms();
		Canvas.ForceUpdateCanvases();
		TutorialManager.PerformAction(TutorialActions.OPEN_FORECAST_WINDOW);
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
