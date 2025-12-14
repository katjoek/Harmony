using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class DeletePersonCommandHandler : ICommandHandler<DeletePersonCommand>
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

    public async Task HandleAsync(DeletePersonCommand command, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(command.Id);
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        
        if (person == null)
            throw new InvalidOperationException($"Person with ID {command.Id} not found");

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
