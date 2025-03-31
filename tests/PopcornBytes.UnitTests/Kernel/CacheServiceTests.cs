using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Series;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Kernel;

public class CacheServiceTests
{
    [Fact]
    public void Get_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var cache = new CacheService<int, TvSeries>(1, 1);
        
        // Act
        bool result = cache.TryGetValue(1, out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Get_ShouldReturnSeries_WhenKeyExists()
    {
        // Arrange
        var cache = new CacheService<int, TvSeries>(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        cache.Set(series.Id, series);
        
        // Act
        var result = cache.TryGetValue(1, out var value);
        
        // Assert
        Assert.True(result);
        Assert.Equal(series, value);
    }

    [Fact]
    public void Set_ShouldCreateItem_WhenKeyDoesNotExist()
    {
        // Arrange
        var cache = new CacheService<int, TvSeries>(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        
        // Act
        cache.Set(series.Id, series);
        
        // Assert
        bool result = cache.TryGetValue(series.Id, out var value);
        Assert.True(result);
        Assert.Equal(series, value);
    }

    [Fact]
    public void Set_ShouldUpdateItem_WhenKeyExists()
    {
        // Arrange
        var cache = new CacheService<int, TvSeries>(1, 1);
        var series = TvSeriesTestUtils.CreateTvSeries();
        
        // Act
        cache.Set(series.Id, series);
        series.Name = "new_name";
        
        // Assert
        bool result = cache.TryGetValue(series.Id, out var value);
        Assert.True(result);
        Assert.Equal(series, value);
    }
}