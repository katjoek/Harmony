using Harmony.Import.Models;

namespace Harmony.Import.Services;

public interface ICsvParserService
{
    IReadOnlyList<PersonData> ParsePersonsSheet(string filePath, IReadOnlyDictionary<string, string> abbreviationToGroupNameMap);
    IReadOnlyList<GroupDefinition> ParseGroupsAndCoordinatorsSheet(string filePath);
}
