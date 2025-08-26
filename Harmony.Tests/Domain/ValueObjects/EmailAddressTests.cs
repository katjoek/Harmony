using Harmony.Domain.ValueObjects;
using Xunit;

namespace Harmony.Tests.Domain.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.org")]
    public void Constructor_WithValidEmail_CreatesEmailAddress(string email)
    {
        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal(email.ToLowerInvariant(), emailAddress.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(email!));
        Assert.Equal("Email address cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Constructor_WithMalformedEmail_ThrowsArgumentException(string email)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EmailAddress(email));
        Assert.Equal("Invalid email address format", exception.Message);
    }

    [Fact]
    public void Constructor_ConvertsToLowerCase()
    {
        // Arrange
        var email = "Test@Example.COM";

        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal("test@example.com", emailAddress.Value);
    }

    [Fact]
    public void FromString_WithValidEmail_ReturnsEmailAddress()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var emailAddress = EmailAddress.FromString(email);

        // Assert
        Assert.NotNull(emailAddress);
        Assert.Equal(email, emailAddress.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FromString_WithNullOrEmpty_ReturnsNull(string? email)
    {
        // Act
        var emailAddress = EmailAddress.FromString(email);

        // Assert
        Assert.Null(emailAddress);
    }
}
