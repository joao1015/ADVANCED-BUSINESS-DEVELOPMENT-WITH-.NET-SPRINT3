namespace RadarMottuAPI.Dtos;

public record MotoCreateDto(string Placa, string Modelo, string Status, double? LastLat, double? LastLng, int? TagId);
public record MotoUpdateDto(string Placa, string Modelo, string Status, double? LastLat, double? LastLng, int? TagId);
public record MotoReadDto(int Id, string Placa, string Modelo, string Status, double? LastLat, double? LastLng, int? TagId);
