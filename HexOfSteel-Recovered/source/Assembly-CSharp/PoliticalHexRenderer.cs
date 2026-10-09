using UnityEngine;

public class PoliticalHexRenderer : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Sprite _normal_sprite;

	[SerializeField]
	private Sprite _occupied_sprite;

	private bool _isOccupiedSprite;

	public void Place(Vector3 p_vector3)
	{
		_transform.position = p_vector3;
	}

	public void SetSpriteAndColor(Color p_color, bool p_occupied)
	{
		if (p_occupied)
		{
			if (!_isOccupiedSprite)
			{
				_spriteRenderer.sprite = _occupied_sprite;
			}
			_isOccupiedSprite = true;
		}
		else
		{
			if (_isOccupiedSprite)
			{
				_spriteRenderer.sprite = _normal_sprite;
			}
			_isOccupiedSprite = false;
		}
		_spriteRenderer.color = p_color;
	}
}
