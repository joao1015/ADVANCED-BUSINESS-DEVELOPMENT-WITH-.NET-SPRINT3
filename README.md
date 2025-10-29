



## 🚀 RadarMottuAPI

API RESTful em .NET 8 (C\#) para a disciplina Advanced Business Development with .NET — Sprint 4.
Domínio: RadarMottu — gestão e rastreamento de motos usando Tags BLE e Anchors (antenas/ESP32).

Status da entrega: CRUD completo, paginação + HATEOAS, Swagger/OpenAPI, Versionamento, JWT, Health Checks e endpoint ML.NET, além de testes unitários (xUnit) e integração (WebApplicationFactory).

-----

## 👥 Integrantes

  * Arthur Bispo de Lima — RM 557568
  * João Paulo Moreira dos Santos — RM 557808
  * Paulo André Carminati — RM 557881

-----

## 🎯 Rubrica & Objetivos

**Obrigatórios:**

  * 3 entidades: Motos, Tags, Anchors
  * CRUD com boas práticas REST
  * Paginação + HATEOAS
  * Swagger/OpenAPI documentado
  * Health Checks (`/health`)
  * Versionamento de API (v1)
  * Segurança com JWT (ou API Key) — usamos JWT
  * ML.NET em ao menos um endpoint
  * Testes: unitários (xUnit) e integração (WebApplicationFactory)
  * README com instruções de execução e testes

**Penalidades evitadas:**

  * Projeto compila ✅
  * Swagger atualizado ✅
  * README completo ✅

-----

## 🧱 Arquitetura & Tecnologias

  * ASP.NET Core 8 (Web API)
  * Entity Framework Core 9 (provider SQLite por padrão; SQL Server/Azure SQL opcional)
  * Swashbuckle.AspNetCore 9 (Swagger/OpenAPI)
  * JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
  * Health Checks (Mongo/DB opcional + `/health`)
  * ML.NET — estimativa de distância por RSSI (BLE)
  * xUnit + `Microsoft.AspNetCore.Mvc.Testing` (testes)

### Estrutura de pastas (resumo):

```text
RadarMottuAPI/
├─ Controllers/
│  └─ v1/
├─ Data/
├─ Dtos/
├─ Models/
├─ Services/
│  └─ ML/
├─ Swagger/                # (exemplos e filtros opcionais)
├─ Migrations/             # geradas pelo EF Core
├─ Properties/launchSettings.json
├─ appsettings.json
├─ Program.cs
└─ RadarMottuAPI.csproj

RadarMottuAPI.Tests/       # projeto de testes (xUnit)
```

-----

## ⚙️ Configuração & Execução (Local)

### Pré-requisitos

  * .NET SDK 8.0+
  * (Opcional) Ferramenta EF:
    ```bash
    dotnet tool update --global dotnet-ef
    ```

### 1\) Restaurar & Compilar

```bash
dotnet restore
dotnet build
```

### 2\) Banco de Dados

Por padrão usamos SQLite (arquivo `radarmottu.db` local).

`appsettings.json` (trecho):

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=radarmottu.db",
    "AzureSql": "Server=tcp:SEU-SERVIDOR.database.windows.net,1433;Initial Catalog=radarmottu;Persist Security Info=False;User ID=SEU-USUARIO;Password=SUA-SENHA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Jwt": {
    "Key": "trocar-em-producao",
    "Issuer": "RadarMottuAPI",
    "Audience": "RadarMottuClients",
    "ExpiresInMinutes": 60
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

Criar/Atualizar banco (migrations):

```bash
dotnet ef database update --project RadarMottuAPI
```

Usar SQL Server/Azure: troque o provider no `Program.cs` (`UseSqlServer`) e a connection `AzureSql` no `appsettings.json`.

### 3\) Rodar API

```bash
dotnet run --project RadarMottuAPI
```

  * **Swagger UI:** `http://localhost:5154/swagger`
  * **Health:** `GET http://localhost:5154/health`

O host/porta podem variar conforme `Properties/launchSettings.json`.

-----

## 🔐 Autenticação (JWT)

Enviar nos headers:

```
Authorization: Bearer <seu_token_jwt>
```

Endpoints liberados: `/health` e ML (para facilitar correção).
Demais endpoints (ex.: CRUDs) podem exigir JWT.

Exemplo de `appsettings.json` (JWT):

```json
"Jwt": {
  "Key": "trocar-em-producao",
  "Issuer": "RadarMottuAPI",
  "Audience": "RadarMottuClients",
  "ExpiresInMinutes": 60
}
```

-----

## 🧭 Versionamento & Swagger

  * **Rota base:** `api/v{version:apiVersion}/...`
  * **Versão atual:** `v1`
  * **Documentos:** `/swagger/v1/swagger.json`
  * **UI:** `/swagger`

Configurado com `Asp.Versioning` e `ConfigureSwaggerOptions` para publicar um doc por versão.

-----

## 📚 Recursos & Endpoints (CRUD + REST)

**Padrão comum:**
`GET /api/{Recurso}?page=&pageSize=` • `GET /api/{Recurso}/{id}` • `POST /api/{Recurso}` • `PUT /api/{Recurso}/{id}` • `DELETE /api/{Recurso}/{id}`

### Motos

  * **Listar:** `GET /api/v1/Motos?page=1&pageSize=10`
  * **Criar (POST):**
    ```json
    {
      "placa": "ABC1D23",
      "modelo": "Honda CG 160",
      "cor": "preta",
      "ano": 2023,
      "status": "disponivel",
      "tagCodigo": null
    }
    ```

### Tags

  * **Criar (POST):**
    ```json
    {
      "uid": "TAG-123456",
      "batteryLevel": 95,
      "status": "ativo",
      "motoId": null
    }
    ```

### Anchors

  * **Criar (POST):**
    ```json
    {
      "nome": "Anchor 1",
      "latitude": -23.567,
      "longitude": -46.648,
      "rangeMeters": 30,
      "status": "ativo"
    }
    ```

### Status Codes

  * `200 OK` (consultas)
  * `201 Created` (POST)
  * `204 No Content` (PUT/DELETE)
  * `400 Bad Request` (id/body inválido)
  * `404 Not Found` (id não existe)
  * `401/403` (se JWT exigido e inválido/ausente)

-----

## 🔗 Paginação + HATEOAS (exemplo)

**Requisição:** `GET /api/v1/Motos?page=1&pageSize=2`

**Resposta:**

```json
{
  "items": [
    { "id": 1, "placa": "ABC1D23", "modelo": "Honda CG 160" },
    { "id": 2, "placa": "XYZ9Z99", "modelo": "Yamaha Factor 150" }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 5,
  "totalPages": 3,
  "links": [
    { "rel": "self", "href": "http://localhost:5154/api/v1/Motos?page=1&pageSize=2", "method": "GET" },
    { "rel": "next", "href": "http://localhost:5154/api/v1/Motos?page=2&pageSize=2", "method": "GET" }
  ]
}
```

-----

## 🤖 Endpoint com ML.NET (requisito)

  * **Rota:** `POST /api/v1/ML/estimate-distance`
  * **Body (camelCase):**
    ```json
    { "rssiDbm": -65 }
    ```

Se você ativou PascalCase no `Program.cs` (desligando `PropertyNamingPolicy`), envie `{ "RssiDbm": -65 }`.

  * **Resposta:**

    ```json
    { "estimatedMeters": 2.87 }
    ```

  * **Curl de exemplo:**

    ```bash
    curl -X POST "http://localhost:5154/api/v1/ML/estimate-distance" \
      -H "Content-Type: application/json" \
      -d '{ "rssiDbm": -65 }'
    ```

-----

## ❤️ Health Checks (requisito)

  * **Rota:** `GET /health`
  * **Esperado:** `200 OK` (corpo padrão do health)
  * **Curl:**
    ```bash
    curl -i http://localhost:5154/health
    ```

-----

## 🧪 Testes (xUnit + WebApplicationFactory) — 30 pts + integração

### Estrutura

  * **Projeto:** `RadarMottuAPI.Tests`
  * **Testes unitários:** lógica do `MLEstimatorService`
  * **Testes integração:** `/health` e `/api/v1/ML/estimate-distance`

### Instalar pacotes no projeto de testes

```bash
dotnet new xunit -n RadarMottuAPI.Tests
dotnet add RadarMottuAPI.Tests/RadarMottuAPI.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add RadarMottuAPI.Tests/RadarMottuAPI.Tests.csproj package FluentAssertions

# Referenciar a API:
dotnet add RadarMottuAPI.Tests/RadarMottuAPI.Tests.csproj reference RadarMottuAPI/RadarMottuAPI.csproj
```

**Importante:** no `Program.cs` da API, existe `public partial class Program { }` (necessário para o `WebApplicationFactory`).

### Testes — Unitários (`MLEstimatorServiceTests.cs`)

```csharp
using FluentAssertions;
using RadarMottuAPI.Services.ML;
using Xunit;

public class MLEstimatorServiceTests
{
    [Fact]
    public void PredictMeters_DeveDiminuirComRSSIMelhor()
    {
        var ml = new MLEstimatorService();
        var longe = ml.PredictMeters(-80);
        var perto = ml.PredictMeters(-60);
        perto.Should().BeLessThan(longe);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void PredictMeters_EntradaInvalida_DeveRetornarNaN(float rssi)
    {
        var ml = new MLEstimatorService();
        ml.PredictMeters(rssi).Should().Be(float.NaN);
    }
}
```

### Testes — Integração (`IntegrationTests.cs`)

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RadarMottuAPI.Dtos;
using Xunit;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public IntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Health_DeveResponder200()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ML_EstimateDistance_DeveResponder200()
    {
        var client = _factory.CreateClient();
        var dto = new RssiEstimateRequestDto(-65);
        var resp = await client.PostAsJsonAsync("/api/v1/ML/estimate-distance", dto);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<RssiEstimateResponseDto>();
        body!.EstimatedMeters.Should().BeGreaterThan(0f);
    }
}
```

### Como executar os testes (README exige instruções)

```bash
dotnet test
```

Saída esperada (resumo): `Passed: 3 Failed: 0`

-----

## 🧰 EF Core — comandos úteis

**Criar migration inicial**

```bash
dotnet ef migrations add InitialCreate --project RadarMottuAPI
```

**Aplicar migration**

```bash
dotnet ef database update --project RadarMottuAPI
```

**Reverter para migration anterior**

```bash
dotnet ef database update NomeDaMigrationAnterior --project RadarMottuAPI
```

-----

## 🌐 Exemplos de Requisições (curl)

### Criar Moto

```bash
curl -X POST "http://localhost:5154/api/v1/Motos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{ "placa":"ABC1D23", "modelo":"Honda CG 160", "cor":"preta", "ano":2023, "status":"disponivel", "tagCodigo": null }'
```

### Listar Motos paginado

```bash
curl "http://localhost:5154/api/v1/Motos?page=1&pageSize=10" \
  -H "Authorization: Bearer <token>"
```

### Estimativa via ML.NET (liberado)

```bash
curl -X POST "http://localhost:5154/api/v1/ML/estimate-distance" \
  -H "Content-Type: application/json" \
  -d '{ "rssiDbm": -65 }'
```

### Health

```bash
curl -i http://localhost:5154/health
```
