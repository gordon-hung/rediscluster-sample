using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisClusterSample.AppHost.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SortedSetController(
	IDatabase database) : ControllerBase
{
	[HttpGet]
	public async IAsyncEnumerable<WeatherForecast> GetAsync()
	{
		var key = string.Concat(typeof(SortedSetController).Name, ":", nameof(GetAsync));

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

			foreach (var weatherForecast in weatherForecasts.OrderBy(weatherForecast => weatherForecast.Date).Select((weatherForecast, index) => new { Index = index, WeatherForecast = weatherForecast }))
			{
				await database.SortedSetAddAsync(
					 key: key,
					 member: JsonSerializer.Serialize(weatherForecast.WeatherForecast),
					 weatherForecast.Index)
					.ConfigureAwait(false);
			}

			await database.KeyExpireAsync(
				key: key,
				expiry: TimeSpan.FromMinutes(1))
				.ConfigureAwait(false);
		}

		weatherForecasts = [];
		var redisValues = await database.SortedSetRandomMembersAsync(key, 5).ConfigureAwait(false);
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
