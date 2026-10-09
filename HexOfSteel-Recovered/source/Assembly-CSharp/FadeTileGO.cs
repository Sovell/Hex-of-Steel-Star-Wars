using System.Collections;
using UnityEngine;

public class FadeTileGO : MonoBehaviour
{
	[SerializeField]
	private float _fadeTime;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	public void StartFade(TileGO p_tileGO)
	{
		transform.position = new Vector3(p_tileGO.transform.position.x, p_tileGO.transform.position.y, p_tileGO.transform.position.z - 0.001f);
		_spriteRenderer.sprite = p_tileGO.GetSprite();
		_spriteRenderer.color = p_tileGO.GetSpriteColor();
		StartCoroutine(CR_Fade());
	}

	private IEnumerator CR_Fade()
	{
		float timer = 0f;
		Color startColor = _spriteRenderer.color;
		Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
		while (timer < _fadeTime)
		{
			timer += Time.deltaTime;
			_spriteRenderer.color = Color.Lerp(startColor, endColor, timer / _fadeTime);
			yield return null;
		}
		Object.Destroy(gameObject);
	}
}
