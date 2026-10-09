using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampaignMenu : MonoBehaviour
{
	public Image map_Image;

	public TextMeshProUGUI scenarioName_Text;

	public GameObject startCampaignButton;

	public Toggle[] countries_Toggles;

	public Toggle[] fronts_Toggles;

	public Toggle[] dates_Toggles;

	public string selectedCountry;

	public string selectedFront;

	public string selectedDate;

	public GameObject achievement;

	public GameObject YESironman;

	public GameObject NOironman;

	private void Update()
	{
		if (MainMenu.selectedMap != null)
		{
			startCampaignButton.SetActive(value: true);
		}
		else
		{
			startCampaignButton.SetActive(value: false);
		}
	}

	public void CloseCampaignPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		scenarioName_Text.text = "";
		MainMenu.selectedMap = null;
		Toggle[] array = fronts_Toggles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		array = dates_Toggles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		gameObject.SetActive(value: false);
		RefreshIronMan();
	}

	public void SelectCountry_Toggle(string p_country)
	{
		SoundManager.instance.PlayUIClickSound();
		selectedCountry = p_country;
		selectedFront = null;
		selectedDate = null;
		Toggle[] array = fronts_Toggles;
		foreach (Toggle obj in array)
		{
			obj.gameObject.SetActive(value: false);
			obj.SetIsOnWithoutNotify(value: false);
		}
		array = dates_Toggles;
		foreach (Toggle obj2 in array)
		{
			obj2.gameObject.SetActive(value: false);
			obj2.SetIsOnWithoutNotify(value: false);
		}
		array = countries_Toggles;
		foreach (Toggle obj3 in array)
		{
			obj3.SetIsOnWithoutNotify(obj3.name == p_country);
		}
		array = fronts_Toggles;
		foreach (Toggle toggle in array)
		{
			foreach (FileInfo listOfAllCampaign in MainMenu.listOfAllCampaigns)
			{
				if (listOfAllCampaign.FullName.Contains(toggle.name) && listOfAllCampaign.FullName.Contains(selectedCountry))
				{
					toggle.gameObject.SetActive(value: true);
				}
			}
		}
		map_Image.GetComponent<Image>().sprite = Resources.Load<Sprite>("CampaignMaps/Europe");
		scenarioName_Text.text = "";
		RefreshIronMan();
	}

	public void SelectFront_Toggle(string p_front)
	{
		SoundManager.instance.PlayUIClickSound();
		selectedFront = p_front;
		selectedDate = null;
		Toggle[] array = dates_Toggles;
		foreach (Toggle obj in array)
		{
			obj.gameObject.SetActive(value: false);
			obj.SetIsOnWithoutNotify(value: false);
		}
		array = fronts_Toggles;
		foreach (Toggle obj2 in array)
		{
			obj2.SetIsOnWithoutNotify(obj2.name == p_front);
		}
		array = dates_Toggles;
		foreach (Toggle toggle in array)
		{
			foreach (FileInfo listOfAllCampaign in MainMenu.listOfAllCampaigns)
			{
				if (listOfAllCampaign.FullName.Contains(toggle.name) && listOfAllCampaign.FullName.Contains(selectedFront) && listOfAllCampaign.FullName.Contains(selectedCountry))
				{
					toggle.gameObject.SetActive(value: true);
				}
			}
		}
		map_Image.GetComponent<Image>().sprite = Resources.Load<Sprite>("CampaignMaps/Europe");
		scenarioName_Text.text = "";
		RefreshIronMan();
	}

	public void SelectDate_Toggle(string p_date)
	{
		SoundManager.instance.PlayUIClickSound();
		selectedDate = p_date;
		Toggle[] array = dates_Toggles;
		foreach (Toggle obj in array)
		{
			obj.SetIsOnWithoutNotify(obj.name == p_date);
		}
		string text = "";
		foreach (FileInfo listOfAllCampaign in MainMenu.listOfAllCampaigns)
		{
			_ = listOfAllCampaign;
			if (selectedFront == "Western")
			{
				switch (selectedDate)
				{
				case "1939":
					if (selectedCountry == "Japan")
					{
						text = "Battle of Khalkhin Gol.sav";
					}
					else if (selectedCountry == "Soviet Union")
					{
						text = "Campaign of Poland.sav";
					}
					break;
				case "1940":
					if (selectedCountry == "Nazi Germany")
					{
						text = "Campaign of the low countries.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "Operation Weserubung.sav";
					}
					break;
				case "1941":
					if (selectedCountry == "Soviet Union")
					{
						text = "Moscow41.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "Defense of the Philippines.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Operation Sonnenblume.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "Operation Crusader.sav";
					}
					break;
				case "1942":
					if (selectedCountry == "Soviet Union")
					{
						text = "Stalingrad42.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "Battle of the Coral Sea.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Rommel's offensive.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "El Alamein 2.sav";
					}
					break;
				case "1943":
					if (selectedCountry == "Soviet Union")
					{
						text = "Kursk43.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "New Georgia campaign.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Defense of Sicily.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "Operation Husky.sav";
					}
					break;
				case "1944":
					if (selectedCountry == "Nazi Germany" || selectedCountry == "Great Britain")
					{
						text = "Operation Overlord.sav";
					}
					else if (selectedCountry == "Soviet Union")
					{
						text = "Operation Bagration.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "Battle of Saipan.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "Imphal44.sav";
					}
					break;
				case "1945":
					if (selectedCountry == "Soviet Union")
					{
						text = "Vistula-Oder Offensive.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Defense of Berlin.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "Battle of Berlin.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "Battle of Iwo Jima.sav";
					}
					break;
				}
			}
			else
			{
				if (!(selectedFront == "Eastern"))
				{
					continue;
				}
				switch (selectedDate)
				{
				case "1939":
					if (selectedCountry == "Nazi Germany")
					{
						text = "Campaign of Poland.sav";
					}
					else if (selectedCountry == "Soviet Union")
					{
						text = "Battle of Khalkhin Gol.sav";
					}
					else if (selectedCountry == "Finland")
					{
						text = "";
					}
					break;
				case "1941":
					if (selectedCountry == "Nazi Germany")
					{
						text = "Operation Barbarossa north.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "Philippines campaign.sav";
					}
					else if (selectedCountry == "Finland")
					{
						text = "";
					}
					break;
				case "1942":
					if (selectedCountry == "Nazi Germany")
					{
						text = "Stalingrad42.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "Battle of the Coral Sea.sav";
					}
					break;
				case "1943":
					if (selectedCountry == "USA")
					{
						text = "Operation Husky.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Kursk43.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "New Georgia campaign.sav";
					}
					break;
				case "1944":
					if (selectedCountry == "USA")
					{
						text = "Operation Overlord.sav";
					}
					else if (selectedCountry == "Nazi Germany")
					{
						text = "Operation Bagration.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "Battle of Saipan.sav";
					}
					else if (selectedCountry == "Great Britain")
					{
						text = "Imphal44.sav";
					}
					else if (selectedCountry == "Finland")
					{
						text = "";
					}
					break;
				case "1945":
					if (selectedCountry == "Nazi Germany")
					{
						text = "Operation Spring Awakening.sav";
					}
					else if (selectedCountry == "USA")
					{
						text = "Battle of Berlin.sav";
					}
					else if (selectedCountry == "Japan")
					{
						text = "Battle of Iwo Jima.sav";
					}
					break;
				}
			}
		}
		foreach (FileInfo listOfAllCampaign2 in MainMenu.listOfAllCampaigns)
		{
			if (listOfAllCampaign2.Name == text && listOfAllCampaign2.FullName.Contains(selectedCountry))
			{
				MainMenu.selectedMap = listOfAllCampaign2;
			}
		}
		try
		{
			GameData.Instance.Load(MainMenu.selectedMap.FullName);
			scenarioName_Text.text = LocalizationManager.GetLocalizedScenario(MainMenu.selectedMap);
			map_Image.sprite = Resources.Load<Sprite>("CampaignMaps/" + MainMenu.selectedMap.Name);
			RefreshIronMan();
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-loading map"));
			map_Image.sprite = Resources.Load<Sprite>("CampaignMaps/Europe");
		}
	}

	public void StartNewCampaign()
	{
		SoundManager.instance.PlayUIClickSound();
		MainMenu.instance.loading_Panel.SetActive(value: true);
		GameData.Instance.SetIronManMode(GameData.Instance.IsIronManModeON(out var _));
		GameData.Instance.map.OriginalCampaignCountry = selectedCountry;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsComputer)
			{
				if (PlayerSettings.Instance.DifficultyLevel == 3 && listOfPlayer.IncomeModifier < 20)
				{
					listOfPlayer.IncomeModifier = 20;
				}
				else if (PlayerSettings.Instance.DifficultyLevel == 4 && listOfPlayer.IncomeModifier < 30)
				{
					listOfPlayer.IncomeModifier = 30;
				}
			}
		}
		SceneManager.LoadScene("Game");
	}

	private void RefreshIronMan()
	{
		achievement.SetActive(MainMenu.selectedMap != null);
		bool flag = GameData.Instance.IsIronManModeON(out var _);
		YESironman.SetActive(flag);
		NOironman.SetActive(!flag);
	}
}
