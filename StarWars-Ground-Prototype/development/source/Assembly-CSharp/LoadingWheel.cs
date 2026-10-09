using UnityEngine;

public class LoadingWheel : MonoBehaviour
{
	public float speed = 180f;

	private void Update()
	{
		transform.Rotate(0f, 0f, (0f - speed) * Time.deltaTime);
	}
}
