using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisClusterSample.AppHost.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ListController(
	IDatabase database) : ControllerBase
{
	[HttpGet]
	public async IAsyncEnumerable<WeatherForecast> GetAsync()
	{
		var key = string.Concat(typeof(ListController).Name, ":", nameof(GetAsync));

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
				await database.ListRightPushAsync(
					 key: key,
					 value: JsonSerializer.Serialize(weatherForecast))
					.ConfigureAwait(false);
			}

			await database.KeyExpireAsync(
				key: key,
				expiry: TimeSpan.FromMinutes(1))
				.ConfigureAwait(false);
		}

		foreach (var entry in await database.ListRangeAsync(key).ConfigureAwait(false))
		{
			yield return JsonSerializer.Deserialize<WeatherForecast>(entry!)!;
		}
	}
}
