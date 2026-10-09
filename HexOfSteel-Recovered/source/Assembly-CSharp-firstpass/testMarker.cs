using ChartAndGraph;
using UnityEngine;

public class testMarker : MonoBehaviour
{
	public GameObject Place;

	private void Start()
	{
	}

	private void Update()
	{
		RadarChart component = GetComponent<RadarChart>();
		if (component.SnapWorldPointToPosition(Input.mousePosition, out var group, out var amount) && component.ItemToWorldPosition(group, amount, out var worldposition))
		{
			Place.transform.position = worldposition;
		}
	}
}
