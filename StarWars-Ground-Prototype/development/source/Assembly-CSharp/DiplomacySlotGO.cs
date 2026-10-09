using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacySlotGO : MonoBehaviour
{
	[SerializeField]
	private Image country_image;

	[SerializeField]
	private Image status_image;

	[SerializeField]
	private Image playerSwapConfiguration_image;

	[SerializeField]
	private TextMeshProUGUI countryName_text;

	[SerializeField]
	private TextMeshProUGUI ideology_text;

	[SerializeField]
	private TextMeshProUGUI faction_text;

	public Button status_button;

	public Button sendMoney_button;

	public Button kick_button;

	public Button inviteToAlliance_button;

	public Button swapPlayerConfiguration_button;

	public TMP_InputField sendMoney_inputfield;

	[SerializeField]
	private Sprite war_sprite;

	[SerializeField]
	private Sprite peace_sprite;

	[SerializeField]
	private Sprite _human_sprite;

	[SerializeField]
	private Sprite _AI_sprite;

	[SerializeField]
	private Transform money_parent;

	public Player player;

	[Header("Colors")]
	[SerializeField]
	private Color normal_color;

	[SerializeField]
	private Color currentPlayer_color;

	[SerializeField]
	private Color allied_color;

	[SerializeField]
	private Color enemy_color;

	public void SetUpDiplomacySlot(Player p_player)
	{
		player = p_player;
		name = p_player.Name;
		countryName_text.text = LocalizationManager.GetLocalizedCountry(p_player.Country).Replace(".", "");
		ideology_text.text = LocalizationManager.GetLocalizedIdeology(p_player.Ideology);
		faction_text.text = LocalizationManager.GetLocalizedFaction(p_player.Faction);
		country_image.sprite = Utils.GetFlagSprite(p_player.Country);
		status_button.gameObject.SetActive(p_player != TurnManager.currPlayer && !GameData.Instance.map.IsCampaign);
		sendMoney_button.gameObject.SetActive(p_player != TurnManager.currPlayer);
		money_parent.gameObject.SetActive(p_player != TurnManager.currPlayer);
		swapPlayerConfiguration_button.gameObject.SetActive(!GameData.Instance.isPBEM && !MultiplayerManager.IsRealtimeMultiplayer() && !GameData.Instance.offlinePBEM);
		playerSwapConfiguration_image.sprite = (p_player.IsComputer ? _AI_sprite : _human_sprite);
		if (GameData.Instance.map.IsCampaign)
		{
			kick_button.gameObject.SetActive(value: false);
			inviteToAlliance_button.gameObject.SetActive(value: false);
		}
		else if (TurnManager.currPlayer.PlayersAtWar.Contains(p_player.Name))
		{
			countryName_text.color = enemy_color;
			status_image.sprite = peace_sprite;
			kick_button.gameObject.SetActive(value: false);
			inviteToAlliance_button.gameObject.SetActive(value: false);
		}
		else
		{
			if (TurnManager.currPlayer == p_player)
			{
				countryName_text.color = currentPlayer_color;
				kick_button.gameObject.SetActive(value: false);
			}
			else if (TurnManager.currPlayer.AlliedPlayers.Contains(p_player.Name))
			{
				countryName_text.color = allied_color;
				kick_button.gameObject.SetActive(value: true);
			}
			else
			{
				countryName_text.color = normal_color;
				kick_button.gameObject.SetActive(value: false);
			}
			status_image.sprite = war_sprite;
			inviteToAlliance_button.gameObject.SetActive(p_player != TurnManager.currPlayer && !GameData.Instance.map.IsCampaign && !TurnManager.currPlayer.AlliedPlayers.Contains(player.Name));
		}
		if (p_player.Country == "Neutral")
		{
			inviteToAlliance_button.gameObject.SetActive(value: false);
		}
		status_button.onClick.RemoveAllListeners();
		sendMoney_button.onClick.RemoveAllListeners();
		inviteToAlliance_button.onClick.RemoveAllListeners();
		swapPlayerConfiguration_button.onClick.RemoveAllListeners();
		sendMoney_inputfield.SetTextWithoutNotify("");
	}
}
