namespace PopcornBytes.Api.Episodes;

public interface IEpisodesCache
{
    Task<List<Episode>?> Get(int seriesId, int seasonNum);

    Task<bool> Set(IEnumerable<Episode> episodes);
}
