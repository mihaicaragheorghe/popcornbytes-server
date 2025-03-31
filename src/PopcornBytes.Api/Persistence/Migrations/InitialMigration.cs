using Dapper;

namespace PopcornBytes.Api.Persistence.Migrations;

public class InitialMigration : Migration
{
    public InitialMigration(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public override ulong Version => 0;
    
    public override string Name => "InitialMigration";
    
    public override void Up()
    {
        using var connection = _dbConnectionFactory.CreateSqlConnection();

        connection.Execute(
            """
            CREATE TABLE IF NOT EXISTS migrations
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                version INTEGER UNIQUE NOT NULL,
                name TEXT NOT NULL,
                executed_at TIMESTAMP NOT NULL
            )
            """);
    }

    public override void Down()
    {
        using var connection = _dbConnectionFactory.CreateSqlConnection();
        
        connection.Execute("DROP TABLE IF EXISTS migrations");
    }
}