using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed record DeletePersonCommand(string Id) : ICommand;
