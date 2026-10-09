using UnityEngine;

public class LobbyChatMenu : MonoBehaviour, IMenu
{
	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		MultiplayerChat.instance.notification.SetActive(value: false);
	}
}
