using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Membership;

public sealed class RemovePersonFromGroupCommandHandler : ICommandHandler<RemovePersonFromGroupCommand>
{
    private readonly IPersonRepository _personRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMembershipService _membershipService;

    public RemovePersonFromGroupCommandHandler(
        IPersonRepository personRepository,
        IGroupRepository groupRepository,
        IMembershipService membershipService)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task HandleAsync(RemovePersonFromGroupCommand command, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(command.PersonId);
        var groupId = GroupId.From(command.GroupId);

        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (person == null)
            throw new InvalidOperationException($"Person with ID {command.PersonId} not found");

        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
            throw new InvalidOperationException($"Group with ID {command.GroupId} not found");

        await _membershipService.RemovePersonFromGroupAsync(personId, groupId, cancellationToken);
    }
}
