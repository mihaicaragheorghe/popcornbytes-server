namespace PopcornBytes.Api.Kernel;

public readonly record struct Error
{
    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public int StatusCode => Type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static Error Validation(
        string code = "Validation",
        string message = "A validation error occurred.") =>
        new(code, message, ErrorType.Validation);

    public static Error Unauthorized(
        string code = "unauthorized",
        string message = "You are not authorized to perform this action.") =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(
        string code = "forbidden",
        string message = "You are forbidden from performing this action.") =>
        new(code, message, ErrorType.Forbidden);

    public static Error NotFound(
        string code = "not_found",
        string message = "The requested resource was not found.") =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(
        string code = "conflict",
        string message = "A conflict occurred while processing the request.") =>
        new(code, message, ErrorType.Conflict);

    public static Error Failure(
        string code = "failure",
        string message = "An error occurred while processing the request.") =>
        new(code, message, ErrorType.Failure);

    public static Error Internal(
        string code = "server_error",
        string message = "An internal error occurred while processing the request.") =>
        new(code, message, ErrorType.Internal);

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }
}

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Failure,
    Internal
}
