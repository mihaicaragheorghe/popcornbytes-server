using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;
using PopcornBytes.UnitTests.TestUtils;

namespace PopcornBytes.UnitTests.Tmdb;

public class TmdbClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly IOptions<TmdbOptions> _options;
    private readonly TmdbClient _sut;

    public TmdbClientTests()
    {
        _options = Options.Create(new TmdbOptions
        {
            BaseURl = "https://api.themoviedb.org",
            ApiKey = "test-api-key",
            ImagesBaseUrl = "https://image.tmdb.org/t/p"
        });

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        HttpClient httpClient = new(_mockHttpMessageHandler.Object) { BaseAddress = new Uri(_options.Value.BaseURl) };

        Mock<IHttpClientFactory> mockHttpClientFactory = new();
        mockHttpClientFactory
            .Setup(factory => factory.CreateClient(It.Is<string>(s => s == nameof(TmdbClient))))
            .Returns(httpClient);

        _sut = new TmdbClient(mockHttpClientFactory.Object, _options);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldThrow_WhenStatusCodeNotOk()
    {
        // Arrange
        object response = new { success = false };
        SetupMockHandlerResponse(HttpStatusCode.Unauthorized, JsonSerializer.Serialize(response));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.AuthenticateAsync());
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldNotThrow_WhenStatusCodeOk()
    {
        // Arrange
        object response = new { success = true };
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var exception = await Record.ExceptionAsync(() => _sut.AuthenticateAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldThrow_WhenStatusCodeNotOk()
    {
        // Arrange
        object response = new { success = false };
        SetupMockHandlerResponse(HttpStatusCode.Unauthorized, JsonSerializer.Serialize(response));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.AuthenticateAsync());
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnConfiguration_WhenApiReturnsValidData()
    {
        // Arrange
        var expected = new TmdbConfiguration
        {
            ChangeKeys =
                ["key_1", "key_2", "key_3"],
            Image = new TmdbImageConfiguration
            {
                BaseUrl = "https://image.tmdb.org/t/p",
                BackdropSizes = ["bs1"],
                LogoSizes = ["ls1", "ls2"],
                PosterSizes = ["ps1", "ps2", "ps3"],
                StillSizes = ["sz1", "sz2", "sz3", "sz4"]
            }
        };
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.GetConfigurationAsync();

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldThrow_WhenResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act
        await Assert.ThrowsAsync<JsonException>(() => _sut.GetConfigurationAsync());
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldReturnFormattedResults_WhenApiReturnsValidData()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateSearchSeriesResponse(pages: 1, totalPages: 1, totalResults: 1);
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.SearchTvSeriesAsync("twin peaks");

        // Assert
        AssertSearchSeriesResponse(expected, actual);
        AssertSearchSeriesResult(expected.Results[0], actual.Results[0]);
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldFormatPosterUrl_WhenPosterPathHasNoLeadingSlash()
    {
        // Arrange
        string posterPath = "poster-without-leading-slash";
        var response = TmdbTestUtils.CreateSearchSeriesResponse(results:
            [TmdbTestUtils.CreateSearchSeriesResult(posterPath: posterPath)]);
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _sut.SearchTvSeriesAsync("succession");

        // Assert
        var url = result.Results[0].PosterUrl;
        Assert.NotNull(result);
        Assert.Contains($"/{posterPath}", url);
        Assert.True(Uri.IsWellFormedUriString(url, UriKind.Absolute));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task SearchTvSeriesAsync_ShouldThrowHttpRequestException_WhenApiReturnsNonSuccessStatusCode(
        HttpStatusCode statusCode)
    {
        // Arrange
        SetupMockHandlerResponse(statusCode, string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _sut.SearchTvSeriesAsync("mr robot"));
        Assert.Equal(statusCode, exception.StatusCode);
    }

    [Fact]
    public async Task SearchTvSeriesAsync_ShouldThrowException_WhenResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() =>
            _sut.SearchTvSeriesAsync("sopranos"));
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldThrowHttpRequestException_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.BadRequest, string.Empty);

        // // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _sut.SearchTvSeriesAsync("atlanta"));
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldThrowException_WhenResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() =>
            _sut.SearchTvSeriesAsync("sopranos"));
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldFormatImageUrls_WhenNoLeadingSlashes()
    {
        // Arrange
        const string posterPath = "poster-without-leading-slash";
        var response = TmdbTestUtils.CreateTmdbTvSeries(
            posterPath: posterPath,
            lastEpisode: TmdbTestUtils.CreateTmdbEpisode(id: 1, stillPath: posterPath),
            nextEpisode: TmdbTestUtils.CreateTmdbEpisode(id: 2, stillPath: posterPath));

        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _sut.GetTvSeriesAsync(response.Id);

        // Assert
        AssertTmdbTvSeries(response, result);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnSeries_WhenApiReturnsValidData()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbTvSeries();
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.GetTvSeriesAsync(expected.Id);

        // Assert
        AssertTmdbTvSeries(expected, actual);
    }

    [Fact]
    public async Task GetTvSeriesAsync_ShouldReturnNull_WhenApiReturns404()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.NotFound, string.Empty);

        // Act
        var actual = await _sut.GetTvSeriesAsync(0);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetSeasonAsync_ShouldReturnNull_WhenApiReturns404()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.NotFound, string.Empty);

        // Act
        var actual = await _sut.GetSeasonAsync(1, 1, CancellationToken.None);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetSeasonAsync_ShouldReturnSeason_WhenApiReturnsValidData()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbSeason();
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.GetSeasonAsync(1, expected.Id, CancellationToken.None);

        // Assert
        AssertTmdbSeason(expected, actual!);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetSeasonAsync_ShouldThrowHttpException_WhenApiReturnsNonSuccessStatusCode(
        HttpStatusCode statusCode)
    {
        // Arrange
        SetupMockHandlerResponse(statusCode, "invalid json");

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<HttpRequestException>(() => _sut.GetSeasonAsync(1, 1, CancellationToken.None));
        Assert.Equal(exception.StatusCode, statusCode);
    }

    [Fact]
    public async Task GetSeasonAsync_ShouldThrowException_WhenResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act
        await Assert.ThrowsAsync<JsonException>(() => _sut.GetSeasonAsync(1, 1, CancellationToken.None));
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldReturnNull_WhenApiReturns404()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.NotFound, string.Empty);

        // Act
        var actual = await _sut.GetEpisodeAsync(1, 1, 1, CancellationToken.None);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldReturnSeason_WhenApiReturnsValidData()
    {
        // Arrange
        var expected = TmdbTestUtils.CreateTmdbEpisode();
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.GetEpisodeAsync(expected.SeriesId, expected.SeasonNumber, expected.Id,
            CancellationToken.None);

        // Assert
        AssertTmdbEpisode(expected, actual!);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetEpisodeAsync_ShouldThrowHttpException_WhenApiReturnsNonSuccessStatusCode(
        HttpStatusCode statusCode)
    {
        // Arrange
        SetupMockHandlerResponse(statusCode, "invalid json");

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<HttpRequestException>(() => _sut.GetEpisodeAsync(1, 1, 1, CancellationToken.None));
        Assert.Equal(exception.StatusCode, statusCode);
    }

    [Fact]
    public async Task GetEpisodeAsync_ShouldThrowException_WhenResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act
        await Assert.ThrowsAsync<JsonException>(() => _sut.GetEpisodeAsync(1, 1, 1, CancellationToken.None));
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnNull_WhenSeasonNotFound()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.NotFound, string.Empty);

        // Act
        var episodes = await _sut.GetEpisodesAsync(1, 1);

        // Assert
        Assert.Null(episodes);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnEmptyCollection_WhenSeasonExistsWithNullEpisodes()
    {
        // Arrange
        var season = TmdbTestUtils.CreateTmdbSeason();
        season.Episodes = null;
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(season));

        // Act
        var episodes = await _sut.GetEpisodesAsync(1, 1);

        // Assert
        Assert.NotNull(episodes);
        Assert.Empty(episodes);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnEmptyCollection_WhenSeasonExistsWithEmptyEpisodes()
    {
        // Arrange
        var season = TmdbTestUtils.CreateTmdbSeason(episodes: []);
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(season));

        // Act
        var episodes = await _sut.GetEpisodesAsync(1, 1);

        // Assert
        Assert.NotNull(episodes);
        Assert.Empty(episodes);
    }

    [Fact]
    public async Task GetEpisodesAsync_ShouldReturnEpisodes_WhenSeasonExistsWithEpisodes()
    {
        // Arrange
        var season = TmdbTestUtils.CreateTmdbSeason(episodes: TmdbTestUtils.CreateTmdbEpisodeCollection(13));
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(season));

        // Act
        var episodes = await _sut.GetEpisodesAsync(1, 1);

        // Assert
        Assert.NotNull(episodes);
        Assert.NotEmpty(episodes);
        for (int i = 0; i < season.Episodes?.Count; i++)
        {
            AssertTmdbEpisode(season.Episodes[i], episodes[i]);
        }
    }

    private static void AssertSearchSeriesResponse(SearchTvSeriesResponse expected, SearchTvSeriesResponse actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.TotalPages, actual.TotalPages);
        Assert.Equal(expected.TotalResults, actual.TotalResults);
    }

    private void AssertSearchSeriesResult(SearchTvSeriesResult expected, SearchTvSeriesResult actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.PosterPath, actual.PosterPath);

        Assert.Contains(expected.PosterUrl, actual.PosterUrl);
        Assert.Contains(expected.PosterUrl, _options.Value.ImagesBaseUrl);
    }

    private void AssertTmdbTvSeries(TmdbTvSeries expected, TmdbTvSeries? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.FirstAirDate, actual.FirstAirDate);
        Assert.Equal(expected.LastAirDate, actual.LastAirDate);
        Assert.Equal(expected.NumberOfSeasons, actual.NumberOfSeasons);
        Assert.Equal(expected.InProduction, actual.InProduction);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.Tagline, actual.Tagline);

        Assert.Contains(expected.PosterPath!, actual.PosterPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, actual.PosterPath);
        Assert.True(Uri.IsWellFormedUriString(actual.PosterPath, UriKind.Absolute));

        AssertTmdbEpisode(expected.NextEpisodeToAir, actual.NextEpisodeToAir);
        AssertTmdbEpisode(expected.LastEpisodeToAir, actual.LastEpisodeToAir);

        Assert.Equal(expected.Seasons.Count, actual.Seasons.Count);
        for (int i = 0; i < expected.Seasons.Count; i++)
        {
            AssertTmdbSeason(expected.Seasons[i], actual.Seasons[i]);
        }
    }

    private void AssertTmdbSeason(TmdbSeason expected, TmdbSeason actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.SeasonNumber, actual.SeasonNumber);
        Assert.Equal(expected.EpisodeCount, actual.EpisodeCount);
        Assert.Equal(expected.AirDate, actual.AirDate);

        Assert.Contains(expected.PosterPath!, actual.PosterPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, actual.PosterPath);
        Assert.True(Uri.IsWellFormedUriString(actual.PosterPath, UriKind.Absolute));
    }

    private void AssertTmdbEpisode(TmdbEpisode? expected, TmdbEpisode? actual)
    {
        if (actual is null)
        {
            Assert.Null(expected);
            return;
        }

        Assert.NotNull(expected);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.Runtime, actual.Runtime);
        Assert.Equal(expected.AirDate, actual.AirDate);
        Assert.Equal(expected.SeasonNumber, actual.SeasonNumber);
        Assert.Equal(expected.EpisodeNumber, actual.EpisodeNumber);
        Assert.Equal(expected.EpisodeType, actual.EpisodeType);

        Assert.Contains(expected.StillPath!, actual.StillPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, actual.StillPath);
        Assert.True(Uri.IsWellFormedUriString(actual.StillPath, UriKind.Absolute));
    }

    private void SetupMockHandlerResponse(HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri!.ToString().StartsWith(_options.Value.BaseURl)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode, Content = new StringContent(content) });
    }
}