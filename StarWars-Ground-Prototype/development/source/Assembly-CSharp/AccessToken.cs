using System;

public sealed class AccessToken
{
	private const float SKEW_SECONDS = 60f;

	public string Type { get; private set; } = "Bearer";

	public string Value { get; private set; }

	public DateTime ExpirationUtc { get; private set; }

	public bool IsAboutToExpire => ExpirationUtc - DateTime.UtcNow <= TimeSpan.FromSeconds(60.0);

	public bool IsExpired => DateTime.UtcNow >= ExpirationUtc;

	public bool IsValid
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Value))
			{
				return !IsExpired;
			}
			return false;
		}
	}

	public AccessToken(string p_type, string p_value, DateTimeOffset p_expirationUtc)
	{
		Type = p_type;
		Value = p_value;
		ExpirationUtc = p_expirationUtc.UtcDateTime;
	}
}
