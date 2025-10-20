using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/health")]
[ApiVersion("1.0")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy", utc = DateTime.UtcNow });
}
