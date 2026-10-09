using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelGO : MonoBehaviour
{
	public static EventPanelGO instance;

	[Header("Setup")]
	[SerializeField]
	private Transform _eventList_parent;

	[SerializeField]
	private Transform _actionList_parent;

	[SerializeField]
	private EventListSlotGO _eventListSlot_prefab;

	[SerializeField]
	private EventActionSlotGO _eventActionSlot_prefab;

	[SerializeField]
	private GameObject _separator_prefab;

	[SerializeField]
	private GameObject _selectedEventStats;

	[SerializeField]
	private GameObject _notBeforeDateBox;

	[SerializeField]
	private GameObject _notBeforeTurnBox;

	[Header("InputFields")]
	[SerializeField]
	private TMP_InputField turn_inputfield;

	[SerializeField]
	private TMP_InputField day_inputfield;

	[SerializeField]
	private TMP_InputField month_inputfield;

	[SerializeField]
	private TMP_InputField year_inputfield;

	[SerializeField]
	private TMP_InputField posX_inputfield;

	[SerializeField]
	private TMP_InputField posY_inputfield;

	[SerializeField]
	private TMP_InputField chance_inputfield;

	[SerializeField]
	private TMP_InputField executions_inputfield;

	[SerializeField]
	private TMP_InputField unitID_inputfield;

	[SerializeField]
	private TMP_InputField _notBeforeTurn_inputfield;

	[SerializeField]
	private TMP_InputField _notBeforeDateYear_inputfield;

	[SerializeField]
	private TMP_InputField _notBeforeDateMonth_inputfield;

	[SerializeField]
	private TMP_InputField _notBeforeDateDay_inputfield;

	[Header("Toggles")]
	[SerializeField]
	private Toggle historicalFocus_toggle;

	[SerializeField]
	private Toggle forceHumanApplyEvent_toggle;

	[SerializeField]
	private Toggle AIonlyEvent_toggle;

	[SerializeField]
	private Toggle humanOnlyEvent_toggle;

	[Header("Dropdowns")]
	[SerializeField]
	private TMP_Dropdown group_dropdown;

	[SerializeField]
	private TMP_Dropdown masterPlayer_dropdown;

	[SerializeField]
	private TMP_Dropdown checkType_dropdown;

	[SerializeField]
	private TMP_Dropdown VPsToBeLost_dropdown;

	[SerializeField]
	private TMP_Dropdown player1check_dropdown;

	[SerializeField]
	private TMP_Dropdown player2check_dropdown;

	[Header("Buttons")]
	[SerializeField]
	private Button addVPLoss_button;

	[SerializeField]
	private Button removeVPLoss_button;

	[Header("InputFields")]
	[SerializeField]
	private TMP_InputField eventName_inputfield;

	private Event _selectedEvent;

	private List<string> _availableScenariosCountries;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		_availableScenariosCountries = new List<string> { "None" };
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!_availableScenariosCountries.Contains(listOfPlayer.Country))
			{
				_availableScenariosCountries.Add(listOfPlayer.Country);
			}
		}
		if (GameData.Instance.map.Events.Contains(_selectedEvent))
		{
			SelectEvent(_selectedEvent);
		}
		else
		{
			_selectedEvent = null;
		}
		RefreshEventList();
	}

	private void Update()
	{
		_selectedEventStats.SetActive(_selectedEvent != null);
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		_selectedEvent = null;
		_selectedEventStats.SetActive(value: false);
		gameObject.SetActive(value: false);
	}

	public void SelectEvent(Event p_event)
	{
		_selectedEvent = p_event;
		RefreshSelectedEventInfos();
		RefreshActions();
	}

	public void RefreshEventList()
	{
		Transform[] componentsInChildren = _eventList_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != _eventList_parent)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
		GameData.Instance.map.Events = (from x in GameData.Instance.map.Events
			orderby !x.IsEventReady(p_includingActions: false) descending, x.Group == "None", x.Group, x.Name
			select x).ToList();
		string text = null;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Event evnt in GameData.Instance.map.Events)
		{
			if (!evnt.IsEventReady(p_includingActions: false))
			{
				if (!hashSet.Contains("Event not ready"))
				{
					hashSet.Add("Event not ready");
					UnityEngine.Object.Instantiate(_separator_prefab, _eventList_parent).GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Translate("txt-events need set up");
				}
			}
			else if (text != evnt.Group && !hashSet.Contains(evnt.Group))
			{
				UnityEngine.Object.Instantiate(_separator_prefab, _eventList_parent).GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(evnt.Group);
				hashSet.Add(evnt.Group);
			}
			EventListSlotGO eventListSlotGO = UnityEngine.Object.Instantiate(_eventListSlot_prefab, _eventList_parent);
			eventListSlotGO.eventName_text.text = evnt.Name;
			eventListSlotGO.select_button.onClick.AddListener(() =>
			{
				SelectEvent(evnt);
			});
			eventListSlotGO.delete_button.onClick.AddListener(() =>
			{
				DeleteEvent_Button(evnt);
			});
			eventListSlotGO.clone_button.onClick.AddListener(() =>
			{
				CloneEvent_Button(evnt);
			});
			if (!evnt.IsEventReady(p_includingActions: true))
			{
				eventListSlotGO.warning_image.gameObject.SetActive(value: true);
			}
		}
	}

	public void RefreshActions()
	{
		EventActionSlotGO[] componentsInChildren = _actionList_parent.GetComponentsInChildren<EventActionSlotGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		foreach (EventAction eventAction in _selectedEvent.EventActions)
		{
			UnityEngine.Object.Instantiate(_eventActionSlot_prefab, _actionList_parent).Setup(_selectedEvent, eventAction);
		}
	}

	private void RefreshSelectedEventInfos()
	{
		if (_selectedEvent == null)
		{
			return;
		}
		eventName_inputfield.SetTextWithoutNotify(_selectedEvent.Name);
		chance_inputfield.SetTextWithoutNotify(_selectedEvent.Chance.ToString());
		executions_inputfield.SetTextWithoutNotify(_selectedEvent.MaxExecutions.ToString());
		unitID_inputfield.SetTextWithoutNotify(_selectedEvent.CheckUnitID.ToString());
		historicalFocus_toggle.SetIsOnWithoutNotify(_selectedEvent.HistoricalFocusOnly);
		forceHumanApplyEvent_toggle.SetIsOnWithoutNotify(_selectedEvent.ForceHumanApplyEvent);
		AIonlyEvent_toggle.SetIsOnWithoutNotify(_selectedEvent.AIonly);
		humanOnlyEvent_toggle.SetIsOnWithoutNotify(_selectedEvent.HumansOnly);
		checkType_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (CheckTypes value in Enum.GetValues(typeof(CheckTypes)))
		{
			checkType_dropdown.options.Add(new TMP_Dropdown.OptionData(Event.TranslateEventCheck(value)));
		}
		checkType_dropdown.SetValueWithoutNotify(Utils.GetIndexByName(checkType_dropdown, Event.TranslateEventCheck(_selectedEvent.CheckType)));
		checkType_dropdown.RefreshShownValue();
		group_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string availableScenariosCountry in _availableScenariosCountries)
		{
			group_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(availableScenariosCountry)));
		}
		group_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedEvent.Group, _availableScenariosCountries));
		group_dropdown.RefreshShownValue();
		masterPlayer_dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (string availableScenariosCountry2 in _availableScenariosCountries)
		{
			masterPlayer_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLocalizedCountry(availableScenariosCountry2)));
		}
		masterPlayer_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(_selectedEvent.MasterPlayer, _availableScenariosCountries));
		masterPlayer_dropdown.RefreshShownValue();
		player1check_dropdown.options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-no player selected"))
		};
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			player1check_dropdown.options.Add(new TMP_Dropdown.OptionData(listOfPlayer.Country));
		}
		if (GameData.Instance.TryFindPlayerByName(_selectedEvent.Player1Check, out var o_player))
		{
			player1check_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(o_player.Country, _availableScenariosCountries));
		}
		else
		{
			player1check_dropdown.SetValueWithoutNotify(0);
		}
		player1check_dropdown.RefreshShownValue();
		player2check_dropdown.options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(LocalizationManager.Translate("txt-no player selected"))
		};
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			player2check_dropdown.options.Add(new TMP_Dropdown.OptionData(listOfPlayer2.Country));
		}
		if (GameData.Instance.TryFindPlayerByName(_selectedEvent.Player2Check, out var o_player2))
		{
			player2check_dropdown.SetValueWithoutNotify(Utils.GetIndexByLocalizedName(o_player2.Country, _availableScenariosCountries));
		}
		else
		{
			player2check_dropdown.SetValueWithoutNotify(0);
		}
		player2check_dropdown.RefreshShownValue();
		RefreshVPsToBeLostDropdown();
		turn_inputfield.gameObject.SetActive(value: false);
		unitID_inputfield.gameObject.SetActive(value: false);
		VPsToBeLost_dropdown.gameObject.SetActive(value: false);
		year_inputfield.gameObject.SetActive(value: false);
		month_inputfield.gameObject.SetActive(value: false);
		day_inputfield.gameObject.SetActive(value: false);
		posX_inputfield.gameObject.SetActive(value: false);
		posY_inputfield.gameObject.SetActive(value: false);
		addVPLoss_button.gameObject.SetActive(value: false);
		removeVPLoss_button.gameObject.SetActive(value: false);
		player1check_dropdown.gameObject.SetActive(value: false);
		player2check_dropdown.gameObject.SetActive(value: false);
		_notBeforeDateBox.SetActive(value: false);
		_notBeforeTurnBox.SetActive(value: false);
		if (_selectedEvent.CheckType == CheckTypes.DATE)
		{
			year_inputfield.SetTextWithoutNotify(_selectedEvent.CheckDate.Year.ToString());
			month_inputfield.SetTextWithoutNotify(_selectedEvent.CheckDate.Month.ToString());
			day_inputfield.SetTextWithoutNotify(_selectedEvent.CheckDate.Day.ToString());
			year_inputfield.gameObject.SetActive(value: true);
			month_inputfield.gameObject.SetActive(value: true);
			day_inputfield.gameObject.SetActive(value: true);
		}
		else if (_selectedEvent.CheckType == CheckTypes.TURN_COUNT)
		{
			turn_inputfield.SetTextWithoutNotify(_selectedEvent.CheckTurnCount.ToString());
			turn_inputfield.gameObject.SetActive(value: true);
		}
		else if (_selectedEvent.CheckType == CheckTypes.VP_LOSS)
		{
			posX_inputfield.SetTextWithoutNotify(_selectedEvent.CheckTileCoordinateX.ToString());
			posY_inputfield.SetTextWithoutNotify(_selectedEvent.CheckTileCoordinateY.ToString());
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			_notBeforeTurnBox.SetActive(value: true);
			_notBeforeTurn_inputfield.SetTextWithoutNotify(_selectedEvent.DontCheckBeforeTurnCount.ToString());
		}
		else if (_selectedEvent.CheckType == CheckTypes.VPS_LOSSES || _selectedEvent.CheckType == CheckTypes.VPS_LOSSES_ONE_OF)
		{
			VPsToBeLost_dropdown.gameObject.SetActive(value: true);
			posX_inputfield.gameObject.SetActive(value: true);
			posY_inputfield.gameObject.SetActive(value: true);
			addVPLoss_button.gameObject.SetActive(value: true);
			removeVPLoss_button.gameObject.SetActive(value: true);
			if (_selectedEvent.VPsToBeLost.Count > 0)
			{
				posX_inputfield.SetTextWithoutNotify(_selectedEvent.VPsToBeLost[0].posX.ToString());
				posY_inputfield.SetTextWithoutNotify(_selectedEvent.VPsToBeLost[0].posY.ToString());
			}
			else
			{
				posX_inputfield.SetTextWithoutNotify(null);
				posY_inputfield.SetTextWithoutNotify(null);
			}
			_notBeforeTurnBox.SetActive(value: true);
			_notBeforeTurn_inputfield.SetTextWithoutNotify(_selectedEvent.DontCheckBeforeTurnCount.ToString());
		}
		else if (_selectedEvent.CheckType == CheckTypes.WAR_DECLARATION || _selectedEvent.CheckType == CheckTypes.ALLIANCE_MADE)
		{
			player1check_dropdown.gameObject.SetActive(value: true);
			player2check_dropdown.gameObject.SetActive(value: true);
			_notBeforeTurnBox.SetActive(value: true);
			_notBeforeTurn_inputfield.SetTextWithoutNotify(_selectedEvent.DontCheckBeforeTurnCount.ToString());
		}
		else if (_selectedEvent.CheckType == CheckTypes.UNIT_DESTROYED)
		{
			unitID_inputfield.gameObject.SetActive(value: true);
			_notBeforeTurnBox.SetActive(value: true);
			_notBeforeTurn_inputfield.SetTextWithoutNotify(_selectedEvent.DontCheckBeforeTurnCount.ToString());
		}
	}

	private void RefreshVPsToBeLostDropdown()
	{
		VPsToBeLost_dropdown.options = new List<TMP_Dropdown.OptionData>();
		if (_selectedEvent.VPsToBeLost.Count == 0)
		{
			VPsToBeLost_dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("drpdwn-no vps added")));
		}
		else
		{
			foreach (Coordinates item in _selectedEvent.VPsToBeLost)
			{
				short posX = item.posX;
				string text = posX.ToString();
				posX = item.posY;
				string text2 = text + " : " + posX;
				text2 = (string.IsNullOrEmpty(GameData.Instance.map.TilesTable[item.posX, item.posY].CityName) ? (text2 + ", " + GameData.Instance.map.TilesTable[item.posX, item.posY].GetLocalizedDisplayName()) : (text2 + ", " + GameData.Instance.map.TilesTable[item.posX, item.posY].CityName));
				VPsToBeLost_dropdown.options.Add(new TMP_Dropdown.OptionData(text2));
			}
		}
		VPsToBeLost_dropdown.SetValueWithoutNotify(0);
		VPsToBeLost_dropdown.RefreshShownValue();
	}

	public void AddEvent_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.Events.Add(new Event(GameData.Instance.map.GetNewEventID()));
		_selectedEvent = null;
		RefreshEventList();
	}

	public void AddAction_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		EventAction eventAction = new EventAction();
		if (!string.IsNullOrEmpty(_selectedEvent.Group) && _selectedEvent.Group != "None" && GameData.Instance.TryFindPlayerByCountry(_selectedEvent.Group, out var o_player))
		{
			eventAction.TargetPlayerName1 = o_player.Name;
			eventAction.UnitCountry = o_player.Country;
		}
		_selectedEvent.EventActions.Add(eventAction);
		RefreshEventList();
		RefreshActions();
	}

	public void CloneEvent()
	{
		CloneEvent_Button(_selectedEvent);
	}

	public void DeleteEvent()
	{
		DeleteEvent_Button(_selectedEvent);
		_selectedEvent = null;
	}

	public void CloneEvent_Button(Event p_event)
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.Events.Add(p_event.Clone());
		RefreshEventList();
	}

	public void DeleteEvent_Button(Event p_event)
	{
		SoundManager.instance.PlayUIClickSound();
		GameData.Instance.map.RemoveEvent(p_event.ID);
		_selectedEvent = null;
		RefreshEventList();
	}

	public void SetEventHistoricalFocusOnly()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.HistoricalFocusOnly = !_selectedEvent.HistoricalFocusOnly;
	}

	public void SetForceHumanApplyEvent()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.ForceHumanApplyEvent = !_selectedEvent.ForceHumanApplyEvent;
	}

	public void SetAIOnlyEvent()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.AIonly = !_selectedEvent.AIonly;
		if (_selectedEvent.AIonly)
		{
			_selectedEvent.HumansOnly = false;
			humanOnlyEvent_toggle.SetIsOnWithoutNotify(value: false);
		}
	}

	public void SetHumanOnlyEvent()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.HumansOnly = !_selectedEvent.HumansOnly;
		if (_selectedEvent.HumansOnly)
		{
			_selectedEvent.AIonly = false;
			AIonlyEvent_toggle.SetIsOnWithoutNotify(value: false);
		}
	}

	public void SetEventName()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.Name = eventName_inputfield.text;
		RefreshEventList();
	}

	public void SetEventCheckType()
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedEvent.SetCheckType((CheckTypes)checkType_dropdown.value);
		RefreshSelectedEventInfos();
		RefreshEventList();
	}

	public void SetEventGroup()
	{
		SoundManager.instance.PlayUIClickSound();
		if (group_dropdown.value == 0)
		{
			_selectedEvent.Group = "None";
		}
		else
		{
			_selectedEvent.Group = _availableScenariosCountries[group_dropdown.value];
		}
		RefreshEventList();
	}

	public void SetEventMasterPlayer()
	{
		SoundManager.instance.PlayUIClickSound();
		if (masterPlayer_dropdown.value == 0)
		{
			_selectedEvent.MasterPlayer = "None";
		}
		else
		{
			_selectedEvent.MasterPlayer = _availableScenariosCountries[masterPlayer_dropdown.value];
		}
		RefreshEventList();
	}

	public void SetEventPlayer1ActionCheck()
	{
		SoundManager.instance.PlayUIClickSound();
		if (GameData.Instance.TryFindPlayerByCountry(_availableScenariosCountries[player1check_dropdown.value], out var o_player))
		{
			_selectedEvent.Player1Check = o_player.Name;
		}
	}

	public void SetEventPlayer2ActionCheck()
	{
		SoundManager.instance.PlayUIClickSound();
		if (GameData.Instance.TryFindPlayerByCountry(_availableScenariosCountries[player2check_dropdown.value], out var o_player))
		{
			_selectedEvent.Player2Check = o_player.Name;
		}
	}

	public void SetEventMaxExecutions()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(executions_inputfield.text) < 1)
		{
			executions_inputfield.SetTextWithoutNotify(1.ToString());
		}
		else
		{
			_selectedEvent.MaxExecutions = int.Parse(executions_inputfield.text);
		}
	}

	public void SetEventCheckUnitID()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.TryParse(unitID_inputfield.text, out var result))
		{
			_selectedEvent.CheckUnitID = result;
		}
		else
		{
			_selectedEvent.CheckUnitID = 0;
		}
	}

	public void SetEventChance()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(chance_inputfield.text) > 100)
		{
			chance_inputfield.SetTextWithoutNotify(100.ToString());
		}
		if (int.Parse(chance_inputfield.text) < 0)
		{
			chance_inputfield.SetTextWithoutNotify(0.ToString());
		}
		_selectedEvent.Chance = int.Parse(chance_inputfield.text);
	}

	public void AddEventVPLosses()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(posX_inputfield.text) >= GameData.Instance.map.SizeX || int.Parse(posX_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posX_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posX_inputfield.text), 0, GameData.Instance.map.SizeX - 1).ToString());
			return;
		}
		if (int.Parse(posY_inputfield.text) >= GameData.Instance.map.SizeY || int.Parse(posY_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posY_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posY_inputfield.text), 0, GameData.Instance.map.SizeY - 1).ToString());
			return;
		}
		Coordinates item = new Coordinates(short.Parse(posX_inputfield.text), short.Parse(posY_inputfield.text));
		foreach (Coordinates item2 in _selectedEvent.VPsToBeLost)
		{
			if (item2.posX == item.posX && item2.posY == item.posY)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-tile already added"));
				return;
			}
		}
		_selectedEvent.VPsToBeLost.Add(item);
		RefreshVPsToBeLostDropdown();
		VPsToBeLost_dropdown.SetValueWithoutNotify(_selectedEvent.VPsToBeLost.Count);
		VPsToBeLost_dropdown.RefreshShownValue();
	}

	public void RemoveEventVPLosses()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_selectedEvent.VPsToBeLost.Count != 0)
		{
			_selectedEvent.VPsToBeLost.RemoveAt(VPsToBeLost_dropdown.value);
			RefreshVPsToBeLostDropdown();
		}
	}

	public void SetEventCheckTurn()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(turn_inputfield.text) < 0)
		{
			turn_inputfield.SetTextWithoutNotify(0.ToString());
		}
		else
		{
			_selectedEvent.CheckTurnCount = short.Parse(turn_inputfield.text);
		}
	}

	public void SetEventCheckYear()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.CheckDate = new DateTime(int.Parse(year_inputfield.text), checkDate.Month, checkDate.Day);
		}
		catch
		{
			_selectedEvent.CheckDate = new DateTime(1939, checkDate.Month, checkDate.Day);
		}
	}

	public void SetEventCheckMonth()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.CheckDate = new DateTime(checkDate.Year, int.Parse(month_inputfield.text), checkDate.Day);
		}
		catch
		{
			_selectedEvent.CheckDate = new DateTime(checkDate.Year, 1, checkDate.Day);
		}
	}

	public void SetEventCheckDay()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.CheckDate = new DateTime(checkDate.Year, checkDate.Month, int.Parse(day_inputfield.text));
		}
		catch
		{
			_selectedEvent.CheckDate = new DateTime(checkDate.Year, checkDate.Month, 1);
		}
	}

	public void SetEventCheckPosXSpawnUnitVPLoss()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(posX_inputfield.text) >= GameData.Instance.map.SizeX || int.Parse(posX_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posX_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posX_inputfield.text), 0, GameData.Instance.map.SizeX - 1).ToString());
		}
		else
		{
			_selectedEvent.CheckTileCoordinateX = short.Parse(posX_inputfield.text);
		}
	}

	public void SetEventCheckPosYSpawnUnitVPLoss()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(posY_inputfield.text) >= GameData.Instance.map.SizeY || int.Parse(posY_inputfield.text) < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-tile does not exist"));
			posY_inputfield.SetTextWithoutNotify(Mathf.Clamp(int.Parse(posY_inputfield.text), 0, GameData.Instance.map.SizeY - 1).ToString());
		}
		else
		{
			_selectedEvent.CheckTileCoordinateY = short.Parse(posY_inputfield.text);
		}
	}

	public void SetEventNotBeforeDateYear()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(int.Parse(_notBeforeDateYear_inputfield.text), checkDate.Month, checkDate.Day);
		}
		catch
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(1, checkDate.Month, checkDate.Day);
		}
	}

	public void SetEventNotBeforeDateMonth()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(checkDate.Year, int.Parse(_notBeforeDateMonth_inputfield.text), checkDate.Day);
		}
		catch
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(checkDate.Year, 1, checkDate.Day);
		}
	}

	public void SetEventNotBeforeDateDay()
	{
		SoundManager.instance.PlayUIClickSound();
		DateTime checkDate = _selectedEvent.CheckDate;
		try
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(checkDate.Year, checkDate.Month, int.Parse(_notBeforeDateDay_inputfield.text));
		}
		catch
		{
			_selectedEvent.DontCheckBeforeDate = new DateTime(checkDate.Year, checkDate.Month, 1);
		}
	}

	public void SetEventNotBeforeTurn()
	{
		SoundManager.instance.PlayUIClickSound();
		if (int.Parse(_notBeforeTurn_inputfield.text) < 0)
		{
			_notBeforeTurn_inputfield.SetTextWithoutNotify("0");
		}
		else
		{
			_selectedEvent.DontCheckBeforeTurnCount = short.Parse(_notBeforeTurn_inputfield.text);
		}
	}
}
