using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories;

public sealed class GroupRepository : IGroupRepository
{
    private readonly HarmonyDbContext _context;

    public GroupRepository(HarmonyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default)
    {
        var groupIds = await _context.PersonGroupMemberships
            .Where(m => m.PersonId == personId)
            .Select(m => m.GroupId)
            .ToListAsync(cancellationToken);

        return await _context.Groups
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(id, cancellationToken);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync(cancellationToken);
            // Associated membership records are automatically deleted via database cascade delete
        }
    }

    public async Task<bool> ExistsAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .AnyAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, GroupId? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Groups.Where(g => g.Name == name);
        
        if (excludeId != null)
        {
            query = query.Where(g => g.Id != excludeId);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
