using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryEditor : MonoBehaviour
{
	[Header("Setup")]
	public Transform parentListOfCountries;

	public GameObject selectedCountry_Panel;

	[SerializeField]
	private Button _add_Button;

	[SerializeField]
	private Button _delete_Button;

	[SerializeField]
	private GameObject _countrySlot_prefab;

	[SerializeField]
	private TMP_InputField _countryName_Inputfield;

	[SerializeField]
	private TMP_Dropdown _mods_Dropdown;

	private string _selectedCountry;

	private void OnEnable()
	{
		_mods_Dropdown.options.Clear();
		_mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		_mods_Dropdown.RefreshShownValue();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			_mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(activeMod.Name));
		}
		RefreshCountryList();
		SteamManager.SetRichPresenceStatus(SteamStatuses.MODDING);
	}

	private void OnDisable()
	{
		ModsManager.selectedMod = null;
		ModsManager.SaveCountriesAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-countries saved"));
		SteamManager.SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
	}

	private void Update()
	{
		_add_Button.interactable = selectedCountry_Panel.activeSelf;
		_delete_Button.interactable = _selectedCountry != null;
		if (Input.GetMouseButton(1))
		{
			_selectedCountry = null;
		}
	}

	public void SaveAll()
	{
		ModsManager.SaveCountriesAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-countries saved"));
	}

	public void SelectModToEdit()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.selectedMod = ModsManager.FindModByName(_mods_Dropdown.options[_mods_Dropdown.value].text);
	}

	public void GenerateCountriesList()
	{
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			foreach (string country in activeMod.CustomCountries.ListOfCustomCountries)
			{
				GameObject gameObject = Object.Instantiate(_countrySlot_prefab, parentListOfCountries);
				gameObject.name = country;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = country;
				gameObject.GetComponent<Button>().onClick.AddListener(() =>
				{
					SelectCountry(country);
				});
			}
		}
	}

	public void SelectCountry(string country)
	{
		_selectedCountry = country;
		ShowCountryName(_selectedCountry);
	}

	public void ShowCountryName(string p_countryName)
	{
		_countryName_Inputfield.text = p_countryName;
	}

	public void RefreshCountryList()
	{
		foreach (Transform parentListOfCountry in parentListOfCountries)
		{
			if (parentListOfCountry != parentListOfCountries)
			{
				Object.Destroy(parentListOfCountry.gameObject);
			}
		}
		GenerateCountriesList();
	}

	public void SetCountryName()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedCountry = _countryName_Inputfield.text;
	}

	public void CreateNewCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		string p_countryName = LocalizationManager.Translate("text-country default name");
		ShowCountryName(p_countryName);
		selectedCountry_Panel.SetActive(value: true);
	}

	public void AddCountryToList()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(_countryName_Inputfield.text))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-empty country name"));
			return;
		}
		if (ModsManager.selectedMod == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no mod selected"));
			return;
		}
		bool flag = false;
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			foreach (string listOfCustomCountry in activeMod.CustomCountries.ListOfCustomCountries)
			{
				if (listOfCustomCountry == _countryName_Inputfield.text)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-country already exist"));
			return;
		}
		ModsManager.selectedMod.CustomCountries.ListOfCustomCountries.Add(_countryName_Inputfield.text);
		_selectedCountry = null;
		RefreshCountryList();
		MainMenu.instance.tooltip.SetActive(value: false);
		selectedCountry_Panel.SetActive(value: false);
	}

	public void RemoveCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			for (int i = 0; i < activeMod.CustomCountries.ListOfCustomCountries.Count; i++)
			{
				if (_selectedCountry == activeMod.CustomCountries.ListOfCustomCountries[i])
				{
					activeMod.CustomCountries.ListOfCustomCountries.RemoveAt(i);
					_selectedCountry = null;
					RefreshCountryList();
					return;
				}
			}
		}
	}
}
