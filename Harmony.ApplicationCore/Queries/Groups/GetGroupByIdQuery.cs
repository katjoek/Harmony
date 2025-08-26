using Harmony.ApplicationCore.DTOs;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Groups;

public sealed record GetGroupByIdQuery(string Id) : IRequest<GroupDto?>;
