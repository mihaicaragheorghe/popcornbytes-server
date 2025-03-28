using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.Middleware;
using PopcornBytes.Api.TvSeries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddTmdbClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapSeriesEndpoints();

app.Run();
