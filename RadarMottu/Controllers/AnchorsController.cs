using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarMottuAPI.Data;
using RadarMottuAPI.Models;
using RadarMottuAPI.Services;

namespace RadarMottuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnchorsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HateoasLinkBuilder _links;

    public AnchorsController(AppDbContext context, HateoasLinkBuilder links)
    {
        _context = context;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Anchor>>> GetAnchors([FromQuery] PaginationQuery query)
    {
        var total = await _context.Anchors.CountAsync();
        var items = await _context.Anchors
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var result = new PagedResult<Anchor>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };

        result.Links.AddRange(_links.GetLinks("Anchors", query.Page, query.PageSize, total));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Anchor>> GetAnchor(int id)
    {
        var anchor = await _context.Anchors.FindAsync(id);
        if (anchor == null) return NotFound();
        return Ok(anchor);
    }

    [HttpPost]
    public async Task<ActionResult<Anchor>> PostAnchor(Anchor anchor)
    {
        _context.Anchors.Add(anchor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAnchor), new { id = anchor.Id }, anchor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAnchor(int id, Anchor anchor)
    {
        if (id != anchor.Id) return BadRequest();
        _context.Entry(anchor).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnchor(int id)
    {
        var anchor = await _context.Anchors.FindAsync(id);
        if (anchor == null) return NotFound();

        _context.Anchors.Remove(anchor);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
