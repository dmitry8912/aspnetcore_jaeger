using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace core_webapp_perfmon.Controllers;

[ApiController]
[Route("[controller]")]
public class OtherController : ControllerBase
{
    private readonly ILogger<OtherController> _logger;
    
    private static readonly ActivitySource Activity = new("Controller");
    private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

    public OtherController(ILogger<OtherController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Dictionary<string, string> data)
    {
        Propagator.Extract(default, data, ExtractTraceContextFromBasicProperties);
        using (var activity = Activity.StartActivity("Other_Update", ActivityKind.Server))
        {
            await Task.Delay(1500);
            return Ok();
        }
    }
    
    private IEnumerable<string> ExtractTraceContextFromBasicProperties(Dictionary<string,string> props, string key)
    {
        try
        {
            if (props.TryGetValue(key, out var value))
            {
                return new[] { value };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to extract trace context: {ex}");
        }

        return Enumerable.Empty<string>();
    }
    
}