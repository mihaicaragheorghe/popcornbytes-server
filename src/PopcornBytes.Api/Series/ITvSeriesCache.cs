namespace PopcornBytes.Api.Series;

public interface ITvSeriesCache
{
    Task<TvSeries?> Get(int id);

    Task<List<TvSeries>> Get(IEnumerable<int> ids);

    Task<bool> Set(TvSeries series);
}
