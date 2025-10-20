using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RadarMottuAPI.ML;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/ml")]
[ApiVersion("1.0")]
public class MLController : ControllerBase
{
    private readonly MLEstimatorService _ml;
    public MLController(MLEstimatorService ml) => _ml = ml;

    public record RssiRequest(float RssiDbm);

    /// <summary>Estima a distância (m) a partir do RSSI (dBm) usando ML.NET.</summary>
    [HttpPost("estimate-distance")]
    public IActionResult Estimate([FromBody] RssiRequest req)
        => Ok(new { rssi = req.RssiDbm, estimatedMeters = _ml.PredictMeters(req.RssiDbm) });
}
