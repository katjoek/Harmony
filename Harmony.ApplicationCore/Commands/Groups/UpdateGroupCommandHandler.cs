using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public UpdateGroupCommandHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository,
        IMembershipService membershipService)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var groupId = GroupId.From(request.Id);
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        
        if (group == null)
            throw new InvalidOperationException($"Group with ID {request.Id} not found");

        // Check if name is unique (excluding current group)
        var isNameUnique = await _groupRepository.IsNameUniqueAsync(request.Name, groupId, cancellationToken);
        if (!isNameUnique)
            throw new InvalidOperationException($"A group with the name '{request.Name}' already exists");

        group.UpdateName(request.Name);

        if (!string.IsNullOrWhiteSpace(request.CoordinatorId))
        {
            var coordinatorId = PersonId.From(request.CoordinatorId);
            var coordinatorExists = await _personRepository.ExistsAsync(coordinatorId, cancellationToken);
            
            if (!coordinatorExists)
                throw new InvalidOperationException($"Coordinator with ID {request.CoordinatorId} not found");

            // Enforce domain rule: coordinator must be a current member of the group
            var memberIds = await _membershipService.GetPersonIdsForGroupAsync(groupId, cancellationToken);
            var isMember = memberIds.Contains(coordinatorId);
            if (!isMember)
                throw new InvalidOperationException("Coördinator moet een groepslid zijn voordat deze kan worden ingesteld.");

            group.SetCoordinator(coordinatorId);
        }
        else
        {
            group.SetCoordinator(null);
        }

        await _groupRepository.UpdateAsync(group, cancellationToken);
    }
}
