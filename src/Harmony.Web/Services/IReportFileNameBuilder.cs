namespace Harmony.Web.Services;

public interface IReportFileNameBuilder
{
    string Build(string reportType, string? groupName, string? birthdayGroupName, int month);
}
