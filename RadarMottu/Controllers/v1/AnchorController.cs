using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RadarMottuAPI.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/anchors")]
[ApiVersion("1.0")]
[Authorize]
public class AnchorController : ControllerBase
{
    private readonly IMongoCollection<BsonDocument> _anchors;
    public AnchorController(IMongoDatabase db) => _anchors = db.GetCollection<BsonDocument>("anchors");

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var list = await _anchors.Find(FilterDefinition<BsonDocument>.Empty).Limit(50).ToListAsync(ct);
        if (list.Count == 0)
        {
            list = new List<BsonDocument>
            {
                new() { { "id", "A-01" }, { "lat", -23.53 }, { "lng", -46.70 } },
                new() { { "id", "A-02" }, { "lat", -23.54 }, { "lng", -46.71 } }
            };
            await _anchors.InsertManyAsync(list, cancellationToken: ct);
        }
        return Ok(list);
    }
}
