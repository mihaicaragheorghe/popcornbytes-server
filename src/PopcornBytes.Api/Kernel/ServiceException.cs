namespace PopcornBytes.Api.Kernel;

public class ServiceException(Error error) : Exception(error.ToString())
{
    public Error Error { get; } = error;
}
