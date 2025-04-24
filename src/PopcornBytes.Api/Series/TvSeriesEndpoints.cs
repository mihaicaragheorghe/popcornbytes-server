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
    }
}