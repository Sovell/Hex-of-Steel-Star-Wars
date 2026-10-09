using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotificationButtonGO : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[Header("Setup")]
	public GameObject highlightRingPrefab;

	public TextMeshProUGUI description_text;

	private GameObject _ringInstance_object;

	private Notification _notification;

	public void Setup(Notification p_notification)
	{
		_notification = p_notification;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			SoundManager.instance.PlayUIClickSound();
			if (_notification.PosX != 0f || _notification.PosY != 0f)
			{
				NotificationManager.instance.CenterCameraOnMap(new Vector3(_notification.PosX, _notification.PosY, -80f));
				StartCoroutine(CR_HighlighTile(_notification.PosX, _notification.PosY));
			}
			Player o_player;
			if (_notification.Type == NotificationTypes.RESEARCH_COMPLETE)
			{
				UIManager.instance.techTree_panel.GetComponent<ResearchMenu>().ManageTechTreePanel();
				UIManager.instance.techTree_panel.GetComponent<ResearchMenu>().SelectFilter(_notification.UnitResearchedFilterType);
			}
			else if (_notification.Type == NotificationTypes.ALLIANCE_INVITATION && GameData.Instance.TryFindPlayerByName(_notification.SenderName, out o_player))
			{
				UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation join alliance") + LocalizationManager.GetLocalizedFaction(o_player.Faction), ConfirmJoinFaction);
			}
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			DestroyNotification();
		}
	}

	private void ConfirmJoinFaction()
	{
		if (GameData.Instance.TryFindPlayerByName(_notification.SenderName, out var o_player))
		{
			Object.Destroy(gameObject);
			if (TurnManager.currPlayer.Faction == o_player.Faction)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-already in alliance"));
				return;
			}
			TurnManager.currPlayer.ChangeFaction(o_player.Faction, p_getNewAllianceWars: true, p_sendRPC: true);
			new Notification(NotificationTypes.ALLIANCE_PROPOSITION_ACCEPTED, _notification.SenderName, 0f, 0f, TurnManager.currPlayer.Country + " accepted your alliance proposition.", p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
			TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		}
	}

	public void DestroyNotification()
	{
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Option select store"));
		UIManager.instance.HideTooltip();
		if (_notification.Type == NotificationTypes.ALLIANCE_INVITATION)
		{
			new Notification(NotificationTypes.ALLIANCE_PROPOSITION_REJECTED, _notification.SenderName, 0f, 0f, TurnManager.currPlayer.Country + " rejected your alliance proposition.", p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
		}
		if (_ringInstance_object != null)
		{
			Object.Destroy(_ringInstance_object);
		}
		Object.Destroy(gameObject);
	}

	private IEnumerator CR_HighlighTile(float posX, float posY)
	{
		yield return new WaitForSeconds(0.3f);
		if (!(_ringInstance_object != null))
		{
			_ringInstance_object = Object.Instantiate(highlightRingPrefab);
			_ringInstance_object.transform.position = new Vector3(posX, posY, -0.48f);
			StartCoroutine(CR_ChangeRingOpacity());
		}
	}

	private IEnumerator CR_ChangeRingOpacity()
	{
		if (!(_ringInstance_object == null))
		{
			for (float t = 0f; t < 0.5f; t += Time.deltaTime)
			{
				float t2 = t / 0.5f;
				_ringInstance_object.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
				yield return null;
			}
			_ringInstance_object.GetComponent<SpriteRenderer>().color = Color.white;
			for (float t = 0f; t < 0.5f; t += Time.deltaTime)
			{
				float t3 = t / 0.5f;
				_ringInstance_object.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t3);
				yield return null;
			}
			_ringInstance_object.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
			Object.Destroy(_ringInstance_object);
		}
	}
}
