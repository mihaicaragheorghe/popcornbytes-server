using System.Reflection;

using Dapper;

namespace PopcornBytes.Api.Persistence;

public class MigrationsRunner(IDbConnectionFactory dbConnectionFactory, ILogger<MigrationsRunner> logger)
{
    public void RunMigrationsInAssembly(Assembly assembly)
    {
        logger.LogInformation("Running migrations for assembly {assembly}", assembly.FullName);
        
        HashSet<ulong> existingVersions = GetExistingVersions();
        List<Migration> migrations = [];

        foreach (var type in assembly.GetTypes()
                     .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration))))
        {
            var instance = (Migration?)Activator.CreateInstance(type, dbConnectionFactory);
            if (instance is null)
            {
                logger.LogError("Could not create migration instance for type {t}", type.FullName);
                continue;
            }

            if (!existingVersions.Contains(instance.Version))
            {
                migrations.Add(instance);
            }
        }

        foreach (var migration in migrations.OrderBy(m => m.Version))
        {
            try
            {
                logger.LogInformation("Running migration {v}, {n}", migration.Version, migration.Name);
                migration.Up();
                RecordMigration(migration);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration {n} failed", migration.Name);
                migration.Down();
                DeleteRecord(migration.Version);
            }
        }

        logger.LogInformation("Finished up migrations for {assembly}", assembly.FullName);
    }

    private HashSet<ulong> GetExistingVersions()
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        bool tableExists = connection.QueryFirstOrDefault<bool>(
            "SELECT 1 FROM sqlite_master WHERE type='table' AND name='migrations'");

        return !tableExists ? [] : connection.Query<ulong>("SELECT version FROM migrations").ToHashSet();
    }

    private void RecordMigration(Migration migration)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        connection.Execute(
            "INSERT INTO migrations (version, name, executed_at) VALUES (@Version, @Name, @UtcNow)",
            new { migration.Version, migration.Name, DateTime.UtcNow });
    }

    private void DeleteRecord(ulong version)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();
        
        connection.Execute("DELETE FROM migrations WHERE version = @Version",
            new { Version = version });
    }
}
