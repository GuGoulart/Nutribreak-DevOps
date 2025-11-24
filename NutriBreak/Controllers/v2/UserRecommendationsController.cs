using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Persistence;

namespace NutriBreak.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users/{id:guid}/recommendations")]
public class UserRecommendationsController : ControllerBase
{
    private readonly NutriBreakDbContext _db;
    public UserRecommendationsController(NutriBreakDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetRecommendations(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Coletar dados para heurística simples
        var recentBreak = await _db.BreakRecords
            .Where(b => b.UserId == id)
            .OrderByDescending(b => b.StartedAt)
            .FirstOrDefaultAsync();
        var lastMeal = await _db.Meals
            .Where(m => m.UserId == id)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        // Lógica simples de exemplo
        var minutesSinceLastBreak = recentBreak != null ? (DateTime.UtcNow - recentBreak.StartedAt).TotalMinutes : double.MaxValue;
        var nextBreakIn = minutesSinceLastBreak > 60 ? 0 : Math.Max(0, 60 - (int)minutesSinceLastBreak);
        var suggestedBreakType = recentBreak == null || minutesSinceLastBreak > 120 ? "alongamento" : "pausa curta";
        var suggestedMeal = lastMeal == null ? "refeição leve" : (lastMeal.TimeOfDay == "lunch" ? "snack saudável" : "refeição balanceada");

        var response = new
        {
            userId = id,
            nextBreakInMinutes = nextBreakIn,
            suggestedBreakType,
            suggestedMeal,
            links = new
            {
                user = Url.Action("GetById", "Users", new { id, version = "1.0" }, Request.Scheme),
                breaks = Url.Action(null, "BreakRecords", new { version = "1.0" }, Request.Scheme),
                meals = Url.Action(null, "Meals", new { version = "1.0" }, Request.Scheme)
            }
        };
        return Ok(response);
    }
}
