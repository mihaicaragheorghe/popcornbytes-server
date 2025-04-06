using System.Reflection;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void RunMigrations(this WebApplication app)
    {
        var runner = app.Services.GetRequiredService<MigrationsRunner>();
        var assembly = Assembly.GetAssembly(typeof(Program))
            ?? throw new Exception($"Could not find assembly of {nameof(Program)}.");

        runner.RunMigrationsInAssembly(assembly);
    }
}