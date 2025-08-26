using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed class GetAllGroupsQueryHandler : IRequestHandler<GetAllGroupsQuery, IReadOnlyList<GroupDto>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public GetAllGroupsQueryHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository,
        IMembershipService membershipService)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task<IReadOnlyList<GroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllAsync(cancellationToken);
        var result = new List<GroupDto>();

        foreach (var group in groups)
        {
            string? coordinatorName = null;
            if (group.CoordinatorId != null)
            {
                var coordinator = await _personRepository.GetByIdAsync(group.CoordinatorId, cancellationToken);
                coordinatorName = coordinator?.Name.FullName;
            }

            // Get actual member IDs and count from the membership service
            var memberIds = await _membershipService.GetPersonIdsForGroupAsync(group.Id, cancellationToken);

            result.Add(new GroupDto(
                group.Id.ToString(),
                group.Name,
                group.CoordinatorId?.ToString(),
                coordinatorName,
                memberIds.Select(m => m.ToString()).ToList(),
                memberIds.Count));
        }

        return result;
    }
}
