namespace PopcornBytes.Api.Series;

public interface ITvSeriesCache
{
    TvSeries? Get(int id);
    
    void Set(int id, TvSeries series);
}
