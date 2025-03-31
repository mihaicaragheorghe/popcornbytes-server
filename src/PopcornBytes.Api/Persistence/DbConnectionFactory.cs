using System.Data;

using Microsoft.Data.Sqlite;

namespace PopcornBytes.Api.Persistence;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateSqlConnection()
    {
        string? connectionString = _configuration.GetConnectionString("PopcornDB");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return new SqliteConnection(connectionString);
    }
}