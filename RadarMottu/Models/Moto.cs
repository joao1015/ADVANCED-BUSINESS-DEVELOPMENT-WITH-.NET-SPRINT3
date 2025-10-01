namespace RadarMottuAPI.Models;

public class Moto
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Status { get; set; } = "disponivel";
    public double? LastLat { get; set; }
    public double? LastLng { get; set; }

    public int? TagId { get; set; }
    public Tag? Tag { get; set; }
}
