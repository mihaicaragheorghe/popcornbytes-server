using PopcornBytes.Contracts.TvSeries;

namespace PopcornBytes.Api.Tmdb;

public interface ITmdbClient
{
    Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1, CancellationToken cancellationToken = default);
}