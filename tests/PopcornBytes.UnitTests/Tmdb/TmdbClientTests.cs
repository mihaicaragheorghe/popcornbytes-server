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
            
        HttpClient httpClient = new(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_options.Value.BaseURl)
        };
            
        Mock<IHttpClientFactory> mockHttpClientFactory = new();
        mockHttpClientFactory
            .Setup(factory => factory.CreateClient(It.Is<string>(s => s == nameof(TmdbClient))))
            .Returns(httpClient);
        
        _sut = new TmdbClient(mockHttpClientFactory.Object, _options);
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
        var response = TmdbTestUtils.CreateSearchSeriesResponse(results: [TmdbTestUtils.CreateSearchSeriesResult(posterPath: posterPath)]);
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(response));

        // Act
        var result = await _sut.SearchTvSeriesAsync("succession");

        // Assert
        var url = result.Results[0].PosterUrl;
        Assert.NotNull(result);
        Assert.Contains($"/{posterPath}", url);
        Assert.True(Uri.IsWellFormedUriString(url, UriKind.Absolute));
    }
    
    [Fact]
    public async Task SearchTvSeriesAsync_ShouldThrowHttpRequestException_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.BadRequest, string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _sut.SearchTvSeriesAsync("mr robot"));
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
        Assert.NotNull(result);
        Assert.Contains($"/{posterPath}", result.PosterPath);
        Assert.Contains($"/{posterPath}", result.LastEpisodeToAir?.StillPath);
        Assert.Contains($"/{posterPath}", result.NextEpisodeToAir?.StillPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, result.PosterPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, result.LastEpisodeToAir?.StillPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, result.NextEpisodeToAir?.StillPath);
        Assert.True(Uri.IsWellFormedUriString(result.PosterPath, UriKind.Absolute));
        Assert.True(Uri.IsWellFormedUriString(result.LastEpisodeToAir?.StillPath, UriKind.Absolute));
        Assert.True(Uri.IsWellFormedUriString(result.NextEpisodeToAir?.StillPath, UriKind.Absolute));
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

    private void AssertTmdbTvSeries(TmdbTvSeries expected, TmdbTvSeries actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.FirstAirDate, actual.FirstAirDate);
        Assert.Equal(expected.LastAirDate, actual.LastAirDate);
        Assert.Equal(expected.NumberOfSeasons, actual.NumberOfSeasons);
        Assert.Equal(expected.InProduction, actual.InProduction);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.Tagline, actual.Tagline);
        
        Assert.Contains(expected.PosterPath, actual.PosterPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, actual.PosterPath);
        
        AssertTmdbEpisode(expected.NextEpisodeToAir, actual.NextEpisodeToAir);
        AssertTmdbEpisode(expected.LastEpisodeToAir, actual.LastEpisodeToAir);
    }

    private void AssertTmdbEpisode(TmdbEpisode? expected, TmdbEpisode? actual)
    {
        if (actual is null)
        {
            Assert.Null(expected);
            return;
        }
        Assert.NotNull(actual);
        Assert.NotNull(expected);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Overview, actual.Overview);
        Assert.Equal(expected.Runtime, actual.Runtime);
        Assert.Equal(expected.AirDate, actual.AirDate);
        Assert.Equal(expected.SeasonNumber, actual.SeasonNumber);
        Assert.Equal(expected.EpisodeNumber, actual.EpisodeNumber);
        Assert.Equal(expected.EpisodeType, actual.EpisodeType);
        
        Assert.Contains(expected.StillPath, actual.StillPath);
        Assert.Contains(_options.Value.ImagesBaseUrl, actual.StillPath);
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
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}