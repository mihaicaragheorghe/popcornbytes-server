namespace PopcornBytes.Api.Kernel;

// todo: use DI and reflection to inject validators to reduce dependencies while testing
public interface IValidator<in T>
{
    Result Validate();
}

public abstract class Validator<T> : IValidator<T>
{
    private readonly T _subject;

    private readonly List<(Func<T, bool> check, Error err)> _rules = [];

    protected Validator(T subject)
    {
        _subject = subject;
    }

    protected void AddRule(Func<T, bool> condition, Error error)
    {
        _rules.Add((condition, error));
    }

    public Result Validate()
    {
        foreach (var rule in _rules)
        {
            if (!rule.check(_subject))
            {
                return Result.Failure(rule.err);
            }
        }

        return Result.Success();
    }
}