using System.Security.Claims;

using PopcornBytes.Api.Security;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

internal static class TvSeriesEndpoints
{
    public static void MapSeriesEndpoints(this WebApplication app)
    {
        app.MapGet("/series/search",
                async Task<IResult> (ITvSeriesService service, string q, int p = 1,
                        CancellationToken cancellation = default) =>
                    Results.Ok(await service.QueryAsync(q, p, cancellation)))
            .Produces<SearchTvSeriesResponse>()
            .RequireAuthorization();

        app.MapGet("/series/{id:int}",
                async Task<IResult> (int id, ITvSeriesService service, CancellationToken cancellation) =>
                {
                    var series = await service.GetByIdAsync(id, cancellation);
                    return series is null ? Results.NotFound() : Results.Ok(series);
                })
            .RequireAuthorization();

        app.MapPost("/series/track/{state}",
                async Task<IResult> (string state, ClaimsPrincipal claims, TrackSeriesRequest request,
                    ITvSeriesService service) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != request.UserId)
                    {
                        return Results.Unauthorized();
                    }

                    if (!Enum.TryParse<TrackedSeriesState>(state, ignoreCase: true, out var stateEnum) ||
                        stateEnum == TrackedSeriesState.Stopped)
                    {
                        return InvalidTrackStateResult;
                    }

                    await service.TrackAsync(request.UserId, request.SeriesId, stateEnum);
                    return Results.Ok();
                })
            .RequireAuthorization();

        app.MapGet("/users/{userId:guid}/series/tracked/{state}",
                async Task<IResult> (Guid userId, string state, ClaimsPrincipal claims, ITvSeriesService service,
                    CancellationToken cancellation) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != userId)
                    {
                        return Results.Unauthorized();
                    }

                    if (!Enum.TryParse<TrackedSeriesState>(state, ignoreCase: true, out var stateEnum))
                    {
                        return Results.BadRequest(new
                        {
                            Error = "Invalid state.", AllowedStates = Enum.GetNames<TrackedSeriesState>()
                        });
                    }

                    var series = await service.GetTrackedAsync(userId, stateEnum, cancellation);
                    return Results.Ok(series);
                })
            .RequireAuthorization();

        app.MapDelete("/users/{userId:guid}/series/{seriesId:int}/tracked/{state}",
                async Task<IResult> (Guid userId, int seriesId, string state, ClaimsPrincipal claims,
                    ITvSeriesService service) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != userId)
                    {
                        return Results.Unauthorized();
                    }

                    if (!Enum.TryParse<TrackedSeriesState>(state, ignoreCase: true, out var stateEnum) ||
                        stateEnum == TrackedSeriesState.Stopped)
                    {
                        return InvalidTrackStateResult;
                    }

                    await service.RemovedTrackedAsync(userId, seriesId, stateEnum);
                    return Results.Ok();
                })
            .RequireAuthorization();

        app.MapPost("/users/{userId:guid}/series/{seriesId:int}/tracked/watching/{action}",
                async Task<IResult> (Guid userId, int seriesId, string action, ClaimsPrincipal claims,
                    ITvSeriesService service) =>
                {
                    if (IdentityUtils.GetUserIdClaim(claims) != userId)
                    {
                        return Results.Unauthorized();
                    }

                    var result = action switch
                    {
                        "stop" => await service.StopWatching(userId: userId, seriesId: seriesId),
                        "resume" => await service.ResumeWatching(userId: userId, seriesId: seriesId),
                        _ => throw new ArgumentOutOfRangeException(nameof(action))
                    };

                    return result.Match(
                        () => Results.Ok(),
                        err => Results.Problem(statusCode: err.StatusCode, title: err.Message, detail: err.Code));
                })
            .RequireAuthorization();
    }

    private static IResult InvalidTrackStateResult => Results.BadRequest(new
    {
        Error = "Invalid state.",
        AllowedStates = Enum.GetNames<TrackedSeriesState>()
            .Where(n => n != nameof(TrackedSeriesState.Stopped))
    });
}