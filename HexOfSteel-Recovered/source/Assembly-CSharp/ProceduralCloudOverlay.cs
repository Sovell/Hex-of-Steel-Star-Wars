using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralCloudOverlay : MonoBehaviour
{
	private static readonly int _opacityID = Shader.PropertyToID("_Opacity");

	private static readonly int _cloudAmountID = Shader.PropertyToID("_CloudAmount");

	private static readonly int _coverageID = Shader.PropertyToID("_Coverage");

	private static readonly int _weatherBlendID = Shader.PropertyToID("_WeatherBlend");

	private static readonly int _windDirectionID = Shader.PropertyToID("_WindDirection");

	private static readonly int _shadowDirectionID = Shader.PropertyToID("_ShadowDirection");

	private static readonly int _primaryTimeID = Shader.PropertyToID("_PrimaryTime");

	private static readonly int _secondaryTimeID = Shader.PropertyToID("_SecondaryTime");

	private static readonly int _invAmountScaleID = Shader.PropertyToID("_InvAmountScale");

	private static readonly int _invBaseScaleID = Shader.PropertyToID("_InvBaseScale");

	private static readonly int _invDetailScaleID = Shader.PropertyToID("_InvDetailScale");

	private static readonly int _invTextureScaleID = Shader.PropertyToID("_InvTextureScale");

	private static readonly int _softnessID = Shader.PropertyToID("_Softness");

	private static readonly int _detailStrengthID = Shader.PropertyToID("_DetailStrength");

	private static readonly int _textureStrengthID = Shader.PropertyToID("_TextureStrength");

	private static readonly int _invWarpScaleID = Shader.PropertyToID("_InvWarpScale");

	private static readonly int _sideWindID = Shader.PropertyToID("_SideWind");

	private static readonly int _warpStrengthID = Shader.PropertyToID("_WarpStrength");

	private static readonly int _shadowOffsetID = Shader.PropertyToID("_ShadowOffset");

	private static readonly int _shadowSoftnessID = Shader.PropertyToID("_ShadowSoftness");

	private static readonly int _shadowStrengthID = Shader.PropertyToID("_ShadowStrength");

	private static readonly int _innerShadowStrengthID = Shader.PropertyToID("_InnerShadowStrength");

	private static readonly int _shadowOpacityID = Shader.PropertyToID("_ShadowOpacity");

	private static readonly int _cloudColorID = Shader.PropertyToID("_CloudColor");

	private static readonly int _shadowColorID = Shader.PropertyToID("_ShadowColor");

	private static readonly int _overlayCenterID = Shader.PropertyToID("_OverlayCenter");

	private static readonly int _overlaySizeID = Shader.PropertyToID("_OverlaySize");

	private static readonly int _useOverlayUVID = Shader.PropertyToID("_UseOverlayUV");

	private static readonly int _srcBlendID = Shader.PropertyToID("_SrcBlend");

	private static readonly int _dstBlendID = Shader.PropertyToID("_DstBlend");

	private static readonly int _seedID = Shader.PropertyToID("_Seed");

	private static readonly int _mainTexID = Shader.PropertyToID("_MainTex");

	private static Mesh _sharedQuadMesh;

	private MeshFilter _meshFilter;

	private MeshRenderer _meshRenderer;

	private Material _material;

	private Material _displayMaterial;

	private RenderTexture _renderTexture;

	private Transform _cachedTransform;

	private float _zPosition;

	private float _viewportPadding;

	private int _renderTextureWidth;

	private int _renderTextureHeight;

	private float _renderTextureScale = -1f;

	private bool _isUsingDownscaledPath;

	private bool _loggedMissingDisplayShader;

	private bool _showDebugInfo;

	private float _debugRenderScale = 1f;

	private int _debugSourceWidth;

	private int _debugSourceHeight;

	private int _debugTargetWidth;

	private int _debugTargetHeight;

	private GUIStyle _debugLabelStyle;

	private float _cachedOpacity = -1f;

	private float _cachedCloudAmount = -1f;

	private float _cachedCoverage = -1f;

	private float _cachedWeatherBlend = -1f;

	private Vector4 _cachedWindDirection = new Vector4(float.NaN, 0f, 0f, 0f);

	private Vector4 _cachedShadowDirection = new Vector4(float.NaN, 0f, 0f, 0f);

	private float _cachedPrimaryTime = -1f;

	private float _cachedSecondaryTime = -1f;

	private float _cachedInvAmountScale = -1f;

	private float _cachedInvBaseScale = -1f;

	private float _cachedInvDetailScale = -1f;

	private float _cachedInvTextureScale = -1f;

	private float _cachedSoftness = -1f;

	private float _cachedDetailStrength = -1f;

	private float _cachedTextureStrength = -1f;

	private float _cachedInvWarpScale = -1f;

	private Vector4 _cachedSideWind = new Vector4(float.NaN, 0f, 0f, 0f);

	private float _cachedWarpStrength = -1f;

	private float _cachedShadowOffset = -1f;

	private float _cachedShadowSoftness = -1f;

	private float _cachedShadowStrength = -1f;

	private float _cachedInnerShadowStrength = -1f;

	private float _cachedShadowOpacity = -1f;

	private Color _cachedCloudColor = new Color(-1f, 0f, 0f, 0f);

	private Color _cachedShadowColor = new Color(-1f, 0f, 0f, 0f);

	private Vector4 _cachedSeed = new Vector4(float.NaN, 0f, 0f, 0f);

	public void Setup(Material p_material, int p_layer, float p_zPosition, float p_viewportPadding)
	{
		_cachedTransform = transform;
		_material = p_material;
		_zPosition = p_zPosition;
		_viewportPadding = Mathf.Max(0f, p_viewportPadding);
		gameObject.layer = p_layer;
		_meshFilter = gameObject.GetComponent<MeshFilter>();
		if (_meshFilter == null)
		{
			_meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		_meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (_meshRenderer == null)
		{
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		if (_sharedQuadMesh == null)
		{
			_sharedQuadMesh = BuildQuadMesh();
		}
		_meshFilter.sharedMesh = _sharedQuadMesh;
		_meshRenderer.sharedMaterial = _material;
		_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		_meshRenderer.receiveShadows = false;
		_meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		_meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		_meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		_meshRenderer.allowOcclusionWhenDynamic = false;
		SetMaterialBlendMode(BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
		_material.SetFloat(_useOverlayUVID, 0f);
	}

	public void SetVisible(bool p_isVisible)
	{
		if (!(_meshRenderer == null) && _meshRenderer.enabled != p_isVisible)
		{
			_meshRenderer.enabled = p_isVisible;
		}
	}

	public void SetDebugInfoEnabled(bool p_isEnabled)
	{
		_showDebugInfo = p_isEnabled;
	}

	public void UpdateDebugSnapshot(Camera p_camera, float p_renderScale)
	{
		if (!(p_camera == null))
		{
			_debugSourceWidth = Mathf.Max(1, p_camera.pixelWidth);
			_debugSourceHeight = Mathf.Max(1, p_camera.pixelHeight);
			_debugRenderScale = Mathf.Clamp01(p_renderScale);
			if (_debugRenderScale <= 0f)
			{
				_debugTargetWidth = 0;
				_debugTargetHeight = 0;
			}
			else if (Mathf.Approximately(_debugRenderScale, 1f))
			{
				_debugTargetWidth = _debugSourceWidth;
				_debugTargetHeight = _debugSourceHeight;
			}
			else
			{
				_debugTargetWidth = Mathf.Max(1, Mathf.RoundToInt((float)_debugSourceWidth * _debugRenderScale));
				_debugTargetHeight = Mathf.Max(1, Mathf.RoundToInt((float)_debugSourceHeight * _debugRenderScale));
			}
		}
	}

	public void Sync(Camera p_camera, float p_opacity, float p_cloudAmount, float p_coverage, float p_weatherBlend, Vector2 p_windDirection, Vector2 p_shadowDirection, float p_primaryTime, float p_secondaryTime, float p_amountScale, float p_baseScale, float p_detailScale, float p_textureScale, float p_softness, float p_detailStrength, float p_textureStrength, float p_warpScale, float p_warpStrength, float p_shadowOffset, float p_shadowSoftness, float p_shadowStrength, float p_innerShadowStrength, float p_shadowOpacity, Color p_cloudColor, Color p_shadowColor, float p_renderScale, Vector4 p_seed)
	{
		if (!(p_camera == null) && !(_material == null))
		{
			UpdateTransform(p_camera);
			Vector2 vector = ((p_windDirection.sqrMagnitude > 0.0001f) ? p_windDirection.normalized : Vector2.right);
			Vector2 vector2 = ((p_shadowDirection.sqrMagnitude > 0.0001f) ? p_shadowDirection.normalized : new Vector2(1f, -0.65f).normalized);
			float num = Mathf.Clamp01(p_opacity);
			float num2 = Mathf.Clamp01(p_cloudAmount);
			float num3 = Mathf.Clamp01(p_coverage);
			float num4 = Mathf.Clamp01(p_weatherBlend);
			Vector4 vector3 = new Vector4(vector.x, vector.y, 0f, 0f);
			Vector4 vector4 = new Vector4(vector2.x, vector2.y, 0f, 0f);
			float num5 = Mathf.Max(0f, p_primaryTime);
			float num6 = Mathf.Max(0f, p_secondaryTime);
			float num7 = 1f / Mathf.Max(1f, p_amountScale);
			float num8 = 1f / Mathf.Max(1f, p_baseScale);
			float num9 = 1f / Mathf.Max(1f, p_detailScale);
			float num10 = 1f / Mathf.Max(1f, p_textureScale);
			float num11 = Mathf.Max(0.01f, p_softness);
			float num12 = Mathf.Clamp01(p_detailStrength);
			float num13 = Mathf.Clamp01(p_textureStrength);
			float num14 = 1f / Mathf.Max(1f, p_warpScale);
			float num15 = Mathf.Clamp01(p_warpStrength);
			Vector2 vector5 = new Vector2(0f - vector.y, vector.x) + vector * 0.35f;
			vector5.Normalize();
			Vector4 vector6 = new Vector4(vector5.x, vector5.y, 0f, 0f);
			float num16 = Mathf.Max(0f, p_shadowOffset);
			float num17 = Mathf.Max(0.01f, p_shadowSoftness);
			float num18 = Mathf.Clamp01(p_shadowStrength);
			float num19 = Mathf.Clamp01(p_innerShadowStrength);
			float num20 = Mathf.Clamp01(p_shadowOpacity);
			if (num != _cachedOpacity)
			{
				_cachedOpacity = num;
				_material.SetFloat(_opacityID, num);
			}
			if (num2 != _cachedCloudAmount)
			{
				_cachedCloudAmount = num2;
				_material.SetFloat(_cloudAmountID, num2);
			}
			if (num3 != _cachedCoverage)
			{
				_cachedCoverage = num3;
				_material.SetFloat(_coverageID, num3);
			}
			if (num4 != _cachedWeatherBlend)
			{
				_cachedWeatherBlend = num4;
				_material.SetFloat(_weatherBlendID, num4);
			}
			if (vector3 != _cachedWindDirection)
			{
				_cachedWindDirection = vector3;
				_material.SetVector(_windDirectionID, vector3);
			}
			if (vector4 != _cachedShadowDirection)
			{
				_cachedShadowDirection = vector4;
				_material.SetVector(_shadowDirectionID, vector4);
			}
			if (num5 != _cachedPrimaryTime)
			{
				_cachedPrimaryTime = num5;
				_material.SetFloat(_primaryTimeID, num5);
			}
			if (num6 != _cachedSecondaryTime)
			{
				_cachedSecondaryTime = num6;
				_material.SetFloat(_secondaryTimeID, num6);
			}
			if (num7 != _cachedInvAmountScale)
			{
				_cachedInvAmountScale = num7;
				_material.SetFloat(_invAmountScaleID, num7);
			}
			if (num8 != _cachedInvBaseScale)
			{
				_cachedInvBaseScale = num8;
				_material.SetFloat(_invBaseScaleID, num8);
			}
			if (num9 != _cachedInvDetailScale)
			{
				_cachedInvDetailScale = num9;
				_material.SetFloat(_invDetailScaleID, num9);
			}
			if (num10 != _cachedInvTextureScale)
			{
				_cachedInvTextureScale = num10;
				_material.SetFloat(_invTextureScaleID, num10);
			}
			if (num11 != _cachedSoftness)
			{
				_cachedSoftness = num11;
				_material.SetFloat(_softnessID, num11);
			}
			if (num12 != _cachedDetailStrength)
			{
				_cachedDetailStrength = num12;
				_material.SetFloat(_detailStrengthID, num12);
			}
			if (num13 != _cachedTextureStrength)
			{
				_cachedTextureStrength = num13;
				_material.SetFloat(_textureStrengthID, num13);
			}
			if (num14 != _cachedInvWarpScale)
			{
				_cachedInvWarpScale = num14;
				_material.SetFloat(_invWarpScaleID, num14);
			}
			if (vector6 != _cachedSideWind)
			{
				_cachedSideWind = vector6;
				_material.SetVector(_sideWindID, vector6);
			}
			if (num15 != _cachedWarpStrength)
			{
				_cachedWarpStrength = num15;
				_material.SetFloat(_warpStrengthID, num15);
			}
			if (num16 != _cachedShadowOffset)
			{
				_cachedShadowOffset = num16;
				_material.SetFloat(_shadowOffsetID, num16);
			}
			if (num17 != _cachedShadowSoftness)
			{
				_cachedShadowSoftness = num17;
				_material.SetFloat(_shadowSoftnessID, num17);
			}
			if (num18 != _cachedShadowStrength)
			{
				_cachedShadowStrength = num18;
				_material.SetFloat(_shadowStrengthID, num18);
			}
			if (num19 != _cachedInnerShadowStrength)
			{
				_cachedInnerShadowStrength = num19;
				_material.SetFloat(_innerShadowStrengthID, num19);
			}
			if (num20 != _cachedShadowOpacity)
			{
				_cachedShadowOpacity = num20;
				_material.SetFloat(_shadowOpacityID, num20);
			}
			if (p_cloudColor != _cachedCloudColor)
			{
				_cachedCloudColor = p_cloudColor;
				_material.SetColor(_cloudColorID, p_cloudColor);
			}
			if (p_shadowColor != _cachedShadowColor)
			{
				_cachedShadowColor = p_shadowColor;
				_material.SetColor(_shadowColorID, p_shadowColor);
			}
			if (p_seed != _cachedSeed)
			{
				_cachedSeed = p_seed;
				_material.SetVector(_seedID, p_seed);
			}
			float num21 = Mathf.Clamp01(p_renderScale);
			_debugSourceWidth = Mathf.Max(1, p_camera.pixelWidth);
			_debugSourceHeight = Mathf.Max(1, p_camera.pixelHeight);
			_debugRenderScale = num21;
			if (Mathf.Approximately(num21, 1f))
			{
				SetDirectRenderPath();
			}
			else if (num21 <= 0f)
			{
				_debugTargetWidth = 0;
				_debugTargetHeight = 0;
				SetDirectRenderPath();
			}
			else if (!SetDownscaledRenderPath(p_camera, num21))
			{
				SetDirectRenderPath();
			}
		}
	}

	private bool SetDownscaledRenderPath(Camera p_camera, float p_renderScale)
	{
		if (!EnsureDisplayMaterial())
		{
			return false;
		}
		if (!EnsureRenderTexture(p_camera, p_renderScale))
		{
			return false;
		}
		Vector2 viewportSize = GetViewportSize(p_camera);
		float x = viewportSize.x * (1f + _viewportPadding);
		float y = viewportSize.y * (1f + _viewportPadding);
		_material.SetFloat(_useOverlayUVID, 1f);
		SetMaterialBlendMode(BlendMode.One, BlendMode.Zero);
		_material.SetVector(_overlayCenterID, new Vector4(p_camera.transform.position.x, p_camera.transform.position.y, 0f, 0f));
		_material.SetVector(_overlaySizeID, new Vector4(x, y, 0f, 0f));
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _renderTexture;
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		RenderTexture.active = active;
		Graphics.Blit(Texture2D.blackTexture, _renderTexture, _material);
		_displayMaterial.SetTexture(_mainTexID, _renderTexture);
		_debugTargetWidth = _renderTextureWidth;
		_debugTargetHeight = _renderTextureHeight;
		if (!_isUsingDownscaledPath)
		{
			_meshRenderer.sharedMaterial = _displayMaterial;
			_isUsingDownscaledPath = true;
		}
		return true;
	}

	private void SetDirectRenderPath()
	{
		SetMaterialBlendMode(BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
		_material.SetFloat(_useOverlayUVID, 0f);
		if (_debugRenderScale > 0f)
		{
			_debugTargetWidth = _debugSourceWidth;
			_debugTargetHeight = _debugSourceHeight;
		}
		else
		{
			_debugTargetWidth = 0;
			_debugTargetHeight = 0;
		}
		if (_isUsingDownscaledPath)
		{
			_meshRenderer.sharedMaterial = _material;
			_isUsingDownscaledPath = false;
		}
	}

	private bool EnsureDisplayMaterial()
	{
		if (_displayMaterial != null)
		{
			return true;
		}
		Shader shader = Shader.Find("Unlit/Transparent");
		if (shader == null)
		{
			shader = Shader.Find("Sprites/Default");
		}
		if (shader == null)
		{
			if (!_loggedMissingDisplayShader)
			{
				Debug.LogError("ProceduralCloudOverlay could not find a display shader for downscaled rendering.");
				_loggedMissingDisplayShader = true;
			}
			return false;
		}
		_displayMaterial = new Material(shader);
		_displayMaterial.name = "Procedural Cloud Overlay Display (Runtime)";
		return true;
	}

	private void SetMaterialBlendMode(BlendMode p_srcBlend, BlendMode p_dstBlend)
	{
		_material.SetInt(_srcBlendID, (int)p_srcBlend);
		_material.SetInt(_dstBlendID, (int)p_dstBlend);
	}

	private bool EnsureRenderTexture(Camera p_camera, float p_renderScale)
	{
		int num = Mathf.Max(1, p_camera.pixelWidth);
		int num2 = Mathf.Max(1, p_camera.pixelHeight);
		int num3 = Mathf.Max(1, Mathf.RoundToInt((float)num * p_renderScale));
		int num4 = Mathf.Max(1, Mathf.RoundToInt((float)num2 * p_renderScale));
		if (_renderTexture != null && _renderTextureWidth == num3 && _renderTextureHeight == num4 && Mathf.Approximately(_renderTextureScale, p_renderScale))
		{
			return true;
		}
		ReleaseRenderTexture();
		_renderTexture = new RenderTexture(num3, num4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_renderTexture.name = "ProceduralCloudOverlayRT";
		_renderTexture.filterMode = FilterMode.Bilinear;
		_renderTexture.wrapMode = TextureWrapMode.Clamp;
		_renderTexture.useMipMap = false;
		_renderTexture.autoGenerateMips = false;
		_renderTexture.Create();
		if (!_renderTexture.IsCreated())
		{
			ReleaseRenderTexture();
			return false;
		}
		_renderTextureWidth = num3;
		_renderTextureHeight = num4;
		_renderTextureScale = p_renderScale;
		return true;
	}

	private void ReleaseRenderTexture()
	{
		if (!(_renderTexture == null))
		{
			_renderTexture.Release();
			Object.Destroy(_renderTexture);
			_renderTexture = null;
			_renderTextureWidth = 0;
			_renderTextureHeight = 0;
			_renderTextureScale = -1f;
		}
	}

	private void UpdateTransform(Camera p_camera)
	{
		Vector2 viewportSize = GetViewportSize(p_camera);
		_cachedTransform.position = new Vector3(p_camera.transform.position.x, p_camera.transform.position.y, _zPosition);
		_cachedTransform.localScale = new Vector3(viewportSize.x * (1f + _viewportPadding), viewportSize.y * (1f + _viewportPadding), 1f);
	}

	private Vector2 GetViewportSize(Camera p_camera)
	{
		float num = p_camera.orthographicSize * 2f;
		return new Vector2(num * p_camera.aspect, num);
	}

	private static Mesh BuildQuadMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "ProceduralCloudOverlayQuad";
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		mesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		mesh.RecalculateBounds();
		return mesh;
	}

	private void OnDestroy()
	{
		ReleaseRenderTexture();
		if (_displayMaterial != null)
		{
			Object.Destroy(_displayMaterial);
		}
		if (_material != null)
		{
			Object.Destroy(_material);
		}
	}

	private void OnGUI()
	{
		if (_showDebugInfo)
		{
			if (_debugLabelStyle == null)
			{
				_debugLabelStyle = new GUIStyle(GUI.skin.box);
				_debugLabelStyle.alignment = TextAnchor.MiddleLeft;
				_debugLabelStyle.fontSize = 12;
				_debugLabelStyle.normal.textColor = Color.white;
				_debugLabelStyle.padding = new RectOffset(8, 8, 4, 4);
			}
			string text;
			if (_debugRenderScale <= 0f)
			{
				text = "Hidden";
			}
			else
			{
				text = ((!_isUsingDownscaledPath) ? "Direct" : "Downscaled");
			}
			string text2 = _debugSourceWidth + "x" + _debugSourceHeight;
			string text3 = _debugTargetWidth + "x" + _debugTargetHeight;
			string text4 = "Clouds " + text + " | Scale " + (_debugRenderScale * 100f).ToString("0.00") + "% | Source " + text2 + " | Target " + text3;
			GUI.Box(new Rect(10f, 10f, 700f, 26f), text4, _debugLabelStyle);
		}
	}
}
