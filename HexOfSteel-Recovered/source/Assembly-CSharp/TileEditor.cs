using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileEditor : MonoBehaviour
{
	[Header("Setup")]
	public Transform parentForListOfTiles;

	public Image tileRepresentation_Image;

	public TextMeshProUGUI totalNumberOfTiles;

	public GameObject selectedTileBackground;

	public Button add_Button;

	public Button addOfficial_Button;

	public Button delete_Button;

	public Button copy_button;

	public Button paste_button;

	public GameObject tileSlotPrefab;

	public TMP_InputField research_InputField;

	[Header("Filters")]
	public TMP_Dropdown mods_Dropdown;

	public TMP_Dropdown biomeFilter_Dropdown;

	[Header("Tile properties")]
	public TMP_Dropdown tileType_Dropdown;

	public TMP_Dropdown tileSeason_Dropdown;

	public TMP_Dropdown tileMapEditorCategory_Dropdown;

	public TMP_InputField tileName_InputField;

	public TMP_InputField moneyProductionPerTurn_InputField;

	public TMP_InputField occupiedMoneyProductionPerTurn_InputField;

	public TMP_InputField captureMoneyReward_InputField;

	public TMP_InputField damageModifier_InputField;

	public TMP_InputField movementPointCost_InputField;

	public TMP_InputField manpower_InputField;

	public TMP_InputField occupiedManpower_InputField;

	public Toggle bridge_toggle;

	public Toggle railroad_toggle;

	public Toggle capturable_toggle;

	public Toggle reinforcement_toggle;

	public Scrollbar verticalScrollbar;

	private Tile _copiedTile;

	private Tile _selectedTile;

	private string _prevSelectedUnitCheckSum;

	private List<string> _availableSeasons;

	private void OnEnable()
	{
		_availableSeasons = new List<string> { "Summer", "Winter", "Desert" };
		mods_Dropdown.options.Clear();
		mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-none")));
		mods_Dropdown.RefreshShownValue();
		tileSeason_Dropdown.options.Clear();
		foreach (string availableSeason in _availableSeasons)
		{
			tileSeason_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(availableSeason)));
		}
		tileSeason_Dropdown.RefreshShownValue();
		biomeFilter_Dropdown.options.Clear();
		foreach (string availableSeason2 in _availableSeasons)
		{
			biomeFilter_Dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate(availableSeason2)));
		}
		biomeFilter_Dropdown.RefreshShownValue();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			mods_Dropdown.options.Add(new TMP_Dropdown.OptionData(activeMod.Name));
		}
		RefreshTotalTiles();
		RefreshTilesList();
		SteamManager.SetRichPresenceStatus(SteamStatuses.MODDING);
	}

	private void OnDisable()
	{
		ModsManager.selectedMod = null;
		OfficialTiles.Save();
		ModsManager.SaveTilesAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-tiles saved"));
		SteamManager.SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			_selectedTile = null;
		}
		paste_button.interactable = _selectedTile != null;
		copy_button.interactable = _selectedTile != null;
		add_Button.interactable = _selectedTile != null;
		delete_Button.interactable = _selectedTile != null;
		selectedTileBackground.SetActive(_selectedTile != null);
		verticalScrollbar.gameObject.SetActive(selectedTileBackground.activeSelf);
		CheckSelectedTileChanges();
	}

	private void CheckSelectedTileChanges()
	{
		if (_selectedTile == null)
		{
			_prevSelectedUnitCheckSum = null;
			tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
			return;
		}
		if (ModsManager.selectedMod == null)
		{
			_prevSelectedUnitCheckSum = null;
			return;
		}
		string text = JsonConvert.SerializeObject(_selectedTile);
		if (_prevSelectedUnitCheckSum != null && _prevSelectedUnitCheckSum != text)
		{
			if (ModsManager.selectedMod.ModifiedBaseTiles.tiles.ContainsKey(_selectedTile.Name + _selectedTile.Season))
			{
				ModsManager.selectedMod.ModifiedBaseTiles.tiles[_selectedTile.Name + _selectedTile.Season] = _selectedTile;
			}
			else
			{
				ModsManager.selectedMod.ModifiedBaseTiles.tiles.Add(_selectedTile.Name + _selectedTile.Season, _selectedTile);
			}
		}
		_prevSelectedUnitCheckSum = text;
	}

	public void SaveAll()
	{
		ModsManager.SaveTilesAllMods();
		UIManager.ShowMessage(LocalizationManager.Translate("info-tiles saved"));
	}

	public void SelectModToEdit()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.selectedMod = ModsManager.FindModByName(mods_Dropdown.options[mods_Dropdown.value].text);
	}

	private void RefreshTotalTiles()
	{
		int num = OfficialTiles.GetInstance().Tiles.Count;
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			num += activeMod.CustomTiles.ListOfCustomTiles.Count();
		}
		totalNumberOfTiles.text = LocalizationManager.Translate("txt-total tiles") + num;
	}

	public void ResearchUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		RefreshTilesList();
	}

	public void GenerateTileList()
	{
		foreach (Tile tile in from x in Utils.GetAllTiles()
			orderby x.Name
			select x)
		{
			if (research_InputField.text != "")
			{
				if (tile.Name.ToLower().Contains(research_InputField.text.ToLower()) && tile.Season == _availableSeasons[biomeFilter_Dropdown.value])
				{
					GameObject gameObject = Object.Instantiate(tileSlotPrefab, parentForListOfTiles);
					gameObject.name = tile.Name;
					gameObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.Name;
					gameObject.GetComponent<Button>().onClick.AddListener(() =>
					{
						SelectTile(tile);
					});
				}
			}
			else if (tile.Season == _availableSeasons[biomeFilter_Dropdown.value])
			{
				GameObject gameObject2 = Object.Instantiate(tileSlotPrefab, parentForListOfTiles);
				gameObject2.name = tile.Name;
				gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = tile.Name;
				gameObject2.GetComponent<Button>().onClick.AddListener(() =>
				{
					SelectTile(tile);
				});
			}
		}
	}

	public void SelectTile(Tile p_tile)
	{
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		tileRepresentation_Image.sprite = null;
		foreach (Tile allTile in Utils.GetAllTiles())
		{
			if (allTile.Name == p_tile.Name && allTile.Season == _availableSeasons[biomeFilter_Dropdown.value])
			{
				_selectedTile = p_tile;
				ShowTileStats();
				break;
			}
		}
	}

	public void ShowTileStats()
	{
		if (_selectedTile == null)
		{
			return;
		}
		bool flag = false;
		tileName_InputField.SetTextWithoutNotify(_selectedTile.Name);
		moneyProductionPerTurn_InputField.SetTextWithoutNotify(_selectedTile.MoneyProductionPerTurn.ToString());
		occupiedMoneyProductionPerTurn_InputField.SetTextWithoutNotify(_selectedTile.OccupiedMoneyProductionPerTurn.ToString());
		captureMoneyReward_InputField.SetTextWithoutNotify(_selectedTile.MoneyReward.ToString());
		damageModifier_InputField.SetTextWithoutNotify(_selectedTile.DamageModificator.ToString());
		movementPointCost_InputField.SetTextWithoutNotify(_selectedTile.MPCost.ToString());
		manpower_InputField.SetTextWithoutNotify(_selectedTile.Manpower.ToString());
		occupiedManpower_InputField.SetTextWithoutNotify(_selectedTile.OccupiedManpower.ToString());
		for (int i = 0; i < tileType_Dropdown.options.Count; i++)
		{
			if (_selectedTile.Type == tileType_Dropdown.options[i].text)
			{
				flag = true;
				tileType_Dropdown.SetValueWithoutNotify(i);
			}
		}
		if (!flag)
		{
			tileType_Dropdown.SetValueWithoutNotify(0);
			tileType_Dropdown.RefreshShownValue();
		}
		flag = false;
		for (int j = 0; j < _availableSeasons.Count; j++)
		{
			if (_selectedTile.Season == _availableSeasons[j])
			{
				flag = true;
				tileSeason_Dropdown.SetValueWithoutNotify(j);
			}
		}
		if (!flag)
		{
			tileSeason_Dropdown.SetValueWithoutNotify(0);
			tileSeason_Dropdown.RefreshShownValue();
		}
		flag = false;
		for (int k = 0; k < tileMapEditorCategory_Dropdown.options.Count; k++)
		{
			if (_selectedTile.FilterType == tileMapEditorCategory_Dropdown.options[k].text)
			{
				flag = true;
				tileMapEditorCategory_Dropdown.SetValueWithoutNotify(k);
			}
		}
		if (!flag)
		{
			tileMapEditorCategory_Dropdown.SetValueWithoutNotify(0);
			tileMapEditorCategory_Dropdown.RefreshShownValue();
		}
		bridge_toggle.SetIsOnWithoutNotify(_selectedTile.Bridge);
		railroad_toggle.SetIsOnWithoutNotify(_selectedTile.Railroad);
		capturable_toggle.SetIsOnWithoutNotify(_selectedTile.Capturable);
		reinforcement_toggle.SetIsOnWithoutNotify(_selectedTile.IsReinforcementTile);
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 1f);
		tileRepresentation_Image.sprite = Utils.GetTileSprite(_selectedTile);
	}

	public void RefreshTilesList()
	{
		foreach (Transform parentForListOfTile in parentForListOfTiles)
		{
			if (parentForListOfTile != parentForListOfTiles)
			{
				Object.Destroy(parentForListOfTile.gameObject);
			}
		}
		GenerateTileList();
	}

	public void SetTileName()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Name = tileName_InputField.text;
	}

	public void SetTileProductionPerTurn()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(moneyProductionPerTurn_InputField.text))
		{
			_selectedTile.MoneyProductionPerTurn = 0;
		}
		else
		{
			_selectedTile.MoneyProductionPerTurn = short.Parse(moneyProductionPerTurn_InputField.text);
		}
	}

	public void SetTileOccupiedProductionPerTurn()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(occupiedMoneyProductionPerTurn_InputField.text))
		{
			_selectedTile.OccupiedMoneyProductionPerTurn = 0;
		}
		else
		{
			_selectedTile.OccupiedMoneyProductionPerTurn = short.Parse(occupiedMoneyProductionPerTurn_InputField.text);
		}
	}

	public void SetTileDamageModifier()
	{
		SoundManager.instance.PlayUIClickSound();
		short result;
		if (string.IsNullOrWhiteSpace(damageModifier_InputField.text))
		{
			_selectedTile.DamageModificator = 0;
		}
		else if (short.TryParse(damageModifier_InputField.text, out result))
		{
			_selectedTile.DamageModificator = result;
		}
	}

	public void SetTileMovemenCost()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(movementPointCost_InputField.text))
		{
			_selectedTile.MPCost = 0;
		}
		else
		{
			_selectedTile.MPCost = byte.Parse(movementPointCost_InputField.text);
		}
	}

	public void SetManpower()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(manpower_InputField.text))
		{
			_selectedTile.Manpower = 0;
		}
		else
		{
			_selectedTile.Manpower = int.Parse(manpower_InputField.text);
		}
	}

	public void SetOccupiedManpower()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(occupiedManpower_InputField.text))
		{
			_selectedTile.OccupiedManpower = 0;
		}
		else
		{
			_selectedTile.OccupiedManpower = int.Parse(occupiedManpower_InputField.text);
		}
	}

	public void SetTileCaptureMoneyReward()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(captureMoneyReward_InputField.text))
		{
			_selectedTile.MoneyReward = 0;
		}
		else
		{
			_selectedTile.MoneyReward = short.Parse(captureMoneyReward_InputField.text);
		}
	}

	public void SetTileSeason()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Season = _availableSeasons[tileSeason_Dropdown.value];
		tileRepresentation_Image.sprite = Utils.GetTileSprite(_selectedTile);
	}

	public void SetTileType()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Type = tileType_Dropdown.options[tileType_Dropdown.value].text;
	}

	public void SetMapEditorCategory()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.FilterType = tileMapEditorCategory_Dropdown.options[tileMapEditorCategory_Dropdown.value].text;
	}

	public void SetBridge()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Bridge = !_selectedTile.Bridge;
	}

	public void SetRailroad()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Railroad = !_selectedTile.Railroad;
	}

	public void SetCapturable()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.Capturable = !_selectedTile.Capturable;
	}

	public void SetReinforcement()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedTile.IsReinforcementTile = !_selectedTile.IsReinforcementTile;
	}

	public void CreateNewTile()
	{
		SoundManager.instance.PlayUIClickSound();
		Tile tile = new Tile();
		tile.Season = "Summer";
		_selectedTile = tile;
		ShowTileStats();
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
	}

	public void AddTile()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedTile.Type == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile issue"));
			return;
		}
		if (_selectedTile.FilterType == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile issue"));
			return;
		}
		if (ModsManager.selectedMod == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no mod selected"));
			return;
		}
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			foreach (Tile listOfCustomTile in activeMod.CustomTiles.ListOfCustomTiles)
			{
				if (listOfCustomTile.Name == _selectedTile.Name && listOfCustomTile.Season == _selectedTile.Season)
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-tile already exist"));
					return;
				}
			}
		}
		_selectedTile.IsOfficial = false;
		ModsManager.selectedMod.CustomTiles.ListOfCustomTiles.Add(_selectedTile);
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		RefreshTilesList();
		_selectedTile = null;
		RefreshTotalTiles();
	}

	public void AddOfficialTile()
	{
		_selectedTile.IsOfficial = true;
		OfficialTiles.GetInstance().Tiles.Add(_selectedTile);
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		RefreshTilesList();
		_selectedTile = null;
		RefreshTotalTiles();
	}

	public void RemoveTile()
	{
		SoundManager.instance.PlayUIClickSound();
		tileRepresentation_Image.color = new Color(1f, 1f, 1f, 0f);
		tileRepresentation_Image.sprite = null;
		if (_selectedTile.IsOfficial)
		{
			for (int i = 0; i < OfficialTiles.GetInstance().Tiles.Count; i++)
			{
				if (_selectedTile.Name == OfficialTiles.GetInstance().Tiles[i].Name && _selectedTile.Season == OfficialTiles.GetInstance().Tiles[i].Season)
				{
					OfficialTiles.GetInstance().Tiles.RemoveAt(i);
				}
			}
		}
		else
		{
			foreach (Mod activeMod in ModsManager.activeMods)
			{
				for (int j = 0; j < activeMod.CustomTiles.ListOfCustomTiles.Count; j++)
				{
					if (_selectedTile.Name == activeMod.CustomTiles.ListOfCustomTiles[j].Name && _selectedTile.Season == activeMod.CustomTiles.ListOfCustomTiles[j].Season)
					{
						activeMod.CustomTiles.ListOfCustomTiles.RemoveAt(j);
						break;
					}
				}
			}
		}
		_selectedTile = null;
		RefreshTilesList();
		RefreshTotalTiles();
		UIManager.HideToolTip();
	}

	public void CopyTile()
	{
		SoundManager.instance.PlayUIClickSound();
		_copiedTile = new Tile();
		PropertyInfo[] properties = _selectedTile.GetType().GetProperties();
		PropertyInfo[] properties2 = _copiedTile.GetType().GetProperties();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			PropertyInfo[] array2 = properties2;
			foreach (PropertyInfo propertyInfo2 in array2)
			{
				if (propertyInfo.Name == propertyInfo2.Name && propertyInfo.PropertyType == propertyInfo2.PropertyType)
				{
					propertyInfo2.SetValue(_copiedTile, propertyInfo.GetValue(_selectedTile));
					break;
				}
			}
		}
		_copiedTile.IsOfficial = false;
	}

	public void PastTile()
	{
		SoundManager.instance.PlayUIClickSound();
		PropertyInfo[] properties = _selectedTile.GetType().GetProperties();
		PropertyInfo[] properties2 = _copiedTile.GetType().GetProperties();
		_copiedTile.IsOfficial = false;
		PropertyInfo[] array = properties2;
		foreach (PropertyInfo propertyInfo in array)
		{
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo2 in array2)
			{
				if (propertyInfo.Name == propertyInfo2.Name && propertyInfo.PropertyType == propertyInfo2.PropertyType)
				{
					propertyInfo2.SetValue(_selectedTile, propertyInfo.GetValue(_copiedTile));
					break;
				}
			}
		}
		ShowTileStats();
	}
}
