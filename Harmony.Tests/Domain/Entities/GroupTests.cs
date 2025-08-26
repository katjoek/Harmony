using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using Xunit;

namespace Harmony.Tests.Domain.Entities;

public sealed class GroupTests
{
    [Fact]
    public void Create_WithValidName_CreatesGroup()
    {
        // Arrange
        var name = "Jeugdgroep";

        // Act
        var group = Group.Create(name);

        // Assert
        Assert.NotNull(group);
        Assert.NotEqual(default, group.Id);
        Assert.Equal(name, group.Name);
        Assert.Null(group.CoordinatorId);
        Assert.Empty(group.MemberIds);
        Assert.Equal(0, group.MemberCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Group.Create(name!));
        Assert.StartsWith("Group name is required", exception.Message);
    }

    [Fact]
    public void AddMember_WithValidPersonId_AddsMember()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();

        // Act
        group.AddMember(personId);

        // Assert
        Assert.Contains(personId, group.MemberIds);
        Assert.True(group.HasMember(personId));
        Assert.Equal(1, group.MemberCount);
    }

    [Fact]
    public void AddMember_WithSamePersonTwice_AddsOnlyOnce()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();

        // Act
        group.AddMember(personId);
        group.AddMember(personId);

        // Assert
        Assert.Single(group.MemberIds);
        Assert.Contains(personId, group.MemberIds);
        Assert.Equal(1, group.MemberCount);
    }

    [Fact]
    public void RemoveMember_WithExistingMember_RemovesMember()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();
        group.AddMember(personId);

        // Act
        group.RemoveMember(personId);

        // Assert
        Assert.DoesNotContain(personId, group.MemberIds);
        Assert.False(group.HasMember(personId));
        Assert.Equal(0, group.MemberCount);
    }

    [Fact]
    public void RemoveMember_WhoIsCoordinator_RemovesMemberAndClearsCoordinator()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();
        group.AddMember(personId);
        group.SetCoordinator(personId);

        // Act
        group.RemoveMember(personId);

        // Assert
        Assert.DoesNotContain(personId, group.MemberIds);
        Assert.Null(group.CoordinatorId);
    }

    [Fact]
    public void SetCoordinator_WithValidPersonId_SetsCoordinator()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();

        // Act
        group.SetCoordinator(personId);

        // Assert
        Assert.Equal(personId, group.CoordinatorId);
    }

    [Fact]
    public void SetCoordinator_WithNull_ClearsCoordinator()
    {
        // Arrange
        var group = Group.Create("Testgroep");
        var personId = PersonId.New();
        group.SetCoordinator(personId);

        // Act
        group.SetCoordinator(null);

        // Assert
        Assert.Null(group.CoordinatorId);
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesName()
    {
        // Arrange
        var group = Group.Create("Oude naam");
        var newName = "Nieuwe naam";

        // Act
        group.UpdateName(newName);

        // Assert
        Assert.Equal(newName, group.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var group = Group.Create("Testgroep");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => group.UpdateName(name!));
        Assert.StartsWith("Group name is required", exception.Message);
    }
}
