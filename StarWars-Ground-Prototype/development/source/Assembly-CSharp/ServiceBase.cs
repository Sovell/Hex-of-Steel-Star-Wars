using System.Net.Http;

public abstract class ServiceBase
{
	protected readonly HttpClient _client;

	public ServiceBase(HttpClient p_client)
	{
		_client = p_client;
	}
}
