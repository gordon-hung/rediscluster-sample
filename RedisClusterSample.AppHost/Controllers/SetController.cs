using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisClusterSample.AppHost.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SetController(
	IDatabase database) : ControllerBase
{
	[HttpGet]
	public async IAsyncEnumerable<WeatherForecast> GetAsync()
	{
		var key = string.Concat(typeof(SetController).Name, ":", nameof(GetAsync));

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

			foreach (var weatherForecast in weatherForecasts.OrderBy(weatherForecast => weatherForecast.Date))
			{
				await database.SetAddAsync(
					 key: key,
					 value: JsonSerializer.Serialize(weatherForecast))
					.ConfigureAwait(false);
			}

			await database.KeyExpireAsync(
				key: key,
				expiry: TimeSpan.FromMinutes(1))
				.ConfigureAwait(false);
		}

		weatherForecasts = [];
		var redisValues = await database.SetMembersAsync(key).ConfigureAwait(false);
		foreach (var redisValue in redisValues)
		{
			if (string.IsNullOrWhiteSpace(redisValue))
			{
				continue;
			}

			weatherForecasts = weatherForecasts.Append(JsonSerializer.Deserialize<WeatherForecast>(redisValue!)!);

		}

		foreach (var weatherForecast in weatherForecasts.OrderBy(weatherForecast => weatherForecast.Date))
		{
			yield return weatherForecast;
		}
	}
}
