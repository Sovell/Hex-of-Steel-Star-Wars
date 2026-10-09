using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroesEditor : MonoBehaviour
{
	public TMP_Dropdown mods_Dropdown;

	public TMP_Dropdown filterType_Dropdown;

	public TMP_Dropdown filterCountry_Dropdown;

	public TMP_InputField search_inputField;

	public Image hero_Image;

	public GameObject heroImagebackground;

	public Button add_Button;

	public Button delete_Button;

	public Transform parentForListOfHeroes;

	public GameObject selectedHeroBackground;

	public GameObject heroSlotPrefab;

	[Header("Hero editor panel")]
	public TMP_InputField heroName_inputfield;

	public TMP_InputField heroYearStart_inputfield;

	public TMP_InputField heroYearStop_inputfield;

	public TMP_Dropdown heroType_Dropdown;

	public TMP_Dropdown heroCountry_Dropdown;

	private Hero _selectedHero;

	private void OnEnable()
	{
		_selectedHero = null;
		search_inputField.SetTextWithoutNotify("");
		mods_Dropdown.options.Clear();
		mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(activeMod.Name));
		}
		filterCountry_Dropdown.options.Clear();
		heroCountry_Dropdown.options.Clear();
		foreach (string country in Database.Countries)
		{
			filterCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
			heroCountry_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
		}
		heroCountry_Dropdown.RefreshShownValue();
		filterCountry_Dropdown.RefreshShownValue();
		mods_Dropdown.RefreshShownValue();
		RefreshHeroesList();
	}

	private void OnDisable()
	{
		_selectedHero = null;
		search_inputField.SetTextWithoutNotify("");
		hero_Image.color = new Color(1f, 1f, 1f, 0f);
		selectedHeroBackground.SetActive(value: false);
		SaveAll();
		foreach (Transform parentForListOfHero in parentForListOfHeroes)
		{
			if (parentForListOfHero != parentForListOfHeroes)
			{
				Object.Destroy(parentForListOfHero.gameObject);
			}
		}
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			_selectedHero = null;
		}
		add_Button.interactable = _selectedHero != null;
		delete_Button.interactable = _selectedHero != null;
		heroImagebackground.SetActive(_selectedHero != null);
		selectedHeroBackground.SetActive(_selectedHero != null);
		hero_Image.gameObject.SetActive(_selectedHero != null);
	}

	public void GenerateHeroesSlots()
	{
		List<Hero> list = (from hero2 in Utils.GetAllHeroes()
			orderby hero2.Name
			select hero2).ToList();
		parentForListOfHeroes.gameObject.SetActive(value: false);
		string text = Database.Countries[filterCountry_Dropdown.value];
		string text2 = filterType_Dropdown.options[filterType_Dropdown.value].text;
		foreach (Hero hero in list)
		{
			if ((string.IsNullOrWhiteSpace(search_inputField.text) || hero.Name.ToLower().Contains(search_inputField.text.ToLower())) && (!(text != "None") || !(text != hero.Country)) && (!(text2 != "None") || !(text2 != hero.Type)))
			{
				GameObject gameObject = Object.Instantiate(heroSlotPrefab, parentForListOfHeroes);
				gameObject.name = hero.Name;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = hero.Name;
				gameObject.transform.localScale = Vector3.one;
				gameObject.GetComponent<Button>().onClick.AddListener(() =>
				{
					SelectHero(hero);
				});
			}
		}
		parentForListOfHeroes.gameObject.SetActive(value: true);
	}

	public void SelectModToEdit()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.selectedMod = ModsManager.FindModByName(mods_Dropdown.options[mods_Dropdown.value].text);
	}

	public void SelectHero(Hero p_hero)
	{
		_selectedHero = p_hero;
		hero_Image.color = new Color(1f, 1f, 1f, 0f);
		hero_Image.sprite = null;
		hero_Image.color = new Color(1f, 1f, 1f, 1f);
		hero_Image.sprite = Utils.GetModdedHeroSprite(p_hero);
		ShowHeroStats();
	}

	public void ShowHeroStats()
	{
		if (_selectedHero != null)
		{
			heroName_inputfield.text = _selectedHero.Name;
			heroYearStart_inputfield.text = _selectedHero.YearStartAvailability.ToString();
			heroYearStop_inputfield.text = _selectedHero.YearStopAvailability.ToString();
			heroCountry_Dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedHero.Country, Database.Countries));
			SetDropDownValue(heroType_Dropdown, _selectedHero.Type);
		}
	}

	public void RefreshHeroesList()
	{
		foreach (Transform parentForListOfHero in parentForListOfHeroes)
		{
			if (parentForListOfHero != parentForListOfHeroes)
			{
				Object.Destroy(parentForListOfHero.gameObject);
			}
		}
		GenerateHeroesSlots();
	}

	public void SetHeroName()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedHero.Name = heroName_inputfield.text;
		RefreshHeroesList();
	}

	public void SetHeroCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedHero.Country = Database.Countries[heroCountry_Dropdown.value];
	}

	public void SetHeroType()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedHero.Type = heroType_Dropdown.options[heroType_Dropdown.value].text;
	}

	public void SetHeroStartYear()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedHero.YearStartAvailability = short.Parse(heroYearStart_inputfield.text);
	}

	public void SetHeroStopYear()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedHero.YearStopAvailability = short.Parse(heroYearStop_inputfield.text);
	}

	public void AddNewHero()
	{
		SoundManager.instance.PlayUIClickSound();
		if (AreConditionsOKtoAddNewHero())
		{
			ModsManager.selectedMod.ModdedHeroes.heroes.Add(_selectedHero);
			hero_Image.color = new Color(1f, 1f, 1f, 0f);
			_selectedHero = null;
			RefreshHeroesList();
			MainMenu.instance.tooltip.SetActive(value: false);
		}
	}

	public void CreateHero()
	{
		SoundManager.instance.PlayUIClickSound();
		Hero selectedHero = new Hero();
		_selectedHero = selectedHero;
		hero_Image.color = new Color(1f, 1f, 1f, 0f);
		ShowHeroStats();
	}

	public void RemoveHero()
	{
		SoundManager.instance.PlayUIClickSound();
		hero_Image.color = new Color(1f, 1f, 1f, 0f);
		hero_Image.sprite = null;
		UIManager.HideToolTip();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			for (int i = 0; i < activeMod.ModdedHeroes.heroes.Count; i++)
			{
				if (_selectedHero.Name == activeMod.ModdedHeroes.heroes[i].Name)
				{
					activeMod.ModdedHeroes.heroes.RemoveAt(i);
					_selectedHero = null;
					RefreshHeroesList();
					return;
				}
			}
		}
	}

	private void SetDropDownValue(TMP_Dropdown p_dropdown, string p_value)
	{
		for (int i = 0; i < p_dropdown.options.Count; i++)
		{
			if (p_value == p_dropdown.options[i].text)
			{
				p_dropdown.value = i;
				p_dropdown.RefreshShownValue();
				return;
			}
		}
		p_dropdown.value = 0;
		p_dropdown.RefreshShownValue();
	}

	private bool AreConditionsOKtoAddNewHero()
	{
		if (ModsManager.selectedMod == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no mod selected"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(_selectedHero.Type))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(_selectedHero.Country))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		if (_selectedHero.YearStartAvailability == 0 || _selectedHero.YearStartAvailability < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		if (_selectedHero.YearStopAvailability == 0 || _selectedHero.YearStopAvailability < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		if (_selectedHero.YearStopAvailability < _selectedHero.YearStartAvailability)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		if (_selectedHero.YearStartAvailability == _selectedHero.YearStopAvailability)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-hero issue"));
			return false;
		}
		foreach (Hero hero in ModsManager.selectedMod.ModdedHeroes.heroes)
		{
			if (hero.Name == _selectedHero.Name)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-hero already exist"));
				return false;
			}
		}
		return true;
	}

	public void SaveAll()
	{
		ModsManager.SaveHeroesAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-heroes saved"));
	}
}
