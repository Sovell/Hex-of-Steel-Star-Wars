using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ArchivedGamesMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private Transform _gameListContainer;

	[SerializeField]
	private ArchivedGameSlotUI _archivedGameSlotPrefab;

	private bool _isFetchingGames;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isFetchingGames = false;
		_cts = new CancellationTokenSource();
		FetchArchivedGames();
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
		FetchArchivedGames();
	}

	private async void FetchArchivedGames()
	{
		if (!_isFetchingGames)
		{
			ServerDTOs.FetchGamesRequest fetchGamesRequest = new ServerDTOs.FetchGamesRequest();
			fetchGamesRequest.states = new GameSessionStates[1] { GameSessionStates.FINISHED };
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
			Object.Instantiate(_archivedGameSlotPrefab, _gameListContainer).Setup(p_gameDetail);
		}
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
