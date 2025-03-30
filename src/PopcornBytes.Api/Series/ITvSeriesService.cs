namespace PopcornBytes.Api.Series;

public interface ITvSeriesService
{
    Task<TvSeries> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);
}
