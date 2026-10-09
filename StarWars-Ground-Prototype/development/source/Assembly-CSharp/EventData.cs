public class EventData
{
	public Player playerCapturing;

	public Player playerLosing;

	public EventData(Player p_playerCapturing)
	{
		playerCapturing = p_playerCapturing;
	}

	public EventData(Player p_playerCapturing, Player p_playerLosing)
	{
		playerCapturing = p_playerCapturing;
		playerLosing = p_playerLosing;
	}

	public EventData Clone()
	{
		return new EventData(playerCapturing, playerLosing);
	}
}
