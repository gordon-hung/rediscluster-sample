using Microsoft.Extensions.Options;
using StackExchange.Redis;
using RedisClusterSample.AppHost;

namespace RedisClusterSample.AppHost;

internal class RedisConnectionFactory : IDisposable
{
	private readonly ConnectionMultiplexer _connectionMultiplexer;
	private bool _disposedValue;

	public IDatabase Database { get; }

	public RedisConnectionFactory(IOptions<RedisOptions> optionsAccessor)
	{
		var options = optionsAccessor.Value;
		_connectionMultiplexer = ConnectionMultiplexer.Connect(options.ConnectionString);
		Database = _connectionMultiplexer.GetDatabase();
	}

	public void Dispose()
	{
		// 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_connectionMultiplexer.Close();
				_connectionMultiplexer.Dispose();
			}

			_disposedValue = true;
		}
	}
}
