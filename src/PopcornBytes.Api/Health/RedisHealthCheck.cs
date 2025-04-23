using Microsoft.Extensions.Diagnostics.HealthChecks;

using StackExchange.Redis;

namespace PopcornBytes.Api.Health;

public class RedisHealthCheck(IConnectionMultiplexer mutex, ILogger<RedisHealthCheck> logger) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = mutex.GetDatabase();
            var latency = db.Ping();
            return Task.FromResult(HealthCheckResult.Healthy(null, new Dictionary<string, object>
            {
                { "Latency", latency }
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{m}", ex.Message);
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}
