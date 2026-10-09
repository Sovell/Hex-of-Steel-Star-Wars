using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileGO : MonoBehaviour
{
	private const float _vpFlagAnimationMaxOrthographicSize = 100f;

	private const float _vpFlagSwitchDuration = 0.42f;

	private const float _vpFlagSwitchInDelay = 0.1f;

	private const float _vpFlagSwitchOverlap = 0.06f;

	private const float _vpFlagHolderShift = 0.55f;

	private const float _vpFlagHolderAccentShift = 0.16f;

	private const float _vpFlagSpriteShift = 1.35f;

	private const float _vpFlagSpriteRotation = 24f;

	private const float _vpFlagHolderScaleMultiplier = 0.82f;

	private const float _vpFlagSpriteScaleMultiplier = 0.58f;

	private const float _vpFlagIncomingStartAlpha = 0.14f;

	private const string _vpFlagRepresentationName = "Flag representation";

	private const string _vpFlagTransitionName = "Flag representation transition";

	private static readonly Vector3 _vpFlagWorldOffset = new Vector3(-1f, 2.3f, -0.9f);

	private static readonly Vector3 _vpFlagHolderLocalScale = new Vector3(0.56f, 0.56f, 0.56f);

	private static readonly Vector3 _vpFlagRepresentationLocalPosition = new Vector3(0f, 0f, 0.1f);

	private static readonly Vector3 _vpFlagRepresentationLocalScale = new Vector3(1.78f, 1.78f, 1.78f);

	[Header("Setup")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Color _clearColor;

	[SerializeField]
	private Color _fogOfWarColor;

	public LineRenderer lineRenderer;

	[Header("RUNTIME INFOS")]
	public bool isInFogOfWar;

	public UnitGO airUnitGO;

	public UnitGO groundUnitGO;

	public GameObject cityNameGO;

	public GameObject flagGO;

	public GameObject smoke;

	public GameObject buildingIcon;

	public Tile tile;

	public UnitGO unitGOmakingThisTileAvailableForDeployment;

	private GameObject _spottedUnitIcon;

	private float _lastTimeFogOfWarSet = -1f;

	private void OnMouseEnter()
	{
		if (Utils.IsPointerOverUIObject())
		{
			return;
		}
		if (PlayerSettings.Instance.ShowInfoTile)
		{
			UIManager.instance.tileInfoRepresentation_Image.sprite = _spriteRenderer.sprite;
			UIManager.instance.frozenTile_Image.gameObject.SetActive(tile.isFrozen);
			UIManager.instance.tileInfoRepresentation_Image.transform.parent.gameObject.SetActive(value: true);
			UIManager.instance.tileDamageModifier_Text.text = tile.DamageModificator + "%";
			UIManager.instance.tileMPCost_Text.text = tile.MPCost.ToString();
			UIManager.instance.tileType_Text.text = tile.GetLocalizedDisplayName();
			UIManager.instance.tileCoordinates_Text.text = tile.PosX + ", " + tile.PosY;
			int num = ((tile.EditorOverwrite_income != 0) ? tile.EditorOverwrite_income : tile.MoneyProductionPerTurn);
			int num2 = num;
			if (tile.IsVictoryPoint)
			{
				if (tile.NumberOfTurnsAtLowProd > 0)
				{
					num2 = 0;
				}
				else if (tile.OriginalCountryOwner != tile.ownerPlayer.Country)
				{
					num2 = tile.OccupiedMoneyProductionPerTurn;
				}
				if (tile.ownerPlayer != null)
				{
					if (tile.NumberOfTurnsAtLowProd > 0 && tile.ownerPlayer.HasPolicy(Policies.Type.Minimized_destruction))
					{
						num2 += (int)Math.Round((float)num2 * 0.5f, MidpointRounding.ToEven);
					}
					if (num > 0)
					{
						if (tile.ownerPlayer.HasPolicy(Policies.Type.Industrial_overtime))
						{
							if (tile.enumType == TileTypes.FACTORY || tile.enumType == TileTypes.REFINERY)
							{
								num2 += (int)Math.Round((float)num * 0.35f, MidpointRounding.ToEven);
							}
							else if (tile.Name == "Village")
							{
								num2 -= (int)Math.Round((float)num * 0.4f, MidpointRounding.ToEven);
							}
						}
						if (tile.ownerPlayer.HasPolicy(Policies.Type.Early_mobilization))
						{
							num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
						}
						else if (tile.ownerPlayer.HasPolicy(Policies.Type.Partial_mobilization))
						{
							num2 += (int)Math.Round((float)num * 0.1f, MidpointRounding.ToEven);
						}
						else if (tile.ownerPlayer.HasPolicy(Policies.Type.War_economy))
						{
							num2 += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
						}
						else if (tile.ownerPlayer.HasPolicy(Policies.Type.Total_mobilization))
						{
							num2 += (int)Math.Round((float)num * 0.25f, MidpointRounding.ToEven);
						}
						if (tile.OriginalCountryOwner != tile.ownerPlayer.Country)
						{
							if (tile.ownerPlayer.HasPolicy(Policies.Type.Balanced_Approach))
							{
								num2 += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
							}
							else if (tile.ownerPlayer.HasPolicy(Policies.Type.Aggressive_Approach))
							{
								num2 += (int)Math.Round((float)num * 0.08f, MidpointRounding.ToEven);
							}
							else if (tile.ownerPlayer.HasPolicy(Policies.Type.Secret_Police))
							{
								num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
							}
							else if (tile.ownerPlayer.HasPolicy(Policies.Type.Forced_labor))
							{
								num2 += (int)Math.Round((float)num * 0.3f, MidpointRounding.ToEven);
							}
						}
					}
				}
			}
			if (num2 == num)
			{
				UIManager.instance.tileProduction_text.text = num2.ToString();
			}
			else
			{
				UIManager.instance.tileProduction_text.text = num2 + " (" + num + ")";
			}
			if (tile.terrainOwner == null)
			{
				UIManager.instance.tileInfoOwnerCountry_Image.sprite = Utils.GetFlagSprite("Neutral");
			}
			else
			{
				UIManager.instance.tileInfoOwnerCountry_Image.sprite = Utils.GetFlagSprite(tile.terrainOwner.Country);
			}
		}
		if (PlayerSettings.Instance.ShowUnitMovePath && MapGO.selectedUnitGO != null && !MapGO.selectedUnitGO.isMoving && !TurnManager.currPlayer.IsComputer && !Utils.IsPointerOverUIObject())
		{
			if (MapGO.selectedUnitGO.listOfpotentialTiles.Contains(this))
			{
				MapGO.selectedUnitGO.GetPathToTile(tile, p_waypointPath: false, p_drawPath: true);
			}
			else
			{
				UnityEngine.Object.Destroy(UIManager.instance.unitMovePath);
			}
		}
	}

	private void OnMouseOver()
	{
		if (TilesManagerGO.isOpen)
		{
			if (Input.GetMouseButtonDown(0) && !Utils.IsPointerOverUIObject())
			{
				TilesManagerGO.instance.ClickedOnTile(this);
			}
			return;
		}
		if (Input.GetKeyUp(PlayerSettings.Instance.MoveUnit) && !Input.GetKey(PlayerSettings.Instance.Waypoints) && (!PlayerSettings.Instance.MoveCameraWithLeftClick || (PlayerSettings.Instance.MoveCameraWithLeftClick && UIManager.LeftClickTime < 0.2f)) && MainMenu.stopwatchAttackMovement.ElapsedMilliseconds > 100 && !EventSystem.current.IsPointerOverGameObject() && MapGO.selectedUnitGO != null && !UIManager.isUIOpen)
		{
			MapGO.selectedUnitGO.MoveUnitToTile(this);
		}
		if (Input.GetKeyUp(PlayerSettings.Instance.MoveUnit) && !Input.GetKey(PlayerSettings.Instance.Waypoints) && (!PlayerSettings.Instance.MoveCameraWithLeftClick || (PlayerSettings.Instance.MoveCameraWithLeftClick && UIManager.LeftClickTime < 0.2f)) && !EventSystem.current.IsPointerOverGameObject())
		{
			SpawnUnit(ReservePanelGO.selectedUnit, p_playSound: true, p_sendRPC: true, p_selectNextUnitToDeploy: true);
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.MoveUnit) && Input.GetKey(PlayerSettings.Instance.Waypoints) && (bool)MapGO.selectedUnitGO)
		{
			ManageWaypoints();
			UIManager.instance.ShowWaypoints(MapGO.selectedUnitGO.unit);
		}
	}

	private void ManageWaypoints()
	{
		for (int i = 0; i < MapGO.selectedUnitGO.unit.Waypoints.Count; i++)
		{
			if (MapGO.selectedUnitGO.unit.Waypoints[i][0] == tile.PosX && MapGO.selectedUnitGO.unit.Waypoints[i][1] == tile.PosY)
			{
				MapGO.selectedUnitGO.unit.Waypoints.RemoveAt(i);
				return;
			}
		}
		MapGO.selectedUnitGO.unit.Waypoints.Add(new int[2] { tile.PosX, tile.PosY });
	}

	public void SetVPFlag()
	{
		if (!tile.IsVictoryPoint)
		{
			return;
		}
		string vPFlagResourcePath = GetVPFlagResourcePath();
		string vPFlagHolderName = GetVPFlagHolderName();
		Sprite flagSprite = Utils.GetFlagSprite(ResolveVPFlagCountry());
		if (flagGO == null)
		{
			flagGO = CreateVPFlagHolder(vPFlagResourcePath);
			SetVPFlagSprite(flagGO, flagSprite);
			return;
		}
		SpriteRenderer vPFlagRenderer = GetVPFlagRenderer(flagGO);
		if (vPFlagRenderer == null)
		{
			UnityEngine.Object.Destroy(flagGO);
			flagGO = CreateVPFlagHolder(vPFlagResourcePath);
			SetVPFlagSprite(flagGO, flagSprite);
			return;
		}
		PositionVPFlagHolder(flagGO);
		bool flag = GetNormalizedVPFlagHolderName(flagGO) != vPFlagHolderName;
		bool flag2 = vPFlagRenderer.sprite != flagSprite;
		bool flag3 = ShouldAnimateVPFlag();
		if (!flag && !flag2)
		{
			return;
		}
		if (flag)
		{
			GameObject gameObject = flagGO;
			GameObject gameObject2 = CreateVPFlagHolder(vPFlagResourcePath);
			SetVPFlagSprite(gameObject2, flagSprite);
			flagGO = gameObject2;
			if (!flag3)
			{
				CleanupVPFlagTransition(gameObject);
				LeanTween.cancel(gameObject);
				UnityEngine.Object.Destroy(gameObject);
				SetVPFlagInstant(gameObject2, flagSprite);
			}
			else
			{
				AnimateVPFlagHolderSwitch(gameObject, gameObject2);
			}
		}
		else if (!flag3)
		{
			SetVPFlagInstant(flagGO, flagSprite);
		}
		else
		{
			AnimateVPFlagSpriteSwitch(flagGO, vPFlagRenderer, flagSprite);
		}
	}

	private bool ShouldAnimateVPFlag()
	{
		if (CameraGO.instance == null || CameraGO.instance.cam == null)
		{
			return false;
		}
		if (CameraGO.instance.cam.orthographicSize > 100f)
		{
			return false;
		}
		if (!CameraGO.instance.IsWithinFOV(transform.position))
		{
			return false;
		}
		return true;
	}

	private string GetVPFlagResourcePath()
	{
		if (tile.PrimaryObjective)
		{
			return "Flags/Major VP";
		}
		if (tile.SecondaryObjective)
		{
			return "Flags/Secondary VP";
		}
		return "Flags/Normal VP";
	}

	private string GetVPFlagHolderName()
	{
		if (tile.PrimaryObjective)
		{
			return "Major VP";
		}
		if (tile.SecondaryObjective)
		{
			return "Secondary VP";
		}
		return "Normal VP";
	}

	private string ResolveVPFlagCountry()
	{
		tile.ownerPlayer = null;
		tile.terrainOwner = null;
		if (string.IsNullOrEmpty(tile.OwnerName))
		{
			return "Neutral";
		}
		if (!GameData.Instance.TryFindPlayerByName(tile.OwnerName, out var o_player))
		{
			return "Neutral";
		}
		tile.ownerPlayer = o_player;
		tile.terrainOwner = o_player;
		return o_player.Country;
	}

	private GameObject CreateVPFlagHolder(string p_resourcePath)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(p_resourcePath), transform);
		PositionVPFlagHolder(gameObject);
		return gameObject;
	}

	private void PositionVPFlagHolder(GameObject p_flagHolder)
	{
		p_flagHolder.transform.position = transform.position + _vpFlagWorldOffset;
	}

	private string GetNormalizedVPFlagHolderName(GameObject p_flagHolder)
	{
		return p_flagHolder.name.Replace("(Clone)", string.Empty).Trim();
	}

	private SpriteRenderer GetVPFlagRenderer(GameObject p_flagHolder)
	{
		Transform transform = p_flagHolder.transform.Find("Flag representation");
		if (transform == null)
		{
			return null;
		}
		return transform.GetComponent<SpriteRenderer>();
	}

	private void SetVPFlagSprite(GameObject p_flagHolder, Sprite p_flagSprite)
	{
		SpriteRenderer vPFlagRenderer = GetVPFlagRenderer(p_flagHolder);
		if (!(vPFlagRenderer == null))
		{
			vPFlagRenderer.sprite = p_flagSprite;
			ResetVPFlagRepresentationTransform(vPFlagRenderer.transform);
			SetSpriteRendererAlpha(vPFlagRenderer, 1f);
		}
	}

	private void SetVPFlagInstant(GameObject p_flagHolder, Sprite p_flagSprite)
	{
		CleanupVPFlagTransition(p_flagHolder);
		LeanTween.cancel(p_flagHolder);
		ResetVPFlagHolderTransform(p_flagHolder);
		SetSpriteRenderersAlpha(p_flagHolder.GetComponentsInChildren<SpriteRenderer>(includeInactive: true), 1f);
		SpriteRenderer vPFlagRenderer = GetVPFlagRenderer(p_flagHolder);
		if (!(vPFlagRenderer == null))
		{
			LeanTween.cancel(vPFlagRenderer.gameObject);
			vPFlagRenderer.sprite = p_flagSprite;
			ResetVPFlagRepresentationTransform(vPFlagRenderer.transform);
			SetSpriteRendererAlpha(vPFlagRenderer, 1f);
		}
	}

	private void ResetVPFlagHolderTransform(GameObject p_flagHolder)
	{
		PositionVPFlagHolder(p_flagHolder);
		p_flagHolder.transform.localScale = _vpFlagHolderLocalScale;
		p_flagHolder.transform.localEulerAngles = Vector3.zero;
	}

	private void AnimateVPFlagHolderSwitch(GameObject p_previousFlagHolder, GameObject p_newFlagHolder)
	{
		CleanupVPFlagTransition(p_previousFlagHolder);
		CleanupVPFlagTransition(p_newFlagHolder);
		LeanTween.cancel(p_previousFlagHolder);
		LeanTween.cancel(p_newFlagHolder);
		SpriteRenderer[] previousRenderers = p_previousFlagHolder.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		SpriteRenderer[] newRenderers = p_newFlagHolder.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		Vector3 finalPosition = transform.position + _vpFlagWorldOffset;
		Vector3 position = finalPosition + new Vector3(-0.09900001f, -0.6875f, 0f);
		Vector3 to = finalPosition + new Vector3(0.077f, 0.55f, 0f);
		Vector3 localScale = p_previousFlagHolder.transform.localScale;
		Vector3 newScale = p_newFlagHolder.transform.localScale;
		float num = Mathf.Max(0f, 0.040000003f);
		float time = 0.42f - num;
		float time2 = 0.16f;
		p_newFlagHolder.transform.position = position;
		p_newFlagHolder.transform.localScale = newScale * 0.82f;
		p_newFlagHolder.transform.localEulerAngles = new Vector3(0f, 0f, -24f);
		SetSpriteRenderersAlpha(newRenderers, 0.14f);
		LeanTween.move(p_newFlagHolder, finalPosition, time).setDelay(num).setEaseOutBack();
		LeanTween.scale(p_newFlagHolder, newScale, time).setDelay(num).setEaseOutBack();
		LeanTween.rotateZ(p_newFlagHolder, 0f, time).setDelay(num).setEaseOutBack();
		LeanTween.value(p_newFlagHolder, 0.14f, 1f, time).setDelay(num).setOnUpdate((float p_alpha) =>
		{
			SetSpriteRenderersAlpha(newRenderers, p_alpha);
		})
			.setOnComplete(() =>
			{
				if (!(p_newFlagHolder == null))
				{
					p_newFlagHolder.transform.position = finalPosition;
					p_newFlagHolder.transform.localScale = newScale;
					p_newFlagHolder.transform.localEulerAngles = Vector3.zero;
					SetSpriteRenderersAlpha(newRenderers, 1f);
				}
			});
		LeanTween.move(p_previousFlagHolder, to, time2).setEaseInBack();
		LeanTween.scale(p_previousFlagHolder, localScale * 0.82f, time2).setEaseInBack();
		LeanTween.rotateZ(p_previousFlagHolder, 24f, time2).setEaseInBack();
		LeanTween.value(p_previousFlagHolder, 1f, 0f, time2).setOnUpdate((float p_alpha) =>
		{
			SetSpriteRenderersAlpha(previousRenderers, p_alpha);
		}).setOnComplete(() =>
		{
			if (p_previousFlagHolder != null)
			{
				UnityEngine.Object.Destroy(p_previousFlagHolder);
			}
		});
	}

	private void AnimateVPFlagSpriteSwitch(GameObject p_flagHolder, SpriteRenderer p_currentFlagRenderer, Sprite p_newFlagSprite)
	{
		if (p_currentFlagRenderer.sprite == null)
		{
			p_currentFlagRenderer.sprite = p_newFlagSprite;
			ResetVPFlagRepresentationTransform(p_currentFlagRenderer.transform);
			return;
		}
		CleanupVPFlagTransition(p_flagHolder);
		LeanTween.cancel(p_flagHolder);
		LeanTween.cancel(p_currentFlagRenderer.gameObject);
		GameObject previousFlagRepresentation = UnityEngine.Object.Instantiate(p_currentFlagRenderer.gameObject, p_currentFlagRenderer.transform.parent);
		previousFlagRepresentation.name = "Flag representation transition";
		SpriteRenderer previousFlagRenderer = previousFlagRepresentation.GetComponent<SpriteRenderer>();
		ResetVPFlagRepresentationTransform(previousFlagRepresentation.transform);
		previousFlagRenderer.sprite = p_currentFlagRenderer.sprite;
		SetSpriteRendererAlpha(previousFlagRenderer, 1f);
		Vector3 localPosition = _vpFlagRepresentationLocalPosition + new Vector3(-0.162f, -1.35f, 0f);
		Vector3 to = _vpFlagRepresentationLocalPosition + new Vector3(0.108f, 1.215f, 0f);
		float num = Mathf.Max(0f, 0.040000003f);
		float time = 0.42f - num;
		float time2 = 0.16f;
		p_currentFlagRenderer.sprite = p_newFlagSprite;
		p_currentFlagRenderer.transform.SetAsLastSibling();
		p_currentFlagRenderer.transform.localPosition = localPosition;
		p_currentFlagRenderer.transform.localScale = _vpFlagRepresentationLocalScale * 0.58f;
		p_currentFlagRenderer.transform.localEulerAngles = new Vector3(0f, 0f, -24f);
		SetSpriteRendererAlpha(p_currentFlagRenderer, 0.14f);
		AnimateVPFlagHolderAccent(p_flagHolder);
		LeanTween.moveLocal(p_currentFlagRenderer.gameObject, _vpFlagRepresentationLocalPosition, time).setDelay(num).setEaseOutBack();
		LeanTween.scale(p_currentFlagRenderer.gameObject, _vpFlagRepresentationLocalScale, time).setDelay(num).setEaseOutBack();
		LeanTween.rotateLocal(p_currentFlagRenderer.gameObject, Vector3.zero, time).setDelay(num).setEaseOutBack();
		LeanTween.value(p_currentFlagRenderer.gameObject, 0.14f, 1f, time).setDelay(num).setOnUpdate((float p_alpha) =>
		{
			SetSpriteRendererAlpha(p_currentFlagRenderer, p_alpha);
		})
			.setOnComplete(() =>
			{
				if (!(p_currentFlagRenderer == null))
				{
					ResetVPFlagRepresentationTransform(p_currentFlagRenderer.transform);
					SetSpriteRendererAlpha(p_currentFlagRenderer, 1f);
				}
			});
		LeanTween.moveLocal(previousFlagRepresentation, to, time2).setEaseInBack();
		LeanTween.scale(previousFlagRepresentation, _vpFlagRepresentationLocalScale * 0.58f, time2).setEaseInBack();
		LeanTween.rotateLocal(previousFlagRepresentation, new Vector3(0f, 0f, 24f), time2).setEaseInBack();
		LeanTween.value(previousFlagRepresentation, 1f, 0f, time2).setOnUpdate((float p_alpha) =>
		{
			if (previousFlagRenderer != null)
			{
				SetSpriteRendererAlpha(previousFlagRenderer, p_alpha);
			}
		}).setOnComplete(() =>
		{
			if (previousFlagRepresentation != null)
			{
				UnityEngine.Object.Destroy(previousFlagRepresentation);
			}
		});
	}

	private void AnimateVPFlagHolderAccent(GameObject p_flagHolder)
	{
		Vector3 finalPosition = transform.position + _vpFlagWorldOffset;
		p_flagHolder.transform.position = finalPosition;
		LeanTween.move(p_flagHolder, finalPosition + Vector3.down * 0.16f, 0.044999998f).setEaseInSine().setOnComplete(() =>
		{
			if (!(p_flagHolder == null))
			{
				LeanTween.move(p_flagHolder, finalPosition, 0.375f).setEaseOutBack();
			}
		});
	}

	private void CleanupVPFlagTransition(GameObject p_flagHolder)
	{
		if (!(p_flagHolder == null))
		{
			Transform transform = p_flagHolder.transform.Find("Flag representation transition");
			if (!(transform == null))
			{
				LeanTween.cancel(transform.gameObject);
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	private void ResetVPFlagRepresentationTransform(Transform p_flagTransform)
	{
		p_flagTransform.localPosition = _vpFlagRepresentationLocalPosition;
		p_flagTransform.localScale = _vpFlagRepresentationLocalScale;
		p_flagTransform.localEulerAngles = Vector3.zero;
	}

	private void SetSpriteRenderersAlpha(SpriteRenderer[] p_renderers, float p_alpha)
	{
		foreach (SpriteRenderer p_renderer in p_renderers)
		{
			SetSpriteRendererAlpha(p_renderer, p_alpha);
		}
	}

	private void SetSpriteRendererAlpha(SpriteRenderer p_renderer, float p_alpha)
	{
		Color color = p_renderer.color;
		color.a = p_alpha;
		p_renderer.color = color;
	}

	public void SetCityName()
	{
		if (!string.IsNullOrEmpty(tile.CityName))
		{
			if (cityNameGO == null)
			{
				cityNameGO = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("CityNames/City Name"), transform);
			}
			cityNameGO.transform.position = new Vector3(transform.position.x, transform.position.y - 2.5f, -0.9f);
			cityNameGO.GetComponent<TextMeshPro>().text = tile.CityName.Replace('*', ' ');
			cityNameGO.name = tile.CityName;
		}
	}

	public bool SpawnUnit(Unit p_unit, bool p_playSound, bool p_sendRPC, bool p_selectNextUnitToDeploy = false)
	{
		if (p_unit == null)
		{
			return false;
		}
		if (!MapGO.listOfSpawnableTiles.Contains(this))
		{
			return false;
		}
		if (p_unit.TileBuilding)
		{
			BuildTile(p_unit);
			return true;
		}
		if ((p_unit.Type == "Plane" && airUnitGO != null) || (p_unit.Type != "Plane" && groundUnitGO != null))
		{
			return false;
		}
		TutorialManager.PerformAction(TutorialActions.DEPLOY_UNIT);
		if (p_unit.BuiltByEngineers)
		{
			TutorialManager.PerformAction(TutorialActions.BUILD_SOMETHING);
		}
		if (p_unit.CanBeRefueledOnCarrier)
		{
			TutorialManager.PerformAction(TutorialActions.DEPLOY_CARRIER_PLANE);
		}
		if (p_unit.IsMine)
		{
			TutorialManager.PerformAction(TutorialActions.DEPLOY_MINES);
			if (p_unit.Type == "Boat")
			{
				TutorialManager.PerformAction(TutorialActions.DEPLOY_NAVAL_MINES);
			}
			else
			{
				TutorialManager.PerformAction(TutorialActions.DEPLOY_LAND_MINES);
			}
		}
		UnitGO spawnedUnitGO;
		if (!p_unit.IsCoreUnit)
		{
			spawnedUnitGO = UnityEngine.Object.Instantiate(Resources.Load<UnitGO>("Prefabs/Unit prefab"));
		}
		else
		{
			spawnedUnitGO = UnityEngine.Object.Instantiate(Resources.Load<UnitGO>("Prefabs/Unit prefab core"));
		}
		spawnedUnitGO.spawnTime = Time.time;
		spawnedUnitGO.unit = p_unit;
		spawnedUnitGO.unit.unitGO = spawnedUnitGO;
		spawnedUnitGO.unit.IsReserve = false;
		spawnedUnitGO.unit.IsDefensive = false;
		if (MainMenu.isEditor || !TurnManager.currPlayer.IsComputer)
		{
			ReservePanelGO.instance.GenerateButtonsList();
		}
		if (isInFogOfWar)
		{
			spawnedUnitGO.backgroundSprite.color = new Color(1f, 1f, 1f, 0f);
			spawnedUnitGO.unitSprite.color = new Color(1f, 1f, 1f, 0f);
		}
		if (GameData.Instance.TryFindPlayerByName(spawnedUnitGO.unit.OwnerName, out var o_player))
		{
			o_player.TurnUnitsBought++;
			o_player.TotalUnitsBought++;
		}
		spawnedUnitGO.unit.SetCommissionDate();
		spawnedUnitGO.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -1f);
		spawnedUnitGO.tileGO = this;
		if (!MainMenu.isEditor)
		{
			spawnedUnitGO.transform.SetParent(UIManager.instance.unitsParent);
			if (spawnedUnitGO.unit.IsCommander)
			{
				MapGO.listOfAllGeneralsOnTheMap.Add(spawnedUnitGO);
			}
		}
		else
		{
			spawnedUnitGO.transform.SetParent(MapEditorGO.instance.unitParent);
		}
		spawnedUnitGO.unit.OwnerPlayer = o_player;
		spawnedUnitGO.unit.AutoSupplies = spawnedUnitGO.unit.OwnerPlayer.OOBautoSupplies;
		if (tile.enumType == TileTypes.SEA && spawnedUnitGO.unit.Type != "Boat" && spawnedUnitGO.unit.Type != "Plane")
		{
			spawnedUnitGO.unit.Type = "Boat";
			spawnedUnitGO.unit.IsOnBoat = true;
			spawnedUnitGO.unit.TampMaxMP = spawnedUnitGO.unit.MaxMP;
			spawnedUnitGO.unit.TampMaxAutonomy = spawnedUnitGO.unit.MaxAutonomy;
			spawnedUnitGO.unit.TampAutonomy = spawnedUnitGO.unit.TampMaxAutonomy;
			spawnedUnitGO.unit.MaxMP = 5;
			spawnedUnitGO.unit.MaxAutonomy = 100;
			spawnedUnitGO.unit.CurrMP = spawnedUnitGO.unit.MaxMP;
			spawnedUnitGO.unit.CurrAutonomy = spawnedUnitGO.unit.MaxAutonomy;
		}
		if (tile.IsVictoryPoint && spawnedUnitGO.unit.GetNameToLower().Contains("garrison"))
		{
			spawnedUnitGO.unit.AsleepState = AsleepStates.SLEEP;
		}
		spawnedUnitGO.RemoveFogOfWarAroundUnit();
		spawnedUnitGO.name = spawnedUnitGO.unit.ID.ToString();
		if (UIManager.instance.coreUnitsPanel.activeSelf && spawnedUnitGO.unit.IsCoreUnit)
		{
			UIManager.instance.GenerateCoreUnitsSlots();
		}
		if (p_playSound)
		{
			string text = "";
			if (spawnedUnitGO.unit.MaxMP == 0 || spawnedUnitGO.unit.FilterType == "FilterBuilding")
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy building"));
			}
			else if (spawnedUnitGO.unit.Type == "Boat")
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy ship"));
			}
			else if (spawnedUnitGO.unit.FilterType == "FilterInfantry" && !spawnedUnitGO.unit.IsParatrooper)
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy infantry"));
			}
			else if (spawnedUnitGO.unit.Type == "Artillery" || spawnedUnitGO.unit.Type == "AA")
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy artillery"));
			}
			else if (spawnedUnitGO.unit.Type == "Plane" || spawnedUnitGO.unit.IsParatrooper)
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy plane"));
			}
			else if (spawnedUnitGO.unit.Type == "Tank")
			{
				SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy tank"));
			}
			if (p_sendRPC)
			{
				MultiplayerManager.Instance.RunRPC("RPC_PlaySound", "O", tile.PosX, tile.PosY, text, "UI");
			}
		}
		if (spawnedUnitGO.unit.Type == "Plane")
		{
			spawnedUnitGO.tileGO.tile.AirUnit = spawnedUnitGO.unit;
			airUnitGO = spawnedUnitGO;
			airUnitGO.gameObject.SetActive(value: true);
			UIManager.showAirUnit = true;
			if (groundUnitGO != null)
			{
				groundUnitGO.gameObject.SetActive(value: false);
			}
		}
		else
		{
			spawnedUnitGO.tileGO.tile.GroundUnit = spawnedUnitGO.unit;
			groundUnitGO = spawnedUnitGO;
			groundUnitGO.gameObject.SetActive(value: true);
			UIManager.showAirUnit = false;
			if (airUnitGO != null)
			{
				airUnitGO.gameObject.SetActive(value: false);
			}
		}
		spawnedUnitGO.unit.HasAttacked = true;
		spawnedUnitGO.unit.HasMoved = true;
		spawnedUnitGO.unit.NumberOfTimesTheUnitMoved = 2;
		spawnedUnitGO.unit.ActionPoints = 0;
		if (spawnedUnitGO.unit.IsPartisans)
		{
			spawnedUnitGO.unit.ActionPoints = 2;
			spawnedUnitGO.unit.HasAttacked = false;
			spawnedUnitGO.unit.HasMoved = false;
		}
		spawnedUnitGO.ManageTwoUnitsIndicator();
		spawnedUnitGO.ManageUnitInfoContainer();
		if (p_unit == ReservePanelGO.selectedUnit)
		{
			ReservePanelGO.selectedUnit = null;
		}
		ReservePanelGO.instance.RefreshDeployNotification();
		spawnedUnitGO.SetSprite();
		if (p_sendRPC)
		{
			MultiplayerManager.Instance.RunRPC("RPC_SpawnUnit", "O", tile.PosX, tile.PosY, spawnedUnitGO.unit);
			MultiplayerManager.Instance.RunRPC("RPC_SyncUnit", "O", spawnedUnitGO.unit, true);
		}
		spawnedUnitGO.TryCaptureTile(tile, p_sendRPC: false);
		if (!isInFogOfWar && CameraGO.instance.IsWithinFOV(transform.position))
		{
			Vector3 localScale = spawnedUnitGO.transform.localScale;
			spawnedUnitGO.transform.localScale = Vector3.one;
			LeanTween.scale(spawnedUnitGO.gameObject, localScale, 0.15f);
			Color backgroundColor = spawnedUnitGO.backgroundSprite.color;
			backgroundColor.a = 0f;
			Color unitColor = spawnedUnitGO.unitSprite.color;
			unitColor.a = 0f;
			spawnedUnitGO.backgroundSprite.color = backgroundColor;
			spawnedUnitGO.unitSprite.color = unitColor;
			LeanTween.alpha(spawnedUnitGO.gameObject, 1f, 0.15f).setOnUpdate((float value) =>
			{
				if (spawnedUnitGO.tileGO.isInFogOfWar)
				{
					spawnedUnitGO.backgroundSprite.color = backgroundColor;
					spawnedUnitGO.unitSprite.color = unitColor;
				}
			});
		}
		if (unitGOmakingThisTileAvailableForDeployment != null)
		{
			unitGOmakingThisTileAvailableForDeployment.unit.HasBuilt = true;
			unitGOmakingThisTileAvailableForDeployment = null;
		}
		if (!TurnManager.currPlayer.IsComputer)
		{
			UIManager.instance.RefreshIncomePerTurnUI();
			if (p_selectNextUnitToDeploy)
			{
				foreach (Unit listOfUnit in TurnManager.currPlayer.ListOfUnits)
				{
					if (listOfUnit.IsReserve)
					{
						ReservePanelGO.instance.SelectNextUnitToDeploy();
						return true;
					}
				}
			}
			ReservePanelGO.instance.CloseReservePanel();
		}
		return true;
	}

	private void BuildTile(Unit p_unit)
	{
		p_unit.IsReserve = false;
		if (p_unit == ReservePanelGO.selectedUnit)
		{
			ReservePanelGO.selectedUnit = null;
		}
		Player o_player;
		if (tile.terrainOwner == null)
		{
			GameData.Instance.TryFindPlayerByName(p_unit.OwnerName, out o_player);
		}
		else
		{
			o_player = tile.terrainOwner;
		}
		if (p_unit.Name == "Airfield")
		{
			p_unit.Name = "Airport";
		}
		foreach (Tile allTile in Utils.GetAllTiles())
		{
			if (!(allTile.Name == p_unit.Name))
			{
				continue;
			}
			SoundManager.instance.UI_Source.PlayOneShot(SoundManager.GetUISound("Deploy building"));
			tile.Name = allTile.Name;
			tile.Type = allTile.Type;
			tile.FilterType = allTile.FilterType;
			tile.MPCost = allTile.MPCost;
			tile.MoneyProductionPerTurn = allTile.MoneyProductionPerTurn;
			tile.OccupiedMoneyProductionPerTurn = allTile.OccupiedMoneyProductionPerTurn;
			tile.Manpower = allTile.Manpower;
			tile.OccupiedManpower = allTile.OccupiedManpower;
			tile.NumberOfTurnsSinceVPGotCaptured = byte.MaxValue;
			tile.DamageModificator = allTile.DamageModificator;
			tile.terrainOwner = o_player;
			tile.Capturable = allTile.Capturable;
			tile.NumberOfTurnsAtLowProd = 0;
			tile.IsReinforcementTile = allTile.IsReinforcementTile;
			tile.Railroad = allTile.Railroad;
			tile.Capturable = allTile.Capturable;
			tile.Bridge = allTile.Bridge;
			if (allTile.FilterType == "FilterVP" || allTile.FilterType == "FilterHarbour")
			{
				tile.IsVictoryPoint = true;
				tile.OwnerName = o_player.Name;
				tile.ownerPlayer = o_player;
				tile.OriginalCountryOwner = o_player.Country;
				SetVPFlag();
				MapGO.victoryPoints.Add(this);
			}
			SetSprite();
			if (TurnManager.currPlayer.IsComputer)
			{
				p_unit.MaxHP = 0;
				p_unit.IsDead = true;
			}
			else
			{
				o_player.RemoveUnit(p_unit, p_sendRPC: true);
				ReservePanelGO.instance.GenerateButtonsList();
				if (!MainMenu.isEditor)
				{
					MapGO.instance.DestroyPotentialTilesAboveMap();
					ReservePanelGO.instance.RefreshDeployNotification();
				}
			}
			MultiplayerManager.RPC("RPC_SyncTile", tile.GetCoord(), tile);
			if (!MultiplayerManager.IsRealtimeMultiplayer() || tile.ownerPlayer.IsLocalClientPlayer())
			{
				if (p_unit.Name == "Factory")
				{
					SteamManager.UnlockAchievement("Build Factory");
				}
				else if (p_unit.Name == "Airport")
				{
					SteamManager.UnlockAchievement("Build Airfield");
				}
				else if (p_unit.Name == "Refinery")
				{
					SteamManager.UnlockAchievement("Build Refinery");
				}
				else if (p_unit.Name == "Trenches")
				{
					SteamManager.UnlockAchievement("Build Trenches");
				}
			}
			break;
		}
		tile.SetRuntimeOptimizations();
		if (tile.GroundUnit != null)
		{
			tile.GroundUnit.EntrenchmentLevel = 0;
		}
	}

	public void SetSprite()
	{
		_spriteRenderer.sprite = Utils.GetTileSprite(tile);
		SetSmoke();
	}

	public void SetSmoke()
	{
		if (tile.NumberOfTurnsAtLowProd > 0 && smoke == null)
		{
			smoke = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Destruction/Smoke"));
			smoke.transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
		}
		else if (tile.NumberOfTurnsAtLowProd == 0 && smoke != null)
		{
			UnityEngine.Object.Destroy(smoke);
		}
	}

	public Sprite GetSprite()
	{
		return _spriteRenderer.sprite;
	}

	public Color GetSpriteColor()
	{
		return _spriteRenderer.color;
	}

	public void CopyTileGOFogOfWarSetting(TileGO p_sourceTileGO)
	{
		if (p_sourceTileGO.isInFogOfWar)
		{
			SetFogOfWar();
		}
		else
		{
			RemoveFogOfWar();
		}
	}

	public void SetFogOfWar()
	{
		if (!isInFogOfWar)
		{
			isInFogOfWar = true;
			LeanTween.cancel(gameObject);
			_spriteRenderer.color = _fogOfWarColor;
		}
	}

	public void RemoveFogOfWar(bool p_smooth = false)
	{
		if (!isInFogOfWar)
		{
			return;
		}
		isInFogOfWar = false;
		if (p_smooth)
		{
			if (Time.time - _lastTimeFogOfWarSet < 0.25f)
			{
				return;
			}
			_lastTimeFogOfWarSet = Time.time;
			LeanTween.color(gameObject, _clearColor, 0.15f).setEaseOutSine().setOnUpdate((Color color) =>
			{
				_spriteRenderer.color = color;
			});
		}
		else
		{
			_spriteRenderer.color = _clearColor;
		}
		ClearUnitSpotted();
	}

	public void ClearUnitSpotted()
	{
		UnityEngine.Object.Destroy(_spottedUnitIcon);
	}

	public void SpotUnit(Unit p_unit)
	{
		ClearUnitSpotted();
		string text = string.Empty;
		if (p_unit.Type == "Plane")
		{
			text = "Prefabs/RADAR spot plane";
		}
		else if (p_unit.Type == "Boat")
		{
			text = "Prefabs/Sonar spot ship";
		}
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("Cannot find spotted unit icon for: " + p_unit.Name + " | " + p_unit.Type, this);
			return;
		}
		_spottedUnitIcon = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(text), transform);
		_spottedUnitIcon.transform.position = transform.position + Vector3.back;
	}

	public void SetTileColor(Color p_color)
	{
		_spriteRenderer.color = p_color;
	}

	public void HighlightTile()
	{
		StartCoroutine(CR_HighlightTile());
	}

	private IEnumerator CR_HighlightTile()
	{
		yield return new WaitForSeconds(0.3f);
		GameObject highlightObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Highlight ring"));
		highlightObj.transform.position = new Vector3(transform.position.x, transform.position.y, -0.48f);
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t2 = t / 0.5f;
			highlightObj.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
			yield return null;
		}
		highlightObj.GetComponent<SpriteRenderer>().color = Color.white;
		for (float t = 0f; t < 0.5f; t += Time.deltaTime)
		{
			float t3 = t / 0.5f;
			highlightObj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t3);
			yield return null;
		}
		highlightObj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
		UnityEngine.Object.Destroy(highlightObj);
	}
}
