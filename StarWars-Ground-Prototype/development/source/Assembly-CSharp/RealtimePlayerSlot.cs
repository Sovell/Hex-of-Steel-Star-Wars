using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimePlayerSlot : MonoBehaviour
{
	[SerializeField]
	private GameObject _host_icon;

	[SerializeField]
	private Button _kick_button;

	[Space]
	[SerializeField]
	private Image _country_flag;

	[Space]
	[SerializeField]
	private TMP_Text _username_text;

	[Space]
	[SerializeField]
	private Toggle _ready_toggle;

	[SerializeField]
	private Toggle _playing_first_toggle;

	[Space]
	[SerializeField]
	private Button _playAs_button;

	[SerializeField]
	private Button _stopPlayingAs_button;

	public void Setup(Player p_player, Photon.Realtime.Player p_photonPlayer, Action<Photon.Realtime.Player> p_onKickButtonClicked)
	{
		if (p_player == null && p_photonPlayer == null)
		{
			Debug.LogError("Both player and photon player are null in RealtimePlayerSlot.Setup");
			return;
		}
		_host_icon.SetActive(value: false);
		_kick_button.gameObject.SetActive(value: false);
		_ready_toggle.gameObject.SetActive(value: false);
		_playing_first_toggle.gameObject.SetActive(value: false);
		_playAs_button.gameObject.SetActive(value: false);
		_stopPlayingAs_button.gameObject.SetActive(value: false);
		if (p_player == null)
		{
			name = p_photonPlayer.NickName;
			_username_text.text = p_photonPlayer.NickName;
			_country_flag.sprite = Utils.GetFlagSprite("None");
			return;
		}
		name = p_player.Name;
		_ready_toggle.gameObject.SetActive(value: true);
		_country_flag.sprite = Utils.GetFlagSprite(p_player.Country);
		_playing_first_toggle.gameObject.SetActive(value: true);
		_playing_first_toggle.SetIsOnWithoutNotify(p_player.IsPlayingFirst);
		_playing_first_toggle.interactable = PhotonNetwork.LocalPlayer.IsMasterClient;
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			_kick_button.onClick.RemoveAllListeners();
			_kick_button.onClick.AddListener(() =>
			{
				p_onKickButtonClicked?.Invoke(p_photonPlayer);
			});
			_playing_first_toggle.onValueChanged.RemoveAllListeners();
			_playing_first_toggle.onValueChanged.AddListener((bool isOn) =>
			{
				OnPlayingFirstChanged(p_player);
			});
		}
		if (p_player.IsComputer)
		{
			_username_text.text = LocalizationManager.Translate("txt-ai");
			_country_flag.sprite = Utils.GetFlagSprite(p_player.Country);
			_ready_toggle.interactable = false;
			_ready_toggle.SetIsOnWithoutNotify(value: true);
			if (_username_text.TryGetComponent<UsernameToProfile>(out var component))
			{
				UnityEngine.Object.Destroy(component);
			}
			return;
		}
		if (p_photonPlayer == null)
		{
			_username_text.text = LocalizationManager.Translate("txt-waiting for human player");
			_ready_toggle.interactable = false;
			_ready_toggle.SetIsOnWithoutNotify(value: false);
			_playAs_button.gameObject.SetActive(value: true);
			_playAs_button.onClick.RemoveAllListeners();
			_playAs_button.onClick.AddListener(() =>
			{
				RequestPlayAs(p_player);
			});
			if (_username_text.TryGetComponent<UsernameToProfile>(out var component2))
			{
				UnityEngine.Object.Destroy(component2);
			}
			return;
		}
		_username_text.text = p_photonPlayer.NickName;
		_host_icon.SetActive(p_photonPlayer.IsMasterClient);
		_kick_button.gameObject.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient && p_photonPlayer != PhotonNetwork.LocalPlayer);
		_ready_toggle.interactable = p_photonPlayer.IsLocal;
		_ready_toggle.SetIsOnWithoutNotify(p_photonPlayer.CustomProperties.TryGetValue("Rdy", out var value) && (bool)value);
		if (p_photonPlayer.IsLocal)
		{
			_stopPlayingAs_button.gameObject.SetActive(value: true);
			_stopPlayingAs_button.onClick.RemoveAllListeners();
			_stopPlayingAs_button.onClick.AddListener(() =>
			{
				OnStopPlayingAs(p_player);
			});
			_ready_toggle.onValueChanged.RemoveAllListeners();
			_ready_toggle.onValueChanged.AddListener((bool isOn) =>
			{
				OnReadyChanged();
			});
		}
	}

	private void OnReadyChanged()
	{
		SoundManager.instance.PlayUIClickSound();
		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{ "Rdy", _ready_toggle.isOn } });
	}

	private void OnPlayingFirstChanged(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {{ "Frst", p_player.Name } });
	}

	private void RequestPlayAs(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{ "Nm", p_player.Name } });
	}

	private void OnStopPlayingAs(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
		{
			"Nm",
			string.Empty
		} });
	}
}
