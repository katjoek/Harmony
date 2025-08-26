using Harmony.ApplicationCore.DTOs;
using Harmony.Web.Models;

namespace Harmony.Web.Services;

public interface IReportService
{
    Task<byte[]> GeneratePdfReportAsync(GroupDto group, List<PersonDto> members, ReportModel config);
    Task<byte[]> GenerateExcelReportAsync(GroupDto group, List<PersonDto> members, ReportModel config);
}
