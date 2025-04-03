using System.Net;

using Microsoft.Extensions.Options;

using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Tmdb;

public class TmdbClient : ITmdbClient
{
    const string Version = "3";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TmdbOptions _options;

    public TmdbClient(IHttpClientFactory httpClientFactory, IOptions<TmdbOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        using var client = CreateHttpClient();

        var response = await client.GetAsync($"/{Version}/authentication?api_key={_options.ApiKey}",
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateHttpClient();

        var response = await client.GetAsync(
            $"/{Version}/search/tv?api_key={_options.ApiKey}&query={query}&page={page}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<SearchTvSeriesResponse>(cancellationToken)
                   ?? throw new Exception($"Failed to find search result for {query}");

        foreach (var show in data.Results)
        {
            show.PosterUrl = ImageUrl(show.PosterPath, TmdbImageSize.W300);
        }

        return data;
    }

    public async Task<TmdbTvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateHttpClient();

        var response = await client.GetAsync($"/{Version}/tv/{id}?api_key={_options.ApiKey}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var series = await response.Content.ReadFromJsonAsync<TmdbTvSeries>(cancellationToken)
                     ?? throw new Exception($"Failed to find tv series for {id}");

        series.PosterPath = ImageUrl(series.PosterPath);
        if (series.NextEpisodeToAir != null)
        {
            series.NextEpisodeToAir.StillPath = ImageUrl(series.NextEpisodeToAir?.StillPath, TmdbImageSize.W300);
        }
        if (series.LastEpisodeToAir != null)
        {
            series.LastEpisodeToAir.StillPath = ImageUrl(series.LastEpisodeToAir.StillPath, TmdbImageSize.W300);
        }
        foreach (var season in series.Seasons)
        {
            season.PosterPath = ImageUrl(season.PosterPath, TmdbImageSize.W300);
        }

        return series;
    }

    private HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(nameof(TmdbClient));
    }

    private string ImageUrl(string? path, string size = TmdbImageSize.Original)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        
        return $"{_options.ImagesBaseUrl}/{size}{path.WithRequiredPrefix('/')}";
    }
}