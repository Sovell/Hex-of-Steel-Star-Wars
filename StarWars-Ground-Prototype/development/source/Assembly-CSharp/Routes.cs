using System;

public static class Routes
{
	public static class Auth
	{
		public const string Base = "/auth";

		public const string Register = "/auth/register";

		public const string ConfirmEmail = "/auth/confirm-email";

		public const string ResendMailConfirmation = "/auth/resend-email-confirm";

		public const string Login = "/auth/login";

		public const string Logout = "/auth/logout";

		public const string RefreshAccessToken = "/auth/refresh-access-token";

		public const string ForgotPassword = "/auth/forgot-password";

		public const string SetPassword = "/auth/set-password";
	}

	public static class Users
	{
		public static class Me
		{
			public const string Base = "/users/me";

			public const string Language = "/users/me/change-language";

			public const string Password = "/users/me/change-password";

			public const string Bio = "/users/me/change-bio";

			public const string Avatar = "/users/me/change-avatar";

			public const string Delete = "/users/me/delete-account";
		}

		public const string Base = "/users";

		public const string Profile = "/users/profile/{username}";

		public const string Avatar = "/users/profile/avatar/{username}";
	}

	public static class Games
	{
		public static class Turns
		{
			public const string Base = "/games/turns";

			public const string Start = "/games/turns/start/{id:guid}";

			public const string End = "/games/turns/end/{id:guid}";
		}

		public const string Base = "/games";

		public const string Fetch = "/games/fetch";

		public const string Host = "/games/host";

		public const string Details = "/games/details/{id:guid}";

		public const string Join = "/games/join";

		public const string Leave = "/games/leave";

		public const string Kick = "/games/kick";

		public const string Forfeit = "/games/forfeit";

		public const string End = "/games/end/{id:guid}";

		public const string Delete = "/games/delete/{id:guid}";
	}

	public static class Metrics
	{
		public const string Base = "/metrics";

		public const string All = "/metrics/all";

		public const string Games = "/metrics/games";

		public const string Users = "/metrics/users";
	}

	public const string Server = "https://hex-of-steel.fr";

	public const string Version = "v1";

	public const string ApiPrefix = "/v1";

	public const string Home = "/";

	public const string Health = "/health";

	public const string Manual = "/manual";

	public const string DataPolicy = "/data-policy";

	public const string PrivacyPolicy = "/privacy-policy";

	public const string ChatBansFile = "/chat-bans.json";

	public const string ChatBans = "https://hex-of-steel.fr/chat-bans.json";

	public static string Api(string relative)
	{
		return Join("/v1", relative);
	}

	private static string Join(string a, string b)
	{
		if (string.IsNullOrEmpty(a))
		{
			return b ?? string.Empty;
		}
		if (string.IsNullOrEmpty(b))
		{
			return a ?? string.Empty;
		}
		return a.TrimEnd('/') + "/" + b.TrimStart('/');
	}

	public static string Format(string route, Guid id)
	{
		route = route.Replace("{id:guid}", Uri.EscapeDataString(id.ToString()));
		return route;
	}

	public static string Format(string route, string username)
	{
		route = route.Replace("{username}", Uri.EscapeDataString(username));
		return route;
	}
}
