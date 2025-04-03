using Dapper;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.Api.Health;

public class DbHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DbHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = dbConnectionFactory.CreateSqlConnection();

            int result = await connection.QuerySingleAsync<int>("SELECT 1");

            return result == 1 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{m}", ex.Message);
            return HealthCheckResult.Unhealthy();
        }
    }
}