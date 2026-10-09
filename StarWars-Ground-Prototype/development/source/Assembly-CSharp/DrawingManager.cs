using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
	public static DrawingManager instance;

	private const float MIN_DISTANCE_THRESHOLD = 0.5f;

	[Header("Setup")]
	[SerializeField]
	private Transform _parent;

	[SerializeField]
	private Transform _parent_panel;

	[SerializeField]
	private GameObject _contentPanel;

	[SerializeField]
	private Line _line_prefab;

	[SerializeField]
	private Button _draw_button;

	[SerializeField]
	private Button _erase_button;

	[SerializeField]
	private TextMeshProUGUI _width_text;

	[SerializeField]
	private FlexibleColorPicker _flexibleColorPicker;

	[Header("DEBUG/INFOS")]
	[SerializeField]
	private Line _activeLine;

	[SerializeField]
	private float _width = 0.3f;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private List<Line> _spawnedLines = new List<Line>();

	[SerializeField]
	private bool _processDrawing;

	[SerializeField]
	private bool _processErasing;

	private List<Vector2> _erasingPath = new List<Vector2>();

	private Camera _camera;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		_camera = CameraGO.instance.cam;
		_width = 0.3f;
		_color = Color.white;
		_width_text.text = _width.ToString();
		if (!PlayerSettings.Instance.ShowInfoTile)
		{
			Vector3 position = _parent_panel.transform.position;
			position.x = 0f;
			_parent_panel.transform.position = position;
		}
		_flexibleColorPicker.SetColor(Color.red);
		SpawnLinesForPlayer(TurnManager.currPlayer);
	}

	private void Update()
	{
		if (!MapGO.IsAIOnly && (TurnManager.currPlayer == null || TurnManager.currPlayer.IsComputer))
		{
			if (_activeLine != null)
			{
				_activeLine = null;
			}
			return;
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.SelectPen))
		{
			SetDrawMode();
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.SelectEraser))
		{
			SetEraseMode();
		}
		if (!_processDrawing && !_processErasing)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			StopDrawing();
			StopErasing();
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (_processDrawing)
			{
				StartDrawing();
			}
			else if (_processErasing)
			{
				StartErasing();
			}
		}
		if (_processDrawing && Input.GetMouseButtonDown(1))
		{
			SetEraseMode();
		}
		if (_processDrawing)
		{
			UpdateDrawing();
		}
		if ((_processErasing && Input.GetMouseButton(0)) || Input.GetMouseButton(1))
		{
			UpdateErasing();
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (_processDrawing)
			{
				StopDrawing();
			}
			StopErasing();
		}
		if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftMeta)) && Input.GetKeyDown(KeyCode.Z) && (_processDrawing || _processErasing))
		{
			RemovePreviousLine();
		}
	}

	public void SetDrawMode()
	{
		if (!TurnManager.currPlayer.IsComputer)
		{
			SoundManager.instance.PlayUIClickSound();
			_processDrawing = !_processDrawing;
			_processErasing = false;
			_contentPanel.SetActive(_processDrawing || _processErasing);
			UpdateButtonsColors();
		}
	}

	public void SetEraseMode()
	{
		if (!TurnManager.currPlayer.IsComputer)
		{
			SoundManager.instance.PlayUIClickSound();
			_processErasing = !_processErasing;
			_processDrawing = false;
			_contentPanel.SetActive(_processDrawing || _processErasing);
			UpdateButtonsColors();
		}
	}

	public void IncreaseWidth()
	{
		_width += 0.1f;
		_width = Mathf.Clamp(_width, 0.1f, 1f);
		_width_text.text = Utils.Truncate(_width, 1).ToString();
	}

	public void DecreaseWidth()
	{
		_width -= 0.1f;
		_width = Mathf.Clamp(_width, 0.1f, 1f);
		_width_text.text = Utils.Truncate(_width, 1).ToString();
	}

	public bool IsErasingOrDrawing()
	{
		if (!_processDrawing)
		{
			return _processErasing;
		}
		return true;
	}

	public void SpawnLinesForPlayer(Player p_player)
	{
		Line[] componentsInChildren = _parent.GetComponentsInChildren<Line>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		_spawnedLines.Clear();
		_contentPanel.SetActive(value: false);
		_processDrawing = false;
		_processErasing = false;
		UpdateButtonsColors();
		if (p_player.IsComputer)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.IsLocalClientPlayer() && listOfPlayer.IsAlliedWith(p_player))
				{
					flag = true;
					flag2 = true;
				}
			}
		}
		else if (TurnManager.currPlayer.IsAlliedWith(p_player))
		{
			flag2 = true;
		}
		if (!flag2)
		{
			return;
		}
		foreach (LineData lineData in p_player.LineDatas)
		{
			Line line = Object.Instantiate(_line_prefab, _parent);
			line.GenerateLineFromLineData(lineData);
			_spawnedLines.Add(line);
		}
		if (!flag)
		{
			return;
		}
		foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
		{
			if (listOfPlayer2 == p_player || !p_player.IsAlliedWith(listOfPlayer2))
			{
				continue;
			}
			foreach (LineData lineData2 in listOfPlayer2.LineDatas)
			{
				Line line2 = Object.Instantiate(_line_prefab, _parent);
				line2.GenerateLineFromLineData(lineData2);
				_spawnedLines.Add(line2);
			}
		}
	}

	private void StartDrawing()
	{
		if (!Utils.IsPointerOverUIObject())
		{
			_activeLine = Object.Instantiate(_line_prefab, _parent);
			_color = _flexibleColorPicker.color;
			_activeLine.Setup(_color, _width);
			_spawnedLines.Add(_activeLine);
		}
	}

	private void UpdateDrawing()
	{
		if (!(_activeLine == null))
		{
			_activeLine.UpdateLine(_camera.ScreenToWorldPoint(Input.mousePosition));
		}
	}

	private void StopDrawing()
	{
		if (_activeLine == null)
		{
			return;
		}
		if (_activeLine.IsInvalidLine())
		{
			_spawnedLines.Remove(_activeLine);
			Object.Destroy(_activeLine.gameObject);
			_activeLine = null;
			return;
		}
		Player o_localPlayer;
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			GameData.Instance.TryFindLocalPlayer(out o_localPlayer);
		}
		else
		{
			o_localPlayer = TurnManager.humanPlayer;
		}
		o_localPlayer.AddLineData(_activeLine.GenerateLineData(), p_sendRPC: true);
		_activeLine = null;
	}

	private void RemovePreviousLine()
	{
		if (_spawnedLines.Count != 0)
		{
			Line line = _spawnedLines.Last();
			_spawnedLines.Remove(line);
			Object.Destroy(line.gameObject);
			Player o_localPlayer;
			if (MultiplayerManager.IsRealtimeMultiplayer())
			{
				GameData.Instance.TryFindLocalPlayer(out o_localPlayer);
			}
			else
			{
				o_localPlayer = TurnManager.humanPlayer;
			}
			LineData p_lineData = o_localPlayer.LineDatas.Last();
			o_localPlayer.RemoveLineData(p_lineData, p_sendRPC: true);
		}
	}

	private void StartErasing()
	{
		_erasingPath.Clear();
	}

	private void StopErasing()
	{
		_erasingPath.Clear();
	}

	private void UpdateErasing()
	{
		Vector2 vector = _camera.ScreenToWorldPoint(Input.mousePosition);
		if (_erasingPath.Count == 0)
		{
			_erasingPath.Add(vector);
		}
		else if (Vector2.Distance(vector, _erasingPath.Last()) > 0.5f)
		{
			CheckEraserIntersection(vector);
			_erasingPath.Add(vector);
		}
	}

	private void CheckEraserIntersection(Vector2 p_mousePosition)
	{
		Player o_localPlayer;
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			GameData.Instance.TryFindLocalPlayer(out o_localPlayer);
		}
		else
		{
			o_localPlayer = TurnManager.humanPlayer;
		}
		List<LineData> list = new List<LineData>();
		foreach (LineData lineData in o_localPlayer.LineDatas)
		{
			for (int i = 0; i < lineData.Points.Length; i++)
			{
				if (i < lineData.Points.Length - 1 && AreSegmentsIntersecting(p_mousePosition, _erasingPath.Last(), new Vector2(lineData.Points[i][0, 0], lineData.Points[i][0, 1]), new Vector2(lineData.Points[i + 1][0, 0], lineData.Points[i + 1][0, 1])))
				{
					list.Add(lineData);
				}
			}
		}
		foreach (LineData item in list)
		{
			o_localPlayer.RemoveLineData(item, p_sendRPC: true);
		}
	}

	public bool AreSegmentsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{
		Vector2 a = p2 - p1;
		Vector2 b = p3 - p1;
		Vector2 b2 = p4 - p1;
		Vector2 a2 = p4 - p3;
		Vector2 b3 = p1 - p3;
		Vector2 b4 = p2 - p3;
		float num = CrossProduct2D(a, b);
		float num2 = CrossProduct2D(a, b2);
		float num3 = CrossProduct2D(a2, b3);
		float num4 = CrossProduct2D(a2, b4);
		if (num * num2 < 0f && num3 * num4 < 0f)
		{
			return true;
		}
		return false;
	}

	private float CrossProduct2D(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	private void UpdateButtonsColors()
	{
		ColorBlock colors = _draw_button.colors;
		ColorBlock colors2 = _erase_button.colors;
		if (_processDrawing)
		{
			colors.normalColor = MainMenu.yellowColor;
			colors.selectedColor = MainMenu.yellowColor;
			colors2.normalColor = MainMenu.greyColor;
			colors2.selectedColor = MainMenu.greyColor;
		}
		else if (_processErasing)
		{
			colors.normalColor = MainMenu.greyColor;
			colors.selectedColor = MainMenu.greyColor;
			colors2.normalColor = MainMenu.yellowColor;
			colors2.selectedColor = MainMenu.yellowColor;
		}
		else
		{
			colors.normalColor = MainMenu.greyColor;
			colors.selectedColor = MainMenu.greyColor;
			colors2.normalColor = MainMenu.greyColor;
			colors2.selectedColor = MainMenu.greyColor;
		}
		_draw_button.colors = colors;
		_erase_button.colors = colors2;
	}
}
