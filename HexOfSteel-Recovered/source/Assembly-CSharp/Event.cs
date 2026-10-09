using System;
using System.Collections.Generic;

[Serializable]
public class Event
{
	public int ID { get; set; }

	public string Name { get; set; }

	public string Group { get; set; }

	public string MasterPlayer { get; set; }

	public CheckTypes CheckType { get; private set; }

	public List<EventAction> EventActions { get; set; }

	public int Chance { get; set; }

	public int MaxExecutions { get; set; }

	public int ExecutionsLeft { get; set; }

	public bool TurnBasedEvent { get; set; }

	public bool HistoricalFocusOnly { get; set; }

	public bool ForceHumanApplyEvent { get; set; }

	public bool AIonly { get; set; }

	public bool HumansOnly { get; set; }

	public short CheckTileCoordinateX { get; set; }

	public short CheckTileCoordinateY { get; set; }

	public short CheckTurnCount { get; set; }

	public short DontCheckBeforeTurnCount { get; set; }

	public int CheckUnitID { get; set; }

	public DateTime CheckDate { get; set; }

	public DateTime DontCheckBeforeDate { get; set; }

	public List<Coordinates> VPsToBeLost { get; set; }

	public string Player1Check { get; set; }

	public string Player2Check { get; set; }

	public Event(int p_ID)
	{
		EventActions = new List<EventAction>();
		VPsToBeLost = new List<Coordinates>();
		Group = "None";
		Chance = 100;
		MaxExecutions = 1;
		CheckDate = new DateTime(1939, 9, 1);
		DontCheckBeforeDate = new DateTime(1930, 1, 1);
		ID = p_ID;
		SetDefaultName();
	}

	public void SetDefaultName()
	{
		Name = LocalizationManager.Translate("txt-new event");
	}

	public void SetCheckType(CheckTypes p_checkType)
	{
		CheckType = p_checkType;
		TurnBasedEvent = CheckType == CheckTypes.TURN_COUNT;
		if (CheckType == CheckTypes.DATE || CheckType == CheckTypes.TURN_COUNT)
		{
			DontCheckBeforeTurnCount = 0;
			DontCheckBeforeDate = new DateTime(1900, 1, 1);
		}
	}

	public void RemoveExecution()
	{
		ExecutionsLeft--;
		if (ExecutionsLeft < 0)
		{
			ExecutionsLeft = 0;
		}
	}

	public bool FullyProcessed()
	{
		foreach (EventAction eventAction in EventActions)
		{
			if (!eventAction.Processed)
			{
				return false;
			}
		}
		return ExecutionsLeft == 0;
	}

	public Event Clone()
	{
		Event obj = new Event(GameData.Instance.map.GetNewEventID());
		obj.Name = Name;
		obj.MasterPlayer = MasterPlayer;
		obj.Group = Group;
		obj.CheckType = CheckType;
		obj.Chance = Chance;
		obj.MaxExecutions = MaxExecutions;
		obj.AIonly = AIonly;
		obj.HumansOnly = HumansOnly;
		obj.HistoricalFocusOnly = HistoricalFocusOnly;
		obj.TurnBasedEvent = TurnBasedEvent;
		obj.ForceHumanApplyEvent = ForceHumanApplyEvent;
		obj.CheckTurnCount = CheckTurnCount;
		obj.DontCheckBeforeTurnCount = DontCheckBeforeTurnCount;
		obj.CheckDate = CheckDate;
		obj.DontCheckBeforeDate = DontCheckBeforeDate;
		obj.CheckTileCoordinateX = CheckTileCoordinateX;
		obj.CheckTileCoordinateY = CheckTileCoordinateY;
		obj.Player1Check = Player1Check;
		obj.Player2Check = Player2Check;
		obj.EventActions = new List<EventAction>();
		foreach (EventAction eventAction in EventActions)
		{
			obj.EventActions.Add(eventAction.Clone());
		}
		obj.VPsToBeLost = new List<Coordinates>();
		foreach (Coordinates item in VPsToBeLost)
		{
			obj.VPsToBeLost.Add(new Coordinates(item.posX, item.posY));
		}
		return obj;
	}

	public static string TranslateEventCheck(CheckTypes p_checkType)
	{
		switch (p_checkType)
		{
		default:
			return LocalizationManager.Translate("None");
		case CheckTypes.TURN_COUNT:
			return LocalizationManager.Translate("eventtype-on turn count reached");
		case CheckTypes.DATE:
			return LocalizationManager.Translate("eventtype-on date reached");
		case CheckTypes.VP_LOSS:
			return LocalizationManager.Translate("eventtype-on vp loss");
		case CheckTypes.VPS_LOSSES:
			return LocalizationManager.Translate("eventtype-on vps loss");
		case CheckTypes.VPS_LOSSES_ONE_OF:
			return LocalizationManager.Translate("eventtype-on one of VPs lost");
		case CheckTypes.WAR_DECLARATION:
			return LocalizationManager.Translate("eventtype-on war declared");
		case CheckTypes.ALLIANCE_MADE:
			return LocalizationManager.Translate("eventtype-on alliance made");
		case CheckTypes.UNIT_DESTROYED:
			return LocalizationManager.Translate("eventtype-on unit destroyed");
		}
	}

	public bool IsEventReady(bool p_includingActions)
	{
		if (CheckType == CheckTypes.NONE)
		{
			return false;
		}
		if (EventActions.Count == 0)
		{
			return false;
		}
		if (CheckType == CheckTypes.UNIT_DESTROYED && CheckUnitID == 0)
		{
			return false;
		}
		if ((CheckType == CheckTypes.VPS_LOSSES || CheckType == CheckTypes.VPS_LOSSES_ONE_OF) && VPsToBeLost.Count == 0)
		{
			return false;
		}
		if ((CheckType == CheckTypes.WAR_DECLARATION || CheckType == CheckTypes.ALLIANCE_MADE) && (!GameData.Instance.TryFindPlayerByName(Player1Check, out var _) || !GameData.Instance.TryFindPlayerByName(Player2Check, out var _)))
		{
			return false;
		}
		if (p_includingActions)
		{
			foreach (EventAction eventAction in EventActions)
			{
				if (!eventAction.IsActionSetup())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsValidForLossTrigger(Tile p_tile)
	{
		foreach (Coordinates item in VPsToBeLost)
		{
			if (item.posX == p_tile.PosX && item.posY == p_tile.PosY)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreAllVPsOccupied(Map p_map, Tile p_tile, Player p_playerCapturing)
	{
		foreach (Coordinates item in VPsToBeLost)
		{
			if (item.posX == p_tile.PosX && item.posY == p_tile.PosY)
			{
				if (p_tile.OriginalCountryOwner == p_playerCapturing.Country)
				{
					return false;
				}
				continue;
			}
			if (p_map.TilesTable[item.posX, item.posY].ownerPlayer == null)
			{
				return false;
			}
			if (p_map.TilesTable[item.posX, item.posY].OriginalCountryOwner == p_map.TilesTable[item.posX, item.posY].ownerPlayer.Country)
			{
				return false;
			}
		}
		return true;
	}
}
