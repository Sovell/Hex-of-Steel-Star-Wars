using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MultiplayerChat : MonoBehaviour, IChatClientListener
{
	[Serializable]
	private class ChatBanList
	{
		public ChatBanEntry[] bans;
	}

	[Serializable]
	private class ChatBanEntry
	{
		public string hardwareID;

		public string userID;

		public string reason;
	}

	public static MultiplayerChat instance;

	private const int HISTORY_SIZE = 50;

	public static bool isTyping;

	public ChatClient chatClient;

	[Header("Inputfield")]
	[SerializeField]
	private TMP_InputField message_inputfield;

	[SerializeField]
	private TMP_Dropdown channel_dropdown;

	[Header("Prefabs")]
	[SerializeField]
	private TextMeshProUGUI message_prefab;

	[SerializeField]
	private ConnectedPlayerSlotGO playerOnline_prefab;

	[Header("Transform")]
	[SerializeField]
	private Transform messages_parent;

	[SerializeField]
	private Transform connectedUsers_parent;

	[Header("Rect transform")]
	[SerializeField]
	private RectTransform message_rectTransform;

	[Header("Gameobjects")]
	public GameObject notification;

	public GameObject friendManagement_gameobject;

	[Header("Colors")]
	[SerializeField]
	private Color _mention_color;

	[SerializeField]
	private Color _admin_color;

	[SerializeField]
	private Color _localClient_color;

	[SerializeField]
	private Color _regular_color;

	protected static List<string> _admins = new List<string> { "acorsicanfrog" };

	protected HashSet<string> _bannedIDs = new HashSet<string>();

	protected HashSet<string> _bannedUserIDs = new HashSet<string>();

	private bool _isReconnecting;

	private bool _isFetchingBanList;

	private void Awake()
	{
		instance = this;
		EnsureChatClient();
	}

	private void OnEnable()
	{
		ConnectToServer();
		RefreshBanList();
		notification.SetActive(value: false);
		friendManagement_gameobject.SetActive(value: false);
	}

	private void OnDisable()
	{
		chatClient?.Disconnect();
		_isReconnecting = false;
		ClearMessages();
		if (notification != null) notification.SetActive(value: false);
		if (friendManagement_gameobject != null) friendManagement_gameobject.SetActive(value: false);
		if (message_inputfield != null && string.IsNullOrWhiteSpace(message_inputfield.text))
		{
			message_inputfield.SetTextWithoutNotify(string.Empty);
		}
	}

	private void Update()
	{
		chatClient.Service();
		isTyping = EventSystem.current.currentSelectedGameObject == message_inputfield.gameObject;
		if (Input.GetKey(KeyCode.Escape))
		{
			friendManagement_gameobject.SetActive(value: false);
			if (string.IsNullOrWhiteSpace(message_inputfield.text))
			{
				message_inputfield.SetTextWithoutNotify(string.Empty);
			}
		}
		if (isTyping)
		{
			notification.SetActive(value: false);
			if (chatClient.State == ChatState.ConnectedToFrontEnd && Input.GetKeyDown(KeyCode.Return))
			{
				SendMessage_button();
			}
		}
	}

	public void ConnectToServer()
	{
		EnsureChatClient();
		if (GameData.Instance.isPBEM || (PlayerSettings.Instance.PublicChatOFF && !PhotonNetwork.InRoom))
		{
			return;
		}
		if (!Server.Instance.IsUserAuthenticated)
		{
			if (!(MainMenu.instance == null) || !(PlayerSettings.Instance.UserId != Guid.Empty))
			{
				return;
			}
			Server.Instance.AuthService.Login(PlayerSettings.Instance.Username, PlayerSettings.Instance.Password, CancellationToken.None).ContinueWith((Task<bool> t) =>
			{
				if (t.Result)
				{
					ConnectToServer();
				}
			});
		}
		else if (chatClient.State == ChatState.Uninitialized || chatClient.State == ChatState.Disconnected)
		{
			chatClient.ConnectAndSetStatus(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "8.4.14", new AuthenticationValues(PlayerSettings.Instance.Username));
		}
	}

	public void ChooseChannel(TMP_Dropdown p_TMP_Dropdown)
	{
		if (!GameData.Instance.isPBEM && Server.Instance.IsUserAuthenticated && !MultiplayerManager.IsRealtimeMultiplayer())
		{
			ClearMessages();
			LeaveChannel(PlayerSettings.Instance.ChatChannel);
			PlayerSettings.Instance.ChatChannel = p_TMP_Dropdown.options[p_TMP_Dropdown.value].text;
			SubscribeToChannel(PlayerSettings.Instance.ChatChannel);
		}
	}

	public void JoinChannel(string p_channel)
	{
		SubscribeToChannel(p_channel);
	}

	public void LeaveChannel(string p_channel)
	{
		chatClient.Unsubscribe(new string[1] { p_channel });
	}

	public void SendMessage_button()
	{
		SoundManager.instance.PlayUIClickSound();
		if (PhotonNetwork.IsConnected && chatClient.State == ChatState.Disconnected)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-chat reconnect"));
			Reconnect();
		}
		else
		{
			if (string.IsNullOrWhiteSpace(message_inputfield.text))
			{
				return;
			}
			if (IsLocalUserBanned())
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-banned from chat"));
				return;
			}
			string text = ((!MultiplayerManager.IsRealtimeMultiplayer()) ? PlayerSettings.Instance.ChatChannel : PhotonNetwork.CurrentRoom.Name);
			if (text == "English")
			{
				string text2 = message_inputfield.text;
				if (ContainsRussianCharacters(text2))
				{
					UIManager.ShowMessage("Пожалуйста, используйте «русский» канал для отправки сообщений на русском языке.");
					return;
				}
				if (ContainsKoreanCharacters(text2))
				{
					UIManager.ShowMessage("한국어로 메시지를 보내시려면 '한국어' 채널을 이용해주세요.");
					return;
				}
				if (ContainsChineseCharacters(text2))
				{
					UIManager.ShowMessage("请使用“中文”频道发送中文消息。");
					return;
				}
				if (ContainsJapaneseCharacters(text2))
				{
					UIManager.ShowMessage("日本語でメッセージを送信するには「日本語」チャンネルを使用してください。");
					return;
				}
			}
			chatClient.PublishMessage(text, JsonUtility.ToJson(new ChatMessageData(PlayerSettings.Instance.Username, message_inputfield.text)));
			chatClient.Service();
			message_inputfield.SetTextWithoutNotify(string.Empty);
			if (isTyping)
			{
				EventSystem.current.SetSelectedGameObject(null);
				EventSystem.current.SetSelectedGameObject(message_inputfield.gameObject);
			}
			notification.SetActive(value: false);
		}
	}

	public void RefreshConnectedUsers()
	{
		Transform[] componentsInChildren = connectedUsers_parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != connectedUsers_parent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		if (chatClient == null || PlayerSettings.Instance.PublicChatOFF)
		{
			return;
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		foreach (KeyValuePair<string, ChatChannel> publicChannel in chatClient.PublicChannels)
		{
			if (!MultiplayerManager.IsRealtimeMultiplayer() && publicChannel.Key != PlayerSettings.Instance.ChatChannel)
			{
				continue;
			}
			foreach (string subscriber in publicChannel.Value.Subscribers)
			{
				if (!(subscriber == PlayerSettings.Instance.Username))
				{
					if (_admins.Contains(subscriber))
					{
						list.Add(subscriber);
					}
					else if (PlayerSettings.Instance.Friends.Contains(subscriber))
					{
						list2.Add(subscriber);
					}
					else
					{
						list3.Add(subscriber);
					}
				}
			}
		}
		List<string> list4 = new List<string>();
		list4.AddRange(list.OrderBy((string x) => x));
		list4.AddRange(list2.OrderBy((string x) => x));
		list4.AddRange(list3.OrderBy((string x) => x));
		foreach (string item in list4)
		{
			ConnectedPlayerSlotGO connectedPlayerSlotGO = UnityEngine.Object.Instantiate(playerOnline_prefab, connectedUsers_parent);
			connectedPlayerSlotGO.name = item;
			connectedPlayerSlotGO.nickname_text.text = item;
			connectedPlayerSlotGO.addFriend_gameobject = friendManagement_gameobject;
			if (_admins.Contains(item))
			{
				connectedPlayerSlotGO.nickname_text.color = MainMenu.yellowColor;
			}
			if (PlayerSettings.Instance.Friends.Contains(item))
			{
				connectedPlayerSlotGO.status_image.color = new Color(0.14f, 0.58f, 0.76f);
			}
			else
			{
				connectedPlayerSlotGO.status_image.color = MainMenu.greenColor;
			}
		}
		foreach (string friend in PlayerSettings.Instance.Friends)
		{
			if (!list4.Contains(friend))
			{
				ConnectedPlayerSlotGO connectedPlayerSlotGO2 = UnityEngine.Object.Instantiate(playerOnline_prefab, connectedUsers_parent);
				connectedPlayerSlotGO2.name = friend;
				connectedPlayerSlotGO2.nickname_text.text = friend;
				connectedPlayerSlotGO2.addFriend_gameobject = friendManagement_gameobject;
				connectedPlayerSlotGO2.status_image.color = MainMenu.redColor;
			}
		}
	}

	public void Disconnect()
	{
		Debug.Log("[Chat] Disconnecting.");
		chatClient?.Disconnect();
	}

	public void Reconnect()
	{
		StartCoroutine(CR_Reconnect());
	}

	private void ClearMessages()
	{
		if (messages_parent == null)
		{
			return;
		}
		Transform[] componentsInChildren = messages_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != messages_parent)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private void EnsureChatClient()
	{
		if (chatClient == null)
		{
			chatClient = new ChatClient(this);
		}
	}

	private void SubscribeToChannel(string p_channel)
	{
		chatClient.Subscribe(p_channel, 0, 50, new ChannelCreationOptions
		{
			PublishSubscribers = true
		});
	}

	private void RefreshBanList()
	{
		if (PrototypeBootstrap.IsIsolatedProfile) return;
		if (!_isFetchingBanList)
		{
			StartCoroutine(CR_FetchBanList());
		}
	}

	private IEnumerator CR_FetchBanList()
	{
		_isFetchingBanList = true;
		using (UnityWebRequest request = UnityWebRequest.Get("https://hex-of-steel.fr/chat-bans.json"))
		{
			request.timeout = 15;
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success)
			{
				ApplyBanList(request.downloadHandler.text);
			}
			else
			{
				Debug.LogWarning("[Chat] Failed to fetch ban list: " + request.error);
			}
		}
		_isFetchingBanList = false;
	}

	private void ApplyBanList(string p_json)
	{
		try
		{
			ChatBanList chatBanList = JsonUtility.FromJson<ChatBanList>(p_json);
			if (chatBanList == null || chatBanList.bans == null)
			{
				Debug.LogWarning("[Chat] Ban list JSON is empty or malformed.");
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> hashSet2 = new HashSet<string>();
			ChatBanEntry[] bans = chatBanList.bans;
			foreach (ChatBanEntry chatBanEntry in bans)
			{
				if (!string.IsNullOrWhiteSpace(chatBanEntry.hardwareID))
				{
					hashSet.Add(chatBanEntry.hardwareID);
				}
				if (!string.IsNullOrWhiteSpace(chatBanEntry.userID))
				{
					hashSet2.Add(chatBanEntry.userID);
				}
			}
			_bannedIDs = hashSet;
			_bannedUserIDs = hashSet2;
			Debug.Log($"[Chat] Loaded {_bannedIDs.Count} banned hardware IDs and {_bannedUserIDs.Count} banned user IDs from the server ban list.");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[Chat] Failed to parse ban list JSON: " + ex.Message);
		}
	}

	private bool IsLocalUserBanned()
	{
		if (_bannedIDs.Contains(SystemInfo.deviceUniqueIdentifier))
		{
			return true;
		}
		return _bannedUserIDs.Contains(PlayerSettings.Instance.UserId.ToString());
	}

	private bool IsMessageBanned(ChatMessageData p_messageData)
	{
		if (!string.IsNullOrWhiteSpace(p_messageData.hardwareID) && _bannedIDs.Contains(p_messageData.hardwareID))
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(p_messageData.userID))
		{
			return _bannedUserIDs.Contains(p_messageData.userID);
		}
		return false;
	}

	private IEnumerator CR_Reconnect()
	{
		if (_isReconnecting || !Server.Instance.IsUserAuthenticated)
		{
			yield break;
		}
		_isReconnecting = true;
		while (chatClient.State == ChatState.Disconnected)
		{
			ConnectToServer();
			float timeout = 10f;
			while (timeout > 0f && chatClient.State != ChatState.ConnectedToFrontEnd && chatClient.State != ChatState.Disconnected)
			{
				timeout -= Time.unscaledDeltaTime;
				yield return null;
			}
			if (chatClient.State == ChatState.ConnectedToFrontEnd)
			{
				break;
			}
			yield return new WaitForSeconds(3f);
		}
		_isReconnecting = false;
	}

	public void OnChatStateChange(ChatState p_newState)
	{
		switch (p_newState)
		{
		case ChatState.ConnectedToFrontEnd:
			if (chatClient.PublicChannels.Keys.ToArray().Length == 0)
			{
				string chatChannel;
				if (MultiplayerManager.IsRealtimeMultiplayer())
				{
					LeaveChannel(PlayerSettings.Instance.ChatChannel);
					chatChannel = PhotonNetwork.CurrentRoom.Name;
				}
				else
				{
					chatChannel = PlayerSettings.Instance.ChatChannel;
				}
				SubscribeToChannel(chatChannel);
			}
			break;
		case ChatState.Disconnected:
			ClearMessages();
			RefreshConnectedUsers();
			break;
		}
	}

	public void OnConnected()
	{
		Debug.Log("[Chat] Connected.");
		_isReconnecting = false;
		ClearMessages();
		chatClient.AddFriends(PlayerSettings.Instance.Friends.ToArray());
	}

	public void OnGetMessages(string p_channelName, string[] p_senders, object[] p_messages)
	{
		if ((p_channelName == PlayerSettings.Instance.ChatChannel && PlayerSettings.Instance.PublicChatOFF) || (p_channelName != PlayerSettings.Instance.ChatChannel && !MultiplayerManager.IsRealtimeMultiplayer()))
		{
			return;
		}
		for (int i = 0; i < p_senders.Length; i++)
		{
			ChatMessageData chatMessageData = JsonUtility.FromJson<ChatMessageData>(p_messages[i].ToString());
			if (IsMessageBanned(chatMessageData))
			{
				continue;
			}
			TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(message_prefab, messages_parent);
			textMeshProUGUI.text = chatMessageData.GenerateMessageContentForUIDisplay();
			textMeshProUGUI.name = chatMessageData.hardwareID;
			string text = p_messages[i].ToString();
			if (_admins.Contains(p_senders[i]))
			{
				textMeshProUGUI.color = _admin_color;
			}
			else if (p_senders[i] == PlayerSettings.Instance.Username)
			{
				textMeshProUGUI.color = _localClient_color;
			}
			else if (text.Contains("@" + PlayerSettings.Instance.Username))
			{
				textMeshProUGUI.color = _mention_color;
			}
			if (p_senders[i] == PlayerSettings.Instance.Username)
			{
				notification.SetActive(value: false);
				continue;
			}
			notification.SetActive(value: true);
			if (!isTyping && text.Contains("@" + PlayerSettings.Instance.Username))
			{
				if (!PlayerSettings.Instance.ChatMentions.Contains(text))
				{
					PlayerSettings.Instance.CheckNeedRefreshChatMentions();
					UIManager.ShowMessage(p_senders[i].ToString() + LocalizationManager.Translate("info-chat mention"));
					notification.GetComponent<Image>().color = _mention_color;
					PlayerSettings.Instance.ChatMentions.Add(text);
				}
			}
			else if (_admins.Contains(p_senders[i]))
			{
				notification.GetComponent<Image>().color = MainMenu.yellowColor;
			}
			else if (PlayerSettings.Instance.Friends.Contains(p_senders[i]))
			{
				notification.GetComponent<Image>().color = new Color(0.14f, 0.58f, 0.76f);
			}
			else
			{
				notification.GetComponent<Image>().color = Color.red;
			}
		}
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(message_rectTransform);
	}

	public void OnStatusUpdate(string p_user, int p_status, bool p_gotMessage, object p_message)
	{
		if (p_status == 2)
		{
			UIManager.ShowMessage(p_user + LocalizationManager.Translate("info-friend online"));
		}
	}

	public void OnSubscribed(string[] p_channels, bool[] p_results)
	{
		Debug.Log("[Chat] Subscribed to: " + string.Join(",", p_channels));
		RefreshConnectedUsers();
	}

	public void OnUnsubscribed(string[] p_channels)
	{
		Debug.Log("[Chat] Unsubscribed from: " + string.Join(",", p_channels));
		RefreshConnectedUsers();
	}

	public void OnUserSubscribed(string p_channel, string p_user)
	{
		RefreshConnectedUsers();
	}

	public void OnUserUnsubscribed(string p_channel, string p_user)
	{
		RefreshConnectedUsers();
	}

	public void DebugReturn(DebugLevel p_level, string p_message)
	{
	}

	public void OnPrivateMessage(string p_sender, object p_message, string p_channelName)
	{
	}

	public void OnDisconnected()
	{
		Debug.Log("[Chat] Disconnected.");
		Reconnect();
	}

	public bool ContainsChineseCharacters(string p_text)
	{
		foreach (char c in p_text)
		{
			if (IsChineseCharacter(c))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsRussianCharacters(string p_text)
	{
		foreach (char c in p_text)
		{
			if (IsRussianCharacter(c))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsJapaneseCharacters(string p_text)
	{
		foreach (char c in p_text)
		{
			if (IsJapaneseCharacter(c))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsKoreanCharacters(string p_text)
	{
		foreach (char c in p_text)
		{
			if (IsKoreanCharacter(c))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsChineseCharacter(char c)
	{
		if (c >= '一')
		{
			return c <= '鿿';
		}
		return false;
	}

	private bool IsRussianCharacter(char c)
	{
		if (c >= 'А')
		{
			return c <= 'я';
		}
		return false;
	}

	private bool IsJapaneseCharacter(char c)
	{
		if (c < '一' || c > '鿿')
		{
			if (c >= '\u3040')
			{
				return c <= 'ヿ';
			}
			return false;
		}
		return true;
	}

	private bool IsKoreanCharacter(char c)
	{
		if (c >= '가')
		{
			return c <= '힣';
		}
		return false;
	}

	private void OnApplicationQuit()
	{
		chatClient?.Disconnect();
	}
}
