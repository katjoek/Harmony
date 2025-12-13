namespace Harmony.Import.Services;

public interface IImportService
{
    Task ImportAsync(string personsSheetPath, string groupsAndCoordinatorsSheetPath, Action<string> logCallback);
}
