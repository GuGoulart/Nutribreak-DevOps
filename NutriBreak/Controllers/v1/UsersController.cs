using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Persistence;
using NutriBreak.Domain;
using NutriBreak.DTOs;
using Asp.Versioning;

namespace NutriBreak.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController : ControllerBase
{
    private readonly NutriBreakDbContext _db;
    private readonly LinkGenerator _links;

    public UsersController(NutriBreakDbContext db, LinkGenerator links)
    {
        _db = db;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetUsers([FromQuery] PaginationParameters pagination)
    {
        var query = _db.Users.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.WorkMode))
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
                create = _links.GetPathByAction(HttpContext, action: nameof(CreateUser), controller: "Users", values: new { version = HttpContext.GetRequestedApiVersion() })
            }
        };

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        var dto = new UserDto(user.Id, user.Name, user.Email, user.WorkMode);
        var links = new
        {
            self = _links.GetPathByAction(HttpContext, nameof(GetById), "Users", new { id, version = HttpContext.GetRequestedApiVersion() }),
            update = _links.GetPathByAction(HttpContext, nameof(UpdateUser), "Users", new { id, version = HttpContext.GetRequestedApiVersion() }),
            delete = _links.GetPathByAction(HttpContext, nameof(DeleteUser), "Users", new { id, version = HttpContext.GetRequestedApiVersion() })
        };
        return Ok(new { data = dto, links });
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserRequest request)
    {
        var exists = await _db.Users.AnyAsync(x => x.Email == request.Email);
        if (exists) return Conflict(new { message = "Email already registered" });

        var user = new User { Name = request.Name, Email = request.Email, WorkMode = request.WorkMode };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { id = user.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Name = request.Name;
        user.WorkMode = request.WorkMode;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
