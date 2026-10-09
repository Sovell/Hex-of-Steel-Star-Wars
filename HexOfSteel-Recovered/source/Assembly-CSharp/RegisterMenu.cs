using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterMenu : MonoBehaviour
{
	[Header("Inputs")]
	[SerializeField]
	private TMP_InputField _username_inputfield;

	[SerializeField]
	private TMP_InputField _email_inputfield;

	[SerializeField]
	private TMP_InputField _password_inputfield;

	[SerializeField]
	private TMP_InputField _passwordConfirmation_inputfield;

	[Header("EULA")]
	[SerializeField]
	private Toggle _eula_toggle;

	[SerializeField]
	private GameObject _eula_container;

	[Header("Buttons")]
	[SerializeField]
	private Button _register_button;

	[SerializeField]
	private Button _resend_mail_button;

	[Header("Loading Indicator")]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isRegistering;

	private bool _isSendingEmail;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isRegistering = false;
		_isSendingEmail = false;
		_loadingIndicator.SetActive(value: false);
		_cts = new CancellationTokenSource();
		ClearForm();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void OnClick_ReadEULA()
	{
		SoundManager.instance.PlayUIClickSound();
		_eula_container.SetActive(value: true);
	}

	public void OnClick_AcceptEULA()
	{
		SoundManager.instance.PlayUIClickSound();
		_eula_container.SetActive(value: false);
		_register_button.interactable = true;
	}

	public void OnClick_DisagreeEULA()
	{
		SoundManager.instance.PlayUIClickSound();
		_eula_container.SetActive(value: false);
		_eula_toggle.SetIsOnWithoutNotify(value: false);
		_register_button.interactable = false;
	}

	public async void OnClick_Register()
	{
		SoundManager.instance.PlayUIClickSound();
		string password = _password_inputfield.text;
		string text = _passwordConfirmation_inputfield.text;
		if (password != text)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-passwords do not match"));
		}
		else if (!_isRegistering)
		{
			_isRegistering = true;
			_loadingIndicator.SetActive(value: true);
			string username = _username_inputfield.text;
			string email = _email_inputfield.text;
			if (await Server.Instance.AuthService.Register(username, password, email, PlayerSettings.Instance.Language, _cts.Token))
			{
				_register_button.gameObject.SetActive(value: false);
				_resend_mail_button.gameObject.SetActive(value: true);
				PlayerSettings.Instance.Username = username;
				PlayerSettings.Instance.Email = email;
				PlayerSettings.Instance.Password = password;
				PlayerSettings.Instance.AvatarData = null;
				PlayerSettings.Save();
				MultiplayerMenuManager.Instance.LoadAvatar(p_updateOnLoaded: true);
				MultiplayerMenuManager.Instance.ShowEmailVerification();
			}
			_isRegistering = false;
			_loadingIndicator.SetActive(value: false);
		}
	}

	public async void OnClick_ResendMail()
	{
		if (!_isSendingEmail)
		{
			_isSendingEmail = true;
			SoundManager.instance.PlayUIClickSound();
			string text = _email_inputfield.text;
			await Server.Instance.AuthService.ResendMailConfirmation(text, _cts.Token);
			_isSendingEmail = false;
		}
	}

	private void ClearForm()
	{
		_username_inputfield.SetTextWithoutNotify(string.Empty);
		_email_inputfield.SetTextWithoutNotify(string.Empty);
		_password_inputfield.SetTextWithoutNotify(string.Empty);
		_passwordConfirmation_inputfield.SetTextWithoutNotify(string.Empty);
		_eula_container.SetActive(value: false);
		_eula_toggle.SetIsOnWithoutNotify(value: false);
		_register_button.interactable = false;
		_resend_mail_button.gameObject.SetActive(value: false);
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
