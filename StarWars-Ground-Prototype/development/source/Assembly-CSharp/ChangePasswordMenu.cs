using System.Threading;
using TMPro;
using UnityEngine;

public class ChangePasswordMenu : MonoBehaviour
{
	[Header("Inputs")]
	[SerializeField]
	private TMP_InputField _currentPassword_inputfield;

	[SerializeField]
	private TMP_InputField _newPassword_inputfield;

	[SerializeField]
	private TMP_InputField _confirmPassword_inputfield;

	[Header("Loading Indicator")]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isChangingPassword;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isChangingPassword = false;
		_loadingIndicator.SetActive(value: false);
		_cts = new CancellationTokenSource();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public async void OnClick_ChangePassword()
	{
		if (!_isChangingPassword)
		{
			SoundManager.instance.PlayUIClickSound();
			_isChangingPassword = true;
			_loadingIndicator.SetActive(value: true);
			string text = _currentPassword_inputfield.text;
			string text2 = _newPassword_inputfield.text;
			string text3 = _confirmPassword_inputfield.text;
			if (await Server.Instance.UsersService.ChangePassword(text, text2, text3, _cts.Token))
			{
				MultiplayerMenuManager.Instance.Logout();
				MultiplayerMenuManager.Instance.ClearLoginCredentials();
			}
			_isChangingPassword = false;
			_loadingIndicator.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		_cts?.Cancel();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}
