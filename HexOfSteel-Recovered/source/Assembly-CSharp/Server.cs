using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Server : MonoBehaviour
{
	private const float TIMEOUT = 30f;

	private const float TOKEN_VALIDITY_CHECK_INTERVAL = 10f;

	private const string GAME_VERSION_HEADER = "Game-Version";

	private static readonly HttpClient _client = new HttpClient
	{
		BaseAddress = new Uri("https://hex-of-steel.fr"),
		Timeout = TimeSpan.FromSeconds(30.0)
	};

	private ServerAuthService _authService;

	private ServerUsersService _usersService;

	private ServerGameService _gameService;

	private AccessToken _accessToken;

	private float _lastTokenValidityCheckTime;

	public static Server Instance { get; private set; }

	public bool IsClientConnected
	{
		get
		{
			if (_client != null)
			{
				return _client.BaseAddress != null;
			}
			return false;
		}
	}

	public bool IsUserAuthenticated
	{
		get
		{
			if (IsClientConnected && _accessToken != null && _client.DefaultRequestHeaders.Authorization != null)
			{
				return !string.IsNullOrWhiteSpace(_client.DefaultRequestHeaders.Authorization.Parameter);
			}
			return false;
		}
	}

	public ServerGameService GameService => _gameService;

	public ServerAuthService AuthService => _authService;

	public ServerUsersService UsersService => _usersService;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		Instance = this;
		_client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Application.productName.Replace(' ', '-'), Application.version.Replace(' ', '.')));
		_client.DefaultRequestHeaders.Remove("Game-Version");
		_client.DefaultRequestHeaders.Add("Game-Version", Application.version);
		_authService = new ServerAuthService(_client);
		_gameService = new ServerGameService(_client, _authService);
		_usersService = new ServerUsersService(_client, _authService);
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		ProcessAccessTokenValidity();
	}

	private void ProcessAccessTokenValidity()
	{
		if (IsUserAuthenticated)
		{
			if (_accessToken.IsExpired)
			{
				Debug.LogWarning("[Server] Access token expired; clearing auth and prompting login");
				ClearAccessToken();
				UIManager.ShowMessage(LocalizationManager.Translate("server.problems.token_expired"));
			}
			else if (_accessToken.IsAboutToExpire && Time.time - _lastTokenValidityCheckTime > 10f)
			{
				_lastTokenValidityCheckTime = Time.time;
				_authService.RefreshAccessToken(_accessToken, CancellationToken.None);
			}
		}
	}

	public void ApplyToken(AccessToken p_accessToken)
	{
		_accessToken = p_accessToken;
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(p_accessToken.Type, p_accessToken.Value);
	}

	public void ClearAccessToken()
	{
		_accessToken = null;
		_client.DefaultRequestHeaders.Authorization = null;
	}

	public async Task CheckServerConnexion(CancellationToken p_ct)
	{
		if (!IsClientConnected)
		{
			Debug.LogWarning("[Ping] Client is not connected");
			return;
		}
		try
		{
			using (HttpResponseMessage res = await _client.GetAsync("/health", p_ct))
			{
				if (!res.IsSuccessStatusCode)
				{
					ServerErrorHandler.HandleErrorResponse(res, await res.Content.ReadAsStringAsync());
					return;
				}
				p_ct.ThrowIfCancellationRequested();
				Debug.Log("[Ping] Server is reachable.");
				UIManager.ShowMessage(LocalizationManager.Translate("info-server reachable"));
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
			UIManager.ShowMessage(LocalizationManager.Translate(ex.Message));
		}
	}
}
