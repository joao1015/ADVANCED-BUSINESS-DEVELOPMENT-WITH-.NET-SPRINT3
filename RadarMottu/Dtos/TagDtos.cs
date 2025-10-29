// Dtos/TagDtos.cs
namespace RadarMottuAPI.Dtos;

public record TagCreateDto(
    string Codigo,
    string Mac,
    int RssiCalibrado,
    int BateriaPercent,
    string Status
);

public record TagUpdateDto(
    string Codigo,
    string Mac,
    int RssiCalibrado,
    int BateriaPercent,
    string Status
);
