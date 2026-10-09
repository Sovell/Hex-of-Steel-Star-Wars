using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgotPasswordMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _info_text;

	[SerializeField]
	private Button _resendMail_button;

	[Header("Inputs")]
	[SerializeField]
	private TMP_InputField _email_inputfield;

	[SerializeField]
	private TMP_InputField _newPassword_inputfield;

	[SerializeField]
	private TMP_InputField _verificationCode_inputfield;

	private bool _isSendingEmail;

	private bool _isSettingPassword;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isSendingEmail = false;
		_isSettingPassword = false;
		_cts = new CancellationTokenSource();
		PrepareForm();
		if (ServerUtils.IsValidEmail(_email_inputfield.text))
		{
			OnClick_SendResetMail();
		}
		else
		{
			_resendMail_button.interactable = true;
		}
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public async void OnClick_SendResetMail()
	{
		if (!_isSendingEmail)
		{
			_isSendingEmail = true;
			SoundManager.instance.PlayUIClickSound();
			string text = _email_inputfield.text;
			if (await Server.Instance.AuthService.SendPasswordRecoveryCode(text, _cts.Token))
			{
				StartCoroutine(CR_UpdateResendMailButtonCooldown(60f));
			}
			_isSendingEmail = false;
		}
	}

	public async void OnClick_SetNewPassword()
	{
		if (!_isSettingPassword)
		{
			_isSettingPassword = true;
			SoundManager.instance.PlayUIClickSound();
			string text = _email_inputfield.text;
			string text2 = _verificationCode_inputfield.text;
			string text3 = _newPassword_inputfield.text;
			if (await Server.Instance.AuthService.SetNewPassword(text, text2, text3, _cts.Token))
			{
				MultiplayerMenuManager.Instance.ShowLogin();
			}
			_isSettingPassword = false;
		}
	}

	private void PrepareForm()
	{
		_email_inputfield.SetTextWithoutNotify(PlayerSettings.Instance.Email ?? string.Empty);
		_newPassword_inputfield.SetTextWithoutNotify(string.Empty);
		_verificationCode_inputfield.SetTextWithoutNotify(string.Empty);
	}

	private IEnumerator CR_UpdateResendMailButtonCooldown(float p_cooldownSeconds)
	{
		_resendMail_button.interactable = false;
		for (float remainingTime = p_cooldownSeconds; remainingTime > 0f; remainingTime -= Time.deltaTime)
		{
			_info_text.text = LocalizationManager.Translate("txt-resend email cooldown") + Mathf.CeilToInt(remainingTime) + "s";
			yield return null;
		}
		_info_text.text = string.Empty;
		_resendMail_button.interactable = true;
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
