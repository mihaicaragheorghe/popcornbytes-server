using Microsoft.Extensions.Options;

using PopcornBytes.Contracts.TvSeries;

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

    public async Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(nameof(TmdbClient));

        var response = await client.GetAsync(
            $"/{Version}/search/tv?api_key={_options.ApiKey}&query={query}&page={page}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<SearchTvSeriesResponse>(cancellationToken: cancellationToken)
            ?? throw new Exception($"Failed to find search result for {query}");

        foreach (var show in data.Results)
        {
            string size = "w300";
            if (!show.PosterPath.StartsWith('/'))
            {
                size += '/';
            }
            show.PosterUrl = $"{_options.ImagesBaseUrl}/{size}{show.PosterPath}";
        }

        return data;
    }
}
