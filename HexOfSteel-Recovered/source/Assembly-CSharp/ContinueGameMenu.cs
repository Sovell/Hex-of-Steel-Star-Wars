using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ContinueGameMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private GameSessionActionsMenu _gameSessionActionsMenu;

	[SerializeField]
	private Transform _gameListContainer;

	[SerializeField]
	private OngoingGameSlotUI _ongoingGameSlotPrefab;

	private bool _isFetchingGames;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isFetchingGames = false;
		_cts = new CancellationTokenSource();
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
		FetchCurrentGames();
	}

	private async void FetchCurrentGames()
	{
		if (!_isFetchingGames)
		{
			ServerDTOs.FetchGamesRequest fetchGamesRequest = new ServerDTOs.FetchGamesRequest();
			fetchGamesRequest.states = new GameSessionStates[1] { GameSessionStates.ONGOING };
			fetchGamesRequest.iAmIn = true;
			ServerDTOs.FetchGamesRequest p_fetchGamesRequest = fetchGamesRequest;
			_isFetchingGames = true;
			List<ServerDTOs.GameSessionDetails> p_gameDetails = await Server.Instance.GameService.FetchGames(p_fetchGamesRequest, _cts.Token);
			_isFetchingGames = false;
			PopulateGameList(p_gameDetails);
		}
	}

	private void PopulateGameList(List<ServerDTOs.GameSessionDetails> p_gameDetails)
	{
		foreach (Transform item in _gameListContainer)
		{
			Object.Destroy(item.gameObject);
		}
		if (p_gameDetails == null)
		{
			return;
		}
		p_gameDetails.Sort((ServerDTOs.GameSessionDetails a, ServerDTOs.GameSessionDetails b) => b.UpdatedAtUtc.CompareTo(a.UpdatedAtUtc));
		foreach (ServerDTOs.GameSessionDetails p_gameDetail in p_gameDetails)
		{
			Object.Instantiate(_ongoingGameSlotPrefab, _gameListContainer).Setup(p_gameDetail, OnGameSlotClicked);
		}
	}

	private void OnGameSlotClicked(ServerDTOs.GameSessionDetails p_gameDetails)
	{
		SoundManager.instance.PlayUIClickSound();
		_gameSessionActionsMenu.OpenForGameSession(p_gameDetails);
	}

	private void OnDisable()
	{
		_cts?.Cancel();
		_gameSessionActionsMenu.Close();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
