using UnityEngine;

public class SupplyMapOverlay : MonoBehaviour
{
	public static SupplyMapOverlay instance;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject _supplyHexPrefab;

	private Transform _overlayParent;

	private SpriteRenderer[,] _overlayRenderers;

	private int _sizeX;

	private int _sizeY;

	private bool _isVisible;

	private static readonly Color COLOR_FULL_SUPPLY = new Color(0f, 0.8f, 0f, 0.55f);

	private static readonly Color COLOR_HIGH_SUPPLY = new Color(0.4f, 0.8f, 0f, 0.55f);

	private static readonly Color COLOR_MEDIUM_SUPPLY = new Color(0.9f, 0.9f, 0f, 0.55f);

	private static readonly Color COLOR_LOW_SUPPLY = new Color(0.9f, 0.5f, 0f, 0.55f);

	private static readonly Color COLOR_NO_SUPPLY = new Color(0.9f, 0.1f, 0f, 0.55f);

	private void Awake()
	{
		instance = this;
	}

	public void Initialize()
	{
		if (_overlayRenderers == null)
		{
			_sizeX = GameData.Instance.map.SizeX;
			_sizeY = GameData.Instance.map.SizeY;
			GameObject gameObject = new GameObject("SupplyMapOverlayParent");
			gameObject.transform.SetParent(transform);
			_overlayParent = gameObject.transform;
			gameObject.SetActive(value: false);
			_overlayRenderers = new SpriteRenderer[_sizeX, _sizeY];
		}
	}

	private SpriteRenderer GetOrCreateOverlayRenderer(int x, int y)
	{
		if (_overlayRenderers[x, y] != null)
		{
			return _overlayRenderers[x, y];
		}
		GameObject gameObject = Object.Instantiate(_supplyHexPrefab, _overlayParent);
		Vector3 position = default;
		if (x % 2 == 0)
		{
			position.Set((float)x * 4.42f, (float)y * 5.1f + 2.55f, -0.9f);
		}
		else
		{
			position.Set((float)x * 4.42f, (float)y * 5.1f, -0.9f);
		}
		gameObject.transform.position = position;
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		_overlayRenderers[x, y] = component;
		return component;
	}

	public void ToggleSupplyMap(Player p_player)
	{
		if (_overlayRenderers == null)
		{
			Initialize();
		}
		_isVisible = !_isVisible;
		if (_isVisible)
		{
			if (MapGO.instance.politicalMap_parent.gameObject.activeSelf)
			{
				MapGO.instance.TogglePoliticalMap();
			}
			RefreshOverlay(p_player);
			_overlayParent.gameObject.SetActive(value: true);
		}
		else
		{
			_overlayParent.gameObject.SetActive(value: false);
		}
	}

	public void RefreshOverlay(Player p_player)
	{
		if (!_isVisible || _overlayRenderers == null)
		{
			return;
		}
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		for (int i = 0; i < _sizeX; i++)
		{
			for (int j = 0; j < _sizeY; j++)
			{
				Unit groundUnit = tilesTable[i, j].GroundUnit;
				if (groundUnit != null && groundUnit.OwnerPlayer == p_player && groundUnit.Type != "Boat")
				{
					SpriteRenderer orCreateOverlayRenderer = GetOrCreateOverlayRenderer(i, j);
					float num = float.MaxValue;
					float a = ((groundUnit.MaxAmmo > 0) ? ((float)groundUnit.CurrAmmo / (float)groundUnit.MaxAmmo) : float.MaxValue);
					float b = ((groundUnit.MaxAutonomy > 0) ? ((float)groundUnit.CurrAutonomy / (float)groundUnit.MaxAutonomy) : float.MaxValue);
					num = Mathf.Min(a, b) * 100f;
					orCreateOverlayRenderer.color = GetSupplyColor(num);
					orCreateOverlayRenderer.enabled = true;
				}
				else
				{
					SpriteRenderer spriteRenderer = _overlayRenderers[i, j];
					if (spriteRenderer != null)
					{
						spriteRenderer.enabled = false;
					}
				}
			}
		}
	}

	private static Color GetSupplyColor(float supplyPercentage)
	{
		if (supplyPercentage >= 90f)
		{
			return COLOR_FULL_SUPPLY;
		}
		if (supplyPercentage >= 65f)
		{
			return COLOR_HIGH_SUPPLY;
		}
		if (supplyPercentage >= 40f)
		{
			return COLOR_MEDIUM_SUPPLY;
		}
		if (supplyPercentage >= 15f)
		{
			return COLOR_LOW_SUPPLY;
		}
		return COLOR_NO_SUPPLY;
	}

	public bool IsVisible()
	{
		return _isVisible;
	}
}
