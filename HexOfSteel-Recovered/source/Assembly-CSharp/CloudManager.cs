using System;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
	public static CloudManager instance;

	private const float SECONDARY_SPEED_MULTIPLIER = 1.65f;

	[Header("Setup")]
	[SerializeField]
	private AnimationCurve _opacityCurve;

	[SerializeField]
	private float _cloudShowLimitCamSize = 500f;

	[Header("Procedural Overlay")]
	[SerializeField]
	private string _shaderResourcePath = "ProceduralCloudOverlay";

	[SerializeField]
	private float _overlayZPosition = -50f;

	[SerializeField]
	private float _viewportPadding = 0.15f;

	[SerializeField]
	private float _opacity = 0.15f;

	[Header("Render Scale")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _desktopTargetRenderScale = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _mobileTargetRenderScale = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _currentTargetRenderScale = 1f;

	[SerializeField]
	private Vector2 _windDirection = new Vector2(-1f, 0f);

	[SerializeField]
	private Color _cloudColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color _shadowColor = new Color(0f, 0f, 0f, 1f);

	[SerializeField]
	private Vector2 _shadowDirection = new Vector2(1f, -1f);

	[SerializeField]
	private float _shadowOffset = 15f;

	[SerializeField]
	private float _shadowSoftness = 0.1f;

	[SerializeField]
	private float _shadowStrength = 1f;

	[SerializeField]
	private float _innerShadowStrength = 0.1f;

	[SerializeField]
	private float _shadowOpacity = 1f;

	[SerializeField]
	private float _clearWeatherAmount = 0.65f;

	[SerializeField]
	private float _badWeatherAmount = 0.75f;

	[SerializeField]
	private float _amountScale = 50f;

	[SerializeField]
	private float _clearWeatherCoverage = 0.5f;

	[SerializeField]
	private float _badWeatherCoverage = 0.75f;

	[SerializeField]
	private float _clearWeatherSpeed = 0.15f;

	[SerializeField]
	private float _badWeatherSpeed = 0.25f;

	[SerializeField]
	private float _weatherTransitionDuration = 5f;

	[SerializeField]
	private float _baseScale = 100f;

	[SerializeField]
	private float _detailScale = 50f;

	[SerializeField]
	private float _warpScale = 35f;

	[SerializeField]
	private float _softness = 0.75f;

	[SerializeField]
	private float _detailStrength = 1f;

	[SerializeField]
	private float _warpStrength = 0.5f;

	[SerializeField]
	private float _textureScale = 25f;

	[SerializeField]
	private float _textureStrength = 1f;

	[Header("Debug")]
	[SerializeField]
	private bool _showRenderScaleDebugInfo;

	private Camera _camera;

	private ProceduralCloudOverlay _overlay;

	private Material _overlayMaterial;

	private bool _isOverlayVisible;

	private bool _loggedMissingShader;

	private Vector4 _seed;

	private float _weatherBlendCurrent;

	private bool _weatherInitialized;

	private float _accumulatedPrimaryTime;

	private float _accumulatedSecondaryTime;

	private void Awake()
	{
		instance = this;
		ApplyPlatformRenderScale();
	}

	private void Start()
	{
		_camera = ((CameraGO.instance != null) ? CameraGO.instance.cam : Camera.main);
		BuildSeed();
		RefreshClouds(p_forceRefresh: true);
	}

	private void OnEnable()
	{
		RefreshClouds(p_forceRefresh: true);
	}

	private void OnDisable()
	{
		SetOverlayVisibility(p_isVisible: false);
	}

	private void Update()
	{
		if (_camera == null)
		{
			_camera = ((CameraGO.instance != null) ? CameraGO.instance.cam : Camera.main);
		}
		if (!(_camera == null))
		{
			RefreshClouds(p_forceRefresh: false);
		}
	}

	private void RefreshClouds(bool p_forceRefresh)
	{
		if (_camera == null)
		{
			_camera = ((CameraGO.instance != null) ? CameraGO.instance.cam : Camera.main);
		}
		if (_camera == null)
		{
			return;
		}
		float num = Mathf.Clamp01(_currentTargetRenderScale);
		bool flag = PlayerSettings.Instance.ShowClouds && _camera.orthographicSize <= _cloudShowLimitCamSize && num > 0f;
		if (!flag && _overlay == null)
		{
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		if (!EnsureOverlay())
		{
			SetOverlayVisibility(p_isVisible: false);
			return;
		}
		_overlay.SetDebugInfoEnabled(_showRenderScaleDebugInfo);
		_overlay.UpdateDebugSnapshot(_camera, num);
		if (p_forceRefresh || flag != _isOverlayVisible)
		{
			SetOverlayVisibility(flag);
		}
		if (flag)
		{
			float time = ((!(CameraGO.instance != null)) ? Mathf.InverseLerp(5f, 1000f, _camera.orthographicSize) : Mathf.InverseLerp(CameraGO.instance.minZoom, CameraGO.instance.maxZoom, _camera.orthographicSize));
			float p_opacity = ((_opacityCurve != null) ? _opacityCurve.Evaluate(time) : _opacity);
			float num2 = (TurnManager.isBadWeather ? 1f : 0f);
			if (!_weatherInitialized)
			{
				_weatherBlendCurrent = num2;
				_weatherInitialized = true;
			}
			else
			{
				float num3 = ((_weatherTransitionDuration > 0f) ? (1f / _weatherTransitionDuration) : 100f);
				float num4 = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
				_weatherBlendCurrent = Mathf.MoveTowards(_weatherBlendCurrent, num2, num3 * num4);
			}
			float p_cloudAmount = Mathf.Lerp(_clearWeatherAmount, _badWeatherAmount, _weatherBlendCurrent);
			float p_coverage = Mathf.Lerp(_clearWeatherCoverage, _badWeatherCoverage, _weatherBlendCurrent);
			float num5 = Mathf.Lerp(_clearWeatherSpeed, _badWeatherSpeed, _weatherBlendCurrent);
			float num6 = Mathf.Min(Time.deltaTime, 0.05f);
			_accumulatedPrimaryTime += num5 * num6;
			_accumulatedSecondaryTime += num5 * 1.65f * num6;
			_overlay.Sync(_camera, p_opacity, p_cloudAmount, p_coverage, _weatherBlendCurrent, _windDirection, _shadowDirection, _accumulatedPrimaryTime, _accumulatedSecondaryTime, _amountScale, _baseScale, _detailScale, _textureScale, _softness, _detailStrength, _textureStrength, _warpScale, _warpStrength, _shadowOffset, _shadowSoftness, _shadowStrength, _innerShadowStrength, _shadowOpacity, _cloudColor, _shadowColor, num, _seed);
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
				Debug.LogError("CloudManager could not find shader resource '" + _shaderResourcePath + "'.");
				_loggedMissingShader = true;
			}
			return false;
		}
		GameObject gameObject = new GameObject("Procedural Cloud Overlay");
		gameObject.transform.SetParent(transform, worldPositionStays: false);
		gameObject.layer = base.gameObject.layer;
		_overlayMaterial = new Material(shader);
		_overlayMaterial.name = "Procedural Cloud Overlay (Runtime)";
		_overlay = gameObject.AddComponent<ProceduralCloudOverlay>();
		_overlay.Setup(_overlayMaterial, base.gameObject.layer, _overlayZPosition, _viewportPadding);
		_overlay.SetDebugInfoEnabled(_showRenderScaleDebugInfo);
		_overlay.UpdateDebugSnapshot(_camera, Mathf.Clamp01(_currentTargetRenderScale));
		return true;
	}

	private void ApplyPlatformRenderScale()
	{
		float value = (Application.isMobilePlatform ? _mobileTargetRenderScale : _desktopTargetRenderScale);
		_currentTargetRenderScale = Mathf.Clamp01(value);
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

	private void BuildSeed()
	{
		int seed = 17;
		if (GameData.Instance != null && GameData.Instance.map != null)
		{
			seed = (GameData.Instance.map.SizeX * 73856093) ^ (GameData.Instance.map.SizeY * 19349663) ^ (GameData.Instance.map.turnCount * 83492791);
		}
		System.Random p_random = new System.Random(seed);
		_seed = new Vector4(GetSeedComponent(p_random), GetSeedComponent(p_random), GetSeedComponent(p_random), GetSeedComponent(p_random));
	}

	private float GetSeedComponent(System.Random p_random)
	{
		return (float)p_random.NextDouble() * 100f;
	}
}
