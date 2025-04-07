using PopcornBytes.Api.Kernel;

namespace PopcornBytes.Api.Users;

public static class UserErrors
{
    public static Error UserDoesNotExist => Error.Validation(
        code: "user.not_exists",
        message: "The user does not exist.");

    public static Error EmptyUsername => Error.Validation(
        code: "user.username.empty",
        message: "Username is required.");

    public static Error InvalidUsernameLength => Error.Validation(
        code: "user.username.invalid_length",
        message: $"Username must be between {User.UsernameMinLength} and {User.UsernameMaxLength} characters long.");

    public static Error BadUsernameFormat => Error.Validation(
        code: "user.username.bad_format",
        message: "Username may only include letters, numbers, dots (.), underscores (_) and hyphens (-).");

    public static Error UsernameAlreadyExists => Error.Validation(
        code: "user.username.already_exists",
        message: "The username already exists.");

    public static Error EmptyEmail => Error.Validation(
        code: "user.email.empty",
        message: "Email is required");

    public static Error BadEmailFormat => Error.Validation(
        code: "user.email.bad_format",
        message: "The provided email is not a valid email address.");

    public static Error EmailAlreadyExists => Error.Validation(
        code: "user.email.already_exists",
        message: "The email already exists.");

    public static Error WeakPassword => Error.Validation(
        code: "user.password.weak",
        message: "The password must be between 8-20 characters long and contain one uppercase letter, one lowercase letter and one number.");

    public static Error WrongPassword => Error.Validation(
        code: "user.password.wrong",
        message: "Wrong password.");
    
    public static Error LongPassword => Error.Validation(
        code: "user.password.too_long",
        message: "The password must not be longer than 20 characters.");
}