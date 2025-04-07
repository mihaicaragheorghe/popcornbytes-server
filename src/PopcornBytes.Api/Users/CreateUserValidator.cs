using PopcornBytes.Api.Kernel;

namespace PopcornBytes.Api.Users;

public class CreateUserValidator : Validator<User>
{
    public CreateUserValidator(User subject) : base(subject: subject)
    {
        AddRule(condition: u => !string.IsNullOrWhiteSpace(value: u.Username),
            error: UserErrors.EmptyUsername);

        AddRule(condition: u => u.Username.Length >= User.UsernameMinLength && u.Username.Length <= User.UsernameMaxLength,
            error: UserErrors.InvalidUsernameLength);

        AddRule(condition: u => UserRegex.Username().IsMatch(input: u.Username),
            error: UserErrors.BadUsernameFormat);

        AddRule(condition: u => UserRegex.Email().IsMatch(input: u.Email),
            error: UserErrors.BadEmailFormat);
    }
}