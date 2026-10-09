using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine;

public static class ServerErrorHandler
{
	public static void HandleErrorResponse(HttpResponseMessage res, string body)
	{
		if (res == null)
		{
			Debug.LogError("[ServerError] Null HttpResponseMessage.");
			UIManager.ShowMessage(Localize("error-unexpected"));
			return;
		}
		HttpRequestMessage requestMessage = res.RequestMessage;
		string text = requestMessage?.Method?.Method ?? "?";
		string text2 = requestMessage?.RequestUri?.ToString() ?? "<unknown>";
		int statusCode = (int)res.StatusCode;
		ServerDTOs.ProblemDetails problemDetails = null;
		if (!string.IsNullOrWhiteSpace(body) && LooksLikeJson(body))
		{
			problemDetails = TryParseProblemDetails(body);
		}
		string text3 = problemDetails?.title ?? res.ReasonPhrase ?? "Server error";
		_ = problemDetails?.detail;
		Debug.LogError($"[ServerError] {statusCode} {text3} @ [{text}] {text2} ");
		if (IsTransientGateway(res.StatusCode))
		{
			UIManager.ShowMessage(Localize("error-server 500"));
			return;
		}
		if (res.StatusCode == HttpStatusCode.TooManyRequests)
		{
			UIManager.ShowMessage(Localize("error-server 429"));
			return;
		}
		if (problemDetails != null && !string.IsNullOrWhiteSpace(problemDetails.title))
		{
			UIManager.ShowMessage(Localize(problemDetails.title));
			return;
		}
		if (res.StatusCode == HttpStatusCode.Unauthorized)
		{
			UIManager.ShowMessage(Localize("error-server 401"));
			return;
		}
		if (res.StatusCode == HttpStatusCode.Forbidden)
		{
			UIManager.ShowMessage(Localize("error-server 403"));
			return;
		}
		string p_key = problemDetails?.title ?? "error-unexpected";
		UIManager.ShowMessage($"[{statusCode}] - {Localize(p_key)}");
	}

	private static bool LooksLikeJson(string s)
	{
		string text = s?.TrimStart();
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (!text.StartsWith("{"))
			{
				return text.StartsWith("[");
			}
			return true;
		}
		return false;
	}

	private static ServerDTOs.ProblemDetails TryParseProblemDetails(string json)
	{
		try
		{
			return JsonConvert.DeserializeObject<ServerDTOs.ProblemDetails>(json);
		}
		catch (JsonException)
		{
			return null;
		}
		catch (Exception ex2)
		{
			Debug.LogWarning("[ServerError] ProblemDetails parse error: " + ex2.Message);
			return null;
		}
	}

	private static bool IsTransientGateway(HttpStatusCode code)
	{
		if (code != HttpStatusCode.BadGateway && code != HttpStatusCode.ServiceUnavailable)
		{
			return code == HttpStatusCode.GatewayTimeout;
		}
		return true;
	}

	private static string TruncateOneLine(string s, int max)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return s;
		}
		s = s.Replace("\r", " ").Replace("\n", " ");
		if (s.Length > max)
		{
			return s.Substring(0, max) + "…";
		}
		return s;
	}

	private static string Localize(string p_key)
	{
		try
		{
			return LocalizationManager.Translate(p_key) ?? p_key;
		}
		catch
		{
			return p_key;
		}
	}
}
