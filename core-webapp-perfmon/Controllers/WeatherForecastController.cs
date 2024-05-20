using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
    private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int? id)
    {
        var entity = new {id = id};
        return Ok(entity);
        
    }
    
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using (var activity = Activity.StartActivity("GetWeatherForecast", ActivityKind.Server))
        {
            WeatherForecast[] data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            
            if (activity != null)
            {
                var carrier = new Dictionary<string, string>();
                Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), carrier, InjectContextIntoHeader);
    
                var client = new HttpClient();
                var result = client.PostAsync("http://localhost:5123/Other/", 
                    new StringContent(JsonConvert.SerializeObject(carrier))).Result;
            }
            
            return data;
        }
    }
    
    private void InjectContextIntoHeader(Dictionary<string, string> props, string key, string value)
    {
        try
        {
            props ??= new Dictionary<string, string>();
            props[key] = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}