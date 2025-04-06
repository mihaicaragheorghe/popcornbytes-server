using PopcornBytes.Api.Security;
using PopcornBytes.Contracts.Users;

namespace PopcornBytes.Api.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/login", IResult (LoginRequest request, IJwtTokenGenerator tokenGenerator) =>
        {
            var id = Guid.NewGuid();
            var token = tokenGenerator.GenerateToken(id, request.Username, request.Password);
            return Results.Ok(new LoginResponse(id, token));
        })
        .Produces<LoginResponse>();
    }
}