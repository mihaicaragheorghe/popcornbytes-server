namespace PopcornBytes.Api.Kernel;

public sealed class Result<T> : IResult
{
    public T? Value { get; }
    
    public Error? Error { get; }

    public bool IsError { get; }

    private Result(T? value)
    {
        Value = value;
        IsError = false;
    }

    private Result(Error error)
    {
        Error = error;
        IsError = true;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);

    public TNext Match<TNext>(
        Func<T, TNext> onSuccess,
        Func<Error, TNext> onError) =>
        IsError ? onError((Error)Error!) : onSuccess(Value!);
}
