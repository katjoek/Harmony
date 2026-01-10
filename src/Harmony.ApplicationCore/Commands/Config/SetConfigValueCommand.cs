using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Config;

public sealed record SetConfigValueCommand(string Key, string? Value) : ICommand;
