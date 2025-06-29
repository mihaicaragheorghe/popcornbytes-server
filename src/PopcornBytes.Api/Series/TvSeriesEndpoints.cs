using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

internal static class TvSeriesEndpoints
{
    public static void MapSeriesEndpoints(this WebApplication app)
    {
        app.MapGet("/series/search",
            async Task<IResult> (ITvSeriesService service, string q, int p = 1, CancellationToken cancellation = default) =>
                Results.Ok(await service.SearchTvSeriesAsync(q, p, cancellation)))
            .Produces<SearchTvSeriesResponse>()
            .RequireAuthorization();

        app.MapGet("/series/{id:int}",
                async Task<IResult> (int id, ITvSeriesService service, CancellationToken cancellation) =>
                {
                    var series = await service.GetTvSeriesAsync(id, cancellation);
                    return series is null ? Results.NotFound() : Results.Ok(series);
                })
            .RequireAuthorization();

        app.MapPost("/users/{userId:guid}/series/{seriesId:int}/watchlist",
                async Task<IResult> (ITvSeriesService service, Guid userId, int seriesId) =>
                {
                    await service.AddToWatchlist(userId, seriesId);
                    return Results.Ok();
                })
            .RequireAuthorization();

        app.MapDelete("/users/{userId:guid}/series/{seriesId:int}/watchlist",
                async Task<IResult> (ITvSeriesService service, Guid userId, int seriesId) =>
                {
                    await service.RemoveFromWatchlist(userId, seriesId);
                    return Results.Ok();
                })
            .RequireAuthorization();

        app.MapGet("/users/{userId:guid}/series/watchlist",
                async Task<IResult> (ITvSeriesService service, Guid userId) =>
                {
                    var watchlist = await service.GetWatchlistAsync(userId);
                    return Results.Ok(watchlist);
                })
                .RequireAuthorization();
    }
}