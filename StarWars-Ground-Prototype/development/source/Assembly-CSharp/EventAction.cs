using System;

[Serializable]
public class EventAction
{
	public ActionTypes Type { get; set; }

	public bool Processed { get; set; }

	public string MessageText { get; set; }

	public string TargetPlayerName1 { get; set; }

	public string TargetPlayerName2 { get; set; }

	public string TargetFactionName { get; set; }

	public string TargetCountryName { get; set; }

	public string TargetIdeology { get; set; }

	public int MoneyAmount { get; set; }

	public short HQPointsAmount { get; set; }

	public int PosX { get; set; }

	public int PosY { get; set; }

	public bool OriginalOwnerOnly { get; set; }

	public string CityName { get; set; }

	public string TileName { get; set; }

	public string TileSeason { get; set; }

	public Unit Unit { get; set; }

	public string UnitName { get; set; }

	public string UnitCountry { get; set; }

	public byte UnitLevel { get; set; }

	public byte PlayerIndex { get; set; }

	public EventAction()
	{
		UnitLevel = 1;
	}

	public void Process()
	{
		Processed = true;
	}

	public void ResetProcess()
	{
		Processed = false;
	}

	public EventAction Clone()
	{
		return new EventAction
		{
			Type = Type,
			MessageText = MessageText,
			TargetPlayerName1 = TargetPlayerName1,
			TargetPlayerName2 = TargetPlayerName2,
			TargetFactionName = TargetFactionName,
			TargetCountryName = TargetCountryName,
			TargetIdeology = TargetIdeology,
			MoneyAmount = MoneyAmount,
			HQPointsAmount = HQPointsAmount,
			PosX = PosX,
			PosY = PosY,
			OriginalOwnerOnly = OriginalOwnerOnly,
			CityName = CityName,
			TileName = TileName,
			TileSeason = TileSeason,
			UnitCountry = UnitCountry,
			UnitName = UnitName,
			UnitLevel = UnitLevel,
			Unit = ((Unit != null) ? Unit.Clone(p_generateNewID: true) : null),
			PlayerIndex = PlayerIndex
		};
	}

	public static string TranslateEventAction(ActionTypes p_actionType)
	{
		switch (p_actionType)
		{
		default:
			return LocalizationManager.Translate("eventaction-no action");
		case ActionTypes.ADD_MONEY:
			return LocalizationManager.Translate("eventaction-add money");
		case ActionTypes.ADD_HQ_POINTS:
			return LocalizationManager.Translate("eventaction-add hq points");
		case ActionTypes.FACTION_CHANGE:
			return LocalizationManager.Translate("eventaction-faction change");
		case ActionTypes.CAPITULATION:
			return LocalizationManager.Translate("eventaction-capitulation");
		case ActionTypes.CHANGE_VP_OWNERSHIP:
			return LocalizationManager.Translate("eventaction-change vp owner");
		case ActionTypes.SHOW_MESSAGE:
			return LocalizationManager.Translate("eventaction-show message");
		case ActionTypes.SPAWN_UNIT:
			return LocalizationManager.Translate("eventaction-spawn unit");
		case ActionTypes.MAKE_PEACE:
			return LocalizationManager.Translate("eventaction-make peace");
		case ActionTypes.MAKE_ALLIANCE:
			return LocalizationManager.Translate("eventaction-make alliance");
		case ActionTypes.DECLARE_WAR:
			return LocalizationManager.Translate("eventaction-declare war");
		case ActionTypes.ADD_UNIT_RESERVE:
			return LocalizationManager.Translate("eventaction-add unit reserve");
		case ActionTypes.MAKE_PEACE_FACTION:
			return LocalizationManager.Translate("eventaction-faction peace");
		case ActionTypes.COUNTRY_CHANGE:
			return LocalizationManager.Translate("eventaction-country change");
		case ActionTypes.DESTROY_TILE:
			return LocalizationManager.Translate("eventaction-destroy tile");
		case ActionTypes.RESET_INCOME:
			return LocalizationManager.Translate("eventaction-reset income");
		case ActionTypes.ADD_FIXED_INCOME:
			return LocalizationManager.Translate("eventaction-add fixed income");
		case ActionTypes.SET_FIXED_INCOME:
			return LocalizationManager.Translate("eventaction-set fixed income");
		case ActionTypes.SET_FIXED_INCOME_PERCENT:
			return LocalizationManager.Translate("eventaction-set fixed income") + " (%)";
		case ActionTypes.SET_FIXED_INCOME_FROM_BASE_INCOME_PERCENT:
			return LocalizationManager.Translate("eventaction-set fixed income from base income");
		case ActionTypes.SET_TILE:
			return LocalizationManager.Translate("eventaction-update tile");
		case ActionTypes.ADD_PLAYER:
			return LocalizationManager.Translate("eventaction-add player");
		case ActionTypes.REMOVE_PLAYER:
			return LocalizationManager.Translate("eventaction-remove player");
		case ActionTypes.ADD_VP:
			return LocalizationManager.Translate("eventaction-add vp");
		case ActionTypes.REMOVE_VP:
			return LocalizationManager.Translate("eventaction-remove vp");
		case ActionTypes.SET_TILE_NAME:
			return LocalizationManager.Translate("eventaction-set tile name");
		}
	}

