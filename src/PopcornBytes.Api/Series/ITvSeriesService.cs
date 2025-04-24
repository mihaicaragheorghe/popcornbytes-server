using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

public interface ITvSeriesService
{
    Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default);

    Task<TvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);
}