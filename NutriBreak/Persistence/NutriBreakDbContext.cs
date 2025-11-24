using Microsoft.EntityFrameworkCore;
using NutriBreak.Domain;

namespace NutriBreak.Persistence;

public class NutriBreakDbContext : DbContext
{
    public NutriBreakDbContext(DbContextOptions<NutriBreakDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<BreakRecord> BreakRecords => Set<BreakRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Meal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(120);
            e.Property(x => x.Calories).IsRequired();
            e.HasOne(x => x.User)
                .WithMany(u => u.Meals)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BreakRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).IsRequired().HasMaxLength(50);
            e.Property(x => x.Mood).HasMaxLength(50);
            e.HasOne(x => x.User)
                .WithMany(u => u.Breaks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
