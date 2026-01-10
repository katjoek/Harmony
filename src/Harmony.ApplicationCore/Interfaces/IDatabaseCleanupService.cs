namespace Harmony.ApplicationCore.Interfaces;

public interface IDatabaseCleanupService
{
    Task<int> CleanupOrphanedMembershipsAsync(CancellationToken cancellationToken = default);
}

