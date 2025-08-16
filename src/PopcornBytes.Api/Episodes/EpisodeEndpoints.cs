using System.Security.Claims;
using PopcornBytes.Api.Security;

namespace PopcornBytes.Api.Episodes;

public static class EpisodeEndpoints
{
    public static void MapEpisodeEndpoints(this WebApplication app)
    {
        app.MapGet("/series/{seriesId:int}/seasons/{season:int}/episodes",
            async Task<IResult> (int seriesId, int season, IEpisodeService service, CancellationToken cancellation) =>
            {
                var series = await service.GetEpisodesAsync(seriesId, season, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            })
            .RequireAuthorization();

        app.MapGet("/series/{seriesId:int}/seasons/{season:int}/episodes/{episode:int}",
            async Task<IResult> (int seriesId, int season, int episode, IEpisodeService service,
                CancellationToken cancellation) =>
            {
                var series = await service.GetEpisodeAsync(seriesId, season, episode, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            })
            .RequireAuthorization();

        app.MapPost("users/{userId:guid}/episodes/completed",
            async Task<IResult> (Guid userId, EpisodeCompletedRequest request, ClaimsPrincipal claims,
                IEpisodeService service) =>
            {
                if (IdentityUtils.GetUserIdClaim(claims) != userId)
                {
                    return Results.Forbid();
                }

                await service.AddToCompletedAsync(
                    userId: userId,
                    seriesId: request.SeriesId,
                    season: request.SeasonNumber,
                    episode: request.EpisodeNumber);

                return Results.NoContent();
            })
            .RequireAuthorization();

        app.MapDelete("/users/{userId:guid}/episodes/completed",
            async Task<IResult> (Guid userId, EpisodeCompletedRequest request, ClaimsPrincipal claims,
                IEpisodeService service) =>
            {
                if (IdentityUtils.GetUserIdClaim(claims) != userId)
                {
                    return Results.Forbid();
                }

                await service.RemoveFromCompletedAsync(
                    userId: userId,
                    seriesId: request.SeriesId,
                    season: request.SeasonNumber,
                    episode: request.EpisodeNumber);

                return Results.NoContent();
            })
            .RequireAuthorization();

        app.MapGet("/users/{userId:guid}/episodes/completed",
            async Task<IResult> (Guid userId, ClaimsPrincipal claims, IEpisodeService service) =>
            {
                if (IdentityUtils.GetUserIdClaim(claims) != userId)
                {
                    return Results.Forbid();
                }

                return Results.Ok(await service.GetCompletedAsync(userId));
            })
            .RequireAuthorization();
    }
}