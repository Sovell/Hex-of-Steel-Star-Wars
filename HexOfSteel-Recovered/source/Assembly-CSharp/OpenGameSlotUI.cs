using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenGameSlotUI : MonoBehaviour
{
	[SerializeField]
	private Button _slotButton;

	[Space]
	[SerializeField]
	private GameObject _lockIcon;

	[Space]
	[SerializeField]
	private TMP_Text _scenarioName_text;

	[SerializeField]
	private TMP_Text _playerCount_text;

	[SerializeField]
	private TMP_Text _hostUsername_text;

	[SerializeField]
	private TMP_Text _description_text;

	[SerializeField]
	private TMP_Text _mods_text;

	[SerializeField]
	private TMP_Text _createdAt_text;

	[SerializeField]
	private TooltipGO _modsTooltip;

	public void Setup(ServerDTOs.GameSessionDetails p_gameDetails, Action<ServerDTOs.GameSessionDetails> p_onClick)
	{
		name = p_gameDetails.Id.ToString();
		_slotButton.onClick.RemoveAllListeners();
		_slotButton.onClick.AddListener(() =>
		{
			p_onClick?.Invoke(p_gameDetails);
		});
		_lockIcon.SetActive(p_gameDetails.IsPasswordProtected);
		_scenarioName_text.text = p_gameDetails.Name;
		_hostUsername_text.text = (string.IsNullOrWhiteSpace(p_gameDetails.HostUsername) ? LocalizationManager.Translate("username.unknown") : p_gameDetails.HostUsername);
		_playerCount_text.text = $"{p_gameDetails.TakenSlotsCount} / {p_gameDetails.TotalSlotsCount}";
		_description_text.text = p_gameDetails.Description;
		_createdAt_text.text = p_gameDetails.CreatedAtUtc.ToLocalTime().ToString("g", LocalizationManager.GetCultureInfo());
		bool flag = p_gameDetails.Mods != null && p_gameDetails.Mods.Count > 0;
		_mods_text.text = (flag ? LocalizationManager.Translate("txt-yes") : "...");
		if (flag)
		{
			_modsTooltip.forcedText = string.Join("\n", p_gameDetails.Mods);
		}
		else
		{
			UnityEngine.Object.Destroy(_modsTooltip);
		}
	}
}
