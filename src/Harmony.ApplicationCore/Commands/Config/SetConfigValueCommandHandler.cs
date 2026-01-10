using Harmony.ApplicationCore.Interfaces;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Config;

public sealed class SetConfigValueCommandHandler : ICommandHandler<SetConfigValueCommand>
{
    private readonly IConfigRepository _configRepository;

    public SetConfigValueCommandHandler(IConfigRepository configRepository)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
    }

    public async Task HandleAsync(SetConfigValueCommand command, CancellationToken cancellationToken)
    {
        await _configRepository.SetValueAsync(command.Key, command.Value, cancellationToken);
    }
}
