using PopcornBytes.Api.Extensions;
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

app.RunMigrations();

app.Run();