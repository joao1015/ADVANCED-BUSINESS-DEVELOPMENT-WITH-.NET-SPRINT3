namespace RadarMottuAPI.Dtos;

public record TagCreateDto(string Uid, int BatteryLevel, string Status, int? MotoId);
public record TagUpdateDto(string Uid, int BatteryLevel, string Status, int? MotoId);
public record TagReadDto(int Id, string Uid, int BatteryLevel, string Status, int? MotoId);
