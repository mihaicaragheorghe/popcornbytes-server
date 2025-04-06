using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Persistence;
using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Security;
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
        services.AddScoped<ISeasonService, SeasonService>();
        services.AddScoped<IEpisodeService, EpisodeService>();

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICacheService<int, TvSeries>>(_ => new CacheService<int, TvSeries>(
            capacity: Convert.ToInt32(configuration["Cache:TvSeries:Capacity"] ?? "256"),
            expirationInHours: Convert.ToInt32(configuration["Cache:TvSeries:ExpirationInHours"] ?? "24")));
        
        services.AddSingleton<ICacheService<string, List<Episode>>>(_ => new CacheService<string, List<Episode>>(
            capacity: Convert.ToInt32(configuration["Cache:Episodes:Capacity"] ?? "64"),
            expirationInHours: Convert.ToInt32(configuration["Cache:Episodes:ExpirationInHours"] ?? "24")));

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddTransient<MigrationsRunner>();
        
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                };
            });

        services.AddAuthorization();

        return services;
    }
}