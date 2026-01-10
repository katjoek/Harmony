using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using Harmony.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories;

public sealed class PersonRepository : IPersonRepository
{
    private readonly HarmonyDbContext _context;

    public PersonRepository(HarmonyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Person?> GetByIdAsync(PersonId id, CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .OrderBy(p => p.Name.FirstName)
            .ThenBy(p => p.Name.Surname)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Person>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        var personIds = await _context.PersonGroupMemberships
            .Where(m => m.GroupId == groupId)
            .Select(m => m.PersonId)
            .ToListAsync(cancellationToken);

        return await _context.Persons
            .Where(p => personIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PersonId id, CancellationToken cancellationToken = default)
    {
        var person = await GetByIdAsync(id, cancellationToken);
        if (person != null)
        {
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync(cancellationToken);
            // Associated membership records are automatically deleted via database cascade delete
        }
    }

    public async Task<bool> ExistsAsync(PersonId id, CancellationToken cancellationToken = default)
    {
        return await _context.Persons
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}
