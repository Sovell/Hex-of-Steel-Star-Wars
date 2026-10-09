using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TextMeshProUGUI _playerName_text;

	[SerializeField]
	private TextMeshProUGUI _rank_text;

	[SerializeField]
	private TextMeshProUGUI _score_text;

	public void SetNoEntry()
	{
		_playerName_text.text = LocalizationManager.Translate("txt-no entries");
		_playerName_text.gameObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
		_playerName_text.gameObject.GetComponent<RectTransform>().anchorMax = Vector2.one;
		_playerName_text.gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		_playerName_text.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
		_playerName_text.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		_rank_text.gameObject.SetActive(value: false);
		_score_text.gameObject.SetActive(value: false);
	}

	public void Setup(SteamLeaderboardEntry p_steamLeaderboardEntry)
	{
		_playerName_text.text = p_steamLeaderboardEntry.nickname;
		_rank_text.text = p_steamLeaderboardEntry.rank.ToString();
		_score_text.text = p_steamLeaderboardEntry.score.ToString();
		if (p_steamLeaderboardEntry.isLocalClient)
		{
			_playerName_text.color = MainMenu.yellowColor;
			_rank_text.color = MainMenu.yellowColor;
			_score_text.color = MainMenu.yellowColor;
		}
	}
}
