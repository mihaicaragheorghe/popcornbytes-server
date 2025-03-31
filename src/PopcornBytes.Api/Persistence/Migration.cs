namespace PopcornBytes.Api.Persistence;

public abstract class Migration
{
    public abstract ulong Version { get; }

    public abstract string Name { get; }
    
    protected readonly IDbConnectionFactory _dbConnectionFactory;

    public Migration(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public abstract void Up();
    
    public abstract void Down();
}
