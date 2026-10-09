using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventActionSlotGO : MonoBehaviour
{
	[Header("Toggles")]
	[SerializeField]
	private Toggle checkOriginalOwner_toggle;

	[Header("Dropdowns")]
	[SerializeField]
	private TMP_Dropdown actionType_dropdown;

	[SerializeField]
	private TMP_Dropdown playerTarget1_dropdown;

	[SerializeField]
	private TMP_Dropdown playerTarget2_dropdown;

	[SerializeField]
	private TMP_Dropdown countriesUnit_dropdown;

	[SerializeField]
	private TMP_Dropdown allCountries_dropdown;

	[SerializeField]
	private TMP_Dropdown units_dropdown;

	[SerializeField]
	private TMP_Dropdown season_dropdown;

	[SerializeField]
	private TMP_Dropdown tiles_dropdown;

	[Header("Button")]
	[SerializeField]
	private Button unit_button;

	[Header("InputFields")]
	[SerializeField]
	private TMP_InputField textMessage_inputfield;

	[SerializeField]
	private TMP_InputField targetFaction_inputfield;

	[SerializeField]
	private TMP_InputField targetIdeology_inputfield;

	[SerializeField]
	private TMP_InputField targetCountryChange_inputfield;

	[SerializeField]
	private TMP_InputField posX_inputfield;

	[SerializeField]
	private TMP_InputField posY_inputfield;

	[SerializeField]
	private TMP_InputField moneyAmount_inputfield;

	[SerializeField]
	private TMP_InputField HQpoints_inputfield;

	[SerializeField]
	private TMP_InputField unitLevel_inputfield;

	[SerializeField]
	private TMP_InputField unitName_inputfield;

	[SerializeField]
	private TMP_InputField playerIndex_inputfield;

	[SerializeField]
	private TMP_InputField cityName_inputfield;

	private Event _event;

	private EventAction _eventAction;

	private List<string> _availableScenariosCountries;

	public void Setup(Event p_event, EventAction p_eventAction)
	{
		_availableScenariosCountries = new List<string> { "None" };
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			_availableScenariosCountries.Add(listOfPlayer.Country);
		}
		_event = p_event;
		_eventAction = p_eventAction;
		RefreshDropdownsOptions();
		RefreshPossibilities();
	}

	public EventAction GetEventAction()
	{
		return _eventAction;
	}

	private void RefreshPossibilities()
	{
		playerTarget1_dropdown.gameObject.SetActive(value: true);
		moneyAmount_inputfield.gameObject.SetActive(value: false);
		HQpoints_inputfield.gameObject.SetActive(value: false);
		targetFaction_inputfield.gameObject.SetActive(value: false);
		targetIdeology_inputfield.gameObject.SetActive(value: false);
		playerTarget2_dropdown.gameObject.SetActive(value: false);
		posX_inputfield.gameObject.SetActive(value: false);
		posY_inputfield.gameObject.SetActive(value: false);
		textMessage_inputfield.gameObject.SetActive(value: false);
		unitLevel_inputfield.gameObject.SetActive(value: false);
		targetCountryChange_inputfield.gameObject.SetActive(value: false);
		checkOriginalOwner_toggle.gameObject.SetActive(value: false);
		season_dropdown.gameObject.SetActive(value: false);
		tiles_dropdown.gameObject.SetActive(value: false);
		unit_button.gameObject.SetActive(value: false);
		allCountries_dropdown.gameObject.SetActive(value: false);
		playerIndex_inputfield.gameObject.SetActive(value: false);
		cityName_inputfield.gameObject.SetActive(value: false);
		unitName_inputfield.gameObject.SetActive(value: false);
		switch (_eventAction.Type)
		{
		default:
			_ = 101;
			break;
		case ActionTypes.SHOW_MESSAGE:
			textMessage_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.SET_TILE_NAME:
			playerTarget1_dropdown.gameObject.SetActive(value: false);
			cityName_inputfield.gameObject.SetActive(value: true);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.SET_TILE:
			playerTarget1_dropdown.gameObject.SetActive(value: false);
			season_dropdown.gameObject.SetActive(value: true);
			tiles_dropdown.gameObject.SetActive(value: true);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.ADD_VP:
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.REMOVE_VP:
			playerTarget1_dropdown.gameObject.SetActive(value: false);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.ADD_MONEY:
		case ActionTypes.SET_FIXED_INCOME:
		case ActionTypes.SET_FIXED_INCOME_PERCENT:
		case ActionTypes.SET_FIXED_INCOME_FROM_BASE_INCOME_PERCENT:
		case ActionTypes.ADD_FIXED_INCOME:
			moneyAmount_inputfield.gameObject.SetActive(value: true);
			moneyAmount_inputfield.characterLimit = ((_eventAction.Type == ActionTypes.SET_FIXED_INCOME_PERCENT || _eventAction.Type == ActionTypes.SET_FIXED_INCOME_FROM_BASE_INCOME_PERCENT) ? 3 : 5);
			break;
		case ActionTypes.ADD_HQ_POINTS:
			HQpoints_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.DESTROY_TILE:
			playerTarget1_dropdown.gameObject.SetActive(value: false);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.FACTION_CHANGE:
		case ActionTypes.MAKE_PEACE_FACTION:
			targetFaction_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.MAKE_PEACE:
		case ActionTypes.MAKE_ALLIANCE:
		case ActionTypes.DECLARE_WAR:
			playerTarget2_dropdown.gameObject.SetActive(value: true);
			break;
		case ActionTypes.COUNTRY_CHANGE:
			targetCountryChange_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.SPAWN_UNIT:
			unitLevel_inputfield.gameObject.SetActive(value: true);
			unitName_inputfield.gameObject.SetActive(value: true);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			unit_button.gameObject.SetActive(value: true);
			break;
		case ActionTypes.ADD_UNIT_RESERVE:
			unitLevel_inputfield.gameObject.SetActive(value: true);
			unitName_inputfield.gameObject.SetActive(value: true);
			unit_button.gameObject.SetActive(value: true);
			break;
		case ActionTypes.CHANGE_VP_OWNERSHIP:
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			checkOriginalOwner_toggle.gameObject.SetActive(value: true);
			break;
		case ActionTypes.ADD_PLAYER:
			playerTarget1_dropdown.gameObject.SetActive(value: false);
			allCountries_dropdown.gameObject.SetActive(value: true);
			playerIndex_inputfield.gameObject.SetActive(value: true);
			targetFaction_inputfield.gameObject.SetActive(value: true);
			targetIdeology_inputfield.gameObject.SetActive(value: true);
			break;
		case ActionTypes.NONE:
		case ActionTypes.CAPITULATION:
		case ActionTypes.RESET_INCOME:
			break;
		}
	}

	private void RefreshDropdownsOptions()
	{
		actionType_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (ActionTypes value in Enum.GetValues(typeof(ActionTypes)))
		{
			if (value != ActionTypes.ADD_PLAYER && value != ActionTypes.REMOVE_PLAYER)
			{
				actionType_dropdown.options.Add(new TMP_Dropdown.OptionData(EventAction.TranslateEventAction(value)));
			}
		}
		actionType_dropdown.RefreshShownValue();
		playerTarget1_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string availableScenariosCountry in _availableScenariosCountries)
		{
			playerTarget1_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(availableScenariosCountry)));
		}
		playerTarget1_dropdown.RefreshShownValue();
		playerTarget2_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string availableScenariosCountry2 in _availableScenariosCountries)
		{
			playerTarget2_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(availableScenariosCountry2)));
		}
		playerTarget2_dropdown.RefreshShownValue();
		season_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string season in Database.seasons)
		{
			season_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(season)));
		}
		season_dropdown.RefreshShownValue();
		tiles_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (Tile item in from x in Utils.GetAllTiles()
			orderby x.Name
			select x)
		{
			if (item.Season == _eventAction.TileSeason)
			{
				tiles_dropdown.options.Add(new TMP_Dropdown.OptionData(item.Name));
			}
		}
		tiles_dropdown.RefreshShownValue();
		allCountries_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string country in Database.Countries)
		{
			allCountries_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(country)));
		}
		allCountries_dropdown.RefreshShownValue();
		RefreshAllValues();
	}

	private void RefreshAllValues()
	{
		actionType_dropdown.SetValueWithoutNotify(Utils.GetIndexByName(actionType_dropdown, EventAction.TranslateEventAction(_eventAction.Type)));
		if (!string.IsNullOrEmpty(_eventAction.TargetPlayerName1) && GameData.Instance.TryFindPlayerByName(_eventAction.TargetPlayerName1, out var o_player))
		{
			playerTarget1_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(o_player.Country, _availableScenariosCountries));
		}
		if (!string.IsNullOrEmpty(_eventAction.TargetPlayerName2) && GameData.Instance.TryFindPlayerByName(_eventAction.TargetPlayerName2, out var o_player2))
		{
			playerTarget2_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(o_player2.Country, _availableScenariosCountries));
		}
		if (!string.IsNullOrEmpty(_eventAction.TileSeason))
		{
			season_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_eventAction.TileSeason, Database.seasons));
		}
		if (!string.IsNullOrEmpty(_eventAction.TileName))
		{
			tiles_dropdown.SetValueWithoutNotify(Utils.GetIndexByName(tiles_dropdown, _eventAction.TileName));
		}
		if (!string.IsNullOrEmpty(_eventAction.TargetCountryName))
		{
			allCountries_dropdown.SetValueWithoutNotify(Utils.GetIndexByName(allCountries_dropdown, _eventAction.TargetCountryName));
		}
		textMessage_inputfield.SetTextWithoutNotify(_eventAction.MessageText);
		cityName_inputfield.SetTextWithoutNotify(_eventAction.CityName);
		targetFaction_inputfield.SetTextWithoutNotify(_eventAction.TargetFactionName);
		targetIdeology_inputfield.SetTextWithoutNotify(_eventAction.TargetIdeology);
		targetCountryChange_inputfield.SetTextWithoutNotify(_eventAction.TargetCountryName);
		moneyAmount_inputfield.SetTextWithoutNotify(_eventAction.MoneyAmount.ToString());
		HQpoints_inputfield.SetTextWithoutNotify(_eventAction.HQPointsAmount.ToString());
		posX_inputfield.SetTextWithoutNotify(_eventAction.PosX.ToString());
		posY_inputfield.SetTextWithoutNotify(_eventAction.PosY.ToString());
		unitLevel_inputfield.SetTextWithoutNotify(_eventAction.UnitLevel.ToString());
		playerIndex_inputfield.SetTextWithoutNotify(_eventAction.PlayerIndex.ToString());
		if (_eventAction.Unit == null)
		{
			unitName_inputfield.SetTextWithoutNotify(_eventAction.UnitName);
		}
		else
		{
			unitName_inputfield.SetTextWithoutNotify(_eventAction.Unit.Name);
		}
		checkOriginalOwner_toggle.SetIsOnWithoutNotify(_eventAction.OriginalOwnerOnly);
	}

	public void SetActionType()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.Type = (ActionTypes)actionType_dropdown.value;
		if (_eventAction.Type == ActionTypes.SET_TILE)
		{
			_eventAction.TileName = "Airport";
			_eventAction.TileSeason = "Summer";
		}
		else
		{
			_eventAction.TileName = null;
			_eventAction.TileSeason = null;
		}
		if (_eventAction.Type == ActionTypes.SPAWN_UNIT)
		{
			_eventAction.TargetPlayerName1 = GameData.Instance.listOfPlayers[Utils.GetIndexByLocalizedName(_event.Group, _availableScenariosCountries) - 1].Name;
			_eventAction.UnitCountry = null;
			_eventAction.UnitName = null;
		}
		else
		{
			_eventAction.UnitCountry = null;
			_eventAction.UnitName = null;
		}
		RefreshPossibilities();
		RefreshDropdownsOptions();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetPlayerTarget1()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = playerTarget1_dropdown.value - 1;
		if (num < 0)
		{
			_eventAction.TargetPlayerName1 = null;
		}
		else
		{
			_eventAction.TargetPlayerName1 = GameData.Instance.listOfPlayers[num].Name;
		}
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetPlayerTarget2()
	{
		SoundManager.instance.PlayUIClickSound();
		int num = playerTarget2_dropdown.value - 1;
		if (num < 0)
		{
			_eventAction.TargetPlayerName2 = null;
		}
		else
		{
			_eventAction.TargetPlayerName2 = GameData.Instance.listOfPlayers[num].Name;
		}
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetEventSeason()
	{
		SoundManager.instance.PlayUIClickSound();
		if (season_dropdown.value == 0)
		{
			_eventAction.TileSeason = Database.seasons[0];
		}
		else
		{
			_eventAction.TileSeason = season_dropdown.options[season_dropdown.value].text;
		}
		RefreshDropdownsOptions();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetEventTile()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.TileName = tiles_dropdown.options[tiles_dropdown.value].text;
		RefreshDropdownsOptions();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetEventUnitCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		if (countriesUnit_dropdown.value == 0)
		{
			_eventAction.UnitCountry = null;
		}
		else
		{
			_eventAction.UnitCountry = Database.Countries[countriesUnit_dropdown.value];
		}
		_eventAction.UnitName = null;
		units_dropdown.SetValueWithoutNotify(0);
		RefreshDropdownsOptions();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetEventUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.UnitName = null;
		foreach (Unit allUnit in Utils.GetAllUnits())
		{
			if (allUnit.Name == units_dropdown.options[units_dropdown.value].text && allUnit.Country == _eventAction.UnitCountry)
			{
				_eventAction.UnitName = allUnit.Name;
				break;
			}
		}
		RefreshDropdownsOptions();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetCheckOriginalOwner()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.OriginalOwnerOnly = !_eventAction.OriginalOwnerOnly;
	}

	public void SetTextMessage()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.MessageText = textMessage_inputfield.text;
	}

	public void SetCityName()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.CityName = cityName_inputfield.text;
	}

	public void SetTargetFaction()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.TargetFactionName = targetFaction_inputfield.text;
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetTargetIdeology()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.TargetIdeology = targetIdeology_inputfield.text;
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetTargetCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.TargetCountryName = targetCountryChange_inputfield.text;
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetMoneyAmount()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.TryParse(moneyAmount_inputfield.text, out var result))
		{
			_eventAction.MoneyAmount = result;
		}
	}

	public void SetHQpointsAmount()
	{
		SoundManager.instance.PlayUIClickSound();
		if (short.TryParse(HQpoints_inputfield.text, out var result))
		{
			_eventAction.HQPointsAmount = result;
		}
	}

	public void SetUnitLevel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(unitLevel_inputfield.text) < 1 || int.Parse(unitLevel_inputfield.text) > 5)
		{
			unitLevel_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(unitLevel_inputfield.text), 1, 5).ToString());
		}
		else
		{
			_eventAction.UnitLevel = byte.Parse(unitLevel_inputfield.text);
		}
	}

	public void SetEventPosXSpawnUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(posX_inputfield.text) >= GameData.Instance.map.SizeX || int.Parse(posX_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posX_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posX_inputfield.text), 0, GameData.Instance.map.SizeX - 1).ToString());
		}
		else
		{
			_eventAction.PosX = int.Parse(posX_inputfield.text);
		}
	}

	public void SetEventPosYSpawnUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(posY_inputfield.text) >= GameData.Instance.map.SizeY || int.Parse(posY_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posY_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posY_inputfield.text), 0, GameData.Instance.map.SizeY - 1).ToString());
		}
		else
		{
			_eventAction.PosY = int.Parse(posY_inputfield.text);
		}
	}

	public void SetUnit()
	{
		SoundManager.instance.PlayUIClickSound();
		if (GameData.Instance.TryFindPlayerByName(_eventAction.TargetPlayerName1, out var o_player))
		{
			TurnManager.currPlayer = o_player;
			MapEditorGO.instance.store_Panel.GetComponent<StoreGO>().OpenPanelInEventMode(this);
			EventPanelGO.instance.RefreshEventList();
		}
	}

	public void OnBoughtUnit()
	{
		_eventAction.UnitLevel = _eventAction.Unit.Lvl;
		RefreshAllValues();
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetNewPlayerCountry()
	{
		SoundManager.instance.PlayUIClickSound();
		_eventAction.TargetCountryName = Database.Countries[allCountries_dropdown.value];
		EventPanelGO.instance.RefreshEventList();
	}

	public void SetPlayerIndex()
	{
		SoundManager.instance.PlayUIClickSound();
		if (byte.TryParse(playerIndex_inputfield.text, out var result))
		{
			_eventAction.PlayerIndex = (byte)Mathf.Clamp(result, 0, (byte)GameData.Instance.listOfPlayers.Count);
		}
		else
		{
			_eventAction.PlayerIndex = 0;
		}
		playerIndex_inputfield.SetTextWithoutNotify(_eventAction.PlayerIndex.ToString());
		EventPanelGO.instance.RefreshEventList();
	}

	public void CloneAction_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		_event.EventActions.Add(_eventAction.Clone());
		EventPanelGO.instance.RefreshEventList();
		EventPanelGO.instance.RefreshActions();
	}

	public void DeleteAction_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		_event.EventActions.Remove(_eventAction);
		EventPanelGO.instance.RefreshEventList();
		EventPanelGO.instance.RefreshActions();
	}
}
