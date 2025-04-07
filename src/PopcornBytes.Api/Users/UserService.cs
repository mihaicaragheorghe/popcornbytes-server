using Microsoft.AspNetCore.Identity;

using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Security;
using PopcornBytes.Contracts.Users;

namespace PopcornBytes.Api.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _userRepository.GetByIdAsync(id);
    }

    public async Task<Result<Guid>> CreateAsync(string username, string email, string password)
    {
        if (!IsStrongPassword(password))
        {
            return UserErrors.WeakPassword;
        }

        if (await _userRepository.GetByEmailAsync(email) != null)
        {
            return UserErrors.EmailAlreadyExists;
        }

        if (await _userRepository.GetByUsernameAsync(username) != null)
        {
            return UserErrors.UsernameAlreadyExists;
        }

        var user = new User(Guid.NewGuid()) { Username = username, Email = email };

        var validationResult = new CreateUserValidator(user).Validate();
        if (validationResult.Error != null)
        {
            return validationResult.Error;
        }

        var passwordHash = _passwordHasher.HashPassword(user, password);
        user.PasswordHash = passwordHash;

        await _userRepository.CreateAsync(user);

        return user.Id;
    }

    public async Task<Result> UpdateAsync(Guid id, string username, string email)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return UserErrors.UserDoesNotExist;
        }

        user.Username = username;
        user.Email = email;

        var validationResult = new CreateUserValidator(user).Validate();
        if (validationResult.Error != null)
        {
            return validationResult.Error;
        }

        await _userRepository.UpdateAsync(id: user.Id, username: user.Username, email: user.Email);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid id, string oldPassword, string newPassword)
    {
        if (!IsStrongPassword(newPassword))
        {
            return UserErrors.WeakPassword;
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return UserErrors.UserDoesNotExist;
        }

        if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword) ==
            PasswordVerificationResult.Failed)
        {
            return UserErrors.WrongPassword;
        }

        var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _userRepository.UpdatePasswordHashAsync(user.Id, newPasswordHash);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        if (await _userRepository.GetByIdAsync(id) == null)
        {
            return UserErrors.UserDoesNotExist;
        }

        await _userRepository.DeleteAsync(id);
        return Result.Success();
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return UserErrors.UserDoesNotExist;
        }

        if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) !=
            PasswordVerificationResult.Success)
        {
            return UserErrors.WrongPassword;
        }
        
        string token = _jwtTokenGenerator.GenerateToken(
            id: user.Id,
            username: user.Username,
            email: user.Email);
        
        return new LoginResponse(user.Id, token);
    }
    
    public static bool IsStrongPassword(string password)
    {
        return UserRegex.Password().IsMatch(password);
    }
}