namespace PopcornBytes.Api.Persistence.Migrations;

public class CreateEpisodesWatchedTableMigration : Migration
{
    public override long Version => 2025_05_05_12_40;
    public override string Description => "Create watched episodes table";

    protected override void Up()
    {
        Execute("""
                CREATE TABLE IF NOT EXISTS episodes_watched
                (
                    user_id TEXT NOT NULL,
                    series_id INT NOT NULL,
                    season_number INT NOT NULL,
                    episode_number INT NOT NULL,
                    watched_at_unix INT NOT NULL,
                    PRIMARY KEY (user_id, series_id, season_number, episode_number)
                );

                CREATE INDEX idx_episodes_watched_user_time ON episodes_watched (user_id, watched_at_unix DESC);
                CREATE INDEX idx_episodes_watched_user_series ON episodes_watched (user_id, series_id);
                """);
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS episodes_watched");
        Execute("DROP INDEX IF EXISTS idx_episodes_watched_user_time");
        Execute("DROP INDEX IF EXISTS idx_episodes_watched_user_series");
    }
}