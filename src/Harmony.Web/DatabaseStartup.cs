namespace Harmony.Web;

using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

internal static class DatabaseStartup
{
    internal static async Task RunAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        if (!await settingsService.IsDatabaseDirectoryConfiguredAsync()) return;

        var context = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
        await EnsureMigrationHistoryExistsAsync(context, app.Logger);
        await context.Database.MigrateAsync();

        var cleanupService = scope.ServiceProvider.GetRequiredService<IDatabaseCleanupService>();
        var deletedCount = await cleanupService.CleanupOrphanedMembershipsAsync();
        if (deletedCount > 0)
            app.Logger.LogInformation("Database cleanup: Removed {DeletedCount} orphaned group membership entries.", deletedCount);
    }

    private static async Task EnsureMigrationHistoryExistsAsync(HarmonyDbContext context, ILogger logger)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var checkHistoryCmd = connection.CreateCommand();
            checkHistoryCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'";
            if (await checkHistoryCmd.ExecuteScalarAsync() != null) return;

            using var checkPersonsCmd = connection.CreateCommand();
            checkPersonsCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Persons'";
            if (await checkPersonsCmd.ExecuteScalarAsync() == null) return;

            logger.LogInformation("Existing database detected without migration history. Creating migration history table...");

            using var createHistoryCmd = connection.CreateCommand();
            createHistoryCmd.CommandText = @"
                CREATE TABLE __EFMigrationsHistory (
                    MigrationId TEXT NOT NULL PRIMARY KEY,
                    ProductVersion TEXT NOT NULL
                )";
            await createHistoryCmd.ExecuteNonQueryAsync();

            var migrationsToMark = new List<string> { "20250826072508_InitialCreate" };

            using var checkIndexCmd = connection.CreateCommand();
            checkIndexCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name='IX_Persons_EmailAddress'";
            if (await checkIndexCmd.ExecuteScalarAsync() != null)
                migrationsToMark.Add("20250826090247_PerformanceIndexes");

            using var checkMembershipCmd = connection.CreateCommand();
            checkMembershipCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='PersonGroupMemberships'";
            if (await checkMembershipCmd.ExecuteScalarAsync() != null)
                migrationsToMark.Add("20260110150415_AddCascadeDeleteToMemberships");

            using var checkConfigCmd = connection.CreateCommand();
            checkConfigCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Configs'";
            if (await checkConfigCmd.ExecuteScalarAsync() != null)
                migrationsToMark.Add("20260110175328_AddConfigTable");

            const string efCoreVersion = "9.0.1";
            foreach (var migrationId in migrationsToMark)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = $"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('{migrationId}', '{efCoreVersion}')";
                await insertCmd.ExecuteNonQueryAsync();
                logger.LogInformation("Marked migration as applied: {MigrationId}", migrationId);
            }

            logger.LogInformation("Migration history created. {Count} migrations marked as applied.", migrationsToMark.Count);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
