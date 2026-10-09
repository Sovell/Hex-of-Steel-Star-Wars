using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileEditorGO : MonoBehaviour
{
	public Tile tile;

	public SpriteRenderer spriteRenderer;

	public LineRenderer lineRenderer;

	public TextMeshPro cityName;

	[Header("Runtime")]
	public GameObject groundUnitGO;

	public GameObject airUnitGO;

	public GameObject flag;

	public GameObject freezeIcon;

	public GameObject seasonChangeIcon;

	private GameObject _defensiveIcon;

	private GameObject _targetIcon;

	private List<TileEditorGO> _listOfNeighbours;

	private void OnMouseOver()
	{
		if (Input.GetKeyDown(PlayerSettings.Instance.EyedropperTile) && MapEditorGO.selectedTile != null)
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}
			MapEditorGO.selectedTile.IsReinforcementTile = tile.IsReinforcementTile;
			MapEditorGO.selectedTile.Name = tile.Name;
			MapEditorGO.selectedTile.Type = tile.Type;
			MapEditorGO.selectedTile.MPCost = tile.MPCost;
			MapEditorGO.selectedTile.FilterType = tile.FilterType;
			MapEditorGO.selectedTile.DamageModificator = tile.DamageModificator;
			MapEditorGO.selectedTile.Bridge = tile.Bridge;
			MapEditorGO.selectedTile.Railroad = tile.Railroad;
			MapEditorGO.selectedTile.Manpower = tile.Manpower;
			MapEditorGO.selectedTile.OccupiedManpower = tile.OccupiedManpower;
		}
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}
			if (Input.GetMouseButtonDown(0))
			{
				if (MapEditorGO.paintMode == PaintMode.CAN_FREEZE)
				{
					MapEditorGO.setTileValueBoolean = !tile.CanFreezeInWinter;
				}
				else if (MapEditorGO.paintMode == PaintMode.CAN_SEASON_CHANGE)
				{
					MapEditorGO.setTileValueBoolean = !tile.SeasonNeverChanges;
				}
			}
			FindNeighboursWithBrushSize();
			if (MapEditorGO.selectedTileGO != null || MapEditorGO.paintMode != PaintMode.TILE_PLACING)
			{
				if (MapEditorGO.filter == "FilterRoad" || MapEditorGO.filter == "FilterCoast" || MapEditorGO.filter == "FilterDesertRoad" || MapEditorGO.filter == "FilterRailroad" || MapEditorGO.filter == "FilterRiver" || MapEditorGO.filter == "FilterDesertRiver")
				{
					SetTile(p_isAutoRoad: true);
				}
				else
				{
					SetTile(p_isAutoRoad: false);
				}
			}
		}
		else
		{
			MapEditorGO.previousTile = null;
			MapEditorGO.previousTileRoad = null;
		}
		if (Input.GetMouseButtonDown(0) && MapEditorGO.selectedTileGO == null)
		{
			if (tile.GroundUnit != null && tile.AirUnit == null)
			{
				MapEditorGO.selectedUnitForRename = tile.GroundUnit;
			}
			else if (tile.GroundUnit == null && tile.AirUnit != null)
			{
				MapEditorGO.selectedUnitForRename = tile.AirUnit;
			}
			else if (tile.GroundUnit != null && tile.AirUnit != null)
			{
				if (MapEditorGO.selectedUnitForRename == null)
				{
					MapEditorGO.selectedUnitForRename = tile.GroundUnit;
				}
				else if (MapEditorGO.selectedUnitForRename == tile.GroundUnit)
				{
					MapEditorGO.selectedUnitForRename = tile.AirUnit;
				}
				else
				{
					MapEditorGO.selectedUnitForRename = tile.GroundUnit;
				}
			}
			if (MapEditorGO.selectedUnitForRename != null)
			{
				MapEditorGO.instance.customNameForUnit_Inputfield.SetTextWithoutNotify(MapEditorGO.selectedUnitForRename.GetUnitName());
				if (MapEditorGO.selectedUnitForRename.HPmodifierOnScenarioStart == 0 || MapEditorGO.selectedUnitForRename.HPmodifierOnScenarioStart > 100)
				{
					MapEditorGO.selectedUnitForRename.HPmodifierOnScenarioStart = (byte)100;
				}
				if (MapEditorGO.selectedUnitForRename.HPmodifierOnScenarioStart.HasValue)
				{
					MapEditorGO.instance.customHealthForUnit_Inputfield.SetTextWithoutNotify(MapEditorGO.selectedUnitForRename.HPmodifierOnScenarioStart + "%");
				}
				else
				{
					MapEditorGO.instance.customHealthForUnit_Inputfield.SetTextWithoutNotify("100%");
				}
				MapEditorGO.instance.entrenchmentOnStart_Inputfield.SetTextWithoutNotify(MapEditorGO.selectedUnitForRename.EntrenchmentLevelOnStart.ToString());
				MapEditorGO.instance.unitID_Inputfield.SetTextWithoutNotify(MapEditorGO.selectedUnitForRename.ID.ToString());
				MapEditorGO.instance.unitLevelOnStart_Inputfield.SetTextWithoutNotify(MapEditorGO.selectedUnitForRename.Lvl.ToString());
			}
			SpawnUnit(ReservePanelGO.selectedUnit);
		}
		if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
		{
			RightClickOnTile();
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.SetDefensive) && !EventSystem.current.IsPointerOverGameObject())
		{
			ManageLittleIconOverUnit("Defense");
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.SetUnitAsGoal) && !EventSystem.current.IsPointerOverGameObject())
		{
			ManageLittleIconOverUnit("Goal");
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.ChangeParatroopersState) && !EventSystem.current.IsPointerOverGameObject())
		{
			ChangeParatrooperState();
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.ChangeSubmarineState) && !EventSystem.current.IsPointerOverGameObject())
		{
			ChangeSubmarineState();
		}
	}

	public void DestroyFlag()
	{
		if ((bool)flag)
		{
			Object.Destroy(flag);
		}
	}

	private List<TileEditorGO> FindNeighboursWithBrushSize()
	{
		_listOfNeighbours = new List<TileEditorGO>();
		Collider2D[] array = ((MapEditorGO.brushSize != 1) ? Physics2D.OverlapCircleAll(gameObject.transform.position, (float)(MapEditorGO.brushSize - 1) * 4.8f) : Physics2D.OverlapCircleAll(gameObject.transform.position, MapEditorGO.brushSize));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<TileEditorGO>(out var component))
			{
				_listOfNeighbours.Add(component);
			}
		}
		return _listOfNeighbours;
	}

	private void ChangeParatrooperState()
	{
		if ((tile.GroundUnit != null && tile.GroundUnit.IsParatrooper && tile.AirUnit == null) || (tile.AirUnit != null && tile.AirUnit.IsParatrooper && tile.GroundUnit == null))
		{
			if (tile.AirUnit != null)
			{
				tile.AirUnit.IsDropable = false;
				tile.AirUnit.Type = "Infantry";
				tile.GroundUnit = tile.AirUnit;
				tile.AirUnit = null;
				groundUnitGO = airUnitGO;
				airUnitGO = null;
				UIManager.ShowMessage(LocalizationManager.Translate("info-paratroopers grounded"), 1f);
				groundUnitGO.transform.Find("Unit representation").GetComponent<SpriteRenderer>().sprite = Utils.GetUnitSprite(tile.GroundUnit);
				groundUnitGO.GetComponent<SpriteRenderer>().sprite = Utils.GetUnitBackground(tile.GroundUnit, null);
			}
			else
			{
				tile.GroundUnit.IsDropable = true;
				tile.GroundUnit.Type = "Plane";
				tile.AirUnit = tile.GroundUnit;
				tile.GroundUnit = null;
				airUnitGO = groundUnitGO;
				groundUnitGO = null;
				UIManager.ShowMessage(LocalizationManager.Translate("info-paratroopers airborne"), 1f);
				airUnitGO.transform.Find("Unit representation").GetComponent<SpriteRenderer>().sprite = Utils.GetUnitSprite(tile.AirUnit);
				airUnitGO.GetComponent<SpriteRenderer>().sprite = Utils.GetUnitBackground(tile.AirUnit, null);
			}
		}
	}

	private void ChangeSubmarineState()
	{
		if (tile.GroundUnit != null && tile.GroundUnit.IsSubmarine)
		{
			tile.GroundUnit.NumberOfTurnsUnderWater = 0;
			tile.GroundUnit.IsSubmerged = !tile.GroundUnit.IsSubmerged;
			if (tile.GroundUnit.IsSubmerged)
			{
				groundUnitGO.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);
				groundUnitGO.transform.Find("Unit representation").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);
				UIManager.ShowMessage(LocalizationManager.Translate("info-submarine submerged"), 1f);
			}
			else
			{
				groundUnitGO.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				groundUnitGO.transform.Find("Unit representation").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
				UIManager.ShowMessage(LocalizationManager.Translate("info-submarine surfaced"), 1f);
			}
		}
	}

	public void SpawnUnit(Unit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}
		if (p_unit.Type == "Boat" && tile.Type != "Sea")
		{
			if (p_unit.IsPatrolBoat && tile.Type != "River" && tile.Type != "Sea")
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invalid spawn tile ship"));
				return;
			}
			if (!p_unit.IsPatrolBoat && tile.Type != "Sea")
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-invalid spawn tile ship"));
				return;
			}
		}
		if (p_unit.IsArmouredTrain && !tile.Railroad)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid spawn tile train"));
			return;
		}
		if (((p_unit.Type == "Plane" && tile.AirUnit != null) || (p_unit.Type != "Plane" && tile.GroundUnit != null)) && !MapEditorMenu.isLoadedMapEditor)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid spawn tile busy"));
			return;
		}
		if (p_unit.TileBuilding)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid spawn unit type"));
			return;
		}
		GameObject gameObject = Object.Instantiate(MapEditorGO.instance.unitPrefab, MapEditorGO.instance.unitParent);
		gameObject.GetComponent<SpriteRenderer>().sprite = Utils.GetUnitBackground(p_unit, null);
		gameObject.transform.Find("Unit representation").GetComponent<SpriteRenderer>().sprite = Utils.GetUnitSprite(p_unit);
		if (p_unit.IsPoliticalUnit)
		{
			gameObject.transform.Find("Political").GetComponent<SpriteRenderer>().sprite = Utils.GetPoliticalSprite(p_unit);
		}
		gameObject.transform.position = new Vector3(base.gameObject.transform.position.x, base.gameObject.transform.position.y, -2f);
		gameObject.transform.localScale = new Vector3(PlayerSettings.Instance.UnitSize, PlayerSettings.Instance.UnitSize, PlayerSettings.Instance.UnitSize);
		gameObject.name = p_unit.ID.ToString();
		Transform transform = gameObject.transform.Find("Ship name");
		if (transform != null && transform.TryGetComponent<TextMeshPro>(out var component))
		{
			string counterDisplayName = p_unit.GetCounterDisplayName();
			if (PlayerSettings.Instance.ShowShipNames && !string.IsNullOrEmpty(counterDisplayName))
			{
				component.text = counterDisplayName;
				transform.gameObject.SetActive(value: true);
			}
			else
			{
				transform.gameObject.SetActive(value: false);
			}
		}
		if (p_unit.Type == "Plane")
		{
			tile.AirUnit = p_unit;
			airUnitGO = gameObject;
			GenerateLittleIconOverUnit(tile.AirUnit);
		}
		else
		{
			tile.GroundUnit = p_unit;
			groundUnitGO = gameObject;
			GenerateLittleIconOverUnit(tile.GroundUnit);
			if (tile.GroundUnit.IsSubmarine && tile.GroundUnit.IsSubmerged)
			{
				groundUnitGO.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);
				groundUnitGO.transform.Find("Unit representation").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);
			}
		}
		p_unit.IsReserve = false;
		ReservePanelGO.selectedUnit = null;
		ReservePanelGO.instance.GenerateButtonsList();
		MapGO.selectedUnitGO = null;
	}

	private void ManageLittleIconOverUnit(string p_parameter)
	{
		if (tile.AirUnit == null && tile.GroundUnit == null)
		{
			return;
		}
		if (tile.AirUnit != null)
		{
			if (p_parameter == "Defense")
			{
				tile.AirUnit.IsDefensive = !tile.AirUnit.IsDefensive;
			}
			else if (p_parameter == "Goal")
			{
				tile.AirUnit.IsKillGoal = !tile.AirUnit.IsKillGoal;
				if (tile.AirUnit.IsKillGoal)
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(tile.AirUnit, value: false);
				}
				else
				{
					GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Remove(tile.AirUnit);
				}
			}
			GenerateLittleIconOverUnit(tile.AirUnit);
		}
		if (tile.GroundUnit == null)
		{
			return;
		}
		if (p_parameter == "Defense")
		{
			tile.GroundUnit.IsDefensive = !tile.GroundUnit.IsDefensive;
		}
		else if (p_parameter == "Goal")
		{
			tile.GroundUnit.IsKillGoal = !tile.GroundUnit.IsKillGoal;
			if (tile.GroundUnit.IsKillGoal)
			{
				GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Add(tile.GroundUnit, value: false);
			}
			else
			{
				GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Remove(tile.GroundUnit);
			}
		}
		GenerateLittleIconOverUnit(tile.GroundUnit);
	}

	private void GenerateLittleIconOverUnit(Unit unit)
	{
		if (unit == null)
		{
			return;
		}
		if (unit.IsDefensive)
		{
			Object.Destroy(_defensiveIcon);
			_defensiveIcon = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Defensive"));
			if (unit.Type == "Plane")
			{
				_defensiveIcon.transform.SetParent(airUnitGO.transform);
			}
			else
			{
				_defensiveIcon.transform.SetParent(groundUnitGO.transform);
			}
			Vector3 position = new Vector3(gameObject.transform.position.x - 1.2f, gameObject.transform.position.y - 1.85f, -5f);
			_defensiveIcon.transform.position = position;
		}
		else
		{
			Object.Destroy(_defensiveIcon);
		}
		if (unit.IsKillGoal)
		{
			Object.Destroy(_targetIcon);
			_targetIcon = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Unit goal"));
			if (unit.Type == "Plane")
			{
				_targetIcon.transform.SetParent(airUnitGO.transform);
			}
			else
			{
				_targetIcon.transform.SetParent(groundUnitGO.transform);
			}
			Vector3 position2 = new Vector3(gameObject.transform.position.x + 1.2f, gameObject.transform.position.y + 1.85f, -5f);
			_targetIcon.transform.position = position2;
		}
		else
		{
			Object.Destroy(_targetIcon);
		}
	}

	private void RightClickOnTile()
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			MapEditorGO.selectedPlacedTile = gameObject;
			MapEditorGO.instance.rightClick_Panel.SetActive(value: true);
			MapEditorGO.instance.originalOwnerCountry_inputfield.SetTextWithoutNotify(tile.OriginalCountryOwner);
			MapEditorGO.instance.customIncome_Inputfield.SetTextWithoutNotify((tile.EditorOverwrite_income != 0) ? tile.EditorOverwrite_income.ToString() : tile.MoneyProductionPerTurn.ToString());
			MapEditorGO.instance.customManpower_Inputfield.SetTextWithoutNotify((tile.EditorOverwrite_manpower != 0) ? tile.EditorOverwrite_manpower.ToString() : tile.Manpower.ToString());
			MapEditorGO.instance.cityName_InputField.SetTextWithoutNotify(tile.CityName);
			MapEditorGO.instance.unitLevelOnStart_Inputfield.gameObject.SetActive(tile.GroundUnit != null || tile.AirUnit != null);
			MapEditorGO.instance.rightClick_Panel.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			MapEditorGO.instance.selectedTileCoordinates_text.text = LocalizationManager.Translate("txt-coordinates") + tile.PosX + ", " + tile.PosY;
			MapEditorGO.instance.removeUnitButton.SetActive(tile.GroundUnit != null || tile.AirUnit != null);
			MapEditorGO.instance.relocateUnitButton.SetActive(tile.GroundUnit != null || tile.AirUnit != null);
			MapEditorGO.instance.noAmmoTurn1Button.SetActive(tile.GroundUnit != null || tile.AirUnit != null);
			MapEditorGO.instance.noEntrenchmentTurn1Button.SetActive(tile.GroundUnit != null || tile.AirUnit != null);
			MapEditorGO.instance.setVpToBeHeldButton.SetActive(tile.IsVictoryPoint);
		}
	}

	public void ManageVPToBeHeld()
	{
		if (GameData.Instance.map.listOfVPThatNeedsToBeHeld.ContainsKey(tile))
		{
			spriteRenderer.color = Color.white;
			GameData.Instance.map.listOfVPThatNeedsToBeHeld.Remove(tile);
			return;
		}
		if (TurnManager.currPlayer == null)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-no player selected"));
			return;
		}
		foreach (KeyValuePair<Tile, string> item in GameData.Instance.map.listOfVPThatNeedsToBeHeld)
		{
			if (item.Key == tile)
			{
				return;
			}
		}
		GameData.Instance.map.listOfVPThatNeedsToBeHeld.Add(tile, TurnManager.currPlayer.Faction);
		if (TurnManager.currPlayer.Faction == "Axis")
		{
			spriteRenderer.color = Color.red;
		}
		else if (TurnManager.currPlayer.Faction == "Allies")
		{
			spriteRenderer.color = Color.blue;
		}
		else if (TurnManager.currPlayer.Faction == "Communists")
		{
			spriteRenderer.color = Color.blue;
		}
		else
		{
			spriteRenderer.color = Color.magenta;
		}
	}

	private void SetTile(bool p_isAutoRoad)
	{
		foreach (TileEditorGO listOfNeighbour in _listOfNeighbours)
		{
			Tile tile = listOfNeighbour.tile;
			if (MapEditorGO.paintMode == PaintMode.TILE_PLACING)
			{
				if (p_isAutoRoad && MapEditorGO.selectedTileGO.name.Contains("Road"))
				{
					SetAutoRoads();
				}
				else if (p_isAutoRoad && MapEditorGO.selectedTileGO.name.Contains("Railroad"))
				{
					SetAutoRailroads();
				}
				else if (p_isAutoRoad && MapEditorGO.selectedTileGO.name.Contains("River"))
				{
					SetAutoRivers();
				}
				else if (p_isAutoRoad && MapEditorGO.selectedTileGO.name.Contains("Coast"))
				{
					Utils.SetAutoCoasts(this.tile);
				}
				if (MapEditorGO.selectedTileGO.name.Contains("Road") && MapEditorGO.previousTile != null)
				{
					ConnectRoadToCityOnEnd();
				}
				tile.Season = MapEditorGO.season;
				tile.Type = MapEditorGO.selectedTile.Type;
				tile.MPCost = MapEditorGO.selectedTile.MPCost;
				tile.DamageModificator = MapEditorGO.selectedTile.DamageModificator;
				tile.MoneyProductionPerTurn = MapEditorGO.selectedTile.MoneyProductionPerTurn;
				tile.MoneyReward = MapEditorGO.selectedTile.MoneyReward;
				tile.Name = MapEditorGO.selectedTile.Name;
				tile.IsReinforcementTile = MapEditorGO.selectedTile.IsReinforcementTile;
				tile.Railroad = MapEditorGO.selectedTile.Railroad;
				tile.IsOfficial = MapEditorGO.selectedTile.IsOfficial;
				tile.Bridge = MapEditorGO.selectedTile.Bridge;
				tile.Manpower = MapEditorGO.selectedTile.Manpower;
				tile.OccupiedManpower = MapEditorGO.selectedTile.OccupiedManpower;
				tile.IsBridgeDestroyed = false;
				tile.isFrozen = false;
				if (tile.CanFreezeInWinter && tile.Type != "Sea" && tile.Type != "River" && !tile.Bridge)
				{
					tile.CanFreezeInWinter = false;
					Object.Destroy(freezeIcon);
				}
				listOfNeighbour.spriteRenderer.sprite = Utils.GetTileSprite(tile);
			}
			else if (MapEditorGO.paintMode == PaintMode.CAN_FREEZE)
			{
				if (tile.Type == "River" || tile.Type == "Sea" || tile.Bridge)
				{
					tile.CanFreezeInWinter = MapEditorGO.setTileValueBoolean;
					Object.Destroy(listOfNeighbour.freezeIcon);
					if (tile.CanFreezeInWinter)
					{
						listOfNeighbour.freezeIcon = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Can Freeze"), listOfNeighbour.transform, worldPositionStays: false);
					}
				}
			}
			else if (MapEditorGO.paintMode == PaintMode.CAN_SEASON_CHANGE)
			{
				if (tile.Season != "Desert")
				{
					tile.SeasonNeverChanges = MapEditorGO.setTileValueBoolean;
					Object.Destroy(listOfNeighbour.seasonChangeIcon);
					if (tile.SeasonNeverChanges)
					{
						listOfNeighbour.seasonChangeIcon = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/No Season Change"), listOfNeighbour.transform, worldPositionStays: false);
					}
				}
			}
			else if (MapEditorGO.paintMode == PaintMode.UNIT_REMOVAL)
			{
				listOfNeighbour.RemoveUnits();
			}
		}
		if (MapEditorGO.paintMode == PaintMode.TILE_PLACING)
		{
			MapEditorGO.instance.GenerateBorders();
		}
	}

	private void ConnectRoadToCityOnEnd()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.gameObject.transform.position, 4.8f);
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = array[i].gameObject;
			if (!(gameObject.GetComponent<TileEditorGO>().tile.Type == "City") && !(gameObject.GetComponent<TileEditorGO>().tile.Type == "Harbour"))
			{
				continue;
			}
			float x = MapEditorGO.previousTile.transform.position.x;
			float y = MapEditorGO.previousTile.transform.position.y;
			float x2 = gameObject.gameObject.transform.position.x;
			float y2 = gameObject.gameObject.transform.position.y;
			string text = MapEditorGO.selectedTile.Name;
			if (x2 == x && y2 > y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road1";
				}
				if (MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road2")
				{
					text = "Road11";
				}
				if (MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road2")
				{
					text = "Road12";
				}
			}
			else if (x2 == x && y2 < y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road1";
				}
				if (MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road3")
				{
					text = "Road13";
				}
				if (MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road3")
				{
					text = "Road10";
				}
			}
			else if (x2 > x && y2 > y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road13";
				}
				if (MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road2" || MapEditorGO.previousTileRoad == "Road4")
				{
					text = "Road4";
				}
				if (MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road3" || MapEditorGO.previousTileRoad == "Road12")
				{
					text = "Road2";
				}
			}
			else if (x2 > x && y2 < y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road12";
				}
				if (MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road3" || MapEditorGO.previousTileRoad == "Road12")
				{
					text = "Road5";
				}
				if (MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road2" || MapEditorGO.previousTileRoad == "Road13")
				{
					text = "Road3";
				}
			}
			else if (x2 < x && y2 > y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road10";
				}
				if (MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road2" || MapEditorGO.previousTileRoad == "Road10")
				{
					text = "Road5";
				}
				if (MapEditorGO.previousTileRoad == "Road3" || MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road11")
				{
					text = "Road2";
				}
			}
			else if (x2 < x && y2 < y)
			{
				if (MapEditorGO.previousTileRoad == "Road1")
				{
					text = "Road11";
				}
				if (MapEditorGO.previousTileRoad == "Road4" || MapEditorGO.previousTileRoad == "Road3" || MapEditorGO.previousTileRoad == "Road11")
				{
					text = "Road4";
				}
				if (MapEditorGO.previousTileRoad == "Road2" || MapEditorGO.previousTileRoad == "Road5" || MapEditorGO.previousTileRoad == "Road10")
				{
					text = "Road3";
				}
			}
			MapEditorGO.selectedTile.Name = text;
		}
	}

	private void SetAutoRoads()
	{
		if (MapEditorGO.previousTile == null)
		{
			MapEditorGO.previousTile = gameObject;
		}
		if (MapEditorGO.previousTile == gameObject)
		{
			return;
		}
		float x = MapEditorGO.previousTile.transform.position.x;
		float y = MapEditorGO.previousTile.transform.position.y;
		float x2 = gameObject.transform.position.x;
		float y2 = gameObject.transform.position.y;
		string text = "";
		Tile tile = MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile;
		Collider2D[] array = Physics2D.OverlapCircleAll(MapEditorGO.previousTile.transform.position, 4.8f);
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i].gameObject;
			if (x == x2 && y < y2)
			{
				if (tile.Name == "Road1")
				{
					text = "Road1";
				}
				else if (tile.Name == "Road4" || tile.Name == "Road13")
				{
					text = "Road11";
				}
				else if (tile.Name == "Road11" || tile.Name == "Road12")
				{
					text = "Road1";
				}
				else if (tile.Name == "Road5")
				{
					text = "Road12";
				}
			}
			else if (x == x2 && y > y2)
			{
				if (tile.Name == "Road1")
				{
					text = "Road1";
				}
				else if (tile.Name == "Road10" || tile.Name == "Road13")
				{
					text = "Road1";
				}
				else if (tile.Name == "Road4" || tile.Name == "Road11")
				{
					text = "Road13";
				}
				else if (tile.Name == "Road5" || tile.Name == "Road12")
				{
					text = "Road10";
				}
			}
			else if (x < x2 && y < y2)
			{
				if (tile.Name == "Road1" || tile.Name == "Road12" || tile.Name == "Road11")
				{
					text = "Road13";
				}
				else if (tile.Name == "Road13" || tile.Name == "Road4" || tile.Name == "Road2")
				{
					text = "Road4";
				}
				else if (tile.Name == "Road5" || tile.Name == "Road12" || tile.Name == "Road3")
				{
					text = "Road2";
				}
			}
			else if (x > x2 && y < y2)
			{
				if (tile.Name == "Road1" || tile.Name == "Road12" || tile.Name == "Road11")
				{
					text = "Road10";
				}
				else if (tile.Name == "Road5" || tile.Name == "Road10" || tile.Name == "Road2")
				{
					text = "Road5";
				}
				else if (tile.Name == "Road4" || tile.Name == "Road11" || tile.Name == "Road3")
				{
					text = "Road2";
				}
			}
			else if (x < x2 && y > y2)
			{
				if (tile.Name == "Road1" || tile.Name == "Road10" || tile.Name == "Road13")
				{
					text = "Road12";
				}
				else if (tile.Name == "Road12" || tile.Name == "Road5" || tile.Name == "Road3")
				{
					text = "Road5";
				}
				else if (tile.Name == "Road4" || tile.Name == "Road13" || tile.Name == "Road2")
				{
					text = "Road3";
				}
			}
			else if (x > x2 && y > y2)
			{
				if (tile.Name == "Road1" || tile.Name == "Road10" || tile.Name == "Road13")
				{
					text = "Road11";
				}
				else if (tile.Name == "Road11" || tile.Name == "Road4" || tile.Name == "Road3")
				{
					text = "Road4";
				}
				else if (tile.Name == "Road5" || tile.Name == "Road10" || tile.Name == "Road2")
				{
					text = "Road3";
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			MapEditorGO.selectedTile.Name = text;
			MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile.Name = text;
			MapEditorGO.previousTileRoad = text;
			MapEditorGO.previousTile.GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile);
			MapEditorGO.previousTile = gameObject;
		}
	}

	private void SetAutoRailroads()
	{
		if (MapEditorGO.previousTile == null)
		{
			MapEditorGO.previousTile = gameObject;
		}
		else
		{
			if (MapEditorGO.previousTile == gameObject)
			{
				return;
			}
			float x = MapEditorGO.previousTile.transform.position.x;
			float y = MapEditorGO.previousTile.transform.position.y;
			float x2 = gameObject.transform.position.x;
			float y2 = gameObject.transform.position.y;
			string text = "";
			Tile tile = MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile;
			Collider2D[] array = Physics2D.OverlapCircleAll(MapEditorGO.previousTile.transform.position, 4.8f);
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i].gameObject;
				if (x == x2 && y < y2)
				{
					if (tile.Name == "Railroad1")
					{
						text = "Railroad1";
					}
					else if (tile.Name == "Railroad2" || tile.Name == "Railroad7")
					{
						text = "Railroad9";
					}
					else if (tile.Name == "Railroad9" || tile.Name == "Railroad6")
					{
						text = "Railroad1";
					}
					else if (tile.Name == "Railroad3")
					{
						text = "Railroad6";
					}
				}
				else if (x == x2 && y > y2)
				{
					if (tile.Name == "Railroad1")
					{
						text = "Railroad1";
					}
					else if (tile.Name == "Railroad8" || tile.Name == "Railroad7")
					{
						text = "Railroad1";
					}
					else if (tile.Name == "Railroad2" || tile.Name == "Railroad9")
					{
						text = "Railroad7";
					}
					else if (tile.Name == "Railroad3" || tile.Name == "Railroad6")
					{
						text = "Railroad8";
					}
				}
				else if (x < x2 && y < y2)
				{
					if (tile.Name == "Railroad1" || tile.Name == "Railroad6" || tile.Name == "Railroad9")
					{
						text = "Railroad7";
					}
					else if (tile.Name == "Railroad7" || tile.Name == "Railroad2" || tile.Name == "Railroad4")
					{
						text = "Railroad2";
					}
					else if (tile.Name == "Railroad3" || tile.Name == "Railroad6" || tile.Name == "Railroad5")
					{
						text = "Railroad4";
					}
				}
				else if (x > x2 && y < y2)
				{
					if (tile.Name == "Railroad1" || tile.Name == "Railroad6" || tile.Name == "Railroad9")
					{
						text = "Railroad8";
					}
					else if (tile.Name == "Railroad3" || tile.Name == "Railroad8" || tile.Name == "Railroad4")
					{
						text = "Railroad3";
					}
					else if (tile.Name == "Railroad2" || tile.Name == "Railroad9" || tile.Name == "Railroad5")
					{
						text = "Railroad4";
					}
				}
				else if (x < x2 && y > y2)
				{
					if (tile.Name == "Railroad1" || tile.Name == "Railroad8" || tile.Name == "Railroad7")
					{
						text = "Railroad6";
					}
					else if (tile.Name == "Railroad6" || tile.Name == "Railroad3" || tile.Name == "Railroad5")
					{
						text = "Railroad3";
					}
					else if (tile.Name == "Railroad2" || tile.Name == "Railroad7" || tile.Name == "Railroad4")
					{
						text = "Railroad5";
					}
				}
				else if (x > x2 && y > y2)
				{
					if (tile.Name == "Railroad1" || tile.Name == "Railroad8" || tile.Name == "Railroad7")
					{
						text = "Railroad9";
					}
					else if (tile.Name == "Railroad9" || tile.Name == "Railroad2" || tile.Name == "Railroad5")
					{
						text = "Railroad2";
					}
					else if (tile.Name == "Railroad3" || tile.Name == "Railroad8" || tile.Name == "Railroad4")
					{
						text = "Railroad5";
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				MapEditorGO.selectedTile.Name = text;
				MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile.Name = text;
				MapEditorGO.previousTileRoad = text;
				MapEditorGO.previousTile.GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile);
				MapEditorGO.previousTile = gameObject;
			}
		}
	}

	private void SetAutoRivers()
	{
		if (MapEditorGO.previousTile == null)
		{
			MapEditorGO.previousTile = gameObject;
		}
		else
		{
			if (MapEditorGO.previousTile == gameObject)
			{
				return;
			}
			float x = MapEditorGO.previousTile.transform.position.x;
			float y = MapEditorGO.previousTile.transform.position.y;
			float x2 = gameObject.transform.position.x;
			float y2 = gameObject.transform.position.y;
			string text = "";
			Tile tile = MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile;
			Collider2D[] array = Physics2D.OverlapCircleAll(MapEditorGO.previousTile.transform.position, 4.8f);
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i].gameObject;
				if (x == x2 && y < y2)
				{
					if (tile.Name == "River1")
					{
						text = "River1";
					}
					else if (tile.Name == "River2" || tile.Name == "River8")
					{
						text = "River6";
					}
					else if (tile.Name == "River6" || tile.Name == "River7")
					{
						text = "River1";
					}
					else if (tile.Name == "River3")
					{
						text = "River7";
					}
				}
				else if (x == x2 && y > y2)
				{
					if (tile.Name == "River1")
					{
						text = "River1";
					}
					else if (tile.Name == "River9" || tile.Name == "River8")
					{
						text = "River1";
					}
					else if (tile.Name == "River2" || tile.Name == "River6")
					{
						text = "River8";
					}
					else if (tile.Name == "River3" || tile.Name == "River7")
					{
						text = "River9";
					}
				}
				else if (x < x2 && y < y2)
				{
					if (tile.Name == "River1" || tile.Name == "River7" || tile.Name == "River6")
					{
						text = "River8";
					}
					else if (tile.Name == "River8" || tile.Name == "River2" || tile.Name == "River4")
					{
						text = "River2";
					}
					else if (tile.Name == "River3" || tile.Name == "River7" || tile.Name == "River5")
					{
						text = "River4";
					}
				}
				else if (x > x2 && y < y2)
				{
					if (tile.Name == "River1" || tile.Name == "River7" || tile.Name == "River6")
					{
						text = "River9";
					}
					else if (tile.Name == "River3" || tile.Name == "River9" || tile.Name == "River4")
					{
						text = "River3";
					}
					else if (tile.Name == "River2" || tile.Name == "River6" || tile.Name == "River5")
					{
						text = "River4";
					}
				}
				else if (x < x2 && y > y2)
				{
					if (tile.Name == "River1" || tile.Name == "River9" || tile.Name == "River8")
					{
						text = "River7";
					}
					else if (tile.Name == "River7" || tile.Name == "River3" || tile.Name == "River5")
					{
						text = "River3";
					}
					else if (tile.Name == "River2" || tile.Name == "River8" || tile.Name == "River4")
					{
						text = "River5";
					}
				}
				else if (x > x2 && y > y2)
				{
					if (tile.Name == "River1" || tile.Name == "River9" || tile.Name == "River8")
					{
						text = "River6";
					}
					else if (tile.Name == "River6" || tile.Name == "River2" || tile.Name == "River5")
					{
						text = "River2";
					}
					else if (tile.Name == "River3" || tile.Name == "River9" || tile.Name == "River4")
					{
						text = "River5";
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				MapEditorGO.selectedTile.Name = text;
				MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile.Name = text;
				MapEditorGO.previousTileRoad = text;
				MapEditorGO.previousTile.GetComponent<SpriteRenderer>().sprite = Utils.GetTileSprite(MapEditorGO.previousTile.GetComponent<TileEditorGO>().tile);
				MapEditorGO.previousTile = gameObject;
			}
		}
	}

	public void RemoveUnits()
	{
		if (tile.AirUnit != null)
		{
			for (int i = 0; i < tile.AirUnit.OwnerPlayer.ListOfUnits.Count; i++)
			{
				if (tile.AirUnit.OwnerPlayer.ListOfUnits[i].ID == tile.AirUnit.ID)
				{
					if (tile.AirUnit.IsKillGoal)
					{
						GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Remove(tile.AirUnit);
					}
					if (tile.AirUnit.MaxPurchaseTime > 0 && tile.AirUnit.OwnerPlayer.PurchasedUnits.ContainsKey(tile.AirUnit.Name) && tile.AirUnit.OwnerPlayer.PurchasedUnits[tile.AirUnit.Name] > 0)
					{
						tile.AirUnit.OwnerPlayer.PurchasedUnits[tile.AirUnit.Name]--;
					}
					tile.AirUnit.OwnerPlayer.ListOfUnits.RemoveAt(i);
					break;
				}
			}
		}
		if (tile.GroundUnit != null)
		{
			for (int j = 0; j < tile.GroundUnit.OwnerPlayer.ListOfUnits.Count; j++)
			{
				if (tile.GroundUnit.OwnerPlayer.ListOfUnits[j].ID == tile.GroundUnit.ID)
				{
					if (tile.GroundUnit.IsKillGoal)
					{
						GameData.Instance.map.listOfUnitsThatNeedToBeKilled.Remove(tile.GroundUnit);
					}
					if (tile.GroundUnit.MaxPurchaseTime > 0 && tile.GroundUnit.OwnerPlayer.PurchasedUnits.ContainsKey(tile.GroundUnit.Name) && tile.GroundUnit.OwnerPlayer.PurchasedUnits[tile.GroundUnit.Name] > 0)
					{
						tile.GroundUnit.OwnerPlayer.PurchasedUnits[tile.GroundUnit.Name]--;
					}
					tile.GroundUnit.OwnerPlayer.ListOfUnits.RemoveAt(j);
					break;
				}
			}
		}
		tile.GroundUnit = null;
		tile.AirUnit = null;
		Object.Destroy(groundUnitGO);
		Object.Destroy(airUnitGO);
	}
}
