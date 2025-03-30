using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTmdbClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TmdbOptions>()
            .Bind(configuration.GetSection("Tmdb"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddHttpClient(nameof(TmdbClient), client =>
        {
            client.BaseAddress = new Uri(configuration["Tmdb:BaseUrl"]!);
        });
        
        services.AddSingleton<ITmdbClient, TmdbClient>();
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITvSeriesService, TvSeriesService>();
        
        return services;
    }
}
