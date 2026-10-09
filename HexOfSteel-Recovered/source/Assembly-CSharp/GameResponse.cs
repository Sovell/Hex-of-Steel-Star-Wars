using Newtonsoft.Json;

public class GameResponse
{
	[JsonProperty("response")]
	public Response Response { get; set; }
}
