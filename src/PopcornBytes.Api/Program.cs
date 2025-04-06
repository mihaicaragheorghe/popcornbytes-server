using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.Health;
using PopcornBytes.Api.Middleware;
using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Users;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddProblemDetails()
    .AddJwtAuthentication(builder.Configuration)
    .AddTmdbClient(builder.Configuration)
    .AddCaching(builder.Configuration)
    .AddPersistence()
    .AddApplicationServices()
    .AddHealthChecks()
        .AddCheck<DbHealthCheck>("db")
        .AddCheck<TmdbHealthCheck>("tmdb");

builder.Host.UseSerilog((_, config) =>
    config.WriteTo.Console());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapUserEndpoints();
app.MapSeriesEndpoints();
app.MapSeasonEndpoints();
app.MapEpisodeEndpoints();
app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    ResponseWriter = HealthCheckExtensions.WriteResponse
});

app.RunMigrations();

app.Run();