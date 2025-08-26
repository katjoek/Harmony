using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand>
{
    private readonly IPersonRepository _personRepository;
    private readonly IGroupRepository _groupRepository;

    public DeletePersonCommandHandler(
        IPersonRepository personRepository,
        IGroupRepository groupRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
    }

    public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(request.Id);
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        
        if (person == null)
            throw new InvalidOperationException($"Person with ID {request.Id} not found");

        // Remove person from all groups (cascading delete)
        var groups = await _groupRepository.GetByPersonIdAsync(personId, cancellationToken);
        foreach (var group in groups)
        {
            group.RemoveMember(personId);
            await _groupRepository.UpdateAsync(group, cancellationToken);
        }

        await _personRepository.DeleteAsync(personId, cancellationToken);
    }
}
