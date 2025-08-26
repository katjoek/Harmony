using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services;

public sealed class MembershipService : IMembershipService
{
    private readonly HarmonyDbContext _context;

    public MembershipService(HarmonyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddPersonToGroupAsync(PersonId personId, GroupId groupId, CancellationToken cancellationToken = default)
    {
        var existingMembership = await _context.PersonGroupMemberships
            .FirstOrDefaultAsync(m => m.PersonId == personId && m.GroupId == groupId, cancellationToken);

        if (existingMembership == null)
        {
            _context.PersonGroupMemberships.Add(new PersonGroupMembership
            {
                PersonId = personId,
                GroupId = groupId
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemovePersonFromGroupAsync(PersonId personId, GroupId groupId, CancellationToken cancellationToken = default)
    {
        var membership = await _context.PersonGroupMemberships
            .FirstOrDefaultAsync(m => m.PersonId == personId && m.GroupId == groupId, cancellationToken);

        if (membership != null)
        {
            _context.PersonGroupMemberships.Remove(membership);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<GroupId>> GetGroupIdsForPersonAsync(PersonId personId, CancellationToken cancellationToken = default)
    {
        return await _context.PersonGroupMemberships
            .Where(m => m.PersonId == personId)
            .Select(m => m.GroupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PersonId>> GetPersonIdsForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return await _context.PersonGroupMemberships
            .Where(m => m.GroupId == groupId)
            .Select(m => m.PersonId)
            .ToListAsync(cancellationToken);
    }
}
