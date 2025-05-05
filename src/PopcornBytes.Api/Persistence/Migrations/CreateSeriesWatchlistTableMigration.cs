namespace PopcornBytes.Api.Persistence.Migrations;

public class CreateSeriesWatchlistTableMigration : Migration
{
    public override long Version => 2025_05_05_12_32;
    public override string Description => "Create series watchlist table";

    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS series_watchlist
                (
                    user_id TEXT NOT NULL,
                    series_id INT NOT NULL,
                    added_at_unix INT NOT NULL,
                    PRIMARY KEY (user_id, series_id)
                );

                CREATE INDEX idx_series_watchlist_user_time ON series_watchlist (user_id, added_at_unix DESC);
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS series_watchlist");
        Execute("DROP INDEX IF EXISTS idx_series_watchlist_user_time");
    }
}