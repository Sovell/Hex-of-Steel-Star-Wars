using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotMapEditor : MonoBehaviour
{
	[Header("Setup")]
	public Toggle playingFirst_toggle;

	public Toggle AI_toggle;

	public Button info_button;

	public Button playerSelection_button;

	public Button removePlayer_button;

	public TMP_Dropdown country_dropdown;

	public TMP_Dropdown faction_dropdown;

	public TMP_Dropdown ideology_dropdown;

	public TMP_InputField playOrder_inputfield;

	public TMP_InputField startIncome_inputfield;

	public TMP_InputField fixedIncome_inputfield;

	public TMP_InputField startHQpoints_inputfield;

	public Color selected_color;

	public Color unselected_color;

	private Player _player;

	public void Setup(Player p_player)
	{
		_player = p_player;
		name = _player.Name;
		RefreshAIplaying();
		RefreshPlayingFirst();
		RefreshSelectedPlayer();
	}

	public void RefreshSelectedPlayer()
	{
		ColorBlock colors = playerSelection_button.colors;
		colors.normalColor = ((_player == TurnManager.currPlayer) ? selected_color : unselected_color);
		colors.selectedColor = ((_player == TurnManager.currPlayer) ? selected_color : unselected_color);
		playerSelection_button.colors = colors;
	}

	public void RefreshPlayingFirst()
	{
		playingFirst_toggle.SetIsOnWithoutNotify(_player.IsPlayingFirst);
	}

	public void RefreshAIplaying()
	{
		AI_toggle.SetIsOnWithoutNotify(_player.IsComputer);
	}
}
