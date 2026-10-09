using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Line : MonoBehaviour
{
	private const float MIN_DISTANCE_THRESHOLD = 0.25f;

	[Header("Setup")]
	[SerializeField]
	private LineRenderer _lineRenderer;

	[Header("DEBUG/INFOS")]
	[SerializeField]
	private float _width;

	[SerializeField]
	private Color _color;

	[SerializeField]
	private List<Vector2> _points;

	public void Setup(Color p_color, float p_width)
	{
		_width = p_width;
		_color = p_color;
		_lineRenderer.widthMultiplier = _width;
		_lineRenderer.material.color = _color;
	}

	public void UpdateLine(Vector2 p_position)
	{
		if (_points == null || _points.Count == 0)
		{
			_points = new List<Vector2>();
			SetPoint(p_position);
		}
		else if (Vector2.Distance(_points.Last(), p_position) > 0.25f)
		{
			SetPoint(p_position);
		}
	}

	private void SetPoint(Vector2 p_point)
	{
		_points.Add(p_point);
		_lineRenderer.positionCount = _points.Count;
		_lineRenderer.SetPosition(_points.Count - 1, new Vector3(p_point.x, p_point.y, -20f));
	}

	public bool IsInvalidLine()
	{
		return _lineRenderer.positionCount <= 1;
	}

	public void GenerateLineFromLineData(LineData p_lineData)
	{
		p_lineData.lineObject = gameObject;
		_points = new List<Vector2>();
		_width = (float)(int)p_lineData.Width / 100f;
		_color = new Color((float)(int)p_lineData.Color[0] / 255f, (float)(int)p_lineData.Color[1] / 255f, (float)(int)p_lineData.Color[2] / 255f);
		Setup(_color, _width);
		float[][,] points = p_lineData.Points;
		foreach (float[,] array in points)
		{
			Vector2 item = new Vector2(array[0, 0], array[0, 1]);
			_points.Add(item);
			_lineRenderer.positionCount = _points.Count;
			_lineRenderer.SetPosition(_points.Count - 1, new Vector3(item.x, item.y, -20f));
		}
	}

	public LineData GenerateLineData()
	{
		Vector3[] array = new Vector3[_lineRenderer.positionCount];
		_lineRenderer.GetPositions(array);
		LineData lineData = new LineData();
		lineData.lineObject = gameObject;
		lineData.Color = new byte[3]
		{
			(byte)(_color.r * 255f),
			(byte)(_color.g * 255f),
			(byte)(_color.b * 255f)
		};
		lineData.Width = (byte)(_width * 100f);
		lineData.Points = new float[_lineRenderer.positionCount][,];
		for (int i = 0; i < array.Length; i++)
		{
			lineData.Points[i] = new float[1, 2];
			lineData.Points[i][0, 0] = array[i].x;
			lineData.Points[i][0, 1] = array[i].y;
		}
		return lineData;
	}
}
