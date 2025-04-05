namespace PopcornBytes.Api.Seasons;

public static class SeasonEndpoints
{
    public static void MapSeasonEndpoints(this WebApplication app)
    {
        app.MapGet("/series/{seriesId:int}/seasons/{season:int}",
            async Task<IResult> (int seriesId, int season, ISeasonService service, CancellationToken cancellation) =>
            {
                var series = await service.GetSeasonAsync(seriesId, season, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            });
    }
}