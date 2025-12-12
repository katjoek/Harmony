using Harmony.Import.Models;

namespace Harmony.Import.Services;

public interface ICsvParserService
{
    IReadOnlyList<PersonData> ParseSheet1(string filePath);
    IReadOnlyList<GroupDefinition> ParseSheet2(string filePath);
}
