namespace RedisClusterSample.AppHost;

public record RedisOptions
{
	public string ConnectionString { get; set; } = "localhost";
}
