// RadarMottuAPI/Controllers/v1/MLController.cs
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadarMottuAPI.Dtos;
using RadarMottuAPI.Services.ML;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MLController : ControllerBase
{
    private readonly MLEstimatorService _ml;
    public MLController(MLEstimatorService ml) => _ml = ml;

    /// <summary>Estima a distância (em metros) a partir do RSSI (dBm).</summary>
    [HttpPost("estimate-distance")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RssiEstimateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<RssiEstimateResponseDto> Estimate([FromBody] RssiEstimateRequestDto dto)
    {
        if (float.IsNaN(dto.RssiDbm) || float.IsInfinity(dto.RssiDbm) || dto.RssiDbm > 0)
            return BadRequest(new ProblemDetails { Title = "RSSI inválido", Detail = "Forneça um RSSI negativo em dBm (ex.: -65)." });

        var meters = _ml.PredictMeters(dto.RssiDbm);

        if (float.IsNaN(meters) || float.IsInfinity(meters) || meters < 0)
            return BadRequest(new ProblemDetails { Title = "Estimativa inválida", Detail = "Não foi possível estimar a distância." });

        return Ok(new RssiEstimateResponseDto(meters));
    }
}
