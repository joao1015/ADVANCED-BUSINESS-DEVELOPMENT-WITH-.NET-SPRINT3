![RadarMottuAPI](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![EFCore](https://img.shields.io/badge/EF%20Core-9.0-68217A?style=for-the-badge&logo=database&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)

# 🚀 RadarMottuAPI

API RESTful em **.NET 8** (C#) para a disciplina **Advanced Business Development with .NET — Sprint 3**.  
Domínio: **RadarMottu** (gestão/rastreamento de motos com **Tags BLE** e **Anchors/antenas**).  
Entrega com **CRUD completo**, **paginação + HATEOAS**, e **Swagger/OpenAPI** com exemplos.

---

## 👥 Integrantes
- **Arthur Bispo de Lima — RM 557568**
- **João Paulo Moreira dos Santos — RM 557808**
- **Paulo André Carminati — RM 557881**

---

## 🎯 Objetivos da Sprint (rubrica)

- **3 entidades principais**: `Motos`, `Tags`, `Anchors`  
- **CRUD** com status codes adequados  
- **Boas práticas REST** (rotas, verbos, códigos, ids)  
- **Paginação** + **HATEOAS** em listagens  
- **Swagger/OpenAPI** com exemplos de payload e modelos  
- **README completo** com instruções e exemplos  
- **Projeto compila e roda** ✅

---

## 🏍️ Domínio & Justificativa (25 pts)

- **Moto**: ativo principal (frota)  
- **Tag (BLE)**: identificador instalado na moto; emite sinais para leitura  
- **Anchor (Antena/ESP32)**: ponto fixo que lê as tags e ajuda a estimar posição

> O trio **Moto–Tag–Anchor** representa fielmente um cenário de **rastreio de ativos**.  
> Atende às necessidades de CRUD, consulta paginada e navegação (HATEOAS), demonstrando práticas REST.

---

## 🧱 Arquitetura

- **.NET 8 Web API**
- **Entity Framework Core 9** (SQLite por padrão; suporta SQL Server/Azure SQL)
- **Swashbuckle.AspNetCore 9** (Swagger/OpenAPI + UI)
- **DTOs** para contratos de entrada/saída
- **HATEOAS** para navegação de páginas

**Estrutura de pastas:**
```text
RadarMottuAPI/
├─ Controllers/
├─ Data/
├─ Dtos/
├─ Models/
├─ Services/
├─ Swagger/
├─ Migrations/                # gerada pelas migrations
├─ Properties/launchSettings.json
├─ appsettings.json
├─ Program.cs
└─ RadarMottuAPI.csproj
🧰 Tecnologias & Pacotes
.NET SDK: 8.0+

EF Core: 9.0.0
Pacotes:

Microsoft.EntityFrameworkCore

Microsoft.EntityFrameworkCore.Sqlite

Microsoft.EntityFrameworkCore.SqlServer (opcional)

Microsoft.EntityFrameworkCore.Tools

Swagger:

Swashbuckle.AspNetCore (9.0.5)

Swashbuckle.AspNetCore.Filters (9.0.0) (exemplos no Swagger)

Se usar SQL Server/Azure, troque o provider no Program.cs e a connection em appsettings.json.

## ⚙️ Configuração & Execução (local)
1) Restaurar pacotes
bash
Copiar código
dotnet restore
2) Criar banco via EF Core (SQLite por padrão)
bash
Copiar código
dotnet ef database update
Caso não tenha as ferramentas:

bash
Copiar código
dotnet tool update --global dotnet-ef
3) Executar API
bash
Copiar código
dotnet run
Por padrão, a API sobe em http://localhost:5154 (ajustável no launchSettings.json).

🗃️ Configuração de Banco
appsettings.json (padrão SQLite):

json
Copiar código
{
  "ConnectionStrings": {
    "Default": "Data Source=radarmottu.db",
    "AzureSql": "Server=tcp:SEU-SERVIDOR.database.windows.net,1433;Initial Catalog=radarmottu;Persist Security Info=False;User ID=SEU-USUARIO;Password=SUA-SENHA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
Trocar para SQL Server/Azure (opcional)
No Program.cs, substitua:

csharp
Copiar código
// SQLite (default)
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(conn));

// SQL Server / Azure (opcional)
//// builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));
📖 Swagger / OpenAPI (15 pts)
Interface disponível em: http://localhost:5154/swagger
Documentação gerada automaticamente com exemplos de payload (via Swagger/Examples.cs).

Como abrir direto em /swagger
No Program.cs (em app.Environment.IsDevelopment()):

csharp
Copiar código
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RadarMottuAPI v1");
    // sem RoutePrefix => UI em /swagger
});
E no Properties/launchSettings.json:

json
Copiar código
"launchUrl": "swagger"
🔄 Endpoints (CRUD + boas práticas) — 50 pts
Todos os recursos seguem o padrão:

GET /api/{Recurso}?page=&pageSize=

GET /api/{Recurso}/{id}

POST /api/{Recurso}

PUT /api/{Recurso}/{id}

DELETE /api/{Recurso}/{id}

Motos
Listar: GET /api/Motos?page=1&pageSize=10

Criar (POST):

json
Copiar código
{
  "placa": "ABC1D23",
  "modelo": "Honda CG 160",
  "status": "disponivel",
  "lastLat": -23.567,
  "lastLng": -46.648,
  "tagId": null
}
Tags
Criar (POST):

json
Copiar código
{
  "uid": "TAG-123456",
  "batteryLevel": 95,
  "status": "ativo",
  "motoId": null
}
Anchors
Criar (POST):

json
Copiar código
{
  "nome": "Anchor 1",
  "latitude": -23.567,
  "longitude": -46.648,
  "rangeMeters": 30,
  "status": "ativo"
}
Status codes adotados
200 OK (consultas), 201 Created (POST), 204 No Content (PUT/DELETE), 400 Bad Request (id/body inválido), 404 Not Found (id inexistente).

📦 Paginação + HATEOAS (exemplo de resposta)
GET /api/Motos?page=1&pageSize=2

json
Copiar código
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
    { "rel": "self", "href": "http://localhost:5154/api/Motos?page=1&pageSize=2", "method": "GET" },
    { "rel": "next", "href": "http://localhost:5154/api/Motos?page=2&pageSize=2", "method": "GET" }
  ]
}
🧪 Cenários de Teste de Paginação
Cenário	Requisição	Resultado esperado
Primeira página	GET /api/Motos?page=1&pageSize=2	2 itens, links com self e next
Página intermediária	GET /api/Motos?page=2&pageSize=2	2 itens, links com prev e next
Última página	GET /api/Motos?page=3&pageSize=2	1 item (se total=5), links com prev
PageSize customizado	GET /api/Tags?page=1&pageSize=5	Até 5 itens, totalItems/totalPages coerentes
Outra entidade	GET /api/Anchors?page=1&pageSize=3	Itens e links conforme total cadastrado

🧪 Testes
(Se houver projeto de testes)

bash
Copiar código
dotnet test
(Sem testes ainda)
Comando exigido pela rubrica: dotnet test — documentado para futura cobertura.

🛠️ Comandos úteis (EF Core)
Criar migration inicial

bash
Copiar código
dotnet ef migrations add InitialCreate
Aplicar migration

bash
Copiar código
dotnet ef database update
Reverter para uma migration anterior

bash
Copiar código
dotnet ef database update LastGoodMigrationName
