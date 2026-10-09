using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static class ServerUtils
{
	public const int BIO_MAX_LENGTH = 100;

	public const int USERNAME_MIN_LENGTH = 2;

	public const int USERNAME_MAX_LENGTH = 16;

	public const int PASSWORD_MIN_LENGTH = 8;

	public const int PASSWORD_MAX_LENGTH = 32;

	public const int SESSION_NAME_MIN_LENGTH = 2;

	public const int SESSION_NAME_MAX_LENGTH = 100;

	public const int SESSION_DESCRIPTION_MAX_LENGTH = 50;

	public const int AVATAR_IMAGE_SIZE_BYTES = 131072;

	private static readonly Regex PasswordRegex = new Regex("^\\S{8,32}$", RegexOptions.Compiled);

	private static readonly Regex UsernameRegex = new Regex("^(?:(?:\\p{L}\\p{M}*)|\\p{N}|[_-]){2,16}$", RegexOptions.Compiled);

	private static readonly Regex SessionNameRegex = new Regex("^[^\\r\\n]{2,100}$", RegexOptions.Compiled);

	private static readonly Regex SessionDescriptionRegex = new Regex("^[^\\r\\n]{0,50}$", RegexOptions.Compiled);

	public static string Sha256Base64(byte[] p_data)
	{
		using (SHA256 sHA = SHA256.Create())
		{
			return Convert.ToBase64String(sHA.ComputeHash(p_data));
		}
	}

	public static bool VerifySha256Base64(byte[] p_data, string p_base64HashFromClient)
	{
		using (SHA256 sHA = SHA256.Create())
		{
			byte[] array = sHA.ComputeHash(p_data);
			return CryptographicOperations.FixedTimeEquals(right: Convert.FromBase64String(p_base64HashFromClient), left: array);
		}
	}

	public static bool IsValidEmail(string p_email)
	{
		if (string.IsNullOrWhiteSpace(p_email))
		{
			return false;
		}
		try
		{
			string text = new MailAddress(p_email).Address.ToLowerInvariant();
			string[] source = new string[2] { "hex-of-steel.fr", "war-frogs-studio.fr" };
			string value = text.Split('@').Last();
			if (source.Contains(value))
			{
				return false;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsValidBio(string p_bio)
	{
		if (p_bio != null)
		{
			return p_bio.Length <= 100;
		}
		return true;
	}

	public static bool IsValidAvatar(byte[] p_bytes)
	{
		if (p_bytes.Length > 131072)
		{
			return false;
		}
		return LooksLikeJpeg(p_bytes);
	}

	public static bool LooksLikeJpeg(ReadOnlySpan<byte> p_bytes)
	{
		if (p_bytes.Length >= 3 && p_bytes[0] == byte.MaxValue && p_bytes[1] == 216)
		{
			return p_bytes[2] == byte.MaxValue;
		}
		return false;
	}

	public static bool IsValidPassword(string s)
	{
		if (s != null)
		{
			return PasswordRegex.IsMatch(NormalizeNfc(s));
		}
		return false;
	}

	public static bool IsValidUsername(string s)
	{
		if (s != null)
		{
			return UsernameRegex.IsMatch(NormalizeNfc(s));
		}
		return false;
	}

	public static bool IsValidSessionName(string s)
	{
		if (s != null)
		{
			return SessionNameRegex.IsMatch(NormalizeNfc(s));
		}
		return false;
	}

	public static bool IsValidSessionDescription(string s)
	{
		if (s != null)
		{
			return SessionDescriptionRegex.IsMatch(NormalizeNfc(s));
		}
		return false;
	}

	private static string NormalizeNfc(string s)
	{
		return s.Normalize(NormalizationForm.FormC);
	}
}
