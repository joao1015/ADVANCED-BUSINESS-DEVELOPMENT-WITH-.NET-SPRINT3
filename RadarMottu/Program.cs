using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RadarMottuAPI.Data;
using RadarMottuAPI.Services;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Conexão de banco local (SQLite). Para Azure, troque no appsettings.json
var conn = builder.Configuration.GetConnectionString("Default") ?? "Data Source=radarmottu.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(conn));

// 🔹 Controllers
builder.Services.AddControllers();

// 🔹 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.ExampleFilters();

    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RadarMottu API",
        Version = "v1",
        Description = "API para gerenciamento de Motos, Tags BLE e Anchors (antenas ESP32)."
    });
});

// 🔹 Carregar exemplos definidos em Swagger/Examples.cs
builder.Services.AddSwaggerExamplesFromAssemblyOf<RadarMottuAPI.Swagger.Examples.MotoExamples>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HateoasLinkBuilder>();

var app = builder.Build();

// 🔹 Ativar Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RadarMottuAPI v1");
        // ⚠️ aqui não tem c.RoutePrefix = string.Empty;
        // isso garante que o Swagger abre em /swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
