using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuManager : MonoBehaviour
{
	[Header("Texts")]
	[SerializeField]
	private TMP_Text _version_text;

	[Space]
	[Header("Menus")]
	[SerializeField]
	private LoginMenu _loginMenu;

	[SerializeField]
	private RegisterMenu _registerMenu;

	[SerializeField]
	private EmailVerificationMenu _emailVerificationMenu;

	[SerializeField]
	private LocalUserMenu _localUserMenu;

	[SerializeField]
	private ForgotPasswordMenu _forgotPasswordMenu;

	[SerializeField]
	private ChangePasswordMenu _changePasswordMenu;

	[SerializeField]
	private AccountDeletionMenu _accountDeletionMenu;

	[SerializeField]
	private MultiplayerGamesMenu _multiplayerGamesMenu;

	[SerializeField]
	private UserProfileMenu _userProfileMenu;

	[SerializeField]
	private RealtimeMultiplayerMenu _realtimeMultiplayerMenu;

	[SerializeField]
	private LobbyChatMenu _lobbyChatMenu;

	[SerializeField]
	private ArchivedGamesMenu _archivedGamesMenu;

	[Space]
	[Header("Control buttons")]
	[SerializeField]
	private Button _accountSettings_button;

	[SerializeField]
	private Button _chat_button;

	[SerializeField]
	private Button _logout_button;

	[Space]
	[Header("Avatar")]
	[SerializeField]
	private Sprite _avatarPlaceholder_sprite;

	[SerializeField]
	private List<Image> _localUserAvatarImages;

	private CancellationTokenSource _cts;

	public static MultiplayerMenuManager Instance { get; private set; }

	public Sprite Avatar_sprite { get; private set; }

	private void Awake()
	{
		Instance = this;
		_cts = new CancellationTokenSource();
		_version_text.text = Application.version;
	}

	private void OnEnable()
	{
		ShowLogin();
		LoadAvatar(p_updateOnLoaded: true);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Close();
		}
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		Logout();
		gameObject.SetActive(value: false);
	}

	public void OpenFromGame()
	{
		if (Server.Instance.IsUserAuthenticated)
		{
			Open();
			ShowMultiplayerGames();
		}
	}

	public void RefreshControlButtons()
	{
		bool isUserAuthenticated = Server.Instance.IsUserAuthenticated;
		_chat_button.gameObject.SetActive(isUserAuthenticated);
		_logout_button.gameObject.SetActive(isUserAuthenticated);
		_accountSettings_button.gameObject.SetActive(isUserAuthenticated);
	}

	public void LoadAvatar(bool p_updateOnLoaded)
	{
		if (!ServerUtils.LooksLikeJpeg(PlayerSettings.Instance.AvatarData))
		{
			Avatar_sprite = null;
		}
		else
		{
			try
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.name = "avatarTexture:" + PlayerSettings.Instance.Username;
				texture2D.LoadImage(PlayerSettings.Instance.AvatarData);
				texture2D.Apply();
				Avatar_sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				Avatar_sprite.name = "avatarSprite:" + PlayerSettings.Instance.Username;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				UIManager.ShowMessage(ex.Message);
			}
		}
		if (p_updateOnLoaded)
		{
			UpdateLocalUserAvatars();
		}
	}

	public void UpdateLocalUserAvatars()
	{
		foreach (Image localUserAvatarImage in _localUserAvatarImages)
		{
			localUserAvatarImage.sprite = ((Avatar_sprite != null) ? Avatar_sprite : _avatarPlaceholder_sprite);
		}
	}

	public async void OnClick_Ping()
	{
		SoundManager.instance.PlayUIClickSound();
		await Server.Instance.CheckServerConnexion(_cts.Token);
	}

	public void OnClick_Open()
	{
		SoundManager.instance.PlayUIClickSound();
		Open();
	}

	public void OnClick_Close()
	{
		SoundManager.instance.PlayUIClickSound();
		Close();
	}

	public void OnClick_Logout()
	{
		Logout();
	}

	public async void Logout()
	{
		await Server.Instance.AuthService.Logout(CancellationToken.None);
		ShowLogin();
	}

	public void ClearLoginCredentials()
	{
		_loginMenu.ClearCredentials();
	}

	public void ShowLogin()
	{
		HideAll();
		_loginMenu.Open();
	}

	public void ShowRegister()
	{
		HideAll();
		_registerMenu.Open();
	}

	public void ShowEmailVerification()
	{
		HideAll();
		_emailVerificationMenu.Open();
	}

	public void ShowLocalUser()
	{
		_localUserMenu.Open();
	}

	public void ShowPasswordReset()
	{
		HideAll();
		_forgotPasswordMenu.Open();
	}

	public void ShowChangePassword()
	{
		_changePasswordMenu.Open();
	}

	public void ShowAccountDeletion()
	{
		_accountDeletionMenu.Open();
	}

	public void ShowMultiplayerGames()
	{
		HideAll();
		_multiplayerGamesMenu.Open();
	}

	public void ShowRealtimeMultiplayer()
	{
		HideAll();
		_realtimeMultiplayerMenu.Open();
	}

	public void ShowUserProfileForPlayer(ServerDTOs.UserProfileResponse p_userProfileResponse)
	{
		_userProfileMenu.OpenForPlayer(p_userProfileResponse);
	}

	private void HideAll()
	{
		_loginMenu.Close();
		_registerMenu.Close();
		_emailVerificationMenu.Close();
		_localUserMenu.Close();
		_userProfileMenu.Close();
		_forgotPasswordMenu.Close();
		_changePasswordMenu.Close();
		_accountDeletionMenu.Close();
		_multiplayerGamesMenu.Close();
		_realtimeMultiplayerMenu.Close();
		_archivedGamesMenu.Close();
		_lobbyChatMenu.Close();
		RefreshControlButtons();
	}

	private void OnDestroy()
	{
		_cts.Cancel();
		_cts.Dispose();
	}
}
