using System;

[Serializable]
public class Notification
{
	public int TurnCreated { get; set; }

	public NotificationTypes Type { get; set; }

	public string OwnerName { get; set; }

	public string SenderName { get; set; }

	public float Amount { get; set; }

	public string Description { get; set; }

	public int UnitID { get; set; }

	public string UnitResearchedFilterType { get; set; }

	public float PosX { get; set; }

	public float PosY { get; set; }

	public bool IsImportant { get; set; }

	public bool HasBeenShown { get; set; }

	public Notification(NotificationTypes p_notificationType, string p_targetPlayerName, float p_PosX, float p_PosY, string p_Description, bool p_isImportant, int p_unitID = 0)
	{
		Type = p_notificationType;
		IsImportant = p_isImportant;
		OwnerName = p_targetPlayerName;
		PosX = p_PosX;
		PosY = p_PosY;
		Description = p_Description;
		UnitID = p_unitID;
		TurnCreated = GameData.Instance.map.turnCount;
		if (IsUnitDeathNotification())
		{
			GameData.Instance.map.RemoveLowHealthNotificationForUnit(UnitID, OwnerName, Description);
		}
		foreach (Notification notification in GameData.Instance.map.Notifications)
		{
			if (Type == notification.Type && TurnCreated == notification.TurnCreated && notification.PosX == PosX && notification.PosY == PosY && notification.Description == Description && OwnerName == notification.OwnerName)
			{
				return;
			}
		}
		GameData.Instance.map.Notifications.Add(this);
	}

	private bool IsUnitDeathNotification()
	{
		if (Type != NotificationTypes.UNIT_KILLED && Type != NotificationTypes.UNIT_DESTROYED && Type != NotificationTypes.UNIT_SUNK && Type != NotificationTypes.UNIT_SHOT_DOWN && Type != NotificationTypes.UNIT_SURRENDERED)
		{
			return Type == NotificationTypes.UNIT_BLEW_UP;
		}
		return true;
	}

	public Notification(NotificationTypes p_notificationType, string p_targetPlayerName, string p_Description, float p_amount, bool p_isImportant)
	{
		Type = p_notificationType;
		IsImportant = p_isImportant;
		OwnerName = p_targetPlayerName;
		Amount = p_amount;
		Description = p_Description;
		TurnCreated = GameData.Instance.map.turnCount;
		GameData.Instance.map.Notifications.Add(this);
	}
}
