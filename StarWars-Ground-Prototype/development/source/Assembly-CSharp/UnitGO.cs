using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitGO : MonoBehaviour
{
	private readonly struct SupplySourceCandidate
	{
		public readonly Tile sourceTile;

		public readonly float distance;

		public readonly float baseSupplyOutput;

		public readonly int maxStoredCostExclusive;

		public SupplySourceCandidate(Tile p_sourceTile, float p_distance, float p_baseSupplyOutput)
		{
			sourceTile = p_sourceTile;
			distance = p_distance;
			baseSupplyOutput = p_baseSupplyOutput;
			maxStoredCostExclusive = Mathf.CeilToInt(p_baseSupplyOutput + (float)(int)p_sourceTile.MPCost);
		}
	}

	private struct SupplyRouteData
	{
		public Tile sourceTile;

		public float routeCost;

		public int pathLength;

		public int dangerScore;

		public SupplyRouteData(Tile p_source, float p_routeCost, int p_pathLength, int p_dangerScore)
		{
			sourceTile = p_source;
			routeCost = p_routeCost;
			pathLength = p_pathLength;
			dangerScore = p_dangerScore;
		}
	}

	[Header("Sprites")]
	[SerializeField]
	private Sprite _roundCoreIcon_sprite;

	[SerializeField]
	private Sprite _squareCoreIcon_sprite;

	[SerializeField]
	private Sprite _noCoreIcon_sprite;

	[Header("Spriterenderer")]
	public SpriteRenderer backgroundSprite;

	public SpriteRenderer unitSprite;

	public SpriteRenderer political_Sprite;

	[SerializeField]
	private SpriteRenderer coreUnitOutline_sprite;

	[SerializeField]
	private SpriteRenderer _levelIcon_spriteRenderer;

	[Header("Heroes")]
	[SerializeField]
	private SpriteRenderer heroIcon_Sprite;

	[SerializeField]
	private Vector3 _heroIconNATOon_position;

	[SerializeField]
	private Vector3 _heroIconNATOoff_position;

	[Header("Text Meshes")]
	public TextMeshPro range;

	public TextMeshPro movementPoint;

	public TextMeshPro softAttack;

	public TextMeshPro hardAttack;

	public TextMeshPro airAttack;

	public TextMeshPro number_textMesh;

	public TextMeshPro HP_TextMesh;

	public TextMeshPro shipName_textMesh;

	[Header("Sprite renderers")]
	public SpriteRenderer canAttack_Sprite;

	public SpriteRenderer canMove_Sprite;

	public SpriteRenderer ammo_Sprite;

	public SpriteRenderer fuel_Sprite;

	[SerializeField]
	private SpriteRenderer morale_GO;

	[SerializeField]
	private SpriteRenderer twoUnitsIndicator_spriteRenderer;

	[Header("Gameobjects")]
	public GameObject unitInfoContainer;

	public GameObject subContainer_GO;

	public GameObject horse_GO;

	public GameObject motorized_GO;

	public GameObject mechanized_GO;

	public GameObject elite_GO;

	public GameObject dropTank_GO;

	public GameObject bomb_GO;

	public GameObject rocket_GO;

	public GameObject torpedo_GO;

	public GameObject anchor_GO;

	public GameObject seaplane_GO;

	public GameObject mountaineer_GO;

	public GameObject winter_GO;

	public GameObject ABomb_GO;

	public GameObject Recon_GO;

	[SerializeField]
	private GameObject levelIconGO;

	public GameObject twoUnitsIndicatorGO;

	[Header("Undo")]
	public bool canUndo;

	public short backupHowManyTimesUnitMoved;

	public byte backupHowManyTimesTilesTheUnitMoved;

	public byte backupEntrenchmentLevel;

	public bool backupHasAttacked;

	public byte prevMP;

	public short prevAutonomy;

	public int prevHP;

	public TileGO prevTile;

	public List<TileGO> listOfNewlyDiscoveredTiles;

	public Unit unit;

	public UnitGO targetedUnitGO;

	public TileGO tileGO;

	public List<TileGO> listOfpotentialTiles;

	public bool isMoving;

	public bool isRetreating;

	public bool canOverrun;

	private bool overrunning;

	[SerializeField]
	private float _speed = 25f;

	private int _moveStepIndex;

	private int _moveTotalSteps;

	private Coroutine _moveAnimCoroutine;

	private Coroutine _swapLerpCoroutine;

	public List<TileGO> currentPath;

	public List<TileGO> supplyLine;

	public TileGO goal;

	public List<TileGO> AIpathToVP;

	private Transform attackerTransform;

	private Transform defenserTransform;

	public int HPsavedForRetaliation;

	public byte moraleBackupRetaliation;

	public float spawnTime;

	private bool _surrendered;

	private bool _captured;

	private TileGO tileToOverrun;

	private bool _convoyArrived;

	private bool _missileArrived;

	private int _attackMotionTweenId = -1;

	private float _HPperc;

	private int _prevHPtextValue = -1;

	private int _prevRangeTextValue = -1;

	private int _prevMPtextValue = -1;

	private int _prevSuffixTextValue = -1;

	private double _prevAirAttackTextValue = -1.0;

	private double _prevSoftAttackTextValue = -1.0;

	private double _prevHardAttackTextValue = -1.0;

	private bool _prevCoreValue;

	private bool _counterValuesShown;

	private bool _textObjectsShown;

	private bool _hidingHP;

	private short _prevUnitLevel = -1;

	private bool _moraleUIShown;

	private bool _twoUnitsIndicatorShown;

	private bool _isFuelUIshown;

	private bool _isAmmoUIshown;

	private bool _isSubcontainerUIshown;

	private byte _fuelIconColor;

	private byte _ammoIconColor;

	private bool _isCanAttackUIShown;

	private bool _isCanMoveUIShown;

	public void UpdateCounter()
	{
		if (unit.CurrHP < 0 || backgroundSprite == null || unitSprite == null)
		{
			return;
		}
		bool flag = false;
		if (unit.IsMine)
		{
			if (TurnManager.humanPlayer != null)
			{
				if (TurnManager.humanPlayer.IsAlliedWith(unit.OwnerPlayer))
				{
					flag = true;
				}
			}
			else if (TurnManager.currPlayer.IsAlliedWith(unit.OwnerPlayer))
			{
				flag = true;
			}
			if (!flag)
			{
				bool flag2 = unit.Type == "Boat";
				foreach (Tile neighbour in tileGO.tile.neighbours)
				{
					if (neighbour.GroundUnit == null || ((!flag2 || !neighbour.GroundUnit.IsDestroyer) && (flag2 || (!neighbour.GroundUnit.IsEngineers && !neighbour.GroundUnit.IsMineFlail))))
					{
						continue;
					}
					if (TurnManager.humanPlayer != null)
					{
						if (TurnManager.humanPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
						{
							flag = true;
						}
					}
					else if (TurnManager.currPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		if (tileGO.isInFogOfWar || (unit.IsMine && !flag))
		{
			backgroundSprite.color = new Color(1f, 1f, 1f, 0f);
			unitSprite.color = new Color(1f, 1f, 1f, 0f);
			unitInfoContainer.SetActive(value: false);
			political_Sprite.gameObject.SetActive(value: false);
			heroIcon_Sprite.gameObject.SetActive(value: false);
			if (PlayerSettings.Instance.DisplayUnitsLevelOnCounter)
			{
				levelIconGO.SetActive(value: false);
			}
			if (unit.IsCoreUnit)
			{
				coreUnitOutline_sprite.gameObject.SetActive(value: false);
			}
			if (shipName_textMesh != null)
			{
				shipName_textMesh.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (!unit.IsSubmerged)
			{
				backgroundSprite.color = Color.white;
				unitSprite.color = Color.white;
				if (unit.IsCoreUnit)
				{
					coreUnitOutline_sprite.color = Color.white;
				}
			}
			else
			{
				backgroundSprite.color = new Color(1f, 1f, 1f, 0.35f);
				unitSprite.color = new Color(1f, 1f, 1f, 0.35f);
				if (unit.IsCoreUnit)
				{
					coreUnitOutline_sprite.color = new Color(1f, 1f, 1f, 0.35f);
				}
			}
			unitInfoContainer.SetActive(value: true);
			if (unit.Lvl > 1 && PlayerSettings.Instance.DisplayUnitsLevelOnCounter && (unit.AirDamage > 0 || unit.SoftDamage > 0 || unit.HardDamage > 0))
			{
				levelIconGO.SetActive(value: true);
			}
			political_Sprite.gameObject.SetActive(unit.IsPoliticalUnit);
			heroIcon_Sprite.gameObject.SetActive(unit.Hero != null);
			if (unit.IsCoreUnit)
			{
				coreUnitOutline_sprite.gameObject.SetActive(value: true);
			}
			if (PlayerSettings.Instance.ShowShipNames && shipName_textMesh != null)
			{
				shipName_textMesh.gameObject.SetActive(!UIManager.isNATO && !string.IsNullOrEmpty(shipName_textMesh.text));
			}
			if (heroIcon_Sprite.gameObject.activeSelf)
			{
				heroIcon_Sprite.transform.localPosition = (UIManager.isNATO ? _heroIconNATOon_position : _heroIconNATOoff_position);
			}
		}
		UpdateCounterValues();
		UpdateMorale();
		ManageUnitInfoContainer();
		CheckPlaneFuel();
		ManageTwoUnitsIndicator();
		if (!PlayerSettings.Instance.UnitIsDoneIndicator || unit.OwnerPlayer.IsComputer || !TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer) || tileGO.isInFogOfWar)
		{
			return;
		}
		if (unit.ActionPoints <= 0 || unit.AsleepState == AsleepStates.SLEEP || unit.AsleepState == AsleepStates.SKIP || (unit.HasAttacked && unit.MaxMP == 0) || (unit.MoveOrAttack && (unit.HasMoved || unit.HasAttacked)))
		{
			if (MultiplayerManager.IsRealtimeMultiplayer() && !unit.OwnerPlayer.IsLocalClientPlayer())
			{
				return;
			}
			if (unit.IsSubmerged)
			{
				backgroundSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
				unitSprite.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
				if (unit.IsCoreUnit)
				{
					coreUnitOutline_sprite.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
				}
			}
			else
			{
				backgroundSprite.color = new Color(0.5f, 0.5f, 0.5f);
				unitSprite.color = new Color(0.5f, 0.5f, 0.5f);
				if (unit.IsCoreUnit)
				{
					coreUnitOutline_sprite.color = new Color(0.5f, 0.5f, 0.5f);
				}
			}
		}
		else if (unit.IsSubmerged)
		{
			backgroundSprite.color = new Color(1f, 1f, 1f, 0.35f);
			unitSprite.color = new Color(1f, 1f, 1f, 0.35f);
			if (unit.IsCoreUnit)
			{
				coreUnitOutline_sprite.color = new Color(1f, 1f, 1f, 0.35f);
			}
		}
		else
		{
			backgroundSprite.color = Color.white;
			unitSprite.color = Color.white;
			if (unit.IsCoreUnit)
			{
				coreUnitOutline_sprite.color = Color.white;
			}
		}
	}

	private void UpdateCounterValues()
	{
		_HPperc = unit.GetHPPercent(p_forDamagePurposes: true);
		bool flag = true;
		if (!MapGO.IsAIOnly)
		{
			flag = !PlayerSettings.Instance.HideEnemyHP || (unit.OwnerPlayer.isAlliedWithHumanPlayer ? true : false);
		}
		if (flag)
		{
			if (_hidingHP || _prevHPtextValue != unit.CurrHP)
			{
				_hidingHP = false;
				HP_TextMesh.text = unit.CurrHP.ToString();
			}
		}
		else if (!_hidingHP)
		{
			_hidingHP = true;
			HP_TextMesh.text = "?";
		}
		_prevHPtextValue = unit.CurrHP;
		if (unit.IsCoreUnit != _prevCoreValue)
		{
			if (unit.IsCoreUnit)
			{
				HP_TextMesh.color = MainMenu.blackColor;
			}
			else
			{
				HP_TextMesh.color = MainMenu.greyColor;
			}
			_prevCoreValue = unit.IsCoreUnit;
		}
		if (unit.Lvl > 1 && PlayerSettings.Instance.DisplayUnitsLevelOnCounter)
		{
			if (unit.Lvl != _prevUnitLevel)
			{
				_prevUnitLevel = unit.Lvl;
				_levelIcon_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Ranks/Outline/" + unit.Lvl);
			}
			Vector3 localPosition = levelIconGO.transform.localPosition;
			if (unit.Type == "Infantry")
			{
				localPosition.x = -4.3f;
				localPosition.y = 1.75f;
			}
			else if (unit.Type == "Tank")
			{
				localPosition.x = -4.25f;
				localPosition.y = 1.3f;
			}
			else if (unit.Type == "Plane")
			{
				localPosition.x = -4.96f;
				localPosition.y = 1.34f;
			}
			else if (unit.Type == "Boat")
			{
				localPosition.x = -4.25f;
				localPosition.y = 2f;
			}
			else if (unit.Type == "Artillery")
			{
				localPosition.x = -4.25f;
				localPosition.y = 0.5f;
			}
			else
			{
				localPosition.x = -4.25f;
				localPosition.y = 0f;
			}
			levelIconGO.transform.localPosition = localPosition;
		}
		if (PlayerSettings.Instance.HideUnitsStats)
		{
			if (!PlayerSettings.Instance.ShowUnitsNumbersOnCounters)
			{
				return;
			}
			if (unit.CategoryID == 0 || unit.MaxMP == 0 || tileGO.isInFogOfWar)
			{
				number_textMesh.fontSize = 0f;
				return;
			}
			if (_prevSuffixTextValue != unit.CategoryID)
			{
				_prevSuffixTextValue = unit.CategoryID;
				number_textMesh.text = unit.CategoryID + unit.GetSuffix();
			}
			if (!_textObjectsShown)
			{
				_textObjectsShown = true;
				number_textMesh.gameObject.SetActive(value: true);
			}
			number_textMesh.fontSize = 16f;
			return;
		}
		if (_prevRangeTextValue != unit.Range)
		{
			_prevRangeTextValue = unit.Range;
			range.text = unit.Range.ToString();
		}
		if (_prevMPtextValue != unit.MaxMP)
		{
			_prevMPtextValue = unit.MaxMP;
			movementPoint.text = unit.MaxMP.ToString();
		}
		double num = Math.Round((float)unit.SoftDamage * _HPperc, MidpointRounding.AwayFromZero);
		if (_prevSoftAttackTextValue != num)
		{
			_prevSoftAttackTextValue = num;
			softAttack.text = num.ToString();
		}
		double num2 = Math.Round((float)unit.HardDamage * _HPperc, MidpointRounding.AwayFromZero);
		if (_prevHardAttackTextValue != num2)
		{
			_prevHardAttackTextValue = num2;
			hardAttack.text = num2.ToString();
		}
		double num3 = Math.Round((float)unit.AirDamage * _HPperc, MidpointRounding.AwayFromZero);
		if (_prevAirAttackTextValue != num3)
		{
			_prevAirAttackTextValue = num3;
			airAttack.text = num3.ToString();
		}
		if (unit.IsMine || tileGO.isInFogOfWar || (unit.AirDamage == 0 && unit.SoftDamage == 0 && unit.HardDamage == 0 && unit.MaxMP == 0))
		{
			if (_counterValuesShown)
			{
				_counterValuesShown = false;
				range.fontSize = 0f;
				movementPoint.fontSize = 0f;
				softAttack.fontSize = 0f;
				hardAttack.fontSize = 0f;
				airAttack.fontSize = 0f;
			}
		}
		else if (!_counterValuesShown)
		{
			_counterValuesShown = true;
			range.fontSize = 16f;
			movementPoint.fontSize = 16f;
			softAttack.fontSize = 16f;
			hardAttack.fontSize = 16f;
			airAttack.fontSize = 16f;
			if (!_textObjectsShown)
			{
				_textObjectsShown = true;
				range.gameObject.SetActive(value: true);
				movementPoint.gameObject.SetActive(value: true);
				softAttack.gameObject.SetActive(value: true);
				hardAttack.gameObject.SetActive(value: true);
				airAttack.gameObject.SetActive(value: true);
			}
		}
	}

	private void UpdateMorale()
	{
		if (PlayerSettings.Instance.IsMorale && unit.MaxMP > 0)
		{
			if (tileGO.isInFogOfWar)
			{
				if (_moraleUIShown)
				{
					_moraleUIShown = false;
					morale_GO.enabled = false;
				}
				return;
			}
			if (!_moraleUIShown)
			{
				_moraleUIShown = true;
				morale_GO.enabled = true;
			}
			switch (unit.GetMoraleState())
			{
			case MoraleStates.EXCELLENT:
				morale_GO.color = new Color(0f, 85f / 106f, 0.004097662f);
				break;
			case MoraleStates.GOOD:
				morale_GO.color = new Color(0.05147396f, 0.5283019f, 0f);
				break;
			default:
				morale_GO.color = new Color(0.5283019f, 0.5283019f, 0.5283019f);
				break;
			case MoraleStates.BAD:
				morale_GO.color = new Color(87f / 106f, 87f / 106f, 0f);
				break;
			case MoraleStates.AWFUL:
				morale_GO.color = new Color(0.85f, 0f, 0f);
				break;
			}
		}
		else if (_moraleUIShown)
		{
			_moraleUIShown = false;
			morale_GO.enabled = false;
		}
	}

	public void ManageTwoUnitsIndicator()
	{
		if (isMoving || tileGO.isInFogOfWar)
		{
			if (_twoUnitsIndicatorShown)
			{
				_twoUnitsIndicatorShown = false;
				twoUnitsIndicatorGO.SetActive(value: false);
			}
		}
		else if (tileGO.airUnitGO != null && tileGO.groundUnitGO != null)
		{
			if (!tileGO.groundUnitGO.unit.IsMine || (!MapGO.IsAIOnly && TurnManager.humanPlayer.IsAlliedWith(tileGO.groundUnitGO.unit.OwnerPlayer)))
			{
				_twoUnitsIndicatorShown = true;
				Player player = ((TurnManager.humanPlayer != null) ? TurnManager.humanPlayer : TurnManager.currPlayer);
				if (tileGO.airUnitGO.gameObject.activeSelf)
				{
					if (player.Name == tileGO.groundUnitGO.unit.OwnerPlayer.Name)
					{
						twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Own ground");
					}
					else if (player.IsAtWarWith(tileGO.groundUnitGO.unit.OwnerPlayer))
					{
						twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Enemy ground");
					}
					else if (player.IsAlliedWith(tileGO.groundUnitGO.unit.OwnerPlayer))
					{
						twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Ally ground");
					}
					else
					{
						twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Neutral ground");
					}
				}
				else if (player.Name == tileGO.airUnitGO.unit.OwnerPlayer.Name)
				{
					twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Own air");
				}
				else if (player.IsAtWarWith(tileGO.airUnitGO.unit.OwnerPlayer))
				{
					twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Enemy air");
				}
				else if (player.IsAlliedWith(tileGO.airUnitGO.unit.OwnerPlayer))
				{
					twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Ally air");
				}
				else
				{
					twoUnitsIndicator_spriteRenderer.sprite = Resources.Load<Sprite>("UI/Two units indicator/Neutral air");
				}
				twoUnitsIndicatorGO.SetActive(value: true);
				twoUnitsIndicatorGO.transform.SetPositionAndRotation(new Vector3(gameObject.transform.position.x + 1.3f, gameObject.transform.position.y + 2.5f, -10f), Quaternion.Euler(Vector3.zero));
			}
			else if (_twoUnitsIndicatorShown)
			{
				_twoUnitsIndicatorShown = false;
				twoUnitsIndicatorGO.SetActive(value: false);
			}
		}
		else if (_twoUnitsIndicatorShown)
		{
			_twoUnitsIndicatorShown = false;
			twoUnitsIndicatorGO.SetActive(value: false);
		}
	}

	private void CheckPlaneFuel()
	{
		if (!PlayerSettings.Instance.RealisticPlanes && unit.CurrHP > 0 && !unit.IsDead && !(unit.Type != "Plane") && !MapGO.IsThereACertainTileAround(tileGO, "Airport") && unit.CurrAutonomy <= 0 && !unit.HasMoved && !isMoving)
		{
			unit.CurrMP = (byte)(unit.MaxMP / 3);
			unit.CurrAutonomy = (byte)(unit.MaxMP / 3);
			unit.CurrAmmo = 0;
		}
	}

	public void ManageUnitInfoContainer()
	{
		if (unit.IsDead)
		{
			return;
		}
		if (unit.OwnerPlayer.IsComputer || (!unit.OwnerPlayer.IsPlaying && !TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer)))
		{
			if (_isCanAttackUIShown)
			{
				_isCanAttackUIShown = false;
				canAttack_Sprite.enabled = false;
			}
			if (_isCanMoveUIShown)
			{
				_isCanMoveUIShown = false;
				canMove_Sprite.enabled = false;
			}
		}
		else
		{
			if (unit.CanMove())
			{
				canMove_Sprite.color = new Color(0.2f, 0.6f, 0.2f);
			}
			else
			{
				canMove_Sprite.color = new Color(0.188f, 0.188f, 0.188f);
			}
			bool flag = false;
			if (!unit.HasAttacked && !unit.IsTrain && unit.CurrAmmo > 0 && !unit.IsDropable)
			{
				flag = true;
			}
			if (unit.ActionPoints <= 0)
			{
				flag = false;
			}
			if (flag && TurnManager.isBadWeather && unit.Type == "Plane" && !unit.IsMissile && (unit.OwnerPlayer == null || !unit.OwnerPlayer.HasPolicy(Policies.Type.All_weather)))
			{
				flag = false;
			}
			if (flag)
			{
				canAttack_Sprite.color = new Color(0.72f, 0.14f, 0.2f);
			}
			else
			{
				canAttack_Sprite.color = new Color(0.188f, 0.188f, 0.188f);
			}
		}
		if (unit.OwnerPlayer.IsComputer || (!unit.OwnerPlayer.IsPlaying && !TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer)))
		{
			if (_isSubcontainerUIshown)
			{
				_isSubcontainerUIshown = false;
				subContainer_GO.SetActive(value: false);
			}
		}
		else if ((!MultiplayerManager.IsRealtimeMultiplayer() || TurnManager.currPlayer.IsLocalClientPlayer()) && !_isSubcontainerUIshown)
		{
			_isSubcontainerUIshown = true;
			subContainer_GO.SetActive(value: true);
		}
		if (!MapGO.IsAIOnly && unit.OwnerPlayer != null && !unit.OwnerPlayer.isAlliedWithHumanPlayer)
		{
			if (_isFuelUIshown)
			{
				_isFuelUIshown = false;
				fuel_Sprite.gameObject.SetActive(value: false);
			}
		}
		else if ((float)unit.CurrAutonomy <= (float)unit.MaxAutonomy * 0.1f && unit.MaxAutonomy > 0)
		{
			_isFuelUIshown = true;
			if (_fuelIconColor != 1)
			{
				fuel_Sprite.color = new Color(0.72f, 0.14f, 0.2f);
			}
			fuel_Sprite.gameObject.SetActive(value: true);
			_fuelIconColor = 1;
		}
		else if ((float)unit.CurrAutonomy <= (float)unit.MaxAutonomy * 0.3f && unit.MaxAutonomy > 0)
		{
			_isFuelUIshown = true;
			if (_fuelIconColor != 2)
			{
				fuel_Sprite.color = new Color(0.87f, 0.72f, 0.15f);
			}
			fuel_Sprite.gameObject.SetActive(value: true);
			_fuelIconColor = 2;
		}
		else if (_isFuelUIshown)
		{
			_isFuelUIshown = false;
			fuel_Sprite.gameObject.SetActive(value: false);
		}
		if (unit.IsCommander)
		{
			if (_isAmmoUIshown)
			{
				_isAmmoUIshown = false;
				ammo_Sprite.gameObject.SetActive(value: false);
			}
		}
		else if (!MapGO.IsAIOnly && unit.OwnerPlayer != null && unit.OwnerPlayer != TurnManager.humanPlayer)
		{
			if (_isAmmoUIshown)
			{
				_isAmmoUIshown = false;
				ammo_Sprite.gameObject.SetActive(value: false);
			}
		}
		else if (unit.CurrAmmo == 0 && unit.MaxAmmo > 0)
		{
			_isAmmoUIshown = true;
			if (_ammoIconColor != 1)
			{
				ammo_Sprite.color = new Color(0.72f, 0.14f, 0.2f);
			}
			ammo_Sprite.gameObject.SetActive(value: true);
			_ammoIconColor = 1;
		}
		else if (unit.MaxAmmo > 1 && ((float)unit.CurrAmmo <= (float)unit.MaxAmmo * 0.3f || (unit.CurrAmmo == 1 && unit.MaxAmmo <= 3)))
		{
			_isAmmoUIshown = true;
			if (_ammoIconColor != 2)
			{
				ammo_Sprite.color = new Color(0.87f, 0.72f, 0.15f);
			}
			ammo_Sprite.gameObject.SetActive(value: true);
			_ammoIconColor = 2;
		}
		else if (_isAmmoUIshown)
		{
			_isAmmoUIshown = false;
			ammo_Sprite.gameObject.SetActive(value: false);
		}
		if (!(Time.time - spawnTime < 0.3f))
		{
			unitInfoContainer.transform.SetPositionAndRotation(new Vector3(transform.position.x - 1f, transform.position.y - 2f, -2f), Quaternion.Euler(Vector3.zero));
		}
	}

	public void CalculateSupplyRoute(bool p_drawSupplyLine)
	{
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.CacheAllianceAndWarForGivenPlayer(unit.OwnerPlayer);
		}
		if (unit.Type == "Plane" || unit.IsTrain || !PlayerSettings.Instance.ComplexSupplies)
		{
			if (unit.Type == "Plane" || !PlayerSettings.Instance.ComplexSupplies)
			{
				unit.SupplyPercentage = 100f;
				bool flag = false;
				if (unit.IsCatapultPlane || unit.CanBeRefueledOnCarrier)
				{
					foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, 1))
					{
						if (item.enumType == TileTypes.SEA && item.GroundUnit != null && item.GroundUnit.OwnerPlayer != null && unit.OwnerPlayer.IsAlliedWith(item.GroundUnit.OwnerPlayer))
						{
							if (item.GroundUnit.IsCarrier)
							{
								flag = true;
							}
							else if (item.GroundUnit.CanCatapultPlanes && unit.IsCatapultPlane)
							{
								flag = true;
							}
						}
					}
				}
				if (unit.Type == "Plane")
				{
					if (flag || MapGO.IsThereACertainTileAround(tileGO, "Airport", p_compareToGivenPlayer: true) || MapGO.IsThereACertainTileAround(tileGO, "Reinforcement", p_compareToGivenPlayer: true))
					{
						unit.SupplyPercentage = 100f;
					}
					else
					{
						unit.SupplyPercentage = 0f;
					}
				}
				if (unit.SupplyPercentage > 0f)
				{
					unit.SupplyPercentage *= CalculateSupplyEncirclement();
				}
			}
			else
			{
				unit.SupplyPercentage = 0f;
			}
			unit.SupplyPercentage = Mathf.Clamp(unit.SupplyPercentage, 0f, 100f);
			if (unit.OwnerPlayer.HasPolicy(Policies.Type.Organized_supplyLines))
			{
				unit.SupplyPercentage += (int)Math.Round(unit.SupplyPercentage * 0.5f, MidpointRounding.ToEven);
			}
			unit.SupplyPercentage = Mathf.Clamp(unit.SupplyPercentage, 0f, 100f);
			return;
		}
		if (unit.Type == "Boat")
		{
			if (MapGO.IsThereACertainTileAround(tileGO, "Harbour", p_compareToGivenPlayer: true) || MapGO.IsThereACertainTileAround(tileGO, "Reinforcement", p_compareToGivenPlayer: true) || (unit.IsPatrolBoat && (MapGO.IsThereACertainTileAround(tileGO, "City", p_compareToGivenPlayer: true) || MapGO.IsThereACertainTileAround(tileGO, "Town", p_compareToGivenPlayer: true))))
			{
				unit.SupplyPercentage = 100f;
				unit.SupplyPercentage *= CalculateSupplyEncirclement();
				unit.SupplyPercentage = Mathf.Clamp(unit.SupplyPercentage, 0f, 100f);
				if (unit.OwnerPlayer.HasPolicy(Policies.Type.Organized_supplyLines))
				{
					unit.SupplyPercentage += (int)Math.Round(unit.SupplyPercentage * 0.5f, MidpointRounding.ToEven);
				}
				unit.SupplyPercentage = Mathf.Clamp(unit.SupplyPercentage, 0f, 100f);
			}
			else
			{
				unit.SupplyPercentage = 0f;
			}
			return;
		}
		Tile tile = tileGO.tile;
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		List<SupplySourceCandidate> list = new List<SupplySourceCandidate>();
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile2 = tilesTable[i, j];
				tile2.SetupForAStar();
				tile2.cost = 1073741823;
				tile2.supplyCost = 1073741823;
				if (tile2.IsValidSupplySource())
				{
					float num = tile2.GetSuppliesOutput() * 100f;
					if (!(num <= 0f))
					{
						list.Add(new SupplySourceCandidate(tile2, Utils.GetHeuristicDistance(tile, tile2), num));
					}
				}
			}
		}
		list.Sort((SupplySourceCandidate a, SupplySourceCandidate supplySourceCandidate2) =>
		{
			float distance = a.distance;
			return distance.CompareTo(supplySourceCandidate2.distance);
		});
		if (list.Count > 30)
		{
			list = list.Take(30).ToList();
		}
		if (list.Count == 0)
		{
			unit.SupplyPercentage = 0f;
			return;
		}
		int num2 = Mathf.Min(99, list.Max((SupplySourceCandidate p_source) => p_source.maxStoredCostExclusive));
		SortedDictionary<int, Queue<Tile>> sortedDictionary = new SortedDictionary<int, Queue<Tile>>();
		Tile tile3 = tileGO.tile;
		tile3.supplyCost = GetSupplyRouteTilePenalty(tile3);
		tile3.cost = tile3.supplyCost;
		if (tile3.supplyCost >= num2)
		{
			unit.SupplyPercentage = 0f;
			return;
		}
		Queue<Tile> queue = new Queue<Tile>();
		queue.Enqueue(tile3);
		sortedDictionary.Add(tile3.cost, queue);
		while (sortedDictionary.Count > 0)
		{
			KeyValuePair<int, Queue<Tile>> keyValuePair = sortedDictionary.First();
			int key = keyValuePair.Key;
			Tile tile4 = keyValuePair.Value.Dequeue();
			if (keyValuePair.Value.Count == 0)
			{
				sortedDictionary.Remove(key);
			}
			if (tile4.cost < key)
			{
				continue;
			}
			for (byte b = 0; b < tile4.amountOfNeighbours; b++)
			{
				Tile tile5 = tile4.neighbours[b];
				int supplyRouteTilePenalty = GetSupplyRouteTilePenalty(tile5);
				int num3 = tile4.supplyCost + supplyRouteTilePenalty;
				int num4 = tile4.cost + supplyRouteTilePenalty;
				if (supplyRouteTilePenalty < 100)
				{
					num4 += GetSupplyRouteDangerScore(tile5);
				}
				if (num3 < num2 && (num4 < tile5.cost || (num4 == tile5.cost && num3 < tile5.supplyCost)))
				{
					tile5.cost = num4;
					tile5.supplyCost = num3;
					tile5.parent = tile4;
					if (sortedDictionary.TryGetValue(num4, out var value))
					{
						value.Enqueue(tile5);
					}
					else
					{
						Queue<Tile> queue2 = new Queue<Tile>();
						queue2.Enqueue(tile5);
						sortedDictionary.Add(num4, queue2);
					}
				}
			}
		}
		int num5 = 0;
		int count = list.Count;
		List<SupplyRouteData> list2 = new List<SupplyRouteData>();
		for (short num6 = 0; num6 < count; num6++)
		{
			SupplySourceCandidate supplySourceCandidate = list[num6];
			Tile sourceTile = supplySourceCandidate.sourceTile;
			if (sourceTile.supplyCost < supplySourceCandidate.maxStoredCostExclusive)
			{
				num5 = 0;
				float num7 = Mathf.Clamp((float)sourceTile.supplyCost - (float)(int)sourceTile.MPCost, 0f, sourceTile.supplyCost);
				num7 += 100f - supplySourceCandidate.baseSupplyOutput;
				if (!(num7 >= 100f))
				{
					Tile tile6 = sourceTile;
					HashSet<Unit> hashSet = new HashSet<Unit>();
					int num8 = 0;
					byte b2 = 0;
					while (b2 < 100 && tile6.parent != null)
					{
						num5++;
						num8 += GetSupplyRouteDangerScore(tile6);
						for (byte b3 = 0; b3 < tile6.amountOfNeighbours; b3++)
						{
							Tile tile5 = tile6.neighbours[b3];
							if (tile5 != tileGO.tile && tile5.GroundUnit != null && !tile5.GroundUnit.IsCommander && !tile5.GroundUnit.IsTrain && !tile5.GroundUnit.IsOnBoat && tile5.GroundUnit.MaxMP > 0 && tile5.GroundUnit.OwnerPlayer.isAtWarWithGivenPlayer && !hashSet.Contains(tile5.GroundUnit))
							{
								hashSet.Add(tile5.GroundUnit);
							}
						}
						tile6 = tile6.parent;
						b2++;
					}
					num7 += (float)hashSet.Count * 10f;
					if (!(num7 >= 100f))
					{
						list2.Add(new SupplyRouteData(sourceTile, num7, num5, num8));
					}
				}
			}
		}
		if (list2.Count == 0)
		{
			unit.SupplyPercentage = 0f;
			return;
		}
		SupplyRouteData supplyRouteData = list2[0];
		float num9 = supplyRouteData.routeCost + (float)supplyRouteData.dangerScore * 2f + (float)supplyRouteData.pathLength * 0.5f;
		for (int num10 = 1; num10 < list2.Count; num10++)
		{
			SupplyRouteData supplyRouteData2 = list2[num10];
			float num11 = supplyRouteData2.routeCost + (float)supplyRouteData2.dangerScore * 2f + (float)supplyRouteData2.pathLength * 0.5f;
			if (num11 < num9)
			{
				num9 = num11;
				supplyRouteData = supplyRouteData2;
			}
		}
		Tile sourceTile2 = supplyRouteData.sourceTile;
		unit.SupplyPercentage = Mathf.Clamp(100f - supplyRouteData.routeCost, 0f, 100f);
		supplyLine = new List<TileGO>();
		for (Tile tile7 = sourceTile2; tile7 != null; tile7 = tile7.parent)
		{
			if (tile7.GroundUnit != null && !tile7.GroundUnit.OwnerPlayer.isAlliedWithGivenPlayer)
			{
				unit.SupplyPercentage = 0f;
			}
			supplyLine.Add(tile7.tileGO);
		}
		supplyLine.Reverse();
		if (unit.SupplyPercentage < 50f && unit.OwnerPlayer.IsComputer)
		{
			AI.instance.supplyDepotNeeded = true;
		}
		unit.SupplyPercentage *= CalculateSupplyEncirclement();
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.HasPolicy(Policies.Type.Organized_supplyLines))
		{
			unit.SupplyPercentage += (int)Math.Round(Mathf.Abs(unit.SupplyPercentage * 0.5f), MidpointRounding.ToEven);
		}
		if (TurnManager.isBadWeather)
		{
			if (GameData.Instance.map.IsWinter && GameData.Instance.map.AllowSnowInWinter)
			{
				unit.SupplyPercentage *= 0.7f;
			}
			else
			{
				unit.SupplyPercentage *= 0.8f;
			}
		}
		unit.SupplyPercentage = Mathf.Clamp(Mathf.Round(unit.SupplyPercentage), 0f, 100f);
		if (p_drawSupplyLine)
		{
			DrawSupplyLine();
		}
	}

	private int GetSupplyRouteTilePenalty(Tile p_tile)
	{
		if (p_tile.IsVictoryPoint && p_tile.ownerPlayer != null && !p_tile.ownerPlayer.isAlliedWithGivenPlayer)
		{
			return 100;
		}
		int num = p_tile.GetTileSuppliesPenalty(unit);
		if (p_tile.terrainOwner != null)
		{
			if (p_tile.terrainOwner.isAtWarWithGivenPlayer)
			{
				num = ((num != 0) ? (num * 2) : (num + p_tile.MPCost));
			}
			else if (!p_tile.terrainOwner.isAlliedWithGivenPlayer)
			{
				return 100;
			}
		}
		if (num >= 99)
		{
			return num;
		}
		if (p_tile.IsBridgeDestroyed || p_tile.enumType == TileTypes.MOUNTAIN || (p_tile.enumType == TileTypes.SEA && !p_tile.Bridge && !p_tile.isFrozen))
		{
			return 100;
		}
		if (p_tile.enumType == TileTypes.RIVER && !p_tile.isFrozen && (p_tile.tileGO.groundUnitGO == null || (p_tile.isVisible && !p_tile.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithGivenPlayer)))
		{
			return 100;
		}
		if (p_tile.isVisible && p_tile.tileGO.groundUnitGO != null && !p_tile.tileGO.groundUnitGO.unit.OwnerPlayer.isAlliedWithGivenPlayer)
		{
			return 100;
		}
		return num;
	}

	private int GetSupplyRouteDangerScore(Tile p_tile)
	{
		if (p_tile == null)
		{
			return 0;
		}
		int num = 0;
		if (p_tile.terrainOwner != null && p_tile.terrainOwner.isAtWarWithGivenPlayer)
		{
			num++;
			if (!p_tile.isVisible)
			{
				num += 2;
			}
		}
		for (byte b = 0; b < p_tile.amountOfNeighbours; b++)
		{
			Tile tile = p_tile.neighbours[b];
			if (tile.GroundUnit != null && !tile.GroundUnit.IsCommander && !tile.GroundUnit.IsTrain && !tile.GroundUnit.IsOnBoat && tile.GroundUnit.MaxMP != 0 && tile.GroundUnit.OwnerPlayer.isAtWarWithGivenPlayer)
			{
				num += 10;
			}
		}
		return num;
	}

	private float CalculateSupplyEncirclement()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (Tile neighbour in tileGO.tile.neighbours)
		{
			if (unit.Type == "Plane")
			{
				if (neighbour.tileGO.airUnitGO != null && unit.OwnerPlayer.IsAtWarWith(neighbour.tileGO.airUnitGO.unit.OwnerPlayer) && neighbour.tileGO.airUnitGO.unit.AirDamage > 0 && neighbour.tileGO.airUnitGO.unit.FilterType == "FilterFighter")
				{
					num++;
				}
				num2++;
				continue;
			}
			if (neighbour.tileGO.groundUnitGO != null && unit.OwnerPlayer.IsAtWarWith(neighbour.tileGO.groundUnitGO.unit.OwnerPlayer) && !neighbour.tileGO.groundUnitGO.unit.IsCommander && !neighbour.tileGO.groundUnitGO.unit.IsTrain && !neighbour.tileGO.groundUnitGO.unit.IsOnBoat && neighbour.tileGO.groundUnitGO.unit.MaxMP > 0)
			{
				num++;
			}
			if (unit.Type == "Boat")
			{
				if (neighbour.enumType == TileTypes.SEA)
				{
					num2++;
				}
			}
			else if (tileGO.tile.enumType == TileTypes.HARBOUR)
			{
				if (neighbour.enumType != TileTypes.RIVER && (!neighbour.Bridge || neighbour.IsBridgeDestroyed) && neighbour.enumType != TileTypes.MOUNTAIN)
				{
					num2++;
				}
			}
			else if (neighbour.enumType == TileTypes.SEA)
			{
				if (neighbour.isFrozen)
				{
					num2++;
				}
			}
			else if (neighbour.enumType == TileTypes.RIVER)
			{
				if (neighbour.isFrozen)
				{
					num2++;
				}
			}
			else if (neighbour.Bridge)
			{
				if (!neighbour.IsBridgeDestroyed)
				{
					num2++;
				}
			}
			else if (neighbour.enumType != TileTypes.MOUNTAIN)
			{
				num2++;
			}
		}
		if (num2 == 0f)
		{
			return 0f;
		}
		return Mathf.Clamp(1f - num / num2, 0f, 1f);
	}

	private void DrawSupplyLine()
	{
		if ((bool)UIManager.instance.supplyLine)
		{
			UnityEngine.Object.Destroy(UIManager.instance.supplyLine);
		}
		UIManager.instance.supplyLine = new GameObject();
		LineRenderer lineRenderer = UnityEngine.Object.Instantiate(Resources.Load<LineRenderer>("Line"), UIManager.instance.supplyLine.transform);
		lineRenderer.positionCount = supplyLine.Count;
		for (int i = 0; i < supplyLine.Count; i++)
		{
			Vector3 position = supplyLine[i].transform.position;
			position.z = -0.1f;
			lineRenderer.SetPosition(i, position);
		}
		if (unit.SupplyPercentage <= 0f)
		{
			Color endColor = (lineRenderer.startColor = new Color(0.6415094f, 0.1543254f, 0.1543254f));
			lineRenderer.endColor = endColor;
		}
		TutorialManager.PerformAction(TutorialActions.CHECK_SUPPLIES_STATUS);
	}

	public void DestroyUnit(bool p_playAnimation, bool p_sendRPC)
	{
		UIManager.instance.hoverUnitUI.Close();
		try
		{
			LeanTween.scale(gameObject, Vector3.zero, 0.75f);
			UnityEngine.Object.Destroy(gameObject, 0.75f);
		}
		catch
		{
			return;
		}
		if (unit.FilterType != "FilterBuilding" && !_convoyArrived)
		{
			unit.OwnerPlayer.TotalUnitsLost++;
			unit.OwnerPlayer.TurnUnitsLost++;
		}
		if (unit.IsKillGoal)
		{
			GameData.Instance.map.listOfUnitsThatNeedToBeKilled[unit] = true;
			MapGO.CheckIfEndOfScenario();
		}
		if (unit.IsPartisans && unit.OwnerPlayer != null)
		{
			unit.OwnerPlayer.NumberOfPartisansLost++;
		}
		if (tileGO.groundUnitGO != null && tileGO.airUnitGO != null)
		{
			tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			if (unit.Type == "Plane")
			{
				tileGO.groundUnitGO.gameObject.SetActive(value: true);
			}
			else
			{
				tileGO.airUnitGO.gameObject.SetActive(value: true);
			}
		}
		if (unit.Type == "Plane")
		{
			tileGO.airUnitGO = null;
			tileGO.tile.AirUnit = null;
		}
		else if (!unit.IsMine)
		{
			tileGO.groundUnitGO = null;
			tileGO.tile.GroundUnit = null;
		}
		if (!unit.IsDead)
		{
			if (p_sendRPC)
			{
				MultiplayerManager.Instance.RunRPC("RPC_KillUnit", "O", unit.ID, unit.OwnerName, p_playAnimation, _surrendered);
			}
			if (!unit.IsMine && !unit.IsArmourObstacle && !_convoyArrived && (!PhotonNetwork.InRoom || (PhotonNetwork.InRoom & p_sendRPC)) && unit.OwnerPlayer != null && !unit.OwnerPlayer.IsComputer && GameData.Instance.ironManMode)
			{
				SteamManager.instance.AddPlayerStat("Most units lost", 1);
			}
			if (p_playAnimation && !tileGO.isInFogOfWar && CameraGO.instance.IsWithinFOV(transform.position))
			{
				UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Explosion")).transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 5f);
			}
			unit.IsDead = true;
			CheckAchievements();
			unit.IsDead = false;
			if (unit.IsCoreUnit)
			{
				UIManager.instance.RefreshCoreUnits();
			}
			if (!unit.PermanentDeath && unit.MaxPurchaseTime > 0 && unit.OwnerPlayer.PurchasedUnits.ContainsKey(unit.Name) && unit.OwnerPlayer.PurchasedUnits[unit.Name] > 0)
			{
				unit.OwnerPlayer.PurchasedUnits[unit.Name]--;
			}
			unit.OwnerPlayer.RemoveUnit(unit, p_sendRPC: false);
			int manpowerCost = unit.manpowerCost;
			manpowerCost = ((!_surrendered && !_captured && (!PlayerSettings.Instance.RealisticPlanes || !(unit.Type == "Plane") || unit.CurrAutonomy != 0)) ? ((int)((float)manpowerCost * 0.3f)) : 0);
			unit.OwnerPlayer.AddManpower(manpowerCost, p_doMarginCheck: false, p_sendRPC: false);
			if (MapGO.selectedUnitGO == this && !unit.OwnerPlayer.IsComputer)
			{
				RemoveFogOfWarAroundUnit();
				MapGO.instance.UnselectUnit();
			}
			if (_surrendered)
			{
				new Notification(NotificationTypes.UNIT_SURRENDERED, unit.OwnerPlayer.Name, tileGO.transform.position.x, tileGO.transform.position.y, unit.GetUnitName(), unit.IsCoreUnit, unit.ID);
				if (_captured && !unit.NoCapture)
				{
					unit.IsReserve = true;
					unit.XP = 0;
					unit.CurrHP = 30;
					unit.Hero = null;
					unit.IsDead = false;
					unit.HasAttacked = false;
					unit.ActionPoints = 0;
					unit.IsCoreUnit = false;
					TurnManager.currPlayer.AddUnit(unit, p_sendRPC);
					if (GameData.Instance.map.IsCampaign && !TurnManager.currPlayer.IsComputer && TurnManager.currPlayer.CountCurrCoreUnits() < Utils.GetMaxCoreUnitsNumber(TurnManager.currPlayer))
					{
						unit.IsCoreUnit = true;
						UIManager.instance.RefreshCoreUnits();
					}
					ReservePanelGO.instance.RefreshDeployNotification();
				}
			}
			else if (!_convoyArrived && !_missileArrived)
			{
				NotificationTypes p_notificationType = NotificationTypes.UNIT_KILLED;
				if (unit.Type == "Plane")
				{
					p_notificationType = NotificationTypes.UNIT_SHOT_DOWN;
				}
				else if (unit.Type == "Boat")
				{
					p_notificationType = ((!unit.IsMine) ? NotificationTypes.UNIT_SUNK : NotificationTypes.UNIT_BLEW_UP);
				}
				else if (unit.FilterType == "FilterBuilding")
				{
					p_notificationType = NotificationTypes.UNIT_DESTROYED;
				}
				new Notification(p_notificationType, unit.OwnerPlayer.Name, tileGO.transform.position.x, tileGO.transform.position.y, unit.GetUnitName(), unit.IsCoreUnit, unit.ID);
			}
			CheckEventTrigger();
		}
		if (!_captured)
		{
			unit.IsDead = true;
		}
	}

	private void CheckEventTrigger()
	{
		if (MultiplayerManager.IsRealtimeMultiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (@event.CheckType == CheckTypes.UNIT_DESTROYED && @event.CheckUnitID == unit.ID)
			{
				TurnManager.instance.ProcessSingleEvent(@event);
			}
		}
	}

	public void MoveUnitToTile(TileGO p_targetTileGO)
	{
		if (Time.timeScale != 0f && !(p_targetTileGO == tileGO) && listOfpotentialTiles.Contains(p_targetTileGO) && !MapGO.listOfTargetedUnit.Contains(p_targetTileGO.gameObject))
		{
			UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
			gameObject.SetActive(value: true);
			GetPathToTile(p_targetTileGO.tile);
			StartCoroutine(CR_MoveUnitToAllTilesOfItsPath(p_targetTileGO));
			TutorialManager.PerformAction(TutorialActions.MOVE_UNIT);
		}
	}

	private IEnumerator CR_MoveUnitToAllTilesOfItsPath(TileGO p_targetTileGO)
	{
		StopAttackMotionAndSnapToTile();
		if (_swapLerpCoroutine != null)
		{
			StopCoroutine(_swapLerpCoroutine);
			_swapLerpCoroutine = null;
			transform.position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, transform.position.z);
		}
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar || !p_targetTileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 0.5f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 0.5f;
			}
			else if (!PlayerSettings.Instance.IsQuickMovement && (!PlayerSettings.Instance.FastAI || !unit.OwnerPlayer.IsComputer))
			{
				aIWaitInstruction.waitTime = 0.5f * (float)currentPath.Count;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		MapGO.ClearTargetedUnits();
		listOfpotentialTiles.Clear();
		TileGO lastTileGO = tileGO;
		_ = tileGO;
		if (unit.Type == "Plane")
		{
			tileGO.airUnitGO = null;
			tileGO.tile.AirUnit = null;
		}
		else
		{
			tileGO.groundUnitGO = null;
			tileGO.tile.GroundUnit = null;
		}
		SetUnitMovementSpeed();
		if (!unit.hasBeenAttackedThisTurn)
		{
			canUndo = true;
		}
		prevTile = tileGO;
		prevMP = unit.CurrMP;
		prevHP = unit.CurrHP;
		backupHasAttacked = unit.HasAttacked;
		backupHowManyTimesUnitMoved = unit.NumberOfTimesTheUnitMoved;
		backupHowManyTimesTilesTheUnitMoved = unit.NumberOfTilesMovedDuringCurrentTurn;
		backupEntrenchmentLevel = unit.EntrenchmentLevel;
		prevAutonomy = unit.CurrAutonomy;
		listOfNewlyDiscoveredTiles = new List<TileGO>();
		unit.IsDefensive = false;
		unit.AsleepState = AsleepStates.NONE;
		bool hasPlayedMovementSound = false;
		bool isWeakToObstacles = unit.IsMotorized || unit.IsMechanized || unit.Type == "Tank";
		bool canReachTargetTile = true;
		bool becauseOfEnemyZoC = false;
		bool stoppedByEnemyMine = false;
		TileGO lastSafeReachableTileGO = tileGO;
		TileGO problematicTile = null;
		foreach (TileGO item in currentPath)
		{
			if (unit.Type == "Plane")
			{
				if (item.airUnitGO != null && !unit.OwnerPlayer.IsAlliedWith(item.airUnitGO.unit.OwnerPlayer))
				{
					canReachTargetTile = false;
					problematicTile = item;
				}
			}
			else
			{
				if (item.groundUnitGO != null && !item.groundUnitGO.unit.IsMine && !unit.OwnerPlayer.IsAlliedWith(item.groundUnitGO.unit.OwnerPlayer))
				{
					canReachTargetTile = false;
					problematicTile = item;
				}
				if (canReachTargetTile && PlayerSettings.Instance.ZoneOfControl && !overrunning)
				{
					foreach (Tile neighbour in item.tile.neighbours)
					{
						if (neighbour.GroundUnit != null && neighbour.GroundUnit.CanApplyZoneOfControl(unit))
						{
							becauseOfEnemyZoC = true;
							canReachTargetTile = false;
							problematicTile = neighbour.tileGO;
							if (item.groundUnitGO == null || (item.groundUnitGO.unit.IsMine && unit.OwnerPlayer.IsAtWarWith(item.groundUnitGO.unit.OwnerPlayer)))
							{
								lastSafeReachableTileGO = item;
							}
							break;
						}
					}
				}
			}
			if (!canReachTargetTile)
			{
				break;
			}
			if (unit.Type == "Plane")
			{
				if (item.airUnitGO == null)
				{
					lastSafeReachableTileGO = item;
				}
			}
			else if (item.groundUnitGO == null)
			{
				lastSafeReachableTileGO = item;
			}
		}
		List<Tile.Coordinates> networkPath = new List<Tile.Coordinates> { tileGO.tile.GetCoord() };
		_moveStepIndex = 0;
		_moveTotalSteps = currentPath.Count;
		foreach (TileGO pathTileGO in currentPath)
		{
			if (!canReachTargetTile && lastSafeReachableTileGO == tileGO)
			{
				CheckForAmbush(pathTileGO, p_targetTileGO, isWeakToObstacles);
				break;
			}
			RemoveFogOfWarAroundUnit();
			if (CheckForAmbush(pathTileGO, p_targetTileGO, isWeakToObstacles) || (TurnManager.currPlayer.IsComputer && ((pathTileGO.tile.enumType == TileTypes.SEA && !pathTileGO.tile.Bridge && !pathTileGO.tile.isFrozen && !unit.IsOnBoat && unit.Type != "Plane" && unit.FilterType != "FilterBoat") || (unit.IsOnBoat && pathTileGO.tile.enumType != TileTypes.SEA && unit.CurrMP != unit.MaxMP) || (pathTileGO.tile.enumType == TileTypes.RIVER && !pathTileGO.tile.isFrozen && !pathTileGO.tile.Bridge && !unit.CanMoveOnRivers() && (pathTileGO.tile.GroundUnit == null || !unit.OwnerPlayer.IsAlliedWith(pathTileGO.tile.GroundUnit.OwnerPlayer))) || (pathTileGO.tile.IsBridgeDestroyed && !pathTileGO.tile.isFrozen && !unit.IsEngineers && !unit.IsAmphibious && unit.FilterType != "FilterBoat" && unit.Type != "Plane"))))
			{
				break;
			}
			if (!hasPlayedMovementSound)
			{
				if (!TurnManager.currPlayer.IsComputer || !tileGO.isInFogOfWar || !pathTileGO.isInFogOfWar)
				{
					SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(unit.GetMovementSound()));
				}
				hasPlayedMovementSound = true;
			}
			if (!unit.IsWinterSpecialized && !unit.ImmuneToWinterDamage && PlayerSettings.Instance.WinterDamageValue > 0 && pathTileGO.tile.Season == "Winter" && !unit.OwnerPlayer.HasPolicy(Policies.Type.Winter_trained) && unit.Type != "Boat" && unit.Type != "Plane" && !unit.IsOnBoat && !unit.IsTrain && !unit.IsDropable)
			{
				if (!tileGO.isInFogOfWar)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
					gameObject.transform.SetParent(tileGO.transform);
					gameObject.GetComponent<TextMeshPro>().text = (-pathTileGO.tile.MPCost * PlayerSettings.Instance.WinterDamageValue).ToString();
					Vector3 position = new Vector3(pathTileGO.transform.position.x, pathTileGO.transform.position.y, -20f);
					gameObject.transform.position = position;
				}
				unit.CurrHP -= pathTileGO.tile.MPCost * PlayerSettings.Instance.WinterDamageValue;
				if (unit.CurrHP <= 0)
				{
					isMoving = false;
					unit.CurrHP = 0;
					DestroyUnit(p_playAnimation: false, p_sendRPC: true);
					yield break;
				}
			}
			if (!unit.OwnerPlayer.IsComputer && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
			{
				MapGO.instance.selectedUnitIcon.SetActive(value: false);
			}
			ShowSelectedUnitStats();
			RotateUnit(tileGO, pathTileGO);
			lastTileGO = pathTileGO;
			Vector3 position2 = transform.position;
			Vector3 vector = new Vector3(pathTileGO.transform.position.x, pathTileGO.transform.position.y, -1f);
			float num = Vector2.Distance(position2, vector) / (_speed * GetMoveSpeedMultiplier());
			isMoving = true;
			unit.NumberOfTilesMovedDuringCurrentTurn++;
			tileGO = pathTileGO;
			networkPath.Add(tileGO.tile.GetCoord());
			if (_moveAnimCoroutine != null)
			{
				StopCoroutine(_moveAnimCoroutine);
			}
			_moveAnimCoroutine = StartCoroutine(DoMoveAnimation(position2, vector, num));
			if (TurnManager.humanPlayer != null && (MapGO.IsAIOnly || unit.OwnerName == TurnManager.humanPlayer.Name || TurnManager.humanPlayer.IsAlliedWith(unit.OwnerPlayer)))
			{
				tileGO.RemoveFogOfWar(p_smooth: true);
			}
			if (unit.IsOnBoat && tileGO.tile.enumType != TileTypes.SEA)
			{
				EmbarkDisembarkBoat(p_sendRPC: true);
			}
			TryCaptureTile(tileGO.tile, p_sendRPC: true);
			unit.TotalDistance++;
			byte b = 0;
			short num2 = 0;
			if (unit.Type == "Plane")
			{
				b = 1;
				num2 = 1;
			}
			else if (pathTileGO.tile.enumType != TileTypes.RIVER || pathTileGO.tile.GroundUnit == null || !pathTileGO.tile.GroundUnit.IsPontoonBridge)
			{
				if (!unit.IsTrain && pathTileGO.tile.isRoadOrPlain && (unit.Type == "Tank" || unit.IsMechanized))
				{
					b = (byte)Mathf.Clamp(pathTileGO.tile.MPCost / 2, 1, pathTileGO.tile.MPCost);
					num2 = (byte)Mathf.Clamp(pathTileGO.tile.MPCost / 2, 1, pathTileGO.tile.MPCost);
				}
				else
				{
					b = pathTileGO.tile.MPCost;
					num2 = pathTileGO.tile.MPCost;
				}
			}
			if (PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
			{
				num2 = 0;
			}
			unit.CurrMP -= b;
			unit.CurrAutonomy -= num2;
			if (unit.CurrMP < 0 || unit.CurrMP > unit.MaxMP)
			{
				unit.CurrMP = 0;
			}
			if (unit.CurrAutonomy < 0 || unit.CurrAutonomy > unit.MaxAutonomy)
			{
				unit.CurrAutonomy = 0;
			}
			if (!PlayerSettings.Instance.IsQuickMovement)
			{
				if (isRetreating)
				{
					if (unit.OwnerPlayer == TurnManager.humanPlayer && !tileGO.isInFogOfWar)
					{
						yield return new WaitForSeconds(num);
					}
					else if (TurnManager.currPlayer.IsComputer)
					{
						yield return new WaitForEndOfFrame();
					}
					else
					{
						yield return new WaitForSeconds(num);
					}
				}
				else if (!unit.OwnerPlayer.IsComputer)
				{
					yield return new WaitForSeconds(num);
				}
				else if (!PlayerSettings.Instance.FastAI && !PlayerSettings.Instance.FastAllies)
				{
					yield return new WaitForSeconds(num);
				}
				else if (PlayerSettings.Instance.FastAllies && !PhotonNetwork.InRoom && TurnManager.humanPlayer != null && TurnManager.humanPlayer.Faction != "Neutral" && unit.OwnerPlayer.Faction != TurnManager.humanPlayer.Faction)
				{
					yield return new WaitForSeconds(num);
				}
			}
			base.gameObject.transform.position = new Vector3(pathTileGO.transform.position.x, pathTileGO.transform.position.y, base.gameObject.transform.position.z);
			_moveStepIndex++;
			if (unit.Type != "Plane" && pathTileGO.groundUnitGO != null && pathTileGO.groundUnitGO.unit.IsMine && !unit.OwnerPlayer.IsAlliedWith(pathTileGO.groundUnitGO.unit.OwnerPlayer))
			{
				if (!unit.IsOnBoat && (unit.IsMineFlail || unit.IsEngineers || unit.IsDestroyer))
				{
					if (!unit.OwnerPlayer.IsComputer)
					{
						SteamManager.UnlockAchievement("Mine flail");
					}
					if (!tileGO.isInFogOfWar)
					{
						SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Unfield unit"));
					}
					GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"), tileGO.transform);
					gameObject2.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-disarmed mine");
					gameObject2.GetComponent<TextMeshPro>().color = new Color(0.2f, 0.6f, 0.2f);
					gameObject2.transform.position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, -20f);
				}
				else
				{
					string attackSound = pathTileGO.groundUnitGO.unit.AttackSound;
					if (!tileGO.isInFogOfWar)
					{
						SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(attackSound));
					}
					MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, attackSound, "Attack");
					if (!unit.OwnerPlayer.IsComputer)
					{
						SteamManager.UnlockAchievement("Watch your step");
					}
					if (unit.Type == "Tank" || unit.IsTrain || unit.Type == "Boat" || unit.FilterType == "FilterBuilding")
					{
						int num3 = GetDeterministicMineDamage(pathTileGO.groundUnitGO.unit.HardDamage, pathTileGO);
						if (pathTileGO.groundUnitGO.unit.OwnerPlayer != null && pathTileGO.groundUnitGO.unit.OwnerPlayer.HasPolicy(Policies.Type.Magnetic_mines))
						{
							num3 += (int)Math.Round((float)num3 * 0.15f, MidpointRounding.ToEven);
						}
						unit.CurrHP -= num3;
						pathTileGO.groundUnitGO.unit.FinalDamage = num3;
					}
					else if (unit.Type == "Infantry" || unit.Type == "AA" || unit.Type == "Artillery" || unit.IsEngineers)
					{
						int deterministicMineDamage = GetDeterministicMineDamage(pathTileGO.groundUnitGO.unit.SoftDamage, pathTileGO);
						unit.CurrHP -= deterministicMineDamage;
						pathTileGO.groundUnitGO.unit.FinalDamage = deterministicMineDamage;
					}
					DisplayDamage(pathTileGO.groundUnitGO, this);
				}
				UpdateCounter();
				RemoveFogOfWarAroundUnit();
				pathTileGO.groundUnitGO.DestroyUnit(p_playAnimation: true, p_sendRPC: true);
				AssignUnitToTile(pathTileGO);
				if (unit.CurrHP <= 0)
				{
					unit.CurrHP = 0;
					isMoving = false;
					unit.HasMoved = true;
					canUndo = false;
					MultiplayerManager.RPC("RPC_MoveUnit", unit, networkPath);
					DestroyUnit(p_playAnimation: true, p_sendRPC: true);
					yield break;
				}
				stoppedByEnemyMine = true;
				canUndo = false;
				break;
			}
			if (PlayerSettings.Instance.RealisticPlanes && unit.Type == "Plane" && unit.CurrMP <= 0 && unit.CurrAutonomy <= 0 && !MapGO.IsThereACertainTileAround(pathTileGO, "Airport"))
			{
				AssignUnitToTile(pathTileGO);
				isMoving = false;
				unit.HasMoved = true;
				canUndo = false;
				unit.CurrHP = 0;
				MultiplayerManager.RPC("RPC_MoveUnit", unit, networkPath);
				DestroyUnit(p_playAnimation: true, p_sendRPC: true);
				yield break;
			}
			if ((pathTileGO.tile.IsBridgeDestroyed && unit.IsEngineers && unit.OwnerPlayer.IsComputer && (pathTileGO.groundUnitGO == null || pathTileGO.groundUnitGO == base.gameObject)) || unit.CurrMP <= 0)
			{
				break;
			}
		}
		AIWaitInstruction aIWaitInstruction2 = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction2.waitTime = 0.75f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction2.waitTime = 0.5f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction2);
		if (!canReachTargetTile && !stoppedByEnemyMine)
		{
			if (!problematicTile.tile.isVisible)
			{
				canUndo = false;
			}
			problematicTile.tile.isVisible = true;
			bool flag = false;
			if (!PlayerSettings.Instance.IsFogOfWar)
			{
				flag = true;
			}
			else if (MapGO.IsAIOnly)
			{
				flag = true;
			}
			else if (TurnManager.humanPlayer.IsAlliedWith(unit.OwnerPlayer))
			{
				flag = true;
			}
			if (flag)
			{
				if (!unit.OwnerPlayer.IsComputer && !becauseOfEnemyZoC)
				{
					UIManager.ShowMessage(LocalizationManager.Translate("info-obstructed path"));
					StartCoroutine(UIManager.instance.HighlighTile(problematicTile.transform.position.x, problematicTile.transform.position.y));
				}
				problematicTile.RemoveFogOfWar(p_smooth: true);
				if (problematicTile.airUnitGO != null)
				{
					problematicTile.airUnitGO.UpdateCounter();
				}
				if (problematicTile.groundUnitGO != null)
				{
					problematicTile.groundUnitGO.UpdateCounter();
				}
			}
		}
		AssignUnitToTile(lastTileGO);
		if (!isRetreating && !overrunning)
		{
			unit.HasMoved = true;
			unit.AddRemoveActionPoints(-1);
		}
		unit.NumberOfTimesTheUnitMoved++;
		unit.EntrenchmentLevel = 0;
		currentPath.Clear();
		listOfpotentialTiles.Clear();
		if (MapGO.selectedUnitGO == base.gameObject)
		{
			UIManager.instance.selectedUnitUI.Show(this);
		}
		if (!unit.IsOnBoat && unit.MoveOrAttack)
		{
			unit.HasAttacked = true;
		}
		RemoveFogOfWarAroundUnit();
		isMoving = false;
		if (!unit.OwnerPlayer.IsComputer)
		{
			if (!isRetreating)
			{
				SelectUnit(p_playAnimation: false);
			}
			CheckAchievements();
		}
		isRetreating = false;
		overrunning = false;
		MultiplayerManager.RPC("RPC_MoveUnit", unit, networkPath);
		if (!unit.IsConvoy)
		{
			yield break;
		}
		foreach (Tile neighbour2 in tileGO.tile.neighbours)
		{
			if (neighbour2.IsVictoryPoint && neighbour2.OwnerName != unit.OwnerName && unit.OwnerPlayer.IsAlliedWith(neighbour2.ownerPlayer))
			{
				float num4 = unit.GetHPPercent(p_forDamagePurposes: false);
				if (num4 > 1f)
				{
					num4 = 1f;
				}
				int num5 = (int)Math.Round((float)unit.CustomCost * num4, MidpointRounding.AwayFromZero);
				neighbour2.ownerPlayer.AddMoney(num5, p_sendRPC: true);
				new Notification(NotificationTypes.MONEY_TRANSFER, neighbour2.ownerPlayer.Name, neighbour2.tileGO.transform.position.x, neighbour2.tileGO.transform.position.y, string.Empty, p_isImportant: true)
				{
					Amount = num5,
					SenderName = unit.OwnerPlayer.Name
				};
				GameObject gameObject3 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"), tileGO.transform);
				gameObject3.GetComponent<TextMeshPro>().text = "+" + num5;
				gameObject3.GetComponent<TextMeshPro>().color = new Color(0.2f, 0.6f, 0.2f);
				gameObject3.transform.position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, -20f);
				_convoyArrived = true;
				unit.CurrHP = 0;
				DestroyUnit(p_playAnimation: false, p_sendRPC: true);
				if (!unit.OwnerPlayer.IsComputer)
				{
					SteamManager.UnlockAchievement("War effort");
				}
				break;
			}
		}
	}

	public bool TryCaptureTile(Tile p_tile, bool p_sendRPC)
	{
		if (!p_tile.Capturable)
		{
			return false;
		}
		if (!p_tile.IsVictoryPoint)
		{
			return false;
		}
		if (unit.IsTrain || unit.IsOnBoat)
		{
			return false;
		}
		if (unit.FilterType == "FilterBuilding")
		{
			return false;
		}
		if (unit.Type == "Plane")
		{
			return false;
		}
		if ((unit.IsSupplyShip && unit.HardDamage == 0) || (unit.IsConvoy && unit.HardDamage == 0))
		{
			return false;
		}
		if (!unit.OwnerPlayer.IsAtWarWith(p_tile.ownerPlayer))
		{
			return false;
		}
		new Notification(NotificationTypes.VP_LOST, p_tile.ownerPlayer.Name, transform.position.x, transform.position.y, p_tile.PosX + ":" + p_tile.PosY, p_isImportant: true);
		canUndo = false;
		unit.OwnerPlayer.NumberOfVPEverConquered++;
		unit.AddXP(3, p_canAddHero: true, p_showXPUI: true);
		if (p_tile.PrimaryObjective)
		{
			unit.OwnerPlayer.AddHQPoints(3, p_sendRPC: false);
		}
		else if (p_tile.SecondaryObjective)
		{
			unit.OwnerPlayer.AddHQPoints(2, p_sendRPC: false);
		}
		else
		{
			unit.OwnerPlayer.AddHQPoints(1, p_sendRPC: false);
		}
		string empty = string.Empty;
		if (string.IsNullOrEmpty(p_tile.CityName))
		{
			empty = ((!p_tile.isNavalConvoyRoute) ? (LocalizationManager.Translate("history-captured vp") + p_tile.Type) : (LocalizationManager.Translate("history-captured vp") + LocalizationManager.Translate("Naval Convoy Route")));
		}
		else
		{
			empty = LocalizationManager.Translate("history-captured vp") + p_tile.CityName;
		}
		if (!string.IsNullOrEmpty(empty))
		{
			unit.AddToHistory(empty, p_sendRPC: false);
		}
		if (p_tile.enumType != TileTypes.SEA && p_tile.ownerPlayer.HasPolicy(Policies.Type.Scorched_earth))
		{
			p_tile.NumberOfTurnsAtLowProd = 3;
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
			UnityEngine.Object.Destroy(gameObject.GetComponent<DamageTextGO>());
			gameObject.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-scorched earth");
			Vector3 position = new Vector3(p_tile.tileGO.transform.position.x, p_tile.tileGO.transform.position.y + 5f, -20f);
			gameObject.transform.position = position;
			gameObject.transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, Vector3.one, 0.5f).setEaseOutCubic();
			UnityEngine.Object.Destroy(gameObject, 2f);
			p_tile.tileGO.SetSmoke();
		}
		else if (p_tile.NumberOfTurnsSinceVPGotCaptured - p_tile.GetMinimumOccupancyForWork() >= 0)
		{
			unit.OwnerPlayer.AddMoney(p_tile.MoneyReward, p_sendRPC: false);
			if (!p_tile.tileGO.isInFogOfWar)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
				gameObject2.GetComponent<TextMeshPro>().text = "+" + p_tile.MoneyReward;
				gameObject2.GetComponent<TextMeshPro>().color = new Color(0.2f, 0.6f, 0.2f);
				gameObject2.transform.position = new Vector3(p_tile.tileGO.transform.position.x, p_tile.tileGO.transform.position.y + 3f, -20f);
				gameObject2.transform.localScale = Vector3.zero;
				LeanTween.scale(gameObject2, Vector3.one, 0.5f).setEaseOutCubic();
				UnityEngine.Object.Destroy(gameObject2, 2f);
			}
		}
		p_tile.NumberOfTurnsSinceVPGotCaptured = 0;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, 5))
		{
			if (item.AirUnit != null && item.AirUnit.OwnerPlayer != null)
			{
				if (item.AirUnit.OwnerPlayer.Name == unit.OwnerPlayer.Name)
				{
					item.AirUnit.AddMorale(5);
				}
				else if (item.AirUnit.OwnerPlayer.Name == p_tile.ownerPlayer.Name)
				{
					item.AirUnit.AddMorale(-5);
				}
				else if (item.AirUnit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
				{
					item.AirUnit.AddMorale(2);
				}
				else if (item.AirUnit.OwnerPlayer.IsAlliedWith(p_tile.ownerPlayer))
				{
					item.AirUnit.AddMorale(-2);
				}
				if ((bool)item.AirUnit.unitGO)
				{
					item.AirUnit.unitGO.UpdateCounter();
				}
			}
			if (item.GroundUnit != null && item.GroundUnit.OwnerPlayer != null)
			{
				if (item.GroundUnit.OwnerPlayer.Name == unit.OwnerPlayer.Name)
				{
					item.GroundUnit.AddMorale(5);
				}
				else if (item.GroundUnit.OwnerPlayer.Name == p_tile.ownerPlayer.Name)
				{
					item.GroundUnit.AddMorale(-5);
				}
				else if (item.GroundUnit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
				{
					item.GroundUnit.AddMorale(2);
				}
				else if (item.GroundUnit.OwnerPlayer.IsAlliedWith(p_tile.ownerPlayer))
				{
					item.GroundUnit.AddMorale(-2);
				}
				if ((bool)item.GroundUnit.unitGO)
				{
					item.GroundUnit.unitGO.UpdateCounter();
				}
			}
		}
		GameData.Instance.map.EventTriggerVPLoss(p_tile, unit.OwnerPlayer, p_tile.ownerPlayer);
		if (!string.IsNullOrEmpty(p_tile.OriginalCountryOwner))
		{
			if (GameData.Instance.TryFindPlayerByCountry(p_tile.OriginalCountryOwner, out var o_player) && o_player.IsAlliedWith(unit.OwnerPlayer))
			{
				p_tile.ownerPlayer = o_player;
			}
			else
			{
				p_tile.ownerPlayer = unit.OwnerPlayer;
			}
		}
		else
		{
			p_tile.ownerPlayer = unit.OwnerPlayer;
		}
		p_tile.terrainOwner = p_tile.ownerPlayer;
		p_tile.OwnerName = p_tile.ownerPlayer.Name;
		p_tile.tileGO.SetVPFlag();
		if (p_tile.PrimaryObjective)
		{
			MapGO.RecountCapturedMajorVPs();
		}
		UIManager.instance.UpdateVPtext();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_CaptureVP", "O", unit, p_tile.PosX, p_tile.PosY);
		}
		if (p_tile.AirUnit != null && p_tile.AirUnit.MaxMP == 0 && p_tile.AirUnit.GetNameToLower().Contains("balloon") && !unit.OwnerPlayer.IsAlliedWith(p_tile.AirUnit.OwnerPlayer))
		{
			p_tile.AirUnit.unitGO.DestroyUnit(p_playAnimation: false, p_sendRPC: true);
		}
		MapGO.ProcessFogOfWar(p_refreshTerrainOwnershipOnly: true);
		MapGO.instance.DrawBordersOnMap();
		MapGO.CheckIfEndOfScenario();
		if (TurnManager.currPlayer.IsComputer)
		{
			AI.instance.needToRefreshEnemyVPs = true;
		}
		else if (GameData.Instance.ironManMode)
		{
			if (TurnManager.currPlayer.NumberOfVPEverConquered >= 100)
			{
				SteamManager.UnlockAchievement("Conqueror_Bronze");
			}
			if (TurnManager.currPlayer.NumberOfVPEverConquered >= 250)
			{
				SteamManager.UnlockAchievement("Conqueror_Silver");
			}
			if (TurnManager.currPlayer.NumberOfVPEverConquered >= 500)
			{
				SteamManager.UnlockAchievement("Conqueror_Gold");
			}
		}
		return true;
	}

	private void AssignUnitToTile(TileGO p_tileGO)
	{
		if (unit.Type == "Plane")
		{
			p_tileGO.tile.AirUnit = unit;
			p_tileGO.airUnitGO = this;
		}
		else
		{
			p_tileGO.tile.GroundUnit = unit;
			p_tileGO.groundUnitGO = this;
		}
		if (unit.Waypoints != null)
		{
			for (int i = 0; i < unit.Waypoints.Count; i++)
			{
				if (unit.Waypoints[i][0] == p_tileGO.tile.PosX && unit.Waypoints[i][1] == p_tileGO.tile.PosY)
				{
					if (unit.Waypoints.Count == 1)
					{
						new Notification(NotificationTypes.UNIT_REACHED_DESTINATION, unit.OwnerPlayer.Name, tileGO.transform.position.x, tileGO.transform.position.y, unit.GetUnitName(), p_isImportant: true);
					}
					unit.Waypoints.RemoveAt(i);
					return;
				}
			}
		}
		if (isRetreating && !TurnManager.currPlayer.IsComputer && MapGO.selectedUnitGO != null && !MapGO.selectedUnitGO.isMoving)
		{
			MapGO.selectedUnitGO.ShowPotentialPaths();
		}
	}

	private IEnumerator DoMoveAnimation(Vector3 p_startPos, Vector3 p_endPos, float p_moveAnimDuration)
	{
		float startTime = Time.time;
		while (isMoving)
		{
			float p_t = (Time.time - startTime) / p_moveAnimDuration;
			transform.position = Vector3.Lerp(p_startPos, p_endPos, ApplyMovementEasing(p_t));
			yield return null;
		}
	}

	private float GetMoveSpeedMultiplier()
	{
		if (PlayerSettings.Instance.IsMovementInertia)
		{
			if (_moveTotalSteps <= 1)
			{
				return 0.55f;
			}
			if (_moveStepIndex == 0)
			{
				return 0.75f;
			}
			if (_moveStepIndex >= _moveTotalSteps - 1)
			{
				return 0.75f;
			}
		}
		return 1f;
	}

	private float ApplyMovementEasing(float p_t)
	{
		if (!PlayerSettings.Instance.IsMovementInertia)
		{
			return p_t;
		}
		p_t = Mathf.Clamp01(p_t);
		if (_moveTotalSteps <= 1)
		{
			return p_t * p_t * p_t * (p_t * (p_t * 6f - 15f) + 10f);
		}
		if (_moveStepIndex == 0)
		{
			return p_t * p_t;
		}
		if (_moveStepIndex >= _moveTotalSteps - 1)
		{
			return p_t * (2f - p_t);
		}
		return p_t;
	}

	private bool CheckForAmbush(TileGO p_tileAboutToEnterGO, TileGO p_targetMoveTileGO, bool p_weakToObstacles)
	{
		if (unit.Type == "Plane" && p_tileAboutToEnterGO.airUnitGO != null)
		{
			if (unit.OwnerPlayer.IsAtWarWith(p_tileAboutToEnterGO.airUnitGO.unit.OwnerPlayer))
			{
				if (!TurnManager.isBadWeather && !unit.IsMissile)
				{
					new Notification(NotificationTypes.UNIT_AMBUSH, p_tileAboutToEnterGO.airUnitGO.unit.OwnerPlayer.Name, tileGO.transform.position.x, tileGO.transform.position.y, unit.GetUnitName(), p_isImportant: false);
					StartCoroutine(CR_Ambush(p_tileAboutToEnterGO.airUnitGO));
				}
				canUndo = false;
				return true;
			}
		}
		else if (unit.Type != "Plane" && p_tileAboutToEnterGO.groundUnitGO != null && !p_tileAboutToEnterGO.groundUnitGO.unit.IsMine && unit.OwnerPlayer.IsAtWarWith(p_tileAboutToEnterGO.groundUnitGO.unit.OwnerPlayer))
		{
			if (!p_tileAboutToEnterGO.groundUnitGO.unit.IsArmourObstacle)
			{
				new Notification(NotificationTypes.UNIT_AMBUSH, p_tileAboutToEnterGO.groundUnitGO.unit.OwnerPlayer.Name, tileGO.transform.position.x, tileGO.transform.position.y, unit.GetUnitName(), p_isImportant: false);
				StartCoroutine(CR_Ambush(p_tileAboutToEnterGO.groundUnitGO));
				canUndo = false;
				return true;
			}
			if (p_weakToObstacles || p_tileAboutToEnterGO == p_targetMoveTileGO)
			{
				canUndo = false;
				return true;
			}
		}
		return false;
	}

	public void RemoveFogOfWarAroundUnit()
	{
		if (TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			MapGO.instance.DestroyPotentialTilesAboveMap();
		}
		bool flag = TurnManager.humanPlayer != null && TurnManager.humanPlayer.IsAlliedWith(TurnManager.currPlayer);
		int num;
		if (unit.IsRADAR && TurnManager.currPlayer.IsAlliedWith(unit.OwnerPlayer))
		{
			num = unit.Range;
			if (unit.IsCarrier)
			{
				num = 5;
			}
			else if (unit.FilterType == "FilterBoat" && num < 3)
			{
				num = 3;
			}
			if (unit.OwnerPlayer != null && unit.OwnerPlayer.HasPolicy(Policies.Type.Improved_radars))
			{
				num += (int)Math.Round((float)num * 0.5f, MidpointRounding.AwayFromZero);
			}
			foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, num))
			{
				if (!MapGO.radarDetections.Contains(item.tileGO) && item.tileGO != tileGO && item.tileGO.airUnitGO != null && !unit.OwnerPlayer.IsAlliedWith(item.tileGO.airUnitGO.unit.OwnerPlayer))
				{
					MapGO.radarDetections.Add(item.tileGO);
					if (item.tileGO.isInFogOfWar && (flag || MapGO.IsAIOnly))
					{
						item.tileGO.SpotUnit(item.tileGO.airUnitGO.unit);
					}
				}
			}
		}
		if (unit.HasSonar && TurnManager.currPlayer.IsAlliedWith(unit.OwnerPlayer))
		{
			num = 3;
			if (unit.OwnerPlayer != null && unit.OwnerPlayer.HasPolicy(Policies.Type.Improved_radars))
			{
				num += (int)Math.Round((float)num * 0.5f, MidpointRounding.AwayFromZero);
			}
			foreach (Tile item2 in Utils.GetAllTilesInRadiusAround(tileGO.tile, num))
			{
				if (item2.Type == "Sea" && !MapGO.sonarDetections.Contains(item2.tileGO) && item2.tileGO != tileGO && item2.tileGO.groundUnitGO != null && !item2.tileGO.groundUnitGO.unit.IsSubmerged && !unit.OwnerPlayer.IsAlliedWith(item2.tileGO.groundUnitGO.unit.OwnerPlayer) && !item2.tileGO.groundUnitGO.unit.IsMine && item2.tileGO.groundUnitGO.unit.Type == "Boat")
				{
					MapGO.sonarDetections.Add(item2.tileGO);
					if (item2.tileGO.isInFogOfWar && (flag || MapGO.IsAIOnly))
					{
						item2.tileGO.SpotUnit(item2.tileGO.groundUnitGO.unit);
					}
				}
			}
		}
		if (unit.IsSubmerged || unit.IsOnBoat)
		{
			num = 1;
		}
		else
		{
			num = unit.BaseVisibility;
			if (unit.Hero != null)
			{
				num += unit.Hero.VisibilityBonus;
			}
			if (TurnManager.isBadWeather)
			{
				if (unit.Type == "Plane")
				{
					num = 0;
				}
				else if (num > 1)
				{
					num--;
				}
			}
			else if (!unit.IsRecon && (tileGO.tile.enumType == TileTypes.HILL || tileGO.tile.enumType == TileTypes.MOUNTAIN) && unit.Type != "Plane" && unit.BaseVisibility > 0)
			{
				num++;
			}
		}
		foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(tileGO.tile, num))
		{
			if (!item3.isVisible && unit.OwnerPlayer.IsAlliedWith(TurnManager.currPlayer))
			{
				item3.isVisible = true;
				listOfNewlyDiscoveredTiles.Add(item3.tileGO);
				if (item3.tileGO.airUnitGO != null && !unit.OwnerPlayer.IsAlliedWith(item3.tileGO.airUnitGO.unit.OwnerPlayer))
				{
					canUndo = false;
				}
				if (item3.tileGO.groundUnitGO != null && !unit.OwnerPlayer.IsAlliedWith(item3.tileGO.groundUnitGO.unit.OwnerPlayer))
				{
					canUndo = false;
				}
				if (item3.IsVictoryPoint && item3.ownerPlayer != null && !unit.OwnerPlayer.IsAlliedWith(item3.ownerPlayer))
				{
					canUndo = false;
				}
			}
			bool isInFogOfWar = item3.tileGO.isInFogOfWar;
			if (MapGO.IsAIOnly)
			{
				if (TurnManager.currPlayer.IsAlliedWith(unit.OwnerPlayer))
				{
					item3.tileGO.RemoveFogOfWar(isMoving);
				}
			}
			else if (TurnManager.humanPlayer.IsAlliedWith(unit.OwnerPlayer))
			{
				item3.tileGO.RemoveFogOfWar(isMoving);
			}
			if ((!MainMenu.isLoadedGame & isInFogOfWar) && !item3.tileGO.isInFogOfWar && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
			{
				unit.OwnerPlayer.ClearedFOW.Add(new Tile.Coordinates(item3));
			}
			if (item3.isVisible)
			{
				MapGO.radarDetections.Remove(item3.tileGO);
				MapGO.sonarDetections.Remove(item3.tileGO);
			}
			if (item3.tileGO.groundUnitGO != null || item3.tileGO.airUnitGO != null)
			{
				if (item3.tileGO.groundUnitGO != null && unit.OwnerPlayer.IsAtWarWith(item3.tileGO.groundUnitGO.unit.OwnerPlayer) && unit.AsleepState == AsleepStates.SLEEP)
				{
					unit.AsleepState = AsleepStates.NONE;
				}
				if (item3.tileGO.airUnitGO != null && unit.OwnerPlayer.IsAtWarWith(item3.tileGO.airUnitGO.unit.OwnerPlayer) && unit.AsleepState == AsleepStates.SLEEP)
				{
					unit.AsleepState = AsleepStates.NONE;
				}
			}
			if (item3.tileGO.airUnitGO != null && item3.tileGO.groundUnitGO == null)
			{
				item3.tileGO.airUnitGO.gameObject.SetActive(value: true);
			}
			if (item3.tileGO.groundUnitGO != null && item3.tileGO.airUnitGO == null)
			{
				item3.tileGO.groundUnitGO.gameObject.SetActive(value: true);
			}
			if (item3.tileGO.airUnitGO != null && item3.tileGO.groundUnitGO != null)
			{
				if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO == item3.tileGO.airUnitGO)
				{
					item3.tileGO.airUnitGO.gameObject.SetActive(value: true);
					if (!item3.tileGO.groundUnitGO.isMoving)
					{
						item3.tileGO.groundUnitGO.gameObject.SetActive(value: false);
					}
				}
				else if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO == item3.tileGO.groundUnitGO)
				{
					if (!item3.tileGO.airUnitGO.isMoving)
					{
						item3.tileGO.airUnitGO.gameObject.SetActive(value: false);
					}
					item3.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				}
				else if (UIManager.showAirUnit)
				{
					item3.tileGO.airUnitGO.gameObject.SetActive(value: true);
					if (!item3.tileGO.groundUnitGO.isMoving)
					{
						item3.tileGO.groundUnitGO.gameObject.SetActive(value: false);
					}
				}
				else if (item3.tileGO.groundUnitGO.unit.IsMine && !item3.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(TurnManager.currPlayer))
				{
					item3.tileGO.airUnitGO.gameObject.SetActive(value: true);
					if (!item3.tileGO.groundUnitGO.isMoving)
					{
						item3.tileGO.groundUnitGO.gameObject.SetActive(value: false);
					}
				}
				else
				{
					if (!item3.tileGO.airUnitGO.isMoving)
					{
						item3.tileGO.airUnitGO.gameObject.SetActive(value: false);
					}
					item3.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				}
			}
			if (item3.tileGO.airUnitGO != null)
			{
				item3.tileGO.airUnitGO.UpdateCounter();
			}
			if (item3.tileGO.groundUnitGO != null)
			{
				item3.tileGO.groundUnitGO.UpdateCounter();
			}
		}
		UpdateCounter();
	}

	private IEnumerator CR_Ambush(UnitGO p_unitGOambushing)
	{
		if (unit.IsSubmerged || (p_unitGOambushing.unit.Type == "Plane" && p_unitGOambushing.unit.FilterType != "FilterFighter"))
		{
			yield break;
		}
		canUndo = false;
		unit.hasBeenAttackedThisTurn = true;
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar || !p_unitGOambushing.tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 1f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 0.5f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		if (tileGO.isInFogOfWar && p_unitGOambushing.tileGO.isInFogOfWar)
		{
			yield return new WaitForEndOfFrame();
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
			UnityEngine.Object.Destroy(gameObject.GetComponent<DamageTextGO>());
			gameObject.transform.SetParent(tileGO.transform);
			gameObject.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-ambush");
			Vector3 position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y + 5f, -20f);
			gameObject.transform.position = position;
			gameObject.transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, Vector3.one, 0.5f).setEaseOutElastic();
			UnityEngine.Object.Destroy(gameObject, 2f);
			if (!unit.OwnerPlayer.IsComputer || !p_unitGOambushing.unit.OwnerPlayer.IsComputer)
			{
				yield return new WaitForSeconds(0.5f);
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
		}
		unit.NumberOfTimesTheUnitMoved = 2;
		p_unitGOambushing.unit.AddRemoveActionPoints(1);
		p_unitGOambushing.unit.HasAttacked = false;
		p_unitGOambushing.ShowPotentialTargets();
		p_unitGOambushing.unit.GetPotentialDamage(this, p_isRetaliation: false, p_isSupportiveFire: false);
		p_unitGOambushing.targetedUnitGO = this;
		p_unitGOambushing.AttackUnit(this, p_isRetaliationPossible: false, p_isSupportPossible: false, p_isAmbush: true);
		p_unitGOambushing.unit.AddRemoveActionPoints(1);
		p_unitGOambushing.unit.HasAttacked = false;
		SelectUnit(p_playAnimation: false);
		if (unit.CurrHP == 0)
		{
			MapGO.instance.UnselectUnit();
		}
	}

	public void AttackUnit(UnitGO p_unitGO, bool p_isRetaliationPossible, bool p_isSupportPossible, bool p_isAmbush)
	{
		if (p_unitGO == null)
		{
			Debug.LogError("Targetted unit is null.", this);
			return;
		}
		if (p_unitGO.unit.Type == "Plane" && unit.FilterType != "FilterAA" && unit.Type != "Plane")
		{
			Debug.LogError("Should not be able to attack a plane: " + unit.Name, this);
			return;
		}
		if (unit.OwnerPlayer.IsAlliedWith(p_unitGO.unit.OwnerPlayer))
		{
			Debug.LogError("ATTACKING A UNIT WE ARE ALLIED WITH: " + p_unitGO, this);
			return;
		}
		canOverrun = false;
		tileToOverrun = null;
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar || !p_unitGO.tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 2f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 1.5f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		canUndo = false;
		float p_distance = Vector2.Distance(p_unitGO.transform.position, transform.position);
		if (tileGO != p_unitGO.tileGO)
		{
			RotateAttackedUnit(this, p_unitGO);
		}
		string text = unit.AttackSound;
		if (p_unitGO.unit.Type != "Plane")
		{
			if (unit.HasBombs && unit.HasRockets)
			{
				text = "Light bomber";
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", p_unitGO.tileGO.tile.PosX, p_unitGO.tileGO.tile.PosY, text, "Attack");
				text = "Panzerschrek";
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", p_unitGO.tileGO.tile.PosX, p_unitGO.tileGO.tile.PosY, text, "Attack");
			}
			else if (unit.HasBombs)
			{
				text = "Light bomber";
			}
			else if (unit.HasRockets)
			{
				text = "Panzerschrek";
			}
			else if (unit.HasTorpedo)
			{
				if (p_unitGO.unit.FilterType == "FilterBoat")
				{
					text = "Bomber torpedo";
				}
				else
				{
					text = ((!(unit.FilterType == "FilterBomber")) ? "Light bomber" : "Heavy bomber");
				}
			}
			else if (unit.IsSubmarine && p_unitGO.unit.Type != "Boat" && !p_unitGO.unit.IsOnBoat)
			{
				text = "35mm AA";
			}
			else if (unit.HasABomb)
			{
				text = "V2";
			}
		}
		if (!p_unitGO.tileGO.isInFogOfWar)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(text));
		}
		MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", p_unitGO.tileGO.tile.PosX, p_unitGO.tileGO.tile.PosY, text, "Attack");
		int actionPoints = unit.ActionPoints;
		int currHP = p_unitGO.unit.CurrHP;
		AttackTile(p_unitGO.tileGO);
		if (unit.ActionPoints < actionPoints)
		{
			unit.AddRemoveActionPoints(1);
		}
		unit.GetPotentialDamage(p_unitGO, p_isRetaliation: false, p_isSupportiveFire: false);
		if (p_isAmbush && unit.OwnerPlayer.HasPolicy(Policies.Type.Camouflage))
		{
			unit.FinalDamage += (int)Math.Round((float)unit.FinalDamage * 0.3f, MidpointRounding.ToEven);
		}
		unit.OwnerPlayer.TotalDamageDealt += unit.FinalDamage;
		if (GameData.Instance.map.NumberOfDamageToWin != 0)
		{
			MapGO.CheckIfEndOfScenario();
		}
		tileToOverrun = p_unitGO.tileGO;
		Attack(p_unitGO);
		if (p_unitGO.unit.IsSubmerged && UnityEngine.Random.Range(0, 101) <= 30)
		{
			p_unitGO.DiveUndiveSubmarine(p_sendRPC: true);
		}
		if (unit.IsSubmarine)
		{
			new Notification(NotificationTypes.UNIT_UNDER_ATTACK, p_unitGO.unit.OwnerPlayer.Name, p_unitGO.tileGO.transform.position.x, p_unitGO.tileGO.transform.position.y, p_unitGO.unit.Name + ";" + unit.Name, p_unitGO.unit.IsCoreUnit);
		}
		if (unit.FinalDamage > p_unitGO.unit.FinalDamage)
		{
			unit.AddMorale(5);
			targetedUnitGO.unit.AddMorale(-5);
		}
		else if (unit.FinalDamage < p_unitGO.unit.FinalDamage)
		{
			unit.AddMorale(-5);
			p_unitGO.unit.AddMorale(5);
		}
		if (p_unitGO.unit.Type != "Plane")
		{
			int num = (int)((float)unit.FinalDamage / (float)currHP * 100f);
			if (num < 5)
			{
				num = 5;
			}
			p_unitGO.unit.AddMorale(num);
		}
		DisplayDamage(this, p_unitGO);
		if (TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			RemoveFogOfWarAroundUnit();
		}
		if (p_isSupportPossible && unit.FilterType != "FilterBoat" && unit.FilterType != "FilterArtillery" && !unit.CoastalBattery && !unit.IsArmourObstacle)
		{
			p_unitGO.CheckForDefenseSupport(this);
		}
		if (p_isRetaliationPossible)
		{
			p_unitGO.Retaliate(this, p_distance);
		}
		if (p_unitGO.unit.CurrHP <= 0 || p_unitGO.isRetreating)
		{
			canOverrun = true;
		}
		UpdateSelectedUnitStats();
		if (!unit.OwnerPlayer.IsComputer && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			MapGO.instance.selectedUnitIcon.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -6f);
		}
		if (TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			MapGO.ClearTargetedUnits();
		}
		if (TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer) && !unit.OwnerPlayer.IsComputer)
		{
			gameObject.SetActive(value: true);
			Invoke("ShowPotentialPaths", 0.1f);
		}
		if (UIManager.instance.potentialDamagePanel.activeSelf)
		{
			UIManager.instance.potentialDamagePanel.SetActive(value: false);
		}
		if (unit.IsMissile)
		{
			unit.CurrHP = 0;
			DestroyUnit(p_playAnimation: true, p_sendRPC: true);
		}
		if (unit.CurrHP == 0)
		{
			MapGO.instance.UnselectUnit();
			canOverrun = false;
		}
		if (p_isAmbush || !canOverrun || !PlayerSettings.Instance.Overrun || tileToOverrun == null || unit.HasOverrun || unit.CurrHP == 0 || unit.IsRecon || tileToOverrun.tile.enumType == TileTypes.SEA || (tileToOverrun.tile.enumType == TileTypes.RIVER && !unit.CanMoveOnRivers()) || tileToOverrun.tile.MPCost > unit.MaxMP || tileToOverrun.tile.MPCost > unit.CurrAutonomy || tileToOverrun.tile.IsBridgeDestroyed || (tileToOverrun.tile.enumType == TileTypes.MOUNTAIN && !unit.CanMoveOverMountains()) || (tileToOverrun.tile.enumType == TileTypes.MARSH && !unit.CanMoveOnMarshes()) || (tileToOverrun.tile.terrainOwner != null && !unit.OwnerPlayer.IsAlliedWith(tileToOverrun.tile.terrainOwner) && !unit.OwnerPlayer.IsAtWarWith(tileToOverrun.tile.terrainOwner)) || (!(unit.FilterType == "FilterTank") && (!(unit.FilterType == "FilterInfantry") || !unit.IsMechanized)))
		{
			return;
		}
		if (unit.OwnerPlayer.IsComputer)
		{
			if (unit.CurrAmmo > 0 && unit.CurrAutonomy >= tileToOverrun.tile.MPCost)
			{
				Overrun();
			}
		}
		else if (!unit.OwnerPlayer.IsComputer)
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("confirmation-overrun"), Overrun);
		}
	}

	private void DelayOverrunWindow()
	{
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("confirmation-overrun"), Overrun);
	}

	private void Overrun()
	{
		if (MapGO.selectedUnitGO.isMoving)
		{
			if (!unit.OwnerPlayer.IsComputer && !TurnManager.currPlayer.IsComputer)
			{
				Invoke("DelayOverrunWindow", 0.1f);
			}
		}
		else
		{
			if (tileToOverrun.tile.GroundUnit != null)
			{
				return;
			}
			AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
			if (!tileGO.isInFogOfWar || !tileToOverrun.isInFogOfWar)
			{
				if (PlayerSettings.Instance.IsAISlowMode)
				{
					aIWaitInstruction.waitTime = 0.75f;
				}
				else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
				{
					aIWaitInstruction.waitTime = 0.35f;
				}
			}
			AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
			if (!tileGO.isInFogOfWar || !tileToOverrun.isInFogOfWar)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
				UnityEngine.Object.Destroy(gameObject.GetComponent<DamageTextGO>());
				gameObject.transform.SetParent(tileGO.transform);
				gameObject.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-overrun");
				Vector3 position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y + 5f, -20f);
				gameObject.transform.position = position;
				gameObject.transform.localScale = Vector3.zero;
				LeanTween.scale(gameObject, Vector3.one, 0.5f).setEaseOutElastic();
				UnityEngine.Object.Destroy(gameObject, 2f);
				MultiplayerManager.Instance.RunRPC("RPC_ShowDamage", "O", tileGO.tile.PosX, tileGO.tile.PosY, "Overrun !");
			}
			if (unit.CurrMP != unit.MaxMP)
			{
				unit.CurrMP += tileToOverrun.tile.MPCost;
			}
			overrunning = true;
			listOfpotentialTiles = new List<TileGO> { tileToOverrun };
			MoveUnitToTile(tileToOverrun);
			unit.AddRemoveActionPoints(1);
			unit.HasAttacked = false;
			unit.HasOverrun = true;
			canUndo = false;
		}
	}

	private void StopAttackMotionAndSnapToTile()
	{
		if (_attackMotionTweenId != -1 && !(tileGO == null))
		{
			Vector3 p_basePos = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, transform.position.z);
			CancelAttackMotionTween(p_basePos);
		}
	}

	private void TryPlayAttackMotion(UnitGO p_targetUnitGO)
	{
		if (!PlayerSettings.Instance.BattleAnimations || isMoving || p_targetUnitGO == null || p_targetUnitGO.tileGO == null || tileGO == null || tileGO.isInFogOfWar || CameraGO.instance == null || CameraGO.instance.cam.orthographicSize > 100f || !CameraGO.instance.IsWithinFOV(transform.position))
		{
			return;
		}
		bool flag = unit.FilterType == "FilterAA" && p_targetUnitGO.unit.Type == "Plane";
		bool flag2 = unit.FilterType == "FilterArtillery";
		bool flag3 = unit.IsSubmarine && p_targetUnitGO.unit.Type == "Boat" && !p_targetUnitGO.unit.IsSubmarine;
		bool flag4 = unit.Type == "Plane" && unit.FilterType == "FilterFighter" && p_targetUnitGO.unit.Type == "Plane";
		bool flag5 = unit.Type == "Boat" && !unit.IsSubmarine && p_targetUnitGO.unit.Type == "Boat";
		bool flag6 = unit.Type == "Boat" && !unit.IsSubmarine && !unit.IsOnBoat && p_targetUnitGO.unit.Type != "Plane" && p_targetUnitGO.unit.Type != "Boat" && !p_targetUnitGO.unit.IsOnBoat;
		bool flag7 = unit.Type == "Plane" && !unit.IsMissile && p_targetUnitGO.unit.Type != "Plane" && p_targetUnitGO.unit.Type != "Boat" && !p_targetUnitGO.unit.IsOnBoat;
		bool flag8 = !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7 && unit.Range == 1 && unit.MaxMP > 0 && unit.Type != "Plane" && (unit.Type != "Boat" || unit.IsOnBoat) && !unit.IsTrain && !unit.IsCommander && !unit.IsMine && unit.FilterType != "FilterBuilding";
		if ((!flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7 && !flag8) || flag)
		{
			return;
		}
		Vector3 basePos = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, transform.position.z);
		Vector3 vector = new Vector3(p_targetUnitGO.tileGO.transform.position.x, p_targetUnitGO.tileGO.transform.position.y, transform.position.z) - basePos;
		if (flag7 && vector.sqrMagnitude < 0.0001f)
		{
			vector = GetAirToGroundMotionDirection();
		}
		if (vector.sqrMagnitude < 0.0001f)
		{
			return;
		}
		vector.Normalize();
		Vector3 vector2 = new Vector3(0f - vector.y, vector.x, 0f);
		if ((tileGO.tile.PosX + tileGO.tile.PosY) % 2 == 0)
		{
			vector2 = -vector2;
		}
		Vector3 motionPos;
		float time;
		float holdDuration;
		float backDuration;
		LeanTweenType ease;
		LeanTweenType backEase;
		if (flag2)
		{
			motionPos = basePos - vector * 0.18f;
			time = 0.04f;
			holdDuration = 0.05f;
			backDuration = 0.2f;
			ease = LeanTweenType.easeOutExpo;
			backEase = LeanTweenType.easeOutBack;
		}
		else if (flag3)
		{
			motionPos = basePos + vector * 0.22f - vector2 * 0.06f;
			time = 0.085f;
			holdDuration = 0.02f;
			backDuration = 0.13f;
			ease = LeanTweenType.easeInOutSine;
			backEase = LeanTweenType.easeOutCubic;
		}
		else if (flag5)
		{
			motionPos = basePos - vector * 0.14f + vector2 * 0.05f;
			time = 0.065f;
			holdDuration = 0.02f;
			backDuration = 0.16f;
			ease = LeanTweenType.easeOutQuart;
			backEase = LeanTweenType.easeOutBack;
		}
		else if (flag6)
		{
			motionPos = basePos - vector * 0.17f + vector2 * 0.03f;
			time = 0.06f;
			holdDuration = 0.025f;
			backDuration = 0.17f;
			ease = LeanTweenType.easeOutQuart;
			backEase = LeanTweenType.easeOutBack;
		}
		else if (flag4)
		{
			motionPos = basePos + vector * 0.34f + vector2 * 0.12f;
			time = 0.045f;
			holdDuration = 0f;
			backDuration = 0.09f;
			ease = LeanTweenType.easeInExpo;
			backEase = LeanTweenType.easeOutExpo;
		}
		else
		{
			if (flag7)
			{
				Vector3 vector3 = new Vector3(0f - vector.y, vector.x, 0f);
				if (vector3.sqrMagnitude < 0.0001f)
				{
					vector3 = Vector3.right;
				}
				else
				{
					vector3.Normalize();
				}
				Vector3 prepPos = basePos - vector * 0.06f - vector3 * 0.08f;
				Vector3 strikePos = basePos + vector * 0.35f + vector3 * 0.14f;
				CancelAttackMotionTween(basePos);
				_attackMotionTweenId = LeanTween.move(gameObject, prepPos, 0.035f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
				{
					transform.position = prepPos;
					_attackMotionTweenId = LeanTween.move(gameObject, strikePos, 0.055f).setEase(LeanTweenType.easeInCubic).setOnComplete(() =>
					{
						transform.position = strikePos;
						StartAttackMotionReturnTween(basePos, 0.005f, 0.16f, LeanTweenType.easeOutExpo);
					})
						.id;
				})
					.id;
				return;
			}
			motionPos = basePos + vector * 0.26f;
			time = 0.055f;
			holdDuration = 0.015f;
			backDuration = 0.07f;
			ease = LeanTweenType.easeInCubic;
			backEase = LeanTweenType.easeOutQuad;
		}
		CancelAttackMotionTween(basePos);
		_attackMotionTweenId = LeanTween.move(gameObject, motionPos, time).setEase(ease).setOnComplete(() =>
		{
			transform.position = motionPos;
			StartAttackMotionReturnTween(basePos, holdDuration, backDuration, backEase);
		})
			.id;
	}

	private Vector3 GetAirToGroundMotionDirection()
	{
		switch (unit.FacingRotation)
		{
		case FacingRotations.WEST:
			return Vector3.left;
		case FacingRotations.EAST:
			return Vector3.right;
		case FacingRotations.NORTH:
			return Vector3.up;
		case FacingRotations.SOUTH:
			return Vector3.down;
		case FacingRotations.NORTH_EAST:
			return new Vector3(1f, 1f, 0f).normalized;
		case FacingRotations.NORTH_WEST:
			return new Vector3(-1f, 1f, 0f).normalized;
		case FacingRotations.SOUTH_EAST:
			return new Vector3(1f, -1f, 0f).normalized;
		case FacingRotations.SOUTH_WEST:
			return new Vector3(-1f, -1f, 0f).normalized;
		default:
			if ((tileGO.tile.PosX + tileGO.tile.PosY) % 2 == 0)
			{
				return new Vector3(1f, 0.35f, 0f).normalized;
			}
			return new Vector3(-1f, 0.35f, 0f).normalized;
		}
	}

	private void CancelAttackMotionTween(Vector3 p_basePos)
	{
		if (_attackMotionTweenId != -1)
		{
			LeanTween.cancel(_attackMotionTweenId);
			_attackMotionTweenId = -1;
		}
		transform.position = p_basePos;
	}

	private void StartAttackMotionReturnTween(Vector3 p_basePos, float p_holdDuration, float p_backDuration, LeanTweenType p_backEase)
	{
		_attackMotionTweenId = LeanTween.move(gameObject, p_basePos, p_backDuration).setDelay(p_holdDuration).setEase(p_backEase)
			.setOnComplete(() =>
			{
				transform.position = p_basePos;
				_attackMotionTweenId = -1;
			})
			.id;
	}

	private void Attack(UnitGO p_targetUnitGO)
	{
		TutorialManager.PerformAction(TutorialActions.ATTACK_ENEMY_UNIT);
		TryPlayAttackMotion(p_targetUnitGO);
		unit.IsDefensive = false;
		p_targetUnitGO.unit.IsDefensive = false;
		p_targetUnitGO.unit.hasBeenAttackedThisTurn = true;
		_missileArrived = unit.IsMissile;
		if (!PlayerSettings.Instance.IsInfiniteAmmoAndFuel && ((!(unit.FilterType == "FilterCAS") && !(unit.FilterType == "FilterBomber")) || !(p_targetUnitGO.unit.FilterType == "FilterFighter")))
		{
			if (unit.HasABomb)
			{
				unit.CurrAmmo = 0;
			}
			else
			{
				unit.CurrAmmo--;
			}
		}
		if (!PlayerSettings.Instance.IsInfiniteAmmoAndFuel && (unit.FilterType == "FilterArtillery" || unit.FilterType == "FilterBomber" || unit.FilterType == "FilterCAS" || unit.HasBombs || unit.HasRockets || unit.Name.Contains("battleship") || unit.Name.Contains("heavy cruiser") || unit.Name.Contains("battlecruiser")) && p_targetUnitGO.unit.Type != "Plane" && p_targetUnitGO.unit.Type != "Boat" && (p_targetUnitGO.unit.CurrAmmo > 0 || p_targetUnitGO.unit.CurrAutonomy > 0))
		{
			short ammoAttrition = unit.GetAmmoAttrition(p_targetUnitGO.unit);
			short fuelAttrition = unit.GetFuelAttrition(p_targetUnitGO.unit);
			p_targetUnitGO.unit.CurrAmmo -= ammoAttrition;
			p_targetUnitGO.unit.CurrAutonomy -= fuelAttrition;
			p_targetUnitGO.DisplayAmmoLoss("-" + ammoAttrition);
			p_targetUnitGO.DisplayFuelLoss("-" + fuelAttrition);
			if (p_targetUnitGO.unit.CurrAmmo <= 0)
			{
				p_targetUnitGO.unit.CurrAmmo = 0;
			}
			if (p_targetUnitGO.unit.CurrAutonomy <= 0)
			{
				p_targetUnitGO.unit.CurrAutonomy = 0;
				p_targetUnitGO.unit.CurrMP = 0;
			}
			if (p_targetUnitGO.unit.CurrAmmo <= 0 && p_targetUnitGO.unit.CurrAutonomy <= 0 && (p_targetUnitGO.unit.MaxAmmo > 0 || p_targetUnitGO.unit.CurrAutonomy > 0))
			{
				new Notification(NotificationTypes.UNIT_NO_SUPPLIES, p_targetUnitGO.unit.OwnerPlayer.Name, p_targetUnitGO.tileGO.transform.position.x, p_targetUnitGO.tileGO.transform.position.y, p_targetUnitGO.unit.GetUnitName(), p_isImportant: true);
			}
			if (unit.HasABomb)
			{
				foreach (Tile neighbour in p_targetUnitGO.tileGO.tile.neighbours)
				{
					if (neighbour.tileGO.groundUnitGO != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer != null)
					{
						neighbour.tileGO.groundUnitGO.unit.CurrHP = 0;
						neighbour.tileGO.groundUnitGO.UpdateCounter();
						DisplayDamage(this, neighbour.tileGO.groundUnitGO);
						if (neighbour.tileGO.groundUnitGO.unit.CurrHP <= 0)
						{
							neighbour.tileGO.groundUnitGO.unit.CurrHP = 0;
							neighbour.tileGO.groundUnitGO.DestroyUnit(p_playAnimation: true, p_sendRPC: true);
						}
					}
				}
			}
			TutorialManager.PerformAction(TutorialActions.BOMBARD_ENEMY_UNIT);
		}
		if (unit.Name == "V1")
		{
			unit.FinalDamage = UnityEngine.Random.Range(5, 36);
		}
		else if (unit.Name == "V2")
		{
			unit.FinalDamage = UnityEngine.Random.Range(10, 51);
		}
		if (PlayerSettings.Instance.RandomDamageModifier > 0)
		{
			float num = UnityEngine.Random.Range(-PlayerSettings.Instance.RandomDamageModifier, PlayerSettings.Instance.RandomDamageModifier + 1);
			num /= 100f;
			int num2 = (int)Math.Round((float)unit.FinalDamage * num, MidpointRounding.ToEven);
			unit.FinalDamage += num2;
			if (unit.FinalDamage < 0)
			{
				unit.FinalDamage = 0;
			}
		}
		p_targetUnitGO.HPsavedForRetaliation = p_targetUnitGO.unit.CurrHP;
		p_targetUnitGO.moraleBackupRetaliation = p_targetUnitGO.unit.Morale;
		p_targetUnitGO.unit.CurrHP -= unit.FinalDamage;
		if ((unit.Type == "Artillery" || unit.Type == "Plane" || unit.FilterType == "FilterBoat") && (p_targetUnitGO.unit.Type != "Boat" || p_targetUnitGO.unit.Type != "Plane") && p_targetUnitGO.unit.MaxMP > 0)
		{
			p_targetUnitGO.unit.AttackedAmountThisTurn++;
		}
		if (p_targetUnitGO.unit.EntrenchmentLevel > 0 && unit.CanRemoveEntrenchment())
		{
			int entrenchmentRemoval = unit.GetEntrenchmentRemoval(p_targetUnitGO.unit);
			p_targetUnitGO.unit.EntrenchmentLevel = (byte)Mathf.Clamp(p_targetUnitGO.unit.EntrenchmentLevel - entrenchmentRemoval, 0, p_targetUnitGO.unit.EntrenchmentLevel);
		}
		if (p_targetUnitGO.unit.CurrHP <= 0)
		{
			TutorialManager.PerformAction(TutorialActions.DESTROY_ENEMY_UNIT);
			p_targetUnitGO.DestroyUnit(p_playAnimation: true, p_sendRPC: true);
			unit.AddToKillCount(p_targetUnitGO.unit);
			unit.OwnerPlayer.TotalUnitsDestroyed++;
			unit.OwnerPlayer.TurnUnitsDestroyed++;
			p_targetUnitGO.unit.CurrHP = 0;
			if (p_targetUnitGO.unit.IsPartisans)
			{
				unit.AddXP(1, p_canAddHero: true, p_showXPUI: true);
			}
			else
			{
				unit.AddXP(7, p_canAddHero: true, p_showXPUI: true);
			}
			string text;
			if (p_targetUnitGO.unit.Type == "Plane")
			{
				text = LocalizationManager.Translate("history-shot down");
			}
			else
			{
				text = ((!(p_targetUnitGO.unit.Type == "Boat")) ? LocalizationManager.Translate("history-destroyed") : LocalizationManager.Translate("history-sank"));
			}
			if (string.IsNullOrEmpty(p_targetUnitGO.tileGO.tile.CityName))
			{
				unit.AddToHistory(text + p_targetUnitGO.unit.Name, p_sendRPC: false);
			}
			else
			{
				unit.AddToHistory(text + p_targetUnitGO.unit.Name + " (" + p_targetUnitGO.tileGO.tile.CityName + ").", p_sendRPC: false);
			}
			if (p_targetUnitGO.unit.Name.Contains("Tiger") || p_targetUnitGO.unit.Name.Contains("Panther") || p_targetUnitGO.unit.Name.Contains("Elefant"))
			{
				unit.OwnerPlayer.SafariKillCount++;
				CheckAchievements();
			}
			if (!unit.OwnerPlayer.IsComputer && unit.Name == "V1")
			{
				SteamManager.UnlockAchievement("V1_Kill_Streak");
			}
		}
		else if (!p_targetUnitGO.unit.IsPartisans)
		{
			unit.AddXP(2, p_canAddHero: true, p_showXPUI: true);
		}
		unit.HasAttacked = true;
		if (!unit.IsSubmarine)
		{
			unit.AddRemoveActionPoints(-1);
		}
		if ((float)p_targetUnitGO.unit.CurrHP <= (float)p_targetUnitGO.unit.MaxHP * 0.15f && p_targetUnitGO.unit.CurrHP > 0 && !p_targetUnitGO._surrendered)
		{
			new Notification(NotificationTypes.UNIT_LOW_HEALTH, p_targetUnitGO.unit.OwnerPlayer.Name, p_targetUnitGO.tileGO.transform.position.x, p_targetUnitGO.tileGO.transform.position.y, p_targetUnitGO.unit.GetUnitName(), p_targetUnitGO.unit.IsCoreUnit, p_targetUnitGO.unit.ID);
		}
		if (unit.CurrAmmo < 0)
		{
			unit.CurrAmmo = 0;
		}
		if (p_targetUnitGO.unit.CurrAmmo < 0)
		{
			p_targetUnitGO.unit.CurrAmmo = 0;
		}
		if (!unit.IsOnBoat && unit.MoveOrAttack)
		{
			unit.HasMoved = true;
		}
		if (PlayerSettings.Instance.OneMoreTilePlanes && unit.Type == "Plane")
		{
			if (unit.NumberOfTimesTheUnitMoved > 0)
			{
				unit.CurrMP = 1;
			}
			unit.HasMoved = false;
			if (unit.ActionPoints <= 0)
			{
				unit.ActionPoints = 1;
			}
			if (unit.NumberOfTimesTheUnitMoved - 1 >= 0)
			{
				unit.NumberOfTimesTheUnitMoved--;
			}
		}
		UpdateCounter();
		p_targetUnitGO.UpdateCounter();
		MultiplayerManager.Instance.RunRPC("RPC_AttackUnit", "O", unit, p_targetUnitGO.unit);
		if (MapGO.selectedUnitGO == this)
		{
			UIManager.instance.selectedUnitUI.RefreshIfIsSelectedUnit(unit);
			UIManager.instance.SelectedUnitExtraStatsUI.RefreshIfIsSelectedUnit(unit);
		}
		else if (MapGO.selectedUnitGO == p_targetUnitGO)
		{
			UIManager.instance.selectedUnitUI.RefreshIfIsSelectedUnit(p_targetUnitGO.unit);
			UIManager.instance.SelectedUnitExtraStatsUI.RefreshIfIsSelectedUnit(p_targetUnitGO.unit);
		}
		CheckAchievements();
	}

	private void CheckForDefenseSupport(UnitGO p_attackingUnitGO)
	{
		int heuristicDistance = Utils.GetHeuristicDistance(tileGO.tile, p_attackingUnitGO.tileGO.tile);
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, 1))
		{
			if (p_attackingUnitGO.unit.Type == "Plane")
			{
				if (item.tileGO.groundUnitGO != null && (item.tileGO.groundUnitGO.unit.FilterType == "FilterAA" || item.tileGO.groundUnitGO.unit.IsDestroyer || item.tileGO.groundUnitGO.unit.GetNameToLower().Contains("cruiser")) && !TurnManager.isBadWeather && (!PlayerSettings.Instance.NoAAsupportDuringAirFight || unit.Type != "Plane"))
				{
					if (item.tileGO.groundUnitGO == this || item.tileGO.groundUnitGO.unit.CurrAmmo <= 0 || item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(p_attackingUnitGO.unit.OwnerPlayer) || !item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer) || item.tileGO.groundUnitGO.unit.AirDamage <= 0 || item.tileGO.groundUnitGO.unit.IsOnBoat || heuristicDistance > item.tileGO.groundUnitGO.unit.Range)
					{
						continue;
					}
					if (item.tileGO.airUnitGO != null && (item.tileGO.airUnitGO.unit.FilterType != "FilterFighter" || item.tileGO.airUnitGO.unit.CurrAmmo == 0 || !item.tileGO.airUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer)))
					{
						item.tileGO.airUnitGO.gameObject.SetActive(value: false);
					}
					item.tileGO.groundUnitGO.gameObject.SetActive(value: true);
					item.tileGO.groundUnitGO.StartCoroutine(item.tileGO.groundUnitGO.CR_SupportAttackedUnit(p_attackingUnitGO));
				}
				if (item.tileGO.airUnitGO != null && item.tileGO.airUnitGO.unit.FilterType == "FilterFighter" && !(p_attackingUnitGO.targetedUnitGO.unit.FilterType == "FilterFighter") && !(item.tileGO.airUnitGO == this) && item.tileGO.airUnitGO.unit.CurrAmmo > 0 && !item.tileGO.airUnitGO.unit.OwnerPlayer.IsAlliedWith(p_attackingUnitGO.unit.OwnerPlayer) && item.tileGO.airUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer) && item.tileGO.airUnitGO.unit.AirDamage > 0 && heuristicDistance <= item.tileGO.airUnitGO.unit.Range)
				{
					if (item.tileGO.groundUnitGO != null && (item.tileGO.groundUnitGO.unit.FilterType != "FilterAA" || item.tileGO.groundUnitGO.unit.CurrAmmo == 0 || !item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer)))
					{
						item.tileGO.groundUnitGO.gameObject.SetActive(value: false);
					}
					item.tileGO.airUnitGO.gameObject.SetActive(value: true);
					item.tileGO.airUnitGO.StartCoroutine(item.tileGO.airUnitGO.CR_SupportAttackedUnit(p_attackingUnitGO));
				}
			}
			else
			{
				if (item.tileGO.groundUnitGO == null || (p_attackingUnitGO.unit.FilterType == "FilterAA" && unit.Type == "Plane") || item.tileGO.groundUnitGO == this || item.tileGO.groundUnitGO.unit.CurrAmmo <= 0 || item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(p_attackingUnitGO.unit.OwnerPlayer) || !item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer) || item.tileGO.groundUnitGO.unit.IsOnBoat || heuristicDistance > item.tileGO.groundUnitGO.unit.Range)
				{
					continue;
				}
				if (item.tileGO.groundUnitGO.unit.FilterType == "FilterArtillery" || (item.tileGO.groundUnitGO.unit.FilterType == "FilterBoat" && item.tileGO.groundUnitGO.unit.Range > 1))
				{
					if (item.tileGO.airUnitGO != null)
					{
						item.tileGO.airUnitGO.gameObject.SetActive(value: false);
					}
					item.tileGO.groundUnitGO.gameObject.SetActive(value: true);
					item.tileGO.groundUnitGO.StartCoroutine(item.tileGO.groundUnitGO.CR_SupportAttackedUnit(p_attackingUnitGO));
				}
				else
				{
					if (!(item.tileGO.groundUnitGO.unit.FilterType == "FilterAntitank") || (p_attackingUnitGO.unit.Type != "Tank" && (!p_attackingUnitGO.unit.IsMechanized || (p_attackingUnitGO.unit.IsMechanized && p_attackingUnitGO.unit.Type != "Infantry"))))
					{
						continue;
					}
					item.tileGO.groundUnitGO.unit.GetPotentialDamage(p_attackingUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
					if (item.tileGO.groundUnitGO.unit.FinalDamage > 0)
					{
						if (item.tileGO.airUnitGO != null)
						{
							item.tileGO.airUnitGO.gameObject.SetActive(value: false);
						}
						item.tileGO.groundUnitGO.gameObject.SetActive(value: true);
						item.tileGO.groundUnitGO.StartCoroutine(item.tileGO.groundUnitGO.CR_SupportAttackedUnit(p_attackingUnitGO));
					}
				}
			}
		}
	}

	private IEnumerator CR_SupportAttackedUnit(UnitGO p_targetUnitGO)
	{
		if (p_targetUnitGO.unit.CurrHP <= 0 || p_targetUnitGO.unit.IsDead)
		{
			yield break;
		}
		p_targetUnitGO.canUndo = false;
		p_targetUnitGO.unit.hasBeenAttackedThisTurn = true;
		p_targetUnitGO.canUndo = false;
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar || !p_targetUnitGO.tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 1f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 0.5f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		if (tileGO.isInFogOfWar && p_targetUnitGO.tileGO.isInFogOfWar)
		{
			yield return new WaitForEndOfFrame();
		}
		else if (PlayerSettings.Instance.IsAISlowMode)
		{
			yield return new WaitForSeconds(1f);
		}
		else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || !unit.OwnerPlayer.isAlliedWithHumanPlayer))
		{
			yield return new WaitForSeconds(0.5f);
		}
		else if (TurnManager.currPlayer.IsComputer)
		{
			yield return new WaitForEndOfFrame();
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (!p_targetUnitGO.tileGO.isInFogOfWar && ((TurnManager.humanPlayer == null) ? TurnManager.currPlayer : TurnManager.humanPlayer).IsAlliedWith(p_targetUnitGO.unit.OwnerPlayer))
		{
			tileGO.RemoveFogOfWar(p_smooth: true);
		}
		tileGO.tile.isVisible = true;
		if (tileGO.airUnitGO != null && tileGO.groundUnitGO != null)
		{
			if (this == tileGO.airUnitGO)
			{
				tileGO.groundUnitGO.backgroundSprite.color = Color.white;
				tileGO.groundUnitGO.unitSprite.color = Color.white;
				if (tileGO.groundUnitGO.unit.IsCoreUnit)
				{
					tileGO.groundUnitGO.coreUnitOutline_sprite.color = Color.white;
				}
				tileGO.groundUnitGO.UpdateCounter();
				tileGO.groundUnitGO.gameObject.SetActive(value: false);
			}
			else
			{
				tileGO.airUnitGO.backgroundSprite.color = Color.white;
				tileGO.airUnitGO.unitSprite.color = Color.white;
				if (tileGO.airUnitGO.unit.IsCoreUnit)
				{
					tileGO.airUnitGO.coreUnitOutline_sprite.color = Color.white;
				}
				tileGO.airUnitGO.UpdateCounter();
				tileGO.airUnitGO.gameObject.SetActive(value: false);
			}
		}
		RotateAttackedUnit(this, p_targetUnitGO);
		targetedUnitGO = p_targetUnitGO;
		unit.GetPotentialDamage(p_targetUnitGO, p_isRetaliation: false, p_isSupportiveFire: true);
		Attack(targetedUnitGO);
		string text = unit.AttackSound;
		if (unit.FilterType == "FilterBoat" && targetedUnitGO.unit.Type == "Plane")
		{
			text = "Heavy AA";
		}
		if (!p_targetUnitGO.tileGO.isInFogOfWar)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(text));
		}
		MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", p_targetUnitGO.tileGO.tile.PosX, p_targetUnitGO.tileGO.tile.PosY, text, "Attack");
		DisplayDamage(this, p_targetUnitGO);
		unit.HasAttacked = false;
		unit.AddRemoveActionPoints(1);
		MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", unit, false);
	}

	public void AttackTile(TileGO p_tileGO)
	{
		if (unit.IsSupplyShip || unit.IsRecon || unit.FilterType != "FilterBomber" || (p_tileGO.tile.enumType == TileTypes.SEA && !p_tileGO.tile.Bridge) || (!p_tileGO.tile.Bridge && !p_tileGO.tile.IsVictoryPoint) || (unit.OwnerPlayer.IsComputer && !p_tileGO.tile.IsVictoryPoint) || (!p_tileGO.tile.Bridge && p_tileGO.tile.FilterType != "FilterVP" && p_tileGO.tile.FilterType != "FilterHarbour") || p_tileGO.tile.Name.Contains("destroyed"))
		{
			return;
		}
		foreach (GameObject item in MapGO.listOfTargetedUnit)
		{
			if (p_tileGO.name != item.name)
			{
				continue;
			}
			if (Utils.GetTileSprite(new Tile
			{
				Season = p_tileGO.tile.Season,
				Name = p_tileGO.tile.Name + " destroyed",
				Type = p_tileGO.tile.Type
			}) == null)
			{
				break;
			}
			canUndo = false;
			if (p_tileGO.tile.Bridge)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
				gameObject.transform.SetParent(tileGO.transform);
				gameObject.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-bridge destroyed");
				p_tileGO.tile.IsBridgeDestroyed = true;
				Vector3 position = new Vector3(p_tileGO.transform.position.x, p_tileGO.transform.position.y, -20f);
				gameObject.transform.position = position;
				unit.AddToHistory(LocalizationManager.Translate("history-bombed bridge"), p_sendRPC: false);
			}
			else if (p_tileGO.tile.IsVictoryPoint && unit.Type == "Plane")
			{
				byte numberOfTurnsAtLowProd;
				if (unit.HasABomb)
				{
					numberOfTurnsAtLowProd = 15;
					if (string.IsNullOrEmpty(p_tileGO.tile.CityName))
					{
						unit.AddToHistory(LocalizationManager.Translate("history-bombed") + p_tileGO.tile.Name, p_sendRPC: false);
					}
					else
					{
						unit.AddToHistory(LocalizationManager.Translate("history-bombed") + p_tileGO.tile.CityName, p_sendRPC: false);
					}
				}
				else if (p_tileGO.tile.Type != "Airport")
				{
					numberOfTurnsAtLowProd = 5;
					if (string.IsNullOrEmpty(p_tileGO.tile.CityName))
					{
						unit.AddToHistory(LocalizationManager.Translate("history-bombed") + p_tileGO.tile.Name, p_sendRPC: false);
					}
					else
					{
						unit.AddToHistory(LocalizationManager.Translate("history-bombed") + p_tileGO.tile.CityName, p_sendRPC: false);
					}
				}
				else
				{
					numberOfTurnsAtLowProd = 3;
					unit.AddToHistory(LocalizationManager.Translate("history-bombed airfield"), p_sendRPC: false);
				}
				p_tileGO.tile.NumberOfTurnsAtLowProd = numberOfTurnsAtLowProd;
				unit.OwnerPlayer.TotalTilesBombed++;
				if (GameData.Instance.map.NumberOfTilesToBombToWin != 0)
				{
					MapGO.CheckIfEndOfScenario();
				}
			}
			unit.AddXP(2, p_canAddHero: true, p_showXPUI: true);
			if (!tileGO.isInFogOfWar)
			{
				string attackSound = unit.AttackSound;
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(attackSound));
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, attackSound, "Attack");
				attackSound = "Explosion2";
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(attackSound));
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, attackSound, "Attack");
			}
			if (!unit.OwnerPlayer.IsComputer)
			{
				SteamManager.UnlockAchievement("Bob the destroyer");
			}
			unit.HasAttacked = true;
			unit.AddRemoveActionPoints(-1);
			RemoveFogOfWarAroundUnit();
			MapGO.ClearTargetedUnits();
			if (!PlayerSettings.Instance.IsInfiniteAmmoAndFuel && tileGO.groundUnitGO == null)
			{
				unit.CurrAmmo--;
			}
			if (unit.IsMissile)
			{
				unit.CurrHP = 0;
				_missileArrived = true;
				DestroyUnit(p_playAnimation: true, p_sendRPC: true);
			}
			p_tileGO.tile.Name = p_tileGO.tile.Name + " destroyed";
			p_tileGO.SetSprite();
			if (PlayerSettings.Instance.OneMoreTilePlanes && unit.Type == "Plane")
			{
				if (unit.NumberOfTimesTheUnitMoved > 0)
				{
					unit.CurrMP = 1;
				}
				unit.HasMoved = false;
				if (unit.ActionPoints <= 0)
				{
					unit.ActionPoints = 1;
				}
				if (unit.NumberOfTimesTheUnitMoved - 1 >= 0)
				{
					unit.NumberOfTimesTheUnitMoved--;
				}
			}
			MultiplayerManager.RPC("RPC_SyncTile", p_tileGO.tile.GetCoord(), p_tileGO.tile);
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", unit, false);
			if (!unit.OwnerPlayer.IsComputer)
			{
				ShowPotentialPaths();
			}
			break;
		}
	}

	private void Retaliate(UnitGO p_attackerUnitGO, float p_distance)
	{
		if (unit.Range == 0)
		{
			if (p_distance >= 6f)
			{
				return;
			}
		}
		else if (p_distance > (float)(int)unit.Range * 5.2f)
		{
			return;
		}
		if (unit.IsTrain || unit.IsDropable || unit.IsOnBoat || (unit.Type == "Plane" && p_attackerUnitGO.unit.FilterType == "FilterAA") || (p_attackerUnitGO.unit.Type == "Plane" && unit.AirDamage <= 0) || (p_attackerUnitGO.unit.FilterType == "FilterBoat" && unit.FilterType != "FilterBoat" && unit.FilterType != "FilterBuilding") || (p_attackerUnitGO.unit.IsSubmarine && !unit.IsDestroyer))
		{
			return;
		}
		if (unit.IsCommander)
		{
			unit.FinalDamage = 0;
		}
		bool flag = false;
		if (CanRetreat(p_attackerUnitGO))
		{
			if (unit.CurrAmmo == 0)
			{
				Retreat(p_attackerUnitGO);
				return;
			}
			float retreatThreshold = GetRetreatThreshold();
			float num = (float)p_attackerUnitGO.unit.FinalDamage / (float)HPsavedForRetaliation;
			if (unit.FinalDamage <= 0)
			{
				if (num >= retreatThreshold)
				{
					Retreat(p_attackerUnitGO);
				}
				return;
			}
			flag = num >= retreatThreshold;
		}
		if (unit.IsCommander)
		{
			return;
		}
		int currHP = unit.CurrHP;
		byte morale = unit.Morale;
		unit.CurrHP = HPsavedForRetaliation;
		unit.Morale = moraleBackupRetaliation;
		unit.GetPotentialDamage(p_attackerUnitGO, p_isRetaliation: true, p_isSupportiveFire: false);
		unit.CurrHP = currHP;
		unit.Morale = morale;
		if (unit.FinalDamage > 0)
		{
			if (unit.FilterType != "FilterBomber" && unit.FilterType != "FilterCAS")
			{
				RotateAttackedUnit(this, p_attackerUnitGO);
			}
			targetedUnitGO = p_attackerUnitGO;
			unit.CurrHP = HPsavedForRetaliation;
			unit.Morale = moraleBackupRetaliation;
			unit.GetPotentialDamage(p_attackerUnitGO, p_isRetaliation: true, p_isSupportiveFire: false);
			Attack(p_attackerUnitGO);
			unit.CurrHP = currHP;
			unit.Morale = morale;
			DisplayDamage(this, p_attackerUnitGO);
			unit.HasAttacked = false;
			unit.AddRemoveActionPoints(1);
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", unit, false);
			string text = unit.AttackSound;
			if (p_attackerUnitGO.unit.Type == "Plane" && (unit.Type == "Boat" || unit.Name.Contains("Bunker") || unit.IsArmouredTrain))
			{
				text = "20mm";
			}
			else if (p_attackerUnitGO.unit.Type == "Plane" && (unit.FilterType == "FilterCAS" || unit.FilterType == "FilterBomber"))
			{
				text = "Fighter";
			}
			if (p_attackerUnitGO != null && !tileGO.isInFogOfWar)
			{
				SoundManager.instance.PlayDelayedUnitSound(0.5f, text);
			}
			MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, text, "Attack");
			if (flag)
			{
				Retreat(p_attackerUnitGO);
			}
			UpdateCounter();
			targetedUnitGO.UpdateCounter();
		}
	}

	public bool CanRetreat(UnitGO p_attackerUnitGO)
	{
		if (unit.MaxMP <= 0)
		{
			return false;
		}
		if (p_attackerUnitGO.unit.FinalDamage <= 0)
		{
			return false;
		}
		if (p_attackerUnitGO.unit.Type == "Plane")
		{
			return false;
		}
		if (p_attackerUnitGO.unit.FilterType == "FilterBoat")
		{
			return false;
		}
		if (unit.OwnerPlayer.HasPolicy(Policies.Type.No_step_back))
		{
			return false;
		}
		if (Utils.HasNKVDAround(tileGO.tile, unit))
		{
			return false;
		}
		if (p_attackerUnitGO.unit.FilterType == "FilterArtillery" && (p_attackerUnitGO.unit.Type != "Tank" || p_attackerUnitGO.unit.Range > 1))
		{
			return false;
		}
		return true;
	}

	public float GetRetreatThreshold()
	{
		float rETREAT_THRESHOLD = Unit.RETREAT_THRESHOLD;
		float num = rETREAT_THRESHOLD;
		if (unit.IsPoliticalUnit)
		{
			num += 0.05f;
		}
		if (unit.Lvl == 2)
		{
			num += 0.05f;
		}
		else if (unit.Lvl == 3)
		{
			num += 0.1f;
		}
		else if (unit.Lvl == 4)
		{
			num += 0.15f;
		}
		else if (unit.Lvl == 5)
		{
			num += 0.2f;
		}
		if (PlayerSettings.Instance.IsMorale)
		{
			if (moraleBackupRetaliation >= 75)
			{
				num *= 2f;
			}
			else if (moraleBackupRetaliation >= 60)
			{
				num += 0.05f;
			}
			else if (moraleBackupRetaliation >= 50)
			{
				num += 0f;
			}
			else if (moraleBackupRetaliation >= 25)
			{
				num -= 0.05f;
			}
			else if (moraleBackupRetaliation < 25)
			{
				num -= 0.1f;
			}
		}
		if (unit.Type == "Artillery" && (unit.FilterType == "FilterArtillery" || unit.FilterType == "FilterAA"))
		{
			num -= rETREAT_THRESHOLD / 2f;
		}
		if (tileGO.tile.IsVictoryPoint)
		{
			if (tileGO.tile.PrimaryObjective)
			{
				num += rETREAT_THRESHOLD * 2f;
			}
			else
			{
				num = ((!tileGO.tile.SecondaryObjective) ? (num + rETREAT_THRESHOLD) : (num + rETREAT_THRESHOLD * 1.5f));
			}
		}
		if (unit.IsPartisans || unit.GetNameToLower().Contains("militia") || unit.GetNameToLower().Contains("volkssturm") || unit.GetNameToLower().Contains("conscript") || unit.GetNameToLower().Contains("garrison"))
		{
			num -= 0.05f;
		}
		return num;
	}

	private void Retreat(UnitGO p_attackerUnitGO)
	{
		if (unit.CurrHP <= 0 || unit.IsDead || unit.FilterType == "FilterBuilding" || unit.Type == "Plane" || unit.IsOnBoat || unit.IsArmouredTrain || unit.IsTrain || isMoving)
		{
			return;
		}
		AIWaitInstruction item = new AIWaitInstruction(0f);
		AI.AIWaitInstructions.Enqueue(item);
		if (unit.NumberOfTimesTheUnitMoved >= 2)
		{
			unit.NumberOfTimesTheUnitMoved = 1;
		}
		if (unit.ActionPoints <= 0)
		{
			unit.ActionPoints++;
		}
		if (unit.CurrMP != unit.MaxMP)
		{
			unit.CurrMP = unit.MaxMP;
		}
		if (unit.HasMoved)
		{
			unit.HasMoved = false;
		}
		isRetreating = true;
		ShowPotentialPaths();
		isRetreating = false;
		if (listOfpotentialTiles.Count == 0)
		{
			bool flag = false;
			foreach (Tile neighbour in this.tileGO.tile.neighbours)
			{
				if (neighbour.MPCost <= unit.MaxMP && neighbour.GroundUnit != null && (neighbour.enumType != TileTypes.SEA || neighbour.isFrozen || unit.IsOnBoat) && (neighbour.enumType != TileTypes.RIVER || neighbour.isFrozen || unit.CanMoveOnRivers()) && (neighbour.enumType != TileTypes.MARSH || unit.CanMoveOnMarshes()) && (neighbour.enumType != TileTypes.MOUNTAIN || unit.CanMoveOverMountains()) && unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				unit.NumberOfTimesTheUnitMoved = short.MaxValue;
			}
			else
			{
				Surrender(p_attackerUnitGO);
			}
			return;
		}
		int num = 0;
		Vector3 zero = Vector3.zero;
		foreach (Tile item3 in Utils.GetAllTilesInRadiusAround(this.tileGO.tile, 3))
		{
			if (item3.GroundUnit != null && unit.OwnerPlayer.IsAtWarWith(item3.GroundUnit.OwnerPlayer))
			{
				num++;
				zero += item3.tileGO.transform.position;
			}
		}
		if (num != 0)
		{
			zero /= (float)num;
		}
		TileGO tileGO = null;
		float num2 = float.MinValue;
		for (byte b = 1; b <= unit.MaxMP; b++)
		{
			foreach (Tile item4 in Utils.GetAllTilesInRadiusAround(this.tileGO.tile, b))
			{
				if (!listOfpotentialTiles.Contains(item4.tileGO))
				{
					continue;
				}
				float num3 = Vector3.Distance(item4.tileGO.transform.position, zero);
				int num4 = 0;
				foreach (Tile item5 in Utils.GetAllTilesInRadiusAround(item4, 1))
				{
					if (item5.GroundUnit != null && unit.OwnerPlayer.IsAtWarWith(item5.GroundUnit.OwnerPlayer))
					{
						num4++;
					}
				}
				float num5 = num3 - (float)num4 * 10f;
				if (num5 > num2)
				{
					num2 = num5;
					tileGO = item4.tileGO;
				}
			}
			if (tileGO != null)
			{
				break;
			}
		}
		AIWaitInstruction item2 = new AIWaitInstruction(0f);
		AI.AIWaitInstructions.Enqueue(item2);
		unit.AddMorale(-15);
		if (!p_attackerUnitGO.tileGO.isInFogOfWar || !this.tileGO.isInFogOfWar)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Damage_Text"));
			gameObject.transform.SetParent(this.tileGO.transform);
			gameObject.GetComponent<TextMeshPro>().text = LocalizationManager.Translate("action-retreat");
			Vector3 position = new Vector3(this.tileGO.transform.position.x, this.tileGO.transform.position.y + 5f, -20f);
			gameObject.transform.position = position;
			gameObject.transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, Vector3.one, 0.5f).setEaseOutElastic();
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
		isRetreating = true;
		if (IsHumanPlayerAttacking(p_attackerUnitGO))
		{
			SteamManager.UnlockAchievement("Retreat");
		}
		p_attackerUnitGO.canOverrun = true;
		MapGO.ClearTargetedUnits();
		MoveUnitToTile(tileGO);
		if (!TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			unit.AddRemoveActionPoints(1);
			unit.HasAttacked = false;
			unit.HasMoved = false;
			unit.CurrMP = unit.MaxMP;
			unit.NumberOfTilesMovedDuringCurrentTurn = 0;
		}
	}

	private static bool IsHumanPlayerAttacking(UnitGO p_attackerUnitGO)
	{
		Player ownerPlayer = p_attackerUnitGO.unit.OwnerPlayer;
		if (ownerPlayer == null || ownerPlayer.IsComputer)
		{
			return false;
		}
		return ownerPlayer == TurnManager.currPlayer;
	}

	private void Surrender(UnitGO p_toUnitGO)
	{
		if (!tileGO.isInFogOfWar && CameraGO.instance.IsWithinFOV(transform.position))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Surrender"));
			gameObject.transform.SetParent(tileGO.transform);
			Vector3 position = new Vector3(tileGO.transform.position.x + 2f, tileGO.transform.position.y + 2f, -5f);
			gameObject.transform.position = position;
			LeanTween.rotateZ(gameObject, 40f, 0.55f).setLoopPingPong().setFrom(0f)
				.setRepeat(-1);
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
		_surrendered = true;
		if (!unit.NoCapture)
		{
			_captured = true;
			unit.XP = 0;
			unit.Lvl = 1;
			unit.IsPoliticalUnit = false;
			unit.CurrHP = 0;
			unit.CategoryID = 0;
			unit.CustomName = null;
			unit.EntrenchmentLevel = 0;
			unit.TurnToProduce = 1;
			if (string.IsNullOrEmpty(tileGO.tile.CityName))
			{
				unit.AddToHistory(LocalizationManager.Translate("history-surrendered"), p_sendRPC: false);
				if (MapGO.selectedUnitGO != null)
				{
					MapGO.selectedUnitGO.unit.AddToHistory(LocalizationManager.Translate("history-captured") + unit.Name, p_sendRPC: false);
				}
			}
			else
			{
				unit.AddToHistory(LocalizationManager.Translate("history-surrendered") + " (" + tileGO.tile.CityName + ").", p_sendRPC: false);
				if (MapGO.selectedUnitGO != null)
				{
					MapGO.selectedUnitGO.unit.AddToHistory(LocalizationManager.Translate("history-captured") + unit.Name + " (" + tileGO.tile.CityName + ").", p_sendRPC: false);
				}
			}
		}
		CheckAchievements();
		DestroyUnit(p_playAnimation: false, p_sendRPC: true);
		p_toUnitGO.unit.OwnerPlayer.TotalUnitsDestroyed++;
		p_toUnitGO.unit.OwnerPlayer.TurnUnitsDestroyed++;
	}

	public void RotateAttackedUnit(UnitGO p_unitGOtoRotate, UnitGO p_refUnitGO)
	{
		if (UIManager.isNATO || p_unitGOtoRotate == null || p_refUnitGO == null)
		{
			return;
		}
		if (p_unitGOtoRotate.unit.AllowedRotation == AllowedRotations.LEFT_RIGHT)
		{
			if (p_unitGOtoRotate.transform.position.x > p_refUnitGO.transform.position.x)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.WEST;
			}
			else if (p_unitGOtoRotate.transform.position.x < p_refUnitGO.transform.position.x)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.EAST;
			}
			p_unitGOtoRotate.SetUnitRotation();
		}
		else
		{
			if (p_unitGOtoRotate.unit.AllowedRotation != AllowedRotations.ALL)
			{
				return;
			}
			if (p_unitGOtoRotate.transform.position.x > p_refUnitGO.transform.position.x)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.WEST;
			}
			else if (p_unitGOtoRotate.transform.position.x < p_refUnitGO.transform.position.x)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.EAST;
			}
			else if (p_unitGOtoRotate.transform.position.y < p_refUnitGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH;
			}
			else if (p_unitGOtoRotate.transform.position.y > p_refUnitGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH;
			}
			if (p_refUnitGO.tileGO.transform.position.x == p_unitGOtoRotate.tileGO.transform.position.x)
			{
				if (p_refUnitGO.tileGO.transform.position.y < p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH;
				}
				else if (p_refUnitGO.tileGO.transform.position.y > p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH;
				}
			}
			else if (p_unitGOtoRotate.tileGO.tile.PosX % 2 == 0)
			{
				if (p_refUnitGO.tileGO.transform.position.x > p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y < p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH_EAST;
				}
				else if (p_refUnitGO.tileGO.transform.position.x < p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y < p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH_WEST;
				}
				else if (p_refUnitGO.tileGO.transform.position.x < p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y > p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH_WEST;
				}
				else if (p_refUnitGO.tileGO.transform.position.x > p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y > p_unitGOtoRotate.tileGO.transform.position.y)
				{
					p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH_EAST;
				}
			}
			else if (p_refUnitGO.tileGO.transform.position.x > p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y < p_unitGOtoRotate.tileGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH_EAST;
			}
			else if (p_refUnitGO.tileGO.transform.position.x < p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y < p_unitGOtoRotate.tileGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.SOUTH_WEST;
			}
			else if (p_refUnitGO.tileGO.transform.position.x < p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y > p_unitGOtoRotate.tileGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH_WEST;
			}
			else if (p_refUnitGO.tileGO.transform.position.x > p_unitGOtoRotate.tileGO.transform.position.x && p_refUnitGO.tileGO.transform.position.y > p_unitGOtoRotate.tileGO.transform.position.y)
			{
				p_unitGOtoRotate.unit.FacingRotation = FacingRotations.NORTH_EAST;
			}
			p_unitGOtoRotate.SetUnitRotation();
		}
	}

	public void RotateUnit(TileGO p_departureTileGO, TileGO p_arrivalTileGO)
	{
		if (UIManager.isNATO)
		{
			return;
		}
		Vector3 position = p_arrivalTileGO.transform.position;
		Vector3 position2 = p_departureTileGO.transform.position;
		if (unit.AllowedRotation == AllowedRotations.LEFT_RIGHT)
		{
			if (position.x > position2.x)
			{
				unit.FacingRotation = FacingRotations.EAST;
			}
			else if (position.x < position2.x)
			{
				unit.FacingRotation = FacingRotations.WEST;
			}
			SetUnitRotation();
		}
		else
		{
			if (unit.AllowedRotation != AllowedRotations.ALL)
			{
				return;
			}
			if (position.x == position2.x)
			{
				if (position.y < position2.y)
				{
					unit.FacingRotation = FacingRotations.SOUTH;
				}
				else if (position.y > position2.y)
				{
					unit.FacingRotation = FacingRotations.NORTH;
				}
			}
			else if (tileGO.tile.PosX % 2 == 0)
			{
				if (position.x > position2.x && position.y < position2.y)
				{
					unit.FacingRotation = FacingRotations.SOUTH_EAST;
				}
				else if (position.x < position2.x && position.y < position2.y)
				{
					unit.FacingRotation = FacingRotations.SOUTH_WEST;
				}
				else if (position.x < position2.x && position.y > position2.y)
				{
					unit.FacingRotation = FacingRotations.NORTH_WEST;
				}
				else if (position.x > position2.x && position.y > position2.y)
				{
					unit.FacingRotation = FacingRotations.NORTH_EAST;
				}
			}
			else if (position.x > position2.x && position.y < position2.y)
			{
				unit.FacingRotation = FacingRotations.SOUTH_EAST;
			}
			else if (position.x < position2.x && position.y < position2.y)
			{
				unit.FacingRotation = FacingRotations.SOUTH_WEST;
			}
			else if (position.x < position2.x && position.y > position2.y)
			{
				unit.FacingRotation = FacingRotations.NORTH_WEST;
			}
			else if (position.x > position2.x && position.y > position2.y)
			{
				unit.FacingRotation = FacingRotations.NORTH_EAST;
			}
			SetUnitRotation();
		}
	}

	private void ShowRangedRadius()
	{
		if ((unit.Range <= 1 && unit.FilterType != "FilterArtillery" && unit.FilterType != "FilterBoat") || (!unit.IsCommander && unit.IsOnBoat) || unit.IsMine || MapGO.instance.commandersParent.childCount > 0)
		{
			return;
		}
		int num = unit.Range;
		if (unit.IsCommander && unit.OwnerPlayer.HasPolicy(Policies.Type.Improved_HQ))
		{
			num += (int)Math.Round((float)num * 0.3f, MidpointRounding.ToEven);
		}
		else if (unit.IsRADAR && unit.MaxMP == 0 && unit.OwnerPlayer.HasPolicy(Policies.Type.Improved_radars))
		{
			num += (int)Math.Round((float)num * 0.5f, MidpointRounding.ToEven);
		}
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, num))
		{
			GameObject gameObject = null;
			if (unit.IsCommander || (unit.IsRADAR && unit.MaxMP == 0))
			{
				gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Commander tile"), MapGO.instance.commandersParent);
			}
			else if (item != tileGO.tile)
			{
				gameObject = ((!PlayerSettings.Instance.ColorBlindMode) ? UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Ranged tile"), MapGO.instance.commandersParent) : UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Ranged tile colorblind"), MapGO.instance.commandersParent));
			}
			if (gameObject != null)
			{
				gameObject.transform.position = new Vector3(item.tileGO.gameObject.transform.position.x, item.tileGO.transform.position.y, -0.4f);
			}
		}
	}

	private void OnMouseOver()
	{
		if (UIManager.isUIOpen || tileGO.isInFogOfWar || (unit.IsMine && unitSprite.color.a == 0f))
		{
			return;
		}
		UnitGO p_unitGO = this;
		if (this == MapGO.selectedUnitGO && unit.Type == "Plane" && tileGO.groundUnitGO != null && (!tileGO.groundUnitGO.unit.IsMine || unit.OwnerPlayer.IsAlliedWith(tileGO.groundUnitGO.unit.OwnerPlayer)))
		{
			p_unitGO = tileGO.groundUnitGO;
		}
		if (Utils.IsPointerOverUIObject())
		{
			if (Time.timeScale == 0f)
			{
				UIManager.instance.hoverUnitUI.Show(p_unitGO);
				ShowRangedRadius();
			}
			else
			{
				UIManager.instance.hoverUnitUI.Close();
			}
			UIManager.instance.potentialDamagePanel.SetActive(value: false);
			return;
		}
		ShowRangedRadius();
		UIManager.instance.hoverUnitUI.Show(p_unitGO);
		if (!TurnManager.currPlayer.IsComputer && !DrawingManager.instance.IsErasingOrDrawing())
		{
			if (Input.GetKeyDown(PlayerSettings.Instance.PutUnitToSleep) && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
			{
				if (unit.AsleepState == AsleepStates.NONE)
				{
					unit.AsleepState = AsleepStates.SKIP;
				}
				else if (unit.AsleepState == AsleepStates.SKIP)
				{
					unit.AsleepState = AsleepStates.SLEEP;
				}
				else if (unit.AsleepState == AsleepStates.SLEEP)
				{
					unit.AsleepState = AsleepStates.NONE;
				}
			}
			if (Input.GetKeyUp(PlayerSettings.Instance.AttackUnit) && !Input.GetKey(PlayerSettings.Instance.Waypoints) && (!PlayerSettings.Instance.MoveCameraWithLeftClick || (PlayerSettings.Instance.MoveCameraWithLeftClick && UIManager.LeftClickTime < 0.2f)))
			{
				if (TurnManager.currPlayer.IsAlliedWith(unit.OwnerPlayer))
				{
					if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO.unit.IsSupplyShip && tileGO.groundUnitGO != null && MapGO.selectedUnitGO != this)
					{
						MapGO.selectedUnitGO.ResupplyAlly(tileGO.groundUnitGO, p_sendRPC: true);
					}
					if (MapGO.selectedUnitGO != null && (MapGO.selectedUnitGO.unit.Type == "Plane" || MapGO.selectedUnitGO.unit.FilterType == "FilterAA") && MapGO.listOfTargetedUnit.Contains(tileGO.gameObject) && Input.GetKeyUp(PlayerSettings.Instance.AttackUnit) && (!PlayerSettings.Instance.MoveCameraWithLeftClick || (PlayerSettings.Instance.MoveCameraWithLeftClick && UIManager.LeftClickTime < 0.2f)))
					{
						if (MapGO.selectedUnitGO.unit.Type == "Plane")
						{
							MapGO.selectedUnitGO.targetedUnitGO = MapGO.selectedUnitGO.tileGO.groundUnitGO;
						}
						else
						{
							MapGO.selectedUnitGO.targetedUnitGO = MapGO.selectedUnitGO.tileGO.airUnitGO;
						}
						MapGO.selectedUnitGO.AttackUnit(MapGO.selectedUnitGO.targetedUnitGO, p_isRetaliationPossible: true, p_isSupportPossible: true, p_isAmbush: false);
						if (MapGO.selectedUnitGO != null)
						{
							MapGO.selectedUnitGO.AttackTile(tileGO);
						}
						return;
					}
				}
				else if (MapGO.selectedUnitGO != null && MapGO.listOfTargetedUnit.Contains(tileGO.gameObject))
				{
					MainMenu.stopwatchAttackMovement.Restart();
					MapGO.selectedUnitGO.targetedUnitGO = this;
					MapGO.selectedUnitGO.AttackUnit(this, p_isRetaliationPossible: true, p_isSupportPossible: true, p_isAmbush: false);
					if (!(MapGO.selectedUnitGO == null))
					{
						if (MapGO.selectedUnitGO.targetedUnitGO != null)
						{
							MapGO.selectedUnitGO.AttackTile(tileGO);
						}
						else
						{
							MapGO.selectedUnitGO.AttackTile(tileGO);
						}
					}
					return;
				}
			}
			if (Input.GetMouseButtonUp(0) && (!PlayerSettings.Instance.MoveCameraWithLeftClick || (PlayerSettings.Instance.MoveCameraWithLeftClick && UIManager.LeftClickTime < 0.2f)))
			{
				if (Utils.IsPointerOverUIObject())
				{
					return;
				}
				if (TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
				{
					if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO.gameObject == gameObject && !MapGO.listOfTargetedUnit.Contains(tileGO.gameObject))
					{
						MapGO.instance.UnselectUnit();
						return;
					}
					if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO != this)
					{
						if (Input.GetKey(KeyCode.LeftShift) && CanSwapUnits(MapGO.selectedUnitGO))
						{
							SwapUnits(MapGO.selectedUnitGO);
							return;
						}
						if (MapGO.selectedUnitGO.unit.IsSupplyShip && tileGO.groundUnitGO != null && MapGO.selectedUnitGO != tileGO.groundUnitGO)
						{
							MapGO.selectedUnitGO.ResupplyAlly(tileGO.groundUnitGO, p_sendRPC: true);
							if (!MapGO.selectedUnitGO.unit.HasAttacked && MapGO.selectedUnitGO.unit.ActionPoints > 0 && MapGO.selectedUnitGO.tileGO.tile.neighbours.Contains(tileGO.tile))
							{
								return;
							}
						}
						if (MapGO.selectedUnitGO.unit.Type == unit.Type || MapGO.selectedUnitGO.unit.CurrMP == 0)
						{
							MapGO.selectedUnitGO.canUndo = false;
							SelectUnit(p_playAnimation: true);
						}
						if (!MapGO.selectedUnitGO.listOfpotentialTiles.Contains(tileGO))
						{
							MapGO.selectedUnitGO.canUndo = false;
							SelectUnit(p_playAnimation: true);
						}
					}
					else
					{
						SelectUnit(p_playAnimation: true);
					}
				}
				else if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO != this && Input.GetKey(KeyCode.LeftShift) && CanSwapUnits(MapGO.selectedUnitGO))
				{
					SwapUnits(MapGO.selectedUnitGO);
					return;
				}
			}
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.ShowUnitHistory) && !DrawingManager.instance.IsErasingOrDrawing())
		{
			OpenStatsMenu();
		}
		ShowPotentialDamage();
	}

	public bool CanSwapUnits(UnitGO p_swapWithUnitGO)
	{
		if (p_swapWithUnitGO == null)
		{
			return false;
		}
		if (p_swapWithUnitGO == this)
		{
			return false;
		}
		if (!TurnManager.currPlayer.CanControlUnitsOf(p_swapWithUnitGO.unit.OwnerPlayer) || !TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			if (TurnManager.currPlayer.CanControlUnitsOf(p_swapWithUnitGO.unit.OwnerPlayer))
			{
				if (p_swapWithUnitGO.unit.HasMoved || p_swapWithUnitGO.unit.ActionPoints <= 0)
				{
					return false;
				}
				if (p_swapWithUnitGO.unit.CurrMP - tileGO.tile.MPCost < 0)
				{
					return false;
				}
			}
			else
			{
				if (unit.HasMoved || unit.ActionPoints <= 0)
				{
					return false;
				}
				if (unit.CurrMP - p_swapWithUnitGO.tileGO.tile.MPCost < 0)
				{
					return false;
				}
			}
		}
		else
		{
			if (p_swapWithUnitGO.unit.HasMoved || unit.HasMoved || p_swapWithUnitGO.unit.ActionPoints <= 0 || unit.ActionPoints <= 0)
			{
				return false;
			}
			if (p_swapWithUnitGO.unit.CurrMP - tileGO.tile.MPCost < 0 || unit.CurrMP - p_swapWithUnitGO.tileGO.tile.MPCost < 0)
			{
				return false;
			}
		}
		if (!tileGO.tile.neighbours.Contains(p_swapWithUnitGO.tileGO.tile) || !p_swapWithUnitGO.tileGO.tile.neighbours.Contains(tileGO.tile))
		{
			return false;
		}
		if ((p_swapWithUnitGO.unit.Type == "Plane" && unit.Type != "Plane") || (p_swapWithUnitGO.unit.Type != "Plane" && unit.Type == "Plane"))
		{
			return false;
		}
		if ((p_swapWithUnitGO.unit.Type == "Boat" && unit.Type != "Boat") || (p_swapWithUnitGO.unit.Type != "Boat" && unit.Type == "Boat"))
		{
			return false;
		}
		if (p_swapWithUnitGO.unit.MaxMP <= 0 || unit.MaxMP <= 0)
		{
			return false;
		}
		if ((p_swapWithUnitGO.unit.IsArmouredTrain && !unit.IsArmouredTrain) || (!p_swapWithUnitGO.unit.IsArmouredTrain && unit.IsArmouredTrain))
		{
			return false;
		}
		if ((p_swapWithUnitGO.unit.IsTrain && !unit.IsTrain) || (!p_swapWithUnitGO.unit.IsTrain && unit.IsTrain))
		{
			if (p_swapWithUnitGO.unit.IsTrain && !tileGO.tile.Railroad && tileGO.tile.Type != "City" && tileGO.tile.Type != "Harbour" && tileGO.tile.Type != "Factory" && tileGO.tile.Type != "Refinery")
			{
				return false;
			}
			if (unit.IsTrain && !p_swapWithUnitGO.tileGO.tile.Railroad && p_swapWithUnitGO.tileGO.tile.Type != "City" && p_swapWithUnitGO.tileGO.tile.Type != "Harbour" && p_swapWithUnitGO.tileGO.tile.Type != "Factory" && p_swapWithUnitGO.tileGO.tile.Type != "Refinery")
			{
				return false;
			}
		}
		if (p_swapWithUnitGO.unit.Type != "Plane" && ((p_swapWithUnitGO.tileGO.tile.Type == "River" && tileGO.tile.Type != "River") || (p_swapWithUnitGO.tileGO.tile.Type != "River" && tileGO.tile.Type == "River")))
		{
			return false;
		}
		if ((p_swapWithUnitGO.tileGO.tile.Type == "Mountain" && (!unit.IsMountaineer || unit.IsMotorized || unit.IsMechanized)) || ((!p_swapWithUnitGO.unit.IsMountaineer || p_swapWithUnitGO.unit.IsMechanized || p_swapWithUnitGO.unit.IsMotorized) && tileGO.tile.Type == "Mountain"))
		{
			return false;
		}
		if (!p_swapWithUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
		{
			return false;
		}
		return true;
	}

	public void SwapUnits(UnitGO p_swapWithUnitGO)
	{
		StopAttackMotionAndSnapToTile();
		p_swapWithUnitGO.StopAttackMotionAndSnapToTile();
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar || !p_swapWithUnitGO.tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 0.8f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!this.unit.OwnerPlayer.isAlliedWithHumanPlayer && this.unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 0.5f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		string text = p_swapWithUnitGO.unit.MovementSound;
		if (p_swapWithUnitGO.unit.IsTrain)
		{
			text = "Train";
		}
		else if (p_swapWithUnitGO.unit.IsDropable)
		{
			text = "Heavy plane";
		}
		else if (p_swapWithUnitGO.unit.IsOnBoat)
		{
			text = "Small boat";
		}
		else if (p_swapWithUnitGO.unit.IsHorsed)
		{
			text = "Horse";
		}
		else if (p_swapWithUnitGO.unit.IsMotorized)
		{
			text = "Motorized";
		}
		else if (p_swapWithUnitGO.unit.IsMechanized)
		{
			text = "Mechanized";
		}
		if (!p_swapWithUnitGO.tileGO.isInFogOfWar || !tileGO.isInFogOfWar)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(text));
		}
		MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, text, "Movement");
		if (p_swapWithUnitGO.unit.Type == "Plane" || p_swapWithUnitGO.unit.IsDropable || p_swapWithUnitGO.unit.IsOnBoat || p_swapWithUnitGO.unit.Type == "Boat")
		{
			p_swapWithUnitGO.unit.CurrAutonomy--;
			p_swapWithUnitGO.unit.CurrMP--;
			this.unit.CurrAutonomy--;
			this.unit.CurrMP--;
		}
		else
		{
			Unit unit = p_swapWithUnitGO.unit;
			unit.CurrAutonomy -= tileGO.tile.MPCost;
			p_swapWithUnitGO.unit.CurrMP -= tileGO.tile.MPCost;
			Unit unit2 = this.unit;
			unit2.CurrAutonomy -= p_swapWithUnitGO.tileGO.tile.MPCost;
			this.unit.CurrMP -= p_swapWithUnitGO.tileGO.tile.MPCost;
		}
		byte entrenchmentLevel = this.unit.EntrenchmentLevel;
		this.unit.EntrenchmentLevel = p_swapWithUnitGO.unit.EntrenchmentLevel;
		p_swapWithUnitGO.unit.EntrenchmentLevel = entrenchmentLevel;
		if (p_swapWithUnitGO.unit.CurrAutonomy < 0 || p_swapWithUnitGO.unit.CurrAutonomy > p_swapWithUnitGO.unit.MaxAutonomy)
		{
			p_swapWithUnitGO.unit.CurrAutonomy = 0;
		}
		if (p_swapWithUnitGO.unit.CurrMP < 0 || p_swapWithUnitGO.unit.CurrMP > p_swapWithUnitGO.unit.MaxMP)
		{
			p_swapWithUnitGO.unit.CurrMP = 0;
		}
		if (this.unit.CurrAutonomy < 0 || this.unit.CurrAutonomy > this.unit.MaxAutonomy)
		{
			this.unit.CurrAutonomy = 0;
		}
		if (this.unit.CurrMP < 0 || this.unit.CurrMP > this.unit.MaxMP)
		{
			this.unit.CurrMP = 0;
		}
		MultiplayerManager.Instance.RunRPC("RPC_SwapUnits", "O", p_swapWithUnitGO.unit.ID, p_swapWithUnitGO.unit.OwnerName, p_swapWithUnitGO.tileGO.tile.PosX, p_swapWithUnitGO.tileGO.tile.PosY, this.unit.ID, this.unit.OwnerName, tileGO.tile.PosX, tileGO.tile.PosY);
		TileGO p_tileGO = p_swapWithUnitGO.tileGO;
		p_swapWithUnitGO.AssignUnitToTile(tileGO);
		p_swapWithUnitGO.tileGO = tileGO;
		AssignUnitToTile(p_tileGO);
		tileGO = p_tileGO;
		Vector3 vector = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, transform.position.z);
		Vector3 vector2 = new Vector3(p_swapWithUnitGO.tileGO.transform.position.x, p_swapWithUnitGO.tileGO.transform.position.y, p_swapWithUnitGO.transform.position.z);
		if (PlayerSettings.Instance.IsQuickMovement || (PlayerSettings.Instance.FastAI && TurnManager.currPlayer.IsComputer) || (PlayerSettings.Instance.FastAllies && TurnManager.currPlayer.isAlliedWithHumanPlayer))
		{
			transform.position = vector;
			p_swapWithUnitGO.transform.position = vector2;
		}
		else
		{
			_swapLerpCoroutine = StartCoroutine(CR_SwapLerp(this, transform.position, vector, 0.25f));
			p_swapWithUnitGO._swapLerpCoroutine = StartCoroutine(CR_SwapLerp(p_swapWithUnitGO, p_swapWithUnitGO.transform.position, vector2, 0.25f));
		}
		p_swapWithUnitGO.unit.HasMoved = true;
		p_swapWithUnitGO.unit.IsDefensive = false;
		p_swapWithUnitGO.unit.NumberOfTimesTheUnitMoved++;
		if (p_swapWithUnitGO.unit.MoveOrAttack)
		{
			p_swapWithUnitGO.unit.HasAttacked = true;
		}
		this.unit.HasMoved = true;
		this.unit.IsDefensive = false;
		this.unit.NumberOfTimesTheUnitMoved++;
		if (this.unit.MoveOrAttack)
		{
			this.unit.HasAttacked = true;
		}
		p_swapWithUnitGO.unit.AsleepState = AsleepStates.NONE;
		this.unit.AsleepState = AsleepStates.NONE;
		p_swapWithUnitGO.SelectUnit(p_playAnimation: false);
		MapGO.instance.selectedUnitIcon.transform.position = new Vector3(p_swapWithUnitGO.tileGO.transform.position.x, p_swapWithUnitGO.tileGO.transform.position.y, -6f);
		SteamManager.UnlockAchievement("Unit swap");
		p_swapWithUnitGO.canUndo = false;
		p_swapWithUnitGO.unit.AddRemoveActionPoints(-1);
		p_swapWithUnitGO.unit.IsDefensive = false;
		this.unit.AddRemoveActionPoints(-1);
		canUndo = false;
		this.unit.IsDefensive = false;
		MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", this.unit, false);
		MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", p_swapWithUnitGO.unit, false);
	}

	private IEnumerator CR_SwapLerp(UnitGO p_unitGO, Vector3 p_from, Vector3 p_to, float p_duration)
	{
		float elapsed = 0f;
		while (elapsed < p_duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / p_duration);
			float t = num * num * (3f - 2f * num);
			p_unitGO.transform.position = Vector3.Lerp(p_from, p_to, t);
			yield return null;
		}
		p_unitGO.transform.position = p_to;
		p_unitGO._swapLerpCoroutine = null;
	}

	private void OnMouseExit()
	{
		MapGO.instance.ClearCommanderTiles();
		UIManager.instance.HideTooltip();
		UIManager.instance.hoverUnitUI.Close();
		UIManager.instance.potentialDamagePanel.SetActive(value: false);
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public void SelectUnit(bool p_playAnimation)
	{
		if (Time.timeScale == 0f || ReservePanelGO.selectedUnit != null || unit.IsDead || unit.CurrHP <= 0 || (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO.isMoving) || (MultiplayerManager.IsRealtimeMultiplayer() && !unit.OwnerPlayer.IsLocalClientPlayer()))
		{
			return;
		}
		UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.RemoveFogOfWarAroundUnit();
		}
		MapGO.ClearTargetedUnits();
		MapGO.selectedUnitGO = this;
		UIManager.instance.hoverUnitUI.Close();
		if (unit.Type == "Plane")
		{
			UIManager.showAirUnit = true;
		}
		else
		{
			UIManager.showAirUnit = false;
		}
		UIManager.instance.ShowHideAirGroundUnit(doesSound: false);
		if (!unit.OwnerPlayer.IsComputer)
		{
			OrderOfBattleGO.instance.ShowSelectedUnitInOOB();
			UIManager.instance.ShowWaypoints(unit);
			ShowPotentialPaths();
			ShowSelectedUnitStats();
			MapGO.instance.selectedUnitIcon.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -6f);
			if (PlayerSettings.Instance.MoreContrastSelectedUnit || tileGO.tile.Season == "Winter")
			{
				MapGO.instance.selectedUnitIcon.GetComponent<SpriteRenderer>().color = new Color(0.1367881f, 75f / 106f, 0.05673729f);
			}
			else
			{
				MapGO.instance.selectedUnitIcon.GetComponent<SpriteRenderer>().color = new Color(0.5960785f, 154f / 255f, 0.6235294f);
			}
			MapGO.instance.selectedUnitIcon.SetActive(value: true);
			if (p_playAnimation)
			{
				MapGO.instance.selectedUnitIcon.transform.localScale = MapGO.instance.selectedUnitIconOriginalScaleBIG;
				LeanTween.cancel(MapGO.instance.selectedUnitIcon);
				LeanTween.scale(MapGO.instance.selectedUnitIcon, MapGO.instance.selectedUnitIconOriginalScale, 0.1f).setEaseOutQuint();
				Color color = MapGO.instance.selectedUnitIcon.GetComponent<SpriteRenderer>().color;
				color.a = 1f;
				LeanTween.color(MapGO.instance.selectedUnitIcon, color, 0.1f).setEaseInBack();
			}
			else
			{
				MapGO.instance.selectedUnitIcon.transform.localScale = MapGO.instance.selectedUnitIconOriginalScale;
			}
			TutorialManager.PerformAction(TutorialActions.SELECT_UNIT);
		}
	}

	public void ShowSelectedUnitStats()
	{
		if (!unit.OwnerPlayer.IsComputer && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer) && (!MultiplayerManager.IsRealtimeMultiplayer() || TurnManager.currPlayer.IsComputer || unit.OwnerPlayer.IsLocalClientPlayer()))
		{
			MapGO.instance.leftSideUI_Panel.SetActive(value: true);
			MapGO.instance.panelAllStatsSelectedUnit.SetActive(value: true);
			UIManager.instance.selectedUnitUI.Show(this);
			UIManager.instance.SelectedUnitExtraStatsUI.Show(this);
		}
	}

	public void UpdateSelectedUnitStats()
	{
		if (!unit.OwnerPlayer.IsComputer && TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer) && (!MultiplayerManager.IsRealtimeMultiplayer() || TurnManager.currPlayer.IsComputer || unit.OwnerPlayer.IsLocalClientPlayer()))
		{
			UIManager.instance.selectedUnitUI.RefreshIfIsSelectedUnit(unit);
			UIManager.instance.SelectedUnitExtraStatsUI.RefreshIfIsSelectedUnit(unit);
		}
	}

	public void DiveUndiveSubmarine(bool p_sendRPC)
	{
		if (unit.IsSubmarine && unit.ActionPoints > 0 && (!((float)unit.CurrHP / (float)unit.MaxHP <= 0.15f) || unit.IsSubmerged) && (unit.NumberOfTurnsUnderWater < 3 || unit.IsSubmerged))
		{
			string text = "";
			text = ((!unit.IsSubmerged) ? "Surface" : "Dive");
			if (!tileGO.isInFogOfWar)
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound(text));
			}
			if (p_sendRPC)
			{
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, text, "Movement");
			}
			unit.IsSubmerged = !unit.IsSubmerged;
			SelectUnit(p_playAnimation: false);
			if (p_sendRPC)
			{
				MultiplayerManager.Instance.RunRPC("RPC_EmbarkDisembark", "O", unit.ID, unit.OwnerName, "Submarine");
			}
			TutorialManager.PerformAction(TutorialActions.SUBMARINE_DIVE);
		}
	}

	public void ResupplyAlly(UnitGO p_supplyTargetUnitGO, bool p_sendRPC)
	{
		if (unit.HasAttacked || unit.ActionPoints <= 0 || p_supplyTargetUnitGO.unit.IsSubmerged || !Utils.GetAllTilesInRadiusAround(tileGO.tile, 1).Contains(p_supplyTargetUnitGO.tileGO.tile))
		{
			return;
		}
		MainMenu.stopwatchAttackMovement.Restart();
		bool flag = p_supplyTargetUnitGO.unit.CurrAmmo < p_supplyTargetUnitGO.unit.MaxAmmo;
		bool flag2 = p_supplyTargetUnitGO.unit.CurrAutonomy < p_supplyTargetUnitGO.unit.MaxAutonomy;
		if (flag && unit.CurrAmmo > 0)
		{
			int num = (p_supplyTargetUnitGO.unit.MaxAmmo - p_supplyTargetUnitGO.unit.CurrAmmo) / 2;
			if (unit.CurrAmmo > 1)
			{
				unit.CurrAmmo--;
			}
			p_supplyTargetUnitGO.unit.CurrAmmo = (short)Mathf.Clamp(p_supplyTargetUnitGO.unit.CurrAmmo + num, 0, p_supplyTargetUnitGO.unit.MaxAmmo);
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply ammo"));
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Ammo loss Text"), p_supplyTargetUnitGO.tileGO.transform);
			gameObject.GetComponentInChildren<TextMeshPro>().text = "+" + num;
			gameObject.transform.position = new Vector3(p_supplyTargetUnitGO.transform.position.x + 3f, p_supplyTargetUnitGO.transform.position.y, -5f);
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
		if (flag2 && unit.CurrAutonomy > 0)
		{
			int num2 = (p_supplyTargetUnitGO.unit.MaxAutonomy - p_supplyTargetUnitGO.unit.CurrAutonomy) / 2;
			if (unit.CurrAutonomy > 1)
			{
				unit.CurrAutonomy--;
			}
			p_supplyTargetUnitGO.unit.CurrAutonomy = (short)Mathf.Clamp(p_supplyTargetUnitGO.unit.CurrAutonomy + num2, 0, p_supplyTargetUnitGO.unit.MaxAutonomy);
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetUISound("Supply fuel"));
			GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Fuel loss Text"), p_supplyTargetUnitGO.tileGO.transform);
			gameObject2.GetComponentInChildren<TextMeshPro>().text = "+" + num2;
			gameObject2.transform.position = new Vector3(p_supplyTargetUnitGO.transform.position.x + 3f, p_supplyTargetUnitGO.transform.position.y + 2f, -5f);
			UnityEngine.Object.Destroy(gameObject2, 2f);
		}
		canUndo = false;
		unit.HasAttacked = true;
		unit.AddRemoveActionPoints(-1);
		UpdateCounter();
		p_supplyTargetUnitGO.UpdateCounter();
		ShowSelectedUnitStats();
		MapGO.ClearTargetedUnits();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", unit, false);
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", p_supplyTargetUnitGO.unit, false);
		}
	}

	private void ShowPotentialDamage()
	{
		if (Utils.IsPointerOverUIObject())
		{
			UIManager.instance.potentialDamagePanel.SetActive(value: false);
		}
		else
		{
			if (MapGO.selectedUnitGO == null || TurnManager.currPlayer.IsComputer || tileGO.isInFogOfWar || MapGO.selectedUnitGO.unit.HasAttacked || MapGO.selectedUnitGO.unit.ActionPoints <= 0)
			{
				return;
			}
			UnitGO unitGO = ((!(MapGO.selectedUnitGO == this) || !(MapGO.selectedUnitGO.unit.Type == "Plane") || !(MapGO.selectedUnitGO.tileGO.groundUnitGO != null) || !MapGO.selectedUnitGO.unit.OwnerPlayer.IsAtWarWith(MapGO.selectedUnitGO.tileGO.groundUnitGO.unit.OwnerPlayer)) ? this : MapGO.selectedUnitGO.tileGO.groundUnitGO);
			if (!MapGO.selectedUnitGO.unit.OwnerPlayer.IsAtWarWith(unitGO.unit.OwnerPlayer) || unitGO.unit.IsMine)
			{
				return;
			}
			if (TurnManager.isBadWeather)
			{
				if (MapGO.selectedUnitGO.unit.Type == "Plane")
				{
					if (!MapGO.selectedUnitGO.unit.IsMissile && (MapGO.selectedUnitGO.unit.OwnerPlayer == null || !MapGO.selectedUnitGO.unit.OwnerPlayer.HasPolicy(Policies.Type.All_weather)))
					{
						return;
					}
				}
				else if (MapGO.selectedUnitGO.unit.FilterType == "FilterAA" && unitGO.unit.Type == "Plane")
				{
					return;
				}
			}
			if ((unitGO.unit.Type == "Boat" && MapGO.selectedUnitGO.unit.FilterType != "FilterBoat" && !MapGO.selectedUnitGO.unit.CanAttackShips && (!unitGO.unit.IsOnBoat || unitGO.tileGO.tile.Type != "Harbour")) || (unitGO.unit.Type == "Plane" && MapGO.selectedUnitGO.unit.Type != "Plane" && MapGO.selectedUnitGO.unit.Type != "Boat" && MapGO.selectedUnitGO.unit.FilterType != "FilterAA") || (MapGO.selectedUnitGO.unit.FilterType == "FilterBoat" && unitGO.unit.Type == "Plane") || (MapGO.selectedUnitGO.unit.IsSubmarine && unit.Type != "Boat" && !unit.IsOnBoat) || (unitGO.unit.IsSubmerged && !MapGO.selectedUnitGO.unit.IsDestroyer) || (MapGO.selectedUnitGO.unit.IsSubmarine && unitGO.unit.IsSubmarine && GameData.Instance.map.Date.Year < 1950 && (MapGO.selectedUnitGO.unit.IsSubmerged || unitGO.unit.IsSubmerged)) || (MapGO.selectedUnitGO.unit.FilterType == "FilterFighter" && !MapGO.selectedUnitGO.unit.HasBombs && unitGO.unit.IsSubmarine) || ((MapGO.selectedUnitGO.unit.FilterType == "FilterBomber" || MapGO.selectedUnitGO.unit.FilterType == "FilterCAS") && unitGO.unit.Type == "Plane"))
			{
				return;
			}
			MapGO.selectedUnitGO.unit.GetPotentialDamage(unitGO, p_isRetaliation: false, p_isSupportiveFire: false);
			unitGO.unit.GetPotentialDamage(MapGO.selectedUnitGO, p_isRetaliation: true, p_isSupportiveFire: false);
			if (PlayerSettings.Instance.HideBattlePreview)
			{
				return;
			}
			Transform[] componentsInChildren = UIManager.instance.potentialDamagePanel.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Defense Text")
				{
					if (MapGO.selectedUnitGO.unit.FinalDamage > 0)
					{
						transform.GetComponentInChildren<TextMeshProUGUI>().text = "-" + MapGO.selectedUnitGO.unit.FinalDamage;
					}
					else
					{
						transform.GetComponentInChildren<TextMeshProUGUI>().text = MapGO.selectedUnitGO.unit.FinalDamage.ToString();
					}
				}
				else if (transform.name == "Attack Text")
				{
					if (unitGO.unit.FinalDamage > 0)
					{
						transform.GetComponentInChildren<TextMeshProUGUI>().text = "-" + unitGO.unit.FinalDamage;
					}
					else
					{
						transform.GetComponentInChildren<TextMeshProUGUI>().text = unitGO.unit.FinalDamage.ToString();
					}
				}
				else if (transform.name == "Attacker")
				{
					transform.GetComponent<Image>().sprite = Utils.GetFlagSprite(unitGO.unit.OwnerPlayer.Country);
					if (MapGO.selectedUnitGO.unit.FinalDamage >= unitGO.unit.CurrHP)
					{
						transform.Find("Deathblow").gameObject.SetActive(value: true);
					}
					else
					{
						transform.Find("Deathblow").gameObject.SetActive(value: false);
					}
				}
				else if (transform.name == "Defender")
				{
					transform.GetComponent<Image>().sprite = Utils.GetFlagSprite(MapGO.selectedUnitGO.unit.OwnerPlayer.Country);
					if (unitGO.unit.FinalDamage >= MapGO.selectedUnitGO.unit.CurrHP)
					{
						transform.Find("Deathblow").gameObject.SetActive(value: true);
					}
					else
					{
						transform.Find("Deathblow").gameObject.SetActive(value: false);
					}
				}
			}
			UIManager.instance.potentialDamagePanel.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y + (float)Screen.height * 0.05f);
			UIManager.instance.potentialDamagePanel.SetActive(value: true);
			if (attackerTransform == null)
			{
				attackerTransform = UIManager.instance.potentialDamagePanel.transform.Find("Breakdown attacker");
			}
			if (defenserTransform == null)
			{
				defenserTransform = UIManager.instance.potentialDamagePanel.transform.Find("Breakdown defenser");
			}
			if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftControl))
			{
				attackerTransform.gameObject.SetActive(value: false);
				defenserTransform.gameObject.SetActive(value: false);
				return;
			}
			attackerTransform.gameObject.SetActive(value: true);
			defenserTransform.gameObject.SetActive(value: true);
			List<Unit> list = new List<Unit>
			{
				MapGO.selectedUnitGO.unit,
				unitGO.unit
			};
			Color color = new Color(0.88f, 0.05f, 0.05f);
			Color color2 = new Color(0.124f, 0.641f, 0.124f);
			foreach (Unit item in list)
			{
				Transform transform2 = null;
				transform2 = ((item != MapGO.selectedUnitGO.unit) ? defenserTransform : attackerTransform);
				componentsInChildren = transform2.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform3 in componentsInChildren)
				{
					if (transform3.name == "Base damage")
					{
						transform3.gameObject.SetActive(value: true);
						transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.baseDamageBreakdown.ToString();
					}
					if (transform3.name == "Hero")
					{
						if (item.heroBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.heroBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Veterancy")
					{
						if (item.veterancyBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.veterancyBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Recon")
					{
						if (item.reconBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.reconBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "General")
					{
						if (item.generalBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.generalBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Flamethrower")
					{
						if (item.flamethrowerBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.flamethrowerBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Political")
					{
						if (item.politicalUnitsBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.politicalUnitsBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Torpedo")
					{
						if (item.torpedoBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.torpedoBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Sub")
					{
						if (item.subVSlandingCraftBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.subVSlandingCraftBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Hill")
					{
						if (item.hillBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.hillBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "HP")
					{
						if (item.HPbreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.HPbreakdown.ToString();
							if (item.HPbreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.HPbreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Morale")
					{
						if (item.moraleBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.moraleBreakdown.ToString();
							if (item.moraleBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.moraleBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Armour")
					{
						if (item.armourBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.armourBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Submerged")
					{
						if (item.destroyerVSsubmergedSubBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.destroyerVSsubmergedSubBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Entrenchment")
					{
						if (item.entrenchmentBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.entrenchmentBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "River")
					{
						if (item.riverBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.riverBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Terrain")
					{
						if (item.terrainBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.terrainBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.terrainBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.terrainBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Terrain armoured")
					{
						if (item.terrainArmouredBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.terrainArmouredBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.terrainArmouredBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.terrainArmouredBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Mountaineers")
					{
						if (item.mountaineerBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.mountaineerBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Encirclement")
					{
						if (item.encirclementBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.encirclementBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Landing")
					{
						if (item.landingBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.landingBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.landingBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.landingBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Strat bomber")
					{
						if (item.heavyBomberVSshipsWithoutTorpedoBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.heavyBomberVSshipsWithoutTorpedoBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Policies")
					{
						if (item.policyBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.policyBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.policyBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.policyBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Combined arms")
					{
						if (item.combinedArmsBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.combinedArmsBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.combinedArmsBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.combinedArmsBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Biome")
					{
						if (item.biomeBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.biomeBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.biomeBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.biomeBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Repeated attacks")
					{
						if (item.repeatedAttacksBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.repeatedAttacksBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.repeatedAttacksBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.repeatedAttacksBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (transform3.name == "Weather")
					{
						if (item.weatherBreakdown != 0)
						{
							transform3.gameObject.SetActive(value: true);
							if (item.weatherBreakdown > 0)
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.weatherBreakdown;
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
							}
							else
							{
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.weatherBreakdown.ToString();
								transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
							}
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
						}
					}
					if (!(transform3.name == "Others"))
					{
						continue;
					}
					if (item.othersBreakdown != 0)
					{
						transform3.gameObject.SetActive(value: true);
						if (item.othersBreakdown > 0)
						{
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = "+" + item.othersBreakdown;
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color2;
						}
						else
						{
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().text = item.othersBreakdown.ToString();
							transform3.Find("Value").GetComponent<TextMeshProUGUI>().color = color;
						}
					}
					else
					{
						transform3.gameObject.SetActive(value: false);
					}
				}
			}
			TutorialManager.PerformAction(TutorialActions.OPEN_DAMAGE_BREAKDOWN);
		}
	}

	public float ApplyCommandersBonuses()
	{
		float num = 0f;
		foreach (UnitGO item in MapGO.listOfAllGeneralsOnTheMap)
		{
			if (!item.unit.IsCommander || !unit.OwnerPlayer.IsAlliedWith(item.unit.OwnerPlayer) || string.IsNullOrEmpty(item.unit.CommanderTargets) || (!item.unit.CommanderTargets.Contains(unit.FilterType) && (!item.unit.CommanderTargets.Contains("FilterTank") || !(unit.Type == "Tank") || unit.MaxMP <= 0)))
			{
				continue;
			}
			int num2 = item.unit.Range;
			if (item.unit.OwnerPlayer.HasPolicy(Policies.Type.Improved_HQ))
			{
				num2 += (int)Math.Round((float)num2 * 0.3f, MidpointRounding.ToEven);
			}
			float num3 = 0f;
			float hPPercent = item.unit.GetHPPercent(p_forDamagePurposes: false);
			if (unit.DamageDealingTypeForCommander == DamageTypes.SOFT)
			{
				if (item.unit.SoftDamage > 0)
				{
					num3 = (float)item.unit.SoftDamage * hPPercent / 100f;
				}
			}
			else if (unit.DamageDealingTypeForCommander == DamageTypes.HARD)
			{
				if (item.unit.HardDamage > 0)
				{
					num3 = (float)item.unit.HardDamage * hPPercent / 100f;
				}
			}
			else if (unit.DamageDealingTypeForCommander == DamageTypes.AIR && item.unit.AirDamage > 0)
			{
				num3 = (float)item.unit.AirDamage * hPPercent / 100f;
			}
			if (num3 > num && Utils.IsTileInsideRadiusSimplified(tileGO.tile, item.tileGO.tile, num2))
			{
				num = num3;
			}
		}
		return num;
	}

	public float ApplyCombinedArmsBonus(TileGO p_tileOfUnitBeingAttacked)
	{
		if (unit.FilterType != "FilterTank" && unit.FilterType != "FilterInfantry")
		{
			return 0f;
		}
		float num = 0f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = unit.Type == "Tank";
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, 1))
		{
			if (item.tileGO.groundUnitGO == null || item.tileGO.groundUnitGO == this || !item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
			{
				continue;
			}
			if (!flag && item.tileGO.groundUnitGO.unit.FilterType == "FilterArtillery")
			{
				flag = true;
			}
			if (flag2 || item.tileGO.groundUnitGO.unit.IsRecon)
			{
				continue;
			}
			bool flag4 = false;
			foreach (Tile neighbour in item.neighbours)
			{
				if (neighbour.tileGO == p_tileOfUnitBeingAttacked)
				{
					flag4 = true;
					break;
				}
			}
			if (!flag4)
			{
				continue;
			}
			if (flag3)
			{
				if (item.tileGO.groundUnitGO.unit.Type == "Infantry")
				{
					flag2 = true;
				}
			}
			else if (item.tileGO.groundUnitGO.unit.Type == "Tank")
			{
				flag2 = true;
			}
		}
		if (flag)
		{
			num += 0.05f;
		}
		if (flag2)
		{
			num += 0.05f;
		}
		return num;
	}

	public float ApplyReconAttackBonus(TileGO p_targetTileGO)
	{
		bool flag = false;
		bool flag2 = false;
		float num = 0f;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_targetTileGO.tile, 1))
		{
			if (!flag2 && item.tileGO.groundUnitGO != null && item.tileGO.groundUnitGO != this && item.tileGO.groundUnitGO.unit.IsRecon && !item.tileGO.groundUnitGO.unit.IsSubmerged && item.tileGO.groundUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer) && (!item.tileGO.groundUnitGO.unit.IsSubmarine || unit.FilterType == "FilterBoat"))
			{
				flag2 = true;
			}
			if (!flag && !TurnManager.isBadWeather && item.tileGO.airUnitGO != null && item.tileGO.airUnitGO != this && item.tileGO.airUnitGO.unit.IsRecon && item.tileGO.airUnitGO.unit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
			{
				flag = true;
			}
			if (flag2 & flag)
			{
				break;
			}
		}
		if (flag2)
		{
			num += 0.15f;
		}
		if (flag)
		{
			num += 0.15f;
		}
		return num;
	}

	public void EmbarkDisembarkTrain(bool p_sendRPC)
	{
		canUndo = false;
		unit.EmbarkDisembarkTrain();
		SetSprite();
		if (unit.Type == "Infantry")
		{
			gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		ShowSelectedUnitStats();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_EmbarkDisembark", "O", unit.ID, unit.OwnerName, "Train");
		}
		TryCaptureTile(tileGO.tile, p_sendRPC: false);
	}

	public void EmbarkDisembarkPlane()
	{
		if (unit.IsParatrooper && !(tileGO.tile.Type != "Airport"))
		{
			canUndo = false;
			if (unit.IsDropable && tileGO.groundUnitGO == null)
			{
				DropParatrooper(p_sendRPC: false);
			}
			else if (!unit.IsDropable && tileGO.airUnitGO == null)
			{
				LoadParatrooper(p_sendRPC: false);
			}
			ShowSelectedUnitStats();
			RemoveFogOfWarAroundUnit();
			ShowPotentialPaths();
			UpdateCounterValues();
		}
	}

	public void EmbarkDisembarkBoat(bool p_sendRPC)
	{
		canUndo = false;
		unit.EmbarkDisembarkBoat();
		SetSprite();
		gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
		unitSprite.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
		RemoveFogOfWarAroundUnit();
		if (tileGO.tile.enumType == TileTypes.HARBOUR)
		{
			ShowPotentialPaths();
		}
		ShowSelectedUnitStats();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_EmbarkDisembark", "O", unit.ID, unit.OwnerName, "Ship");
		}
		if (!unit.IsOnBoat)
		{
			TutorialManager.PerformAction(TutorialActions.PERFORM_LANDING);
		}
	}

	public void DropParatrooper(bool p_sendRPC)
	{
		if (!unit.IsParatrooper || !unit.IsDropable)
		{
			return;
		}
		CheckAAattackDroppedParatroopers();
		if (unit.IsDead)
		{
			return;
		}
		AIWaitInstruction aIWaitInstruction = new AIWaitInstruction(0f);
		if (!tileGO.isInFogOfWar)
		{
			if (PlayerSettings.Instance.IsAISlowMode)
			{
				aIWaitInstruction.waitTime = 0.7f;
			}
			else if (!PlayerSettings.Instance.FastAI && (!PlayerSettings.Instance.FastAllies || (!unit.OwnerPlayer.isAlliedWithHumanPlayer && unit.OwnerPlayer == TurnManager.currPlayer)))
			{
				aIWaitInstruction.waitTime = 0.3f;
			}
		}
		AI.AIWaitInstructions.Enqueue(aIWaitInstruction);
		unit.EmbarkDisembarkPlane();
		tileGO.airUnitGO = null;
		tileGO.tile.AirUnit = null;
		CheckLandmineDrop(p_sendRPC);
		if (unit.IsDead)
		{
			return;
		}
		tileGO.groundUnitGO = this;
		tileGO.tile.GroundUnit = unit;
		SetSprite();
		TryCaptureTile(tileGO.tile, p_sendRPC: false);
		ShowSelectedUnitStats();
		UIManager.showAirUnit = false;
		RemoveFogOfWarAroundUnit();
		ShowPotentialTargets();
		UpdateCounterValues();
		RotateUnit(tileGO, tileGO);
		if (tileGO.tile.enumType != TileTypes.AIRPORT)
		{
			unit.HasMoved = true;
			unit.AddXP(2, p_canAddHero: true, p_sendRPC);
			if (!tileGO.isInFogOfWar)
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetMovementSound("Drop paras"));
			}
		}
		gameObject.transform.rotation = Quaternion.identity;
		listOfpotentialTiles.Clear();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_EmbarkDisembark", "O", unit.ID, unit.OwnerName, "DropPara");
			MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tileGO.tile.PosX, tileGO.tile.PosY, "Drop paras", "Movement");
		}
		CheckAchievements();
	}

	public void LoadParatrooper(bool p_sendRPC)
	{
		unit.IsTrain = false;
		tileGO.tile.GroundUnit = null;
		tileGO.groundUnitGO = null;
		tileGO.airUnitGO = this;
		tileGO.tile.AirUnit = unit;
		unit.EmbarkDisembarkPlane();
		SetSprite();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_EmbarkDisembark", "O", unit.ID, unit.OwnerName, "LoadPara");
		}
	}

	private void CheckAAattackDroppedParatroopers()
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, 10))
		{
			if (unit.IsDead)
			{
				break;
			}
			if (item.tileGO.groundUnitGO == null || item.tileGO.groundUnitGO.unit.FilterType != "FilterAA" || !unit.OwnerPlayer.IsAtWarWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) || Vector2.Distance(item.tileGO.transform.position, tileGO.transform.position) > (float)(int)item.tileGO.groundUnitGO.unit.Range * 5.2f)
			{
				continue;
			}
			item.tileGO.groundUnitGO.unit.GetPotentialDamage(this, p_isRetaliation: false, p_isSupportiveFire: false);
			if (item.tileGO.groundUnitGO.unit.FinalDamage <= 0 || item.tileGO.groundUnitGO.unit.CurrAmmo <= 0)
			{
				continue;
			}
			if (item.tileGO.airUnitGO != null && item.tileGO.groundUnitGO != null)
			{
				item.tileGO.groundUnitGO.backgroundSprite.color = Color.white;
				item.tileGO.groundUnitGO.unitSprite.color = Color.white;
				if (item.tileGO.groundUnitGO.unit.IsCoreUnit)
				{
					item.tileGO.groundUnitGO.coreUnitOutline_sprite.color = Color.white;
				}
				item.tileGO.groundUnitGO.UpdateCounter();
				item.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				item.tileGO.airUnitGO.gameObject.SetActive(value: false);
			}
			RotateAttackedUnit(item.tileGO.groundUnitGO, this);
			item.tileGO.groundUnitGO.targetedUnitGO = this;
			item.tileGO.groundUnitGO.Attack(this);
			if (!item.tileGO.isInFogOfWar)
			{
				SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(item.tileGO.groundUnitGO.unit.AttackSound));
			}
			MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", item.tileGO.tile.PosX, item.tileGO.tile.PosY, item.tileGO.groundUnitGO.unit.AttackSound, "Attack");
			item.tileGO.groundUnitGO.DisplayDamage(item.tileGO.groundUnitGO, this);
			item.tileGO.groundUnitGO.unit.HasAttacked = false;
			item.tileGO.groundUnitGO.unit.AddRemoveActionPoints(1);
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", item.tileGO.groundUnitGO.unit, false);
		}
	}

	private int GetDeterministicMineDamage(int p_maxDamage, TileGO p_tileGO)
	{
		if (p_maxDamage <= 10)
		{
			return p_maxDamage;
		}
		return new System.Random(unit.ID + p_tileGO.tile.PosX + p_tileGO.tile.PosY).Next(10, p_maxDamage);
	}

	private void CheckLandmineDrop(bool p_sendRPC)
	{
		if (tileGO.groundUnitGO == null || !tileGO.groundUnitGO.unit.IsMine)
		{
			return;
		}
		string attackSound = tileGO.groundUnitGO.unit.AttackSound;
		if (!tileGO.isInFogOfWar)
		{
			SoundManager.instance.unit_Source.PlayOneShot(SoundManager.GetAttackSound(attackSound));
		}
		if (!unit.OwnerPlayer.IsComputer)
		{
			SteamManager.UnlockAchievement("Watch your step");
		}
		if (unit.Type == "Tank" || unit.IsTrain || unit.Type == "Boat" || unit.FilterType == "FilterBuilding")
		{
			int num = GetDeterministicMineDamage(tileGO.groundUnitGO.unit.HardDamage, tileGO);
			if (tileGO.groundUnitGO.unit.OwnerPlayer != null && tileGO.groundUnitGO.unit.OwnerPlayer.HasPolicy(Policies.Type.Magnetic_mines))
			{
				num += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
			}
			unit.CurrHP -= num;
			tileGO.groundUnitGO.unit.FinalDamage = num;
		}
		else if (unit.Type == "Infantry" || unit.Type == "AA" || unit.Type == "Artillery" || unit.IsEngineers)
		{
			int deterministicMineDamage = GetDeterministicMineDamage(tileGO.groundUnitGO.unit.SoftDamage, tileGO);
			unit.CurrHP -= deterministicMineDamage;
			tileGO.groundUnitGO.unit.FinalDamage = deterministicMineDamage;
		}
		DisplayDamage(tileGO.groundUnitGO, this);
		UpdateCounter();
		RemoveFogOfWarAroundUnit();
		tileGO.groundUnitGO.DestroyUnit(p_playAnimation: true, p_sendRPC);
		if (unit.CurrHP <= 0)
		{
			unit.CurrHP = 0;
			DestroyUnit(p_playAnimation: true, p_sendRPC);
		}
		canUndo = false;
	}

	public void ShowPotentialTargets()
	{
		if (unit.HasAttacked || unit.ActionPoints <= 0 || unit.IsTrain || unit.CurrAmmo == 0 || unit.IsDropable || unit.IsCommander || (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsComputer && !unit.OwnerPlayer.IsLocalClientPlayer()))
		{
			return;
		}
		MapGO.listOfTargetedUnit = new List<GameObject>();
		if (unit.OwnerPlayer.Faction == "Neutral" && unit.OwnerPlayer.IsComputer)
		{
			return;
		}
		List<Tile> list = ((!unit.IsOnBoat) ? Utils.GetAllTilesInRadiusAround(tileGO.tile, unit.Range) : Utils.GetAllTilesInRadiusAround(tileGO.tile, 1));
		foreach (Tile item in list)
		{
			if (TurnManager.isBadWeather && !GameData.Instance.map.DisableWeather && unit.Type == "Plane" && !unit.IsMissile && (unit.OwnerPlayer == null || (unit.OwnerPlayer != null && !unit.OwnerPlayer.HasPolicy(Policies.Type.All_weather))))
			{
				break;
			}
			if (!item.isVisible || (unit.IsOnBoat && item.enumType == TileTypes.SEA))
			{
				continue;
			}
			if (unit.IsSupplyShip)
			{
				if (item.tileGO.groundUnitGO != null && item.tileGO.groundUnitGO.gameObject != gameObject && !item.tileGO.groundUnitGO.unit.IsSubmerged && unit.OwnerPlayer.IsAlliedWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) && ((unit.CurrAmmo > 0 && item.tileGO.groundUnitGO.unit.CurrAmmo < item.tileGO.groundUnitGO.unit.MaxAmmo) || (unit.CurrAutonomy > 0 && item.tileGO.groundUnitGO.unit.CurrAutonomy < item.tileGO.groundUnitGO.unit.MaxAutonomy)))
				{
					MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
				}
				continue;
			}
			if (unit.OwnerPlayer.Faction == "Neutral")
			{
				return;
			}
			if (unit.FilterType == "FilterAA")
			{
				if (!TurnManager.isBadWeather && item.tileGO.airUnitGO != null && item.tileGO.airUnitGO.unit.Name != "V2" && unit.OwnerPlayer.IsAtWarWith(item.tileGO.airUnitGO.unit.OwnerPlayer))
				{
					MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
				}
				if (item.tileGO.groundUnitGO != null && unit.OwnerPlayer.IsAtWarWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) && (item.tileGO.airUnitGO == null || !UIManager.showAirUnit || item.tileGO.groundUnitGO.gameObject.activeSelf) && !item.tileGO.groundUnitGO.unit.IsMine && (item.tileGO.groundUnitGO.unit.Type != "Boat" || (item.tileGO.groundUnitGO.unit.IsOnBoat && item.Type == "Harbour")))
				{
					MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
				}
				if (item.tileGO.groundUnitGO != null && item.tileGO.airUnitGO != null)
				{
					if (unit.OwnerPlayer.IsAtWarWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) && unit.OwnerPlayer.IsAlliedWith(item.tileGO.airUnitGO.unit.OwnerPlayer))
					{
						item.tileGO.groundUnitGO.gameObject.SetActive(value: true);
						item.tileGO.airUnitGO.gameObject.SetActive(value: false);
					}
					else if (unit.OwnerPlayer.IsAlliedWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) && unit.OwnerPlayer.IsAtWarWith(item.tileGO.airUnitGO.unit.OwnerPlayer))
					{
						item.tileGO.groundUnitGO.gameObject.SetActive(value: false);
						item.tileGO.airUnitGO.gameObject.SetActive(value: true);
					}
				}
			}
			else if (unit.Type != "Plane")
			{
				if (!(item.tileGO.groundUnitGO != null) || item.tileGO.groundUnitGO.unit.OwnerPlayer == null || !unit.OwnerPlayer.IsAtWarWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) || item.tileGO.groundUnitGO.unit.IsMine)
				{
					continue;
				}
				if (item.tileGO.groundUnitGO.unit.Type == "Boat" && unit.CanAttackShips)
				{
					if (unit.IsSubmarine)
					{
						if (!item.tileGO.groundUnitGO.unit.IsSubmarine)
						{
							if (GameData.Instance.map.Date.Year < 1950)
							{
								if (!unit.IsSubmerged)
								{
									MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
								}
							}
							else
							{
								MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
							}
						}
						else if (GameData.Instance.map.Date.Year < 1950)
						{
							if (!unit.IsSubmerged && !item.tileGO.groundUnitGO.unit.IsSubmerged)
							{
								MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
							}
						}
						else
						{
							MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
						}
					}
					else if ((unit.IsDestroyer && item.tileGO.groundUnitGO.unit.IsSubmerged) || !item.tileGO.groundUnitGO.unit.IsSubmerged)
					{
						MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
					}
				}
				else
				{
					if (!(item.tileGO.groundUnitGO.unit.Type != "Boat") && (!item.tileGO.groundUnitGO.unit.IsOnBoat || !(item.Type == "Harbour")))
					{
						continue;
					}
					if (unit.IsSubmarine)
					{
						if (item.tileGO.groundUnitGO.unit.IsOnBoat || item.tileGO.groundUnitGO.unit.Type == "Boat")
						{
							MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
						}
					}
					else if (!item.tileGO.groundUnitGO.unit.IsOnBoat || (item.tileGO.groundUnitGO.unit.IsOnBoat && (item.Type == "Harbour" || unit.FilterType == "FilterArtillery" || unit.FilterType == "FilterBoat")))
					{
						MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
					}
				}
			}
			else
			{
				if (!(unit.Type == "Plane"))
				{
					continue;
				}
				bool flag = false;
				if (item.tileGO.airUnitGO != null && item.tileGO.airUnitGO.unit.Name != "V2" && unit.OwnerPlayer.IsAtWarWith(item.tileGO.airUnitGO.unit.OwnerPlayer) && unit.FilterType == "FilterFighter")
				{
					MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
					flag = true;
				}
				if (item.tileGO.groundUnitGO != null && (unit.Range > 1 || item.tileGO == tileGO) && unit.OwnerPlayer.IsAtWarWith(item.tileGO.groundUnitGO.unit.OwnerPlayer) && !item.tileGO.groundUnitGO.unit.IsMine)
				{
					if (item.tileGO.groundUnitGO.unit.IsSubmarine && !item.tileGO.groundUnitGO.unit.IsSubmerged)
					{
						if (unit.FilterType == "FilterBomber" || unit.FilterType == "FilterCAS" || unit.HasBombs || unit.HasRockets || unit.HasABomb)
						{
							MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
							flag = true;
						}
					}
					else if (!item.tileGO.groundUnitGO.unit.IsSubmerged)
					{
						MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
						flag = true;
					}
				}
				if (!flag && unit.FilterType == "FilterBomber" && item.tileGO == tileGO && ((item.MoneyProductionPerTurn > 0 && item.enumType != TileTypes.SEA) || (item.Bridge && !item.IsBridgeDestroyed) || item.enumType == TileTypes.AIRPORT) && (!unit.IsRecon || unit.HasABomb))
				{
					if (item.Bridge && !unit.OwnerPlayer.IsComputer)
					{
						MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
					}
					else if (item.ownerPlayer != null && unit.OwnerPlayer.IsAtWarWith(item.ownerPlayer) && item.NumberOfTurnsAtLowProd == 0)
					{
						MapGO.listOfTargetedUnit.Add(item.tileGO.gameObject);
					}
				}
			}
		}
		if (unit.OwnerPlayer.IsComputer || TurnManager.currPlayer.IsComputer)
		{
			return;
		}
		foreach (GameObject item2 in MapGO.listOfTargetedUnit)
		{
			TargetGO targetGO = ((!unit.IsSupplyShip) ? UnityEngine.Object.Instantiate(Resources.Load<TargetGO>("Prefabs/UI/Target"), MapGO.instance.targetsParent) : UnityEngine.Object.Instantiate(Resources.Load<TargetGO>("Prefabs/UI/Supply"), MapGO.instance.targetsParent));
			targetGO.transform.position = new Vector3(item2.transform.position.x, item2.transform.position.y, -0.95f);
		}
	}

	public void MoveToWayPoint()
	{
		if (unit.HasMoved && !unit.IsRecon && ((unit.OwnerPlayer != null && !unit.OwnerPlayer.HasPolicy(Policies.Type.Careful_planning)) || unit.OwnerPlayer == null))
		{
			return;
		}
		SelectUnit(p_playAnimation: true);
		listOfpotentialTiles.Clear();
		TileGO tileGO = GameData.Instance.map.TilesTable[unit.Waypoints[0][0], unit.Waypoints[0][1]].tileGO;
		GetPathToTile(tileGO.tile, p_waypointPath: true);
		if (currentPath.Count == 0)
		{
			new Notification(NotificationTypes.UNIT_CANNOT_FIND_PATH, unit.OwnerPlayer.Name, this.tileGO.gameObject.transform.position.x, this.tileGO.gameObject.transform.position.y, unit.GetUnitName(), p_isImportant: true);
			return;
		}
		List<TileGO> list = new List<TileGO>();
		int num = unit.CurrMP;
		foreach (TileGO item in currentPath)
		{
			if (num - item.tile.MPCost >= 0)
			{
				list.Add(item);
				num -= item.tile.MPCost;
				continue;
			}
			break;
		}
		currentPath = list;
		for (int num2 = currentPath.Count - 1; num2 >= 0; num2--)
		{
			if (unit.Type == "Plane")
			{
				if (currentPath[num2].tile.AirUnit != null)
				{
					currentPath.RemoveAt(num2);
				}
			}
			else if (currentPath[num2].tile.GroundUnit != null)
			{
				currentPath.RemoveAt(num2);
			}
		}
		if (currentPath.Count != 0)
		{
			StartCoroutine(CR_MoveUnitToAllTilesOfItsPath(tileGO));
		}
	}

	public void GetPathToTile(Tile p_targetTile, bool p_waypointPath = false, bool p_drawPath = false)
	{
		currentPath.Clear();
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		float num = 1f / ((float)(sizeX * sizeY) * 2f + 1f);
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				tilesTable[i, j].SetupForAStar();
			}
		}
		bool zoneOfControl = PlayerSettings.Instance.ZoneOfControl;
		byte currMP = unit.CurrMP;
		bool flag = unit.Type == "Plane";
		bool flag2 = unit.FilterType == "FilterBoat";
		bool isOnBoat = unit.IsOnBoat;
		bool flag3 = unit.Type == "Tank" || unit.IsMechanized;
		bool flag4 = unit.IsArmouredTrain || unit.IsTrain;
		bool flag5 = unit.CanMoveOnRivers();
		bool flag6 = unit.CanMoveOverMountains();
		bool flag7 = unit.CanMoveOnMarshes();
		PriorityQueue<Tile> priorityQueue = new PriorityQueue<Tile>();
		Dictionary<Tile, Tile> dictionary = new Dictionary<Tile, Tile>();
		Dictionary<Tile, int> dictionary2 = new Dictionary<Tile, int>();
		Dictionary<Tile, int> dictionary3 = new Dictionary<Tile, int>();
		bool flag8 = false;
		int num2 = 1;
		Tile tile = null;
		Tile tile2 = tileGO.tile;
		priorityQueue.Enqueue(tile2, 0f);
		dictionary2[tile2] = 0;
		dictionary3[tile2] = 0;
		while (num2 > 0)
		{
			tile = priorityQueue.Dequeue();
			num2--;
			if (tile == p_targetTile)
			{
				flag8 = true;
				break;
			}
			for (byte b = 0; b < tile.amountOfNeighbours; b++)
			{
				int num3 = 0;
				Tile tile3 = tile.neighbours[b];
				if (tile3.hasTerrainOwnerPlayer && !tile3.terrainOwner.IsAtWarWith(unit.OwnerPlayer) && !tile3.terrainOwner.IsAlliedWith(unit.OwnerPlayer))
				{
					continue;
				}
				if (flag)
				{
					if (tile3.AirUnit != null && tile3.isVisible && !unit.OwnerPlayer.IsAlliedWith(tile3.AirUnit.OwnerPlayer))
					{
						continue;
					}
					num3++;
				}
				else if (flag2)
				{
					if (tile3.isFrozen)
					{
						continue;
					}
					if (unit.IsPatrolBoat)
					{
						if (tile3.enumType != TileTypes.SEA && tile3.enumType != TileTypes.RIVER && !tile3.Bridge && ((tile3.enumType != TileTypes.HARBOUR && tile3.enumType != TileTypes.CITY) || (tile3.ownerPlayer != null && !unit.OwnerPlayer.IsAlliedWith(tile3.ownerPlayer))))
						{
							continue;
						}
					}
					else if (tile3.enumType != TileTypes.SEA && (tile3.enumType != TileTypes.HARBOUR || (tile3.ownerPlayer != null && !unit.OwnerPlayer.IsAlliedWith(tile3.ownerPlayer))))
					{
						continue;
					}
					if (tile3.GroundUnit != null && tile3.isVisible && !tile3.GroundUnit.IsMine && !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer))
					{
						continue;
					}
					if (zoneOfControl && num3 < currMP)
					{
						foreach (Tile neighbour in tile3.neighbours)
						{
							if (neighbour.isVisible && neighbour.GroundUnit != null && neighbour.GroundUnit.CanApplyZoneOfControl(unit))
							{
								tile3.applyZoneOfControl = true;
								break;
							}
						}
					}
					num3 += tile3.MPCost;
				}
				else if (isOnBoat)
				{
					if (tile3.isFrozen || (tile3.enumType != TileTypes.SEA && tile3 != p_targetTile) || (tile3.GroundUnit != null && tile3.isVisible && !tile3.GroundUnit.IsMine && !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer)))
					{
						continue;
					}
					num3 += tile3.MPCost;
				}
				else if (flag4)
				{
					if ((tile3.GroundUnit != null && tile3.isVisible && !tile3.GroundUnit.IsMine && !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer)) || (!tile3.Railroad && tile3.enumType != TileTypes.CITY && tile3.enumType != TileTypes.HARBOUR && tile3.enumType != TileTypes.FACTORY && tile3.enumType != TileTypes.REFINERY && tile3.enumType != TileTypes.AIRPORT && tile3.enumType != TileTypes.TRENCH) || (tile3.Bridge && tile3.IsBridgeDestroyed))
					{
						continue;
					}
					num3 += tile3.MPCost;
					if (zoneOfControl && num3 < currMP)
					{
						foreach (Tile neighbour2 in tile3.neighbours)
						{
							if (neighbour2.isVisible && neighbour2.GroundUnit != null && neighbour2.GroundUnit.CanApplyZoneOfControl(unit))
							{
								tile3.applyZoneOfControl = true;
								break;
							}
						}
					}
				}
				else
				{
					if ((tile3.enumType == TileTypes.MOUNTAIN && !flag6) || (tile3.enumType == TileTypes.MARSH && !flag7) || (tile3.enumType == TileTypes.SEA && !tile3.Bridge && !tile3.isFrozen) || (tile3.GroundUnit != null && tile3.isVisible && !tile3.GroundUnit.IsMine && !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer)))
					{
						continue;
					}
					if (tile3.IsBridgeDestroyed && !tile3.isFrozen)
					{
						if (!flag5)
						{
							continue;
						}
						num3 = Mathf.Clamp(unit.CurrMP - 1, 0, unit.CurrMP);
					}
					else if (tile3.enumType != TileTypes.RIVER || tile3.isFrozen)
					{
						num3 = ((!flag3 || !tile3.isRoadOrPlain) ? (num3 + tile3.MPCost) : (num3 + Mathf.Clamp(tile3.MPCost / 2, 1, tile3.MPCost)));
					}
					else if (flag5)
					{
						if (tile3.GroundUnit == null || !tile3.GroundUnit.IsPontoonBridge || !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer))
						{
							num3 = unit.CurrMP;
						}
					}
					else
					{
						if (tile3.GroundUnit == null || !tile3.GroundUnit.IsPontoonBridge || !unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer))
						{
							continue;
						}
						num3 = 0;
					}
					if (zoneOfControl && num3 < currMP)
					{
						foreach (Tile neighbour3 in tile3.neighbours)
						{
							if (neighbour3.isVisible && neighbour3.GroundUnit != null && neighbour3.GroundUnit.CanApplyZoneOfControl(unit))
							{
								tile3.applyZoneOfControl = true;
								break;
							}
						}
					}
				}
				int num4 = dictionary2[tile] + num3;
				if (zoneOfControl && tile3.applyZoneOfControl)
				{
					num4 += Mathf.Clamp(currMP - num4, 0, currMP);
				}
				int num5 = dictionary3[tile];
				if (!tile3.isVisible)
				{
					num5 = ((tile3.terrainOwner == null || !unit.OwnerPlayer.IsAlliedWith(tile3.terrainOwner)) ? (num5 + 2) : (num5 + 1));
				}
				if (!flag)
				{
					if (num4 >= currMP && tile3 != p_targetTile && tile3.GroundUnit != null && !tile3.GroundUnit.IsMine && tile3.GroundUnit.OwnerPlayer != null && unit.OwnerPlayer.IsAlliedWith(tile3.GroundUnit.OwnerPlayer))
					{
						continue;
					}
					if (p_waypointPath && tile3.GroundUnit != null && tile3 != tile2 && num4 <= currMP)
					{
						bool flag9 = false;
						foreach (Tile neighbour4 in tile3.neighbours)
						{
							if (neighbour4 != tile && num4 + neighbour4.MPCost <= currMP && (neighbour4.enumType != TileTypes.MOUNTAIN || flag6) && (neighbour4.enumType != TileTypes.MARSH || flag7) && (neighbour4.enumType != TileTypes.SEA || neighbour4.Bridge || neighbour4.isFrozen) && (neighbour4.enumType != TileTypes.RIVER || neighbour4.isFrozen || flag5) && (!neighbour4.IsBridgeDestroyed || neighbour4.isFrozen || flag5))
							{
								flag9 = true;
								break;
							}
						}
						if (!flag9)
						{
							continue;
						}
					}
				}
				bool flag10 = !dictionary2.ContainsKey(tile3) || num4 < dictionary2[tile3];
				if (!flag10 && num4 == dictionary2[tile3] && num5 < dictionary3[tile3])
				{
					flag10 = true;
				}
				if (flag10)
				{
					dictionary2[tile3] = num4;
					dictionary3[tile3] = num5;
					float p_priority = (float)(num4 + Utils.GetHeuristicDistance(p_targetTile, tile3)) + (float)num5 * num;
					if (num4 <= currMP || p_waypointPath)
					{
						priorityQueue.Enqueue(tile3, p_priority);
						dictionary[tile3] = tile;
						num2++;
					}
				}
			}
		}
		if (!flag8)
		{
			if (p_drawPath)
			{
				UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
			}
			return;
		}
		List<Tile> list = new List<Tile>();
		while (dictionary.ContainsKey(tile))
		{
			list.Add(tile);
			tile = dictionary[tile];
		}
		list.Reverse();
		for (int k = 0; k < list.Count; k++)
		{
			currentPath.Add(list[k].tileGO);
		}
		if (p_drawPath)
		{
			ShowMovementPath(currentPath);
		}
	}

	private void ShowMovementPath(List<TileGO> p_tileGOs)
	{
		if (listOfpotentialTiles.Count == 0 || p_tileGOs.Count == 0 || !listOfpotentialTiles.Contains(p_tileGOs.Last()))
		{
			UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
			return;
		}
		UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
		UIManager.instance.unitMovePath = new GameObject("Unit movement path");
		Transform parent = UIManager.instance.unitMovePath.transform;
		LineRenderer original = Resources.Load<LineRenderer>("Line movement");
		p_tileGOs.Insert(0, tileGO);
		int count = p_tileGOs.Count;
		Vector3[] array = new Vector3[count];
		LineRenderer lineRenderer = UnityEngine.Object.Instantiate(original, parent);
		lineRenderer.positionCount = count;
		lineRenderer.transform.position = p_tileGOs[0].transform.position;
		for (int i = 0; i < count; i++)
		{
			array[i] = p_tileGOs[i].transform.position;
			array[i].z = -0.1f;
		}
		lineRenderer.SetPositions(array);
		if (count > 1)
		{
			LineRenderer lineRenderer2 = UnityEngine.Object.Instantiate(Resources.Load<LineRenderer>("Line movement end"), parent);
			Vector3 position = p_tileGOs[count - 1].transform.position;
			position.z = -0.1f;
			lineRenderer2.transform.position = position;
			lineRenderer2.SetPosition(0, lineRenderer2.transform.position);
			lineRenderer2.SetPosition(1, lineRenderer2.transform.position);
		}
	}

	public void ShowPotentialPaths()
	{
		listOfpotentialTiles.Clear();
		if (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsComputer && !unit.OwnerPlayer.IsLocalClientPlayer())
		{
			return;
		}
		MapGO.instance.DestroyPotentialTilesAboveMap();
		ShowPotentialTargets();
		if (!unit.CanMove())
		{
			return;
		}
		bool zoneOfControl = PlayerSettings.Instance.ZoneOfControl;
		if (zoneOfControl)
		{
			foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, unit.CurrMP))
			{
				item.applyZoneOfControl = false;
			}
		}
		HashSet<Tile> hashSet = new HashSet<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		tileGO.tile.cost = 0;
		queue.Enqueue(tileGO.tile);
		bool flag = unit.Type == "Plane";
		bool flag2 = unit.Type == "Tank" || unit.IsMechanized;
		bool flag3 = unit.Type == "Boat";
		bool isOnBoat = unit.IsOnBoat;
		bool flag4 = unit.IsTrain || unit.IsArmouredTrain;
		bool flag5 = unit.CanMoveOnMarshes();
		bool flag6 = unit.CanMoveOnRivers();
		bool flag7 = unit.CanMoveOverMountains();
		while (queue.Count != 0)
		{
			foreach (Tile neighbour in queue.First().neighbours)
			{
				if (neighbour == tileGO.tile)
				{
					continue;
				}
				int num = 0;
				if (flag)
				{
					num++;
					if (neighbour.AirUnit != null && (neighbour.isVisible || isRetreating) && !unit.OwnerPlayer.IsAlliedWith(neighbour.AirUnit.OwnerPlayer))
					{
						continue;
					}
				}
				else if (isOnBoat)
				{
					if (neighbour.isFrozen || (neighbour.enumType == TileTypes.RIVER && !flag6) || (neighbour.enumType == TileTypes.MOUNTAIN && !flag7) || (neighbour.enumType == TileTypes.MARSH && !flag5) || (neighbour.GroundUnit != null && !neighbour.GroundUnit.IsMine && (neighbour.isVisible || isRetreating) && !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer)))
					{
						continue;
					}
					if (neighbour.enumType == TileTypes.SEA)
					{
						num += neighbour.MPCost;
					}
					else
					{
						if (!neighbour.neighbours.Contains(tileGO.tile))
						{
							continue;
						}
						num += unit.CurrMP;
					}
				}
				else if (flag3)
				{
					if (neighbour.isFrozen)
					{
						continue;
					}
					if (unit.IsPatrolBoat)
					{
						if (neighbour.enumType != TileTypes.SEA && neighbour.enumType != TileTypes.RIVER && !neighbour.Bridge && ((neighbour.enumType != TileTypes.HARBOUR && neighbour.enumType != TileTypes.CITY) || (neighbour.ownerPlayer != null && !unit.OwnerPlayer.IsAlliedWith(neighbour.ownerPlayer))))
						{
							continue;
						}
					}
					else if (neighbour.enumType != TileTypes.SEA && (neighbour.enumType != TileTypes.HARBOUR || (neighbour.ownerPlayer != null && !unit.OwnerPlayer.IsAlliedWith(neighbour.ownerPlayer))))
					{
						continue;
					}
					if (neighbour.GroundUnit != null && !neighbour.GroundUnit.IsMine && (neighbour.isVisible || isRetreating) && !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
					{
						continue;
					}
					if (zoneOfControl && num < unit.CurrMP)
					{
						foreach (Tile neighbour2 in neighbour.neighbours)
						{
							if (neighbour2.isVisible && neighbour2.GroundUnit != null && neighbour2.GroundUnit.CanApplyZoneOfControl(unit))
							{
								neighbour.applyZoneOfControl = true;
								break;
							}
						}
					}
					num += neighbour.MPCost;
				}
				else if (flag4)
				{
					if (neighbour.IsBridgeDestroyed || (!neighbour.Railroad && neighbour.enumType != TileTypes.CITY && neighbour.enumType != TileTypes.HARBOUR && neighbour.enumType != TileTypes.FACTORY && neighbour.enumType != TileTypes.REFINERY && neighbour.enumType != TileTypes.AIRPORT && neighbour.enumType != TileTypes.TRENCH) || (neighbour.GroundUnit != null && !neighbour.GroundUnit.IsMine && (neighbour.isVisible || isRetreating) && !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer)))
					{
						continue;
					}
					num += neighbour.MPCost;
					if (zoneOfControl && num < unit.CurrMP)
					{
						foreach (Tile neighbour3 in neighbour.neighbours)
						{
							if (neighbour3.isVisible && neighbour3.GroundUnit != null && neighbour3.GroundUnit.CanApplyZoneOfControl(unit))
							{
								neighbour.applyZoneOfControl = true;
								break;
							}
						}
					}
					if (!unit.IsArmouredTrain && neighbour.terrainOwner != null && !unit.OwnerPlayer.IsAlliedWith(neighbour.terrainOwner))
					{
						continue;
					}
				}
				else
				{
					if ((neighbour.enumType == TileTypes.SEA && !neighbour.Bridge && !neighbour.isFrozen) || (neighbour.enumType == TileTypes.MARSH && !flag5) || (neighbour.enumType == TileTypes.MOUNTAIN && !flag7) || (neighbour.GroundUnit != null && !neighbour.GroundUnit.IsMine && (neighbour.isVisible || isRetreating) && !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer)))
					{
						continue;
					}
					num = ((!flag2 || !neighbour.isRoadOrPlain) ? (num + neighbour.MPCost) : (num + Mathf.Clamp(neighbour.MPCost / 2, 1, neighbour.MPCost)));
					if (!neighbour.isFrozen)
					{
						if (neighbour.enumType == TileTypes.RIVER)
						{
							if (flag6)
							{
								if (unit.CurrMP > 0 && (neighbour.GroundUnit == null || !neighbour.GroundUnit.IsPontoonBridge || !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer)))
								{
									num = unit.CurrMP;
								}
							}
							else
							{
								if (neighbour.GroundUnit == null || !neighbour.GroundUnit.IsPontoonBridge || !unit.OwnerPlayer.IsAlliedWith(neighbour.GroundUnit.OwnerPlayer))
								{
									continue;
								}
								num = 0;
							}
						}
						if (neighbour.IsBridgeDestroyed)
						{
							if (unit.IsEngineers)
							{
								num = Mathf.Clamp(unit.CurrMP - 1, 0, unit.CurrMP);
							}
							else if (!unit.IsAmphibious)
							{
								continue;
							}
						}
					}
					if (zoneOfControl && num < unit.CurrMP)
					{
						foreach (Tile neighbour4 in neighbour.neighbours)
						{
							if (neighbour4.isVisible && neighbour4.GroundUnit != null && neighbour4.GroundUnit.CanApplyZoneOfControl(unit))
							{
								neighbour.applyZoneOfControl = true;
								break;
							}
						}
					}
				}
				if (neighbour.terrainOwner != null && neighbour.terrainOwner.Name != unit.OwnerName && !unit.OwnerPlayer.IsAlliedWith(neighbour.terrainOwner) && !unit.OwnerPlayer.IsAtWarWith(neighbour.terrainOwner))
				{
					continue;
				}
				int num2 = queue.First().cost + num;
				if (num2 <= unit.CurrMP)
				{
					if (neighbour.applyZoneOfControl)
					{
						num2 += Mathf.Clamp(unit.CurrMP - num2, 0, unit.CurrMP);
					}
					if (!hashSet.Contains(neighbour))
					{
						neighbour.cost = num2;
						queue.Enqueue(neighbour);
						hashSet.Add(neighbour);
					}
					else if (num2 < neighbour.cost)
					{
						neighbour.cost = num2;
						queue.Enqueue(neighbour);
					}
				}
			}
			queue.Dequeue();
		}
		foreach (Tile item2 in hashSet)
		{
			if (unit.Type == "Plane")
			{
				if (item2.AirUnit != null && (item2.isVisible || isRetreating))
				{
					continue;
				}
			}
			else if (item2.GroundUnit != null)
			{
				if (item2.GroundUnit.IsMine)
				{
					if (item2.GroundUnit.OwnerPlayer.IsAlliedWith(unit.OwnerPlayer))
					{
						continue;
					}
				}
				else if (item2.isVisible || isRetreating)
				{
					continue;
				}
			}
			listOfpotentialTiles.Add(item2.tileGO);
		}
		if (TurnManager.currPlayer.IsComputer || unit.OwnerPlayer.IsComputer)
		{
			return;
		}
		MapGO.listOfPotentialTilesAboveTheMap = new List<TileGO>();
		foreach (TileGO listOfpotentialTile in listOfpotentialTiles)
		{
			MapGO.listOfPotentialTilesAboveTheMap.Add(listOfpotentialTile);
		}
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.instance.ShowPotentialtileColor(isDeployment: false);
		}
	}

	public void OpenStatsMenu()
	{
		HeroBonusesGO.selectedHero = unit.Hero;
		UIManager.instance.stats_Panel.Open(this);
	}

	public void DisplayDamage(UnitGO p_attackingUnitGO, UnitGO p_attackedUnitGO)
	{
		MultiplayerManager.Instance.RunRPC("RPC_ShowDamage", "O", p_attackedUnitGO.tileGO.tile.PosX, p_attackedUnitGO.tileGO.tile.PosY, "-" + p_attackingUnitGO.unit.FinalDamage);
		if (!p_attackedUnitGO.tileGO.isInFogOfWar && !p_attackedUnitGO.unit.IsDead && !(CameraGO.instance.cam.orthographicSize > 70f) && CameraGO.instance.IsWithinFOV(p_attackedUnitGO.transform.position))
		{
			bool flag = p_attackingUnitGO.tileGO == p_attackedUnitGO.tileGO;
			if (p_attackingUnitGO.unit.IsMine || (p_attackingUnitGO.unit.Type == "Plane" && (p_attackedUnitGO.unit.AirDamage == 0 || p_attackedUnitGO.unit.IsCommander || p_attackedUnitGO.unit.CurrAmmo == 0)))
			{
				flag = false;
			}
			DamageTextGO damageTextGO = UnityEngine.Object.Instantiate(Resources.Load<DamageTextGO>("Prefabs/UI/Damage_Text"), p_attackedUnitGO.tileGO.transform);
			damageTextGO.text.text = "-" + p_attackingUnitGO.unit.FinalDamage;
			Vector3 position;
			if (flag)
			{
				position = ((!(p_attackedUnitGO.unit.Type != "Plane")) ? new Vector3(p_attackedUnitGO.transform.position.x + 1.5f, p_attackedUnitGO.transform.position.y + 1.5f, -20f) : new Vector3(p_attackedUnitGO.transform.position.x - 1.5f, p_attackedUnitGO.transform.position.y + 1.5f, -20f));
			}
			else
			{
				position = new Vector3(p_attackedUnitGO.transform.position.x, p_attackedUnitGO.transform.position.y + 1.5f, -20f);
			}
			damageTextGO.transform.position = position;
			damageTextGO.Animate();
		}
	}

	public void DisplayLevelUp()
	{
		MultiplayerManager.Instance.RunRPC("RPC_DisplayLevelUp", "O", tileGO.tile.PosX, tileGO.tile.PosY, unit.Lvl);
		if (!tileGO.isInFogOfWar && !unit.IsDead && !(CameraGO.instance.cam.orthographicSize > 70f) && CameraGO.instance.IsWithinFOV(transform.position))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Level up"));
			gameObject.transform.SetParent(tileGO.transform);
			gameObject.transform.position = new Vector3(tileGO.transform.position.x + 2f, tileGO.transform.position.y, 0f);
			gameObject.transform.localScale = Vector3.zero;
			gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("UI/Ranks/Outline/" + unit.Lvl);
			LeanTween.scale(gameObject, Vector3.one, 0.4f);
			LeanTween.move(gameObject, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 3f, -10f), 0.75f);
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
	}

	public void DisplayXPGain(string p_value, bool p_sendRPC)
	{
		if (p_sendRPC)
		{
			MultiplayerManager.RaiseEvent(98, p_value, tileGO.tile.PosX, tileGO.tile.PosY);
		}
		if (!tileGO.isInFogOfWar && !unit.IsDead && !(CameraGO.instance.cam.orthographicSize > 70f) && CameraGO.instance.IsWithinFOV(transform.position))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/XP_Text"));
			gameObject.transform.SetParent(tileGO.transform);
			gameObject.GetComponentInChildren<TextMeshPro>().text = p_value;
			gameObject.transform.position = new Vector3(tileGO.transform.position.x, tileGO.transform.position.y, 0f);
			LeanTween.move(gameObject, new Vector3(tileGO.transform.position.x, tileGO.transform.position.y + 2.5f, -10f), 1f);
			UnityEngine.Object.Destroy(gameObject, 3.5f);
		}
	}

	public void DisplayFuelLoss(string p_value)
	{
		MultiplayerManager.Instance.RunRPC("RPC_DisplayFuelLoss", "O", tileGO.tile.PosX, tileGO.tile.PosY, p_value);
		if (!tileGO.isInFogOfWar && !unit.IsDead && !(CameraGO.instance.cam.orthographicSize > 70f) && CameraGO.instance.IsWithinFOV(transform.position))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Fuel loss Text"));
			gameObject.transform.SetParent(tileGO.transform);
			gameObject.GetComponentInChildren<TextMeshPro>().text = p_value;
			Vector3 position = new Vector3(transform.position.x + 2.5f, transform.position.y, -10f);
			gameObject.transform.position = position;
			gameObject.transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, new Vector3(0.19f, 0.19f, 0.17f), 0.35f);
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
	}

	public void DisplayAmmoLoss(string p_value)
	{
		MultiplayerManager.Instance.RunRPC("RPC_DisplayAmmoLoss", "O", tileGO.tile.PosX, tileGO.tile.PosY, p_value);
		if (!tileGO.isInFogOfWar && !unit.IsDead && !(CameraGO.instance.cam.orthographicSize > 70f) && CameraGO.instance.IsWithinFOV(transform.position))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Ammo loss Text"));
			gameObject.transform.SetParent(tileGO.transform);
			gameObject.GetComponentInChildren<TextMeshPro>().text = p_value;
			Vector3 position = new Vector3(transform.position.x + 2.5f, transform.position.y + 1f, -10f);
			gameObject.transform.position = position;
			gameObject.transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, new Vector3(0.19f, 0.19f, 0.17f), 0.35f);
			UnityEngine.Object.Destroy(gameObject, 2f);
		}
	}

	public int CalculateEncirclement()
	{
		int num = 0;
		foreach (Tile neighbour in tileGO.tile.neighbours)
		{
			if (unit.Type == "Plane")
			{
				if (neighbour.tileGO.airUnitGO != null && unit.OwnerPlayer.IsAtWarWith(neighbour.tileGO.airUnitGO.unit.OwnerPlayer) && !neighbour.tileGO.airUnitGO.unit.IsRecon && neighbour.tileGO.airUnitGO.unit.FilterType != "FilterBomber" && !neighbour.tileGO.airUnitGO.unit.IsMissile)
				{
					num++;
				}
			}
			else if (neighbour.tileGO.groundUnitGO != null && unit.OwnerPlayer.IsAtWarWith(neighbour.tileGO.groundUnitGO.unit.OwnerPlayer) && !neighbour.tileGO.groundUnitGO.unit.IsCommander && !neighbour.tileGO.groundUnitGO.unit.IsTrain && neighbour.tileGO.groundUnitGO.unit.MaxMP > 0)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasAvailableRailroadNearby()
	{
		if (!tileGO.tile.Railroad && tileGO.tile.enumType != TileTypes.FACTORY && tileGO.tile.enumType != TileTypes.CITY && tileGO.tile.enumType != TileTypes.HARBOUR && tileGO.tile.enumType != TileTypes.AIRPORT && tileGO.tile.enumType != TileTypes.REFINERY)
		{
			return false;
		}
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(tileGO.tile, 1))
		{
			if ((item.enumType == TileTypes.FACTORY || item.enumType == TileTypes.CITY || item.enumType == TileTypes.HARBOUR || item.enumType == TileTypes.REFINERY || item.enumType == TileTypes.AIRPORT) && item.IsVictoryPoint && item.ownerPlayer != null && MapGO.IsThereACertainTileAround(item.tileGO, "Railroad"))
			{
				return true;
			}
		}
		return false;
	}

	public void SetSprite()
	{
		transform.rotation = Quaternion.identity;
		backgroundSprite.sprite = Utils.GetUnitBackground(unit, null);
		unitSprite.sprite = Utils.GetUnitSprite(unit);
		political_Sprite.sprite = Utils.GetPoliticalSprite(unit);
		heroIcon_Sprite.sprite = Utils.GetHeroIconSprite(unit);
		horse_GO.SetActive(unit.IsHorsed);
		mechanized_GO.SetActive(unit.IsMechanized);
		motorized_GO.SetActive(unit.IsMotorized);
		Recon_GO.SetActive(unit.IsRecon);
		dropTank_GO.SetActive(unit.HasDropTank);
		bomb_GO.SetActive(unit.HasBombs);
		rocket_GO.SetActive(unit.HasRockets);
		torpedo_GO.SetActive(unit.HasTorpedo);
		ABomb_GO.SetActive(unit.HasABomb);
		winter_GO.SetActive(unit.IsWinterSpecialized || (unit.ImmuneToWinterDamage && unit.FilterType != "FilterBoat" && unit.FilterType != "FilterFighter" && unit.FilterType != "FilterCAS" && unit.FilterType != "FilterBomber" && unit.FilterType != "FilterBuilding"));
		mountaineer_GO.SetActive(unit.IsMountaineer);
		political_Sprite.gameObject.SetActive(unit.IsPoliticalUnit);
		anchor_GO.SetActive(unit.CanBeRefueledOnCarrier || unit.IsCarrier);
		seaplane_GO.SetActive(unit.IsCatapultPlane || unit.CanCatapultPlanes);
		if (unit.IsCoreUnit)
		{
			if (!PlayerSettings.Instance.HideVisualCueCoreUnits)
			{
				if (unit.Type == "Boat" || unit.Type == "Plane")
				{
					coreUnitOutline_sprite.sprite = _roundCoreIcon_sprite;
				}
				else
				{
					coreUnitOutline_sprite.sprite = _squareCoreIcon_sprite;
				}
				if (!coreUnitOutline_sprite.gameObject.activeSelf)
				{
					coreUnitOutline_sprite.gameObject.SetActive(value: true);
				}
			}
		}
		else if (coreUnitOutline_sprite.gameObject.activeSelf)
		{
			coreUnitOutline_sprite.gameObject.SetActive(value: false);
		}
		if (!PlayerSettings.Instance.HideUnitsStats)
		{
			if (unit.Type == "Plane")
			{
				hardAttack.transform.localPosition = new Vector3(hardAttack.transform.localPosition.x, 5f, hardAttack.transform.localPosition.z);
				range.transform.localPosition = new Vector3(range.transform.localPosition.x, -5f, range.transform.localPosition.z);
				movementPoint.transform.localPosition = new Vector3(movementPoint.transform.localPosition.x, -4.4f, movementPoint.transform.localPosition.z);
			}
			else
			{
				hardAttack.transform.localPosition = new Vector3(hardAttack.transform.localPosition.x, 4f, hardAttack.transform.localPosition.z);
				range.transform.localPosition = new Vector3(range.transform.localPosition.x, -3.9f, range.transform.localPosition.z);
				movementPoint.transform.localPosition = new Vector3(movementPoint.transform.localPosition.x, -3.9f, movementPoint.transform.localPosition.z);
			}
		}
		else if (PlayerSettings.Instance.ShowUnitsNumbersOnCounters)
		{
			if (unit.Type == "Plane" || unit.Type == "Boat")
			{
				number_textMesh.transform.localPosition = new Vector3(number_textMesh.transform.localPosition.x, 5f, number_textMesh.transform.localPosition.z);
			}
			else
			{
				number_textMesh.transform.localPosition = new Vector3(number_textMesh.transform.localPosition.x, 4f, number_textMesh.transform.localPosition.z);
			}
		}
		if (shipName_textMesh != null)
		{
			if (PlayerSettings.Instance.ShowShipNames)
			{
				shipName_textMesh.text = unit.GetCounterDisplayName();
			}
			else
			{
				shipName_textMesh.gameObject.SetActive(value: false);
				shipName_textMesh.text = string.Empty;
			}
		}
		UpdateCounter();
		SetUnitRotation();
		transform.localScale = Vector3.one * PlayerSettings.Instance.UnitSize;
	}

	private void SetUnitMovementSpeed()
	{
		_speed = 25f;
		if (!unit.OwnerPlayer.IsComputer || !PlayerSettings.Instance.IsAISlowMode)
		{
			if (unit.Type == "Plane")
			{
				_speed = 35f;
			}
			else if (unit.Type == "Boat")
			{
				_speed = 30f;
			}
			else if (unit.IsTrain || unit.IsArmouredTrain)
			{
				_speed = 50f;
			}
			else if (unit.Type == "Tank" && unit.MaxMP > 1)
			{
				_speed = 30f;
			}
			else if (unit.IsHorsed || unit.IsMotorized || unit.IsMechanized)
			{
				_speed = 25f;
			}
		}
	}

	public void SetUnitRotation()
	{
		if (UIManager.isNATO)
		{
			unitSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else
		{
			if (unit.AllowedRotation == AllowedRotations.NONE || unit.AllowedRotation == AllowedRotations.NOT_SETUP)
			{
				return;
			}
			float num = 0f;
			if (unit.AllowedRotation == AllowedRotations.LEFT_RIGHT)
			{
				if (unit.FacingRotation != FacingRotations.EAST && unit.FacingRotation != FacingRotations.WEST)
				{
					unit.FacingRotation = FacingRotations.WEST;
				}
				switch (unit.FacingRotation)
				{
				case FacingRotations.WEST:
					num = 0f;
					break;
				case FacingRotations.EAST:
					num = 180f;
					break;
				}
				unitSprite.transform.rotation = Quaternion.Euler(0f, num, 0f);
			}
			else if (unit.AllowedRotation == AllowedRotations.ALL)
			{
				switch (unit.FacingRotation)
				{
				case FacingRotations.NORTH:
					num = -90f;
					break;
				case FacingRotations.SOUTH:
					num = 90f;
					break;
				case FacingRotations.EAST:
					num = 180f;
					break;
				case FacingRotations.WEST:
					num = 0f;
					break;
				case FacingRotations.NORTH_EAST:
					num = -150f;
					break;
				case FacingRotations.NORTH_WEST:
					num = -30f;
					break;
				case FacingRotations.SOUTH_EAST:
					num = 150f;
					break;
				case FacingRotations.SOUTH_WEST:
					num = 30f;
					break;
				}
				unitSprite.transform.rotation = Quaternion.Euler(0f, 0f, num);
			}
		}
	}

	private void CheckAchievements()
	{
		if (!GameData.Instance.ironManMode || MapGO.IsAIOnly || unit.OwnerPlayer.IsComputer || !TurnManager.currPlayer.CanControlUnitsOf(unit.OwnerPlayer))
		{
			return;
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.SafariKillCount >= 5)
		{
			SteamManager.UnlockAchievement("Safari bronze");
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.SafariKillCount >= 10)
		{
			SteamManager.UnlockAchievement("Safari silver");
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.SafariKillCount >= 20)
		{
			SteamManager.UnlockAchievement("Safari gold");
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.TotalUnitsLost >= 10)
		{
			SteamManager.UnlockAchievement("Statistics bronze");
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.TotalUnitsLost >= 50)
		{
			SteamManager.UnlockAchievement("Statistics silver");
		}
		if (unit.OwnerPlayer != null && unit.OwnerPlayer.TotalUnitsLost >= 100)
		{
			SteamManager.UnlockAchievement("Statistics gold");
		}
		if (unit.TotalDistance >= 100)
		{
			SteamManager.UnlockAchievement("Globetrotter_Bronze");
		}
		if (unit.TotalDistance >= 500)
		{
			SteamManager.UnlockAchievement("Globetrotter_Silver");
		}
		if (unit.TotalDistance >= 1000)
		{
			SteamManager.UnlockAchievement("Globetrotter_Gold");
		}
		if (unit.Hero != null && unit.IsDead)
		{
			SteamManager.UnlockAchievement("LoseHero");
		}
		if (!unit.IsDropable && unit.IsParatrooper)
		{
			SteamManager.UnlockAchievement("Airborne");
		}
		if (unit.TotalKill >= 10)
		{
			SteamManager.UnlockAchievement("Kill_Bronze");
		}
		if (unit.TotalKill >= 50)
		{
			SteamManager.UnlockAchievement("Kill_Silver");
		}
		if (unit.TotalKill >= 100)
		{
			SteamManager.UnlockAchievement("Kill_Gold");
		}
		if (unit.Type == "Plane")
		{
			if (unit.FighterKill + unit.HeavyBomberKill + unit.CASKill >= 5)
			{
				SteamManager.UnlockAchievement("Air Ace Bronze");
			}
			if (unit.FighterKill + unit.HeavyBomberKill + unit.CASKill >= 10)
			{
				SteamManager.UnlockAchievement("Air Ace Silver");
			}
			if (unit.FighterKill + unit.HeavyBomberKill + unit.CASKill >= 15)
			{
				SteamManager.UnlockAchievement("Air Ace Gold");
			}
		}
		if (unit.FilterType == "FilterAntitank")
		{
			if (unit.TankKill >= 5)
			{
				SteamManager.UnlockAchievement("Antitank Bronze");
			}
			if (unit.TankKill >= 10)
			{
				SteamManager.UnlockAchievement("Antitank Silver");
			}
			if (unit.TankKill >= 15)
			{
				SteamManager.UnlockAchievement("Antitank Gold");
			}
		}
		if (unit.BoatKill >= 5 && unit.FilterType == "FilterBoat")
		{
			SteamManager.UnlockAchievement("Terror of the sea Bronze");
		}
		if (unit.BoatKill >= 10 && unit.FilterType == "FilterBoat")
		{
			SteamManager.UnlockAchievement("Terror of the sea Silver");
		}
		if (unit.BoatKill >= 15 && unit.FilterType == "FilterBoat")
		{
			SteamManager.UnlockAchievement("Terror of the sea Gold");
		}
		if (unit.IsSubmarine && unit.BoatKill > 0)
		{
			SteamManager.UnlockAchievement("Wolfpack");
		}
		if (!(MapGO.selectedUnitGO != null))
		{
			return;
		}
		if (_surrendered && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			SteamManager.UnlockAchievement("Surrender");
			if (_captured)
			{
				SteamManager.instance.AddPlayerStat("Most captured units", 1);
			}
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.isRetreating && !MapGO.selectedUnitGO.unit.OwnerPlayer.IsComputer)
		{
			SteamManager.UnlockAchievement("Retreat");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead && MapGO.selectedUnitGO.targetedUnitGO.unit.IsCommander)
		{
			SteamManager.UnlockAchievement("Regicide");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead && MapGO.selectedUnitGO.targetedUnitGO.unit.IsConvoy)
		{
			SteamManager.UnlockAchievement("Convoy killer");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead && MapGO.selectedUnitGO.targetedUnitGO.unit.IsCarrier)
		{
			SteamManager.UnlockAchievement("Carrier killer");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead && MapGO.selectedUnitGO.targetedUnitGO.unit.Name.Contains("battleship"))
		{
			SteamManager.UnlockAchievement("Battleship killer");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead && MapGO.selectedUnitGO.targetedUnitGO.unit.IsPartisans)
		{
			SteamManager.UnlockAchievement("Crush the rebellion");
		}
		if (MapGO.selectedUnitGO.targetedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO.unit.IsDead)
		{
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.FilterType == "FilterTank")
			{
				SteamManager.instance.AddPlayerStat("Most tank kills", 1);
			}
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.FilterType == "FilterBoat")
			{
				SteamManager.instance.AddPlayerStat("Most ship kills", 1);
			}
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.Type == "Plane")
			{
				SteamManager.instance.AddPlayerStat("Most plane kills", 1);
			}
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.Type == "Infantry")
			{
				SteamManager.instance.AddPlayerStat("Most infantry kills", 1);
			}
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.FilterType == "FilterArtillery")
			{
				SteamManager.instance.AddPlayerStat("Most artillery kills", 1);
			}
			if (MapGO.selectedUnitGO.targetedUnitGO.unit.FilterType == "FilterAA")
			{
				SteamManager.instance.AddPlayerStat("Most AA kills", 1);
			}
			SteamManager.instance.AddPlayerStat("Most units killed", 1);
		}
	}
}
