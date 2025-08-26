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
        // Get all data in parallel for maximum performance
        var groupsTask = _groupRepository.GetAllAsync(cancellationToken);
        var allPersonsTask = _personRepository.GetAllAsync(cancellationToken);
        var allMembershipsTask = _membershipService.GetAllMembershipsAsync(cancellationToken);

        await Task.WhenAll(groupsTask, allPersonsTask, allMembershipsTask);

        var groups = await groupsTask;
        var allPersons = await allPersonsTask;
        var allMemberships = await allMembershipsTask;

        // Create lookup dictionaries for fast access
        var personLookup = allPersons.ToDictionary(p => p.Id, p => p.Name.FullName);
        var membershipsByGroup = allMemberships
            .GroupBy(m => m.GroupId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.PersonId.ToString()).ToList());

        // Build DTOs efficiently
        var result = groups.Select(group => new GroupDto(
            group.Id.ToString(),
            group.Name,
            group.CoordinatorId?.ToString(),
            group.CoordinatorId != null ? personLookup.GetValueOrDefault(group.CoordinatorId) : null,
            membershipsByGroup.GetValueOrDefault(group.Id, new List<string>()),
            membershipsByGroup.GetValueOrDefault(group.Id, new List<string>()).Count)).ToList();

        return result;
    }
}
