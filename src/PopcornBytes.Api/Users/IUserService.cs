using PopcornBytes.Api.Kernel;
using PopcornBytes.Contracts.Users;

namespace PopcornBytes.Api.Users;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
    
    Task<Result<Guid>> CreateAsync(string username, string email, string password);

    Task<Result> UpdateAsync(Guid id, string username, string email);
    
    Task<Result> ChangePasswordAsync(Guid id, string oldPassword, string newPassword);
    
    Task<Result> DeleteAsync(Guid id);
    
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}
