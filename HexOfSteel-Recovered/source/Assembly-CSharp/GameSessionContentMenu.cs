using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameSessionContentMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private MultiplayerGamesMenu _multiplayerGamesMenu;

	[SerializeField]
	private JoinGameMenu _joinGameMenu;

	[Space]
	[SerializeField]
	private Transform _slotsListContainer;

	[SerializeField]
	private RectTransform _entriesRectTransform;

	[SerializeField]
	private GameContentSlotUI _gameContentSlotPrefab;

	[Space]
	[SerializeField]
	private Button _deleteGameButton;

	private CancellationTokenSource _cts;

	private bool _isPerformingAction;

	private Guid _currentSessionId = Guid.Empty;

	private string _currentSessionPassword = string.Empty;

	private ServerDTOs.GameSessionDetails _currentGameSessionDetails;

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public async void OpenWithSessionId(Guid p_gameSessionId, string p_password, ServerDTOs.GameSessionDetails p_gameSessionDetails)
	{
		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		_currentSessionId = p_gameSessionId;
		_currentSessionPassword = p_password;
		_currentGameSessionDetails = p_gameSessionDetails;
		if (_currentGameSessionDetails == null)
		{
			_currentGameSessionDetails = await Server.Instance.GameService.GetGameSessionDetails(p_gameSessionId, p_password, _cts.Token);
		}
		if (_currentGameSessionDetails == null || _currentGameSessionDetails.State != GameSessionStates.OPEN)
		{
			Close();
			return;
		}
		Open();
		_deleteGameButton.gameObject.SetActive(_currentGameSessionDetails.IsHost);
		_entriesRectTransform.offsetMin = new Vector2(_entriesRectTransform.offsetMin.x, _currentGameSessionDetails.IsHost ? 65f : 10f);
		SetupSlots(_currentGameSessionDetails);
	}

	private void SetupSlots(ServerDTOs.GameSessionDetails p_gameSessionDetails)
	{
		ClearSlots();
		ServerDTOs.PlayerSlotItem[] playerSlots = p_gameSessionDetails.PlayerSlots;
		foreach (ServerDTOs.PlayerSlotItem p_slot in playerSlots)
		{
			UnityEngine.Object.Instantiate(_gameContentSlotPrefab, _slotsListContainer).Setup(p_gameSessionDetails, p_slot, OnClick_JoinSlot, OnClick_LeaveSlot, OnClick_KickPlayer);
		}
	}

	public void OnClick_CloseMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		Close();
	}

	public void OnClick_DeleteGame()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirm game deletion"), DeleteGameAsync);
	}

	private async void DeleteGameAsync()
	{
		if (!_isPerformingAction)
		{
			_isPerformingAction = true;
			if (await Server.Instance.GameService.DeleteGame(_currentSessionId, _cts.Token))
			{
				Close();
				_joinGameMenu.RefreshGamesListing();
			}
			_isPerformingAction = false;
		}
	}

	private void OnClick_JoinSlot(ServerDTOs.PlayerSlotItem slot)
	{
		SoundManager.instance.PlayUIClickSound();
		if (!HasRequiredMods(_currentGameSessionDetails))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing required mods") + string.Join(", ", _currentGameSessionDetails.Mods));
		}
		else
		{
			JoinSlotAsync(slot);
		}
	}

	private async void JoinSlotAsync(ServerDTOs.PlayerSlotItem p_slot)
	{
		if (_isPerformingAction)
		{
			return;
		}
		_isPerformingAction = true;
		bool isGameAboutToStart = false;
		if (await Server.Instance.GameService.JoinGame(_currentSessionId, p_slot.index, _currentSessionPassword, _cts.Token))
		{
			p_slot.userId = PlayerSettings.Instance.UserId;
			isGameAboutToStart = _currentGameSessionDetails.PlayerSlots.All((ServerDTOs.PlayerSlotItem s) => s.isAIcontrolled || s.userId.HasValue);
		}
		_isPerformingAction = false;
		if (isGameAboutToStart)
		{
			_multiplayerGamesMenu.ShowContinueGameMenu();
		}
		else
		{
			OnActionPerformed();
		}
	}

	private void OnClick_LeaveSlot(ServerDTOs.PlayerSlotItem p_slot)
	{
		SoundManager.instance.PlayUIClickSound();
		LeaveSlotAsync(p_slot);
	}

	private async void LeaveSlotAsync(ServerDTOs.PlayerSlotItem p_slot)
	{
		if (!_isPerformingAction)
		{
			_isPerformingAction = true;
			await Server.Instance.GameService.LeaveGame(_currentSessionId, p_slot.index, _cts.Token);
			OnActionPerformed();
			_isPerformingAction = false;
		}
	}

	private void OnClick_KickPlayer(ServerDTOs.PlayerSlotItem p_slot)
	{
		SoundManager.instance.PlayUIClickSound();
		KickPlayerAsync(p_slot);
	}

	private async void KickPlayerAsync(ServerDTOs.PlayerSlotItem p_slot)
	{
		if (!_isPerformingAction)
		{
			_isPerformingAction = true;
			await Server.Instance.GameService.KickPlayerFromGame(_currentSessionId, p_slot.index, _cts.Token);
			OnActionPerformed();
			_isPerformingAction = false;
		}
	}

	private void OnActionPerformed()
	{
		_joinGameMenu.FetchAndOpenSession(_currentSessionId, _currentSessionPassword);
	}

	private void ClearSlots()
	{
		foreach (Transform item in _slotsListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private bool HasRequiredMods(ServerDTOs.GameSessionDetails p_gameSessionDetails)
	{
		if (p_gameSessionDetails == null || p_gameSessionDetails.Mods == null || p_gameSessionDetails.Mods.Count == 0)
		{
			return true;
		}
		IEnumerable<string> second = ModsManager.activeMods.Select((Mod m) => m.Name);
		return p_gameSessionDetails.Mods.Intersect(second).Count() > 0;
	}

	private void OnDisable()
	{
		_currentSessionId = Guid.Empty;
		_currentSessionPassword = string.Empty;
		_currentGameSessionDetails = null;
		_cts?.Cancel();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
