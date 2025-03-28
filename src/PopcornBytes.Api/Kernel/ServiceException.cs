namespace PopcornBytes.Api.Kernel;

public class ServiceException(Error error) : Exception($"{error.Code}: {error.Message}")
{
    public Error Error { get; } = error;
}
