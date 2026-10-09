using System.Collections.Generic;

public class SteamLeaderboard
{
	public string name;

	public List<SteamLeaderboardEntry> entries;

	public SteamLeaderboardEntry localPlayerEntry;

	public SteamLeaderboard(string p_name)
	{
		name = p_name;
		entries = new List<SteamLeaderboardEntry>();
	}
}
