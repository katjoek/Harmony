using Harmony.ApplicationCore.Interfaces;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Config;

public sealed class GetConfigValueQueryHandler : IQueryHandler<GetConfigValueQuery, string?>
{
    private readonly IConfigRepository _configRepository;

    public GetConfigValueQueryHandler(IConfigRepository configRepository)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
    }

    public async Task<string?> HandleAsync(GetConfigValueQuery query, CancellationToken cancellationToken)
    {
        return await _configRepository.GetValueAsync(query.Key, cancellationToken);
    }
}
