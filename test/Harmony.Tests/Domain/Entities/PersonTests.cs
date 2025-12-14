using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using Xunit;

namespace Harmony.Tests.Domain.Entities;

public sealed class PersonTests
{
    [Fact]
    public void Create_WithValidName_CreatesPerson()
    {
        // Arrange
        var name = new PersonName("Jan", "van", "Berg");

        // Act
        var person = Person.Create(name);

        // Assert
        Assert.NotNull(person);
        Assert.NotEqual(default, person.Id);
        Assert.Equal(name, person.Name);
        Assert.Null(person.DateOfBirth);
        Assert.Null(person.Address);
        Assert.Null(person.PhoneNumber);
        Assert.Null(person.EmailAddress);
        Assert.Empty(person.GroupIds);
    }

    [Fact]
    public void AddToGroup_WithValidGroupId_AddsGroup()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var groupId = GroupId.New();

        // Act
        person.AddToGroup(groupId);

        // Assert
        Assert.Contains(groupId, person.GroupIds);
        Assert.True(person.IsMemberOf(groupId));
    }

    [Fact]
    public void AddToGroup_WithSameGroupTwice_AddsOnlyOnce()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var groupId = GroupId.New();

        // Act
        person.AddToGroup(groupId);
        person.AddToGroup(groupId);

        // Assert
        Assert.Single(person.GroupIds);
        Assert.Contains(groupId, person.GroupIds);
    }

    [Fact]
    public void RemoveFromGroup_WithExistingGroup_RemovesGroup()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var groupId = GroupId.New();
        person.AddToGroup(groupId);

        // Act
        person.RemoveFromGroup(groupId);

        // Assert
        Assert.DoesNotContain(groupId, person.GroupIds);
        Assert.False(person.IsMemberOf(groupId));
    }

    [Fact]
    public void RemoveFromGroup_WithNonExistingGroup_DoesNotThrow()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var groupId = GroupId.New();

        // Act & Assert
        person.RemoveFromGroup(groupId); // Should not throw
        Assert.Empty(person.GroupIds);
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesName()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var newName = new PersonName("Piet", "de", "Vries");

        // Act
        person.UpdateName(newName);

        // Assert
        Assert.Equal(newName, person.Name);
    }

    [Fact]
    public void UpdateName_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => person.UpdateName(null!));
    }

    [Fact]
    public void UpdateDateOfBirth_WithValidDate_UpdatesDate()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var dateOfBirth = new DateOnly(1990, 5, 15);

        // Act
        person.UpdateDateOfBirth(dateOfBirth);

        // Assert
        Assert.Equal(dateOfBirth, person.DateOfBirth);
    }

    [Fact]
    public void UpdateEmailAddress_WithValidEmail_UpdatesEmail()
    {
        // Arrange
        var person = Person.Create(new PersonName("Jan"));
        var email = new EmailAddress("jan@example.com");

        // Act
        person.UpdateEmailAddress(email);

        // Assert
        Assert.Equal(email, person.EmailAddress);
    }
}
