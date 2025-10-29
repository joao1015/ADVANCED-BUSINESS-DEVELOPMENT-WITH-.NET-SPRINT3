using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RadarMottuAPI.Controllers.v1;

public record AnchorCreateDto(string Nome, double Latitude, double Longitude, double RangeMeters, string Status);
public record AnchorReadDto(string Id, string Nome, double Latitude, double Longitude, double RangeMeters, string Status);

[ApiController]
[Route("api/v{version:apiVersion}/anchors")]
[ApiVersion("1.0")]
[Authorize] // deixe [AllowAnonymous] nos endpoints enquanto testa, se quiser
public class AnchorsController : ControllerBase
{
    private readonly IMongoCollection<BsonDocument> _anchors;

    public AnchorsController(IMongoDatabase db)
    {
        _anchors = db.GetCollection<BsonDocument>("anchors");
    }

    // GET /api/v1/anchors
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AnchorReadDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var docs = await _anchors.Find(FilterDefinition<BsonDocument>.Empty)
                                 .Limit(200)
                                 .ToListAsync(ct);

        // Se quiser sem "seed", remova este bloco:
        if (docs.Count == 0)
        {
            var seed = new[]
            {
                new BsonDocument { { "nome", "Anchor A-01" }, { "latitude", -23.53 }, { "longitude", -46.70 }, { "rangeMeters", 12.5 }, { "status", "ativo" } },
                new BsonDocument { { "nome", "Anchor A-02" }, { "latitude", -23.54 }, { "longitude", -46.71 }, { "rangeMeters", 10.0 }, { "status", "ativo" } }
            };
            await _anchors.InsertManyAsync(seed, cancellationToken: ct);
            docs = seed.ToList();
        }

        var result = docs.Select(d => new AnchorReadDto(
            d["_id"].AsObjectId.ToString(),
            d.GetValue("nome", "").AsString,
            d.GetValue("latitude", 0d).ToDouble(),
            d.GetValue("longitude", 0d).ToDouble(),
            d.GetValue("rangeMeters", 0d).ToDouble(),
            d.GetValue("status", "").AsString
        ));

        return Ok(result);
    }

    // GET /api/v1/anchors/{id}
    [HttpGet("{id:length(24)}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AnchorReadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var oid)) return NotFound();

        var doc = await _anchors.Find(Builders<BsonDocument>.Filter.Eq("_id", oid))
                                .FirstOrDefaultAsync(ct);
        if (doc is null) return NotFound();

        var dto = new AnchorReadDto(
            doc["_id"].AsObjectId.ToString(),
            doc.GetValue("nome", "").AsString,
            doc.GetValue("latitude", 0d).ToDouble(),
            doc.GetValue("longitude", 0d).ToDouble(),
            doc.GetValue("rangeMeters", 0d).ToDouble(),
            doc.GetValue("status", "").AsString
        );

        return Ok(dto);
    }

    // POST /api/v1/anchors
    [HttpPost]
    [ProducesResponseType(typeof(AnchorReadDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] AnchorCreateDto anchor, CancellationToken ct)
    {
        if (anchor is null) return BadRequest("Corpo inválido.");

        var doc = new BsonDocument
        {
            { "nome", anchor.Nome },
            { "latitude", anchor.Latitude },
            { "longitude", anchor.Longitude },
            { "rangeMeters", anchor.RangeMeters },
            { "status", anchor.Status },
            { "createdAt", DateTime.UtcNow }
        };

        await _anchors.InsertOneAsync(doc, cancellationToken: ct);

        var read = new AnchorReadDto(
            doc["_id"].AsObjectId.ToString(),
            anchor.Nome,
            anchor.Latitude,
            anchor.Longitude,
            anchor.RangeMeters,
            anchor.Status
        );

        return CreatedAtAction(nameof(GetById), new { id = read.Id, version = "1.0" }, read);
    }
}
