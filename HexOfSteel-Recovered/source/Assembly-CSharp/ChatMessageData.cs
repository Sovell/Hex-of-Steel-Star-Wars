using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class ChatMessageData
{
	public string senderNickname;

	public string content;

	public string timeSent;

	public string hardwareID;

	public string userID;

	public ChatMessageData()
	{
	}

	public ChatMessageData(string p_senderNickname, string p_content)
	{
		senderNickname = p_senderNickname;
		content = p_content;
		timeSent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
		hardwareID = SystemInfo.deviceUniqueIdentifier;
		userID = PlayerSettings.Instance.UserId.ToString();
	}

	public string GenerateMessageContentForUIDisplay()
	{
		try
		{
			DateTime dateTime = DateTime.ParseExact(timeSent, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local).ToShortTimeString() + " " + senderNickname + ": " + content;
		}
		catch
		{
			return "TIME_ERROR " + senderNickname + ": " + content;
		}
	}
}
