using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed class CreateGroupCommandHandler : ICommandHandler<CreateGroupCommand, string>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public CreateGroupCommandHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository,
        IMembershipService membershipService)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task<string> HandleAsync(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        // Check if name is unique
        var isNameUnique = await _groupRepository.IsNameUniqueAsync(command.Name, null, cancellationToken);
        if (!isNameUnique)
            throw new InvalidOperationException($"A group with the name '{command.Name}' already exists");

        var group = Group.Create(command.Name);

        if (!string.IsNullOrWhiteSpace(command.CoordinatorId))
        {
            var coordinatorId = PersonId.From(command.CoordinatorId);
            var coordinatorExists = await _personRepository.ExistsAsync(coordinatorId, cancellationToken);
            
            if (!coordinatorExists)
                throw new InvalidOperationException($"Coordinator with ID {command.CoordinatorId} not found");

            group.SetCoordinator(coordinatorId);
        }

        await _groupRepository.AddAsync(group, cancellationToken);

        return group.Id.ToString();
    }
}
