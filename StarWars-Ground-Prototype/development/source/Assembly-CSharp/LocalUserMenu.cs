using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;

public class LocalUserMenu : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _username_text;

	[SerializeField]
	private TMP_Text _accountCreationDate_text;

	[SerializeField]
	private TMP_InputField _bio_inputfield;

	[SerializeField]
	private TMP_Text _bioRemainingChar_text;

	[SerializeField]
	private TMP_Dropdown _language_dropdown;

	private string _lastBioSent;

	private bool _isUpdatingBio;

	private bool _isChangingLanguage;

	private bool _isUploadingAvatar;

	private CancellationTokenSource _cts;

	private void OnEnable()
	{
		_lastBioSent = null;
		_isUpdatingBio = false;
		_isChangingLanguage = false;
		_isUploadingAvatar = false;
		_cts = new CancellationTokenSource();
		PrefillUserInfo();
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

	public void OnClick_EditAvatar()
	{
		SoundManager.instance.PlayUIClickSound();
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		FileBrowser.SetFilters(false, new FileBrowser.Filter("Image", ".jpg"));
		FileBrowser.ShowLoadDialog((string[] selection) =>
		{
			Success(selection);
		}, Cancel, FileBrowser.PickMode.Files, allowMultiSelection: false, folderPath, null, string.Empty, LocalizationManager.Translate("btn-upload"));
	}

	public void OnBioValueChanged()
	{
		int num = 100 - _bio_inputfield.text.Length;
		_bioRemainingChar_text.text = num.ToString();
	}

	private void Success(string[] p_paths)
	{
		byte[] p_bytes = File.ReadAllBytes(new FileInfo(p_paths[0]).FullName);
		UploadAvatar(p_bytes);
	}

	private void Cancel()
	{
	}

	public async void UploadAvatar(byte[] p_bytes)
	{
		if (!_isUploadingAvatar)
		{
			_isUploadingAvatar = true;
			if (await Server.Instance.UsersService.ChangeAvatar(p_bytes, CancellationToken.None))
			{
				PlayerSettings.Instance.AvatarData = p_bytes;
				MultiplayerMenuManager.Instance.LoadAvatar(p_updateOnLoaded: true);
			}
			_isUploadingAvatar = false;
		}
	}

	public async void OnClick_ChangeLanguage()
	{
		if (_language_dropdown.options.Count > 0 && !_isChangingLanguage)
		{
			_isChangingLanguage = true;
			SoundManager.instance.PlayUIClickSound();
			GameLanguages value = (GameLanguages)_language_dropdown.value;
			if (Enum.IsDefined(typeof(GameLanguages), value))
			{
				await Server.Instance.UsersService.ChangeLanguage(value, CancellationToken.None);
			}
			_isChangingLanguage = false;
		}
	}

	public async void OnEndEdit_UpdateBio()
	{
		if (_isUpdatingBio)
		{
			return;
		}
		string newBio = _bio_inputfield.text;
		if (!(_lastBioSent == newBio))
		{
			_isUpdatingBio = true;
			if (await Server.Instance.UsersService.UpdateBio(newBio, CancellationToken.None))
			{
				_lastBioSent = newBio;
			}
			_isUpdatingBio = false;
		}
	}

	private async void PrefillUserInfo()
	{
		_username_text.text = string.Empty;
		_accountCreationDate_text.text = LocalizationManager.Translate("txt-member since") + "...";
		_language_dropdown.ClearOptions();
		_language_dropdown.options.Add(new TMP_Dropdown.OptionData
		{
			text = "..."
		});
		_language_dropdown.SetValueWithoutNotify(0);
		_language_dropdown.RefreshShownValue();
		_bio_inputfield.SetTextWithoutNotify("...");
		_bio_inputfield.characterLimit = 100;
		OnBioValueChanged();
		ServerDTOs.UserProfileResponse userProfileResponse = await Server.Instance.UsersService.GetProfile(PlayerSettings.Instance.Username, _cts.Token);
		if (userProfileResponse == null)
		{
			return;
		}
		_username_text.text = userProfileResponse.username;
		_accountCreationDate_text.text = LocalizationManager.Translate("txt-member since") + LocalizationManager.GetLocalizedDate(userProfileResponse.createdAtUtc.ToLocalTime().DateTime);
		_lastBioSent = userProfileResponse.bio;
		_bio_inputfield.SetTextWithoutNotify(userProfileResponse.bio);
		OnBioValueChanged();
		List<string> list = new List<string>();
		foreach (GameLanguages value in Enum.GetValues(typeof(GameLanguages)))
		{
			list.Add(LocalizationManager.Translate("language." + value));
		}
		_language_dropdown.ClearOptions();
		_language_dropdown.AddOptions(list);
		if (Enum.TryParse<GameLanguages>(userProfileResponse.language, out var result))
		{
			_language_dropdown.SetValueWithoutNotify((int)result);
		}
		_language_dropdown.RefreshShownValue();
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
