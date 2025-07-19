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
    private readonly Mock<ITvSeriesRepository> _seriesRepositoryMock = new();
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
            searchCache: _searchCacheMock.Object,
            seriesRepository: _seriesRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        var expected = TvSeriesTestUtils.CreateTvSeries();
        _cacheMock
            .Setup(repo => repo.Get(expected.Id))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetByIdAsync(expected.Id);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldGetFromTmdb_WhenNotCached()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbTvSeries();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetByIdAsync(expected.Id);

        // Assert
        Assert.Equivalent(actual, TvSeries.FromTmdbSeries(expected));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCacheSeries_WhenRetrievedFromTmdb()
    {
        // Arrange
        var tmdbSeries = TmdbTestUtils.CreateTmdbTvSeries();
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(tmdbSeries.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbSeries);

        // Act
        var series = await _sut.GetByIdAsync(tmdbSeries.Id);

        // Assert
        Assert.NotNull(series);
        _cacheMock.Verify(cache => cache.Set(series), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);

        // Act
        var series = await _sut.GetByIdAsync(0);

        // Assert
        Assert.Null(series);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotCacheSeries_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbTvSeries?)null);

        // Act
        _ = await _sut.GetByIdAsync(0);

        // Assert
        _cacheMock.Verify(cache => cache.Set(It.IsAny<TvSeries>()), Times.Never());
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        const string q = "twin peaks";
        const int p = 1;
        var expected = TmdbTestUtils.CreateSearchSeriesResponse();
        _searchCacheMock
            .Setup(cache => cache.TryGetValue($"{q}:{p}", out expected))
            .Returns(true);

        // Act
        var actual = await _sut.QueryAsync(q);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task QueryAsync_ShouldQueryTmdb_WhenNotCached()
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
        var actual = await _sut.QueryAsync(q);

        // Assert
        Assert.Equivalent(actual, expected);
    }

    [Fact]
    public async Task QueryAsync_ShouldCacheResponse_WhenRetrievedFromTmdb()
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
        await _sut.QueryAsync(q);

        // Assert
        _searchCacheMock.Verify(x => x.Set($"{q}:{p}", response), Times.Once);
    }

    [Fact]
    public async Task TrackAsync_ShouldCallRepositoryAddToWatchlist_WhenStateIsWatchlist()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        var addedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watchlist);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.AddToWatchlistAsync(userId, seriesId, addedAtUnix), Times.Once);
    }

    [Fact]
    public async Task TrackAsync_ShouldCallRepositoryAddToWatching_WhenStateIsWatching()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        var addedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.AddToWatchingAsync(userId, seriesId, addedAtUnix), Times.Once);
    }

    [Fact]
    public async Task TrackAsync_ShouldCallRepositoryAddToCompleted_WhenStateIsCompleted()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        var addedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Completed);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.AddToCompletedAsync(userId, seriesId, addedAtUnix), Times.Once);
    }

    [Fact]
    public async Task TrackAsync_ShouldThrowArgumentOutOfRangeException_WhenStateIsInvalid()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();

        // Act
        var exception =
            await Record.ExceptionAsync(() => _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Stopped));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public async Task TrackAsync_ShouldRemoveFromWatchlist_WhenStartedWatching()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToWatchingAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(1);

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Once);
    }
    
    [Fact]
    public async Task TrackAsync_ShouldNotRemoveFromWatchlist_WhenAddToWatchingFailed()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToWatchingAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(0);

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Never);
    }
    
    [Fact]
    public async Task TrackAsync_ShouldRemoveFromWatchlist_WhenStateIsCompleted()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToCompletedAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(1);

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Completed);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Once);
    }
    
    [Fact]
    public async Task TrackAsync_ShouldNotRemoveFromWatchlist_WhenAddToCompletedFailed()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToCompletedAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(0);

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Never);
    }
    
    [Fact]
    public async Task TrackAsync_ShouldRemoveFromWatching_WhenStateIsCompleted()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToCompletedAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(1);
        
        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Completed);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchingAsync(userId, seriesId), Times.Once);
    }
    
    [Fact]
    public async Task TrackAsync_ShouldNotRemoveFromWatching_WhenAddToCompletedFailed()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();
        _seriesRepositoryMock
            .Setup(x => x.AddToCompletedAsync(userId, seriesId, It.IsAny<long>()))
            .ReturnsAsync(0);

        // Act
        await _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Never);
    }

    [Fact]
    public async Task RemoveTrackedAsync_ShouldCallRepositoryRemovedFromWatchlist_WhenStateIsWatchlist()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();

        // Act
        await _sut.RemovedTrackedAsync(userId, seriesId, TrackedSeriesState.Watchlist);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchlistAsync(userId, seriesId), Times.Once);
    }

    [Fact]
    public async Task RemoveTrackedAsync_ShouldCallRepositoryRemovedFromWatching_WhenStateIsWatching()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();

        // Act
        await _sut.RemovedTrackedAsync(userId, seriesId, TrackedSeriesState.Watching);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromWatchingAsync(userId, seriesId), Times.Once);
    }

    [Fact]
    public async Task RemoveTrackedAsync_ShouldCallRepositoryRemovedFromCompleted_WhenStateIsCompleted()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();

        // Act
        await _sut.RemovedTrackedAsync(userId, seriesId, TrackedSeriesState.Completed);

        // Assert
        _seriesRepositoryMock.Verify(repo => repo.RemoveFromCompletedAsync(userId, seriesId), Times.Once);
    }

    [Fact]
    public async Task RemoveTrackedAsync_ShouldThrowArgumentOutOfRangeException_WhenStateIsInvalid()
    {
        // Arrange
        const int seriesId = 123;
        var userId = Guid.NewGuid();

        // Act
        var exception =
            await Record.ExceptionAsync(() => _sut.TrackAsync(userId, seriesId, TrackedSeriesState.Stopped));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public async Task GetTrackedAsync_ShouldRetrieveIdsFromWatchlist_WhenStateIsWatchlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<int> { 1, 2, 3 };
        var expected = TvSeriesTestUtils.CreateCollection(count: 3, startId: expectedIds[0]);

        _seriesRepositoryMock
            .Setup(repo => repo.GetWatchlistAsync(userId))
            .ReturnsAsync(expectedIds);
        _cacheMock
            .Setup(cache => cache.Get(expectedIds))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetTrackedAsync(userId, TrackedSeriesState.Watchlist, CancellationToken.None);

        // Assert
        Assert.NotEmpty(actual);
        Assert.Equal(expected, actual);
        _seriesRepositoryMock.Verify(repo => repo.GetWatchlistAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetTrackedAsync_ShouldRetrieveIdsFromWatching_WhenStateIsWatching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<int> { 1, 2, 3 };
        var expected = TvSeriesTestUtils.CreateCollection(count: 3, startId: expectedIds[0]);

        _seriesRepositoryMock
            .Setup(repo => repo.GetWatchingAsync(userId))
            .ReturnsAsync(expectedIds);
        _cacheMock
            .Setup(cache => cache.Get(expectedIds))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetTrackedAsync(userId, TrackedSeriesState.Watching, CancellationToken.None);

        // Assert
        Assert.NotEmpty(actual);
        Assert.Equal(expected, actual);
        _seriesRepositoryMock.Verify(repo => repo.GetWatchingAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetTrackedAsync_ShouldRetrieveIdsFromCompleted_WhenStateIsCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<int> { 1, 2, 3 };
        var expected = TvSeriesTestUtils.CreateCollection(count: 3, startId: expectedIds[0]);

        _seriesRepositoryMock
            .Setup(repo => repo.GetCompletedAsync(userId))
            .ReturnsAsync(expectedIds);
        _cacheMock
            .Setup(cache => cache.Get(expectedIds))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetTrackedAsync(userId, TrackedSeriesState.Completed, CancellationToken.None);

        // Assert
        Assert.NotEmpty(actual);
        Assert.Equal(expected, actual);
        _seriesRepositoryMock.Verify(repo => repo.GetCompletedAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetTrackedAsync_ShouldRetrieveIdsFromStopped_WhenStateIsStopped()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<int> { 1, 2, 3 };
        var expected = TvSeriesTestUtils.CreateCollection(count: 3, startId: expectedIds[0]);

        _seriesRepositoryMock
            .Setup(repo => repo.GetStoppedAsync(userId))
            .ReturnsAsync(expectedIds);
        _cacheMock
            .Setup(cache => cache.Get(expectedIds))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetTrackedAsync(userId, TrackedSeriesState.Stopped, CancellationToken.None);

        // Assert
        Assert.NotEmpty(actual);
        Assert.Equal(expected, actual);
        _seriesRepositoryMock.Verify(repo => repo.GetStoppedAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetTrackedAsync_ShouldFetchFromTmdb_WhenNotCached()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };
        var cached = TvSeriesTestUtils.CreateCollection(count: 3, startId: expectedIds[0]);
        var fetched = TmdbTestUtils.CreateTmdbTvSeriesCollection(count: 2, startId: expectedIds[3]);
        var expected = new List<TvSeries>(cached)
        {
            TvSeries.FromTmdbSeries(fetched[0]), TvSeries.FromTmdbSeries(fetched[1])
        };

        _seriesRepositoryMock
            .Setup(repo => repo.GetWatchlistAsync(userId))
            .ReturnsAsync(expectedIds);
        _cacheMock
            .Setup(cache => cache.Get(expectedIds))
            .ReturnsAsync(cached);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expectedIds[3], It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched[0]);
        _tmdbClientMock
            .Setup(client => client.GetTvSeriesAsync(expectedIds[4], It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched[1]);

        // Act
        var actual = await _sut.GetTrackedAsync(userId, TrackedSeriesState.Watchlist, CancellationToken.None);

        // Assert
        Assert.NotEmpty(actual);
        Assert.Equivalent(expected, actual);
    }

    private void SetupCacheMiss()
    {
        _cacheMock
            .Setup(cache => cache.Get(It.IsAny<int>()))
            .ReturnsAsync((TvSeries?)null);
    }
}