// RadarMottuAPI/Dtos/MLDtos.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RadarMottuAPI.Dtos;

public record RssiEstimateRequestDto(
    [param: Required]                                
    [property: JsonPropertyName("rssiDbm")]          
    float RssiDbm
);

public record RssiEstimateResponseDto(
    [property: JsonPropertyName("estimatedMeters")]
    float EstimatedMeters
);
