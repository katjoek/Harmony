using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed record CreatePersonCommand(
    string FirstName,
    string? Prefix,
    string? Surname,
    DateOnly? DateOfBirth,
    string? Street,
    string? ZipCode,
    string? City,
    string? PhoneNumber,
    string? EmailAddress) : ICommand<string>;
