using Harmony.Import.Models;

namespace Harmony.Import.Services;

public interface ICsvParserService
{
    IReadOnlyList<PersonData> ParseSheet1(string filePath, IReadOnlyDictionary<string, string> abbreviationToGroupNameMap);
    IReadOnlyList<GroupDefinition> ParseSheet2(string filePath);
}
