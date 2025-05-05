namespace PopcornBytes.Api.Persistence.Migrations;

public class CreateSeriesCompletedTableMigration : Migration
{
    public override long Version => 2025_05_05_12_37;
    public override string Description => "Create completed series table";

    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS series_completed
                (
                    user_id TEXT NOT NULL,
                    series_id INT NOT NULL,
                    completed_at_unix INT NOT NULL,
                    PRIMARY KEY (user_id, series_id)
                );
                
                CREATE INDEX idx_series_completed_user_time ON series_completed (user_id, completed_at_unix DESC);
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS series_completed");
        Execute("DROP INDEX IF EXISTS idx_series_completed_user_time");
    }
}