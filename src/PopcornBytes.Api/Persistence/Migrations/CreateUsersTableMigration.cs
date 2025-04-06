namespace PopcornBytes.Api.Persistence.Migrations;

public class CreateUsersTableMigration : Migration
{
    public override long Version => 2025_04_06_10_25;
    public override string Description => "Create users table";
    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS users
                (
                    id TEXT NOT NULL PRIMARY KEY,
                    email TEXT NOT NULL UNIQUE,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    created_at TIMESTAMP NOT NULL
                )
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS users");
    }
}