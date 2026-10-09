using UnityEngine;
using UnityEngine.UI;

public class RefreshAnimator : MonoBehaviour
{
	[SerializeField]
	private float _rotationDuration = 0.35f;

	[SerializeField]
	private LeanTweenType _easeType = LeanTweenType.easeInOutSine;

	[SerializeField]
	private Button _targetButton;

	[SerializeField]
	private GameObject _targetObject;

	private bool _isAnimating;

	private void Awake()
	{
		_targetButton.onClick.AddListener(Play);
	}

	public void Play()
	{
		if (!_isAnimating)
		{
			_isAnimating = true;
			float to = _targetObject.transform.localEulerAngles.z + 180f;
			LeanTween.rotateZ(_targetObject, to, _rotationDuration).setEase(_easeType).setOnComplete(() =>
			{
				_isAnimating = false;
			});
		}
	}

	private void OnDestroy()
	{
		_targetButton.onClick.RemoveListener(Play);
	}
}
