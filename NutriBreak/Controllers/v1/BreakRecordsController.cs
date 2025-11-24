using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Persistence;
using NutriBreak.Domain;
using NutriBreak.DTOs;
using Asp.Versioning;

namespace NutriBreak.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/break-records")]
public class BreakRecordsController : ControllerBase
{
    private readonly NutriBreakDbContext _db;
    private readonly LinkGenerator _links;

    public BreakRecordsController(NutriBreakDbContext db, LinkGenerator links)
    {
        _db = db;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetBreaks([FromQuery] PaginationParameters pagination)
    {
        var query = _db.BreakRecords.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(b => new BreakRecordDto(b.Id, b.UserId, b.StartedAt, b.DurationMinutes, b.Type, b.Mood, b.EnergyLevel, b.ScreenTimeMinutes))
            .ToListAsync();
        var result = new
        {
            total,
            pagination.PageNumber,
            pagination.PageSize,
            items,
            links = new
            {
                self = Url.Action(null, null, new { pageNumber = pagination.PageNumber, pageSize = pagination.PageSize }, Request.Scheme),
                next = pagination.PageNumber * pagination.PageSize < total ? Url.Action(null, null, new { pageNumber = pagination.PageNumber + 1, pageSize = pagination.PageSize }, Request.Scheme) : null,
                prev = pagination.PageNumber > 1 ? Url.Action(null, null, new { pageNumber = pagination.PageNumber - 1, pageSize = pagination.PageSize }, Request.Scheme) : null,
                create = _links.GetPathByAction(HttpContext, nameof(CreateBreak), "BreakRecords", new { version = HttpContext.GetRequestedApiVersion() })
            }
        };
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetBreak(Guid id)
    {
        var breakRecord = await _db.BreakRecords.FindAsync(id);
        if (breakRecord == null) return NotFound();
        var dto = new BreakRecordDto(breakRecord.Id, breakRecord.UserId, breakRecord.StartedAt, breakRecord.DurationMinutes, breakRecord.Type, breakRecord.Mood, breakRecord.EnergyLevel, breakRecord.ScreenTimeMinutes);
        var links = new
        {
            self = _links.GetPathByAction(HttpContext, nameof(GetBreak), "BreakRecords", new { id, version = HttpContext.GetRequestedApiVersion() }),
            update = _links.GetPathByAction(HttpContext, nameof(UpdateBreak), "BreakRecords", new { id, version = HttpContext.GetRequestedApiVersion() }),
            delete = _links.GetPathByAction(HttpContext, nameof(DeleteBreak), "BreakRecords", new { id, version = HttpContext.GetRequestedApiVersion() })
        };
        return Ok(new { data = dto, links });
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateBreak([FromBody] CreateBreakRecordRequest request)
    {
        var userExists = await _db.Users.AnyAsync(x => x.Id == request.UserId);
        if (!userExists) return BadRequest(new { message = "User does not exist" });
        var br = new BreakRecord
        {
            UserId = request.UserId,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            Mood = request.Mood,
            EnergyLevel = request.EnergyLevel,
            ScreenTimeMinutes = request.ScreenTimeMinutes
        };
        _db.BreakRecords.Add(br);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBreak), new { id = br.Id, version = HttpContext.GetRequestedApiVersion() }, new { id = br.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBreak(Guid id, [FromBody] UpdateBreakRecordRequest request)
    {
        var br = await _db.BreakRecords.FindAsync(id);
        if (br == null) return NotFound();
        br.DurationMinutes = request.DurationMinutes;
        br.Type = request.Type;
        br.Mood = request.Mood;
        br.EnergyLevel = request.EnergyLevel;
        br.ScreenTimeMinutes = request.ScreenTimeMinutes;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBreak(Guid id)
    {
        var br = await _db.BreakRecords.FindAsync(id);
        if (br == null) return NotFound();
        _db.BreakRecords.Remove(br);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
