namespace NutriBreak.DTOs;

public record UserDto(Guid Id, string Name, string Email, string WorkMode);
public record CreateUserRequest(string Name, string Email, string WorkMode);
public record UpdateUserRequest(string Name, string WorkMode);
