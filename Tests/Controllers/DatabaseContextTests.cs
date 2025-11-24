using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NutriBreak.Domain;
using NutriBreak.Persistence;

namespace Tests.Controllers;

public class DatabaseContextTests : IDisposable
{
    private readonly NutriBreakDbContext _context;

    public DatabaseContextTests()
    {
        var options = new DbContextOptionsBuilder<NutriBreakDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NutriBreakDbContext(options);
    }

    [Fact]
    public async Task DbContext_DevePermitirAdicionarUsuario()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "João Silva", Email = "joao@test.com" };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userInDb = await _context.Users.FindAsync(user.Id);
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task DbContext_DevePermitirListarUsuarios()
    {
        var user1 = new User { Id = Guid.NewGuid(), Name = "Maria", Email = "maria@test.com" };
        var user2 = new User { Id = Guid.NewGuid(), Name = "Pedro", Email = "pedro@test.com" };
        
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var users = await _context.Users.ToListAsync();
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task DbContext_DevePermitirAtualizarUsuario()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Ana", Email = "ana@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        user.Name = "Ana Silva";
        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Name.Should().Be("Ana Silva");
    }

    [Fact]
    public async Task DbContext_DevePermitirDeletarUsuario()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Carlos", Email = "carlos@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        var deletedUser = await _context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
