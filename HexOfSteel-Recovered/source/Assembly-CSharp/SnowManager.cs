using UnityEngine;

public class SnowManager : MonoBehaviour
{
	public static SnowManager instance;

	[Header("Setup")]
	[SerializeField]
	private string _shaderResourcePath = "SnowOverlay";

	[SerializeField]
	private float _overlayZPosition = -48f;

	[SerializeField]
	private float _viewportPadding = 0.15f;

	[Header("Snow")]
	[SerializeField]
	private Color _snowColor = new Color(0.9f, 0.92f, 0.95f, 0.35f);

	[SerializeField]
	private float _intensity = 0.4f;

	[SerializeField]
	private float _windAngle = -0.55f;

	[SerializeField]
	private float _speed = 20f;

	[SerializeField]
	private float _flakeSize = 0.25f;

	[SerializeField]
	private float _drift = 0.3f;

	[SerializeField]
	private float _columnDensity = 1f;

	[Header("Fog / Whiteout")]
	[SerializeField]
	private float _fogIntensity = 0.05f;

	[Header("Vignette")]
	[SerializeField]
	private Color _vignetteColor = new Color(0.08f, 0.1f, 0.15f, 1f);

	[SerializeField]
	private float _vignetteSmoothness = 0.35f;

	[SerializeField]
	private float _vignetteIntensity = 0.6f;

	[Header("Transition")]
	[SerializeField]
	private float _weatherTransitionDuration = 5f;

	[SerializeField]
	private float _snowShowLimitCamSize = 150f;

	private Camera _camera;

	private ProceduralSnowOverlay _overlay;

	private Material _overlayMaterial;

	private bool _isOverlayVisible;

	private bool _loggedMissingShader;

	private float _weatherBlendCurrent;

	private bool _weatherInitialized;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		_camera = ((CameraGO.instance != null) ? CameraGO.instance.cam : Camera.main);
	}

	private void Update()
	{
		if (_camera == null)
		{
			_camera = ((CameraGO.instance != null) ? CameraGO.instance.cam : Camera.main);
		}
		if (!(_camera == null))
		{
			RefreshSnow();
		}
	}

	private void OnDisable()
	{
		SetOverlayVisibility(p_isVisible: false);
	}

	private void RefreshSnow()
	{
		bool flag = TurnManager.isBadWeather && GameData.Instance != null && GameData.Instance.map != null && GameData.Instance.map.IsWinter;
		bool flag2 = flag && PlayerSettings.Instance.IsWeatherParticles && _camera.orthographicSize <= _snowShowLimitCamSize;
		if (!flag2 && _overlay == null)
		{
			UpdateWeatherBlend(flag);
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		if (!EnsureOverlay())
		{
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		UpdateWeatherBlend(flag);
		if (_weatherBlendCurrent < 0.001f)
		{
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		if (!flag2 && _weatherBlendCurrent < 0.001f)
		{
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		SetOverlayVisibility(p_isVisible: true);
		float weatherBlendCurrent = _weatherBlendCurrent;
		float num = weatherBlendCurrent * _intensity;
		float num2 = weatherBlendCurrent * _vignetteIntensity;
		float num3 = weatherBlendCurrent * _fogIntensity;
		Color snowColor = _snowColor;
		snowColor.a = weatherBlendCurrent * _snowColor.a;
		float num4 = 1f;
		if (_camera.orthographicSize > _snowShowLimitCamSize * 0.7f)
		{
			num4 = 1f - Mathf.InverseLerp(_snowShowLimitCamSize * 0.7f, _snowShowLimitCamSize, _camera.orthographicSize);
		}
		num *= num4;
		num2 *= num4;
		num3 *= num4;
		_overlay.Sync(_camera, num, snowColor, _windAngle, _speed, _flakeSize, _drift, _columnDensity, num3, num2, _vignetteColor, _vignetteSmoothness);
	}

	private void UpdateWeatherBlend(bool p_isSnowWeather)
	{
		float num = (p_isSnowWeather ? 1f : 0f);
		if (!_weatherInitialized)
		{
			_weatherBlendCurrent = num;
			_weatherInitialized = true;
		}
		else
		{
			float num2 = ((_weatherTransitionDuration > 0f) ? (1f / _weatherTransitionDuration) : 100f);
			float num3 = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
			_weatherBlendCurrent = Mathf.MoveTowards(_weatherBlendCurrent, num, num2 * num3);
		}
	}

	private bool EnsureOverlay()
	{
		if (_overlay != null)
		{
			return true;
		}
		Shader shader = Resources.Load<Shader>(_shaderResourcePath);
		if (shader == null)
		{
			if (!_loggedMissingShader)
			{
				Debug.LogError("SnowManager could not find shader resource '" + _shaderResourcePath + "'.");
				_loggedMissingShader = true;
			}
			return false;
		}
		GameObject gameObject = new GameObject("Procedural Snow Overlay");
		gameObject.transform.SetParent(transform, worldPositionStays: false);
		gameObject.layer = base.gameObject.layer;
		_overlayMaterial = new Material(shader);
		_overlayMaterial.name = "Procedural Snow Overlay (Runtime)";
		_overlay = gameObject.AddComponent<ProceduralSnowOverlay>();
		_overlay.Setup(_overlayMaterial, base.gameObject.layer, _overlayZPosition, _viewportPadding);
		return true;
	}

	private void SetOverlayVisibility(bool p_isVisible)
	{
		if (_overlay == null)
		{
			_isOverlayVisible = false;
			return;
		}
		_overlay.SetVisible(p_isVisible);
		_isOverlayVisible = p_isVisible;
	}
}
