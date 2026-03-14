namespace Harmony.Web.Services;

using System.Globalization;

public sealed class ReportFileNameBuilder(TimeProvider timeProvider) : IReportFileNameBuilder
{
    public string Build(string reportType, string? groupName, string? birthdayGroupName, int month)
    {
        var ts = timeProvider.GetLocalNow().ToString("yyyyMMdd_HHmmss");
        if (reportType == "Group")
        {
            var adaptedGroupName = (groupName ?? "").Replace(" ", "_");
            return $"{adaptedGroupName}_{ts}";
        }
        // Birthday
        var culture = new CultureInfo("nl-NL");
        var monthName = culture.DateTimeFormat.GetMonthName(month).Replace(" ", "_");
        monthName = char.ToUpper(monthName[0]) + monthName[1..];
        if (birthdayGroupName is not null)
        {
            var gn = birthdayGroupName.Replace(" ", "_");
            return $"Verjaardagen_{gn}_{monthName}_{ts}";
        }
        return $"Verjaardagen_{monthName}_{ts}";
    }
}
