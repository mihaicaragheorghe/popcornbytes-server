namespace PopcornBytes.Api.Kernel;

public interface IResult
{
    Error? Error { get; }
    bool IsError { get; }
}
