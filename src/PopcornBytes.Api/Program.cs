using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.Health;
using PopcornBytes.Api.Middleware;
using PopcornBytes.Api.Series;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddTmdbClient(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddHealthChecks()
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

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.MapSeriesEndpoints();
app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    ResponseWriter = HealthCheckExtensions.WriteResponse
});

app.RunMigrations();

app.Run();