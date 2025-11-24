namespace NutriBreak.Domain;

public class BreakRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public int DurationMinutes { get; set; }
    public string Type { get; set; } = string.Empty; // pausa curta, alongamento, alimentacao
    public string Mood { get; set; } = string.Empty; // humor reportado
    public int EnergyLevel { get; set; } // 1-10
    public int ScreenTimeMinutes { get; set; }
}
