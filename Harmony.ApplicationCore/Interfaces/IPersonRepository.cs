using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;

namespace Harmony.ApplicationCore.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(PersonId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = default);
    Task AddAsync(Person person, CancellationToken cancellationToken = default);
    Task UpdateAsync(Person person, CancellationToken cancellationToken = default);
    Task DeleteAsync(PersonId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(PersonId id, CancellationToken cancellationToken = default);
}
