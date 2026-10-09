using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyGO : MonoBehaviour
{
	public static DiplomacyGO instance;

	public Transform parent;

	public DiplomacySlotGO diplomacySlotPrefab;

	[Header("Faction creation")]
	public GameObject createFactionPanel;

	public TMP_InputField factionNameInputfield;

	[Header("Faction choice")]
	public TMP_Dropdown existingFactions_dropdown;

	[Header("Tooltip")]
	public TextMeshProUGUI tooltip_country_text;

	public Transform tooltip_atWar_parent;

	public Transform tooltip_alliances_parent;

	public Transform tooltip_truces_parent;

	public GameObject tooltipCountryPrefab;

	public Button createFaction_button;

	public GameObject diplomacyTooltip;

	private int _moneyNeeded;

	private Player _targetPlayer;

	private GameObject _confirmationWindow;

	private List<string> _existingFactions;

	private List<DiplomacySlotGO> _diplomacySlotsGO;

	private string _factionTarget;

	private int _factionChangeCost;

	private void Awake()
	{
		instance = this;
		_diplomacySlotsGO = new List<DiplomacySlotGO>();
	}

	private void Start()
	{
		createFaction_button.gameObject.SetActive(!GameData.Instance.map.IsCampaign);
	}

	private void OnEnable()
	{
		RefreshDiplomacy();
		SteamManager.SetRichPresenceStatus(SteamStatuses.DIPLOMACY);
		UIManager.instance.HandleMenuStacking(gameObject);
		TutorialManager.PerformAction(TutorialActions.OPEN_DIPLOMACY_MENU);
	}

	private void OnDisable()
	{
		_moneyNeeded = 0;
		_targetPlayer = null;
		diplomacyTooltip.SetActive(value: false);
		Object.Destroy(_confirmationWindow);
		SteamManager.SetRichPresenceStatus(SteamStatuses.DEFAULT);
	}

	public void ClosePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	public void TransferVP_Button()
	{
		ClosePanel();
		TilesManagerGO.instance.EnableTilesManagerPanel();
	}

	public void JoinFaction()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = _existingFactions.ElementAt(existingFactions_dropdown.value);
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(text))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-empty faction"));
			return;
		}
		if (text == TurnManager.currPlayer.Faction)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-empty faction"));
			return;
		}
		if (TurnManager.currPlayer.Faction.ToLower() != "neutral" && text.ToLower() != "neutral" && text != "Neutral")
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-need to be neutral"));
			return;
		}
		_factionTarget = text;
		_factionChangeCost = TurnManager.currPlayer.GetFactionChangeCost(_factionTarget);
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-future cost") + _factionChangeCost + LocalizationManager.Translate("info-diplomacy confirmation"), ConfirmJoinFaction);
	}

	public void ManageCreateFactionPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		createFactionPanel.SetActive(!createFactionPanel.activeSelf);
	}

	public void CreateFaction()
	{
		SoundManager.instance.PlayUIClickSound();
		if (string.IsNullOrWhiteSpace(factionNameInputfield.text) || string.IsNullOrEmpty(factionNameInputfield.text))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-empty faction"));
			return;
		}
		if (factionNameInputfield.text == TurnManager.currPlayer.Faction)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-empty faction"));
			return;
		}
		_factionTarget = factionNameInputfield.text.Substring(0, 1).ToUpper() + factionNameInputfield.text.Substring(1).ToLower();
		int num = 0;
		foreach (KeyValuePair<string, string> keyValuePair in LocalizationManager.activeLocale.keyValuePairs)
		{
			num++;
			if ((keyValuePair.Key == "Axis" || keyValuePair.Key == "Allies" || keyValuePair.Key == "Communists" || keyValuePair.Key == "Neutral") && keyValuePair.Value.ToLower().Contains(_factionTarget.ToLower()))
			{
				_factionTarget = keyValuePair.Key;
				break;
			}
			if (num >= 100)
			{
				break;
			}
		}
		if (TurnManager.currPlayer.Faction.ToLower() != "neutral" && factionNameInputfield.text.ToLower() != "neutral" && _factionTarget != "Neutral")
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-need to be neutral"));
			return;
		}
		_factionChangeCost = TurnManager.currPlayer.GetFactionChangeCost(_factionTarget);
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-future cost") + _factionChangeCost + LocalizationManager.Translate("info-diplomacy confirmation"), ConfirmJoinFaction);
	}

	private void ConfirmJoinFaction()
	{
		if (TurnManager.currPlayer.MoneyAmount - _factionChangeCost < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-going to war not enough funds 2"));
			return;
		}
		TurnManager.currPlayer.RemoveMoney(_factionChangeCost, p_sendRPC: false);
		int num = ((TurnManager.currPlayer.PlayersAtWar.Count > 0) ? (_factionChangeCost / TurnManager.currPlayer.PlayersAtWar.Count) : 0);
		if (num != 0)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (TurnManager.currPlayer.IsAtWarWith(listOfPlayer))
				{
					listOfPlayer.AddMoney(num, p_sendRPC: true);
					new Notification(NotificationTypes.MONEY_TRANSFER_NEUTRALITY, listOfPlayer.Name, 0f, 0f, string.Empty, p_isImportant: true)
					{
						SenderName = TurnManager.currPlayer.Name,
						Amount = num
					};
				}
			}
		}
		TurnManager.currPlayer.ChangeFaction(_factionTarget, p_getNewAllianceWars: true, p_sendRPC: true);
		RefreshDiplomacy();
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		TurnManager.instance.DiplomacyChanged(factionNameInputfield);
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		if ((bool)StoreGO.instance && StoreGO.instance.enabled)
		{
			StoreGO.instance.CloseStorePanel();
			StoreGO.instance.gameObject.SetActive(value: true);
		}
	}

	private void RefreshDiplomacy()
	{
		if (_diplomacySlotsGO.Count != GameData.Instance.listOfPlayers.Count)
		{
			for (int i = _diplomacySlotsGO.Count; i < GameData.Instance.listOfPlayers.Count; i++)
			{
				DiplomacySlotGO diplomacySlotGO = Object.Instantiate(diplomacySlotPrefab, parent);
				_diplomacySlotsGO.Add(diplomacySlotGO);
				diplomacySlotGO.transform.localScale = Vector3.one;
			}
		}
		for (int j = 0; j < GameData.Instance.listOfPlayers.Count; j++)
		{
			Player player = GameData.Instance.listOfPlayers[j];
			DiplomacySlotGO diplomacySlotGO2 = _diplomacySlotsGO[j];
			_diplomacySlotsGO[j].SetUpDiplomacySlot(player);
			_diplomacySlotsGO[j].sendMoney_button.onClick.AddListener(() =>
			{
				SendMoney(diplomacySlotGO2);
			});
			_diplomacySlotsGO[j].kick_button.onClick.AddListener(() =>
			{
				KickFromFaction(diplomacySlotGO2);
			});
			_diplomacySlotsGO[j].status_button.onClick.AddListener(() =>
			{
				ManageRelations(diplomacySlotGO2);
			});
			_diplomacySlotsGO[j].inviteToAlliance_button.onClick.AddListener(() =>
			{
				InviteToAlliance(diplomacySlotGO2);
			});
			_diplomacySlotsGO[j].swapPlayerConfiguration_button.onClick.AddListener(() =>
			{
				SwapPlayerConfiguration(player);
			});
		}
		_existingFactions = new List<string>();
		existingFactions_dropdown.ClearOptions();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (!_existingFactions.Contains(listOfPlayer.Faction))
			{
				_existingFactions.Add(listOfPlayer.Faction);
				existingFactions_dropdown.options.Add(new TMP_Dropdown.OptionData
				{
					text = LocalizationManager.GetLocalizedFaction(listOfPlayer.Faction)
				});
			}
		}
		existingFactions_dropdown.RefreshShownValue();
	}

	private void SwapPlayerConfiguration(Player p_player)
	{
		SoundManager.instance.PlayUIClickSound();
		if (p_player.IsComputer)
		{
			AI.instance.PlayerControlAboutToChange(p_player);
		}
		p_player.LocalPassword = null;
		p_player.IsComputer = !p_player.IsComputer;
		if (GameData.Instance.IsIronManModeON(out var _))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-ironman mode OFF"));
		}
		GameData.Instance.violatedIronmanRule = true;
		GameData.Instance.SetIronManMode(p_value: false);
		if (TurnManager.humanPlayer == p_player && p_player.IsComputer)
		{
			TurnManager.humanPlayer = null;
		}
		bool isAIOnly = true;
		bool flag = false;
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.IsPlaying)
			{
				flag = true;
			}
			if (!listOfPlayer.IsComputer)
			{
				isAIOnly = false;
				if (flag)
				{
					TurnManager.humanPlayer = listOfPlayer;
				}
			}
		}
		MapGO.IsAIOnly = isAIOnly;
		RefreshDiplomacy();
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
	}

	public void InviteToAlliance(DiplomacySlotGO p_diplomacySlotGO)
	{
		SoundManager.instance.PlayUIClickSound();
		if (TurnManager.currPlayer.Faction == "Neutral")
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invite to alliance while neutral"));
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name != p_diplomacySlotGO.player.Name)
			{
				continue;
			}
			if (!listOfPlayer.IsComputer)
			{
				foreach (Notification notification in GameData.Instance.map.Notifications)
				{
					if (notification.Type == NotificationTypes.ALLIANCE_INVITATION && notification.SenderName == TurnManager.currPlayer.Name)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("info-alliance proposition already sent"));
						return;
					}
				}
				new Notification(NotificationTypes.ALLIANCE_INVITATION, listOfPlayer.Name, 0f, 0f, string.Empty, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
				UIManager.ShowMessage(LocalizationManager.Translate("info-faction invitation sent"));
			}
			else if (listOfPlayer.Country == "Neutral")
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invite alliance"));
			}
			else if (listOfPlayer.IsDefeated())
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invite alliance vanquished"));
			}
			else
			{
				int allianceInvitationCost = TurnManager.currPlayer.GetAllianceInvitationCost(listOfPlayer);
				if (TurnManager.currPlayer.MoneyAmount >= allianceInvitationCost)
				{
					_targetPlayer = listOfPlayer;
					_moneyNeeded = allianceInvitationCost;
					_confirmationWindow = UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation invite cost") + _moneyNeeded, InviteToAllianceConfirmation);
				}
				else
				{
					UIManager.ShowMessage(LocalizationManager.Translate("error-not enough funds invite") + allianceInvitationCost);
				}
			}
			break;
		}
	}

	private void InviteToAllianceConfirmation()
	{
		SoundManager.instance.PlayUIClickSound();
		TurnManager.currPlayer.RemoveMoney(_moneyNeeded, p_sendRPC: true);
		_targetPlayer.AddMoney(_moneyNeeded, p_sendRPC: true);
		_targetPlayer.ChangeFaction(TurnManager.currPlayer.Faction, p_getNewAllianceWars: true, p_sendRPC: true);
		MultiplayerManager.RPC("RPC_AllianceMade", _targetPlayer.Name, TurnManager.currPlayer.Name);
		SteamManager.UnlockAchievement("Strength in numbers");
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		RefreshDiplomacy();
		_moneyNeeded = 0;
		_targetPlayer = null;
		TutorialManager.PerformAction(TutorialActions.INVITE_TO_FACTION);
	}

	private void DeclareWarConfirmation()
	{
		SoundManager.instance.PlayUIClickSound();
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Declare war"));
		if (TurnManager.currPlayer.HasTruceWith(_targetPlayer))
		{
			_targetPlayer.AddMoney(TurnManager.currPlayer.turnIncome + _targetPlayer.turnIncome, p_sendRPC: true);
			TurnManager.currPlayer.RemoveMoney(TurnManager.currPlayer.turnIncome + _targetPlayer.turnIncome, p_sendRPC: true);
		}
		if (TurnManager.currPlayer.Faction == "Neutral")
		{
			TurnManager.currPlayer.ChangeFaction(TurnManager.currPlayer.Country, p_getNewAllianceWars: false, p_sendRPC: true);
		}
		if (_targetPlayer.Country != "Neutral")
		{
			if (_targetPlayer.Faction == "Neutral")
			{
				if (_targetPlayer.Ideology == TurnManager.currPlayer.Ideology && _targetPlayer.Ideology != "Neutral" && (TurnManager.currPlayer.Faction == "Axis" || TurnManager.currPlayer.Faction == "Allies" || TurnManager.currPlayer.Faction == "Communists"))
				{
					_targetPlayer.ChangeFaction(_targetPlayer.Country, p_getNewAllianceWars: true, p_sendRPC: true);
				}
				else if (TurnManager.currPlayer.Faction == "Axis")
				{
					_targetPlayer.ChangeFaction("Allies", p_getNewAllianceWars: false, p_sendRPC: true);
				}
				else if (TurnManager.currPlayer.Faction == "Allies")
				{
					_targetPlayer.ChangeFaction("Axis", p_getNewAllianceWars: false, p_sendRPC: true);
				}
				else if (_targetPlayer.Faction == "Neutral")
				{
					if (_targetPlayer.Ideology == "Fascism" || _targetPlayer.Ideology == "National Socialism")
					{
						_targetPlayer.ChangeFaction("Axis", p_getNewAllianceWars: false, p_sendRPC: true);
					}
					else if (_targetPlayer.Ideology == "Democracy" || _targetPlayer.Ideology == "Neutral")
					{
						_targetPlayer.ChangeFaction("Allies", p_getNewAllianceWars: false, p_sendRPC: true);
					}
					else if (_targetPlayer.Ideology == "Communism")
					{
						_targetPlayer.ChangeFaction("Communists", p_getNewAllianceWars: false, p_sendRPC: true);
					}
					else
					{
						_targetPlayer.ChangeFaction((Random.Range(0, 2) == 0) ? "Axis" : "Allies", p_getNewAllianceWars: false, p_sendRPC: true);
					}
				}
			}
			else if (_targetPlayer.Faction == TurnManager.currPlayer.Faction)
			{
				string text = null;
				if (TurnManager.currPlayer.Faction == "Axis")
				{
					text = "Allies";
				}
				else if (TurnManager.currPlayer.Faction == "Allies")
				{
					text = "Axis";
				}
				else if (TurnManager.currPlayer.Faction == "Communists")
				{
					text = "Allies";
				}
				if (!string.IsNullOrEmpty(text))
				{
					foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
					{
						if (_targetPlayer.IsAtWarWith(listOfPlayer) && listOfPlayer.Faction == text)
						{
							text = _targetPlayer.Faction + " betrayal";
							break;
						}
					}
					_targetPlayer.ChangeFaction(text, p_getNewAllianceWars: true, p_sendRPC: true);
				}
			}
		}
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer2.Faction == TurnManager.currPlayer.Faction && TurnManager.currPlayer.IsAlliedWith(listOfPlayer2))
			{
				listOfPlayer2.AddWarWith(_targetPlayer, listOfPlayer2 == TurnManager.currPlayer, p_sendRPC: true);
			}
		}
		MultiplayerManager.RPC("RPC_WarDeclaration", TurnManager.currPlayer.Name, _targetPlayer.Name);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		RefreshDiplomacy();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		_targetPlayer = null;
		SteamManager.UnlockAchievement("Warmonger");
		TutorialManager.PerformAction(TutorialActions.DECLARE_WAR);
	}

	public void ManageRelations(DiplomacySlotGO p_diplomacySlotGO)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name != p_diplomacySlotGO.player.Name)
			{
				continue;
			}
			if (!TurnManager.currPlayer.PlayersAtWar.Contains(listOfPlayer.Name))
			{
				_targetPlayer = listOfPlayer;
				if (TurnManager.currPlayer.HasTruceWith(listOfPlayer))
				{
					if (TurnManager.currPlayer.MoneyAmount < TurnManager.currPlayer.turnIncome + _targetPlayer.turnIncome)
					{
						UIManager.ShowMessage(LocalizationManager.Translate("info-going to war not enough funds 1") + (TurnManager.currPlayer.turnIncome + _targetPlayer.turnIncome) + "." + LocalizationManager.Translate("info-going to war not enough funds 2"));
						break;
					}
					_confirmationWindow = UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-you have a truce") + LocalizationManager.GetLocalizedCountry(listOfPlayer.Country).Replace(".", "") + "." + LocalizationManager.Translate("info-going to war cost") + (TurnManager.currPlayer.turnIncome + _targetPlayer.turnIncome) + LocalizationManager.Translate("txt-funds") + "." + LocalizationManager.Translate("info-are you sure war"), DeclareWarConfirmation);
				}
				else
				{
					_confirmationWindow = UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation war") + LocalizationManager.GetLocalizedCountry(listOfPlayer.Country).Replace(".", "") + " ?", DeclareWarConfirmation);
				}
				break;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < GameData.Instance.map.SizeX; i++)
			{
				for (int j = 0; j < GameData.Instance.map.SizeY; j++)
				{
					Tile tile = GameData.Instance.map.TilesTable[i, j];
					if (tile.ownerPlayer == null)
					{
						continue;
					}
					if (tile.ownerPlayer.Country == TurnManager.currPlayer.Country)
					{
						if (tile.MoneyProductionPerTurn > 0)
						{
							num += tile.MoneyProductionPerTurn;
						}
					}
					else if (tile.ownerPlayer.Country == listOfPlayer.Country && tile.MoneyProductionPerTurn > 0)
					{
						num2 += tile.MoneyProductionPerTurn;
					}
				}
			}
			float num3 = 1f;
			if (TurnManager.currPlayer.HasPolicy(Policies.Type.Good_diplomats))
			{
				num3 = 0.7f;
			}
			int num4 = num2 * 2 + num;
			num4 = (int)((float)num4 * num3);
			if (TurnManager.currPlayer.MoneyAmount >= num4)
			{
				_moneyNeeded = num4;
				_targetPlayer = listOfPlayer;
				_confirmationWindow = UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-future cost") + _moneyNeeded + LocalizationManager.Translate("info-diplomacy confirmation"), MakePeaceConfirmation);
			}
			else
			{
				UIManager.ShowMessage(LocalizationManager.Translate("info-make peace cost") + num4);
			}
			break;
		}
	}

	private void MakePeaceConfirmation()
	{
		SoundManager.instance.PlayUIClickSound();
		SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Sign peace"));
		TurnManager.currPlayer.RemoveMoney(_moneyNeeded, p_sendRPC: true);
		_targetPlayer.AddMoney(_moneyNeeded, p_sendRPC: true);
		TurnManager.currPlayer.RemoveWarWith(_targetPlayer, p_sendRPC: true);
		TurnManager.currPlayer.AddTruceWith(_targetPlayer, p_sendRPC: true);
		MultiplayerManager.RPC("RPC_SignedPeace", TurnManager.currPlayer.Name, _targetPlayer.Name);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		SteamManager.UnlockAchievement("Gandhi");
		RefreshDiplomacy();
		_moneyNeeded = 0;
		_targetPlayer = null;
	}

	public void KickFromFaction(DiplomacySlotGO p_diplomacySlotGO)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (string item in new List<string>(TurnManager.currPlayer.AlliedPlayers))
		{
			if (GameData.Instance.TryFindPlayerByName(item, out var o_player))
			{
				p_diplomacySlotGO.player.RemoveAllianceWith(o_player, p_sendRPC: true);
			}
		}
		p_diplomacySlotGO.player.RemoveAllianceWith(TurnManager.currPlayer, p_sendRPC: true);
		p_diplomacySlotGO.player.ChangeFaction(p_diplomacySlotGO.player.Country, p_getNewAllianceWars: false, p_sendRPC: true);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.UnselectUnit();
		}
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		RefreshDiplomacy();
	}

	public void SendMoney(DiplomacySlotGO p_diplomacySlotGO)
	{
		SoundManager.instance.PlayUIClickSound();
		int num = int.Parse(p_diplomacySlotGO.sendMoney_inputfield.text);
		if (num > 0)
		{
			SendMoneyTo(p_diplomacySlotGO.player.Name, num);
			RefreshDiplomacy();
		}
	}

	private void SendMoneyTo(string p_playerName, int p_amount)
	{
		if (TurnManager.currPlayer.MoneyAmount - p_amount < 0)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-diplomacy not enough funds"));
			return;
		}
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer.Name == p_playerName)
			{
				new Notification(NotificationTypes.MONEY_TRANSFER, listOfPlayer.Name, 0f, 0f, string.Empty, p_isImportant: true)
				{
					Amount = p_amount,
					SenderName = TurnManager.currPlayer.Name
				};
				listOfPlayer.AddMoney(p_amount, p_sendRPC: true);
				TurnManager.currPlayer.RemoveMoney(p_amount, p_sendRPC: true);
				SteamManager.UnlockAchievement("The bribe");
				break;
			}
		}
	}
}
