using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewMinimap : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public Camera minimapCamera;

	public RawImage RawImage;

	public RenderTexture texture;

	public SpriteRenderer rectangle;

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

	private bool _isMouseOverMiniMap;

	private bool _dragging;

	private float _clickDownTime;

	private float _clickUpTime;

	private void Awake()
	{
		minimapCamera.transform.position = new Vector3((float)GameData.Instance.map.SizeX * 2.2f, (float)GameData.Instance.map.SizeY * 2.53433f, -100f);
		float num;
		if (GameData.Instance.map.SizeX > GameData.Instance.map.SizeY)
		{
			num = _camSize_width.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeX));
		}
		else
		{
			num = ((GameData.Instance.map.SizeY <= GameData.Instance.map.SizeX) ? _camSize_width.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeX)) : _camSize_height.Evaluate(Mathf.InverseLerp(_minMapSize, _maxMapSize, GameData.Instance.map.SizeY)));
		}
		num = Mathf.Clamp(minimapCamera.orthographicSize * _maxZoomLevel * num, _minZoomLevel, _maxZoomLevel);
		minimapCamera.orthographicSize = num;
		minimapCamera.gameObject.SetActive(value: true);
		RawImage.rectTransform.sizeDelta = new Vector2(64f, 64f);
		RawImage.rectTransform.offsetMax = Vector3.zero;
		RawImage.rectTransform.offsetMin = Vector3.zero;
	}

	private void OnEnable()
	{
		minimapCamera.gameObject.SetActive(value: true);
	}

	private void OnDisable()
	{
		minimapCamera.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		rectangle.size = new Vector2(Camera.main.orthographicSize * 3.3f, Camera.main.orthographicSize * 2f);
		if (!_isMouseOverMiniMap)
		{
			return;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (minimapCamera.orthographicSize > 45f)
			{
				minimapCamera.orthographicSize -= axis * 15f;
			}
			else
			{
				minimapCamera.orthographicSize -= axis * 5f;
			}
			minimapCamera.orthographicSize = Mathf.Clamp(minimapCamera.orthographicSize, 15f, 500f);
		}
		if (_dragging)
		{
			if (minimapCamera.orthographicSize > 40f)
			{
				minimapCamera.transform.position += new Vector3(Input.GetAxis("Mouse X") * 10f * -1f, Input.GetAxis("Mouse Y") * 10f * -1f, 0f);
			}
			else if (minimapCamera.orthographicSize > 20f)
			{
				minimapCamera.transform.position += new Vector3(Input.GetAxis("Mouse X") * 5f * -1f, Input.GetAxis("Mouse Y") * 10f * -1f, 0f);
			}
			else
			{
				minimapCamera.transform.position += new Vector3(Input.GetAxis("Mouse X") * 2f * -1f, Input.GetAxis("Mouse Y") * 2f * -1f, 0f);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isMouseOverMiniMap = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isMouseOverMiniMap = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_isMouseOverMiniMap)
		{
			_dragging = true;
		}
		else
		{
			_dragging = false;
		}
		_clickDownTime = Time.time;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_dragging = false;
		_clickUpTime = Time.time - _clickDownTime;
		if (_clickUpTime < 0.25f && RectTransformUtility.ScreenPointToLocalPointInRectangle(RawImage.rectTransform, eventData.position, null, out var localPoint))
		{
			Rect rect = RawImage.rectTransform.rect;
			localPoint.x = localPoint.x / rect.width + RawImage.rectTransform.pivot.x;
			localPoint.y = localPoint.y / rect.height + RawImage.rectTransform.pivot.y;
			Ray ray = minimapCamera.ViewportPointToRay(localPoint);
			new Plane(Vector3.up, Vector3.zero).Raycast(ray, out var enter);
			Vector3 point = ray.GetPoint(enter);
			StartCoroutine(LerpFromTo(Camera.main.transform.position, new Vector3(point.x, point.y, Camera.main.transform.position.z), 0.3f));
		}
	}

	private IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			Camera.main.transform.position = Vector3.Lerp(pos1, pos2, t / duration);
			yield return 0;
		}
		Camera.main.transform.position = new Vector3(pos2.x, pos2.y, -80f);
	}
}
