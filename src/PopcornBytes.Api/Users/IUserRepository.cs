namespace PopcornBytes.Api.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    
    Task<User?> GetByUsernameAsync(string username);
    
    Task<User?> GetByEmailAsync(string email);
    
    Task CreateAsync(User user);
    
    Task UpdateAsync(Guid id, string username, string email);
    
    Task UpdatePasswordHashAsync(Guid id, string passwordHash);
    
    Task DeleteAsync(Guid id);
}