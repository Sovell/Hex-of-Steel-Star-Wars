using System.Collections;
using TMPro;
using UnityEngine;

public class XPanimationGO : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private float delay;

	[SerializeField]
	private float fadeTime;

	[SerializeField]
	private TextMeshPro[] texts;

	[SerializeField]
	private SpriteRenderer[] spriteRenderers;

	private void Start()
	{
		StartCoroutine(CR_FadeAlpha());
	}

	private IEnumerator CR_FadeAlpha()
	{
		float timer = 0f;
		while (true)
		{
			timer += Time.deltaTime;
			if (delay > 0f)
			{
				if (timer >= delay)
				{
					timer = 0f;
					delay = 0f;
				}
				yield return null;
				continue;
			}
			for (int i = 0; i < texts.Length; i++)
			{
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, Mathf.Lerp(1f, 0f, timer / fadeTime));
			}
			for (int j = 0; j < spriteRenderers.Length; j++)
			{
				spriteRenderers[j].color = new Color(spriteRenderers[j].color.r, spriteRenderers[j].color.g, spriteRenderers[j].color.b, Mathf.Lerp(1f, 0f, timer / fadeTime));
			}
			yield return null;
			if (timer > fadeTime)
			{
				break;
			}
		}
	}
}
