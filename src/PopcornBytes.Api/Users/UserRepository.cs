using Dapper;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.Api.Users;

public class UserRepository(IDbConnectionFactory dbConnectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.QuerySingleOrDefaultAsync<User?>(
            "SELECT id, email, username, password_hash, created_at FROM users WHERE id = @id", new { id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.QuerySingleOrDefaultAsync<User?>(
            "SELECT id, email, username, password_hash, created_at FROM users WHERE username = @username",
            new { username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.QuerySingleOrDefaultAsync<User?>(
            "SELECT id, email, username, password_hash, created_at FROM users WHERE email = @email",
            new { email });
    }

    public async Task CreateAsync(User user)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        int rowsAffected = await connection.ExecuteAsync(
            """
            INSERT INTO users (id, email, username, password_hash, created_at)
            VALUES (@Id, @Email, @Username, @PasswordHash, @CreatedAt)
            """,
            user);

        if (rowsAffected == 0)
        {
            throw new Exception($"Could not create user {user.Id}");
        }
    }

    public async Task UpdateAsync(Guid id, string username, string email)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        int rowsAffected = await connection.ExecuteAsync(
            """
            UPDATE users SET email = @email, username = @username
            WHERE id = @id
            """,
            new { id, username, email });

        if (rowsAffected == 0)
        {
            throw new Exception($"Could not update user {id}");
        }
    }

    public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();
        
        int rowsAffected = await connection.ExecuteAsync(
            "UPDATE users SET password_hash = @passwordHash WHERE id = @id",
            new { id, passwordHash });

        if (rowsAffected == 0)
        {
            throw new Exception($"Could not update password for user {id}");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        await connection.ExecuteAsync("DELETE FROM users WHERE id = @id", new { id });
    }
}