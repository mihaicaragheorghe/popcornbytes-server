using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Seasons;

public class SeasonServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<ITvSeriesCache> _seriesCacheMock;
    private readonly SeasonService _sut;

    public SeasonServiceTests()
    {
        Mock<ILogger<SeasonService>> loggerMock = new();
        _tmdbClientMock = new Mock<ITmdbClient>();
        _seriesCacheMock = new Mock<ITvSeriesCache>();
        _sut = new SeasonService(loggerMock.Object, _seriesCacheMock.Object, _tmdbClientMock.Object);
    }
    
    [Fact]
    public async Task GetSeasonAsync_ShouldReturnFromCache_WhenSeriesIsCached()
    {
        // Arrange
        var series = TvSeriesTestUtils.CreateTvSeries();
        var expected = SeasonTestUtils.CreateSeason(id: 666, tvSeriesId: series.Id, seasonNumber: 777);
        series.Seasons.Add(expected);
        _seriesCacheMock
            .Setup(cache => cache.Get(expected.TvSeriesId))
            .ReturnsAsync(series);
        
        // Act
        var actual = await _sut.GetSeasonAsync(expected.TvSeriesId, expected.Number);
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task GetSeasonAsync_ShouldReturnNull_WhenCachedSeriesDoesNotContainSeason()
    {
        // Arrange
        var series = TvSeriesTestUtils.CreateTvSeries();
        _seriesCacheMock
            .Setup(cache => cache.Get(series.Id))
            .ReturnsAsync(series);
        
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
        _seriesCacheMock
            .Setup(cache => cache.Get(It.IsAny<int>()))
            .ReturnsAsync((TvSeries?)null);
    }
}