using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Series;

public class TvSeriesServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<ITvSeriesCache> _tvSeriesCacheMock;
    private readonly TvSeriesService _sut;

    public TvSeriesServiceTests()
    {
        Mock<ILogger<TvSeriesService>> loggerMock = new();
        _tmdbClientMock = new Mock<ITmdbClient>();
        _tvSeriesCacheMock = new Mock<ITvSeriesCache>();
        _sut = new TvSeriesService(loggerMock.Object, _tmdbClientMock.Object, _tvSeriesCacheMock.Object);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        var expected = TvSeriesTestUtils.CreateTvSeries();
        _tvSeriesCacheMock
            .Setup(cache => cache.Get(expected.Id))
            .Returns(expected);
        
        // Act
        var actual = await _sut.GetTvSeriesAsync(expected.Id);
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldGetFromTmdb_WhenNotCached()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbTvSeries();
        _tvSeriesCacheMock
            .Setup(cache => cache.Get(expected.Id))
            .Returns((TvSeries?)null);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        
        // Act
        var actual = await _sut.GetTvSeriesAsync(expected.Id);
        
        // Assert
        Assert.Equivalent(actual, expected.ToTvSeries());
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldCacheSeries_WhenRetrievedFromTmdb()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbTvSeries();
        _tvSeriesCacheMock
            .Setup(cache => cache.Get(expected.Id))
            .Returns((TvSeries?)null);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        
        // Act
        var actual = await _sut.GetTvSeriesAsync(expected.Id);
        
        // Assert
        Assert.NotNull(actual);
        _tvSeriesCacheMock.Verify(cache => cache.Set(actual.Id, actual), Times.Once);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        // Arrange
        _tvSeriesCacheMock
            .Setup(cache => cache.Get(It.IsAny<int>()))
            .Returns((TvSeries?)null);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);
        
        // Act
        var actual = await _sut.GetTvSeriesAsync(0);
        
        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldNotCacheSeries_WhenTmdbClientReturnsNull()
    {
        // Arrange
        _tvSeriesCacheMock
            .Setup(cache => cache.Get(It.IsAny<int>()))
            .Returns((TvSeries?)null);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);
        
        // Act
        _ = await _sut.GetTvSeriesAsync(0);
        
        // Assert
        _tvSeriesCacheMock.Verify(cache => cache.Set(It.IsAny<int>(), It.IsAny<TvSeries>()), Times.Never());
    }
}