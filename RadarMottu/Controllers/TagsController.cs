using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RadarMottuAPI.Dtos;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/tags")]
[ApiVersion("1.0")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IMongoCollection<BsonDocument> _tags;

    public TagsController(IMongoDatabase db)
    {
        _tags = db.GetCollection<BsonDocument>("tags");
    }

    /// <summary>Lista as tags</summary>
    [HttpGet]
    [AllowAnonymous] // deixe anônimo enquanto testa
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _tags.Find(FilterDefinition<BsonDocument>.Empty).Limit(200).ToListAsync(ct);
        // Mapeia p/ saída amigável
        var result = list.Select(d => new {
            id = d.GetValue("_id", BsonNull.Value).ToString(),
            codigo = d.GetValue("codigo", "").AsString,
            mac = d.GetValue("mac", "").AsString,
            rssiCalibrado = d.Contains("rssiCalibrado") ? d["rssiCalibrado"].ToInt32() : 0,
            bateriaPercent = d.Contains("bateriaPercent") ? d["bateriaPercent"].ToInt32() : 0,
            status = d.GetValue("status", "").AsString
        });
        return Ok(result);
    }

    /// <summary>Cria uma tag</summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] TagCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var doc = new BsonDocument {
            { "codigo", dto.Codigo },
            { "mac", dto.Mac },
            { "rssiCalibrado", dto.RssiCalibrado },
            { "bateriaPercent", dto.BateriaPercent },
            { "status", dto.Status }
        };

        await _tags.InsertOneAsync(doc, cancellationToken: ct);

        return CreatedAtAction(nameof(GetAll), new { }, new
        {
            id = doc["_id"].ToString(),
            codigo = dto.Codigo,
            mac = dto.Mac,
            rssiCalibrado = dto.RssiCalibrado,
            bateriaPercent = dto.BateriaPercent,
            status = dto.Status
        });
    }

    /// <summary>Atualiza uma tag</summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(string id, [FromBody] TagUpdateDto dto, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var oid)) return NotFound("id inválido");

        var update = Builders<BsonDocument>.Update
            .Set("codigo", dto.Codigo)
            .Set("mac", dto.Mac)
            .Set("rssiCalibrado", dto.RssiCalibrado)
            .Set("bateriaPercent", dto.BateriaPercent)
            .Set("status", dto.Status);

        var result = await _tags.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", oid),
            update,
            cancellationToken: ct);

        if (result.MatchedCount == 0) return NotFound();

        return NoContent();
    }

    /// <summary>Remove uma tag</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var oid)) return NotFound("id inválido");
        var result = await _tags.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", oid), ct);
        if (result.DeletedCount == 0) return NotFound();
        return NoContent();
    }
}
