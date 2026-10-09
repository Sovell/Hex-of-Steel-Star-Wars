using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ServerUsersService : ServiceBase
{
	private readonly ServerAuthService _authService;

	public ServerUsersService(HttpClient p_client, ServerAuthService p_authService)
		: base(p_client)
	{
		_authService = p_authService;
	}

	public async Task<bool> GetMe(CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[GetMe] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		using (HttpResponseMessage res = await _client.GetAsync(Routes.Api("/users/me"), p_ct))
		{
			p_ct.ThrowIfCancellationRequested();
			string text = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, text);
				return false;
			}
			ServerDTOs.UserAccountResponse userAccountResponse = JsonConvert.DeserializeObject<ServerDTOs.UserAccountResponse>(text);
			Debug.Log("[GetMe] Me: " + userAccountResponse.username);
			return true;
		}
	}

	public async Task<ServerDTOs.UserProfileResponse> GetProfile(string p_username, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[GetProfile] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return null;
		}
		if (string.IsNullOrWhiteSpace(p_username))
		{
			Debug.LogError("[GetProfile] Username is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-username required"));
			return null;
		}
		string requestUri = Routes.Api(Routes.Format("/users/profile/{username}", p_username));
		using (HttpResponseMessage res = await _client.GetAsync(requestUri, p_ct))
		{
			p_ct.ThrowIfCancellationRequested();
			string text = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, text);
				return null;
			}
			ServerDTOs.UserProfileResponse userProfileResponse = JsonConvert.DeserializeObject<ServerDTOs.UserProfileResponse>(text);
			Debug.Log("[GetProfile] Retrieved profile for " + userProfileResponse.username);
			return userProfileResponse;
		}
	}

	public async Task<bool> ChangeLanguage(GameLanguages p_lang, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[ChangeLanguage] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.ChangeAccountLanguageRequest
		{
			language = p_lang.ToString().ToUpperInvariant()
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PatchAsync(Routes.Api("/users/me/change-language"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[ChangeLanguage] New account language: {p_lang}");
				return true;
			}
		}
	}

	public async Task<bool> UpdateBio(string p_bio, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[UpdateBio] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (!ServerUtils.IsValidBio(p_bio))
		{
			Debug.LogError("[UpdateBio] Invalid bio format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid bio format"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.ChangeBioRequest
		{
			bio = p_bio
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PatchAsync(Routes.Api("/users/me/change-bio"), content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log("[UpdateBio] New bio: " + p_bio);
				return true;
			}
		}
	}

	public async Task<bool> ChangeAvatar(byte[] p_bytes, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[ChangeAvatar] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (!ServerUtils.IsValidAvatar(p_bytes))
		{
			Debug.LogError("[ChangeAvatar] Invalid avatar data.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid avatar"));
			return false;
		}
		string requestUri = Routes.Api(Routes.Format("/users/me/change-avatar", PlayerSettings.Instance.Username));
		string content = JsonConvert.SerializeObject(new ServerDTOs.ChangeAvatarRequest
		{
			avatarData = p_bytes
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(requestUri, content2, p_ct))
			{
				string body = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, body);
					return false;
				}
				Debug.Log($"[ChangeAvatar] New avatar uploaded ({p_bytes.Length} bytes).");
				PlayerSettings.Instance.AvatarData = p_bytes;
				return true;
			}
		}
	}

	public async Task<byte[]> GetAvatar(string p_username, CancellationToken ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[GetAvatar] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return null;
		}
		if (string.IsNullOrWhiteSpace(p_username))
		{
			Debug.LogError("[GetAvatar] Username is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-username required"));
			return null;
		}
		string requestUri = Routes.Api(Routes.Format("/users/profile/avatar/{username}", p_username));
		using (HttpResponseMessage res = await _client.GetAsync(requestUri, ct))
		{
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, await res.Content.ReadAsStringAsync());
				return null;
			}
			if (res.StatusCode == HttpStatusCode.NoContent)
			{
				Debug.Log("[GetAvatar] User '" + p_username + "' has no avatar.");
				return null;
			}
			byte[] array = await res.Content.ReadAsByteArrayAsync();
			Debug.Log($"[GetAvatar] Retrieved avatar for user '{p_username}' ({array.Length} bytes).");
			return array;
		}
	}

	public async Task<Sprite> GetAvatar(string p_username, bool p_readable, CancellationToken ct)
	{
		byte[] array = await GetAvatar(p_username, ct);
		if (array == null)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.name = "avatarTexture:" + p_username;
		if (!texture2D.LoadImage(array, !p_readable))
		{
			Debug.LogError("[GetAvatar] Failed to decode JPEG bytes.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-failed to decode avatar"));
			Object.Destroy(texture2D);
			return null;
		}
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		sprite.name = "avatarSprite:" + p_username;
		return sprite;
	}

	public async Task<bool> ChangePassword(string p_currentPassword, string p_newPassword, string p_newPasswordConfirmation, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[ChangePassword] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(p_currentPassword))
		{
			Debug.LogError("[ChangePassword] Current password is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-current password required"));
			return false;
		}
		if (p_newPassword == PlayerSettings.Instance.Password)
		{
			Debug.LogError("[ChangePassword] Current password is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-new password same as current"));
			return false;
		}
		if (p_newPassword != p_newPasswordConfirmation)
		{
			Debug.LogError("[ChangePassword] New password and confirmation do not match.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-passwords do not match"));
			return false;
		}
		if (!ServerUtils.IsValidPassword(p_newPassword))
		{
			Debug.LogError("[ChangePassword] Invalid new password format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password format"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new
		{
			currentPassword = p_currentPassword,
			newPassword = p_newPassword
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/users/me/change-password"), content2, p_ct))
			{
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, await res.Content.ReadAsStringAsync());
					return false;
				}
				PlayerSettings.Instance.Password = p_newPassword;
				PlayerSettings.Save();
				Debug.Log("[ChangePassword] Changed password successfully.");
				return true;
			}
		}
	}

	public async Task<bool> DeleteAccount(string p_currentPassword, CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[DeleteAccount] User is not authenticated");
			UIManager.ShowMessage(LocalizationManager.Translate("error-not logged in"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(p_currentPassword))
		{
			Debug.LogError("[DeleteAccount] Password is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-password required"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.DeleteAccountRequest
		{
			password = p_currentPassword
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/users/me/delete-account"), content2, p_ct))
			{
				p_ct.ThrowIfCancellationRequested();
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				Server.Instance.ClearAccessToken();
				Debug.Log(string.Format("[DeleteAccount] {0} {1}{2}", (int)res.StatusCode, res.StatusCode, string.IsNullOrWhiteSpace(text) ? string.Empty : (" - " + text)));
				return true;
			}
		}
	}
}
