using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using RadarMottuAPI.Services;
using RadarMottuAPI.Services.ML; // serviço ML (namespace alinhado)

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers =====
builder.Services.AddControllers();

// (opcional) manter PascalCase no JSON (senão use camelCase no body)
// builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = null);

// ===== Versionamento (gera grupos v1, v2, ...) =====
builder.Services
    .AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'V";          // => "v1" (evita "v1.0")
        o.SubstituteApiVersionInUrl = true;
    });

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.CustomSchemaIds(t => t.FullName);
    c.MapType<BsonDocument>(() => new OpenApiSchema { Type = "object", AdditionalPropertiesAllowed = true });
    c.MapType<ObjectId>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString(ObjectId.GenerateNewId().ToString()) });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT no header. Ex.: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});
// gera 1 doc por versão
builder.Services.ConfigureOptions<RadarMottuAPI.ConfigureSwaggerOptions>();

// ===== MongoDB (Atlas-friendly: TLS 1.2 + ServerApi V1) =====
var mongoCfg = builder.Configuration.GetSection("MongoDb");
var mongoConn = mongoCfg.GetValue<string>("ConnectionString") ?? "mongodb://localhost:27017";
var mongoDbName = mongoCfg.GetValue<string>("Database") ?? "radarmottu";

builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var settings = MongoClientSettings.FromConnectionString(mongoConn);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
    settings.ConnectTimeout = TimeSpan.FromSeconds(15);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
    return new MongoClient(settings);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbName);
});

// ===== Health Checks =====
builder.Services.AddHealthChecks().AddCheck<RadarMottuAPI.MongoPingHealthCheck>("mongodb");

// ===== JWT =====
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt.GetValue<string>("Key") ?? "DEV-KEY-CHANGE-ME";
var issuer = jwt.GetValue<string>("Issuer") ?? "radarmottu";
var audience = jwt.GetValue<string>("Audience") ?? "radarmottu-clients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ===== DI auxiliares =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HateoasLinkBuilder>();
builder.Services.AddSingleton<MLEstimatorService>();

var app = builder.Build();

// ===== Swagger UI =====
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"RadarMottu API {desc.GroupName.ToUpperInvariant()}");
    // options.RoutePrefix = string.Empty; // opcional: servir na raiz
});

// ===== Pipeline =====
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

namespace RadarMottuAPI
{
    using Asp.Versioning.ApiExplorer;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Swashbuckle.AspNetCore.SwaggerGen;

    // Swagger por versão
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = "RadarMottu API",
                    Version = desc.ApiVersion.ToString(),
                    Description = "API para gerenciamento de Motos, Tags BLE e Anchors (ESP32)."
                });
            }
        }
    }

    // HealthCheck do Mongo
    public class MongoPingHealthCheck : IHealthCheck
    {
        private readonly IMongoClient _client;
        private readonly string _dbName;

        public MongoPingHealthCheck(IMongoClient client, IConfiguration cfg)
        {
            _client = client;
            _dbName = cfg.GetSection("MongoDb").GetValue<string>("Database") ?? "radarmottu";
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _client.GetDatabase(_dbName);
                var result = await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                return result.GetValue("ok", 0).ToDouble() == 1.0
                    ? HealthCheckResult.Healthy("MongoDB OK")
                    : HealthCheckResult.Unhealthy("MongoDB ping != 1");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB unreachable", ex);
            }
        }
    }
}
