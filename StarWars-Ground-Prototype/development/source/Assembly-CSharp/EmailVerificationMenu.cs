using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailVerificationMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _info_text;

	[SerializeField]
	private Button _resendMail_button;

	[SerializeField]
	private TMP_InputField _email_inputfield;

	[Header("Loading Indicator")]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isSendingEmail;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isSendingEmail = false;
		_loadingIndicator.SetActive(value: false);
		_cts = new CancellationTokenSource();
		PrepareForm();
		StartCoroutine(CR_UpdateResendMailButtonCooldown(60f));
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public async void OnClick_ResendVerificationEmail()
	{
		if (!_isSendingEmail)
		{
			SoundManager.instance.PlayUIClickSound();
			_isSendingEmail = true;
			_loadingIndicator.SetActive(value: true);
			string text = _email_inputfield.text;
			StartCoroutine(CR_UpdateResendMailButtonCooldown(60f));
			await Server.Instance.AuthService.ResendMailConfirmation(text, _cts.Token);
			_isSendingEmail = false;
			_loadingIndicator.SetActive(value: false);
		}
	}

	private void PrepareForm()
	{
		_email_inputfield.SetTextWithoutNotify(string.IsNullOrWhiteSpace(PlayerSettings.Instance.Email) ? string.Empty : PlayerSettings.Instance.Email);
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
