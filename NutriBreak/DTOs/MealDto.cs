namespace NutriBreak.DTOs;

public record MealDto(Guid Id, Guid UserId, string Title, int Calories, string TimeOfDay);
public record CreateMealRequest(Guid UserId, string Title, int Calories, string TimeOfDay);
public record UpdateMealRequest(string Title, int Calories, string TimeOfDay);
