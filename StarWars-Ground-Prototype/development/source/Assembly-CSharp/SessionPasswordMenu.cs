using System;
using System.Threading;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class SessionPasswordMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private JoinGameMenu _joinGameMenu;

	[Space]
	[SerializeField]
	private TMP_InputField _TMP_InputField;

	private bool _isVerifying;

	private CancellationTokenSource _cts;

	private Guid _currentSessionId;

	private Action<Guid, string, ServerDTOs.GameSessionDetails> _onVerifiedSession;

	private RoomInfo _currentRoomInfo;

	private Action<RoomInfo, string> _onVerifiedRoom;

	private void OnEnable()
	{
		_isVerifying = false;
		_TMP_InputField.SetTextWithoutNotify(string.Empty);
		_cts = new CancellationTokenSource();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void OpenForRoom(RoomInfo p_roomInfo, Action<RoomInfo, string> p_onVerified)
	{
		_currentRoomInfo = p_roomInfo;
		_onVerifiedRoom = p_onVerified;
		_onVerifiedSession = null;
		_currentSessionId = Guid.Empty;
		Open();
	}

	public void OpenForSession(Guid p_sessionId, Action<Guid, string, ServerDTOs.GameSessionDetails> p_onVerified)
	{
		_currentSessionId = p_sessionId;
		_onVerifiedSession = p_onVerified;
		_currentRoomInfo = null;
		_onVerifiedRoom = null;
		Open();
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void OnClick_CloseMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		Close();
	}

	public async void OnClick_VerifySessionPassword()
	{
		if (_isVerifying)
		{
			return;
		}
		SoundManager.instance.PlayUIClickSound();
		string password = _TMP_InputField.text?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(password))
		{
			Debug.LogWarning("Password is empty.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-password required"));
			return;
		}
		if (_currentRoomInfo != null)
		{
			Hashtable customProperties = _currentRoomInfo.CustomProperties;
			if (customProperties.ContainsKey("Pwd") && (string)customProperties["Pwd"] != password)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password"));
				return;
			}
			_onVerifiedRoom?.Invoke(_currentRoomInfo, password);
		}
		else
		{
			_isVerifying = true;
			ServerDTOs.GameSessionDetails gameSessionDetails = await Server.Instance.GameService.GetGameSessionDetails(_currentSessionId, password, _cts.Token);
			_isVerifying = false;
			if (gameSessionDetails == null)
			{
				return;
			}
			_onVerifiedSession?.Invoke(_currentSessionId, password, gameSessionDetails);
		}
		Close();
	}

	private void OnDisable()
	{
		_cts?.Cancel();
		_onVerifiedSession = null;
		_currentSessionId = Guid.Empty;
		_onVerifiedRoom = null;
		_currentRoomInfo = null;
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
