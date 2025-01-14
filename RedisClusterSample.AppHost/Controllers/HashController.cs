using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisClusterSample.AppHost.Controllers;
[Route("api/[controller]")]
[ApiController]
public class HashController(
	IDatabase database) : ControllerBase
{
	[HttpGet]
	public async IAsyncEnumerable<WeatherForecast> GetAsync()
	{
		var key = string.Concat(typeof(HashController).Name, ":", nameof(GetAsync));

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

			await database.HashSetAsync(
				key: key,
				hashFields: weatherForecasts
				.OrderBy(weatherForecast => weatherForecast.Date)
				.Select((weatherForecast, index) => new HashEntry(index, JsonSerializer.Serialize(weatherForecast)))
				.ToArray())
				.ConfigureAwait(false);

			await database.KeyExpireAsync(
				key: key,
				expiry: TimeSpan.FromMinutes(1))
				.ConfigureAwait(false);
		}

		var hashValues = await database.HashGetAllAsync(key).ConfigureAwait(false);
		foreach (var entry in hashValues.OrderBy(hash => hash.Name))
		{
			if (string.IsNullOrWhiteSpace(entry.Value))
			{
				continue;
			}

			yield return JsonSerializer.Deserialize<WeatherForecast>(entry.Value!)!;
		}
	}
}
