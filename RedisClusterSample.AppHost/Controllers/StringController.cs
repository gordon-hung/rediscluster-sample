using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisClusterSample.AppHost.Controllers;
[Route("api/[controller]")]
[ApiController]
public class StringController(
	IDatabase database) : ControllerBase
{
	[HttpGet]
	public async IAsyncEnumerable<WeatherForecast> GetAsync()
	{
		var key = string.Concat(typeof(StringController).Name, ":", nameof(GetAsync));

		IEnumerable<WeatherForecast> weatherForecasts = default!;

		if (!await database.KeyExistsAsync(
			key: key)
			.ConfigureAwait(false))
		{
			weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
			}).ToArray();

			await database.StringSetAsync(
				key: key,
				value: JsonSerializer.Serialize(weatherForecasts),
				expiry: TimeSpan.FromMinutes(1))
				.ConfigureAwait(false);
		}
		else
		{
			var redisValue = await database.StringGetAsync(
				key: key)
				.ConfigureAwait(false);

			if (!string.IsNullOrWhiteSpace(redisValue))
			{
				weatherForecasts = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(redisValue!)!;
			}
		}

		foreach (var weatherForecast in weatherForecasts)
		{
			yield return weatherForecast;
		}
	}
}
