using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Config;

public sealed record GetConfigValueQuery(string Key) : IQuery<string?>;
