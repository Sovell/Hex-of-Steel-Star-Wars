using System;

[Serializable]
public class WeatherPredictionData
{
	public int turnNumber;

	public DateTime date;

	public WeatherType actualWeather;

	public WeatherType predictedWeather;

	public float predictionAccuracy;
}
