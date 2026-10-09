using Newtonsoft.Json;

public class Response
{
	[JsonProperty("game_count")]
	public long GameCount { get; set; }

	[JsonProperty("games")]
	public Game[] Games { get; set; }
}
