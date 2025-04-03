using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Tmdb;

public interface ITmdbClient
{
    Task AuthenticateAsync(CancellationToken cancellationToken = default);
    
    Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1, CancellationToken cancellationToken = default);
    
    Task<TmdbTvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);
}
