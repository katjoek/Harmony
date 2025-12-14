using Harmony.Domain.ValueObjects;

namespace Harmony.ApplicationCore.Interfaces;

public interface IMembershipService
{
    Task AddPersonToGroupAsync(PersonId personId, GroupId groupId, CancellationToken cancellationToken = default);
    Task RemovePersonFromGroupAsync(PersonId personId, GroupId groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupId>> GetGroupIdsForPersonAsync(PersonId personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonId>> GetPersonIdsForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(PersonId PersonId, GroupId GroupId)>> GetAllMembershipsAsync(CancellationToken cancellationToken = default);
}
