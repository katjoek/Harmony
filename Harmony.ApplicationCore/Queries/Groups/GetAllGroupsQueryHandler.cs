using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed class GetAllGroupsQueryHandler : IRequestHandler<GetAllGroupsQuery, IReadOnlyList<GroupDto>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;

    public GetAllGroupsQueryHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
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

            result.Add(new GroupDto(
                group.Id.ToString(),
                group.Name,
                group.CoordinatorId?.ToString(),
                coordinatorName,
                group.MemberIds.Select(m => m.ToString()).ToList(),
                group.MemberCount));
        }

        return result;
    }
}
