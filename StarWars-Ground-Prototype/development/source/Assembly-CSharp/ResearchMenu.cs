using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchMenu : MonoBehaviour
{
	public static ResearchMenu instance;

	public GameObject yearSlotPrefab;

	public ResearchUnitSlot unitSlotPrefab;

	public Transform parent;

	public TextMeshProUGUI numberOfResearchPoints;

	public GameObject techTreeTooltip;

	public List<Image> categoriesImages;

	public List<GameObject> selectedBackgrounds;

	protected string _selectedFilter = "FilterInfantry";

	public string SelectedFilter => _selectedFilter;

	private void Awake()
	{
		instance = this;
		SelectFilter(_selectedFilter);
	}

	private void OnEnable()
	{
		SetupUnitsHeader();
		UpdateResearchPointsDisplay();
		RefreshAllSlots();
		UIManager.instance.HandleMenuStacking(gameObject);
		SteamManager.SetRichPresenceStatus(SteamStatuses.RESEARCH);
		TutorialManager.PerformAction(TutorialActions.OPEN_RESEARCH_MENU);
		if (TurnManager.currPlayer.CurrentUnitsResearch.Count > 0)
		{
			TutorialManager.PerformAction(TutorialActions.START_RESEARCH);
		}
	}

	public virtual void UpdateResearchPointsDisplay()
	{
		numberOfResearchPoints.text = LocalizationManager.Translate("txt-research points") + TurnManager.currPlayer.NumberOfAvailableResearchPoints;
	}

	public virtual void SetupUnitsHeader()
	{
		int year = GameData.Instance.map.Date.Year;
		string country = TurnManager.currPlayer.Country;
		foreach (Image categoriesImage in categoriesImages)
		{
			string headerSpritePath = GetHeaderSpritePath(country, year, categoriesImage.name);
			categoriesImage.sprite = ((headerSpritePath != null) ? Resources.Load<Sprite>(headerSpritePath) : null);
			categoriesImage.enabled = categoriesImage.sprite != null;
		}
	}

	public virtual string GetHeaderSpritePath(string p_country, int p_year, string p_categoryName)
	{
		string headerEra = GetHeaderEra(p_year);
		if (headerEra == null)
		{
			return null;
		}
		if (headerEra == "WWII")
		{
			if (IsCountryWithCustomHeader(p_country))
			{
				return "UI/Store headers/WWII/" + p_country + "/" + p_categoryName;
			}
			return "UI/Store headers/WWII/Generic/" + p_categoryName;
		}
		return "UI/Store headers/" + headerEra + "/" + p_categoryName;
	}

	public virtual string GetHeaderEra(int p_year)
	{
		if (p_year > 1920 && p_year < 1950)
		{
			return "WWII";
		}
		if (p_year > 1900 && p_year < 1939)
		{
			return "WWI";
		}
		if (p_year > 1950 && p_year < 2100)
		{
			return "Modern";
		}
		if (p_year >= 1750 && p_year < 1900)
		{
			return "Napoleon";
		}
		return null;
	}

	public virtual bool IsCountryWithCustomHeader(string p_country)
	{
		switch (p_country)
		{
		default:
			return p_country == "Spain";
		case "Nazi Germany":
		case "Italy":
		case "Soviet Union":
		case "France":
		case "USA":
		case "Great Britain":
		case "Japan":
		case "Hungary":
		case "Romania":
		case "Finland":
		case "Poland":
			return true;
		}
	}

	public void ManageTechTreePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (CanOpenTechTree())
		{
			gameObject.SetActive(!gameObject.activeSelf);
		}
	}

	public virtual bool CanOpenTechTree()
	{
		if (GameData.Instance.map.IsCampaign && !gameObject.activeSelf)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-not available in campaign"));
			return false;
		}
		if (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsLocalClientPlayer())
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-not your turn"));
			return false;
		}
		if (TurnManager.currPlayer.IsComputer && !MapGO.IsAIOnly)
		{
			return false;
		}
		if (!PlayerSettings.Instance.TechTree)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-disabled research"));
			return false;
		}
		return true;
	}

	public void SelectFilter(string p_filterName)
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedFilter = p_filterName;
		RefreshAllSlots();
		UpdateFilterVisuals(p_filterName);
	}

	public virtual void UpdateFilterVisuals(string p_filterName)
	{
		foreach (GameObject selectedBackground in selectedBackgrounds)
		{
			selectedBackground.SetActive(selectedBackground.name.Contains(p_filterName));
		}
		foreach (Image categoriesImage in categoriesImages)
		{
			categoriesImage.color = (categoriesImage.name.Contains(p_filterName) ? MainMenu.blackColor : Color.white);
		}
	}

	public virtual void GetResearchSetup(out int o_minYear, out int o_maxYear, out int o_timeStep)
	{
		int year = GameData.Instance.map.Date.Year;
		o_timeStep = 1;
		o_minYear = 1936;
		o_maxYear = 1946;
		if (year >= 1790 && year <= 1820)
		{
			o_minYear = 1790;
			o_maxYear = 1820;
		}
		else if (year >= 1861 && year <= 1865)
		{
			o_minYear = 1861;
			o_maxYear = 1865;
		}
		else if (year >= 1913 && year <= 1919)
		{
			o_minYear = 1913;
			o_maxYear = 1919;
		}
		else if (year >= 1947 && year <= 1991)
		{
			o_timeStep = 3;
			o_minYear = 1947;
			o_maxYear = 1991;
		}
		else if (year >= 2000 && year <= 2030)
		{
			o_timeStep = 3;
			o_minYear = 2000;
			o_maxYear = 2030;
		}
	}

	public virtual void RefreshAllSlots()
	{
		DeleteEverything();
		GetResearchSetup(out var o_minYear, out var o_maxYear, out var o_timeStep);
		for (int i = o_minYear; i < o_maxYear; i += o_timeStep)
		{
			GameObject p_yearSlot = CreateYearSlot(i);
			foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
			{
				if (ShouldDisplayUnit(listOfAllUnit, i, o_minYear, o_timeStep))
				{
					CreateAndSetupUnitSlot(listOfAllUnit, p_yearSlot);
				}
			}
		}
		Canvas.ForceUpdateCanvases();
		UpdateResearchPointsDisplay();
	}

	public virtual GameObject CreateYearSlot(int p_year)
	{
		GameObject gameObject = Object.Instantiate(yearSlotPrefab, parent);
		gameObject.name = p_year.ToString();
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = p_year.ToString();
		return gameObject;
	}

	public virtual bool ShouldDisplayUnit(Unit p_unit, int p_year, int p_minYear, int p_timeStep)
	{
		if (p_unit.FilterType != _selectedFilter || p_unit.Country != TurnManager.currPlayer.Country)
		{
			return false;
		}
		if (p_unit.IsPrototype && PlayerSettings.Instance.PrototypesDisabled)
		{
			return false;
		}
		if (p_unit.IsCommander)
		{
			return false;
		}
		return IsUnitInYearRange(p_unit, p_year, p_minYear, p_timeStep);
	}

	public virtual bool IsUnitInYearRange(Unit p_unit, int p_year, int p_minYear, int p_timeStep)
	{
		if (p_unit.ProductionYear != p_year && (p_year != p_minYear || p_unit.ProductionYear > p_minYear))
		{
			if (p_unit.ProductionYear > p_year - p_timeStep)
			{
				return p_unit.ProductionYear <= p_year;
			}
			return false;
		}
		return true;
	}

	public virtual ResearchUnitSlot CreateAndSetupUnitSlot(Unit p_unit, GameObject p_yearSlot)
	{
		ResearchUnitSlot unitResearchSlot = Object.Instantiate(unitSlotPrefab, p_yearSlot.transform);
		unitResearchSlot.name = p_unit.Name;
		unitResearchSlot.button.onClick.AddListener(() =>
		{
			SelectResearch(unitResearchSlot);
		});
		bool flag = ApplyResearchProgress(unitResearchSlot, p_unit);
		unitResearchSlot.Setup(p_unit);
		if (!TurnManager.currPlayer.ListOfUnitsResearched.Contains(p_unit.Name) && !flag)
		{
			unitResearchSlot.transform.Find("To research").gameObject.SetActive(value: true);
		}
		return unitResearchSlot;
	}

	public virtual bool ApplyResearchProgress(ResearchUnitSlot p_slot, Unit p_unit)
	{
		if (!TurnManager.currPlayer.CurrentUnitsResearch.ContainsKey(p_unit.Name))
		{
			return false;
		}
		p_slot.progress_slider.value = TurnManager.currPlayer.CurrentUnitsResearch[p_unit.Name];
		p_slot.progress_text.text = Utils.Truncate(TurnManager.currPlayer.CurrentUnitsResearch[p_unit.Name] * 100f, 0) + "%";
		if (TurnManager.currPlayer.CurrentUnitsResearch[p_unit.Name] < 1f)
		{
			p_slot.progress_slider.gameObject.SetActive(value: true);
		}
		return true;
	}

	public virtual void SelectResearch(ResearchUnitSlot p_researchUnitSlot)
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer.ListOfUnitsResearched.Contains(p_researchUnitSlot.unit.Name))
		{
			return;
		}
		if (TurnManager.currPlayer.CurrentUnitsResearch.ContainsKey(p_researchUnitSlot.unit.Name))
		{
			TurnManager.currPlayer.CancelResearch(p_researchUnitSlot.unit, p_sendRPC: true);
			RefreshAllSlots();
		}
		else if (TurnManager.currPlayer.NumberOfAvailableResearchPoints > 0)
		{
			Unit unit = FindUnit(p_researchUnitSlot.unit.Name);
			if (unit == null)
			{
				Debug.LogError("Couldn't find the unit.");
				return;
			}
			if (!HasCompletedPrerequisites(unit))
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-need research all previous"));
				return;
			}
			SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Start research"));
			TurnManager.currPlayer.StartResearch(p_researchUnitSlot.unit, p_sendRPC: true);
			RefreshAllSlots();
		}
	}

	public virtual Unit FindUnit(string p_unitName)
	{
		foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
		{
			if (listOfAllUnit.Country == TurnManager.currPlayer.Country && listOfAllUnit.FilterType == _selectedFilter && listOfAllUnit.Name == p_unitName)
			{
				return listOfAllUnit;
			}
		}
		return null;
	}

	public virtual bool HasCompletedPrerequisites(Unit p_targetUnit)
	{
		foreach (Unit listOfAllUnit in MapGO.instance.listOfAllUnits)
		{
			if (!listOfAllUnit.IsCommander && !listOfAllUnit.IsPrototype && listOfAllUnit.Country == TurnManager.currPlayer.Country && listOfAllUnit.FilterType == _selectedFilter && listOfAllUnit.ProductionYear < p_targetUnit.ProductionYear && !TurnManager.currPlayer.ListOfUnitsResearched.Contains(listOfAllUnit.Name))
			{
				return false;
			}
		}
		return true;
	}

	private void OnDisable()
	{
		DeleteEverything();
		SteamManager.SetRichPresenceStatus(SteamStatuses.DEFAULT);
	}

	public virtual void DeleteEverything()
	{
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != parent)
			{
				Object.Destroy(transform.gameObject);
			}
		}
	}
}
