using Microsoft.Extensions.Logging;

using Moq;

using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Episodes;

public class EpisodeServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<IEpisodesCache> _cacheMock;
    private readonly EpisodeService _sut;

    public EpisodeServiceTests()
    {
        Mock<ILogger<EpisodeService>> loggerMock = new();
        _tmdbClientMock = new Mock<ITmdbClient>();
        _cacheMock = new Mock<IEpisodesCache>();
        _sut = new EpisodeService(loggerMock.Object, _tmdbClientMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        const int series = 777;
        const int season = 3;
        var expected = EpisodeTestUtils.CreateEpisodesCollection(count: 13, seriesId: series, seasonNumber: season);
        _cacheMock
            .Setup(cache => cache.Get(series, season))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetEpisodesAsync(series, season);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(x => x.GetEpisodesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<TmdbEpisode>?)null);

        // Act
        var episodes = await _sut.GetEpisodesAsync(1, 1);

        // Assert
        Assert.Null(episodes);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldGetEpisodesFromTmdb_WhenNotCached()
    {
        // Arrange
        SetupCacheMiss();
        const int series = 777;
        const int season = 3;
        var expected = TmdbTestUtils.CreateTmdbEpisodeCollection(count: 13, seriesId: series, season: season);
        _tmdbClientMock
            .Setup(x => x.GetEpisodesAsync(series, season, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetEpisodesAsync(series, season);

        // Assert
        Assert.Equivalent(expected.Select(Episode.FromTmdbEpisode).ToList(), actual);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldCacheEpisodes_WhenRetrievedFromTmdb()
    {
        // Arrange
        SetupCacheMiss();
        const int series = 777;
        const int season = 3;
        var tmdbEpisodes = TmdbTestUtils.CreateTmdbEpisodeCollection(count: 13, seriesId: series, season: season);
        _tmdbClientMock
            .Setup(x => x.GetEpisodesAsync(series, season, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tmdbEpisodes);

        // Act
        var episodes = await _sut.GetEpisodesAsync(series, season);

        // Assert
        Assert.NotNull(episodes);
        _cacheMock.Verify(cache => cache.Set(episodes), Times.Once);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldNotCacheSeries_WhenTmdbClientReturnsNull()
    {
        // Arrange
        const int series = 777;
        const int season = 3;
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(x => x.GetEpisodesAsync(series, season, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<TmdbEpisode>?)null);

        // Act
        _ = await _sut.GetEpisodesAsync(series, season);

        // Assert
        _cacheMock.Verify(cache => cache.Set(It.IsAny<List<Episode>>()), Times.Never);
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        const int series = 777;
        const int season = 3;
        var episodes = EpisodeTestUtils.CreateEpisodesCollection(count: 13, seriesId: series, seasonNumber: season);
        var expected = episodes[0];
        _cacheMock
            .Setup(cache => cache.Get(series, season))
            .ReturnsAsync(episodes);

        // Act
        var actual = await _sut.GetEpisodeAsync(series, season, expected.EpisodeNumber);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldReturnNull_WhenTmdbClientReturnsNull()
    {
        // Arrange
        SetupCacheMiss();
        _tmdbClientMock
            .Setup(x => x.GetEpisodeAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TmdbEpisode?)null);

        // Act
        var episode = await _sut.GetEpisodeAsync(1, 1, 1);

        // Assert
        Assert.Null(episode);
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldGetEpisodesFromTmdb_WhenNotCached()
    {
        // Arrange
        SetupCacheMiss();
        const int series = 777;
        const int season = 3;
        const int episode = 6;
        var expected = TmdbTestUtils.CreateTmdbEpisode(seriesId: series, seasonNumber: season, episodeNumber: episode);
        _tmdbClientMock
            .Setup(x => x.GetEpisodeAsync(series, season, episode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await _sut.GetEpisodeAsync(series, season, episode);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(Episode.FromTmdbEpisode(expected), actual);
    }

    private void SetupCacheMiss()
    {
        _cacheMock
            .Setup(cache => cache.Get(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((List<Episode>?)null);
    }
}