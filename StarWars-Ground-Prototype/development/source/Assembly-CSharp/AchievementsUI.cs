using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;

public class AchievementsUI : MonoBehaviour
{
	private struct AchievementData
	{
		public string apiName;

		public string displayName;

		public string description;

		public bool unlocked;
	}

	[Header("Setup")]
	[SerializeField]
	private AchievementElementUI _prefab;

	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private TextMeshProUGUI _achievements_text;

	private List<AchievementData> _achievements;

	public void OpenMenu()
	{
		if (!SteamAPI.IsSteamRunning())
		{
			UIManager.ShowMessage("Steam isn't running. Cannot load Steam achievements.");
		}
		else
		{
			gameObject.SetActive(value: true);
		}
	}

	public void CloseMenu()
	{
		gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		LoadAchievementData();
		UpdateAchievementList();
	}

	private void LoadAchievementData()
	{
		_achievements = new List<AchievementData>();
		uint numAchievements = SteamUserStats.GetNumAchievements();
		for (uint num = 0u; num < numAchievements; num++)
		{
			string achievementName = SteamUserStats.GetAchievementName(num);
			string achievementDisplayAttribute = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "name");
			string achievementDisplayAttribute2 = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "desc");
			SteamUserStats.GetAchievement(achievementName, out var pbAchieved);
			_achievements.Add(new AchievementData
			{
				apiName = achievementName,
				displayName = achievementDisplayAttribute,
				description = achievementDisplayAttribute2,
				unlocked = pbAchieved
			});
		}
	}

	private void UpdateAchievementList()
	{
		foreach (Transform item in _transform)
		{
			if (item != _transform)
			{
				Object.Destroy(item.gameObject);
			}
		}
		_achievements = _achievements.OrderByDescending((AchievementData x) => !x.unlocked).ToList();
		int num = 0;
		foreach (AchievementData achievement in _achievements)
		{
			Object.Instantiate(_prefab, _transform).Setup(achievement.unlocked, achievement.displayName, achievement.description);
			if (achievement.unlocked)
			{
				num++;
			}
		}
		_achievements_text.text = LocalizationManager.Translate("txt-achievements unlocked") + num + " / " + _achievements.Count();
	}
}
