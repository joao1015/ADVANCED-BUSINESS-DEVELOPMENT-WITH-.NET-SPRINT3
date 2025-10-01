namespace RadarMottuAPI.Models;

public class Anchor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RangeMeters { get; set; } = 30;
    public string Status { get; set; } = "ativo";
}
