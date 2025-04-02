namespace PopcornBytes.Api.Persistence.Migrations;

public class InitialMigration : Migration
{
    public override long Version => 0;

    public override string Description => "Initial migration";

    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS version_info
                (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    version INTEGER UNIQUE NOT NULL,
                    description TEXT NOT NULL,
                    executed_at TIMESTAMP NOT NULL
                )
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS version_info");
    }
}
