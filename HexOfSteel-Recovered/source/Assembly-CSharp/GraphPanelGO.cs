using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using TMPro;
using UnityEngine;

public class GraphPanelGO : MonoBehaviour
{
	public static GraphPanelGO instance;

	public GameObject graphPanel;

	[Header("Text")]
	public TextMeshProUGUI graphTitle_text;

	[Header("Bar chart")]
	public CanvasBarChart canvasBarChart;

	public CategoryLabels categoryLabels;

	[Header("Graph chart")]
	public Material baseGraph_material;

	public Material baseGraphPoint_material;

	public GraphChart graphChart;

	[Header("Highlight")]
	public ChartItemEffect lineOver;

	public ChartItemEffect pointOver;

	[Header("Toggles parent groups")]
	public Transform barChartGroup;

	public Transform graphChartGroup;

	private void Awake()
	{
		instance = this;
	}

	public void ManageGraphPanel()
	{
		graphPanel.SetActive(!graphPanel.activeSelf);
		if (graphPanel.activeSelf)
		{
			if ((bool)barChartGroup)
			{
				GenerateBarChart("Units killed");
			}
			else
			{
				GenerateGraphChart("Army size");
			}
		}
	}

	public void SwitchGraphBarChart()
	{
		if (canvasBarChart.gameObject.activeSelf)
		{
			GenerateGraphChart("Army size");
		}
		else
		{
			GenerateBarChart("Units killed");
		}
	}

	public void GenerateBarChart(string p_statName)
	{
		graphTitle_text.text = p_statName;
		canvasBarChart.DataSource.ClearCategories();
		canvasBarChart.DataSource.ClearGroups();
		canvasBarChart.DataSource.ClearValues();
		canvasBarChart.gameObject.SetActive(value: true);
		barChartGroup.gameObject.SetActive(value: true);
		graphChartGroup.gameObject.SetActive(value: false);
		graphChart.gameObject.SetActive(value: false);
		Shader shader = Shader.Find("Unlit/Color");
		canvasBarChart.DataSource.StartBatch();
		canvasBarChart.DataSource.AutomaticMaxValue = true;
		canvasBarChart.DataSource.MinValue = 0.0;
		canvasBarChart.DataSource.AddGroup("All");
		List<Player> list = new List<Player>(GameData.Instance.listOfPlayers);
		switch (p_statName)
		{
		case "Units killed":
			list = list.OrderByDescending((Player x) => x.TotalUnitsDestroyed).ToList();
			break;
		case "Units lost":
			list = list.OrderByDescending((Player x) => x.TotalUnitsLost).ToList();
			break;
		case "Units fielded":
			list = list.OrderByDescending((Player x) => x.TotalUnitsBought).ToList();
			break;
		case "Money spent":
			list = list.OrderByDescending((Player x) => x.TotalMoneySpent).ToList();
			break;
		case "Money earned":
			list = list.OrderByDescending((Player x) => x.TotalMoneyEarned).ToList();
			break;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Player item in list)
		{
			Material material = new Material(shader);
			Sprite backgroundForColour = GetBackgroundForColour(item.Country);
			material.color = backgroundForColour.texture.GetPixel(backgroundForColour.texture.width / 2, backgroundForColour.texture.height / 2);
			string category;
			if (dictionary.ContainsKey(item.Country))
			{
				dictionary[item.Country]++;
				category = item.Country + dictionary[item.Country];
			}
			else
			{
				dictionary.Add(item.Country, 1);
				category = item.Country;
			}
			canvasBarChart.DataSource.AddCategory(category, material);
			float num = 0f;
			switch (p_statName)
			{
			case "Units killed":
				num = item.TotalUnitsDestroyed;
				break;
			case "Units lost":
				num = item.TotalUnitsLost;
				break;
			case "Units fielded":
				num = item.TotalUnitsBought;
				break;
			case "Money spent":
				num = item.TotalMoneySpent;
				break;
			case "Money earned":
				num = item.TotalMoneyEarned;
				break;
			}
			canvasBarChart.DataSource.SetValue(category, "All", num);
		}
		canvasBarChart.DataSource.EndBatch();
		if ((bool)canvasBarChart.GetComponent<BarAnimation>() && Time.timeScale != 0f)
		{
			canvasBarChart.GetComponent<BarAnimation>().Animate();
		}
	}

