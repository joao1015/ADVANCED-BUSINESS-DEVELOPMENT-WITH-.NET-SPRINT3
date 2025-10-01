using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarMottuAPI.Data;
using RadarMottuAPI.Models;
using RadarMottuAPI.Services;

namespace RadarMottuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HateoasLinkBuilder _links;

    public TagsController(AppDbContext context, HateoasLinkBuilder links)
    {
        _context = context;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Tag>>> GetTags([FromQuery] PaginationQuery query)
    {
        var total = await _context.Tags.CountAsync();
        var items = await _context.Tags
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var result = new PagedResult<Tag>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };

        result.Links.AddRange(_links.GetLinks("Tags", query.Page, query.PageSize, total));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> PostTag(Tag tag)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTag(int id, Tag tag)
    {
        if (id != tag.Id) return BadRequest();
        _context.Entry(tag).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
