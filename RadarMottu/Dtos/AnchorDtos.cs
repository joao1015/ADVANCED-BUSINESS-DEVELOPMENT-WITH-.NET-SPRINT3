namespace RadarMottuAPI.Dtos;

public record AnchorCreateDto(string Nome, double Latitude, double Longitude, double RangeMeters, string Status);
public record AnchorUpdateDto(string Nome, double Latitude, double Longitude, double RangeMeters, string Status);
public record AnchorReadDto(int Id, string Nome, double Latitude, double Longitude, double RangeMeters, string Status);
