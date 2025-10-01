namespace RadarMottuAPI.Models;

public class Tag
{
    public int Id { get; set; }
    public string Uid { get; set; } = string.Empty;
    public int BatteryLevel { get; set; } = 100;
    public string Status { get; set; } = "ativo";

    public int? MotoId { get; set; }
    public Moto? Moto { get; set; }
}
