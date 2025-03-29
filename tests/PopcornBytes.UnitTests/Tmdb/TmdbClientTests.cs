using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using PopcornBytes.Api.Tmdb;
using PopcornBytes.Contracts.TvSeries;

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
        var expected = CreateSampleResponse(pages: 1, totalPages: 1, totalResults: 1);
        SetupMockHandlerResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expected));

        // Act
        var actual = await _sut.SearchTvSeriesAsync("twin peaks");

        // Assert
        AssertResponse(actual, expected);
        AssertResult(actual.Results[0], expected.Results[0]);
    }
    
    [Fact]
    public async Task SearchTvSeriesAsync_Should_FormatPosterUrl_When_PosterPathHasNoLeadingSlash()
    {
        // Arrange
        string posterPath = "poster-without-leading-slash";
        var response = CreateSampleResponse(results: [CreateSampleResult(posterPath: posterPath)]);
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
    public async Task SearchTvSeriesAsync_Should_ThrowHttpRequestException_When_ApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.BadRequest, string.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _sut.SearchTvSeriesAsync("mr robot"));
    }

    [Fact]
    public async Task SearchTvSeriesAsync_Should_ThrowException_When_ResponseCannotBeDeserialized()
    {
        // Arrange
        SetupMockHandlerResponse(HttpStatusCode.OK, "invalid json");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => 
            _sut.SearchTvSeriesAsync("sopranos"));
    }

    private static void AssertResponse(SearchTvSeriesResponse actual, SearchTvSeriesResponse expected)
    {
        Assert.NotNull(actual);
        Assert.Equal(actual.Page, expected.Page);
        Assert.Equal(actual.TotalPages, expected.TotalPages);
        Assert.Equal(actual.TotalResults, expected.TotalResults);
    }

    private void AssertResult(SearchTvSeriesResult actual, SearchTvSeriesResult expected)
    {
        Assert.NotNull(actual);
        Assert.Equal(actual.Id, expected.Id);
        Assert.Equal(actual.Name, expected.Name);
        Assert.Equal(actual.Overview, expected.Overview);
        Assert.Equal(actual.PosterPath, expected.PosterPath);
        
        Assert.Contains(expected.PosterUrl, actual.PosterUrl);
        Assert.Contains(expected.PosterUrl, _options.Value.ImagesBaseUrl);
    }
    
    private static SearchTvSeriesResponse CreateSampleResponse(
        int pages = 1,
        int totalPages = 1,
        int totalResults = 1,
        SearchTvSeriesResult[]? results = null) =>
        new()
        {
            Page = pages,
            TotalPages = totalPages,
            TotalResults = totalResults,
            Results = results ?? CreateSampleResults(totalResults)
        };

    private static SearchTvSeriesResult CreateSampleResult(
        int id = 1,
        string name = "Twin Peaks",
        string overview = "Laura Palmer",
        string posterPath = "/poster.jpg") =>
        new()
        {
            Id = id, Name = name, Overview = overview, PosterPath = posterPath,
        };
    
    private static SearchTvSeriesResult[] CreateSampleResults(int count) => Enumerable
        .Range(1, count)
        .Select(i => CreateSampleResult(id: i))
        .ToArray();

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