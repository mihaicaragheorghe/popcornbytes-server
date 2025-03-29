namespace PopcornBytes.Api.Users;

public class User
{
    public Guid Id { get; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public User(Guid id)
    {
        Id = id;
    }

    private User() { }
}
