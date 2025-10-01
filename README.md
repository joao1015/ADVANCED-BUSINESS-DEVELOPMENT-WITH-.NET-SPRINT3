![RadarMottuAPI](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![EF Core](https://img.shields.io/badge/EF%20Core-9.0-68217A?style=for-the-badge&logo=database&logoColor=white) ![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=white)

# 🏍️ **RadarMottuAPI – Advanced Business Development with .NET (Sprint 3)**

API RESTful em **.NET 8** para o domínio **RadarMottu** (gestão/rastreamento de motos com **Tags BLE** e **Anchors**).  
Implementa **CRUD completo** para 3 entidades, **paginação + HATEOAS**, e **Swagger/OpenAPI** com exemplos.

---

## 📑 Índice
1. [Projeto](#projeto)  
2. [Repositório do Projeto](#repositorio-do-projeto)  
3. [Equipe](#equipe)  
4. [Tech Stack](#tech-stack)  
5. [Sobre o Projeto](#sobre-o-projeto)  
6. [Domínio e Justificativa (25 pts)](#dominio-e-justificativa-25-pts)  
7. [Arquitetura & Estrutura de Pastas](#arquitetura--estrutura-de-pastas)  
8. [Como Rodar (Passo a Passo)](#como-rodar-passo-a-passo)  
9. [Configuração de Banco (SQLite/SQL Server)](#configuração-de-banco-sqlitesql-server)  
10. [Swagger & Exemplos (15 pts)](#swagger--exemplos-15-pts)  
11. [Endpoints & Exemplos (CRUD + REST)](#endpoints--exemplos-crud--rest)  
12. [Paginação + HATEOAS (50 pts)](#paginação--hateoas-50-pts)  
13. [Testes (Comandos)](#testes-comandos)  
14. [Checklist da Rubrica](#checklist-da-rubrica)  
15. [Problemas conhecidos & Soluções](#problemas-conhecidos--soluções)  
16. [Licença](#licença)

---

## <a name="projeto"></a>📌 **Projeto**

**RadarMottuAPI** é uma Web API em ASP.NET Core 8 que expõe recursos para **Motos**, **Tags BLE** e **Anchors** (antenas ESP32).  
Fornece endpoints para **criar, consultar, atualizar e excluir** registros, com **paginação** e **links HATEOAS** nas listagens.

---

## <a name="repositorio-do-projeto"></a>📂 **Repositório do Projeto**

> GitHub: **https://github.com/joao1015/ADVANCED-BUSINESS-DEVELOPMENT-WITH-.NET-SPRINT3**

---

## <a name="equipe"></a>👨‍💻 **Equipe**

| Nome | RM | Função |
|------|----|--------|
| **João Paulo Moreira dos Santos** | **557808** | Dev / Backend (.NET) |

_(adicione outros integrantes/RMs, se houver)_

---

## <a name="tech-stack"></a>⚙️ **Tech Stack**

- **Linguagem/Runtime:** C# / .NET 8  
- **Framework:** ASP.NET Core Web API  
- **ORM:** Entity Framework Core 9 (Providers: SQLite, SQL Server/Azure)  
- **Docs:** Swashbuckle (Swagger/OpenAPI) + Examples (Filters)  
- **Padrões:** REST, DTOs, Paginação, HATEOAS

Badges:  
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![EFCore](https://img.shields.io/badge/EF%20Core-9.0-68217A?style=flat)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat&logo=sqlite&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black)

---

## <a name="sobre-o-projeto"></a>📑 **Sobre o Projeto**

O sistema modela o contexto de pátios com frotas de motos:  
- **Motos** são os ativos principais.  
- **Tags BLE** identificam motos e emitem sinais.  
- **Anchors** capturam os sinais para estimar posição/telemetria.  

A API permite operações administrativas e integrações com outros serviços (app mobile, IoT, dashboard).

---

## <a name="dominio-e-justificativa-25-pts"></a>🧭 **Domínio e Justificativa (25 pts)**

- **Moto**: recurso de negócio central.  
- **Tag**: identificador BLE instalado na moto.  
- **Anchor**: ponto fixo que lê as tags (ESP32).  

> A tríade **Moto–Tag–Anchor** é comum em cenários de rastreamento de ativos, viabilizando **CRUD**, **paginação** e **navegação** (HATEOAS) – requisitos ideais para demonstrar **boas práticas REST**.

---

## <a name="arquitetura--estrutura-de-pastas"></a>🏗️ **Arquitetura & Estrutura de Pastas**

RadarMottuAPI/
├─ Controllers/ # API Controllers (Motos, Tags, Anchors)
├─ Data/ # AppDbContext (EF Core) e mapeamentos
├─ Dtos/ # DTOs de entrada/saída
├─ Models/ # Entidades de domínio + PagedResult + Link + PaginationQuery
├─ Services/ # HateoasLinkBuilder
├─ Swagger/ # Examples.cs (exemplos no Swagger)
├─ Migrations/ # EF Core (gerada por Add-Migration)
├─ Properties/launchSettings.json
├─ appsettings.json
├─ Program.cs
└─ RadarMottuAPI.csproj

yaml
Copiar código

**Boas práticas aplicadas:** separação por camadas, DTOs, status codes corretos, paginação, HATEOAS e documentação.

---

## <a name="como-rodar-passo-a-passo"></a>▶️ **Como Rodar (Passo a Passo)**

```bash
# 1) Restaurar pacotes
dotnet restore

# 2) (opcional) Instalar ferramenta de migrations
dotnet tool update --global dotnet-ef

# 3) Criar/atualizar o banco (SQLite por padrão)
dotnet ef database update

# 4) Executar a API
dotnet run
A API sobe (por padrão) em: http://localhost:5154

O Swagger abre em: http://localhost:5154/swagger (ver seção abaixo)

Caso use o Visual Studio, o launchSettings.json está configurado para abrir direto o Swagger.

<a name="configuração-de-banco-sqlitesql-server"></a>🗃️ Configuração de Banco (SQLite/SQL Server)
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
Program.cs (trocar provider, se necessário):

csharp
Copiar código
// SQLite (default)
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(conn));

// SQL Server / Azure
// builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));
<a name="swagger--exemplos-15-pts"></a>📖 Swagger & Exemplos (15 pts)
UI: http://localhost:5154/swagger

Configurado em Program.cs com:

SwaggerDoc + IncludeXmlComments (opcional)

Swashbuckle.AspNetCore.Filters para exemplos (Swagger/Examples.cs)

Acesso pela raiz vs /swagger:

Este projeto abre em /swagger (sem RoutePrefix = string.Empty).

Se quiser na raiz /, ajuste no Program.cs.

<a name="endpoints--exemplos-crud--rest"></a>🔗 Endpoints & Exemplos (CRUD + REST)
Motos
GET /api/Motos?page=1&pageSize=10

GET /api/Motos/{id}

POST /api/Motos

PUT /api/Motos/{id}

DELETE /api/Motos/{id}

Exemplo POST /api/Motos:

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
GET /api/Tags?page=1&pageSize=10

GET /api/Tags/{id}

POST /api/Tags

PUT /api/Tags/{id}

DELETE /api/Tags/{id}

Exemplo POST /api/Tags:

json
Copiar código
{
  "uid": "TAG-123456",
  "batteryLevel": 95,
  "status": "ativo",
  "motoId": null
}
Anchors
GET /api/Anchors?page=1&pageSize=10

GET /api/Anchors/{id}

POST /api/Anchors

PUT /api/Anchors/{id}

DELETE /api/Anchors/{id}

Exemplo POST /api/Anchors:

json
Copiar código
{
  "nome": "Anchor 1",
  "latitude": -23.567,
  "longitude": -46.648,
  "rangeMeters": 30,
  "status": "ativo"
}
Status codes adotados:
200 OK (GET), 201 Created (POST), 204 No Content (PUT/DELETE), 400 Bad Request, 404 Not Found.

<a name="paginação--hateoas-50-pts"></a>📦 Paginação + HATEOAS (50 pts)
As listagens retornam PagedResult<T> com metadados e links HATEOAS.

Exemplo GET /api/Motos?page=1&pageSize=2:

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
Cenários de teste sugeridos:

page=1&pageSize=2 → deve exibir 2 itens e next.

page=2&pageSize=2 → deve exibir itens seguintes, prev e possivelmente next.

Última página → sem next.

<a name="testes-comandos"></a>🧪 Testes (Comandos)
Mesmo sem testes implementados, a rubrica pede o comando.

bash
Copiar código
dotnet test
