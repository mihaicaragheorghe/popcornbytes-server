namespace PopcornBytes.Api.Users;

public class User
{
    public Guid Id { get; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; init; }

    public const int UsernameMaxLength = 16;
    public const int UsernameMinLength = 3;

    public User(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private User() { }
}
