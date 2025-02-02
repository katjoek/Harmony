// Harmony.Application/Features/People/Commands/DeletePerson/DeletePersonCommandHandler.cs
using Harmony.Application.Interfaces;
using Harmony.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.People.Commands.DeletePerson;

internal sealed class DeletePersonCommandHandler(IApplicationDbContext context) : IRequestHandler<DeletePersonCommand>
{
    public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await context.People
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.Id} not found.");
        }

        context.People.Remove(person);
        await context.SaveChangesAsync(cancellationToken);
    }
}