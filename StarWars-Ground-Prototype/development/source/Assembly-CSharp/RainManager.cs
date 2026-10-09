using UnityEngine;

public class RainManager : MonoBehaviour
{
	public static RainManager instance;

	[Header("Setup")]
	[SerializeField]
	private string _shaderResourcePath = "RainOverlay";

	[SerializeField]
	private float _overlayZPosition = -49f;

	[SerializeField]
	private float _viewportPadding = 0.25f;

	[Header("Rain")]
	[SerializeField]
	private Color _rainColor = new Color(0.7f, 0.75f, 0.82f, 0.3f);

	[SerializeField]
	private float _intensity = 0.35f;

	[SerializeField]
	private float _windAngle = -0.5f;

	[SerializeField]
	private float _speed = 55f;

	[SerializeField]
	private float _streakLength = 1f;

	[SerializeField]
	private float _streakWidth = 0.1f;

	[SerializeField]
	private float _columnDensity = 1f;

	[SerializeField]
	private float _colorAlpha = 0.25f;

	[Header("Vignette")]
	[SerializeField]
	private Color _vignetteColor = new Color(0.05f, 0.07f, 0.1f, 1f);

	[SerializeField]
	private float _vignetteSmoothness = 0.5f;

	[SerializeField]
	private float _vignetteIntensity = 0.75f;

	[Header("Transition")]
	[SerializeField]
	private float _weatherTransitionDuration = 5f;

	[SerializeField]
	private float _rainShowLimitCamSize = 150f;

	private Camera _camera;

	private ProceduralRainOverlay _overlay;

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
			RefreshRain();
		}
	}

	private void OnDisable()
	{
		SetOverlayVisibility(p_isVisible: false);
	}

	private void RefreshRain()
	{
		bool flag = TurnManager.isBadWeather && GameData.Instance != null && GameData.Instance.map != null && !GameData.Instance.map.IsWinter;
		bool flag2 = flag && PlayerSettings.Instance.IsWeatherParticles && _camera.orthographicSize <= _rainShowLimitCamSize;
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
		Color rainColor = _rainColor;
		rainColor.a = weatherBlendCurrent * _colorAlpha;
		float num3 = 1f;
		if (_camera.orthographicSize > _rainShowLimitCamSize * 0.7f)
		{
			num3 = 1f - Mathf.InverseLerp(_rainShowLimitCamSize * 0.7f, _rainShowLimitCamSize, _camera.orthographicSize);
		}
		num *= num3;
		num2 *= num3;
		_overlay.Sync(_camera, num, rainColor, _windAngle, _speed, _streakLength, _streakWidth, _columnDensity, num2, _vignetteColor, _vignetteSmoothness);
	}

	private void UpdateWeatherBlend(bool p_isRainWeather)
	{
		float num = (p_isRainWeather ? 1f : 0f);
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
				Debug.LogError("RainManager could not find shader resource '" + _shaderResourcePath + "'.");
				_loggedMissingShader = true;
			}
			return false;
		}
		GameObject gameObject = new GameObject("Procedural Rain Overlay");
		gameObject.transform.SetParent(transform, worldPositionStays: false);
		gameObject.layer = base.gameObject.layer;
		_overlayMaterial = new Material(shader);
		_overlayMaterial.name = "Procedural Rain Overlay (Runtime)";
		_overlay = gameObject.AddComponent<ProceduralRainOverlay>();
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
