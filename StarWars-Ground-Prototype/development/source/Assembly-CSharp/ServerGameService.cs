using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameService : ServiceBase
{
	private readonly ServerAuthService _authService;

	public ServerGameService(HttpClient p_client, ServerAuthService p_authService)
		: base(p_client)
	{
		_authService = p_authService;
	}

	public async Task<(bool, Guid)> HostGame(GameData p_gameData, string p_description, string p_password, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[HostGame] User is not authenticated.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return (false, Guid.Empty);
		}
		if (p_gameData == null || p_gameData.listOfPlayers == null || p_gameData.map == null)
		{
			Debug.LogError("[HostGame] Game data is incomplete.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid scenario data"));
			return (false, Guid.Empty);
		}
		if (p_gameData.listOfPlayers.Count((Player p) => !p.IsComputer) < 2)
		{
			Debug.LogError("[HostGame] At least two human players are required to host a game.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not enough human players"));
			return (false, Guid.Empty);
		}
		if (p_gameData.listOfPlayers.Count((Player p) => p.IsPlayingFirst) != 1)
		{
			Debug.LogError("[HostGame] Exactly one player must be marked as playing first.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid playing first player"));
			return (false, Guid.Empty);
		}
		string text = (string.IsNullOrWhiteSpace(p_gameData.map.ScenarioName) ? "Untitled" : p_gameData.map.ScenarioName);
		if (!ServerUtils.IsValidSessionName(text))
		{
			Debug.LogError("[HostGame] Invalid session name.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid session name"));
			return (false, Guid.Empty);
		}
		if (!ServerUtils.IsValidSessionDescription(p_description))
		{
			Debug.LogError("[HostGame] Invalid session description.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid session description"));
			return (false, Guid.Empty);
		}
		ServerDTOs.HostGameRequest hostGameRequest = new ServerDTOs.HostGameRequest();
		hostGameRequest.name = text;
		hostGameRequest.description = p_description;
		hostGameRequest.password = p_password;
		string settings = JsonConvert.SerializeObject(p_gameData.gameSettings);
		hostGameRequest.settings = settings;
		hostGameRequest.mods = (from m in ModsManager.activeMods
			where m.HasGameplayImpact()
			select m.Name).ToList();
		hostGameRequest.playerSlots = p_gameData.listOfPlayers.Select((Player p, int index) => new ServerDTOs.PlayerSlotItem
		{
			index = index,
			nation = p.Country,
			playerName = p.Name,
			isPlayingFirst = p.IsPlayingFirst,
			isAIcontrolled = p.IsComputer
		}).ToArray();
		p_gameData.map.IsCampaign = false;
		p_gameData.messages = new List<string>();
		hostGameRequest.data = Utils.ConvertObjectToByteArray(p_gameData);
		hostGameRequest.dataHash = ServerUtils.Sha256Base64(hostGameRequest.data);
		Debug.Log($"[HostGame] Data sent: {hostGameRequest.dataHash} ({hostGameRequest.data.Length} bytes)");
		string content = JsonConvert.SerializeObject(hostGameRequest);
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/host"), content2, p_ct))
			{
				string text2 = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text2);
					return (false, Guid.Empty);
				}
				Debug.Log($"[HostGame] {(int)res.StatusCode} {res.StatusCode}");
				Guid item = JsonConvert.DeserializeObject<Guid>(text2);
				return (true, item);
			}
		}
	}

	public async Task<List<ServerDTOs.GameSessionDetails>> FetchGames(ServerDTOs.FetchGamesRequest p_fetchGamesRequest, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[FetchGames] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return null;
		}
		string content = JsonConvert.SerializeObject(p_fetchGamesRequest);
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/fetch"), content2, p_ct))
			{
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return null;
				}
				List<ServerDTOs.GameSessionDetails> list = JsonConvert.DeserializeObject<List<ServerDTOs.GameSessionDetails>>(text);
				Debug.Log($"[FetchGames] {list.Count} games fetched.");
				return list;
			}
		}
	}

	public async Task<ServerDTOs.GameSessionDetails> GetGameSessionDetails(Guid p_gameID, string p_password, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[GetGameSessionDetails] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return null;
		}
		string text = Routes.Api(Routes.Format("/games/details/{id:guid}", p_gameID));
		if (!string.IsNullOrEmpty(p_password))
		{
			text = text + "?password=" + Uri.EscapeDataString(p_password);
		}
		using (HttpResponseMessage res = await _client.GetAsync(text, p_ct))
		{
			string text2 = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, text2);
				return null;
			}
			ServerDTOs.GameSessionDetails result = JsonConvert.DeserializeObject<ServerDTOs.GameSessionDetails>(text2);
			Debug.Log($"[GetGameSessionDetails] sessionID: {p_gameID}");
			return result;
		}
	}

	public async Task<bool> JoinGame(Guid p_gameID, int p_slotIndex, string p_password, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[JoinGame] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.JoinGameRequest
		{
			gameSessionId = p_gameID,
			slotIndex = p_slotIndex,
			password = p_password
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/join"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[JoinGame] SessionID: {p_gameID} - SlotIndex: {p_slotIndex}");
				return true;
			}
		}
	}

	public async Task<bool> LeaveGame(Guid p_gameID, int p_slotIndex, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[LeaveGame] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.LeaveGameRequest
		{
			gameSessionId = p_gameID,
			slotIndex = p_slotIndex
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/leave"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[LeaveGame] Left: {p_gameID} - SlotIndex: {p_slotIndex}");
				return true;
			}
		}
	}

	public async Task<bool> KickPlayerFromGame(Guid p_gameID, int p_slotIndex, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[KickPlayerFromGame] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.KickPlayerRequest
		{
			gameSessionId = p_gameID,
			slotIndex = p_slotIndex
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/kick"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[KickPlayerFromGame] Kicked player in slot: {p_slotIndex} from game: {p_gameID}");
				return true;
			}
		}
	}

	public async Task<bool> PlayTurn(Guid p_gameID, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[PlayTurn] Not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		p_ct.ThrowIfCancellationRequested();
		ServerDTOs.GameSessionDetails sessionDetails = await GetGameSessionDetails(p_gameID, string.Empty, p_ct);
		if (sessionDetails == null)
		{
			return false;
		}
		Debug.Log($"[PlayTurn] Turn Number: {sessionDetails.TurnNumber}");
		List<string> list = (from m in ModsManager.activeMods
			where m.HasGameplayImpact()
			select m.Name).ToList();
		List<string> list2 = sessionDetails.Mods.Except(list).ToList();
		if (list2.Count > 0)
		{
			Debug.LogError("[PlayTurn] Missing required mods: " + string.Join(", ", list2));
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing required mods") + string.Join(", ", list2));
			return false;
		}
		List<string> list3 = list.Except(sessionDetails.Mods).ToList();
		if (list3.Any())
		{
			Debug.LogError("[PlayTurn] Extra mods enabled: " + string.Join(", ", list3));
			UIManager.ShowMessage(LocalizationManager.Translate("error-extra mods") + string.Join(", ", list3));
			return false;
		}
		string requestUri = Routes.Api(Routes.Format("/games/turns/start/{id:guid}", p_gameID));
		using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUri))
		{
			req.Headers.Accept.Clear();
			req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
			p_ct.ThrowIfCancellationRequested();
			using (HttpResponseMessage res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, p_ct))
			{
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, await res.Content.ReadAsStringAsync());
					return false;
				}
				byte[] array = await res.Content.ReadAsByteArrayAsync();
				Debug.Log("Received data length: " + array.Length);
				if (!res.Headers.TryGetValues("X-Data-Hash", out var values) || !ServerUtils.VerifySha256Base64(array, values.FirstOrDefault()))
				{
					Debug.LogError("[PlayTurn] Data integrity check failed (invalid hash).");
					UIManager.ShowMessage(LocalizationManager.Translate("error-game data integrity compromised"));
					return false;
				}
				GameData gameData = (GameData)Utils.ConvertByteArrayToObject(array);
				Guid sessionID = gameData.sessionID;
				Debug.Log("Session ID: " + sessionID.ToString());
				if (gameData.listOfPlayers.Count((Player p) => !p.IsComputer) > sessionDetails.PlayerSlots.Count((ServerDTOs.PlayerSlotItem s) => !s.isAIcontrolled))
				{
					Debug.LogError("[PlayTurn] Player count mismatch");
					UIManager.ShowMessage(LocalizationManager.Translate("error-players number mismatch"));
					return false;
				}
				gameData.isPBEM = true;
				gameData.sessionID = p_gameID;
				foreach (Player listOfPlayer in gameData.listOfPlayers)
				{
					ServerDTOs.PlayerSlotItem[] playerSlots = sessionDetails.PlayerSlots;
					foreach (ServerDTOs.PlayerSlotItem playerSlotItem in playerSlots)
					{
						if (listOfPlayer.Name == playerSlotItem.playerName)
						{
							listOfPlayer.UserId = playerSlotItem.userId;
							listOfPlayer.IsComputer = playerSlotItem.isAIcontrolled;
							break;
						}
					}
				}
				GameData.Instance.SetData(gameData);
				GameData.Instance.ironManMode = false;
				Debug.Log($"[PlayTurn] {(int)res.StatusCode} {res.StatusCode}");
				try
				{
					MainMenu.instance.loading_Panel.SetActive(value: true);
					MainMenu.isLoadedGame = GameData.Instance.map.timePlayed > 0f;
					SceneManager.LoadScene("Game");
				}
				catch (Exception ex)
				{
					MainMenu.instance.loading_Panel.SetActive(value: false);
					UIManager.ShowMessage(ex.Message);
				}
			}
		}
		return true;
	}

	public async Task<bool> EndTurn(GameData p_gameData, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[EndTurn] Not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (p_gameData == null || p_gameData.map == null || p_gameData.listOfPlayers == null)
		{
			Debug.LogError("[EndTurn] Incomplete game data");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid scenario data"));
			return false;
		}
		if (p_gameData.sessionID == Guid.Empty)
		{
			Debug.LogError("[EndTurn] Game ID is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing session ID"));
			return false;
		}
		GameData.Instance.map.timePlayed += MapGO.timeElapsedSinceGameStarted.ElapsedMilliseconds;
		MapGO.timeElapsedSinceGameStarted.Restart();
		byte[] array = Utils.ConvertObjectToByteArray(p_gameData);
		Debug.Log("Sent data length: " + array.Length);
		UIManager.ShowMessage(LocalizationManager.Translate("info-upload file size") + Utils.GetSizeInMB(array));
		string text = ServerUtils.Sha256Base64(array);
		Debug.Log("Data hash: " + text);
		string requestUri = Routes.Api(Routes.Format("/games/turns/end/{id:guid}", p_gameData.sessionID));
		string content = JsonConvert.SerializeObject(new ServerDTOs.EndTurnRequest
		{
			gameSessionId = p_gameData.sessionID,
			turnNumber = p_gameData.map.turnCount,
			data = array,
			dataHash = text
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(requestUri, content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-try send file again"), async () =>
					{
						await EndTurn(p_gameData, CancellationToken.None);
					});
					return false;
				}
				Debug.Log($"[EndTurn] {(int)res.StatusCode} {res.StatusCode}");
				MainMenu.openMultiplayerMenuAfterGame = true;
				SceneManager.LoadScene("MainMenu");
				return true;
			}
		}
	}

	public async Task<bool> Forfeit(Guid p_gameID, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[Forfeit] Not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (p_gameID == Guid.Empty)
		{
			Debug.LogError("[Forfeit] Game ID is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing session ID"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new
		{
			gameSessionId = p_gameID
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/games/forfeit"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[Forfeit] {(int)res.StatusCode} {res.StatusCode}");
				SteamManager.instance.AddPlayerStat("Lost games", 1);
				return true;
			}
		}
	}

	public async Task<bool> EndGame(GameData p_gameData, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[EndGame] Not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (p_gameData == null || p_gameData.map == null || p_gameData.listOfPlayers == null)
		{
			Debug.LogError("[EndGame] Incomplete game data");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid scenario data"));
			return false;
		}
		if (p_gameData.sessionID == Guid.Empty)
		{
			Debug.LogError("[EndGame] Game ID is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing session ID"));
			return false;
		}
		byte[] array = Utils.ConvertObjectToByteArray(p_gameData);
		Debug.Log("Sent data length: " + array.Length);
		string content = JsonConvert.SerializeObject(new ServerDTOs.EndGameRequest
		{
			gameSessionId = p_gameData.sessionID,
			winnerUserIds = (from p in p_gameData.listOfPlayers
				where p.UserId.HasValue && p.GameOutcome == GameOutcomes.VICTORY
				select p.UserId.Value).ToArray()
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			string requestUri = Routes.Api(Routes.Format("/games/end/{id:guid}", p_gameData.sessionID));
			using (HttpResponseMessage res = await _client.PostAsync(requestUri, content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[EndGame] {(int)res.StatusCode} {res.StatusCode}");
				return true;
			}
		}
	}

	public async Task<bool> DeleteGame(Guid p_gameID, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[DeleteGame] Not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (p_gameID == Guid.Empty)
		{
			Debug.LogError("[DeleteGame] Game ID is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing session ID"));
			return false;
		}
		string requestUri = Routes.Api(Routes.Format("/games/delete/{id:guid}", p_gameID));
		using (HttpResponseMessage res = await _client.DeleteAsync(requestUri, p_ct))
		{
			string body = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, body);
				return false;
			}
			Debug.Log($"[DeleteGame] {(int)res.StatusCode} {res.StatusCode}");
			return true;
		}
	}
}
