using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ITmdbClient _tmdbClient;

    public TvSeriesService(ITmdbClient tmdbClient)
    {
        _tmdbClient = tmdbClient;
    }
    
    public async Task<TvSeries> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
    {
        var tmdbResponse = await _tmdbClient.GetTvSeriesAsync(id, cancellationToken);
        return tmdbResponse.ToTvSeries();
    }
}