namespace Harmony.Web;

using Harmony.ApplicationCore.Interfaces;
using Harmony.Web.Commands;

internal static class SeedCommandRunner
{
    internal static async Task<bool> TryRunAsync(WebApplication app, string[] args)
    {
        if (args.Length == 0 || args[0] != "seed") return false;

        using var scope = app.Services.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();

        if (!await settingsService.IsDatabaseDirectoryConfiguredAsync())
        {
            Console.WriteLine("ERROR: Database directory is not configured.");
            Console.WriteLine("Run the application and configure the database directory through the web interface.");
            return true;
        }

        Console.WriteLine("Seeding database...");
        await scope.ServiceProvider.GetRequiredService<SeedDataCommand>().ExecuteAsync();
        Console.WriteLine("Seeding completed. Exiting application.");
        return true;
    }
}
