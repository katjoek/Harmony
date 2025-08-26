using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPersonRepository _personRepository;

    public DeleteGroupCommandHandler(
        IGroupRepository groupRepository,
        IPersonRepository personRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var groupId = GroupId.From(request.Id);
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        
        if (group == null)
            throw new InvalidOperationException($"Group with ID {request.Id} not found");

        // Remove group reference from all persons (non-cascading delete)
        var members = await _personRepository.GetByGroupIdAsync(groupId, cancellationToken);
        foreach (var member in members)
        {
            member.RemoveFromGroup(groupId);
            await _personRepository.UpdateAsync(member, cancellationToken);
        }

        await _groupRepository.DeleteAsync(groupId, cancellationToken);
    }
}
