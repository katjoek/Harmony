namespace Harmony.Import.Services;

public interface IImportService
{
    Task ImportAsync(string sheet1Path, string sheet2Path, Action<string> logCallback);
}
