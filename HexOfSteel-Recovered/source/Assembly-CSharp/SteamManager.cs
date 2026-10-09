using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour
{
	public static SteamManager instance;

	public const byte MAX_LEADERBOARD_ENTRIES = 50;

	private static readonly TimeSpan TimestampSkew = TimeSpan.FromSeconds(5.0);

	[Header("Setup")]
	[SerializeField]
	private List<string> _leaderboardNames;

	[SerializeField]
	private List<string> _statsNames;

	private static SteamStatuses _prevStatus;

	private bool _areLeaderboardsReady;

	private int _leaderboardsAmount;

	private int _leaderboardsRetrievedAmount;

	private Dictionary<string, SteamLeaderboard_t> _leaderboardNameToHandle;

	private Dictionary<SteamLeaderboard_t, SteamLeaderboard> _steamLeaderboardToObject;

	public static bool IsSteamReady { get; private set; }

	public static CSteamID LocalPlayerSteamID { get; private set; }

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		_areLeaderboardsReady = false;
		_leaderboardsAmount = _leaderboardNames.Count;
		if (MainMenu.isDemo)
		{
			IsSteamReady = false;
			return;
		}
		bool flag = SteamAPI.Init();
		bool flag2 = SteamAPI.IsSteamRunning();
		IsSteamReady = flag & flag2;
		if (IsSteamReady)
		{
			LocalPlayerSteamID = SteamUser.GetSteamID();
			SceneManager.sceneLoaded += OnSceneLoaded;
			InitializePlayerStats();
			DownloadAndWriteToDiskSteamCloudSaveFiles();
		}
	}

	private void Update()
	{
		if (IsSteamReady)
		{
			SteamAPI.RunCallbacks();
		}
	}

	private void InitializePlayerStats()
	{
		if (GetPlayerStatInt("Version", out var _) && int.TryParse(Application.version.Replace(".", "").Replace(" ", ""), out var result) && result < 762)
		{
			SteamUserStats.ResetAllStats(bAchievementsToo: false);
			SteamUserStats.RequestCurrentStats();
			SetPlayerStat("Version", result);
		}
	}

	private void OnSceneLoaded(Scene p_scene, LoadSceneMode p_loadSceneMode)
	{
		if (p_scene.buildIndex == 0)
		{
			SteamUserStats.RequestCurrentStats();
			ProcessLoadedMainMenu();
		}
	}

	private void ProcessLoadedMainMenu()
	{
		SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
		UploadUserScoresAndStats();
	}

	public static bool ShouldApplyCensor()
	{
		if (!IsSteamReady)
		{
			return PlayerSettings.Instance.CensoredGermany;
		}
		return SteamUtils.GetIPCountry() == "DE";
	}

	public static bool OwnsDLC(AppId_t p_appId_T)
	{
		if (!IsSteamReady)
		{
			return false;
		}
		return SteamApps.BIsDlcInstalled(p_appId_T);
	}

	public static string GetSteamCloudSavesLocation()
	{
		return Path.Combine(Application.persistentDataPath, "Saves", "Steam Cloud");
	}

	public List<string> GetLeaderboardsNames()
	{
		return _leaderboardNames;
	}

	private void RefreshLeaderboards()
	{
		_areLeaderboardsReady = false;
		_leaderboardsRetrievedAmount = 0;
		_leaderboardNameToHandle = new Dictionary<string, SteamLeaderboard_t>();
		_steamLeaderboardToObject = new Dictionary<SteamLeaderboard_t, SteamLeaderboard>();
		foreach (string leaderboardName in _leaderboardNames)
		{
			RetrieveAndCacheLeaderboard(leaderboardName);
		}
	}

	private void UploadUserScoresAndStats()
	{
		StartCoroutine(CR_UploadLeaderboardsAndStats());
	}

	private void RetrieveAndCacheLeaderboard(string p_leaderboardName)
	{
		SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard(p_leaderboardName);
		CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult).Set(hAPICall);
	}

	private void OnLeaderboardFindResult(LeaderboardFindResult_t p_result, bool p_failure)
	{
		_leaderboardsRetrievedAmount++;
		if (p_failure || p_result.m_bLeaderboardFound == 0)
		{
			Debug.LogError("Failed to find leaderboard.");
			return;
		}
		SteamLeaderboard_t hSteamLeaderboard = p_result.m_hSteamLeaderboard;
		string leaderboardName = SteamUserStats.GetLeaderboardName(hSteamLeaderboard);
		if (!_steamLeaderboardToObject.ContainsKey(hSteamLeaderboard))
		{
			SteamLeaderboard value = new SteamLeaderboard(leaderboardName);
			_steamLeaderboardToObject[hSteamLeaderboard] = value;
			_leaderboardNameToHandle[leaderboardName] = hSteamLeaderboard;
		}
		DownloadLocalPlayerEntry(hSteamLeaderboard);
		if (_leaderboardsRetrievedAmount == _leaderboardsAmount)
		{
			_areLeaderboardsReady = true;
		}
	}

	private void DownloadLeaderboardEntries(SteamLeaderboard_t p_leaderboardHandle)
	{
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(p_leaderboardHandle, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 50);
		CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded).Set(hAPICall);
	}

	private void DownloadLocalPlayerEntry(SteamLeaderboard_t p_leaderboardHandle)
	{
		CSteamID[] array = new CSteamID[1] { LocalPlayerSteamID };
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntriesForUsers(p_leaderboardHandle, array, array.Length);
		CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardLocalPlayerScoresDownloaded).Set(hAPICall);
	}

	private void OnLeaderboardLocalPlayerScoresDownloaded(LeaderboardScoresDownloaded_t p_result, bool p_failure)
	{
		SteamLeaderboard value;
		if (p_failure)
		{
			Debug.LogError("Failed to download leaderboard scores for local player.");
		}
		else if (_steamLeaderboardToObject.TryGetValue(p_result.m_hSteamLeaderboard, out value) && p_result.m_cEntryCount > 0)
		{
			SteamUserStats.GetDownloadedLeaderboardEntry(p_result.m_hSteamLeaderboardEntries, 0, out var pLeaderboardEntry, null, 0);
			value.localPlayerEntry = new SteamLeaderboardEntry(SteamFriends.GetFriendPersonaName(LocalPlayerSteamID), pLeaderboardEntry.m_nScore, pLeaderboardEntry.m_nGlobalRank, p_isLocalClient: true);
		}
	}

	private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t p_result, bool p_failure)
	{
		SteamLeaderboard value;
		if (p_failure)
		{
			Debug.LogError("Failed to download leaderboard scores.");
		}
		else if (_steamLeaderboardToObject.TryGetValue(p_result.m_hSteamLeaderboard, out value))
		{
			value.entries.Clear();
			for (int i = 0; i < p_result.m_cEntryCount; i++)
			{
				SteamUserStats.GetDownloadedLeaderboardEntry(p_result.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, null, 0);
				value.entries.Add(new SteamLeaderboardEntry(SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser), pLeaderboardEntry.m_nScore, pLeaderboardEntry.m_nGlobalRank, pLeaderboardEntry.m_steamIDUser == LocalPlayerSteamID));
			}
		}
		else
		{
			Debug.LogError("Could not find associated handle");
		}
	}

	public static bool IsDLCInstalled(uint p_DLCID)
	{
		return SteamApps.BIsDlcInstalled(new AppId_t(p_DLCID));
	}

	public static void UnlockAchievement(string p_ID)
	{
		if (IsSteamReady)
		{
			SteamUserStats.SetAchievement(p_ID);
			SteamUserStats.StoreStats();
		}
	}

	public static void SetRichPresenceStatus(SteamStatuses p_steamStatus)
	{
		if (IsSteamReady && _prevStatus != p_steamStatus)
		{
			_prevStatus = p_steamStatus;
			SteamFriends.SetRichPresence("text", GetStatusString(p_steamStatus));
			SteamFriends.SetRichPresence("steam_display", "#status");
		}
	}

	public void AddPlayerStat(string p_statName, int p_value)
	{
		if (IsSteamReady && GetPlayerStatInt(p_statName, out var o_value))
		{
			SetPlayerStat(p_statName, o_value + p_value);
		}
	}

	public void SetPlayerStat(string p_statName, int p_value)
	{
		if (IsSteamReady)
		{
			if (SteamUserStats.SetStat(p_statName, p_value))
			{
				SteamUserStats.StoreStats();
			}
			else
			{
				Debug.LogError("Failed to set stat " + p_statName);
			}
		}
	}

	public bool GetPlayerStatInt(string p_statName, out int o_value)
	{
		if (SteamUserStats.GetStat(p_statName, out o_value))
		{
			return true;
		}
		Debug.LogError("Failed to get stat " + p_statName);
		return false;
	}

	public void SetLeaderboard(int p_score, string p_leaderboardName, LeaderboardActions p_leaderboardAction)
	{
		if (!IsSteamReady || !_areLeaderboardsReady)
		{
			return;
		}
		foreach (KeyValuePair<string, SteamLeaderboard_t> item in _leaderboardNameToHandle)
		{
			if (item.Key == p_leaderboardName)
			{
				SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(item.Value, GetUploadTypeAction(p_leaderboardAction), p_score, new int[0], 0);
				CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoresUploaded).Set(hAPICall);
				break;
			}
		}
	}

	private ELeaderboardUploadScoreMethod GetUploadTypeAction(LeaderboardActions p_leaderboardAction)
	{
		switch (p_leaderboardAction)
		{
		case LeaderboardActions.KEEP_BEST:
			return ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
		case LeaderboardActions.FORCE_UPDATE:
			return ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate;
		default:
			return ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodNone;
		}
	}

	private void OnLeaderboardScoresUploaded(LeaderboardScoreUploaded_t p_result, bool p_failure)
	{
		if (p_failure)
		{
			Debug.LogError("Failed to upload leaderboard score.");
		}
	}

	private IEnumerator CR_UploadLeaderboardsAndStats()
	{
		while (!IsSteamReady)
		{
			yield return new WaitForEndOfFrame();
		}
		RefreshLeaderboards();
		while (!_areLeaderboardsReady)
		{
			yield return new WaitForEndOfFrame();
		}
		foreach (KeyValuePair<string, SteamLeaderboard_t> item in _leaderboardNameToHandle)
		{
			foreach (string statsName in _statsNames)
			{
				if (!(statsName != item.Key) && GetPlayerStatInt(statsName, out var o_value) && o_value > 0)
				{
					SetLeaderboard(o_value, item.Key, LeaderboardActions.KEEP_BEST);
					break;
				}
			}
		}
	}

	private static string GetStatusString(SteamStatuses p_steamStatus)
	{
		switch (p_steamStatus)
		{
		default:
			return "NONE";
		case SteamStatuses.DEFAULT:
			if (TurnManager.humanPlayer != null)
			{
				return "Playing " + GameData.Instance.map.ScenarioName + " as " + TurnManager.humanPlayer.Country.Replace(".", "");
			}
			return "Witnessing the greatest AI battle.";
		case SteamStatuses.CREATING_MAP:
			if (!string.IsNullOrEmpty(GameData.Instance.map.ScenarioName))
			{
				return "Designing a scenario: " + GameData.Instance.map.ScenarioName;
			}
			return "Designing a scenario.";
		case SteamStatuses.MAIN_MENU:
			return "At the main menu.";
		case SteamStatuses.DIPLOMACY:
			return "Doing diplomacy.";
		case SteamStatuses.RESEARCH:
			return "Researching new technologies.";
		case SteamStatuses.LEADERBOARD:
			return "Checking if he is the best of the best.";
		case SteamStatuses.END_GAME_STATISTICS:
			return "Admiring the sheer chaos after the battle.";
		case SteamStatuses.STARTING_CAMPAIGN:
			return "Starting a new campaign.";
		case SteamStatuses.STARTING_QUICKGAME:
			return "About to start a new scenario.";
		case SteamStatuses.MODDING:
			return "Working on a new mod.";
		case SteamStatuses.SETTINGS:
			return "Looking for an 'instant win' setting.";
		}
	}

	public void UnsubscribeFromWorkshopItem(PublishedFileId_t p_publishedFileId_T)
	{
		if (IsSteamReady)
		{
			SteamUGC.UnsubscribeItem(p_publishedFileId_T);
		}
	}

	public void UploadSaveFileToSteamCloud(string p_fileName, GameData p_gameData)
	{
		if (IsSteamReady)
		{
			byte[] array = Utils.ConvertObjectToByteArray(p_gameData);
			int cubData = array.Length;
			if (SteamRemoteStorage.FileWrite(p_fileName, array, cubData))
			{
				Debug.Log("Successfully uploaded save file to SteamCloud: " + p_fileName);
			}
			else
			{
				Debug.LogError("Failed to write save file to SteamCloud.");
			}
		}
	}

	public void DeleteSteamCloudFile(string p_fileName)
	{
		if (IsSteamReady)
		{
			if (SteamRemoteStorage.FileDelete(p_fileName))
			{
				Debug.Log("Successfully delete save file from SteamCloud: " + p_fileName);
			}
			else
			{
				Debug.LogError("Failed to delete save file from SteamCloud: " + p_fileName);
			}
		}
	}

	public void RenameSteamCloudFile(string p_oldName, string p_newName)
	{
		if (!IsSteamReady)
		{
			return;
		}
		int fileSize = SteamRemoteStorage.GetFileSize(p_oldName);
		if (fileSize <= 0)
		{
			Debug.LogError("Failed to find the Steam Cloud file: " + p_oldName);
			return;
		}
		byte[] pvData = new byte[fileSize];
		int num = SteamRemoteStorage.FileRead(p_oldName, pvData, fileSize);
		if (num != fileSize)
		{
			Debug.LogError($"Failed to read the full Steam Cloud file: {p_oldName}. Expected {fileSize} bytes but read {num} bytes.");
		}
		else if (SteamRemoteStorage.FileWrite(p_newName, pvData, fileSize))
		{
			Debug.Log("Successfully wrote data to SteamCloud: " + p_newName);
			DeleteSteamCloudFile(p_oldName);
		}
		else
		{
			Debug.LogError("Failed to write the new Steam Cloud file: " + p_newName);
		}
	}

	public List<string> GetSteamCloudSaves()
	{
		List<string> list = new List<string>();
		if (!IsSteamReady)
		{
			return list;
		}
		int fileCount = SteamRemoteStorage.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			list.Add(SteamRemoteStorage.GetFileNameAndSize(i, out var _));
		}
		return list;
	}

	private void DownloadAndWriteToDiskSteamCloudSaveFiles()
	{
		if (!IsSteamReady)
		{
			return;
		}
		string steamCloudSavesLocation = GetSteamCloudSavesLocation();
		Directory.CreateDirectory(steamCloudSavesLocation);
		List<string> steamCloudSaves = GetSteamCloudSaves();
		if (steamCloudSaves == null || steamCloudSaves.Count == 0)
		{
			return;
		}
		foreach (string item in steamCloudSaves)
		{
			string text = BuildLocalPath(steamCloudSavesLocation, item);
			if (IsLocalUpToDate(text, item))
			{
				continue;
			}
			if (!SteamRemoteStorage.FileExists(item))
			{
				Debug.LogError("Cloud file not found: " + item);
				continue;
			}
			int fileSize = SteamRemoteStorage.GetFileSize(item);
			if (fileSize < 0)
			{
				Debug.LogWarning($"Invalid cloud file size for {item}: {fileSize}");
				continue;
			}
			byte[] array = new byte[fileSize];
			int num = SteamRemoteStorage.FileRead(item, array, fileSize);
			if (num != fileSize)
			{
				Debug.LogWarning($"Cloud read short for {item}: {num}/{fileSize}");
				continue;
			}
			try
			{
				File.WriteAllBytes(text, array);
				DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(SteamRemoteStorage.GetFileTimestamp(item)).UtcDateTime;
				File.SetCreationTimeUtc(text, utcDateTime);
				File.SetLastWriteTimeUtc(text, utcDateTime);
				Debug.Log($"Downloaded {item} → {text} ({fileSize} bytes).");
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed writing {item} to disk: {arg}");
			}
		}
	}

	private static string BuildLocalPath(string p_root, string p_cloudName)
	{
		string path = (p_cloudName.EndsWith(".sav", StringComparison.OrdinalIgnoreCase) ? p_cloudName : (p_cloudName + ".sav"));
		return Path.Combine(p_root, path);
	}

	private static bool IsLocalUpToDate(string p_localPath, string p_cloudName)
	{
		if (!File.Exists(p_localPath))
		{
			return false;
		}
		long fileTimestamp = SteamRemoteStorage.GetFileTimestamp(p_cloudName);
		if (fileTimestamp <= 0)
		{
			return false;
		}
		DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(fileTimestamp).UtcDateTime;
		return File.GetLastWriteTimeUtc(p_localPath) + TimestampSkew >= utcDateTime;
	}

	private void OnApplicationQuit()
	{
		if (IsSteamReady)
		{
			SteamFriends.ClearRichPresence();
			SteamAPI.Shutdown();
		}
	}
}
