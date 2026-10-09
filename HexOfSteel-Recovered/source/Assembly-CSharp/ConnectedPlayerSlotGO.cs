using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConnectedPlayerSlotGO : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Image status_image;

	public TextMeshProUGUI nickname_text;

	[HideInInspector]
	public GameObject addFriend_gameobject;

	public void OnPointerClick(PointerEventData eventData)
	{
		addFriend_gameobject.GetComponent<Button>().onClick.RemoveAllListeners();
		if (PlayerSettings.Instance.Friends.Contains(nickname_text.text))
		{
			addFriend_gameobject.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Translate("btn-remove friend");
			addFriend_gameobject.GetComponent<Button>().onClick.AddListener(RemoveFriend);
		}
		else
		{
			addFriend_gameobject.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Translate("btn-add friend");
			addFriend_gameobject.GetComponent<Button>().onClick.AddListener(AddFriend);
		}
		addFriend_gameobject.SetActive(value: true);
		addFriend_gameobject.transform.position = new Vector3(Input.mousePosition.x + addFriend_gameobject.GetComponent<RectTransform>().rect.width / 1.5f, Input.mousePosition.y);
	}

	private void LateUpdate()
	{
		if (addFriend_gameobject.activeSelf && Vector2.Distance(addFriend_gameobject.transform.position, Input.mousePosition) > 160f)
		{
			addFriend_gameobject.SetActive(value: false);
		}
	}

	public void AddFriend()
	{
		PlayerSettings.Instance.Friends.Add(nickname_text.text);
		MultiplayerChat.instance.chatClient.AddFriends(new string[1] { nickname_text.text });
		MultiplayerChat.instance.RefreshConnectedUsers();
		addFriend_gameobject.SetActive(value: false);
	}

	public void RemoveFriend()
	{
		PlayerSettings.Instance.Friends.Remove(nickname_text.text);
		MultiplayerChat.instance.chatClient.RemoveFriends(new string[1] { nickname_text.text });
		MultiplayerChat.instance.RefreshConnectedUsers();
		addFriend_gameobject.SetActive(value: false);
	}
}
