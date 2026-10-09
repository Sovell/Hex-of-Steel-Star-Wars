using UnityEngine;

public class Explosion : MonoBehaviour
{
	public Animator Animator;

	private void Start()
	{
		Object.Destroy(gameObject, 1f);
		Animator.Play("Explosion");
	}
}
