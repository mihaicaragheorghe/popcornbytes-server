using PopcornBytes.Api.Kernel;

namespace PopcornBytes.Api.Users;

public class CreateUserValidator : Validator<User>
{
    public CreateUserValidator(User subject) : base(subject)
    {
        AddRule(rule: u => !string.IsNullOrWhiteSpace(u.Username),
            error: UserErrors.EmptyUsername);

        AddRule(rule: u => u.Username.Length >= User.UsernameMinLength && u.Username.Length <= User.UsernameMaxLength,
            error: UserErrors.InvalidUsernameLength);

        AddRule(rule: u => UserRegex.Username().IsMatch(u.Username),
            error: UserErrors.BadUsernameFormat);
        
        AddRule(rule: u => UserRegex.Email().IsMatch(u.Email),
            error: UserErrors.BadEmailFormat);
    }
}