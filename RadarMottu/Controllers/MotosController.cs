using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RadarMottuAPI.Dtos;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/motos")]
[ApiVersion("1.0")]
[Authorize]
public class MotosController : ControllerBase
{
    private readonly IMongoCollection<BsonDocument> _motos;

    public MotosController(IMongoDatabase db)
    {
        _motos = db.GetCollection<BsonDocument>("motos");
    }

    // --------- HELPERS ---------
    private static MotoReadDto MapToReadDto(BsonDocument d)
    {
        // ID nunca nulo: tenta "_id" como ObjectId; fallback para "id" string; senão vazio.
        string id =
            (d.TryGetValue("_id", out var vId) && vId.BsonType == BsonType.ObjectId)
                ? vId.AsObjectId.ToString()
                : (d.TryGetValue("id", out var vIdStr) ? vIdStr.ToString() : string.Empty);

        string placa = d.GetValue("placa", "").ToString();
        string modelo = d.GetValue("modelo", "").ToString();
        string cor = d.GetValue("cor", "").ToString();
        int ano = d.GetValue("ano", 0).ToInt32();
        string status = d.GetValue("status", "").ToString();
        string? tagCodigo = d.TryGetValue("tagCodigo", out var vTag)
                            ? (vTag.IsBsonNull ? null : vTag.ToString())
                            : null;

        return new MotoReadDto(id, placa, modelo, cor, ano, status, tagCodigo);
    }

    // --------- ENDPOINTS ---------

    [HttpGet]
    [AllowAnonymous] // se quiser testar sem JWT
    public async Task<ActionResult<IEnumerable<MotoReadDto>>> GetAll(CancellationToken ct)
    {
        var list = await _motos.Find(FilterDefinition<BsonDocument>.Empty).Limit(200).ToListAsync(ct);

        // Seed opcional (remova se não quiser inserir nada automaticamente)
        if (list.Count == 0)
        {
            var seed = new[]
            {
                new BsonDocument { { "placa","ABC1D23" }, { "modelo","CG 160" }, { "cor","preta" }, { "ano", 2022 }, { "status","ativa" }, { "tagCodigo", BsonNull.Value } },
                new BsonDocument { { "placa","DEF4G56" }, { "modelo","Factor 150" }, { "cor","vermelha" }, { "ano", 2021 }, { "status","manutenção" }, { "tagCodigo", "TAG-0002" } }
            };
            await _motos.InsertManyAsync(seed, cancellationToken: ct);
            list = seed.ToList();
        }

        return Ok(list.Select(MapToReadDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MotoReadDto>> GetById(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objId)) return NotFound();

        var doc = await _motos.Find(Builders<BsonDocument>.Filter.Eq("_id", objId)).FirstOrDefaultAsync(ct);
        if (doc is null) return NotFound();

        return Ok(MapToReadDto(doc));
    }

    [HttpPost]
    public async Task<ActionResult<MotoReadDto>> Create([FromBody] MotoCreateDto dto, CancellationToken ct)
    {
        var doc = new BsonDocument
        {
            { "placa", dto.Placa },
            { "modelo", dto.Modelo },
            { "cor", dto.Cor },
            { "ano", dto.Ano },
            { "status", dto.Status },
            { "tagCodigo", dto.TagCodigo is null ? BsonNull.Value : new BsonString(dto.TagCodigo) }
        };

        await _motos.InsertOneAsync(doc, cancellationToken: ct);

        var read = MapToReadDto(doc);
        return CreatedAtAction(nameof(GetById), new { id = read.Id, version = "1.0" }, read);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MotoUpdateDto dto, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objId)) return NotFound("id inválido");

        var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        var updates = new List<UpdateDefinition<BsonDocument>>
        {
            Builders<BsonDocument>.Update.Set("placa", dto.Placa),
            Builders<BsonDocument>.Update.Set("modelo", dto.Modelo),
            Builders<BsonDocument>.Update.Set("cor", dto.Cor),
            Builders<BsonDocument>.Update.Set("ano", dto.Ano),
            Builders<BsonDocument>.Update.Set("status", dto.Status)
        };

        // >>> Aqui evitamos o CS0411: sem ternário misturando string/BsonNull
        if (dto.TagCodigo is null)
            updates.Add(Builders<BsonDocument>.Update.Unset("tagCodigo")); // ou .Set("tagCodigo", BsonNull.Value)
        else
            updates.Add(Builders<BsonDocument>.Update.Set("tagCodigo", dto.TagCodigo));

        var update = Builders<BsonDocument>.Update.Combine(updates);

        var result = await _motos.UpdateOneAsync(filter, update, cancellationToken: ct);
        if (result.MatchedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objId)) return NotFound("id inválido");

        var result = await _motos.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", objId), ct);
        if (result.DeletedCount == 0) return NotFound();

        return NoContent();
    }
}
