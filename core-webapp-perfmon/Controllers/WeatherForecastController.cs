using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace core_webapp_perfmon.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    
    private static readonly ActivitySource Activity = new("Controller");
    
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using (var activity = Activity.StartActivity("GetWeatherForecast", ActivityKind.Server))
        {
            if (activity == null)
            {
                _logger.LogError("Activity is null");
            }
            WeatherForecast[] data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();

            activity?.SetTag("weathers_count", data.Length.ToString());
            
            return data;
        }
    }
}