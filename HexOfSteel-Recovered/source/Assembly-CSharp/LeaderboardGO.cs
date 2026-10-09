using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardGO : MonoBehaviour
{
	public Transform listOfScenarios_Parent;

	public Transform leaderboard_parent;

	public GameObject leaderboardScenario_prefab;

	public LeaderboardEntryUI leaderboardSlot_prefab;

	public TextMeshProUGUI columnValues_text;

	private SteamLeaderboard _steamLeaderboard;

	private void OnEnable()
	{
		RefreshListOfScenarios();
		SteamManager.SetRichPresenceStatus(SteamStatuses.LEADERBOARD);
	}

	private void OnDisable()
	{
		SteamManager.SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
	}

	private void RefreshListOfScenarios()
	{
		Transform[] componentsInChildren = listOfScenarios_Parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == listOfScenarios_Parent))
			{
				Object.Destroy(transform.gameObject);
			}
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string leaderboardsName in SteamManager.instance.GetLeaderboardsNames())
		{
			if (leaderboardsName.Contains("Most"))
			{
				list2.Add(leaderboardsName);
			}
			else
			{
				list.Add(leaderboardsName);
			}
		}
		list = list.OrderBy((string x) => x).ToList();
		list2 = list2.OrderBy((string x) => x).ToList();
		foreach (string leaderboard in list.Concat(list2).ToList())
		{
			GameObject gameObject = Object.Instantiate(leaderboardScenario_prefab, listOfScenarios_Parent);
			gameObject.name = leaderboard;
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = leaderboard;
			gameObject.GetComponent<Button>().onClick.AddListener(() =>
			{
				SelectScenario(leaderboard);
			});
		}
	}

	public void SelectScenario(string p_leaderboardName)
	{
		SoundManager.instance.PlayUIClickSound();
		if (p_leaderboardName.Contains("kill"))
		{
			columnValues_text.text = LocalizationManager.Translate("txt-kills");
		}
		else if (p_leaderboardName.Contains("lost"))
		{
			columnValues_text.text = LocalizationManager.Translate("txt-lost");
		}
		else if (p_leaderboardName.Contains("captured"))
		{
			columnValues_text.text = LocalizationManager.Translate("txt-captured");
		}
		else
		{
			columnValues_text.text = LocalizationManager.Translate("txt-turns");
		}
		GetSteamLeaderboard(p_leaderboardName);
	}

	private void GetSteamLeaderboard(string p_leaderboardName)
	{
		_steamLeaderboard = new SteamLeaderboard(p_leaderboardName);
		SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard(_steamLeaderboard.name);
		CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult).Set(hAPICall);
	}

	private void OnLeaderboardFindResult(LeaderboardFindResult_t p_result, bool p_failure)
	{
		if (p_failure || p_result.m_bLeaderboardFound == 0)
		{
			Debug.LogError("Failed to find leaderboard.");
			return;
		}
		DownloadLocalPlayerEntry(p_result.m_hSteamLeaderboard);
		DownloadLeaderboardEntries(p_result.m_hSteamLeaderboard);
	}

	private void DownloadLeaderboardEntries(SteamLeaderboard_t p_leaderboardHandle)
	{
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(p_leaderboardHandle, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 50);
		CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded).Set(hAPICall);
	}

	private void DownloadLocalPlayerEntry(SteamLeaderboard_t p_leaderboardHandle)
	{
		CSteamID[] array = new CSteamID[1] { SteamManager.LocalPlayerSteamID };
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntriesForUsers(p_leaderboardHandle, array, array.Length);
		CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardLocalPlayerScoresDownloaded).Set(hAPICall);
	}

	private void OnLeaderboardLocalPlayerScoresDownloaded(LeaderboardScoresDownloaded_t p_result, bool p_failure)
	{
		if (p_failure)
		{
			Debug.LogError("Failed to download leaderboard scores for local player.");
			return;
		}
		if (p_result.m_cEntryCount > 0)
		{
			SteamUserStats.GetDownloadedLeaderboardEntry(p_result.m_hSteamLeaderboardEntries, 0, out var pLeaderboardEntry, null, 0);
			_steamLeaderboard.localPlayerEntry = new SteamLeaderboardEntry(SteamFriends.GetFriendPersonaName(SteamManager.LocalPlayerSteamID), pLeaderboardEntry.m_nScore, pLeaderboardEntry.m_nGlobalRank, p_isLocalClient: true);
		}
		RefreshLeaderboardEntries(_steamLeaderboard);
	}

	private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t p_result, bool p_failure)
	{
		if (p_failure)
		{
			Debug.LogError("Failed to download leaderboard scores.");
			return;
		}
		_steamLeaderboard.entries.Clear();
		for (int i = 0; i < p_result.m_cEntryCount; i++)
		{
			SteamUserStats.GetDownloadedLeaderboardEntry(p_result.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, null, 0);
			_steamLeaderboard.entries.Add(new SteamLeaderboardEntry(SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser), pLeaderboardEntry.m_nScore, pLeaderboardEntry.m_nGlobalRank, pLeaderboardEntry.m_steamIDUser == SteamManager.LocalPlayerSteamID));
		}
		RefreshLeaderboardEntries(_steamLeaderboard);
	}

	private void RefreshLeaderboardEntries(SteamLeaderboard p_steamLeaderboard)
	{
		Transform[] componentsInChildren = leaderboard_parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == leaderboard_parent))
			{
				Object.Destroy(transform.gameObject);
			}
		}
		if (p_steamLeaderboard.entries.Count == 0)
		{
			Object.Instantiate(leaderboardSlot_prefab, leaderboard_parent).SetNoEntry();
			return;
		}
		foreach (SteamLeaderboardEntry entry in p_steamLeaderboard.entries)
		{
			Object.Instantiate(leaderboardSlot_prefab, leaderboard_parent).Setup(entry);
		}
		if (p_steamLeaderboard.localPlayerEntry != null && p_steamLeaderboard.localPlayerEntry.rank > 50)
		{
			Object.Instantiate(leaderboardSlot_prefab, leaderboard_parent).Setup(p_steamLeaderboard.localPlayerEntry);
		}
	}
}
