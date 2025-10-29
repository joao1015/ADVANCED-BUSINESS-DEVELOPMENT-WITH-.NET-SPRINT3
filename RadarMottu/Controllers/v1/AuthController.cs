using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RadarMottuAPI.Dtos;   

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg) => _cfg = cfg;

    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public IActionResult Login([FromBody] LoginDto dto)  
    {
        if (dto is null || dto.Usuario != "jp" || dto.Senha != "123")
            return Unauthorized();

        var key = _cfg["Jwt:Key"] ?? "DEV-KEY-CHANGE-ME";
        var issuer = _cfg["Jwt:Issuer"] ?? "radarmottu";
        var audience = _cfg["Jwt:Audience"] ?? "radarmottu-clients";

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[] { new Claim(ClaimTypes.Name, dto.Usuario) },
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
