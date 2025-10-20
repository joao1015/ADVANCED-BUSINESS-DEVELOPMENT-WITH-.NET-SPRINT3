using System.Reflection;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using RadarMottuAPI.ML;
using RadarMottuAPI.Services;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers =====
builder.Services.AddControllers();

// ===== Versionamento =====
builder.Services
    .AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

// ===== Swagger + Examples + JWT =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
    c.ExampleFilters();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RadarMottu API",
        Version = "v1",
        Description = "API para gerenciamento de Motos, Tags BLE e Anchors (ESP32)."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT no header. Ex: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<RadarMottuAPI.Swagger.Examples.MotoExamples>();

// ===== MongoDB (IMongoClient + IMongoDatabase em DI) =====
var mongoCfg = builder.Configuration.GetSection("MongoDb");
var mongoConn = mongoCfg.GetValue<string>("ConnectionString") ?? "mongodb://localhost:27017";
var mongoDbName = mongoCfg.GetValue<string>("Database") ?? "radarmottu";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbName);
});

// ===== Health Checks (inclui ping do MongoDB) =====
builder.Services.AddHealthChecks()
    .AddCheck("mongodb", new MongoPingHealthCheck(mongoConn, mongoDbName));

// ===== Autenticação/Autorização (JWT) =====
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt.GetValue<string>("Key")!;
var issuer = jwt.GetValue<string>("Issuer");
var audience = jwt.GetValue<string>("Audience");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization();

// ===== DI auxiliares =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HateoasLinkBuilder>();

// ===== ML.NET =====
builder.Services.AddSingleton<MLEstimatorService>();

var app = builder.Build();

// ===== Swagger =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RadarMottuAPI v1");
    });
}

// ===== Pipeline =====
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }

// ===== HealthCheck específico do Mongo =====
public class MongoPingHealthCheck : IHealthCheck
{
    private readonly string _conn;
    private readonly string _dbName;

    public MongoPingHealthCheck(string conn, string dbName)
    {
        _conn = conn;
        _dbName = dbName;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new MongoClient(_conn);
            var db = client.GetDatabase(_dbName);
            // comando 'ping' retorna ok: 1
            var result = await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            var ok = result.GetValue("ok", 0).ToDouble();
            return ok == 1.0 ? HealthCheckResult.Healthy("MongoDB OK") : HealthCheckResult.Unhealthy("MongoDB ping != 1");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB unreachable", ex);
        }
    }
}
