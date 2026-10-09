public class SteamLeaderboardEntry
{
	public string nickname;

	public int score;

	public int rank;

	public bool isLocalClient;

	public SteamLeaderboardEntry(string p_nickname, int p_score, int p_rank, bool p_isLocalClient)
	{
		nickname = p_nickname;
		score = p_score;
		rank = p_rank;
		isLocalClient = p_isLocalClient;
	}
}
