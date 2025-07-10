namespace PopcornBytes.Api.Persistence.Migrations;

public class CreateSeriesWatchingTableMigration : Migration
{
    public override long Version => 2025_05_05_12_35;
    public override string Description => "Create watching series table";

    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS series_watching
                (
                    user_id TEXT NOT NULL,
                    series_id INT NOT NULL,
                    started_at_unix INT NOT NULL,
                    is_stopped INT NOT NULL DEFAULT 0,
                    updated_at_unix INT NOT NULL DEFAULT (strftime('%s', 'now')),
                    PRIMARY KEY (user_id, series_id)
                );

                CREATE INDEX idx_series_watching_user_time ON series_watching (user_id, started_at_unix DESC);
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS series_watching");
        Execute("DROP INDEX IF EXISTS idx_series_watching_user_time");
    }
}