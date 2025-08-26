using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Membership;

public sealed class AddPersonToGroupCommandHandler : IRequestHandler<AddPersonToGroupCommand>
{
    private readonly IPersonRepository _personRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMembershipService _membershipService;

    public AddPersonToGroupCommandHandler(
        IPersonRepository personRepository,
        IGroupRepository groupRepository,
        IMembershipService membershipService)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task Handle(AddPersonToGroupCommand request, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(request.PersonId);
        var groupId = GroupId.From(request.GroupId);

        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (person == null)
            throw new InvalidOperationException($"Person with ID {request.PersonId} not found");

        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
            throw new InvalidOperationException($"Group with ID {request.GroupId} not found");

        await _membershipService.AddPersonToGroupAsync(personId, groupId, cancellationToken);
    }
}
