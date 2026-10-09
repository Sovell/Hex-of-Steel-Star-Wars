using UnityEngine;

public class OwnerPanelEditorGO : MonoBehaviour
{
	private RectTransform rectTransform;

	private void Start()
	{
		rectTransform = gameObject.GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		Vector2 point = rectTransform.InverseTransformPoint(Input.mousePosition);
		if (!rectTransform.rect.Contains(point))
		{
			MapEditorGO.instance.tooltip.SetActive(value: false);
			gameObject.SetActive(value: false);
		}
	}
}
