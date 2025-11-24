using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Persistence;
using NutriBreak.Domain;
using NutriBreak.DTOs;
using Asp.Versioning;

namespace NutriBreak.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/meals")]
public class MealsController : ControllerBase
{
    private readonly NutriBreakDbContext _db;
    private readonly LinkGenerator _links;

    public MealsController(NutriBreakDbContext db, LinkGenerator links)
    {
        _db = db;
        _links = links;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetMeals([FromQuery] PaginationParameters pagination)
    {
        var query = _db.Meals.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(m => new MealDto(m.Id, m.UserId, m.Title, m.Calories, m.TimeOfDay))
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
                create = _links.GetPathByAction(HttpContext, nameof(CreateMeal), "Meals", new { version = HttpContext.GetRequestedApiVersion() })
            }
        };

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetMeal(Guid id)
    {
        var meal = await _db.Meals.FindAsync(id);
        if (meal == null) return NotFound();

        var dto = new MealDto(meal.Id, meal.UserId, meal.Title, meal.Calories, meal.TimeOfDay);

        var links = new
        {
            self = _links.GetPathByAction(HttpContext, nameof(GetMeal), "Meals", new { id, version = HttpContext.GetRequestedApiVersion() }),
            update = _links.GetPathByAction(HttpContext, nameof(UpdateMeal), "Meals", new { id, version = HttpContext.GetRequestedApiVersion() }),
            delete = _links.GetPathByAction(HttpContext, nameof(DeleteMeal), "Meals", new { id, version = HttpContext.GetRequestedApiVersion() })
        };

        return Ok(new { data = dto, links });
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateMeal([FromBody] CreateMealRequest request)
    {
        var userExists = await _db.Users.AnyAsync(x => x.Id == request.UserId);
        if (!userExists) return BadRequest(new { message = "User does not exist" });

        var meal = new Meal
        {
            UserId = request.UserId,
            Title = request.Title,
            Calories = request.Calories,
            TimeOfDay = request.TimeOfDay,
            ConsumedAt = DateTime.UtcNow, 
            CreatedAt = DateTime.UtcNow
        };

        _db.Meals.Add(meal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeal), new { id = meal.Id, version = HttpContext.GetRequestedApiVersion() }, new { id = meal.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMeal(Guid id, [FromBody] UpdateMealRequest request)
    {
        var meal = await _db.Meals.FindAsync(id);
        if (meal == null) return NotFound();

        meal.Title = request.Title;
        meal.Calories = request.Calories;
        meal.TimeOfDay = request.TimeOfDay;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMeal(Guid id)
    {
        var meal = await _db.Meals.FindAsync(id);
        if (meal == null) return NotFound();

        _db.Meals.Remove(meal);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
