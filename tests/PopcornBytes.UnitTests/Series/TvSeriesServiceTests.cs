using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Series;

public class TvSeriesServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<ITvSeriesCache> _cacheMock;
    private readonly Mock<ICacheService<string, SearchTvSeriesResponse>> _searchCacheMock;
    private readonly TvSeriesService _sut;

    public TvSeriesServiceTests()
    {
        Mock<ILogger<TvSeriesService>> loggerMock = new();
        _tmdbClientMock = new Mock<ITmdbClient>();
        _cacheMock = new Mock<ITvSeriesCache>();
        _searchCacheMock = new Mock<ICacheService<string, SearchTvSeriesResponse>>();
        _sut = new TvSeriesService(
            logger: loggerMock.Object,
            tmdbClient: _tmdbClientMock.Object,
            cache: _cacheMock.Object,
            searchCache: _searchCacheMock.Object);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        var expected = TvSeriesTestUtils.CreateTvSeries();
        _cacheMock
            .Setup(repo => repo.Get(expected.Id))
            .ReturnsAsync(expected);

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
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetTvSeriesAsync(expected.Id);

        // Assert
        Assert.Equivalent(actual, TvSeries.FromTmdbSeries(expected));
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldCacheSeries_WhenRetrievedFromTmdb()
    {
        // Arrange
        var tmdbSeries = TmdbTestUtils.CreateTmdbTvSeries();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(tmdbSeries.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbSeries);

        // Act
        var series = await _sut.GetTvSeriesAsync(tmdbSeries.Id);

        // Assert
        Assert.NotNull(series);
        _cacheMock.Verify(cache => cache.Set(series), Times.Once);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);

        // Act
        var series = await _sut.GetTvSeriesAsync(0);

        // Assert
        Assert.Null(series);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldNotCacheSeries_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);

        // Act
        _ = await _sut.GetTvSeriesAsync(0);

        // Assert
        _cacheMock.Verify(cache => cache.Set(It.IsAny<TvSeries>()), Times.Never());
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        const string q = "twin peaks";
        const int p = 1;
        var expected = TmdbTestUtils.CreateSearchSeriesResponse();
        _searchCacheMock
            .Setup(cache => cache.TryGetValue($"{q}:{p}", out expected))
            .Returns(true);

        // Act
        var actual = await _sut.SearchTvSeriesAsync(q);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldQueryTmdb_WhenNotCached()
    {
        // Arrange
        const string q = "twin peaks";
        const int p = 1;
        var expected = TmdbTestUtils.CreateSearchSeriesResponse();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.SearchTvSeriesAsync(q, p, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.SearchTvSeriesAsync(q, p);

        // Assert
        Assert.Equivalent(actual, expected);
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldCacheResponse_WhenRetrievedFromTmdb()
    {
        // Arrange
        const string q = "twin peaks";
        const int p = 1;
        var response = TmdbTestUtils.CreateSearchSeriesResponse();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.SearchTvSeriesAsync(q, p, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actual = await _sut.SearchTvSeriesAsync(q, p);

        // Assert
        _searchCacheMock.Verify(x => x.Set($"{q}:{p}", response), Times.Once);
    }

    private void SetupCacheMiss()
    {
        _cacheMock
            .Setup(cache => cache.Get(It.IsAny<int>()))
            .ReturnsAsync((TvSeries?)null);
    }
}