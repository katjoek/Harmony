using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services;

public sealed class DatabaseCleanupService : IDatabaseCleanupService
{
    private readonly HarmonyDbContext _context;

    public DatabaseCleanupService(HarmonyDbContext context)
    {
        _context = context;
    }

    public async Task<int> CleanupOrphanedMembershipsAsync(CancellationToken cancellationToken = default)
    {
        // Find all PersonGroupMembership entries where PersonId doesn't exist in Persons table
        // or GroupId doesn't exist in Groups table
        var orphanedMemberships = await _context.PersonGroupMemberships
            .Where(m => !_context.Persons.Any(p => p.Id == m.PersonId) ||
                        !_context.Groups.Any(g => g.Id == m.GroupId))
            .ToListAsync(cancellationToken);

        if (orphanedMemberships.Count == 0)
        {
            return 0;
        }

        // Remove orphaned memberships
        _context.PersonGroupMemberships.RemoveRange(orphanedMemberships);
        var deletedCount = await _context.SaveChangesAsync(cancellationToken);

        return deletedCount;
    }
}

