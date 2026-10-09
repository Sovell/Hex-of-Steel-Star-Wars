using System.Threading;
using TMPro;
using UnityEngine;

public class AccountDeletionMenu : MonoBehaviour
{
	[Header("Inputs")]
	[SerializeField]
	private TMP_InputField _password_inputfield;

	[Header("Loading Indicator")]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isDeletingAccount;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_isDeletingAccount = false;
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

	public async void OnClick_DeleteAccount()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirm account deletion"), OnClick_ConfirmDeleteAccount);
	}

	private async void OnClick_ConfirmDeleteAccount()
	{
		if (!_isDeletingAccount)
		{
			_isDeletingAccount = true;
			_loadingIndicator.SetActive(value: true);
			string text = _password_inputfield.text;
			if (await Server.Instance.UsersService.DeleteAccount(text, _cts.Token))
			{
				PlayerSettings.Instance.AvatarData = null;
				PlayerSettings.Instance.ClearCredentials();
				PlayerSettings.Save();
				MultiplayerMenuManager.Instance.ShowLogin();
				MultiplayerMenuManager.Instance.LoadAvatar(p_updateOnLoaded: true);
				MultiplayerMenuManager.Instance.ClearLoginCredentials();
			}
			_isDeletingAccount = false;
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
