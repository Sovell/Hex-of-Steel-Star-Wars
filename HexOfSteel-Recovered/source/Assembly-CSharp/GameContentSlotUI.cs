using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameContentSlotUI : MonoBehaviour
{
	[SerializeField]
	private Button _slotButton;

	[Space]
	[SerializeField]
	private Image _nationFlag_image;

	[Space]
	[SerializeField]
	private TMP_Text _nationName_text;

	[SerializeField]
	private TMP_Text _username_text;

	[SerializeField]
	private TMP_Text _configuration_text;

	[Space]
	[SerializeField]
	private Button _join_button;

	[SerializeField]
	private Button _leave_button;

	[SerializeField]
	private Button _kick_button;

	public ServerDTOs.PlayerSlotItem SlotData { get; private set; }

	public void Setup(ServerDTOs.GameSessionDetails p_gameSessionDetails, ServerDTOs.PlayerSlotItem p_slot, Action<ServerDTOs.PlayerSlotItem> p_onJoinClicked, Action<ServerDTOs.PlayerSlotItem> p_onLeaveClicked, Action<ServerDTOs.PlayerSlotItem> p_onKickClicked)
	{
		SlotData = p_slot;
		name = $"Slot: {p_slot.index} - Nation: {p_slot.nation} - Username: {p_slot.username}";
		_slotButton.interactable = !p_slot.isAIcontrolled && (string.IsNullOrEmpty(p_slot.username) || p_slot.username == PlayerSettings.Instance.Username);
		_nationFlag_image.sprite = Utils.GetFlagSprite(p_slot.nation);
		_nationName_text.text = p_slot.nation;
		_username_text.text = (string.IsNullOrWhiteSpace(p_slot.username) ? "..." : p_slot.username);
		if (string.IsNullOrWhiteSpace(p_slot.username) && _username_text.TryGetComponent<Button>(out var component))
		{
			component.interactable = false;
		}
		_configuration_text.text = (p_slot.isAIcontrolled ? LocalizationManager.Translate("txt-ai") : LocalizationManager.Translate("txt-human"));
		_join_button.gameObject.SetActive(!p_slot.isAIcontrolled && string.IsNullOrEmpty(p_slot.username));
		_leave_button.gameObject.SetActive(!p_slot.isAIcontrolled && p_slot.username == PlayerSettings.Instance.Username);
		_kick_button.gameObject.SetActive(!p_slot.isAIcontrolled && !string.IsNullOrEmpty(p_slot.username) && p_slot.username != PlayerSettings.Instance.Username && p_gameSessionDetails.HostUsername == PlayerSettings.Instance.Username);
		_join_button.onClick.RemoveAllListeners();
		_join_button.onClick.AddListener(() =>
		{
			p_onJoinClicked?.Invoke(p_slot);
		});
		_leave_button.onClick.RemoveAllListeners();
		_leave_button.onClick.AddListener(() =>
		{
			p_onLeaveClicked?.Invoke(p_slot);
		});
		_kick_button.onClick.RemoveAllListeners();
		_kick_button.onClick.AddListener(() =>
		{
			p_onKickClicked?.Invoke(p_slot);
		});
	}
}
