using PopcornBytes.Api.Series;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Series;

public class TvSeriesCacheTests
{
    [Fact]
    public void Get_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var cache = new TvSeriesCache(1, 1);
        
        // Act
        var result = cache.Get(1);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Get_ShouldReturnSeries_WhenItemExists()
    {
        // Arrange
        var cache = new TvSeriesCache(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        cache.Set(series.Id, series);
        
        // Act
        var result = cache.Get(1);
        
        // Assert
        Assert.Equal(series, result);
    }

    [Fact]
    public void Set_ShouldCreateItem_WhenDoesNotExist()
    {
        // Arrange
        var cache = new TvSeriesCache(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        
        // Act
        cache.Set(series.Id, series);
        
        // Assert
        var result = cache.Get(1);
        Assert.Equal(series, result);
    }

    [Fact]
    public void Set_ShouldUpdateItem_WhenExists()
    {
        // Arrange
        var cache = new TvSeriesCache(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        
        // Act
        cache.Set(series.Id, series);
        series.Name = "new_name";
        
        // Assert
        var result = cache.Get(series.Id);
        Assert.Equal(series, result);
    }
}