using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Series;

internal static class TvSeriesEndpoints
{
    public static void MapSeriesEndpoints(this WebApplication app)
    {
        app.MapGet("/series/search",
            async Task<IResult> (ITmdbClient client, string q, int p = 1, CancellationToken cancellation = default) =>
                Results.Ok(await client.SearchTvSeriesAsync(q, p, cancellation)));

        app.MapGet("/series/{id:int}",
            async Task<IResult> (int id, ITvSeriesService service, CancellationToken cancellation) =>
            {
                var series = await service.GetTvSeriesAsync(id, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            });
    }
}