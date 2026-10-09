using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeMultiplayerMenu : MonoBehaviourPunCallbacks, IMenu
{
	public const byte MIN_PLAYERS = 2;

	public const byte MAX_PLAYERS = 16;

	public const string ID = "ID";

	public const string NAME = "Nm";

	public const string IS_READY = "Rdy";

	public const string HOST = "Hst";

	public const string SCENARIO = "Scn";

	public const string PLAYERS = "Plrs";

	public const string COUNTRIES = "Ctrys";

	public const string AIS = "AIs";

	public const string FIRST = "Frst";

	public const string DESCRIPTION = "Desc";

	public const string PASSWORD = "Pwd";

	public const string MODS = "Mds";

	public const string GAME_SETTINGS = "Sttgs";

	[Header("Password")]
	[SerializeField]
	private SessionPasswordMenu _sessionPasswordMenu;

	[Header("Headers")]
	[SerializeField]
	private GameObject _header_rooms;

	[SerializeField]
	private GameObject _header_players;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI _players_text;

	[Header("Prefabs")]
	[SerializeField]
	private RealtimeRoomSlot _roomSlot_prefab;

	[SerializeField]
	private RealtimePlayerSlot _playerSlot_prefab;

	[Header("Buttons")]
	[SerializeField]
	private Button _join_button;

	[SerializeField]
	private Button _leaveRoom_button;

	[SerializeField]
	private Button _start_button;

	[Header("Other")]
	[SerializeField]
	private Transform _container;

	[SerializeField]
	private GameObject _loadingIndicator;

	[SerializeField]
	private TMP_Dropdown _serverRegion_dropdown;

	private bool _changingRegion;

	private byte[] _rxBuffer;

	private int _rxTotalBytes;

	private int _rxTotalChunks;

	private int _rxReceivedChunks;

	private long _rxReceivedBytes;

	private readonly Dictionary<string, RoomInfo> _cachedRooms = new Dictionary<string, RoomInfo>();

	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		FreeDownloadBuffer();
		StartCoroutine(CR_RefreshPlayerCount());
		_header_rooms.SetActive(value: true);
		_header_players.SetActive(value: false);
		_join_button.gameObject.SetActive(value: false);
		_start_button.gameObject.SetActive(value: false);
		_leaveRoom_button.gameObject.SetActive(value: false);
		_serverRegion_dropdown.gameObject.SetActive(value: true);
		RefreshRoomsUI();
		ConnectToRealtimeServices();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("[RealtimeMenu] Connected to master.");
		PhotonNetwork.JoinLobby();
		if ((PlayerSettings.Instance.Language == GameLanguages.CNS || PlayerSettings.Instance.Language == GameLanguages.CNT) && (PlayerSettings.Instance.DefaultServerRegion == "ASIA" || PlayerSettings.Instance.DefaultServerRegion == "HK" || PlayerSettings.Instance.DefaultServerRegion == "JP" || PlayerSettings.Instance.DefaultServerRegion == "KR"))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("warning-no server in china"));
		}
	}

	public override void OnRoomListUpdate(List<RoomInfo> p_rooms)
	{
		UpdateGamesCache(p_rooms);
		RefreshRoomsUI();
	}

	public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player p_targetPlayer, ExitGames.Client.Photon.Hashtable p_changedProps)
	{
		RefreshPlayersSlots();
		if (PhotonNetwork.IsMasterClient)
		{
			_start_button.gameObject.SetActive(CheckForStartButton());
		}
	}

	public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		RefreshPlayersSlots();
	}

	public override void OnMasterClientSwitched(Photon.Realtime.Player p_player)
	{
		UIManager.ShowMessage(LocalizationManager.Translate("info-host left game"));
		LeaveRoom();
	}

	public override void OnJoinRoomFailed(short p_returnCode, string p_message)
	{
		UIManager.ShowMessage(p_message);
		_loadingIndicator.SetActive(value: false);
	}

	public override void OnCreateRoomFailed(short p_returnCode, string p_message)
	{
		Debug.LogError("[RealtimeMenu] Failed to create room.");
		_loadingIndicator.SetActive(value: false);
		UIManager.ShowMessage(LocalizationManager.Translate("error-unexpected"));
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("[RealtimeMenu] Joined the room.");
		FreeDownloadBuffer();
		ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable
		{
			{
				"ID",
				PlayerSettings.Instance.UserId.ToString()
			},
			{
				"Nm",
				string.Empty
			},
			{ "Rdy", false }
		};
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
		MultiplayerChat.instance.LeaveChannel(PlayerSettings.Instance.ChatChannel);
		MultiplayerChat.instance.JoinChannel(PhotonNetwork.CurrentRoom.Name);
		RefreshPlayersSlots();
		_leaveRoom_button.gameObject.SetActive(value: true);
		_header_rooms.SetActive(value: false);
		_header_players.SetActive(value: true);
		_loadingIndicator.SetActive(value: false);
		_serverRegion_dropdown.gameObject.SetActive(value: false);
		PhotonNetwork.KeepAliveInBackground = 300f;
	}

	public override void OnLeftRoom()
	{
		Debug.Log("[RealtimeMenu] Left the room.");
		_leaveRoom_button.gameObject.SetActive(value: false);
		_header_rooms.SetActive(value: true);
		_header_players.SetActive(value: false);
		_loadingIndicator.SetActive(value: false);
		PhotonNetwork.RemovePlayerCustomProperties(new string[3] { "ID", "Nm", "Rdy" });
		MultiplayerChat.instance.JoinChannel(PlayerSettings.Instance.ChatChannel);
		ClearAllSlots();
		RefreshRoomsUI();
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player p_photonPlayer)
	{
		Debug.Log("[RealtimeMenu] Player entered the room: " + p_photonPlayer.NickName);
		if (PhotonNetwork.IsMasterClient)
		{
			_start_button.gameObject.SetActive(CheckForStartButton());
		}
		RefreshPlayersSlots();
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player p_photonPlayer)
	{
		Debug.Log("[RealtimeMenu] Player left the room: " + p_photonPlayer.NickName);
		if (PhotonNetwork.IsMasterClient)
		{
			_start_button.gameObject.SetActive(CheckForStartButton());
		}
		RefreshPlayersSlots();
	}

	public override void OnDisconnected(DisconnectCause p_cause)
	{
		Debug.LogWarning($"[RealtimeMenu] Disconnected from Photon with reason: {p_cause}");
		MainMenu.instance.loading_Panel.SetActive(value: false);
		ClearAllSlots();
		if (p_cause == DisconnectCause.DisconnectByClientLogic)
		{
			if (_changingRegion)
			{
				_changingRegion = false;
				PhotonNetwork.ConnectToRegion(_serverRegion_dropdown.options[_serverRegion_dropdown.value].text);
			}
		}
		else
		{
			ConnectToRealtimeServices();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		FreeDownloadBuffer();
		ClearAllSlots();
		StopAllCoroutines();
		_serverRegion_dropdown.gameObject.SetActive(value: false);
	}

	[PunRPC]
	private void RPC_KickPlayer()
	{
		LeaveRoom();
		UIManager.ShowMessage(LocalizationManager.Translate("info-kicked out"));
	}

	[PunRPC]
	private void RPC_ReceiveGameStateChunk(byte[] p_chunk, int p_chunkIndex, int p_totalChunks, int p_totalBytes)
	{
		MainMenu.instance.loading_Panel.SetActive(value: true);
		if (_rxBuffer == null || _rxTotalBytes != p_totalBytes || _rxTotalChunks != p_totalChunks)
		{
			_rxReceivedBytes = 0L;
			_rxReceivedChunks = 0;
			_rxTotalBytes = p_totalBytes;
			_rxTotalChunks = p_totalChunks;
			_rxBuffer = new byte[p_totalBytes];
		}
		int dstOffset = p_chunkIndex * 16384;
		Buffer.BlockCopy(p_chunk, 0, _rxBuffer, dstOffset, p_chunk.Length);
		_rxReceivedChunks++;
		_rxReceivedBytes += p_chunk.Length;
		UIManager.ShowMessage(LocalizationManager.Translate("txt-receiving data") + "\n" + Utils.FormatReceivedTotal(_rxReceivedBytes, _rxTotalBytes), 1f);
		if (_rxReceivedChunks < _rxTotalChunks)
		{
			return;
		}
		try
		{
			GameData gameData = (GameData)Utils.ConvertByteArrayToObject(_rxBuffer);
			GameData.Instance.SetData(gameData);
			if (gameData.map.timePlayed > 0f)
			{
				MainMenu.isLoadedGame = true;
			}
		}
		catch (Exception ex)
		{
			MainMenu.instance.loading_Panel.SetActive(value: false);
			UIManager.ShowMessage(ex.Message);
		}
		finally
		{
			FreeDownloadBuffer();
		}
	}

	public void ConnectToRegion()
	{
		_changingRegion = true;
		PhotonNetwork.Disconnect();
		MultiplayerChat.instance.Disconnect();
		PlayerSettings.Instance.DefaultServerRegion = _serverRegion_dropdown.options[_serverRegion_dropdown.value].text;
		Debug.Log("[RealtimeMenu] Changing server region to: " + PlayerSettings.Instance.DefaultServerRegion);
		MultiplayerChat.instance.ConnectToServer();
	}

	private void ConnectToRealtimeServices()
	{
		if (string.IsNullOrWhiteSpace(PlayerSettings.Instance.Username))
		{
			Debug.LogWarning("[RealtimeMenu] Cannot connect to multiplayer, invalid username.");
		}
		else if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
		{
			PhotonNetwork.NickName = PlayerSettings.Instance.Username;
			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.JoinLobby();
				return;
			}
			PhotonNetwork.GameVersion = "8.4.14";
			PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "8.4.14";
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = PlayerSettings.Instance.DefaultServerRegion;
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public async Task CreateRoom(GameData p_gameData, string p_description, string p_password)
	{
		_loadingIndicator.SetActive(value: true);
		p_gameData.isPBEM = false;
		p_gameData.map.IsOfficial = false;
		p_gameData.map.IsCampaign = false;
		p_gameData.map.NextMap = null;
		p_gameData.map.NextMap2 = null;
		p_gameData.map.NextMapDefeat = null;
		p_gameData.map.NextMap2Defeat = null;
		p_gameData.ironManMode = false;
		p_gameData.messages = new List<string>();
		p_gameData.GenerateGameSettings(p_lockSaveFile: true);
		p_gameData.listOfPlayers.Count((Player p) => !p.IsComputer);
		RoomOptions roomOptions = new RoomOptions
		{
			IsOpen = true,
			IsVisible = true,
			CleanupCacheOnLeave = false,
			MaxPlayers = (byte)Mathf.Clamp(p_gameData.listOfPlayers.Count, 2, 16)
		};
		string value = JsonConvert.SerializeObject(p_gameData.gameSettings);
		roomOptions.CustomRoomPropertiesForLobby = new string[6] { "Hst", "Scn", "Desc", "Pwd", "Mds", "Sttgs" };
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
		{
			{
				"Hst",
				PlayerSettings.Instance.Username
			},
			{ "Desc", p_description },
			{ "Pwd", p_password },
			{
				"Mds",
				string.Join("\n", from m in ModsManager.activeMods
					where m.HasGameplayImpact()
					select m.Name)
			},
			{
				"Scn",
				p_gameData.map.ScenarioName
			},
			{
				"Plrs",
				p_gameData.listOfPlayers.Select((Player p) => p.Name).ToArray()
			},
			{
				"Ctrys",
				p_gameData.listOfPlayers.Select((Player p) => p.Country).ToArray()
			},
			{
				"AIs",
				p_gameData.listOfPlayers.Select((Player p) => p.IsComputer).ToArray()
			},
			{
				"Frst",
				p_gameData.listOfPlayers.FirstOrDefault((Player p) => p.IsPlayingFirst).Name
			},
			{ "Sttgs", value }
		};
		while (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
		{
			await Task.Yield();
		}
		if (PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName + UnityEngine.Random.Range(0, 999999), roomOptions))
		{
			_start_button.onClick.RemoveAllListeners();
			_start_button.onClick.AddListener(() =>
			{
				OnClick_StartGame(p_gameData);
			});
			UIManager.HideToolTip();
		}
	}

	public void LeaveRoom()
	{
		if (PhotonNetwork.InRoom)
		{
			SoundManager.instance.PlayUIClickSound();
			if (PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;
				PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
				PhotonNetwork.CurrentRoom.PlayerTtl = 0;
			}
			MultiplayerChat.instance.LeaveChannel(PhotonNetwork.CurrentRoom.Name);
			PhotonNetwork.LeaveRoom(becomeInactive: false);
		}
	}

	private void OnClick_StartGame(GameData p_gameData)
	{
		StartCoroutine(CR_StartGameCoroutine(p_gameData));
	}

	private void FreeDownloadBuffer()
	{
		_rxBuffer = null;
		_rxTotalBytes = 0;
		_rxTotalChunks = 0;
		_rxReceivedBytes = 0L;
		_rxReceivedChunks = 0;
	}

	private void UpdateGamesCache(List<RoomInfo> p_rooms)
	{
		for (int i = 0; i < p_rooms.Count; i++)
		{
			if (p_rooms[i].RemovedFromList)
			{
				_cachedRooms.Remove(p_rooms[i].Name);
			}
			else
			{
				_cachedRooms[p_rooms[i].Name] = p_rooms[i];
			}
		}
	}

	private void RefreshPlayersSlots()
	{
		ClearAllSlots();
		if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.CustomProperties == null || !PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Plrs", out var value))
		{
			return;
		}
		string[] array = (string[])value;
		if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Ctrys", out var value2))
		{
			return;
		}
		string[] array2 = (string[])value2;
		if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AIs", out var value3) || !PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Frst", out var value4))
		{
			return;
		}
		string text = (string)value4;
		bool[] array3 = (bool[])value3;
		Player[] players = new Player[array.Length];
		for (int i = 0; i < players.Length; i++)
		{
			Player player = new Player(array[i]);
			player.IsPlayingFirst = array[i] == text;
			player.IsComputer = array3[i];
			player.Country = array2[i];
			players[i] = player;
		}
		foreach (KeyValuePair<int, Photon.Realtime.Player> player2 in PhotonNetwork.CurrentRoom.Players)
		{
			if (player2.Value.CustomProperties.TryGetValue("Nm", out var value5) && string.IsNullOrWhiteSpace((string)value5))
			{
				UnityEngine.Object.Instantiate(_playerSlot_prefab, _container).Setup(null, player2.Value, OnClick_KickPlayer);
			}
		}
		int j;
		for (j = 0; j < players.Length; j++)
		{
			UnityEngine.Object.Instantiate(_playerSlot_prefab, _container).Setup(p_photonPlayer: PhotonNetwork.PlayerList.FirstOrDefault((Photon.Realtime.Player p) => p.CustomProperties["Nm"] as string == players[j].Name), p_player: players[j], p_onKickButtonClicked: OnClick_KickPlayer);
		}
	}

	private void OnClick_KickPlayer(Photon.Realtime.Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		photonView.RPC("RPC_KickPlayer", p_player);
	}

	private void RefreshRoomsUI()
	{
		if (PhotonNetwork.InRoom)
		{
			return;
		}
		ClearAllSlots();
		foreach (KeyValuePair<string, RoomInfo> cachedRoom in _cachedRooms)
		{
			if (!cachedRoom.Value.RemovedFromList)
			{
				UnityEngine.Object.Instantiate(_roomSlot_prefab, _container).Setup(cachedRoom.Value, OnClick_JoinRoom);
			}
		}
	}

	private void OnClick_JoinRoom(RoomInfo p_roomInfo)
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.HideToolTip();
		if (p_roomInfo == null)
		{
			Debug.Log("[RealtimeMenu] The room you want to join is null.");
		}
		else if (PhotonNetwork.NetworkClientState == ClientState.Joining)
		{
			Debug.Log("[RealtimeMenu] Already joining a room, please wait.");
		}
		else if (p_roomInfo.CustomProperties.ContainsKey("Pwd") && !string.IsNullOrWhiteSpace((string)p_roomInfo.CustomProperties["Pwd"]))
		{
			Debug.Log("[RealtimeMenu] The room you want to join is password protected.");
			_sessionPasswordMenu.OpenForRoom(p_roomInfo, OnJoinRoomWithPassword);
		}
		else
		{
			PhotonNetwork.JoinRoom(p_roomInfo.Name);
		}
	}

	private void OnJoinRoomWithPassword(RoomInfo p_roomInfo, string p_password)
	{
		if (p_roomInfo == null || !p_roomInfo.IsOpen || !p_roomInfo.IsVisible)
		{
			Debug.Log("[RealtimeMenu] The room you want to join is null, closed, or invisible.");
			_sessionPasswordMenu.Close();
			return;
		}
		if (p_roomInfo.CustomProperties.TryGetValue("Pwd", out var value))
		{
			string text = (string)value;
			if (!string.IsNullOrWhiteSpace(text) && text != p_password)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password"));
				return;
			}
		}
		PhotonNetwork.JoinRoom(p_roomInfo.Name);
	}

	private void ClearAllSlots()
	{
		Transform[] componentsInChildren = _container.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != _container)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private bool CheckForStartButton()
	{
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			return false;
		}
		if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
		{
			return false;
		}
		Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].CustomProperties.TryGetValue("Rdy", out var value))
			{
				return false;
			}
			if (!(bool)value)
			{
				return false;
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].CustomProperties.TryGetValue("Nm", out var value2))
			{
				if (string.IsNullOrWhiteSpace((string)value2))
				{
					return false;
				}
				if (hashSet.Contains((string)value2))
				{
					return false;
				}
				hashSet.Add((string)value2);
				continue;
			}
			return false;
		}
		return true;
	}

	private IEnumerator CR_RefreshPlayerCount()
	{
		_players_text.text = string.Empty;
		while (true)
		{
			yield return new WaitForSeconds(1f);
			if (PhotonNetwork.IsConnected)
			{
				_players_text.text = LocalizationManager.Translate("txt-players in lobby") + (PhotonNetwork.CountOfPlayersOnMaster - 1);
			}
			else
			{
				_players_text.text = LocalizationManager.Translate("error-cannot reach server");
			}
		}
	}

	private IEnumerator CR_StartGameCoroutine(GameData p_gameData)
	{
		MainMenu.instance.loading_Panel.SetActive(value: true);
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
		PhotonNetwork.CurrentRoom.PlayerTtl = 300000;
		MainMenu.isLoadedGame = p_gameData.map.timePlayed > 0f;
		Photon.Realtime.Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Photon.Realtime.Player player in playerList)
		{
			if (p_gameData.TryFindPlayerByName((string)player.CustomProperties["Nm"], out var o_player))
			{
				Guid value = Guid.Parse((string)player.CustomProperties["ID"]);
				o_player.UserId = value;
				o_player.IsComputer = false;
				o_player.LocalPassword = null;
			}
		}
		foreach (Player listOfPlayer in p_gameData.listOfPlayers)
		{
			if (listOfPlayer.Name == PhotonNetwork.CurrentRoom.CustomProperties["Frst"] as string)
			{
				listOfPlayer.IsPlayingFirst = true;
			}
			else
			{
				listOfPlayer.IsPlayingFirst = false;
			}
			if (listOfPlayer.IsComputer)
			{
				listOfPlayer.UserId = null;
				listOfPlayer.LocalPassword = null;
			}
		}
		yield return null;
		int sentBytes = 0;
		byte[] bytesData = Utils.ConvertObjectToByteArray(p_gameData);
		int totalBytes = bytesData.Length;
		int totalChunks = (totalBytes + 16384 - 1) / 16384;
		for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
		{
			float timeWaited = 0f;
			while (PhotonNetwork.NetworkingClient.LoadBalancingPeer.QueuedOutgoingCommands > 8)
			{
				if (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Joined)
				{
					Debug.LogError("Connection lost while sending chunks");
					yield break;
				}
				yield return new WaitForSeconds(0.1f);
				timeWaited += 0.1f;
				if (timeWaited > 10f)
				{
					Debug.LogError("Timeout while sending chunks");
					yield break;
				}
			}
			int num = chunkIndex * 16384;
			int num2 = Mathf.Min(16384, totalBytes - num);
			byte[] array = new byte[num2];
			sentBytes += array.Length;
			Buffer.BlockCopy(bytesData, num, array, 0, num2);
			photonView.RPC("RPC_ReceiveGameStateChunk", RpcTarget.Others, array, chunkIndex, totalChunks, totalBytes);
			PhotonNetwork.SendAllOutgoingCommands();
			UIManager.ShowMessage(LocalizationManager.Translate("txt-sending data") + "\n" + Utils.FormatReceivedTotal(sentBytes, totalBytes), 1f);
			yield return null;
		}
		PhotonNetwork.LoadLevel(1);
	}
}
