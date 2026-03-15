namespace Harmony.Web.Services;

using Harmony.ApplicationCore.Interfaces;

public interface IUpdateNotificationState
{
    ReleaseInfo? PendingUpdate { get; }
    DateTime? LastChecked { get; }
    event Action? StateChanged;
    void SetUpdate(ReleaseInfo? update, DateTime checkedAt);
}

public sealed class UpdateNotificationState : IUpdateNotificationState
{
    public ReleaseInfo? PendingUpdate { get; private set; }
    public DateTime? LastChecked { get; private set; }

    public event Action? StateChanged;

    public void SetUpdate(ReleaseInfo? update, DateTime checkedAt)
    {
        PendingUpdate = update;
        LastChecked = checkedAt;
        StateChanged?.Invoke();
    }
}
