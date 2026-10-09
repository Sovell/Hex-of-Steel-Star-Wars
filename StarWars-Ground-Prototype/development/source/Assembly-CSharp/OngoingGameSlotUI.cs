using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OngoingGameSlotUI : MonoBehaviour
{
	[SerializeField]
	private Button _slotButton;

	[SerializeField]
	private TMP_Text _scenario_text;

	[SerializeField]
	private TMP_Text _turn_text;

	[SerializeField]
	private TMP_Text _activePlayerUsername_text;

	[SerializeField]
	private TMP_Text _playerCount_text;

	[SerializeField]
	private TMP_Text _hostUsername_text;

	[SerializeField]
	private TMP_Text _updatedAt_text;

	[SerializeField]
	private TMP_Text _createdAt_text;

	public void Setup(ServerDTOs.GameSessionDetails p_gameDetails, Action<ServerDTOs.GameSessionDetails> p_onClick)
	{
		name = p_gameDetails.Id.ToString();
		_slotButton.onClick.RemoveAllListeners();
		_slotButton.onClick.AddListener(() =>
		{
			p_onClick?.Invoke(p_gameDetails);
		});
		_scenario_text.text = p_gameDetails.Name;
		_turn_text.text = p_gameDetails.TurnNumber.ToString();
		_activePlayerUsername_text.text = (string.IsNullOrWhiteSpace(p_gameDetails.ActivePlayerUsername) ? LocalizationManager.Translate("username.unknown") : p_gameDetails.ActivePlayerUsername);
		_playerCount_text.text = $"{p_gameDetails.TakenSlotsCount} / {p_gameDetails.TotalSlotsCount}";
		_hostUsername_text.text = (string.IsNullOrWhiteSpace(p_gameDetails.HostUsername) ? LocalizationManager.Translate("username.unknown") : p_gameDetails.HostUsername);
		_createdAt_text.text = p_gameDetails.CreatedAtUtc.ToLocalTime().ToString("g", LocalizationManager.GetCultureInfo());
		_updatedAt_text.text = p_gameDetails.UpdatedAtUtc.ToLocalTime().ToString("g", LocalizationManager.GetCultureInfo());
	}
}
