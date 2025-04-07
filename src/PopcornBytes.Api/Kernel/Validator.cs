namespace PopcornBytes.Api.Kernel;

public class Validator<T>
{
    private readonly T _subject;

    private readonly List<(Func<T, bool> check, Error err)> _rules = [];

    protected Validator(T subject)
    {
        _subject = subject;
    }

    protected void AddRule(Func<T, bool> rule, Error error)
    {
        _rules.Add((rule, error));
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