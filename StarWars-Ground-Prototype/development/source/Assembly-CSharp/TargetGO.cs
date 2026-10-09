using System.Collections;
using UnityEngine;

public class TargetGO : MonoBehaviour
{
	[SerializeField]
	private bool scaleChange;

	[SerializeField]
	private float fadeTime;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private void OnEnable()
	{
		if (scaleChange)
		{
			Vector3 localScale = transform.localScale;
			transform.localScale = Vector3.zero;
			LeanTween.scale(gameObject, localScale, fadeTime);
		}
		StartCoroutine(CR_ChangeAlpha(p_fadeIn: true));
	}

	public void Destroy()
	{
		StartCoroutine(CR_ChangeAlpha(p_fadeIn: false));
	}

	private IEnumerator CR_ChangeAlpha(bool p_fadeIn)
	{
		float timer = 0f;
		while (timer < fadeTime)
		{
			timer += Time.deltaTime;
			if (p_fadeIn)
			{
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(0f, 1f, timer / fadeTime));
			}
			else
			{
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(1f, 0f, timer / fadeTime));
			}
			yield return null;
		}
		if (!p_fadeIn)
		{
			Object.Destroy(gameObject);
		}
	}
}
