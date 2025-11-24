namespace NutriBreak.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WorkMode { get; set; } = string.Empty; // remoto, hibrido, presencial
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    public ICollection<BreakRecord> Breaks { get; set; } = new List<BreakRecord>();
}
