using Microsoft.Extensions.Diagnostics.HealthChecks;

using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Health;

public class TmdbHealthCheck(ITmdbClient client, ILogger<TmdbHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await client.AuthenticateAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{m}", ex.Message);
            return HealthCheckResult.Unhealthy();
        }
    }
}