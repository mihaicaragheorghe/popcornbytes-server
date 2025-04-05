namespace PopcornBytes.Api.Episodes;

public static class EpisodeEndpoints
{
    public static void MapEpisodeEndpoints(this WebApplication app)
    {
        app.MapGet("/series/{seriesId:int}/seasons/{season:int}/episodes",
            async Task<IResult> (int seriesId, int season, IEpisodeService service, CancellationToken cancellation) =>
            {
                var series = await service.GetEpisodesAsync(seriesId, season, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            });

        app.MapGet("/series/{seriesId:int}/seasons/{season:int}/episodes/{episode:int}",
            async Task<IResult> (int seriesId, int season, int episode, IEpisodeService service,
                CancellationToken cancellation) =>
            {
                var series = await service.GetEpisodeAsync(seriesId, season, episode, cancellation);
                return series is null ? Results.NotFound() : Results.Ok(series);
            });
    }
}