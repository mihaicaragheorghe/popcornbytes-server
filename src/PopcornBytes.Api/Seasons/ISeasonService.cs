namespace PopcornBytes.Api.Seasons;

public interface ISeasonService
{
    Task<Season?> GetSeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);
}
