using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Seasons;

public class SeasonServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<ICacheService<int, TvSeries>> _cacheMock;
    private readonly SeasonService _sut;

    public SeasonServiceTests()
    {
        Mock<ILogger<SeasonService>> loggerMock = new();
        _tmdbClientMock = new Mock<ITmdbClient>();
        _cacheMock = new Mock<ICacheService<int, TvSeries>>();
        _sut = new SeasonService(loggerMock.Object, _cacheMock.Object, _tmdbClientMock.Object);
    }
    
    [Fact]
    public async Task GetSeasonAsync_ShouldReturnFromCache_WhenSeriesIsCached()
    {
        // Arrange
        var series = TvSeriesTestUtils.CreateTvSeries();
        var expected = SeasonTestUtils.CreateSeason(id: 666, tvSeriesId: series.Id, seasonNumber: 777);
        series.Seasons.Add(expected);
        _cacheMock
            .Setup(cache => cache.TryGetValue(expected.TvSeriesId, out series))
            .Returns(true);
        
        // Act
        var actual = await _sut.GetSeasonAsync(expected.TvSeriesId, expected.SeasonNumber);
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task GetSeasonAsync_ShouldReturnNull_WhenCachedSeriesDoesNotContainSeason()
    {
        // Arrange
        var series = TvSeriesTestUtils.CreateTvSeries();
        _cacheMock
            .Setup(cache => cache.TryGetValue(series.Id, out series))
            .Returns(true);
        
        // Act
        var season = await _sut.GetSeasonAsync(series.Id, -1);
        
        // Assert
        Assert.Null(season);
    }

    [Fact]
    public async Task GetSeasonAsync_ShouldGetSeasonFromTmdb_WhenNotCached()
    {
        const int seriesId = 888;
        var expected = TmdbTestUtils.CreateTmdbSeason();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetSeasonAsync(seriesId, expected.SeasonNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        
        // Act
        var actual = await _sut.GetSeasonAsync(seriesId, expected.SeasonNumber);
        
        // Assert
        Assert.Equivalent(actual, Season.FromTmdbSeason(expected, seriesId));
    }

    [Fact]
    public async Task GetSeasonAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetSeasonAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbSeason?)null);
        
        // Act
        var season = await _sut.GetSeasonAsync(1, 1);
        
        // Assert
        Assert.Null(season);
    }
    
    private void SetupCacheMiss()
    {
        TvSeries? cachedValue = null;
        _cacheMock
            .Setup(cache => cache.TryGetValue(It.IsAny<int>(), out cachedValue))
            .Returns(false);
    }
}
