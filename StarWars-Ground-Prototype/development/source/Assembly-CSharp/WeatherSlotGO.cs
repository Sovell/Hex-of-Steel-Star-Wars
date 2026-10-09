using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherSlotGO : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Image _image;

	[SerializeField]
	private TextMeshProUGUI _predictionAccuracy_text;

	[SerializeField]
	private TextMeshProUGUI _turnCount_text;

	public void Setup(WeatherPredictionData p_weatherPredictionData)
	{
		_image.sprite = UIManager.instance.GetWeatherSprite(p_weatherPredictionData);
		if (p_weatherPredictionData.turnNumber == GameData.Instance.map.turnCount)
		{
			_turnCount_text.text = LocalizationManager.Translate("txt-current weather");
			_predictionAccuracy_text.text = string.Empty;
		}
		else
		{
			_turnCount_text.text = LocalizationManager.Translate("txt-turn") + " " + p_weatherPredictionData.turnNumber;
			_predictionAccuracy_text.text = p_weatherPredictionData.predictionAccuracy + "%";
		}
	}
}