	public bool IsActionSetup()
	{
		switch (Type)
		{
		case ActionTypes.NONE:
			return false;
		case ActionTypes.ADD_MONEY:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		case ActionTypes.ADD_HQ_POINTS:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		case ActionTypes.SET_TILE:
			if (string.IsNullOrEmpty(TileName))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TileSeason))
			{
				return false;
			}
			if (TileName == "Null")
			{
				return false;
			}
			if (TileSeason == "Null")
			{
				return false;
			}
			break;
		case ActionTypes.SET_TILE_NAME:
			if (string.IsNullOrEmpty(CityName))
			{
				return false;
			}
			break;
		case ActionTypes.FACTION_CHANGE:
		case ActionTypes.MAKE_PEACE_FACTION:
			if (string.IsNullOrEmpty(TargetFactionName))
			{
				return false;
			}
			break;
		case ActionTypes.CAPITULATION:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		case ActionTypes.MAKE_PEACE:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TargetPlayerName2))
			{
				return false;
			}
			break;
		case ActionTypes.MAKE_ALLIANCE:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TargetPlayerName2))
			{
				return false;
			}
			break;
		case ActionTypes.DECLARE_WAR:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TargetPlayerName2))
			{
				return false;
			}
			break;
		case ActionTypes.COUNTRY_CHANGE:
			if (string.IsNullOrEmpty(TargetCountryName))
			{
				return false;
			}
			break;
		case ActionTypes.CHANGE_VP_OWNERSHIP:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		case ActionTypes.ADD_VP:
		case ActionTypes.REMOVE_VP:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		case ActionTypes.ADD_UNIT_RESERVE:
			if (Unit == null)
			{
				if (string.IsNullOrEmpty(TargetPlayerName1))
				{
					return false;
				}
				if (string.IsNullOrEmpty(UnitName))
				{
					return false;
				}
				if (string.IsNullOrEmpty(UnitCountry))
				{
					return false;
				}
			}
			break;
		case ActionTypes.SPAWN_UNIT:
			if (Unit == null)
			{
				if (string.IsNullOrEmpty(TargetPlayerName1))
				{
					return false;
				}
				if (string.IsNullOrEmpty(UnitName))
				{
					return false;
				}
				if (string.IsNullOrEmpty(UnitCountry))
				{
					return false;
				}
			}
			break;
		case ActionTypes.ADD_PLAYER:
			if (string.IsNullOrEmpty(TargetCountryName))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TargetFactionName))
			{
				return false;
			}
			if (PlayerIndex > GameData.Instance.listOfPlayers.Count)
			{
				return false;
			}
			break;
		case ActionTypes.REMOVE_PLAYER:
			if (string.IsNullOrEmpty(TargetPlayerName1))
			{
				return false;
			}
			break;
		}
		return true;
	}
}
