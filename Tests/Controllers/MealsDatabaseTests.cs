using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Domain;
using NutriBreak.Persistence;

namespace Tests.Controllers;

public class MealsDatabaseTests : IDisposable
{
    private readonly NutriBreakDbContext _context;
    private readonly Guid _testUserId;

    public MealsDatabaseTests()
    {
        var options = new DbContextOptionsBuilder<NutriBreakDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NutriBreakDbContext(options);
        
        _testUserId = Guid.NewGuid();
        _context.Users.Add(new User { Id = _testUserId, Name = "Test User", Email = "test@test.com" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task DbContext_DevePermitirAdicionarMeal()
    {
        var meal = new Meal { UserId = _testUserId, Title = "Café", Calories = 300, TimeOfDay = "breakfast" };
        
        _context.Meals.Add(meal);
        await _context.SaveChangesAsync();

        var mealInDb = await _context.Meals.FindAsync(meal.Id);
        mealInDb.Should().NotBeNull();
        mealInDb!.Title.Should().Be("Café");
    }

    [Fact]
    public async Task DbContext_DevePermitirListarMeals()
    {
        _context.Meals.AddRange(
            new Meal { UserId = _testUserId, Title = "Almoço", Calories = 600, TimeOfDay = "lunch" },
            new Meal { UserId = _testUserId, Title = "Jantar", Calories = 500, TimeOfDay = "dinner" }
        );
        await _context.SaveChangesAsync();

        var meals = await _context.Meals.ToListAsync();
        meals.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

public class BreakRecordsDatabaseTests : IDisposable
{
    private readonly NutriBreakDbContext _context;
    private readonly Guid _testUserId;

    public BreakRecordsDatabaseTests()
    {
        var options = new DbContextOptionsBuilder<NutriBreakDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NutriBreakDbContext(options);
        
        _testUserId = Guid.NewGuid();
        _context.Users.Add(new User { Id = _testUserId, Name = "Test User", Email = "test@test.com" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task DbContext_DevePermitirAdicionarBreakRecord()
    {
        var breakRecord = new BreakRecord { UserId = _testUserId, Type = "quick", DurationMinutes = 5, Mood = "normal" };
        
        _context.BreakRecords.Add(breakRecord);
        await _context.SaveChangesAsync();

        var recordInDb = await _context.BreakRecords.FindAsync(breakRecord.Id);
        recordInDb.Should().NotBeNull();
        recordInDb!.Type.Should().Be("quick");
    }

    [Fact]
    public async Task DbContext_DevePermitirListarBreakRecords()
    {
        _context.BreakRecords.AddRange(
            new BreakRecord { UserId = _testUserId, Type = "stretching", DurationMinutes = 10, Mood = "tired" },
            new BreakRecord { UserId = _testUserId, Type = "breathing", DurationMinutes = 3, Mood = "stressed" }
        );
        await _context.SaveChangesAsync();

        var records = await _context.BreakRecords.ToListAsync();
        records.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
