using TMPro;
using UnityEngine;

public class TutorialTaskUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _label;

	[SerializeField]
	private GameObject _pendingIcon;

	[SerializeField]
	private GameObject _fulfilledIcon;

	public void Setup(string p_text, bool p_done)
	{
		_label.text = p_text;
		_fulfilledIcon.SetActive(p_done);
		_pendingIcon.SetActive(!p_done);
		if (p_done)
		{
			AnimateFulfilled();
		}
	}

	public void UpdateTaskStatus(bool p_done)
	{
		_fulfilledIcon.SetActive(p_done);
		_pendingIcon.SetActive(!p_done);
		if (p_done)
		{
			AnimateFulfilled();
		}
	}

	private void AnimateFulfilled()
	{
		Vector3 localScale = _fulfilledIcon.transform.localScale;
		_fulfilledIcon.transform.localScale *= 2f;
		LeanTween.scale(_fulfilledIcon, localScale, 1f).setEaseOutElastic();
	}
}
