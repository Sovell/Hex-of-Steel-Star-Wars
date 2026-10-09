using System;
using Newtonsoft.Json;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeRoomSlot : MonoBehaviour
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

	[Space]
	[SerializeField]
	private TooltipGO _modsTooltip;

	[SerializeField]
	private TooltipGO _settingsTooltip;

	public void Setup(RoomInfo p_roomInfo, Action<RoomInfo> p_onClick)
	{
		name = p_roomInfo.Name;
		_slotButton.onClick.RemoveAllListeners();
		_slotButton.onClick.AddListener(() =>
		{
			p_onClick?.Invoke(p_roomInfo);
		});
		_lockIcon.SetActive(!string.IsNullOrWhiteSpace((string)p_roomInfo.CustomProperties["Pwd"]));
		_scenarioName_text.text = (string)p_roomInfo.CustomProperties["Scn"];
		_playerCount_text.text = p_roomInfo.PlayerCount + " / " + p_roomInfo.MaxPlayers;
		_hostUsername_text.text = (string)p_roomInfo.CustomProperties["Hst"];
		_description_text.text = (string)p_roomInfo.CustomProperties["Desc"];
		if (string.IsNullOrWhiteSpace((string)p_roomInfo.CustomProperties["Mds"]))
		{
			_mods_text.text = "...";
			_modsTooltip.forcedText = string.Empty;
			_modsTooltip.enabled = false;
		}
		else
		{
			_mods_text.text = LocalizationManager.Translate("txt-yes");
			_modsTooltip.forcedText = p_roomInfo.CustomProperties["Mds"].ToString();
			_modsTooltip.enabled = true;
		}
		GameSettings gameSettings = JsonConvert.DeserializeObject<GameSettings>((string)p_roomInfo.CustomProperties["Sttgs"]);
		string text = LocalizationManager.Translate("tggl-fog of war") + ": " + gameSettings.fogOfWarON;
		text = text + "\n" + LocalizationManager.Translate("tggl-zone of control") + ": " + gameSettings.zoneOfControlON;
		text = text + "\n" + LocalizationManager.Translate("tggl-upkeep") + ": " + gameSettings.upkeepON;
		text = text + "\n" + LocalizationManager.Translate("tggl-policies") + ": " + gameSettings.policiesON;
		text = text + "\n" + LocalizationManager.Translate("tggl-manpower") + ": " + gameSettings.manpowerON;
		text = text + "\n" + LocalizationManager.Translate("tggl-units production time") + ": " + gameSettings.productionTakesTimeON;
		text = text + "\n" + LocalizationManager.Translate("tggl-realistic supplies") + ": " + gameSettings.realisticPlanesON;
		text = text + "\n" + LocalizationManager.Translate("tggl-research") + ": " + gameSettings.researchON;
		text = text + "\n" + LocalizationManager.Translate("tggl-unlimited supplies") + ": " + gameSettings.infiniteAmmoAndFuelON;
		text = text + "\n" + LocalizationManager.Translate("tggl-morale") + ": " + gameSettings.moraleON;
		text = text + "\n" + LocalizationManager.Translate("tggl-partisans") + ": " + gameSettings.partisansON;
		text = text + "\n" + LocalizationManager.Translate("txt-winter damage modifier") + ": x" + gameSettings.winterDamageValue;
		text = text + "\n" + LocalizationManager.Translate("txt-AI difficulty") + ": " + gameSettings.difficultyValue;
		text = text + "\n" + LocalizationManager.Translate("tggl-one more tile planes") + ": " + gameSettings.planeMoveAfterAttackON;
		text = text + "\n" + LocalizationManager.Translate("tggl-overrun") + ": " + gameSettings.overrunON;
		_settingsTooltip.forcedText = text;
	}
}
