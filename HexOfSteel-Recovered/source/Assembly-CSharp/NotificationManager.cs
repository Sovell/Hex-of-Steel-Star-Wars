using System.Collections;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
	public static NotificationManager instance;

	[Header("Setup")]
	public Transform parent;

	[SerializeField]
	private Transform _notificationPanel;

	[SerializeField]
	private Transform _targetParent;

	[SerializeField]
	private NotificationButtonGO _regularNotification_prefab;

	[SerializeField]
	private NotificationButtonGO _importantNotification_prefab;

	private void Awake()
	{
		instance = this;
	}

	public void CenterCameraOnMap(Vector3 endPos)
	{
		StartCoroutine(CR_LerpFromTo(Camera.main.transform.position, endPos, 0.3f));
	}

	private IEnumerator CR_LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			CameraGO.instance.transform.position = Vector3.Lerp(pos1, pos2, t / duration);
			yield return 0;
		}
		CameraGO.instance.transform.position = new Vector3(pos2.x, pos2.y, -80f);
	}

	public void CreateNewUInotification(Notification p_notification)
	{
		NotificationButtonGO notificationButtonGO = (p_notification.IsImportant ? Object.Instantiate(_importantNotification_prefab, parent) : Object.Instantiate(_regularNotification_prefab, parent));
		if (TurnManager.currPlayer.IsComputer || p_notification.OwnerName != TurnManager.currPlayer.Name)
		{
			notificationButtonGO.gameObject.SetActive(value: false);
		}
		notificationButtonGO.Setup(p_notification);
		notificationButtonGO.transform.localScale = Vector3.one;
		GameData.Instance.TryFindPlayerByName(p_notification.SenderName, out var o_player);
		if (p_notification.Type == NotificationTypes.RESEARCH_COMPLETE)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-new tech") + p_notification.Description;
		}
		else if (p_notification.Type == NotificationTypes.REINFORCEMENTS_PLAYER)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-player got reinforcements");
		}
		else if (p_notification.Type == NotificationTypes.REINFORCEMENTS_ALLIES)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-allies got reinforcements");
		}
		else if (p_notification.Type == NotificationTypes.REINFORCEMENTS_ENEMIES)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-enemy got reinforcements");
		}
		else if (p_notification.Type == NotificationTypes.TERRITORY_GAIN)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-you gained territory");
		}
		else if (p_notification.Type == NotificationTypes.TERRITORY_LOSS)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-you lost territory");
		}
		else if (p_notification.Type == NotificationTypes.COUNTRY_CHANGED)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-your country changed") + LocalizationManager.GetLocalizedCountry(p_notification.Description);
		}
		else if (p_notification.Type == NotificationTypes.UNIT_REACHED_DESTINATION)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-reached destination");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_CANNOT_FIND_PATH)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-no path found");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_AMBUSH)
		{
			notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-you ambushed") + p_notification.Description;
		}
		else if (p_notification.Type == NotificationTypes.UNIT_UNDER_ATTACK)
		{
			string[] array = p_notification.Description.Split(';');
			notificationButtonGO.description_text.text = array[0] + LocalizationManager.Translate("notif-was attacked") + array[1];
		}
		else if (p_notification.Type == NotificationTypes.UNIT_KILLED)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-was killed");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_DESTROYED)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-was destroyed");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_SHOT_DOWN)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-was shot down");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_SUNK)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-was sunk");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_BLEW_UP)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-detonated");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_SURRENDERED)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-surrendered");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_LOW_HEALTH)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-low health");
		}
		else if (p_notification.Type == NotificationTypes.UNIT_NO_SUPPLIES)
		{
			notificationButtonGO.description_text.text = p_notification.Description + LocalizationManager.Translate("notif-out of supplies");
		}
		else if (p_notification.Type == NotificationTypes.ALLIANCE_INVITATION)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("notif-player invited you in his alliance") + LocalizationManager.GetLocalizedFaction(o_player.Faction);
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: ALLIANCE PROPOSITION.";
			}
		}
		else if (p_notification.Type == NotificationTypes.ALLIANCE_PROPOSITION_ACCEPTED)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("info-alliance proposition accepted");
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: ALLIANCE PROPOSITION ACCEPTED.";
			}
		}
		else if (p_notification.Type == NotificationTypes.ALLIANCE_PROPOSITION_REJECTED)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("info-alliance proposition rejected");
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: ALLIANCE PROPOSITION REJECTED.";
			}
		}
		else if (p_notification.Type == NotificationTypes.MONEY_TRANSFER_NEUTRALITY)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("info-sent you money") + p_notification.Amount + LocalizationManager.Translate("txt-funds");
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: NEUTRALITY MONEY TRANSFER.";
			}
		}
		else if (p_notification.Type == NotificationTypes.UNIT_PURCHASE)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("notif-bought from you") + p_notification.Description;
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: UNIT PURCHASE.";
			}
		}
		else if (p_notification.Type == NotificationTypes.MONEY_TRANSFER)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.Translate("notif-received funds 1") + " " + p_notification.Amount + LocalizationManager.Translate("notif-received funds 2") + LocalizationManager.GetLocalizedCountry(o_player.Country);
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: MONEY PURCHASE.";
			}
		}
		else if (p_notification.Type == NotificationTypes.UNIT_TRANSFER)
		{
			if (o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("notif-transferred you") + p_notification.Description;
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: UNIT TRANSFER.";
			}
		}
		else if (p_notification.Type == NotificationTypes.VP_ANNEXED)
		{
			string[] array2 = p_notification.Description.Split(':');
			int num = int.Parse(array2[0]);
			int num2 = int.Parse(array2[1]);
			Tile tile = GameData.Instance.map.TilesTable[num, num2];
			if (tile != null && o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("txt-tile annexed") + tile.GetLocalizedDisplayName();
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: VP ANNEXED.";
			}
		}
		else if (p_notification.Type == NotificationTypes.VP_RECEIVED)
		{
			string[] array3 = p_notification.Description.Split(':');
			int num3 = int.Parse(array3[0]);
			int num4 = int.Parse(array3[1]);
			Tile tile2 = GameData.Instance.map.TilesTable[num3, num4];
			if (tile2 != null && o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("txt-tile transfer received") + tile2.GetLocalizedDisplayName();
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: VP RECEIVED.";
			}
		}
		else if (p_notification.Type == NotificationTypes.VP_OWNERSHIP_LOSS)
		{
			string[] array4 = p_notification.Description.Split(':');
			int num5 = int.Parse(array4[0]);
			int num6 = int.Parse(array4[1]);
			Tile tile3 = GameData.Instance.map.TilesTable[num5, num6];
			if (tile3 != null && o_player != null)
			{
				notificationButtonGO.description_text.text = LocalizationManager.GetLocalizedCountry(o_player.Country) + LocalizationManager.Translate("txt-tile transfer lost ownership") + tile3.GetLocalizedDisplayName();
			}
			else
			{
				notificationButtonGO.description_text.text = "ERROR: VP OWNERSHIP LOSS.";
			}
		}
		else if (p_notification.Type == NotificationTypes.VP_LOST)
		{
			string[] array5 = p_notification.Description.Split(':');
			int num7 = int.Parse(array5[0]);
			int num8 = int.Parse(array5[1]);
			Tile tile4 = GameData.Instance.map.TilesTable[num7, num8];
			string text = "";
			if (string.IsNullOrEmpty(tile4.CityName))
			{
				if (tile4.isNavalConvoyRoute)
				{
					text = LocalizationManager.Translate("Naval Convoy Route");
				}
				else
				{
					text = ((!(tile4.Type == "City")) ? LocalizationManager.Translate(tile4.Type) : LocalizationManager.Translate(tile4.Name.Replace(" destroyed", string.Empty)));
				}
			}
			else
			{
				text = tile4.CityName;
			}
			notificationButtonGO.description_text.text = text + LocalizationManager.Translate("notif-lost vp");
		}
		else
		{
			notificationButtonGO.description_text.text = p_notification.Description;
		}
	}
}
