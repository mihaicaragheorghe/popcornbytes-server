using System.Data;

using Dapper;

namespace PopcornBytes.Api.Persistence;

public abstract class Migration
{
    public abstract long Version { get; }

    public abstract string Description { get; }

    protected abstract void Up();

    protected abstract void Down();

    protected ILogger<MigrationsRunner> Logger => Context.Logger;

    private MigrationContext Context { get; set; } = null!;

    public void Run(MigrationContext context)
    {
        Context = context;

        if (context.Direction == MigrationDirection.Up)
        {
            Up();
        }
        else
        {
            Down();
        }
    }

    protected IDbConnection NewConnection() => Context.ConnectionFactory.CreateSqlConnection();

    protected int Execute(string cmd)
    {
        Logger.LogInformation("{n}{cmd}{n}", Environment.NewLine, cmd, Environment.NewLine);
        using var connection = Context.ConnectionFactory.CreateSqlConnection();
        connection.Open();

        if (!Context.UseTransaction)
        {
            return connection.Execute(cmd);
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            int rows = connection.Execute(cmd, transaction);
            transaction.Commit();
            return rows;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}