	public void GenerateGraphChart(string p_statName)
	{
		graphTitle_text.text = string.Empty;
		graphChart.DataSource.Clear();
		graphChartGroup.gameObject.SetActive(value: true);
		graphChart.gameObject.SetActive(value: true);
		canvasBarChart.gameObject.SetActive(value: false);
		barChartGroup.gameObject.SetActive(value: false);
		graphChart.DataSource.StartBatch();
		graphChart.AutoScrollHorizontally = true;
		graphChart.AutoScrollVertically = true;
		if (GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0)
		{
			graphChart.GetComponent<HorizontalAxis>().Format = AxisFormat.Date;
		}
		else
		{
			graphChart.GetComponent<HorizontalAxis>().Format = AxisFormat.Number;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			Material material = new Material(baseGraphPoint_material);
			Material material2 = new Material(baseGraph_material);
			Material material3 = new Material(baseGraph_material);
			Sprite backgroundForColour = GetBackgroundForColour(listOfPlayer.Country);
			material.color = backgroundForColour.texture.GetPixel(backgroundForColour.texture.width / 2, backgroundForColour.texture.height / 2);
			material3.color = new Color(material.color.r - 0.15f, material.color.g - 0.15f, material.color.b - 0.15f, 1f);
			material2.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
			string category;
			if (dictionary.ContainsKey(listOfPlayer.Country))
			{
				dictionary[listOfPlayer.Country]++;
				category = listOfPlayer.Country + dictionary[listOfPlayer.Country];
			}
			else
			{
				dictionary.Add(listOfPlayer.Country, 1);
				category = listOfPlayer.Country;
			}
			graphChart.DataSource.AddCategory(category, material3, 2.569999933242798, default, material2, strechFill: true, material, 11.220000267028809);
			foreach (KeyValuePair<ushort, Statistic> stat in listOfPlayer.Stats)
			{
				double y = 0.0;
				switch (p_statName)
				{
				case "Army size":
					y = stat.Value.NumberOfUnits;
					break;
				case "Controlled Victory Points":
					y = stat.Value.VPsAmount;
					break;
				case "Infantry amount":
					y = stat.Value.InfantryNumber;
					break;
				case "Planes amount":
					y = stat.Value.PlanesNumber;
					break;
				case "Tanks amount":
					y = stat.Value.TanksNumber;
					break;
				case "Artillery amount":
					y = stat.Value.ArtilleryNumber;
					break;
				case "Ships amount":
					y = stat.Value.ShipsNumber;
					break;
				case "Income":
					y = stat.Value.IncomeAmount;
					break;
				case "Fielded manpower":
					y = stat.Value.ManpowerAmount;
					break;
				}
				if (GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0)
				{
					graphChart.DataSource.AddPointToCategory(category, GameData.Instance.map.Date.AddDays(-GameData.Instance.map.NumberOfDaysThatGoesByPerTurn * (GameData.Instance.map.turnCount - stat.Key)), y);
				}
				else
				{
					graphChart.DataSource.AddPointToCategory(category, (int)stat.Key, y);
				}
			}
		}
		graphChart.DataSource.EndBatch();
		graphChart.AutoScrollHorizontally = false;
		graphChart.AutoScrollVertically = false;
	}

	private Sprite GetBackgroundForColour(string p_country)
	{
		foreach (Sprite moddedUnitsGraphic in ModsManager.moddedUnitsGraphics)
		{
			if (moddedUnitsGraphic.name.Contains(p_country) && moddedUnitsGraphic.name.Contains("Counter full square"))
			{
				return moddedUnitsGraphic;
			}
		}
		return Resources.Load<Sprite>("Units/" + p_country + "/Counter full square");
	}
}
