using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.TvSeries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTmdbClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapSeriesEndpoints();

app.Run();
