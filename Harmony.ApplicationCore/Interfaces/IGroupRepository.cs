using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;

namespace Harmony.ApplicationCore.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    Task UpdateAsync(Group group, CancellationToken cancellationToken = default);
    Task DeleteAsync(GroupId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(GroupId id, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string name, GroupId? excludeId = null, CancellationToken cancellationToken = default);
}
