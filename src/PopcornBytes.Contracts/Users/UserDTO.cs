namespace PopcornBytes.Contracts.Users;

public record UserDto(Guid Id, string Username, string Email, DateTime CreatedAt);
