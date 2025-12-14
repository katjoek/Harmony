using Harmony.ApplicationCore.DTOs;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed record GetGroupByIdQuery(string Id) : IQuery<GroupDto?>;
