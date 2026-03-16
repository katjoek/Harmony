namespace Harmony.Tests.Web.Services;

using Harmony.ApplicationCore.Interfaces;
using Harmony.Web.Services;
using Xunit;

public sealed class UpdateNotificationStateTests
{
    private static readonly ReleaseInfo SomeRelease =
        new(new Version(2, 0, 0), "v2.0.0", IsPreRelease: false, InstallerDownloadUrl: null, InstallerFileName: null);

    [Fact]
    public void SetUpdate_WithRelease_StoresPendingUpdateAndTimestamp()
    {
        // Arrange
        var state = new UpdateNotificationState();
        var checkedAt = new DateTime(2026, 3, 15, 10, 0, 0);

        // Act
        state.SetUpdate(SomeRelease, checkedAt);

        // Assert
        Assert.Equal(SomeRelease, state.PendingUpdate);
        Assert.Equal(checkedAt, state.LastChecked);
    }

    [Fact]
    public void SetUpdate_WithNull_ClearsPendingUpdate()
    {
        // Arrange
        var state = new UpdateNotificationState();
        state.SetUpdate(SomeRelease, DateTime.Now);

        // Act
        state.SetUpdate(null, DateTime.Now);

        // Assert
        Assert.Null(state.PendingUpdate);
    }

    [Fact]
    public void SetUpdate_FiresStateChangedEvent()
    {
        // Arrange
        var state = new UpdateNotificationState();
        var eventFired = 0;
        state.StateChanged += () => eventFired++;

        // Act
        state.SetUpdate(SomeRelease, DateTime.Now);

        // Assert
        Assert.Equal(1, eventFired);
    }

    [Fact]
    public void SetUpdate_WhenNoHandlerRegistered_DoesNotThrow()
    {
        // Arrange
        var state = new UpdateNotificationState();

        // Act & Assert
        var exception = Record.Exception(() => state.SetUpdate(SomeRelease, DateTime.Now));
        Assert.Null(exception);
    }
}
