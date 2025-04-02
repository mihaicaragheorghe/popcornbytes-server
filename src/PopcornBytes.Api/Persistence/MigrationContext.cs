namespace PopcornBytes.Api.Persistence;

public class MigrationContext
{
    public IDbConnectionFactory ConnectionFactory { get; }
    
    public ILogger<MigrationsRunner> Logger { get; }

    public MigrationDirection Direction { get; set; } = MigrationDirection.Up;

    public bool UseTransaction { get; set; } = true;
    
    public MigrationContext(IDbConnectionFactory connectionFactory, ILogger<MigrationsRunner> logger)
    {
        ConnectionFactory = connectionFactory;
        Logger = logger;
    }
}

public enum MigrationDirection { Up, Down }
