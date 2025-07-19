using System.Security.Claims;

using PopcornBytes.Api.Security;
using PopcornBytes.Contracts.Users;

namespace PopcornBytes.Api.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/login", async Task<IResult> (LoginRequest request, IUserService userService) =>
            {
                var result = await userService.LoginAsync(request);
                return result.Match(
                    Results.Ok,
                    err => Results.Problem(statusCode: err.StatusCode, title: err.Message, detail: err.Code));
            })
            .Produces<LoginResponse>();

        app.MapPost("/users", async Task<IResult> (CreateUserRequest request, IUserService userService) =>
            {
                var result = await userService.CreateAsync(
                    username: request.Username,
                    email: request.Email,
                    password: request.Password);

                return result.Match(
                    Results.Ok,
                    err => Results.Problem(statusCode: err.StatusCode, title: err.Message, detail: err.Code));
            })
            .Produces<LoginResponse>();

        app.MapPut("/users/{id:guid}",
                async Task<IResult> (
                    Guid id,
                    UpdateUserRequest request,
                    ClaimsPrincipal claims,
                    IUserService userService) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != id)
                    {
                        return Results.Forbid();
                    }

                    var result = await userService.UpdateAsync(
                        id: id,
                        username: request.Username,
                        email: request.Email);

                    return result.IsError
                        ? Results.Problem(
                            statusCode: result.Error?.StatusCode,
                            title: result.Error?.Message,
                            detail: result.Error?.Code)
                        : Results.NoContent();
                })
            .RequireAuthorization();

        app.MapPut("/users/{id:guid}/password",
                async Task<IResult> (
                    Guid id,
                    ChangeUserPasswordRequest request,
                    ClaimsPrincipal claims,
                    IUserService userService) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != id)
                    {
                        return Results.Forbid();
                    }

                    var result = await userService.ChangePasswordAsync(
                        id: id,
                        oldPassword: request.OldPassword,
                        newPassword: request.NewPassword);

                    return result.IsError
                        ? Results.Problem(
                            statusCode: result.Error?.StatusCode,
                            title: result.Error?.Message,
                            detail: result.Error?.Code)
                        : Results.NoContent();
                })
            .RequireAuthorization();

        app.MapDelete("/users/{id:guid}",
                async Task<IResult> (Guid id, IUserService userService, ClaimsPrincipal claims) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != id)
                    {
                        return Results.Forbid();
                    }

                    var result = await userService.DeleteAsync(id);
                    return result.IsError
                        ? Results.Problem(
                            statusCode: result.Error?.StatusCode,
                            title: result.Error?.Message,
                            detail: result.Error?.Code)
                        : Results.NoContent();
                })
            .RequireAuthorization();

        app.MapGet("/users/{id:guid}", async (Guid id, IUserService userService) =>
            {
                var user = await userService.GetByIdAsync(id);
                return user is null ? Results.NotFound() : Results.Ok(MapToDto(user));
            })
            .RequireAuthorization()
            .Produces<UserDto>();
    }

    private static UserDto MapToDto(User user) =>
        new(Id: user.Id, Username: user.Username, Email: user.Email, CreatedAt: user.CreatedAt);
}