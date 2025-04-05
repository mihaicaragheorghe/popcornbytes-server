using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Tmdb;

public interface ITmdbClient
{
    Task AuthenticateAsync(CancellationToken cancellationToken = default);

    Task<TmdbConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

    Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default);

    Task<TmdbTvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);

    Task<TmdbSeason?> GetSeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);

    Task<List<TmdbEpisode>?> GetEpisodesAsync(int seriesId, int seasonNumber,
        CancellationToken cancellationToken = default);

    Task<TmdbEpisode?> GetEpisodeAsync(int seriesId, int seasonNumber, int episodeNumber,
        CancellationToken cancellationToken = default);
}
