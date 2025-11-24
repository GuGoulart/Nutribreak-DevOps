namespace NutriBreak.DTOs;

public record BreakRecordDto(Guid Id, Guid UserId, DateTime StartedAt, int DurationMinutes, string Type, string Mood, int EnergyLevel, int ScreenTimeMinutes);
public record CreateBreakRecordRequest(Guid UserId, int DurationMinutes, string Type, string Mood, int EnergyLevel, int ScreenTimeMinutes);
public record UpdateBreakRecordRequest(int DurationMinutes, string Type, string Mood, int EnergyLevel, int ScreenTimeMinutes);
