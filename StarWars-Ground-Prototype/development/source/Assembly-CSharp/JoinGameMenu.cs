using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JoinGameMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private GameSessionContentMenu _gameSessionContentMenu;

	[SerializeField]
	private SessionPasswordMenu _sessionPasswordMenu;

	[Space]
	[SerializeField]
	private Transform _gameListContainer;

	[SerializeField]
	private OpenGameSlotUI _openGameSlotPrefab;

	private bool _isFetchingGames;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isFetchingGames = false;
		_cts = new CancellationTokenSource();
		ClearGameList();
		FetchCurrentGames();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void RefreshGamesListing()
	{
		_sessionPasswordMenu.Close();
		_gameSessionContentMenu.Close();
		FetchCurrentGames();
	}

	public void FetchAndOpenSession(Guid p_sessionId, string p_password)
	{
		RefreshGamesListing();
		_gameSessionContentMenu.OpenWithSessionId(p_sessionId, p_password, null);
	}

	private async void FetchCurrentGames()
	{
		if (!_isFetchingGames)
		{
			ServerDTOs.FetchGamesRequest fetchGamesRequest = new ServerDTOs.FetchGamesRequest();
			fetchGamesRequest.states = new GameSessionStates[1];
			ServerDTOs.FetchGamesRequest p_fetchGamesRequest = fetchGamesRequest;
			_isFetchingGames = true;
			List<ServerDTOs.GameSessionDetails> p_gameDetails = await Server.Instance.GameService.FetchGames(p_fetchGamesRequest, _cts.Token);
			_isFetchingGames = false;
			PopulateGameList(p_gameDetails);
		}
	}

	private void PopulateGameList(List<ServerDTOs.GameSessionDetails> p_gameDetails)
	{
		ClearGameList();
		if (p_gameDetails == null)
		{
			return;
		}
		p_gameDetails.Sort((ServerDTOs.GameSessionDetails a, ServerDTOs.GameSessionDetails b) => b.CreatedAtUtc.CompareTo(a.CreatedAtUtc));
		foreach (ServerDTOs.GameSessionDetails p_gameDetail in p_gameDetails)
		{
			UnityEngine.Object.Instantiate(_openGameSlotPrefab, _gameListContainer).Setup(p_gameDetail, OnSlotButtonClick);
		}
	}

	private void OnSlotButtonClick(ServerDTOs.GameSessionDetails p_gameDetails)
	{
		SoundManager.instance.PlayUIClickSound();
		_sessionPasswordMenu.Close();
		_gameSessionContentMenu.Close();
		if (p_gameDetails.IsPasswordProtected && !p_gameDetails.IsHost && !p_gameDetails.IsParticipant)
		{
			_sessionPasswordMenu.OpenForSession(p_gameDetails.Id, OnPasswordVerified);
		}
		else
		{
			_gameSessionContentMenu.OpenWithSessionId(p_gameDetails.Id, string.Empty, p_gameDetails);
		}
	}

	private void OnPasswordVerified(Guid p_sessionId, string p_password, ServerDTOs.GameSessionDetails p_gameDetails)
	{
		_gameSessionContentMenu.OpenWithSessionId(p_sessionId, p_password, p_gameDetails);
	}

	private void ClearGameList()
	{
		foreach (Transform item in _gameListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void OnDisable()
	{
		_sessionPasswordMenu.Close();
		_gameSessionContentMenu.Close();
		_cts?.Cancel();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
