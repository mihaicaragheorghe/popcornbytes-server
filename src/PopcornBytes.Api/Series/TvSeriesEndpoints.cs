using PopcornBytes.Api.Tmdb;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

internal static class TvSeriesEndpoints
{
    public static void MapSeriesEndpoints(this WebApplication app)
    {
        app.MapGet("/series/search",
            async Task<SearchTvSeriesResponse> (ITmdbClient client, string q, int page = 1,
                CancellationToken cancellation = default) =>
                    await client.SearchTvSeriesAsync(q, page, cancellation));

        app.MapGet("/series/{id:int}",
            async Task<TvSeries> (int id, ITvSeriesService service, CancellationToken cancellation = default) =>
                await service.GetTvSeriesAsync(id, cancellation));
    }
}