using PopcornBytes.Api.Kernel;

namespace PopcornBytes.Api.Users;

public class CreateUserValidator : ValidatorBase<User>
{
    public CreateUserValidator(User value) : base(value)
    {
    }

    public Result Validate()
    {
        Validate(rule: u => !string.IsNullOrWhiteSpace(u.Username),
            error: UserErrors.EmptyUsername);

        Validate(rule: u => u.Username.Length >= User.UsernameMinLength && u.Username.Length <= User.UsernameMaxLength,
            error: UserErrors.InvalidUsernameLength);

        Validate(rule: u => UserRegex.Username().IsMatch(u.Username),
            error: UserErrors.InvalidUsernameFormat);
        
        Validate(rule: u => !string.IsNullOrWhiteSpace(u.Email),
            error: UserErrors.EmptyEmail);
        
        Validate(rule: u => UserRegex.Email().IsMatch(u.Email),
            error: UserErrors.InvalidEmailFormat);
        
        return Result.Success();
    }
}