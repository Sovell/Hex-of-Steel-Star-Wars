using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameSessionActionsMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private ContinueGameMenu _continueGameMenu;

	[Space]
	[SerializeField]
	private Button _playTurn_button;

	[SerializeField]
	private Button _leave_button;

	[SerializeField]
	private Button _forfeit_button;

	[SerializeField]
	private Button _deleteGame_button;

	private CancellationTokenSource _cts;

	private bool _isPerformingAction;

	private ServerDTOs.GameSessionDetails _gameSessionDetails;

	private void OnEnable()
	{
		_isPerformingAction = false;
		_gameSessionDetails = null;
		_cts = new CancellationTokenSource();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void OpenForGameSession(ServerDTOs.GameSessionDetails p_gameDetails)
	{
		Open();
		_gameSessionDetails = p_gameDetails;
		RefreshButtonsInteractivity(p_gameDetails);
	}

	public async void OnClick_PlayTurn()
	{
		if (!_isPerformingAction)
		{
			SoundManager.instance.PlayUIClickSound();
			_isPerformingAction = true;
			_playTurn_button.interactable = false;
			MainMenu.instance.loading_Panel.SetActive(value: true);
			if (await Server.Instance.GameService.PlayTurn(_gameSessionDetails.Id, _cts.Token))
			{
				OnActionPerformed();
			}
			else
			{
				MainMenu.instance.loading_Panel.SetActive(value: false);
				_playTurn_button.interactable = true;
			}
			_isPerformingAction = false;
		}
	}

	public async void OnClick_LeaveGame()
	{
		if (!_isPerformingAction)
		{
			SoundManager.instance.PlayUIClickSound();
			_isPerformingAction = true;
			if (await Server.Instance.GameService.LeaveGame(_gameSessionDetails.Id, -1, _cts.Token))
			{
				OnActionPerformed();
			}
			_isPerformingAction = false;
		}
	}

	public async void OnClick_Forfeit()
	{
		if (!_isPerformingAction)
		{
			SoundManager.instance.PlayUIClickSound();
			_isPerformingAction = true;
			if (await Server.Instance.GameService.Forfeit(_gameSessionDetails.Id, _cts.Token))
			{
				OnActionPerformed();
			}
			_isPerformingAction = false;
		}
	}

	public void OnClick_DeleteGame()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirm game deletion"), ConfirmDeleteGameAsync);
	}

	private async void ConfirmDeleteGameAsync()
	{
		if (!_isPerformingAction)
		{
			SoundManager.instance.PlayUIClickSound();
			_isPerformingAction = true;
			if (await Server.Instance.GameService.DeleteGame(_gameSessionDetails.Id, _cts.Token))
			{
				OnActionPerformed();
			}
			_isPerformingAction = false;
		}
	}

	private void RefreshButtonsInteractivity(ServerDTOs.GameSessionDetails p_gameDetails)
	{
		TimeSpan timeSpan = TimeSpan.FromDays(2.0);
		bool flag = !p_gameDetails.IsHost && !p_gameDetails.IsActivePlayer && ((p_gameDetails.LastPlayedAtUtc.HasValue && DateTime.UtcNow - p_gameDetails.LastPlayedAtUtc.Value >= timeSpan) || DateTime.UtcNow - p_gameDetails.UpdatedAtUtc >= timeSpan);
		_playTurn_button.interactable = p_gameDetails.IsActivePlayer;
		_deleteGame_button.interactable = p_gameDetails.IsHost;
		_forfeit_button.interactable = p_gameDetails.IsParticipant && p_gameDetails.TurnNumber > 0 && !flag;
		_leave_button.interactable = flag && p_gameDetails.IsParticipant;
	}

	private void OnActionPerformed()
	{
		Close();
		_continueGameMenu.RefreshGamesListing();
	}

	private void OnDisable()
	{
		_cts?.Cancel();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
