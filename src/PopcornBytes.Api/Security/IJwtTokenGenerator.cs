namespace PopcornBytes.Api.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid id, string username, string email);
}
