using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, GroupDto?>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public GetGroupByIdQueryHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository,
        IMembershipService membershipService)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task<GroupDto?> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var groupId = GroupId.From(request.Id);
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

        if (group == null)
            return null;

        string? coordinatorName = null;
        if (group.CoordinatorId != null)
        {
            var coordinator = await _personRepository.GetByIdAsync(group.CoordinatorId, cancellationToken);
            coordinatorName = coordinator?.Name.FullName;
        }

        // Get actual member IDs and count from the membership service
        var memberIds = await _membershipService.GetPersonIdsForGroupAsync(group.Id, cancellationToken);

        return new GroupDto(
            group.Id.ToString(),
            group.Name,
            group.CoordinatorId?.ToString(),
            coordinatorName,
            memberIds.Select(m => m.ToString()).ToList(),
            memberIds.Count);
    }
}
