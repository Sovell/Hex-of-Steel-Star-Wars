using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ServerAuthService : ServiceBase
{
	public bool IsRefreshingAccess { get; private set; }

	public bool NeedsEmailVerification { get; private set; }

	public ServerAuthService(HttpClient p_client)
		: base(p_client)
	{
	}

	public async Task<bool> Login(string p_username, string p_password, CancellationToken p_ct)
	{
		NeedsEmailVerification = false;
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[Login] Client is not connected");
			return false;
		}
		if (string.IsNullOrWhiteSpace(p_username))
		{
			Debug.LogError("[Login] Invalid username.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid username"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(p_password))
		{
			Debug.LogError("[Login] Invalid password.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.LoginRequest
		{
			username = p_username,
			password = p_password
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/login"), content2, p_ct))
			{
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					if (text.Contains("email_verification_required"))
					{
						NeedsEmailVerification = true;
					}
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				ServerDTOs.LoginResponse loginResponse = JsonConvert.DeserializeObject<ServerDTOs.LoginResponse>(text);
				DateTimeOffset p_expirationUtc = loginResponse.expiresAtUtc;
				if (res.Headers.Date.HasValue)
				{
					TimeSpan timeSpan = loginResponse.expiresAtUtc - res.Headers.Date.Value;
					p_expirationUtc = DateTimeOffset.UtcNow.Add(timeSpan);
				}
				AccessToken p_accessToken = new AccessToken("Bearer", loginResponse.accessToken, p_expirationUtc);
				Server.Instance.ApplyToken(p_accessToken);
				PlayerSettings.Instance.UserId = loginResponse.userId;
				PlayerSettings.Instance.Username = loginResponse.username;
				PlayerSettings.Instance.Password = p_password;
				Debug.Log("[Login] OK.");
				return true;
			}
		}
	}

	public async Task<bool> Register(string p_username, string p_password, string p_email, GameLanguages p_language, CancellationToken p_ct)
	{
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[Register] Client is not connected");
			return false;
		}
		if (!ServerUtils.IsValidEmail(p_email))
		{
			Debug.LogError("[Register] Invalid email format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid email"));
			return false;
		}
		if (!ServerUtils.IsValidUsername(p_username))
		{
			Debug.LogError("[Register] Invalid username format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid username format"));
			return false;
		}
		if (!ServerUtils.IsValidPassword(p_password))
		{
			Debug.LogError("[Register] Invalid password format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password format"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.RegisterRequest
		{
			username = p_username,
			password = p_password,
			email = p_email,
			language = p_language.ToString().ToUpperInvariant()
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/register"), content2, p_ct))
			{
				p_ct.ThrowIfCancellationRequested();
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				Debug.Log(string.Format("[Register] {0} {1}{2}", (int)res.StatusCode, res.StatusCode, string.IsNullOrWhiteSpace(text) ? string.Empty : (" - " + text)));
				Server.Instance.ClearAccessToken();
				PlayerSettings.Instance.Username = p_username;
				PlayerSettings.Instance.Email = p_email;
				PlayerSettings.Instance.Password = p_password;
				return true;
			}
		}
	}

	public async Task<bool> ResendMailConfirmation(string p_email, CancellationToken p_ct)
	{
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[ResendMailConfirmation] Client is not connected");
			return false;
		}
		if (!ServerUtils.IsValidEmail(p_email))
		{
			Debug.LogError("[ResendMailConfirmation] Invalid email format.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid email"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.ResendMailConfirmationRequest
		{
			email = p_email
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/resend-email-confirm"), content2, p_ct))
			{
				p_ct.ThrowIfCancellationRequested();
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				Debug.Log(string.Format("[ResendMailConfirmation] {0} {1}{2}", (int)res.StatusCode, res.StatusCode, string.IsNullOrWhiteSpace(text) ? string.Empty : (" - " + text)));
				return true;
			}
		}
	}

	public async Task<bool> Logout(CancellationToken p_ct)
	{
		if (!Server.Instance.IsUserAuthenticated)
		{
			Debug.LogWarning("[Logout] Client is not connected");
			return false;
		}
		using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/logout"), null, p_ct))
		{
			if (!res.IsSuccessStatusCode)
			{
				ServerErrorHandler.HandleErrorResponse(res, await res.Content.ReadAsStringAsync());
				return false;
			}
			Server.Instance.ClearAccessToken();
			Debug.Log("[Logout] OK.");
			return true;
		}
	}

	public async Task<bool> RefreshAccessToken(AccessToken p_accessToken, CancellationToken p_ct)
	{
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[RefreshAccessToken] Client is not connected");
			return false;
		}
		if (p_accessToken == null)
		{
			Debug.LogWarning("[RefreshAccessToken] Access token is null");
			return false;
		}
		if (!p_accessToken.IsAboutToExpire)
		{
			Debug.LogWarning("[RefreshAccessToken] Access token is valid");
			return false;
		}
		if (p_accessToken.IsExpired)
		{
			Debug.LogWarning("[RefreshAccessToken] Access token is expired");
			return false;
		}
		if (IsRefreshingAccess)
		{
			Debug.LogWarning("[RefreshAccessToken] Already refreshing access token");
			return false;
		}
		IsRefreshingAccess = true;
		string content = JsonConvert.SerializeObject(new ServerDTOs.RefreshRequest
		{
			token = p_accessToken.Value
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/refresh-access-token"), content2, p_ct))
			{
				p_ct.ThrowIfCancellationRequested();
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					Server.Instance.ClearAccessToken();
				}
				else
				{
					ServerDTOs.RefreshAccessResponse refreshAccessResponse = JsonConvert.DeserializeObject<ServerDTOs.RefreshAccessResponse>(text);
					DateTimeOffset p_expirationUtc = refreshAccessResponse.expiresAtUtc;
					if (res.Headers.Date.HasValue)
					{
						TimeSpan timeSpan = refreshAccessResponse.expiresAtUtc - res.Headers.Date.Value;
						p_expirationUtc = DateTimeOffset.UtcNow.Add(timeSpan);
					}
					AccessToken p_accessToken2 = new AccessToken("Bearer", refreshAccessResponse.accessToken, p_expirationUtc);
					Server.Instance.ApplyToken(p_accessToken2);
					Debug.Log("[RefreshAccessToken] Token refreshed.");
				}
				IsRefreshingAccess = false;
				return res.IsSuccessStatusCode;
			}
		}
	}

	public async Task<bool> SendPasswordRecoveryCode(string p_email, CancellationToken p_ct)
	{
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[ForgotPassword] Client is not connected");
			return false;
		}
		if (!ServerUtils.IsValidEmail(p_email))
		{
			Debug.LogError("[ForgotPassword] Invalid email.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid email"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.ForgotPasswordRequest
		{
			email = p_email.Trim()
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/forgot-password"), content2, p_ct))
			{
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				Debug.Log(string.Format("[ForgotPassword] {0} {1}{2}", (int)res.StatusCode, res.StatusCode, string.IsNullOrWhiteSpace(text) ? string.Empty : (" - " + text)));
				return true;
			}
		}
	}

	public async Task<bool> SetNewPassword(string p_email, string p_verificationCode, string p_newPassword, CancellationToken p_ct)
	{
		if (!Server.Instance.IsClientConnected)
		{
			Debug.LogWarning("[SetNewPassword] Client is not connected");
			return false;
		}
		if (!ServerUtils.IsValidEmail(p_email))
		{
			Debug.LogError("[SetNewPassword] A valid email is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid email"));
			return false;
		}
		if (string.IsNullOrWhiteSpace(p_verificationCode))
		{
			Debug.LogError("[SetNewPassword] A valid verification code is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid verification code"));
			return false;
		}
		if (!ServerUtils.IsValidPassword(p_newPassword))
		{
			Debug.LogError("[SetNewPassword] A valid password is required.");
			UIManager.ShowMessage(LocalizationManager.Translate("error-invalid password format"));
			return false;
		}
		string content = JsonConvert.SerializeObject(new ServerDTOs.SetPasswordRequest
		{
			email = p_email,
			pinCode = p_verificationCode.Trim(),
			newPassword = p_newPassword
		});
		using (StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json"))
		{
			using (HttpResponseMessage res = await _client.PostAsync(Routes.Api("/auth/set-password"), content2, p_ct))
			{
				string text = await res.Content.ReadAsStringAsync();
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, text);
					return false;
				}
				PlayerSettings.Instance.Password = p_newPassword;
				Debug.Log(string.Format("[SetNewPassword] {0} {1}{2}", (int)res.StatusCode, res.StatusCode, string.IsNullOrWhiteSpace(text) ? string.Empty : (" - " + text)));
				Server.Instance.ClearAccessToken();
				return true;
			}
		}
	}
}
