using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public enum SyncActions
	{
		UNIT_DISBAND,
		PLAYER_ADD_UNIT,
		PLAYER_PURCHASE_UNIT,
		PLAYER_REMOVE_UNIT,
		PLAYER_DRAWINGS,
		PLAYER_RESEARCH,
		PLAYER_DIPLOMACY,
		PLAYER_FACTION_CHANGE,
		PLAYER_ADD_ALLIANCE,
		PLAYER_REMOVE_ALLIANCE,
		PLAYER_ADD_WAR,
		PLAYER_REMOVE_WAR,
		PLAYER_ADD_TRUCE,
		PLAYER_REMOVE_TRUCE,
		PLAYER_PURCHASE_POLICY,
		PLAYER_ADD_POLICY,
		PLAYER_REMOVE_POLICY,
		PLAYER_ADD_MONEY,
		PLAYER_REMOVE_MONEY,
		PLAYER_ADD_MANPOWER,
		PLAYER_REMOVE_MANPOWER,
		PLAYER_ADD_HQ_POINTS,
		PLAYER_REMOVE_HQ_POINTS,
		PLAYER_AUTOMATIC_SUPPLIES
	}

	public const string VERSION = "8.4.14";

	public const int CHUNK_SIZE = 16384;

	private const byte RELIABLE_CHANNEL = 1;

	public const byte EventCode_MapChunk = 99;

	public const byte EventCode_XP_display = 98;

	public const byte EventCode_ammoLoss_display = 97;

	public const byte EventCode_fuelLoss_display = 96;

	public const byte EventCode_AI_loadingBar = 95;

	private const float SYNC_EXPIRE_TIME = 60f;

	[Header("Requirements")]
	[SerializeField]
	private PhotonView _photonView;

	[Space(10f)]
	[Header("DEBUG -- INFOS")]
	[SerializeField]
	private bool _isRealtimeMultiplayer;

	private int _receivedBytes;

	private readonly Dictionary<string, List<byte[]>> _buffers = new Dictionary<string, List<byte[]>>();

	private readonly Dictionary<string, int> _expectedChunks = new Dictionary<string, int>();

	private readonly Dictionary<string, int> _totalBytes = new Dictionary<string, int>();

	public static MultiplayerManager Instance { get; private set; }

	public static bool IsRealtimeMultiplayer()
	{
		return PhotonNetwork.InRoom;
	}

	private void Awake()
	{
		Instance = this;
		_isRealtimeMultiplayer = IsRealtimeMultiplayer();
	}

	private void Start()
	{
		if (MainMenu.isLoadedGame && _isRealtimeMultiplayer)
		{
			ForceFullDataResync();
		}
	}

	public override void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	public override void OnDisconnected(DisconnectCause p_cause)
	{
		if (p_cause != DisconnectCause.DisconnectByClientLogic)
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("error-disconnected from multiplayer"), Reconnect);
			UIManager.ShowMessage(p_cause.ToString());
		}
	}

	public override void OnMasterClientSwitched(Photon.Realtime.Player p_player)
	{
		UIManager.ShowMessage(LocalizationManager.Translate("info-host left game"));
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player p_otherPlayer)
	{
		UIManager.ShowMessage(p_otherPlayer.NickName + " " + LocalizationManager.Translate("info-left the game"));
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player p_player)
	{
		UIManager.ShowMessage(p_player.NickName + " " + LocalizationManager.Translate("info-joined the game"));
		if (PhotonNetwork.IsMasterClient)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-sending map to") + p_player.NickName + " ...");
			ForceFullDataResync();
		}
	}

	public void Reconnect()
	{
		SoundManager.instance.PlayUIClickSound();
		StartCoroutine(CR_ReconnectCoroutine());
	}

	public static void RaiseEvent(byte eventCode, object payload)
	{
		RaiseInternal(eventCode, payload);
	}

	public static void RaiseEvent(byte eventCode, params object[] payloads)
	{
		RaiseInternal(eventCode, payloads);
	}

	private static void RaiseInternal(byte p_eventCode, object p_content)
	{
		if (Instance == null)
		{
			Debug.LogWarning("Instance of MultiplayerManager is null.");
		}
		else if (Instance._isRealtimeMultiplayer)
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others,
				CachingOption = EventCaching.DoNotCache
			};
			SendOptions sendOptions = new SendOptions
			{
				Reliability = true,
				Channel = 1
			};
			PhotonNetwork.RaiseEvent(p_eventCode, p_content, raiseEventOptions, sendOptions);
		}
	}

	public static void RPC(string p_RPCname, params object[] p_args)
	{
		if (Instance == null)
		{
			Debug.LogWarning("Instance of MultiplayerManager is null.");
		}
		else
		{
			Instance.RunRPC(p_RPCname, RpcTarget.Others, p_args);
		}
	}

	public void RunRPC(string p_RPCname, string p_targetCode, params object[] p_args)
	{
		if (!_isRealtimeMultiplayer)
		{
			return;
		}
		if (string.Equals(p_targetCode, "A", StringComparison.OrdinalIgnoreCase))
		{
			DispatchRPC(p_RPCname, RpcTarget.All, p_args);
			return;
		}
		if (string.Equals(p_targetCode, "O", StringComparison.OrdinalIgnoreCase))
		{
			DispatchRPC(p_RPCname, RpcTarget.Others, p_args);
			return;
		}
		Photon.Realtime.Player player = PhotonNetwork.CurrentRoom?.Players?.Values.FirstOrDefault((Photon.Realtime.Player p) => p.NickName == p_targetCode);
		if (player != null)
		{
			DispatchRPC(p_RPCname, player, p_args);
		}
		else
		{
			Debug.LogWarning("RunRPC: could not find player '" + p_targetCode + "'.");
		}
	}

	public void RunRPC(string p_RPCname, RpcTarget p_target, params object[] p_args)
	{
		DispatchRPC(p_RPCname, p_target, p_args);
	}

	public void RunRPC(string p_RPCname, Photon.Realtime.Player p_player, params object[] p_args)
	{
		DispatchRPC(p_RPCname, p_player, p_args);
	}

	private void DispatchRPC(string p_RPCname, RpcTarget p_target, params object[] p_args)
	{
		if (_isRealtimeMultiplayer)
		{
			if ((p_args?.Length ?? 0) > 0)
			{
				p_args = new object[1] { Utils.ConvertObjectToByteArray(p_args) };
			}
			photonView.RPC(p_RPCname, p_target, p_args ?? Array.Empty<object>());
		}
	}

	private void DispatchRPC(string p_RPCname, Photon.Realtime.Player p_player, params object[] p_args)
	{
		if (_isRealtimeMultiplayer)
		{
			if ((p_args?.Length ?? 0) > 0)
			{
				p_args = new object[1] { Utils.ConvertObjectToByteArray(p_args) };
			}
			photonView.RPC(p_RPCname, p_player, p_args ?? Array.Empty<object>());
		}
	}

	private void LogError(string p_message, bool p_displayOnUI)
	{
		Debug.LogError(p_message);
		if (p_displayOnUI)
		{
			UIManager.ShowMessage(p_message, 3f);
		}
	}

	[PunRPC]
	public void RPC_EndTurn()
	{
		AI.isPlaying = false;
		AI.instance.AIplayingPanel.SetActive(value: false);
		TurnManager.instance.NextTurn(p_sendRPC: false);
	}

	[PunRPC]
	public void RPC_SyncTile(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Tile.Coordinates coordinates = (Tile.Coordinates)array[0];
			Tile tile = (Tile)array[1];
			ActionTypes actionTypes = ActionTypes.NONE;
			if (array.Length >= 3 && array[2].GetType() == typeof(ActionTypes))
			{
				actionTypes = (ActionTypes)array[2];
			}
			Tile tile2 = GameData.Instance.map.TilesTable[coordinates.X, coordinates.Y];
			tile2.Name = tile.Name;
			tile2.Type = tile.Type;
			tile2.Season = tile.Season;
			tile2.FilterType = tile.FilterType;
			tile2.CityName = tile.CityName;
			tile2.Capturable = tile.Capturable;
			tile2.IsReinforcementTile = tile.IsReinforcementTile;
			tile2.CanFreezeInWinter = tile.CanFreezeInWinter;
			tile2.Manpower = tile.Manpower;
			tile2.OccupiedManpower = tile.OccupiedManpower;
			tile2.MoneyProductionPerTurn = tile.MoneyProductionPerTurn;
			tile2.OccupiedMoneyProductionPerTurn = tile.OccupiedMoneyProductionPerTurn;
			tile2.OriginalCountryOwner = tile.OriginalCountryOwner;
			tile2.Bridge = tile.Bridge;
			tile2.Railroad = tile.Railroad;
			tile2.IsBridgeDestroyed = tile.IsBridgeDestroyed;
			tile2.NumberOfTurnsAtLowProd = tile.NumberOfTurnsAtLowProd;
			tile2.NumberOfTurnsSinceVPGotCaptured = tile.NumberOfTurnsSinceVPGotCaptured;
			tile2.SetTileType();
			tile2.SetTileSeason();
			switch (actionTypes)
			{
			case ActionTypes.SET_TILE_NAME:
				if (string.IsNullOrEmpty(tile2.CityName))
				{
					UnityEngine.Object.Destroy(tile2.tileGO.cityNameGO);
				}
				else
				{
					tile2.tileGO.SetCityName();
				}
				break;
			case ActionTypes.ADD_VP:
			{
				if (!GameData.Instance.TryFindPlayerByName(tile.OwnerName, out var o_player))
				{
					return;
				}
				tile2.IsVictoryPoint = true;
				tile2.ownerPlayer = o_player;
				tile2.OwnerName = o_player.Name;
				tile2.terrainOwner = o_player;
				tile2.NumberOfTurnsSinceVPGotCaptured = 0;
				tile2.tileGO.SetVPFlag();
				MapGO.instance.UpdateVpsList();
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: true);
				MapGO.CheckIfEndOfScenario();
				UIManager.instance.UpdateVPtext();
				break;
			}
			case ActionTypes.REMOVE_VP:
				tile2.IsVictoryPoint = false;
				tile2.SecondaryObjective = false;
				tile2.PrimaryObjective = false;
				tile2.ownerPlayer = null;
				tile2.OwnerName = null;
				if (tile2.tileGO.flagGO != null)
				{
					UnityEngine.Object.Destroy(tile2.tileGO.flagGO);
				}
				MapGO.instance.UpdateVpsList();
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: true);
				MapGO.CheckIfEndOfScenario();
				UIManager.instance.UpdateVPtext();
				break;
			}
			if (tile.IsVictoryPoint)
			{
				if (!tile2.IsVictoryPoint)
				{
					tile2.IsVictoryPoint = tile.IsVictoryPoint;
					tile2.OwnerName = tile.OwnerName;
					GameData.Instance.TryFindPlayerByName(GameData.Instance.map.TilesTable[tile.PosX, tile.PosY].OwnerName, out var o_player2);
					tile2.ownerPlayer = o_player2;
					tile2.tileGO.SetSprite();
					tile2.tileGO.SetVPFlag();
				}
				else if (tile.OwnerName != tile2.OwnerName)
				{
					tile2.OwnerName = tile.OwnerName;
					tile2.tileGO.SetVPFlag();
				}
			}
			tile2.tileGO.SetSprite();
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-RPC processing") + " sync tile");
		}
	}

	[PunRPC]
	public void RPC_SyncUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit unit = (Unit)array[0];
			bool p_refreshGraphics = (bool)array[1];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID == unit.ID)
				{
					listOfUnit.SyncFromNetwork(unit, p_refreshGraphics);
					break;
				}
			}
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-RPC processing") + " sync unit");
		}
	}

	[PunRPC]
	public void RPC_AddPlayer(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Player item = (Player)array[0];
			byte index = (byte)array[1];
			GameData.Instance.listOfPlayers.Insert(index, item);
			TurnManager.instance.DiplomacyChanged(p_refreshBorders: true);
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_AddPlayer", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_RemovePlayer(byte[] p_bytes)
	{
		try
		{
			string text = (string)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			for (byte b = 0; b < GameData.Instance.listOfPlayers.Count; b++)
			{
				if (GameData.Instance.listOfPlayers[b].Name == text)
				{
					GameData.Instance.listOfPlayers.RemoveAt(b);
					TurnManager.instance.DiplomacyChanged(p_refreshBorders: true);
					break;
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_RemovePlayer", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncPlayer(byte[] p_bytes)
	{
		try
		{
			Player player = (Player)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			if (GameData.Instance.TryFindPlayerByName(player.Name, out var o_player))
			{
				o_player.Faction = player.Faction;
				o_player.MoneyAmount = player.MoneyAmount;
				o_player.SetAvailableManpower(player.ManpowerAvailable);
				o_player.HighCommandPoints = player.HighCommandPoints;
				o_player.ListActivePolicies = player.ListActivePolicies;
				o_player.CurrentUnitsResearch = player.CurrentUnitsResearch;
				o_player.ListOfUnitsResearched = player.ListOfUnitsResearched;
				o_player.NumberOfVPEverConquered = player.NumberOfVPEverConquered;
				o_player.NumberOfAvailableResearchPoints = player.NumberOfAvailableResearchPoints;
				o_player.OOBautoSupplies = player.OOBautoSupplies;
				o_player.LineDatas = player.LineDatas;
				o_player.UnitCategoriesQuantity = player.UnitCategoriesQuantity;
				o_player.TurnUnitsLost = player.TurnUnitsLost;
				o_player.TurnUnitsDestroyed = player.TurnUnitsDestroyed;
				o_player.TurnUnitsBought = player.TurnUnitsBought;
				o_player.TurnMoneyEarned = player.TurnMoneyEarned;
				o_player.TurnMoneySpent = player.TurnMoneySpent;
				o_player.TotalMoneyEarned = player.TotalMoneyEarned;
				o_player.TotalMoneySpent = player.TotalMoneySpent;
				o_player.TotalUnitsBought = player.TotalUnitsBought;
				o_player.TotalUnitsDestroyed = player.TotalUnitsDestroyed;
				o_player.TotalUnitsLost = player.TotalUnitsLost;
				o_player.PlayersAtWar = player.PlayersAtWar;
				o_player.AlliedPlayers = player.AlliedPlayers;
				o_player.Truces = player.Truces;
				o_player.Stats = player.Stats;
				o_player.IncomeModifier = player.IncomeModifier;
				o_player.EventMoneyToProcess = player.EventMoneyToProcess;
			}
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-RPC processing") + " sync player");
		}
	}

	[PunRPC]
	public void RPC_CountryChange(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			string text = (string)array[1];
			if (!GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				return;
			}
			UIManager.ShowRelationshipChangeMessage(o_player.Country, "", LocalizationManager.Translate("notif-country change") + LocalizationManager.GetLocalizedCountry(text));
			o_player.Country = text;
			for (int i = 0; i < GameData.Instance.map.SizeX; i++)
			{
				for (int j = 0; j < GameData.Instance.map.SizeY; j++)
				{
					if (GameData.Instance.map.TilesTable[i, j].IsVictoryPoint && GameData.Instance.map.TilesTable[i, j].OwnerName == o_player.Name)
					{
						GameData.Instance.map.TilesTable[i, j].tileGO.SetVPFlag();
					}
				}
			}
			MapGO.instance.DrawBordersOnMap();
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if ((bool)listOfUnit.unitGO)
				{
					listOfUnit.unitGO.SetSprite();
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_CountryChange", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SwapUnits(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			int num = (int)array[0];
			string p_playerName = (string)array[1];
			short num2 = (short)array[2];
			short num3 = (short)array[3];
			int num4 = (int)array[4];
			string p_playerName2 = (string)array[5];
			short num5 = (short)array[6];
			short num6 = (short)array[7];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				foreach (Unit listOfUnit in o_player.ListOfUnits)
				{
					if (listOfUnit.ID == num)
					{
						if (listOfUnit.Type == "Plane")
						{
							GameData.Instance.map.TilesTable[num5, num6].AirUnit = listOfUnit;
							GameData.Instance.map.TilesTable[num5, num6].tileGO.airUnitGO = listOfUnit.unitGO;
						}
						else
						{
							GameData.Instance.map.TilesTable[num5, num6].GroundUnit = listOfUnit;
							GameData.Instance.map.TilesTable[num5, num6].tileGO.groundUnitGO = listOfUnit.unitGO;
						}
						listOfUnit.unitGO.tileGO = GameData.Instance.map.TilesTable[num5, num6].tileGO;
						listOfUnit.unitGO.transform.position = new Vector3(GameData.Instance.map.TilesTable[num5, num6].tileGO.transform.position.x, GameData.Instance.map.TilesTable[num5, num6].tileGO.transform.position.y, -1f);
						break;
					}
				}
			}
			if (!GameData.Instance.TryFindPlayerByName(p_playerName2, out var o_player2))
			{
				return;
			}
			foreach (Unit listOfUnit2 in o_player2.ListOfUnits)
			{
				if (listOfUnit2.ID == num4)
				{
					if (listOfUnit2.Type == "Plane")
					{
						GameData.Instance.map.TilesTable[num2, num3].AirUnit = listOfUnit2;
						GameData.Instance.map.TilesTable[num2, num3].tileGO.airUnitGO = listOfUnit2.unitGO;
					}
					else
					{
						GameData.Instance.map.TilesTable[num2, num3].GroundUnit = listOfUnit2;
						GameData.Instance.map.TilesTable[num2, num3].tileGO.groundUnitGO = listOfUnit2.unitGO;
					}
					listOfUnit2.unitGO.tileGO = GameData.Instance.map.TilesTable[num2, num3].tileGO;
					listOfUnit2.unitGO.transform.position = new Vector3(GameData.Instance.map.TilesTable[num2, num3].tileGO.transform.position.x, GameData.Instance.map.TilesTable[num2, num3].tileGO.transform.position.y, -1f);
					break;
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SwapUnits", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_MoveUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit unit = (Unit)array[0];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID != unit.ID)
				{
					continue;
				}
				List<Tile.Coordinates> list = (List<Tile.Coordinates>)array[1];
				listOfUnit.SyncFromNetwork(unit, p_refreshGraphics: false);
				Tile tile = GameData.Instance.map.TilesTable[list[0].X, list[0].Y];
				Tile tile2 = GameData.Instance.map.TilesTable[list[list.Count - 1].X, list[list.Count - 1].Y];
				if (listOfUnit.Type == "Plane")
				{
					tile.tileGO.airUnitGO = null;
					tile.AirUnit = null;
					tile2.tileGO.airUnitGO = listOfUnit.unitGO;
					tile2.AirUnit = listOfUnit;
					if (tile.tileGO.groundUnitGO != null)
					{
						tile.tileGO.groundUnitGO.RemoveFogOfWarAroundUnit();
					}
				}
				else
				{
					tile.tileGO.groundUnitGO = null;
					tile.GroundUnit = null;
					tile2.tileGO.groundUnitGO = listOfUnit.unitGO;
					tile2.GroundUnit = listOfUnit;
					if (tile.tileGO.airUnitGO != null)
					{
						tile.tileGO.airUnitGO.RemoveFogOfWarAroundUnit();
					}
				}
				listOfUnit.unitGO.tileGO = tile2.tileGO;
				UIManager.showAirUnit = listOfUnit.Type == "Plane";
				UIManager.instance.ShowHideAirGroundUnit(doesSound: false);
				listOfUnit.unitGO.gameObject.SetActive(value: true);
				listOfUnit.unitGO.RemoveFogOfWarAroundUnit();
				StartCoroutine(CR_MoveUnitToTile(listOfUnit, list));
				break;
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_MoveUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_KillUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			int num = (int)array[0];
			string p_playerName = (string)array[1];
			bool p_playAnimation = (bool)array[2];
			bool flag = (bool)array[3];
			if (!GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID == num)
				{
					if (flag && !listOfUnit.unitGO.tileGO.isInFogOfWar)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Surrender"));
						gameObject.transform.SetParent(listOfUnit.unitGO.tileGO.transform);
						UnityEngine.Object.Destroy(gameObject, 0.9f);
						Vector3 position = new Vector3(listOfUnit.unitGO.tileGO.transform.position.x + 2f, listOfUnit.unitGO.tileGO.transform.position.y + 2f, -5f);
						gameObject.transform.position = position;
					}
					listOfUnit.unitGO.DestroyUnit(p_playAnimation, p_sendRPC: false);
					break;
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_KillUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_RedeployUnit(byte[] p_bytes)
	{
		try
		{
			Unit unit = (Unit)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID != unit.ID)
				{
					continue;
				}
				listOfUnit.SyncFromNetwork(unit, p_refreshGraphics: false);
				if (!listOfUnit.unitGO)
				{
					break;
				}
				if (listOfUnit.IsCommander)
				{
					MapGO.listOfAllGeneralsOnTheMap.Remove(listOfUnit.unitGO);
				}
				if (listOfUnit.Type == "Plane")
				{
					if (listOfUnit.unitGO.tileGO.groundUnitGO != null)
					{
						listOfUnit.unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
						listOfUnit.unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
					}
					listOfUnit.unitGO.tileGO.tile.AirUnit = null;
				}
				else
				{
					if (listOfUnit.unitGO.tileGO.airUnitGO != null)
					{
						listOfUnit.unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
						listOfUnit.unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
					}
					listOfUnit.unitGO.tileGO.tile.GroundUnit = null;
				}
				UnityEngine.Object.Destroy(listOfUnit.unitGO.gameObject);
				break;
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_RedeployUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_DespawnUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			int num = (int)array[0];
			string p_playerName = (string)array[1];
			if (!GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID != num)
				{
					continue;
				}
				if ((bool)listOfUnit.unitGO)
				{
					if (listOfUnit.IsCommander)
					{
						MapGO.listOfAllGeneralsOnTheMap.Remove(listOfUnit.unitGO);
					}
					UnityEngine.Object.Destroy(listOfUnit.unitGO.gameObject);
				}
				break;
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_DespawnUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SpawnUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			Unit unit = (Unit)array[2];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID == unit.ID)
				{
					MapGO.listOfSpawnableTiles = new HashSet<TileGO> { GameData.Instance.map.TilesTable[num, num2].tileGO };
					GameData.Instance.map.TilesTable[num, num2].tileGO.SpawnUnit(listOfUnit, p_playSound: false, p_sendRPC: false);
					if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar && (PlayerSettings.Instance.FollowAIMoves || !listOfUnit.OwnerPlayer.IsComputer))
					{
						UIManager.instance.StartCoroutine(UIManager.instance.CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y, Camera.main.transform.position.z), 0.2f));
					}
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SpawnUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_CaptureVP(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit unit = (Unit)array[0];
			short num = (short)array[1];
			short num2 = (short)array[2];
			UnitGO[] componentsInChildren = UIManager.instance.unitsParent.GetComponentsInChildren<UnitGO>(includeInactive: true);
			foreach (UnitGO unitGO in componentsInChildren)
			{
				if (unitGO.unit.ID == unit.ID)
				{
					unitGO.TryCaptureTile(GameData.Instance.map.TilesTable[num, num2], p_sendRPC: false);
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_CaptureVP", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_EmbarkDisembark(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			int num = (int)array[0];
			string p_playerName = (string)array[1];
			string text = (string)array[2];
			if (!GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID != num)
				{
					continue;
				}
				if (listOfUnit.unitGO == null)
				{
					Debug.LogError("THIS UNIT DOESNT EXIST " + listOfUnit.ID);
					break;
				}
				switch (text)
				{
				case "Ship":
					listOfUnit.unitGO.EmbarkDisembarkBoat(p_sendRPC: false);
					break;
				case "Train":
					listOfUnit.unitGO.EmbarkDisembarkTrain(p_sendRPC: false);
					break;
				case "DropPara":
					listOfUnit.unitGO.DropParatrooper(p_sendRPC: false);
					break;
				case "LoadPara":
					listOfUnit.unitGO.LoadParatrooper(p_sendRPC: false);
					break;
				case "Submarine":
					listOfUnit.unitGO.DiveUndiveSubmarine(p_sendRPC: false);
					break;
				}
				break;
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_EmbarkDisembark", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_ShowDamage(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			string text = (string)array[2];
			if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				DamageTextGO damageTextGO = UnityEngine.Object.Instantiate(Resources.Load<DamageTextGO>("Prefabs/UI/Damage_Text"));
				damageTextGO.transform.SetParent(GameData.Instance.map.TilesTable[num, num2].tileGO.transform);
				damageTextGO.GetComponent<TextMeshPro>().text = text;
				Vector3 position = new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y + 3.5f, -20f);
				damageTextGO.transform.position = position;
				damageTextGO.Animate();
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_ShowDamage", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_DisplayXP(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			string text = (string)array[2];
			if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/XP_Text"));
				gameObject.transform.SetParent(GameData.Instance.map.TilesTable[num, num2].tileGO.transform);
				gameObject.GetComponentInChildren<TextMeshPro>().text = text;
				Vector3 position = new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y, -10f);
				gameObject.transform.position = position;
				LeanTween.move(gameObject, new Vector3(position.x, position.y + 2.5f, -10f), 1f);
				UnityEngine.Object.Destroy(gameObject, 3.5f);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_DisplayXP", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_DisplayLevelUp(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			byte b = (byte)array[2];
			if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Level up"));
				base.gameObject.transform.SetParent(GameData.Instance.map.TilesTable[num, num2].tileGO.transform);
				new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x + 2f, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y + 0.5f, 0f);
				gameObject.transform.localScale = Vector3.zero;
				gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("UI/Ranks/" + b);
				LeanTween.scale(gameObject, Vector3.one, 0.4f);
				LeanTween.move(gameObject, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 3f, -10f), 0.75f);
				UnityEngine.Object.Destroy(gameObject, 2f);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_DisplayLevelUp", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_DisplayFuelLoss(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			string text = (string)array[2];
			if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Fuel loss Text"));
				gameObject.transform.SetParent(GameData.Instance.map.TilesTable[num, num2].tileGO.transform);
				gameObject.GetComponentInChildren<TextMeshPro>().text = text;
				Vector3 position = new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x + 2.5f, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y, -10f);
				gameObject.transform.position = position;
				UnityEngine.Object.Destroy(gameObject, 2f);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_DisplayFuelLoss", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_DisplayAmmoLoss(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			string text = (string)array[2];
			if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Ammo loss Text"));
				gameObject.transform.SetParent(GameData.Instance.map.TilesTable[num, num2].tileGO.transform);
				gameObject.GetComponentInChildren<TextMeshPro>().text = text;
				Vector3 position = new Vector3(GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.x + 2.5f, GameData.Instance.map.TilesTable[num, num2].tileGO.transform.position.y + 1f, -10f);
				gameObject.transform.position = position;
				UnityEngine.Object.Destroy(gameObject, 2f);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_DisplayAmmoLoss", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_AttackUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit unit = (Unit)array[0];
			Unit unit2 = (Unit)array[1];
			Unit unit3 = null;
			Unit unit4 = null;
			UnitGO unitGO = null;
			UnitGO unitGO2 = null;
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player) || !GameData.Instance.TryFindPlayerByName(unit2.OwnerName, out var o_player2))
			{
				return;
			}
			foreach (Unit listOfUnit in o_player.ListOfUnits)
			{
				if (listOfUnit.ID == unit.ID)
				{
					unit3 = listOfUnit;
					unitGO = listOfUnit.unitGO;
					break;
				}
			}
			foreach (Unit listOfUnit2 in o_player2.ListOfUnits)
			{
				if (listOfUnit2.ID == unit2.ID)
				{
					unit4 = listOfUnit2;
					unitGO2 = listOfUnit2.unitGO;
					break;
				}
			}
			if (unit3 == null || unit4 == null)
			{
				return;
			}
			unit4.XP = unit2.XP;
			unit4.Lvl = unit2.Lvl;
			unit4.Hero = unit2.Hero;
			unit4.Morale = unit2.Morale;
			unit4.CurrHP = unit2.CurrHP;
			unit4.CurrAmmo = unit2.CurrAmmo;
			unit4.CurrAutonomy = unit2.CurrAutonomy;
			unit4.HasAttacked = unit2.HasAttacked;
			unit4.EntrenchmentLevel = unit2.EntrenchmentLevel;
			unit4.StuffThatHappened = unit2.StuffThatHappened;
			unit3.XP = unit.XP;
			unit3.Lvl = unit.Lvl;
			unit3.Hero = unit.Hero;
			unit3.Morale = unit.Morale;
			unit3.CurrHP = unit.CurrHP;
			unit3.CurrAmmo = unit.CurrAmmo;
			unit3.CurrAutonomy = unit.CurrAutonomy;
			unit3.HasAttacked = unit.HasAttacked;
			unit3.EntrenchmentLevel = unit.EntrenchmentLevel;
			unit3.StuffThatHappened = unit.StuffThatHappened;
			if (unitGO2 != null && unitGO != null)
			{
				if (!unitGO2.tileGO.isInFogOfWar)
				{
					unitGO.tileGO.RemoveFogOfWar();
				}
				unitGO2.RotateAttackedUnit(unitGO2, unitGO);
			}
			if (unitGO2 != null)
			{
				unitGO2.UpdateCounter();
			}
			if (unitGO != null)
			{
				unitGO.UpdateCounter();
				if (!unitGO.tileGO.isInFogOfWar && (PlayerSettings.Instance.FollowAIMoves || !unit4.OwnerPlayer.IsComputer))
				{
					UIManager.instance.StartCoroutine(UIManager.instance.CenterCameraOnUnitCoroutine(CameraGO.instance.transform.position, new Vector3(unitGO.tileGO.transform.position.x, unitGO.tileGO.transform.position.y, CameraGO.instance.transform.position.z), 0.2f));
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_AttackUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_PlaySound(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			short num = (short)array[0];
			short num2 = (short)array[1];
			string p_soundName = (string)array[2];
			string text = (string)array[3];
			if (text == "UI")
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound(p_soundName));
			}
			else if (!GameData.Instance.map.TilesTable[num, num2].tileGO.isInFogOfWar)
			{
				if (text == "Movement")
				{
					SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(p_soundName));
				}
				else if (text == "Attack")
				{
					SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(p_soundName));
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_PlaySound", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_AddUnit(byte[] p_bytes)
	{
		try
		{
			Unit unit = (Unit)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player))
			{
				return;
			}
			o_player.AddUnit(unit, p_sendRPC: false);
			if (unit.IsReserve)
			{
				return;
			}
			UnitGO[] componentsInChildren = UIManager.instance.unitsParent.GetComponentsInChildren<UnitGO>(includeInactive: true);
			foreach (UnitGO unitGO in componentsInChildren)
			{
				if (unit.ID == unitGO.unit.ID)
				{
					unitGO.unit = unit;
					unit.unitGO = unitGO;
					unit.unitGO.SetSprite();
					break;
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_AddUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_RemoveUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit p_unit = (Unit)array[0];
			string p_playerName = (string)array[1];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				o_player.RemoveUnit(p_unit, p_sendRPC: false);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_RemoveUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_TransferUnit(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			Unit unit = (Unit)array[0];
			string p_playerName = (string)array[1];
			if (!GameData.Instance.TryFindPlayerByName(unit.OwnerName, out var o_player) || !GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player2))
			{
				return;
			}
			o_player.RemoveUnit(unit, p_sendRPC: false);
			o_player2.AddUnit(unit, p_sendRPC: false);
			UnitGO[] componentsInChildren = UIManager.instance.unitsParent.GetComponentsInChildren<UnitGO>(includeInactive: true);
			foreach (UnitGO unitGO in componentsInChildren)
			{
				if (unit.ID != unitGO.unit.ID || !(unitGO.unit.OwnerName == o_player.Name))
				{
					continue;
				}
				if (unit.IsReserve)
				{
					if (unit.Type == "Plane")
					{
						if (unitGO.tileGO.groundUnitGO != null)
						{
							unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
							unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
						}
						unitGO.tileGO.airUnitGO = null;
						unitGO.tileGO.tile.AirUnit = null;
					}
					else
					{
						if (unitGO.tileGO.airUnitGO != null)
						{
							unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
							unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
						}
						unitGO.tileGO.groundUnitGO = null;
						unitGO.tileGO.tile.GroundUnit = null;
					}
					if (unit.IsCommander)
					{
						MapGO.listOfAllGeneralsOnTheMap.Remove(unitGO);
					}
					UnityEngine.Object.Destroy(unitGO.gameObject);
				}
				else
				{
					unitGO.unit = unit;
					unit.unitGO = unitGO;
					unitGO.SetSprite();
				}
				break;
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_TransferUnit", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncNotifications(byte[] p_bytes)
	{
		try
		{
			List<Notification> notifications = (List<Notification>)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			GameData.Instance.map.Notifications = notifications;
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncNotifications", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncEvents(byte[] p_bytes)
	{
		try
		{
			List<Event> events = (List<Event>)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
			GameData.Instance.map.Events = events;
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncEvents", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_ShowRelationshipWindow(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string country = (string)array[0];
			string country2 = (string)array[1];
			string text = (string)array[2];
			UIManager.ShowRelationshipChangeMessage(country, country2, text);
			TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_ShowRelationshipWindow", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_WarDeclaration(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			string p_playerName2 = (string)array[1];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player) && GameData.Instance.TryFindPlayerByName(p_playerName2, out var o_player2))
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Declare war"));
				UIManager.ShowRelationshipChangeMessage(o_player.Country, o_player2.Country, LocalizationManager.Translate("relationship-war declaration"));
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_WarDeclaration", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_AllianceMade(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			string p_playerName2 = (string)array[1];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player) && GameData.Instance.TryFindPlayerByName(p_playerName2, out var o_player2))
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Sign peace"));
				UIManager.ShowRelationshipChangeMessage(o_player.Country, o_player2.Country, LocalizationManager.Translate("relationship-new alliance"));
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_AllianceMade", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SignedPeace(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			string p_playerName2 = (string)array[1];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player) && GameData.Instance.TryFindPlayerByName(p_playerName2, out var o_player2))
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Sign peace"));
				UIManager.ShowRelationshipChangeMessage(o_player.Country, o_player2.Country, LocalizationManager.Translate("relationship-made peace"));
				TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SignedPeace", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncLineDatas(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			List<LineData> lineDatas = (List<LineData>)array[1];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				o_player.LineDatas = lineDatas;
				DrawingManager.instance.SpawnLinesForPlayer(o_player);
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncLineDatas", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncUnitData(byte[] p_bytes)
	{
		try
		{
			_ = (SyncActions)((object[])Utils.ConvertByteArrayToObject(p_bytes))[0];
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncPlayerData", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncPlayerData(byte[] p_bytes)
	{
		try
		{
			object[] array = (object[])Utils.ConvertByteArrayToObject(p_bytes);
			string p_playerName = (string)array[0];
			if (GameData.Instance.TryFindPlayerByName(p_playerName, out var o_player))
			{
				switch ((SyncActions)array[1])
				{
				case SyncActions.PLAYER_PURCHASE_UNIT:
				{
					Unit unit3 = (Unit)array[2];
					unit3.PatchOnNetworkReceived();
					int cost = (int)array[3];
					int manpowerCost = (int)array[4];
					unit3.Cost = cost;
					unit3.manpowerCost = manpowerCost;
					o_player.PurchaseUnit(unit3, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_ADD_UNIT:
				{
					Unit unit2 = (Unit)array[2];
					unit2.PatchOnNetworkReceived();
					o_player.AddUnit(unit2, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_REMOVE_UNIT:
				{
					Unit unit = (Unit)array[2];
					unit.PatchOnNetworkReceived();
					o_player.RemoveUnit(unit, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_RESEARCH:
				{
					Player player2 = (Player)array[2];
					o_player.CurrentUnitsResearch = player2.CurrentUnitsResearch;
					o_player.ListOfUnitsResearched = player2.ListOfUnitsResearched;
					o_player.NumberOfAvailableResearchPoints = player2.NumberOfAvailableResearchPoints;
					break;
				}
				case SyncActions.PLAYER_FACTION_CHANGE:
				{
					string p_newFactionName = (string)array[2];
					bool p_getNewAllianceWars = (bool)array[3];
					o_player.ChangeFaction(p_newFactionName, p_getNewAllianceWars, p_sendRPC: false);
					TurnManager.instance.DiplomacyChanged(p_refreshBorders: false);
					break;
				}
				case SyncActions.PLAYER_DIPLOMACY:
				{
					Player player = (Player)array[2];
					o_player.Faction = player.Faction;
					o_player.Truces = player.Truces;
					o_player.AlliedPlayers = player.AlliedPlayers;
					o_player.PlayersAtWar = player.PlayersAtWar;
					break;
				}
				case SyncActions.PLAYER_DRAWINGS:
				{
					List<LineData> lineDatas = (List<LineData>)array[2];
					o_player.LineDatas = lineDatas;
					DrawingManager.instance.SpawnLinesForPlayer(o_player);
					break;
				}
				case SyncActions.PLAYER_PURCHASE_POLICY:
				{
					Policy p_policy3 = (Policy)array[2];
					o_player.PurchasePolicy(p_policy3, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_ADD_POLICY:
				{
					Policy p_policy2 = (Policy)array[2];
					o_player.AddPolicy(p_policy2, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_REMOVE_POLICY:
				{
					Policy p_policy = (Policy)array[2];
					o_player.RemovePolicy(p_policy, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_ADD_MONEY:
				{
					int p_amount4 = (int)array[2];
					o_player.AddMoney(p_amount4, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_REMOVE_MONEY:
				{
					int p_amount3 = (int)array[2];
					o_player.RemoveMoney(p_amount3, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_ADD_MANPOWER:
				{
					int p_amount2 = (int)array[2];
					bool p_doMarginCheck = (bool)array[3];
					o_player.AddManpower(p_amount2, p_doMarginCheck, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_REMOVE_MANPOWER:
				{
					int p_amount = (int)array[2];
					o_player.RemoveManpower(p_amount, p_sendRPC: false);
					break;
				}
				case SyncActions.PLAYER_AUTOMATIC_SUPPLIES:
				{
					bool oOBautoSupplies = (bool)array[2];
					o_player.OOBautoSupplies = oOBautoSupplies;
					break;
				}
				case SyncActions.PLAYER_ADD_ALLIANCE:
				case SyncActions.PLAYER_REMOVE_ALLIANCE:
				case SyncActions.PLAYER_ADD_WAR:
				case SyncActions.PLAYER_REMOVE_WAR:
				case SyncActions.PLAYER_ADD_TRUCE:
				case SyncActions.PLAYER_REMOVE_TRUCE:
				case SyncActions.PLAYER_ADD_HQ_POINTS:
				case SyncActions.PLAYER_REMOVE_HQ_POINTS:
					break;
				}
			}
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncPlayerData", arg), p_displayOnUI: true);
		}
	}

	[PunRPC]
	public void RPC_SyncAnything(byte[] p_bytes)
	{
		try
		{
			_ = (object[])Utils.ConvertByteArrayToObject(p_bytes);
		}
		catch (Exception arg)
		{
			LogError(string.Format("{0} {1} ; {2}", LocalizationManager.Translate("error-RPC processing"), "RPC_SyncAnything", arg), p_displayOnUI: true);
		}
	}

	public void ForceFullDataResync()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			Debug.LogWarning("SendMapToOthers called but not MasterClient.");
			return;
		}
		byte[] array = Utils.ConvertObjectToByteArray(GameData.Instance);
		string text = Guid.NewGuid().ToString("N");
		int num = (array.Length + 16384 - 1) / 16384;
		Debug.Log($"[MapSync] Sending {array.Length} bytes in {num} chunks (ID {text}).");
		StartCoroutine(CR_SendChunksCoroutine(text, num, array));
	}

	private IEnumerator CR_SendChunksCoroutine(string p_transferID, int p_totalChunks, byte[] p_payload)
	{
		int sentBytes = 0;
		for (int i = 0; i < p_totalChunks; i++)
		{
			int num = i * 16384;
			int num2 = Math.Min(16384, p_payload.Length - num);
			byte[] array = new byte[num2];
			Buffer.BlockCopy(p_payload, num, array, 0, num2);
			RaiseEvent(99, p_transferID, i, p_totalChunks, p_payload.Length, array);
			Debug.Log("Sent chunk: " + (i + 1) + "/" + p_totalChunks);
			sentBytes += num2;
			UIManager.ShowMessage(LocalizationManager.Translate("txt-sending data") + "\n" + Utils.FormatReceivedTotal(sentBytes, p_payload.Length), 1f);
			yield return null;
		}
	}

	public void OnEvent(ExitGames.Client.Photon.EventData p_photonEvent)
	{
		ProcessEvent(p_photonEvent.Code, p_photonEvent.CustomData);
	}

	private void ProcessEvent(byte p_eventCode, object p_object)
	{
		switch (p_eventCode)
		{
		case 99:
			DataChunkReceived(p_object);
			break;
		case 95:
			UpdateAILoadingBar(p_object);
			break;
		case 98:
			DisplayXPchange(p_object);
			break;
		case 96:
		case 97:
			break;
		}
	}

	private void UpdateAILoadingBar(object p_object)
	{
		AI.instance.loadingBar.value = (float)p_object;
	}

	private void DisplayXPchange(object p_object)
	{
		object[] array = (object[])p_object;
		string text = (string)array[0];
		short num = (short)array[1];
		short num2 = (short)array[2];
		if (num > GameData.Instance.map.SizeX || num2 > GameData.Instance.map.SizeY)
		{
			Debug.LogWarning("Invalid map coordinates to display XP change.");
			return;
		}
		Tile tile = GameData.Instance.map.TilesTable[num, num2];
		if (!tile.tileGO.isInFogOfWar)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/XP_Text"));
			gameObject.transform.SetParent(tile.tileGO.transform);
			gameObject.GetComponentInChildren<TextMeshPro>().text = text;
			Vector3 position = new Vector3(tile.tileGO.transform.position.x, tile.tileGO.transform.position.y, -10f);
			gameObject.transform.position = position;
			LeanTween.move(gameObject, new Vector3(position.x, position.y + 2.5f, -10f), 1f);
			UnityEngine.Object.Destroy(gameObject, 3.5f);
		}
	}

	private void DataChunkReceived(object p_object)
	{
		try
		{
			object[] array = (object[])p_object;
			string text = (string)array[0];
			int num = (int)array[1];
			int num2 = (int)array[2];
			int num3 = (int)array[3];
			byte[] array2 = (byte[])array[4];
			_receivedBytes += array2.Length;
			Debug.Log("Received chunk: " + (num + 1) + "/" + num2);
			UIManager.ShowMessage(LocalizationManager.Translate("txt-receiving data") + "\n" + Utils.FormatReceivedTotal(_receivedBytes, num3), 1f);
			if (!_buffers.TryGetValue(text, out var value))
			{
				value = new List<byte[]>(new byte[num2][]);
				_buffers[text] = value;
				_expectedChunks[text] = num2;
				_totalBytes[text] = num3;
				StartCoroutine(CR_ExpireAfter(text, 60f));
			}
			value[num] = array2;
			if (value.All((byte[] b) => b != null))
			{
				int num4 = 0;
				byte[] array3 = new byte[_totalBytes[text]];
				for (int num5 = 0; num5 < value.Count; num5++)
				{
					Buffer.BlockCopy(value[num5], 0, array3, num4, value[num5].Length);
					num4 += value[num5].Length;
				}
				_buffers.Remove(text);
				_expectedChunks.Remove(text);
				_totalBytes.Remove(text);
				MainMenu.isLoadedGame = true;
				GameData data = (GameData)Utils.ConvertByteArrayToObject(array3);
				GameData.Instance.SetData(data);
				Debug.Log($"[MapSync] Received and applied {num4} bytes (ID {text}).");
				PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
			}
		}
		catch (Exception ex)
		{
			UIManager.ShowMessage("Map sync error: " + ex.Message);
		}
	}

	private IEnumerator CR_ExpireAfter(string p_transferID, float p_seconds)
	{
		yield return new WaitForSeconds(p_seconds);
		if (_buffers.ContainsKey(p_transferID))
		{
			_buffers.Remove(p_transferID);
			_expectedChunks.Remove(p_transferID);
			_totalBytes.Remove(p_transferID);
			Debug.LogWarning("[MapSync] Transfer " + p_transferID + " expired / incomplete.");
		}
	}

	private IEnumerator CR_MoveUnitToTile(Unit p_unit, List<Tile.Coordinates> p_coordinates)
	{
		if (p_unit.unitGO == null)
		{
			Debug.LogWarning("UnitGO is null.");
			yield break;
		}
		if (p_coordinates == null || p_coordinates.Count < 2)
		{
			Debug.LogWarning("Invalid network path for unit to move (null or < 2)");
			yield break;
		}
		bool playedMovementSound = false;
		while (p_coordinates.Count > 1)
		{
			Tile.Coordinates coordinates = p_coordinates[0];
			Tile.Coordinates coordinates2 = p_coordinates[1];
			Tile prevTile = GameData.Instance.map.TilesTable[coordinates.X, coordinates.Y];
			Tile nextTile = GameData.Instance.map.TilesTable[coordinates2.X, coordinates2.Y];
			if (prevTile?.tileGO == null || nextTile?.tileGO == null)
			{
				Debug.LogWarning("One of the tiles or tileGOs is null.");
				break;
			}
			if (!playedMovementSound && (!prevTile.tileGO.isInFogOfWar || !nextTile.tileGO.isInFogOfWar))
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(p_unit.GetMovementSound()));
			}
			Vector3 prevPos = new Vector3(prevTile.tileGO.transform.position.x, prevTile.tileGO.transform.position.y, -1f);
			Vector3 nextPos = new Vector3(nextTile.tileGO.transform.position.x, nextTile.tileGO.transform.position.y, -1f);
			float segmentDist = Vector3.Distance(prevPos, nextPos);
			if (!nextTile.tileGO.isInFogOfWar && (PlayerSettings.Instance.FollowAIMoves || !p_unit.OwnerPlayer.IsComputer))
			{
				UIManager.instance.StartCoroutine(UIManager.instance.CenterCameraOnUnitCoroutine(Camera.main.transform.position, new Vector3(nextTile.tileGO.transform.position.x, nextTile.tileGO.transform.position.y, Camera.main.transform.position.z), 0.2f));
			}
			if (PlayerSettings.Instance.IsQuickMovement)
			{
				p_unit.unitGO.transform.position = nextPos;
				yield return null;
			}
			else
			{
				if (segmentDist <= Mathf.Epsilon)
				{
					p_unit.unitGO.transform.position = nextPos;
					p_coordinates.RemoveAt(0);
					continue;
				}
				float startTime = Time.time;
				float num = 0f;
				while (num < 1f)
				{
					yield return null;
					p_unit.unitGO.RotateUnit(prevTile.tileGO, nextTile.tileGO);
					num = (Time.time - startTime) * 25f / segmentDist;
					p_unit.unitGO.transform.position = Vector3.Lerp(prevPos, nextPos, Mathf.Clamp01(num));
				}
				p_unit.unitGO.transform.position = nextPos;
			}
			p_coordinates.RemoveAt(0);
		}
	}

	private IEnumerator CR_ReconnectCoroutine()
	{
		WaitForSeconds waitForSeconds = new WaitForSeconds(3f);
		while (!PhotonNetwork.IsConnected)
		{
			PhotonNetwork.ReconnectAndRejoin();
			UIManager.ShowMessage(LocalizationManager.Translate("info-multiplayer reconnecting"));
			yield return waitForSeconds;
		}
		UIManager.ShowMessage(LocalizationManager.Translate("info-multiplayer reconnected"));
		MultiplayerChat.instance.ConnectToServer();
	}

	private void OnApplicationQuit()
	{
		PhotonNetwork.Disconnect();
	}
}
