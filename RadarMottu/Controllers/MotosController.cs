using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadarMottuAPI.Data;
using RadarMottuAPI.Models;
using RadarMottuAPI.Services;

namespace RadarMottuAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HateoasLinkBuilder _links;

    public MotosController(AppDbContext context, HateoasLinkBuilder links)
    {
        _context = context;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Moto>>> GetMotos([FromQuery] PaginationQuery query)
    {
        var total = await _context.Motos.CountAsync();
        var items = await _context.Motos
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var result = new PagedResult<Moto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };

        result.Links.AddRange(_links.GetLinks("Motos", query.Page, query.PageSize, total));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Moto>> GetMoto(int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null) return NotFound();
        return Ok(moto);
    }

    [HttpPost]
    public async Task<ActionResult<Moto>> PostMoto(Moto moto)
    {
        _context.Motos.Add(moto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, moto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutMoto(int id, Moto moto)
    {
        if (id != moto.Id) return BadRequest();
        _context.Entry(moto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMoto(int id)
    {
        var moto = await _context.Motos.FindAsync(id);
        if (moto == null) return NotFound();

        _context.Motos.Remove(moto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
