using System.Collections;
using Photon.Pun;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraGO : MonoBehaviour
{
	public static CameraGO instance;

	private float camsize;

	private float camSpeed;

	private float camSpeedMultiplier = 2f;

	public float zoomFactor = 3f;

	public float targetZoom;

	public float zoomLerpSpeed = 15f;

	public int minZoom = 5;

	public int maxZoom = 1000;

	public float limitXPLUS;

	public float limitYPLUS;

	public float limitXMINUS;

	public float limitYMINUS;

	public Camera cam;

	[Header("Camera initial zoom level")]
	[SerializeField]
	private float _minZoomLevel = 5f;

	[SerializeField]
	private float _maxZoomLevel = 200f;

	[SerializeField]
	private float _minMapSize = 1f;

	[SerializeField]
	private float _maxMapSize = 300f;

	[SerializeField]
	private AnimationCurve _camSize_width;

	[SerializeField]
	private AnimationCurve _camSize_height;

	private Vector2 _mouseClickPos;

	private Vector2 _mouseCurrentPos;

	private bool _panning;

	private bool _isCRrunning;

	private bool _retablishCamPos;

	private float _prevZoomValue;

	private float _totalZoom;

	private Vector3 _prevCamPos;

	private float _totalDistanceMoved;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		limitXPLUS = (float)GameData.Instance.map.SizeX * 5.2f;
		limitYPLUS = (float)GameData.Instance.map.SizeY * 5.2f;
		transform.position = new Vector3(GameData.Instance.map.SizeX * 2, (float)GameData.Instance.map.SizeY * 2.5f, -80f);
		if (GameData.Instance.map.SizeX > GameData.Instance.map.SizeY)
		{
			targetZoom = _camSize_width.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeX));
		}
		else if (GameData.Instance.map.SizeY > GameData.Instance.map.SizeX)
		{
			targetZoom = _camSize_height.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeY));
		}
		else
		{
			targetZoom = _camSize_width.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeX));
		}
		targetZoom = Mathf.Clamp(cam.orthographicSize * _maxZoomLevel * targetZoom, _minZoomLevel, _maxZoomLevel);
		cam.orthographicSize = targetZoom;
		if (ColorUtility.TryParseHtmlString(PlayerSettings.Instance.CameraBackgroundColor, out var color))
		{
			cam.backgroundColor = color;
		}
		_prevCamPos = transform.position;
		_prevZoomValue = cam.orthographicSize;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			Utils.MakeScreenshot();
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.JumpLeftSideMap))
		{
			JumpLeftSideMap();
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.JumpRightSideMap))
		{
			JumpRightSideMap();
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.JumpDownSideMap))
		{
			JumpDownSideMap();
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.JumpUpSideMap))
		{
			JumpUpSideMap();
		}
		if (UIManager.isUIOpen)
		{
			_retablishCamPos = true;
			return;
		}
		if (_retablishCamPos)
		{
			_panning = false;
			_retablishCamPos = false;
		}
		CheckMouseInput();
		Zooming();
		CheckKeyboardInput();
		LimitPosition();
		if (Application.isFocused && !UIManager.isUIOpen && PlayerSettings.Instance.MoveCameraWithMouseOnEdge)
		{
			MoveCameraWithEdge();
		}
		if (!MainMenu.isEditor && TurnManager.currPlayer != null)
		{
			if (TurnManager.currPlayer.IsComputer)
			{
				StopUnitsBlink();
			}
			if (cam.orthographicSize < 30f && !PlayerSettings.Instance.UnitsAlwaysBlink)
			{
				StopUnitsBlink();
			}
			else if (!_isCRrunning)
			{
				StartCoroutine(CR_StartUnitsBlink());
			}
			MapGO.instance.UpdateCountryNamesOpacity();
		}
		if (TutorialManager.HasAction(TutorialActions.CAMERA_ZOOM))
		{
			_totalZoom += Mathf.Abs(cam.orthographicSize - _prevZoomValue);
			if (_totalZoom > 50f)
			{
				TutorialManager.PerformAction(TutorialActions.CAMERA_ZOOM);
			}
			_prevZoomValue = cam.orthographicSize;
		}
		if (TutorialManager.HasAction(TutorialActions.CAMERA_MOVE))
		{
			_totalDistanceMoved += Vector3.Distance(_prevCamPos, transform.position);
			if (_totalDistanceMoved > 75f)
			{
				TutorialManager.PerformAction(TutorialActions.CAMERA_MOVE);
			}
			_prevCamPos = transform.position;
		}
	}

	private void MoveCameraWithEdge()
	{
		Rect safeArea = Screen.safeArea;
		Vector2 vector = Input.mousePosition;
		vector.x = Mathf.Clamp(vector.x, safeArea.xMin, safeArea.xMax);
		vector.y = Mathf.Clamp(vector.y, safeArea.yMin, safeArea.yMax);
		float num = (vector.x - safeArea.xMin) / safeArea.width;
		float num2 = (vector.y - safeArea.yMin) / safeArea.height;
		float num3 = cam.orthographicSize * camSpeedMultiplier;
		if (num2 >= 0.995f)
		{
			transform.position += num3 * Time.deltaTime * Vector3.up;
		}
		if (num2 <= 0.005f)
		{
			transform.position += num3 * Time.deltaTime * Vector3.down;
		}
		if (num >= 0.995f)
		{
			transform.position += num3 * Time.deltaTime * Vector3.right;
		}
		if (num <= 0.005f)
		{
			transform.position += num3 * Time.deltaTime * Vector3.left;
		}
	}

	private void Zooming()
	{
		if (Input.GetKey(KeyCode.LeftControl) || (EventSystem.current.IsPointerOverGameObject() && Time.timeScale != 0f))
		{
			return;
		}
		zoomFactor = ((cam.orthographicSize > 45f) ? 30f : 6f) * PlayerSettings.Instance.ZoomFactor;
		float num = Input.GetAxis("Mouse ScrollWheel");
		if (SteamManager.IsSteamReady && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			num *= 10f;
		}
		float num2 = num;
		if (PlayerSettings.Instance.InvertedMouseSroll)
		{
			num2 *= -1f;
		}
		if (Mathf.Approximately(num2, 0f))
		{
			if (Input.GetKey(PlayerSettings.Instance.ZoomOutCamera))
			{
				num2 = -0.01f;
			}
			else if (Input.GetKey(PlayerSettings.Instance.ZoomInCamera))
			{
				num2 = 0.01f;
			}
		}
		targetZoom -= num2 * zoomFactor;
		targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
		cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.unscaledDeltaTime * zoomLerpSpeed);
		if (PlayerSettings.Instance.MoveCameraToMousePosWhenZooming && num2 != 0f)
		{
			transform.position = Vector3.Lerp(transform.position, cam.ScreenToWorldPoint(Input.mousePosition), Time.unscaledDeltaTime * 8f);
		}
	}

	private void CheckKeyboardInput()
	{
		if (!UIManager.isUIOpen && !Input.GetKey(KeyCode.LeftControl) && (!EventSystem.current.currentSelectedGameObject || (!EventSystem.current.currentSelectedGameObject.TryGetComponent<InputField>(out var _) && !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _))))
		{
			camsize = cam.orthographicSize;
			camSpeed = camsize * camSpeedMultiplier;
			if (Input.GetKey(PlayerSettings.Instance.MoveCameraUp))
			{
				transform.position += new Vector3(0f, camSpeed, 0f) * Time.unscaledDeltaTime;
			}
			if (Input.GetKey(PlayerSettings.Instance.MoveCameraDown))
			{
				transform.position += new Vector3(0f, 0f - camSpeed, 0f) * Time.unscaledDeltaTime;
			}
			if (Input.GetKey(PlayerSettings.Instance.MoveCameraRight))
			{
				transform.position += new Vector3(camSpeed, 0f, 0f) * Time.unscaledDeltaTime;
			}
			if (Input.GetKey(PlayerSettings.Instance.MoveCameraLeft))
			{
				transform.position += new Vector3(0f - camSpeed, 0f, 0f) * Time.unscaledDeltaTime;
			}
		}
	}

	private void LimitPosition()
	{
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, limitXMINUS, limitXPLUS), Mathf.Clamp(transform.position.y, limitYMINUS, limitYPLUS), -80f);
	}

	private void CheckMouseInput()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			return;
		}
		if (!MainMenu.isEditor && DrawingManager.instance.IsErasingOrDrawing() && PlayerSettings.Instance.MoveCameraWithLeftClick)
		{
			_panning = false;
			return;
		}
		if (PlayerSettings.Instance.MoveCameraWithLeftClick && Utils.IsPointerOverUIObject())
		{
			_panning = false;
			return;
		}
		if (!_panning && ((!PlayerSettings.Instance.MoveCameraWithLeftClick && Input.GetMouseButton(2)) || (PlayerSettings.Instance.MoveCameraWithLeftClick && Input.GetMouseButton(0))))
		{
			_mouseClickPos = cam.ScreenToWorldPoint(Input.mousePosition);
			_panning = true;
		}
		if (_panning)
		{
			_mouseCurrentPos = cam.ScreenToWorldPoint(Input.mousePosition);
			Vector2 vector = _mouseCurrentPos - _mouseClickPos;
			transform.position += new Vector3(0f - vector.x, 0f - vector.y, 0f);
		}
		if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2))
		{
			_panning = false;
		}
	}

	private IEnumerator CR_StartUnitsBlink()
	{
		if (_isCRrunning || !PlayerSettings.Instance.BlinkInactiveUnits || TurnManager.currPlayer.IsComputer || (PhotonNetwork.InRoom && !TurnManager.currPlayer.IsLocalClientPlayer()))
		{
			yield break;
		}
		_isCRrunning = true;
		float flashLength = 0.65f;
		while (true)
		{
			for (float t = 0f; t < flashLength; t += Time.unscaledDeltaTime)
			{
				float t2 = t / flashLength;
				for (int i = 0; i < TurnManager.currPlayer.ListOfUnits.Count; i++)
				{
					if (TurnManager.currPlayer.ListOfUnits[i].ActionPoints != 0 && TurnManager.currPlayer.ListOfUnits[i].NumberOfTimesTheUnitMoved == 0 && !TurnManager.currPlayer.ListOfUnits[i].IsMine && !TurnManager.currPlayer.ListOfUnits[i].IsArmourObstacle && ((TurnManager.currPlayer.ListOfUnits[i].CurrMP != 0 && (!TurnManager.currPlayer.ListOfUnits[i].HasMoved || TurnManager.currPlayer.ListOfUnits[i].IsRecon)) || !TurnManager.currPlayer.ListOfUnits[i].HasAttacked) && TurnManager.currPlayer.ListOfUnits[i].AsleepState == AsleepStates.NONE && (bool)TurnManager.currPlayer.ListOfUnits[i].unitGO && (bool)TurnManager.currPlayer.ListOfUnits[i].unitGO.tileGO && !TurnManager.currPlayer.ListOfUnits[i].unitGO.tileGO.isInFogOfWar)
					{
						if (!_isCRrunning)
						{
							yield break;
						}
						TurnManager.currPlayer.ListOfUnits[i].unitGO.backgroundSprite.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t2);
						TurnManager.currPlayer.ListOfUnits[i].unitGO.unitSprite.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t2);
					}
				}
				yield return null;
			}
			for (float t = 0f; t < flashLength; t += Time.unscaledDeltaTime)
			{
				float t2 = t / flashLength;
				for (int j = 0; j < TurnManager.currPlayer.ListOfUnits.Count; j++)
				{
					if (TurnManager.currPlayer.ListOfUnits[j].ActionPoints != 0 && TurnManager.currPlayer.ListOfUnits[j].NumberOfTimesTheUnitMoved == 0 && !TurnManager.currPlayer.ListOfUnits[j].IsMine && !TurnManager.currPlayer.ListOfUnits[j].IsArmourObstacle && ((TurnManager.currPlayer.ListOfUnits[j].CurrMP != 0 && (!TurnManager.currPlayer.ListOfUnits[j].HasMoved || TurnManager.currPlayer.ListOfUnits[j].IsRecon)) || !TurnManager.currPlayer.ListOfUnits[j].HasAttacked) && TurnManager.currPlayer.ListOfUnits[j].AsleepState == AsleepStates.NONE && (bool)TurnManager.currPlayer.ListOfUnits[j].unitGO && (bool)TurnManager.currPlayer.ListOfUnits[j].unitGO.tileGO && !TurnManager.currPlayer.ListOfUnits[j].unitGO.tileGO.isInFogOfWar)
					{
						if (!_isCRrunning)
						{
							yield break;
						}
						TurnManager.currPlayer.ListOfUnits[j].unitGO.backgroundSprite.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
						TurnManager.currPlayer.ListOfUnits[j].unitGO.unitSprite.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, t2);
					}
				}
				yield return null;
			}
		}
	}

	private void StopUnitsBlink()
	{
		if (!_isCRrunning || !PlayerSettings.Instance.BlinkInactiveUnits)
		{
			return;
		}
		if (_isCRrunning)
		{
			StopCoroutine(CR_StartUnitsBlink());
		}
		_isCRrunning = false;
		for (int i = 0; i < TurnManager.currPlayer.ListOfUnits.Count; i++)
		{
			if ((bool)TurnManager.currPlayer.ListOfUnits[i].unitGO)
			{
				TurnManager.currPlayer.ListOfUnits[i].unitGO.UpdateCounter();
			}
		}
	}

	private void JumpUpSideMap()
	{
		Vector3 position = GameData.Instance.map.TilesTable[0, GameData.Instance.map.SizeY - 1].tileGO.transform.position;
		transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		LimitPosition();
	}

	private void JumpDownSideMap()
	{
		Vector3 position = GameData.Instance.map.TilesTable[0, 0].tileGO.transform.position;
		transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
		LimitPosition();
	}

	private void JumpLeftSideMap()
	{
		Vector3 position = GameData.Instance.map.TilesTable[0, GameData.Instance.map.SizeY - 1].tileGO.transform.position;
		transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		LimitPosition();
	}

	private void JumpRightSideMap()
	{
		Vector3 position = GameData.Instance.map.TilesTable[GameData.Instance.map.SizeX - 1, GameData.Instance.map.SizeY - 1].tileGO.transform.position;
		transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
		LimitPosition();
	}

	public bool IsWithinFOV(Vector3 p_worldPoint)
	{
		Vector3 position = p_worldPoint;
		position.z = 0f;
		Vector3 vector = cam.WorldToViewportPoint(position);
		if (vector.x >= 0f && vector.x <= 1f && vector.y >= 0f)
		{
			return vector.y <= 1f;
		}
		return false;
	}

	public void MoveCameraToPosition(Vector3 p_targetPosition)
	{
		StartCoroutine(CR_LerpFromTo(transform.position, p_targetPosition, 0.3f));
	}

	private IEnumerator CR_LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(pos1, pos2, t / duration);
			yield return 0;
		}
		transform.position = new Vector3(pos2.x, pos2.y, -80f);
	}
}
