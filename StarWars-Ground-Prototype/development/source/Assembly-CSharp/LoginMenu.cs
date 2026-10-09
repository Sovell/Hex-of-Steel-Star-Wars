using System.Threading;
using TMPro;
using UnityEngine;

public class LoginMenu : MonoBehaviour
{
	[Header("Inputs")]
	[SerializeField]
	private TMP_InputField _username_inputfield;

	[SerializeField]
	private TMP_InputField _password_inputfield;

	[Header("Loading Indicator")]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isLoggingIn;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isLoggingIn = false;
		_loadingIndicator.SetActive(value: false);
		_cts = new CancellationTokenSource();
		PrefillCredentials(PlayerSettings.Instance.Username, PlayerSettings.Instance.Password);
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void PrefillCredentials(string p_username, string p_password)
	{
		_username_inputfield.SetTextWithoutNotify(string.IsNullOrWhiteSpace(p_username) ? string.Empty : p_username);
		_password_inputfield.SetTextWithoutNotify(string.IsNullOrWhiteSpace(p_password) ? string.Empty : p_password);
	}

	public void ClearCredentials()
	{
		_username_inputfield.SetTextWithoutNotify(string.Empty);
		_password_inputfield.SetTextWithoutNotify(string.Empty);
	}

	public async void OnClick_Login()
	{
		if (!_isLoggingIn)
		{
			_isLoggingIn = true;
			_loadingIndicator.SetActive(value: true);
			SoundManager.instance.PlayUIClickSound();
			string username = _username_inputfield.text;
			string password = _password_inputfield.text;
			if (await Server.Instance.AuthService.Login(username, password, _cts.Token))
			{
				PlayerSettings.Instance.Username = username;
				PlayerSettings.Instance.Password = password;
				PlayerSettings.Save();
				PlayerSettings instance = PlayerSettings.Instance;
				instance.AvatarData = await Server.Instance.UsersService.GetAvatar(username, _cts.Token);
				MultiplayerMenuManager.Instance.ShowMultiplayerGames();
				MultiplayerMenuManager.Instance.LoadAvatar(p_updateOnLoaded: true);
				PlayerSettings.Save();
			}
			else if (Server.Instance.AuthService.NeedsEmailVerification)
			{
				MultiplayerMenuManager.Instance.ShowEmailVerification();
			}
			_isLoggingIn = false;
			_loadingIndicator.SetActive(value: false);
		}
	}

	public void OnClick_ForgotPassword()
	{
		MultiplayerMenuManager.Instance.ShowPasswordReset();
	}

	public void OnClick_Register()
	{
		MultiplayerMenuManager.Instance.ShowRegister();
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
