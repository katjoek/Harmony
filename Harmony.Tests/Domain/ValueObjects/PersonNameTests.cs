using Harmony.Domain.ValueObjects;
using Xunit;

namespace Harmony.Tests.Domain.ValueObjects;

public sealed class PersonNameTests
{
    [Fact]
    public void Constructor_WithValidFirstName_CreatesPersonName()
    {
        // Arrange
        var firstName = "Jan";

        // Act
        var personName = new PersonName(firstName);

        // Assert
        Assert.Equal(firstName, personName.FirstName);
        Assert.Null(personName.Prefix);
        Assert.Null(personName.Surname);
        Assert.Equal(firstName, personName.FullName);
    }

    [Fact]
    public void Constructor_WithAllComponents_CreatesPersonName()
    {
        // Arrange
        var firstName = "Jan";
        var prefix = "van";
        var surname = "Berg";

        // Act
        var personName = new PersonName(firstName, prefix, surname);

        // Assert
        Assert.Equal(firstName, personName.FirstName);
        Assert.Equal(prefix, personName.Prefix);
        Assert.Equal(surname, personName.Surname);
        Assert.Equal("Jan van Berg", personName.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidFirstName_ThrowsArgumentException(string? firstName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PersonName(firstName!));
        Assert.Equal("First name is required", exception.Message);
    }

    [Fact]
    public void FullName_WithPrefixButNoSurname_ReturnsCorrectFormat()
    {
        // Arrange
        var personName = new PersonName("Jan", "van");

        // Act
        var fullName = personName.FullName;

        // Assert
        Assert.Equal("Jan van", fullName);
    }

    [Fact]
    public void FullName_WithSurnameButNoPrefix_ReturnsCorrectFormat()
    {
        // Arrange
        var personName = new PersonName("Jan", null, "Berg");

        // Act
        var fullName = personName.FullName;

        // Assert
        Assert.Equal("Jan Berg", fullName);
    }

    [Fact]
    public void Constructor_TrimsWhitespace_FromAllComponents()
    {
        // Arrange & Act
        var personName = new PersonName("  Jan  ", "  van  ", "  Berg  ");

        // Assert
        Assert.Equal("Jan", personName.FirstName);
        Assert.Equal("van", personName.Prefix);
        Assert.Equal("Berg", personName.Surname);
    }
}
