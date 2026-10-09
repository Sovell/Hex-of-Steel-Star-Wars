using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UsernameToProfile : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _username_text;

	private CancellationTokenSource _cts;

	private bool _isFetchingProfile;

	private void OnEnable()
	{
		_isFetchingProfile = false;
		_cts = new CancellationTokenSource();
	}

	private void Start()
	{
		if (TryGetComponent<Button>(out var component))
		{
			ColorBlock colors = component.colors;
			colors.normalColor = ((_username_text.text == PlayerSettings.Instance.Username) ? MainMenu.yellowColor : MainMenu.greyColor);
			component.colors = colors;
		}
	}

	public void OnClick_FetchUserProfile()
	{
		SoundManager.instance.PlayUIClickSound();
		FetchUserProfile();
	}

	private async void FetchUserProfile()
	{
		if (!_isFetchingProfile)
		{
			_isFetchingProfile = true;
			SoundManager.instance.PlayUIClickSound();
			string text = _username_text.text;
			ServerDTOs.UserProfileResponse userProfileResponse = await Server.Instance.UsersService.GetProfile(text, _cts.Token);
			_isFetchingProfile = false;
			if (userProfileResponse != null)
			{
				MultiplayerMenuManager.Instance.ShowUserProfileForPlayer(userProfileResponse);
			}
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
