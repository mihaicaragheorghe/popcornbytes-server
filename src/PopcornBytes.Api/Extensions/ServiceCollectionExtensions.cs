using System.Text;

using Dapper;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Persistence;
using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Security;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Api.Users;
using PopcornBytes.Contracts.Series;

using StackExchange.Redis;

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
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITvSeriesCache, TvSeriesCache>();
        services.AddSingleton<IEpisodesCache, EpisodesCache>();
        services.AddSingleton<ICacheService<string, SearchTvSeriesResponse>>(_ =>
            new CacheService<string, SearchTvSeriesResponse>(
                capacity: Convert.ToInt32(configuration["Cache:TvSeriesSearch:Capacity"] ?? "256"),
                expirationInHours: Convert.ToInt32(configuration["Cache:TvSeries:ExpirationInHours"] ?? "24")));

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddTransient<MigrationsRunner>();
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
            configuration.GetConnectionString("Redis")
            ?? throw new ArgumentException("No connection string configured for redis")));

        services.AddScoped<IUserRepository, UserRepository>();

        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new SqlGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}