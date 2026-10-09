using TMPro;
using UnityEngine;

public class DamageTextGO : MonoBehaviour
{
	public TextMeshPro text;

	public void Animate()
	{
		if (PlayerSettings.Instance.ColorBlindMode)
		{
			text.color = new Color(0f, 0.6992469f, 1f);
		}
		LeanTween.move(gameObject, new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), 1f).setEaseOutCubic().setOnComplete(Disappear);
		Object.Destroy(gameObject, 1.5f);
	}

	private void Disappear()
	{
		LeanTween.scale(gameObject, Vector3.zero, 0.5f);
	}
}
