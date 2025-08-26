using Harmony.ApplicationCore.DTOs;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed record GetAllGroupsQuery : IRequest<IReadOnlyList<GroupDto>>;
