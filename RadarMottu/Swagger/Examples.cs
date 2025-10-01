using Swashbuckle.AspNetCore.Filters;
using RadarMottuAPI.Models;

namespace RadarMottuAPI.Swagger.Examples;

public class MotoExamples : IExamplesProvider<Moto>
{
    public Moto GetExamples()
    {
        return new Moto
        {
            Id = 1,
            Placa = "ABC1D23",
            Modelo = "Honda CG 160",
            Status = "disponivel",
            LastLat = -23.567,
            LastLng = -46.648
        };
    }
}
