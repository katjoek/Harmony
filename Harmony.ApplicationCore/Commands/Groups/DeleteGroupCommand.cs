using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record DeleteGroupCommand(string Id) : ICommand;
