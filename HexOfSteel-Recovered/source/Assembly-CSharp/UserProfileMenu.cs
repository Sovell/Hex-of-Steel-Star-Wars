using System;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserProfileMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private Image _avatar_image;

	[SerializeField]
	private Sprite _avatarPlaceholder_sprite;

	[SerializeField]
	private GameObject _banned_indicator1;

	[SerializeField]
	private GameObject _banned_indicator2;

	[SerializeField]
	private TMP_Text _username_text;

	[SerializeField]
	private TMP_Text _accountCreationDate_text;

	[SerializeField]
	private TMP_Text _bio_text;

	[SerializeField]
	private TMP_Text _lastLogin_text;

	[SerializeField]
	private TMP_Text _language_text;

	[SerializeField]
	private TMP_Text _wins_text;

	[SerializeField]
	private TMP_Text _losses_text;

	[SerializeField]
	private TMP_Text _forfeits_text;

	[SerializeField]
	private TMP_Text _turnsPlayed_text;

	[SerializeField]
	private GameObject _loading_indicator;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_avatar_image.sprite = _avatarPlaceholder_sprite;
		_cts = new CancellationTokenSource();
		_loading_indicator.SetActive(value: false);
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void OnClick_Close()
	{
		SoundManager.instance.PlayUIClickSound();
		Close();
	}

	public void OnClick_ReportUser()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = "support@hex-of-steel.fr";
		StringBuilder stringBuilder = new StringBuilder();
		string s = "User Report";
		stringBuilder.AppendLine("I want to report: " + _username_text.text);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Reason for the user report: ");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("=========================");
		stringBuilder.AppendLine("Generated automatically");
		stringBuilder.AppendLine("=========================");
		stringBuilder.AppendLine("Game: " + Application.productName + " v" + Application.version);
		stringBuilder.AppendLine($"Language: {Application.systemLanguage}");
		stringBuilder.AppendLine($"Platform: {Application.platform}");
		stringBuilder.AppendLine("OS: " + SystemInfo.operatingSystem);
		stringBuilder.AppendLine("Device: " + SystemInfo.deviceModel);
		stringBuilder.AppendLine("Graphics: " + SystemInfo.graphicsDeviceName);
		stringBuilder.AppendLine("=========================");
		string url = "mailto:" + text + "?subject=" + MailToEncode(s) + "&body=" + MailToEncode(stringBuilder.ToString());
		try
		{
			Application.OpenURL(url);
		}
		catch
		{
			GUIUtility.systemCopyBuffer = text;
			Debug.LogWarning("Couldn't open mail app. Support email copied to clipboard.");
			UIManager.ShowMessage("Couldn't open mail app. Support email copied to clipboard.");
		}
	}

	private string MailToEncode(string s)
	{
		return Uri.EscapeDataString(s).Replace("%0A", "%0D%0A");
	}

	public void OpenForPlayer(ServerDTOs.UserProfileResponse p_userProfileResponse)
	{
		Open();
		_banned_indicator1.SetActive(p_userProfileResponse.banned);
		_banned_indicator2.SetActive(p_userProfileResponse.banned);
		_username_text.text = p_userProfileResponse.username;
		_accountCreationDate_text.text = LocalizationManager.Translate("txt-member since") + p_userProfileResponse.createdAtUtc.LocalDateTime.ToString("g", LocalizationManager.GetCultureInfo());
		_bio_text.text = (string.IsNullOrWhiteSpace(p_userProfileResponse.bio) ? "..." : p_userProfileResponse.bio);
		_lastLogin_text.text = LocalizationManager.Translate("txt-last login date") + GetRelativeTimeString(p_userProfileResponse.lastLoginUtc);
		_language_text.text = LocalizationManager.Translate("txt-spoken language") + LocalizationManager.Translate("language." + p_userProfileResponse.language);
		_wins_text.text = LocalizationManager.Translate("txt-won games") + p_userProfileResponse.gamesWon;
		_losses_text.text = LocalizationManager.Translate("txt-lost games") + p_userProfileResponse.gamesLost;
		_forfeits_text.text = LocalizationManager.Translate("txt-forfeited games") + p_userProfileResponse.gamesForfeited;
		_turnsPlayed_text.text = LocalizationManager.Translate("txt-turns played") + p_userProfileResponse.turnsPlayed;
		LoadAvatarForPlayer(p_userProfileResponse.username);
	}

	private async void LoadAvatarForPlayer(string p_username)
	{
		_loading_indicator.SetActive(value: true);
		Sprite sprite = await Server.Instance.UsersService.GetAvatar(p_username, p_readable: false, _cts.Token);
		_loading_indicator.SetActive(value: false);
		if (sprite == null)
		{
			_avatar_image.sprite = _avatarPlaceholder_sprite;
		}
		else
		{
			_avatar_image.sprite = sprite;
		}
	}

	private string GetRelativeTimeString(DateTimeOffset? p_dateTimeUtc)
	{
		if (!p_dateTimeUtc.HasValue)
		{
			return LocalizationManager.Translate("txt-never");
		}
		TimeSpan timeSpan = DateTimeOffset.UtcNow - p_dateTimeUtc.Value;
		if (timeSpan.TotalMinutes < 1.0)
		{
			return LocalizationManager.Translate("txt-just now");
		}
		if (timeSpan.TotalMinutes < 60.0)
		{
			return LocalizationManager.Translate("txt-a few minutes ago");
		}
		if (timeSpan.TotalHours < 24.0)
		{
			return LocalizationManager.Translate("txt-a few hours ago");
		}
		if (timeSpan.TotalDays < 7.0)
		{
			return LocalizationManager.Translate("txt-a few days ago");
		}
		if (timeSpan.TotalDays < 14.0)
		{
			return LocalizationManager.Translate("txt-a week ago");
		}
		if (timeSpan.TotalDays < 30.0)
		{
			return LocalizationManager.Translate("txt-a few weeks ago");
		}
		if (timeSpan.TotalDays < 60.0)
		{
			return LocalizationManager.Translate("txt-a month ago");
		}
		if (timeSpan.TotalDays < 365.0)
		{
			return LocalizationManager.Translate("txt-a few months ago");
		}
		if (timeSpan.TotalDays < 730.0)
		{
			return LocalizationManager.Translate("txt-a year ago");
		}
		return LocalizationManager.Translate("txt-a few years ago");
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
