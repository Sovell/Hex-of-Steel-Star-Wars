using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilesManagerGO : MonoBehaviour
{
	public static bool isOpen;

	public static TilesManagerGO instance;

	[Header("Setup")]
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private Transform _content;

	[SerializeField]
	private GameObject _transferPanel;

	[SerializeField]
	private GameObject _transferSlot;

	[SerializeField]
	private TileTransferSlotGO _tileTransferSlotGO;

	[SerializeField]
	private GameObject _transferSeparator;

	[SerializeField]
	private GameObject _requestSeparator;

	[SerializeField]
	private GameObject _annexationSeparator;

	[Header("DEBUG")]
	[SerializeField]
	private List<TileGO> _selectedTiles;

	[SerializeField]
	private List<TileTransferSlotGO> _tileTransferSlotGOs;

	private void Awake()
	{
		instance = this;
		_selectedTiles = new List<TileGO>();
		_tileTransferSlotGOs = new List<TileTransferSlotGO>();
	}

	public void EnableTilesManagerPanel()
	{
		isOpen = true;
		_requestSeparator.SetActive(value: false);
		_transferSeparator.SetActive(value: false);
		_annexationSeparator.SetActive(value: false);
		_panel.SetActive(value: true);
		MapGO.instance.UnselectUnit();
		UIManager.instance.ShowHideAllUnits(p_show: false);
		TurnManager.instance.rightSideUI_panel.SetActive(value: false);
		UIManager.instance.HandleMenuStacking(_panel);
	}

	public void DisableTilesManagerPanel()
	{
		isOpen = false;
		_panel.SetActive(value: false);
		_transferPanel.SetActive(value: false);
		UIManager.instance.ShowHideAllUnits(p_show: true);
		TurnManager.instance.rightSideUI_panel.SetActive(value: true);
		_selectedTiles.Clear();
		ClearAllSlots();
	}

	public void ToggleTilesManagerPanel()
	{
		if (_panel.activeSelf)
		{
			DisableTilesManagerPanel();
		}
		else
		{
			EnableTilesManagerPanel();
		}
	}

	public void ClickedOnTile(TileGO p_tileGO)
	{
		if (_selectedTiles.Contains(p_tileGO))
		{
			ProcessTileRemoval(p_tileGO);
		}
		else
		{
			ProcessTileAddition(p_tileGO);
		}
		ReorganizeSlots();
	}

	public GameObject GetTransferPanel()
	{
		return _transferPanel;
	}

	public GameObject GetTransferSlot()
	{
		return _transferSlot;
	}

	public void AnnexTile(TileGO p_tileGO, Player p_player)
	{
		int annexTileCost = p_tileGO.tile.GetAnnexTileCost(p_player);
		p_player.RemoveMoney(annexTileCost, p_sendRPC: true);
		GameData.Instance.TryFindPlayerByCountry(p_tileGO.tile.OriginalCountryOwner, out var o_player);
		Notification notification = ((o_player != null) ? new Notification(NotificationTypes.VP_ANNEXED, o_player.Name, p_tileGO.transform.position.x, p_tileGO.transform.position.y, p_tileGO.tile.PosX + ":" + p_tileGO.tile.PosY, p_isImportant: true) : new Notification(NotificationTypes.VP_ANNEXED, string.Empty, p_tileGO.transform.position.x, p_tileGO.transform.position.y, p_tileGO.tile.PosX + ":" + p_tileGO.tile.PosY, p_isImportant: true));
		notification.SenderName = p_player.Name;
		p_tileGO.tile.ownerPlayer = p_player;
		p_tileGO.tile.OwnerName = p_player.Name;
		p_tileGO.tile.OriginalCountryOwner = p_player.Country;
		p_tileGO.tile.terrainOwner = p_player;
		p_tileGO.SetVPFlag();
		if (_panel.activeSelf)
		{
			ClickedOnTile(p_tileGO);
		}
		OnTileOwnershipChanged(p_tileGO);
	}

	public void TransferTile(TileGO p_tileGO, Player p_player)
	{
		CheckForCapitulationEvents(p_tileGO);
		p_tileGO.tile.ownerPlayer = p_player;
		p_tileGO.tile.OwnerName = p_player.Name;
		p_tileGO.tile.terrainOwner = p_player;
		p_tileGO.tile.NumberOfTurnsSinceVPGotCaptured = 0;
		p_tileGO.SetVPFlag();
		new Notification(NotificationTypes.VP_RECEIVED, p_player.Name, p_tileGO.transform.position.x, p_tileGO.transform.position.y, p_tileGO.tile.PosX + ":" + p_tileGO.tile.PosY, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
		if (_panel.activeSelf)
		{
			ClickedOnTile(p_tileGO);
		}
		OnTileOwnershipChanged(p_tileGO);
		_transferPanel.SetActive(value: false);
	}

	public void RequestTile(TileGO p_tileGO)
	{
		int requestOwnershipCost = p_tileGO.tile.GetRequestOwnershipCost(TurnManager.currPlayer);
		TurnManager.currPlayer.RemoveMoney(requestOwnershipCost, p_sendRPC: true);
		p_tileGO.tile.ownerPlayer.AddMoney(requestOwnershipCost, p_sendRPC: true);
		CheckForCapitulationEvents(p_tileGO);
		p_tileGO.tile.ownerPlayer = TurnManager.currPlayer;
		p_tileGO.tile.OwnerName = TurnManager.currPlayer.Name;
		p_tileGO.tile.terrainOwner = TurnManager.currPlayer;
		p_tileGO.tile.OriginalCountryOwner = TurnManager.currPlayer.Country;
		p_tileGO.tile.NumberOfTurnsSinceVPGotCaptured = 0;
		p_tileGO.SetVPFlag();
		new Notification(NotificationTypes.VP_OWNERSHIP_LOSS, p_tileGO.tile.ownerPlayer.Name, p_tileGO.transform.position.x, p_tileGO.transform.position.y, p_tileGO.tile.PosX + ":" + p_tileGO.tile.PosY, p_isImportant: true).SenderName = TurnManager.currPlayer.Name;
		if (_panel.activeSelf)
		{
			ClickedOnTile(p_tileGO);
		}
		OnTileOwnershipChanged(p_tileGO);
	}

	private void ProcessTileAddition(TileGO p_tileGO)
	{
		if (p_tileGO.tile.ownerPlayer != null)
		{
			if (TurnManager.currPlayer.IsAtWarWith(p_tileGO.tile.ownerPlayer))
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-request enemy tile"));
				return;
			}
			if (!TurnManager.currPlayer.IsAlliedWith(p_tileGO.tile.ownerPlayer))
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-request neutral tile"));
				return;
			}
			_selectedTiles.Add(p_tileGO);
			TileTransferSlotGO tileTransferSlotGO = Object.Instantiate(_tileTransferSlotGO, _content);
			tileTransferSlotGO.Setup(p_tileGO);
			_tileTransferSlotGOs.Add(tileTransferSlotGO);
		}
	}

	private void OnTileOwnershipChanged(TileGO p_tileGO)
	{
		if (p_tileGO.tile.AirUnit != null && !p_tileGO.tile.ownerPlayer.IsAlliedWith(p_tileGO.tile.AirUnit.OwnerPlayer))
		{
			p_tileGO.tile.AirUnit.IsReserve = true;
			p_tileGO.tile.AirUnit.SetRedeploymentTime();
			MultiplayerManager.RPC("RPC_RedeployUnit", p_tileGO.tile.AirUnit);
			Object.Destroy(p_tileGO.airUnitGO.gameObject);
			p_tileGO.airUnitGO = null;
			p_tileGO.tile.AirUnit = null;
		}
		if (p_tileGO.tile.GroundUnit != null && !p_tileGO.tile.ownerPlayer.IsAlliedWith(p_tileGO.tile.GroundUnit.OwnerPlayer))
		{
			p_tileGO.tile.GroundUnit.IsReserve = true;
			p_tileGO.tile.GroundUnit.SetRedeploymentTime();
			MultiplayerManager.RPC("RPC_RedeployUnit", p_tileGO.tile.GroundUnit);
			Object.Destroy(p_tileGO.groundUnitGO.gameObject);
			p_tileGO.groundUnitGO = null;
			p_tileGO.tile.GroundUnit = null;
		}
		MultiplayerManager.RPC("RPC_SyncTile", p_tileGO.tile.GetCoord(), p_tileGO.tile, ActionTypes.ADD_VP);
		ReservePanelGO.instance.RefreshDeployNotification();
		TurnManager.instance.DiplomacyChanged(p_refreshBorders: true);
		MapGO.CheckIfEndOfScenario();
		UIManager.instance.UpdateVPtext();
	}

	private void ProcessTileRemoval(TileGO p_tileGO)
	{
		_selectedTiles.Remove(p_tileGO);
		for (int i = 0; i < _tileTransferSlotGOs.Count; i++)
		{
			if (_tileTransferSlotGOs[i].IsLinkedToTile(p_tileGO))
			{
				Object.Destroy(_tileTransferSlotGOs[i].gameObject);
				_tileTransferSlotGOs.Remove(_tileTransferSlotGOs[i]);
			}
		}
	}

	private void ClearAllSlots()
	{
		for (int i = 0; i < _tileTransferSlotGOs.Count; i++)
		{
			Object.Destroy(_tileTransferSlotGOs[i].gameObject);
		}
		_tileTransferSlotGOs.Clear();
	}

	private void ReorganizeSlots()
	{
		_requestSeparator.SetActive(value: false);
		_transferSeparator.SetActive(value: false);
		_annexationSeparator.SetActive(value: false);
		_transferSeparator.transform.SetParent(null);
		_requestSeparator.transform.SetParent(null);
		_annexationSeparator.transform.SetParent(null);
		foreach (TileTransferSlotGO tileTransferSlotGO in _tileTransferSlotGOs)
		{
			tileTransferSlotGO.transform.SetParent(null);
		}
		_tileTransferSlotGOs = (from x in _tileTransferSlotGOs
			orderby x.GetTileGO().tile.OriginalCountryOwner == TurnManager.currPlayer.Country descending, x.GetTileGO().tile.ownerPlayer == TurnManager.currPlayer descending
			select x).ToList();
		foreach (TileTransferSlotGO tileTransferSlotGO2 in _tileTransferSlotGOs)
		{
			Player ownerPlayer = tileTransferSlotGO2.GetTileGO().tile.ownerPlayer;
			if (ownerPlayer == null)
			{
				continue;
			}
			if (ownerPlayer == TurnManager.currPlayer)
			{
				if (!_transferSeparator.activeSelf)
				{
					_transferSeparator.transform.SetParent(_content);
					_transferSeparator.SetActive(value: true);
				}
				if (tileTransferSlotGO2.GetTileGO().tile.OriginalCountryOwner != TurnManager.currPlayer.Country && !_annexationSeparator.activeSelf)
				{
					_annexationSeparator.transform.SetParent(_content);
					_annexationSeparator.SetActive(value: true);
				}
			}
			else if (!_requestSeparator.activeSelf)
			{
				_requestSeparator.transform.SetParent(_content);
				_requestSeparator.SetActive(value: true);
			}
			tileTransferSlotGO2.transform.SetParent(_content);
		}
	}

	private void CheckForCapitulationEvents(TileGO p_tileGO)
	{
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (@event.CheckType != CheckTypes.VP_LOSS && @event.CheckType != CheckTypes.VPS_LOSSES && @event.CheckType != CheckTypes.VPS_LOSSES_ONE_OF)
			{
				continue;
			}
			List<EventAction> list = new List<EventAction>();
			bool flag = false;
			if (@event.CheckTileCoordinateX == p_tileGO.tile.PosX && @event.CheckTileCoordinateY == p_tileGO.tile.PosY)
			{
				flag = true;
			}
			else if (@event.VPsToBeLost.Contains(new Coordinates(p_tileGO.tile.PosX, p_tileGO.tile.PosY)))
			{
				@event.VPsToBeLost.Remove(new Coordinates(p_tileGO.tile.PosX, p_tileGO.tile.PosY));
				if (@event.VPsToBeLost.Count == 0)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (EventAction eventAction in @event.EventActions)
			{
				if (eventAction.Type == ActionTypes.CAPITULATION && eventAction.TargetPlayerName1 == p_tileGO.tile.ownerPlayer.Name)
				{
					list.Add(eventAction);
				}
			}
			foreach (EventAction item in list)
			{
				@event.EventActions.Remove(item);
			}
		}
	}
}
