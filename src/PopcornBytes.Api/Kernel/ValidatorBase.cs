namespace PopcornBytes.Api.Kernel;

public class ValidatorBase<T>
{
    private readonly T _value;

    protected ValidatorBase(T value)
    {
        _value = value;
    }

    protected Result Validate(Func<T, bool> rule, Error? error = null)
    {
        if (rule(_value))
        {
            return Result.Success();
        }

        if (error != null) return error;

        string type = typeof(T).Name;
        return Error.Validation(
            code: $"{type.ToLower()}.invalid",
            message: $"{type} is invalid");
    }
}
