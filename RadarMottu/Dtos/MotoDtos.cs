namespace RadarMottuAPI.Dtos;

public record MotoCreateDto(
    string Placa,
    string Modelo,
    string Cor,
    int Ano,
    string Status,
    string? TagCodigo // opcional: vínculo da TAG BLE
);

public record MotoUpdateDto(
    string Placa,
    string Modelo,
    string Cor,
    int Ano,
    string Status,
    string? TagCodigo
);

public record MotoReadDto(
    string Id,
    string Placa,
    string Modelo,
    string Cor,
    int Ano,
    string Status,
    string? TagCodigo
);
