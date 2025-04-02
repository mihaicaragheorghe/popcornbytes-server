using System.Reflection;

using Dapper;

namespace PopcornBytes.Api.Persistence;

public class MigrationsRunner(IDbConnectionFactory dbConnectionFactory, ILogger<MigrationsRunner> logger)
{
    public void RunMigrationsInAssembly(Assembly assembly)
    {
        logger.LogInformation("Running migrations for assembly {assembly}", assembly.FullName);

        long latest = GetCurrentVersion();
        List<Migration> migrationsToRun = [];

        foreach (var type in assembly.GetTypes()
                     .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration))))
        {
            var migration = (Migration?)Activator.CreateInstance(type);
            if (migration is null)
            {
                logger.LogError("Could not create migration instance for type {t}", type.FullName);
                continue;
            }

            if (migration.Version > latest)
            {
                migrationsToRun.Add(migration);
            }
        }

        var context = new MigrationContext(dbConnectionFactory, logger);

        foreach (var migration in migrationsToRun.OrderBy(m => m.Version))
        {
            try
            {
                logger.LogInformation("Updating to version {v}, {n}...", migration.Version, migration.Description);

                context.Direction = MigrationDirection.Up;
                migration.Run(context);
                UpdateVersion(migration.Version, migration.Description);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration {n} failed", migration.Description);

                context.Direction = MigrationDirection.Down;
                migration.Run(context);
                DeleteVersion(migration.Version);
            }
        }

        logger.LogInformation("Finished up migrations for {assembly}", assembly.FullName);
    }

    private long GetCurrentVersion()
    {
        if (IsInitialMigration()) return -1;

        using var connection = dbConnectionFactory.CreateSqlConnection();
        return connection.QuerySingle("SELECT version FROM version_info ORDER BY version DESC LIMIT 1");
    }

    private bool IsInitialMigration()
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return !connection.QueryFirstOrDefault<bool>(
            "SELECT 1 FROM sqlite_master WHERE type='table' AND name='version_info'");
    }

    private void UpdateVersion(long version, string description)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        connection.Execute(
            "INSERT INTO version_info (version, description, executed_at) VALUES (@version, @description, @UtcNow)",
            new { version, description, DateTime.UtcNow });
    }

    private void DeleteVersion(long version)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        connection.Execute("DELETE FROM version_info WHERE version = @version", new { version });
    }
}