using PopcornBytes.Api.Tmdb;
using PopcornBytes.Contracts.TvSeries;

namespace PopcornBytes.Api.TvSeries;

internal static class TvSeriesEndpoints
{
    public static void MapSeriesEndpoints(this WebApplication app)
    {
        app.MapGet("/series/search",
            async Task<SearchTvSeriesResponse> (ITmdbClient client, string q, int page = 1,
                CancellationToken cancellation = default) =>
                    await client.SearchTvSeriesAsync(q, page, cancellation));
    }
}