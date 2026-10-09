using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralRainOverlay : MonoBehaviour
{
	private static readonly int _colorID = Shader.PropertyToID("_Color");

	private static readonly int _intensityID = Shader.PropertyToID("_Intensity");

	private static readonly int _windAngleID = Shader.PropertyToID("_WindAngle");

	private static readonly int _speedID = Shader.PropertyToID("_Speed");

	private static readonly int _streakLengthID = Shader.PropertyToID("_StreakLength");

	private static readonly int _streakWidthID = Shader.PropertyToID("_StreakWidth");

	private static readonly int _columnDensityID = Shader.PropertyToID("_ColumnDensity");

	private static readonly int _vignetteIntensityID = Shader.PropertyToID("_VignetteIntensity");

	private static readonly int _vignetteColorID = Shader.PropertyToID("_VignetteColor");

	private static readonly int _vignetteSmoothnessID = Shader.PropertyToID("_VignetteSmoothness");

	private static Mesh _sharedQuadMesh;

	private MeshFilter _meshFilter;

	private MeshRenderer _meshRenderer;

	private Material _material;

	private Transform _cachedTransform;

	private float _zPosition;

	private float _viewportPadding;

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
	}

	public void SetVisible(bool p_isVisible)
	{
		if (!(_meshRenderer == null) && _meshRenderer.enabled != p_isVisible)
		{
			_meshRenderer.enabled = p_isVisible;
		}
	}

	public void Sync(Camera p_camera, float p_intensity, Color p_color, float p_windAngle, float p_speed, float p_streakLength, float p_streakWidth, float p_columnDensity, float p_vignetteIntensity, Color p_vignetteColor, float p_vignetteSmoothness)
	{
		if (!(p_camera == null) && !(_material == null))
		{
			UpdateTransform(p_camera);
			_material.SetColor(_colorID, p_color);
			_material.SetFloat(_intensityID, Mathf.Clamp01(p_intensity));
			_material.SetFloat(_windAngleID, p_windAngle);
			_material.SetFloat(_speedID, Mathf.Max(0f, p_speed));
			_material.SetFloat(_streakLengthID, Mathf.Max(0.01f, p_streakLength));
			_material.SetFloat(_streakWidthID, Mathf.Max(0.001f, p_streakWidth));
			_material.SetFloat(_columnDensityID, Mathf.Max(1f, p_columnDensity));
			_material.SetFloat(_vignetteIntensityID, Mathf.Clamp01(p_vignetteIntensity));
			_material.SetColor(_vignetteColorID, p_vignetteColor);
			_material.SetFloat(_vignetteSmoothnessID, Mathf.Max(0.01f, p_vignetteSmoothness));
		}
	}

	private void UpdateTransform(Camera p_camera)
	{
		float num = p_camera.orthographicSize * 2f;
		float num2 = num * p_camera.aspect;
		_cachedTransform.position = new Vector3(p_camera.transform.position.x, p_camera.transform.position.y, _zPosition);
		_cachedTransform.localScale = new Vector3(num2 * (1f + _viewportPadding), num * (1f + _viewportPadding), 1f);
	}

	private void OnDestroy()
	{
		if (_material != null)
		{
			Object.Destroy(_material);
		}
	}

	private static Mesh BuildQuadMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "ProceduralRainOverlayQuad";
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
}
