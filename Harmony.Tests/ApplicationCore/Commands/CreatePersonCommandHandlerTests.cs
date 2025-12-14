using Harmony.ApplicationCore.Commands.Persons;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using NSubstitute;
using Xunit;

namespace Harmony.Tests.ApplicationCore.Commands;

public sealed class CreatePersonCommandHandlerTests
{
    private readonly IPersonRepository _personRepository;
    private readonly CreatePersonCommandHandler _handler;

    public CreatePersonCommandHandlerTests()
    {
        _personRepository = Substitute.For<IPersonRepository>();
        _handler = new CreatePersonCommandHandler(_personRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesPerson()
    {
        // Arrange
        var command = new CreatePersonCommand(
            "Jan",
            "van",
            "Berg",
            new DateOnly(1990, 5, 15),
            "Hoofdstraat 1",
            "1234AB",
            "Amsterdam",
            "06-12345678",
            "jan@example.com");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        await _personRepository.Received(1).AddAsync(
            Arg.Is<Person>(p => 
                p.Name.FirstName == "Jan" &&
                p.Name.Prefix == "van" &&
                p.Name.Surname == "Berg" &&
                p.DateOfBirth == new DateOnly(1990, 5, 15) &&
                p.Address!.Street == "Hoofdstraat 1" &&
                p.Address.ZipCode == "1234AB" &&
                p.Address.City == "Amsterdam" &&
                p.PhoneNumber!.Value == "06-12345678" &&
                p.EmailAddress!.Value == "jan@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMinimalCommand_CreatesPersonWithRequiredFieldsOnly()
    {
        // Arrange
        var command = new CreatePersonCommand(
            "Jan",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        await _personRepository.Received(1).AddAsync(
            Arg.Is<Person>(p => 
                p.Name.FirstName == "Jan" &&
                p.Name.Prefix == null &&
                p.Name.Surname == null &&
                p.DateOfBirth == null &&
                p.Address == null &&
                p.PhoneNumber == null &&
                p.EmailAddress == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreatePersonCommand(
            "Jan",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "invalid-email");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidPhoneNumber_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreatePersonCommand(
            "Jan",
            null,
            null,
            null,
            null,
            null,
            null,
            "123", // Too short
            null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _handler.HandleAsync(command, CancellationToken.None));
    }
}
