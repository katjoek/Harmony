using Harmony.ApplicationCore.Commands.Groups;
using Harmony.ApplicationCore.Commands.Membership;
using Harmony.ApplicationCore.Commands.Persons;
using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Queries.Groups;
using Harmony.ApplicationCore.Queries.Persons;
using Harmony.Import.Models;
using MediatR;

namespace Harmony.Import.Services;

public sealed class ImportService : IImportService
{
    private readonly IMediator _mediator;
    private readonly ICsvParserService _csvParser;
    private readonly IDatabaseBackupService _databaseBackup;

    public ImportService(
        IMediator mediator,
        ICsvParserService csvParser,
        IDatabaseBackupService databaseBackup)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        _databaseBackup = databaseBackup ?? throw new ArgumentNullException(nameof(databaseBackup));
    }

    public async Task ImportAsync(string sheet1Path, string sheet2Path, Action<string> logCallback)
    {
        logCallback("Starting import process...");

        // Step 1: Backup database
        logCallback("Backing up existing database...");
        var backupSuccess = await _databaseBackup.BackupDatabaseAsync();
        if (!backupSuccess)
        {
            logCallback("Import cancelled by user.");
            return;
        }
        logCallback("Database backup completed.");

        // Step 2: Parse CSV files
        logCallback("Parsing CSV files...");
        IReadOnlyList<GroupDefinition> groups;
        IReadOnlyList<PersonData> persons;

        try
        {
            groups = _csvParser.ParseSheet2(sheet2Path);
            logCallback($"Found {groups.Count} groups in Sheet 2.");
            
            persons = _csvParser.ParseSheet1(sheet1Path);
            logCallback($"Found {persons.Count} persons in Sheet 1.");
        }
        catch (Exception ex)
        {
            logCallback($"ERROR parsing CSV files: {ex.Message}");
            throw;
        }

        // Step 3: Create groups (without coordinators first)
        logCallback("Creating groups...");
        var groupCodeToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var group in groups)
        {
            try
            {
                var command = new CreateGroupCommand(group.Name, null);
                var groupId = await _mediator.Send(command);
                groupCodeToIdMap[group.Code] = groupId;
                logCallback($"Created group: {group.Name} (Code: {group.Code})");
            }
            catch (Exception ex)
            {
                logCallback($"ERROR creating group {group.Name}: {ex.Message}");
                // Continue with other groups
            }
        }

        // Step 4: Create persons
        logCallback("Creating persons...");
        var personEmailToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var personNameToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var personData in persons)
        {
            try
            {
                var command = new CreatePersonCommand(
                    personData.FirstName,
                    personData.Prefix,
                    personData.Surname,
                    personData.DateOfBirth,
                    personData.Street,
                    personData.ZipCode,
                    personData.City,
                    personData.PhoneNumber,
                    personData.EmailAddress);

                var personId = await _mediator.Send(command);
                
                var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
                personNameToIdMap[fullName] = personId;
                
                if (!string.IsNullOrWhiteSpace(personData.EmailAddress))
                {
                    personEmailToIdMap[personData.EmailAddress] = personId;
                }

                logCallback($"Created person: {fullName}");
            }
            catch (Exception ex)
            {
                var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
                logCallback($"ERROR creating person {fullName}: {ex.Message}");
                // Continue with other persons
            }
        }

        // Step 5: Create memberships
        logCallback("Creating group memberships...");
        foreach (var personData in persons)
        {
            var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
            
            if (!personNameToIdMap.TryGetValue(fullName, out var personId))
            {
                logCallback($"WARNING: Person '{fullName}' not found when creating memberships");
                continue;
            }

            foreach (var groupCode in personData.GroupCodes)
            {
                try
                {
                    if (!groupCodeToIdMap.TryGetValue(groupCode, out var groupId))
                    {
                        logCallback($"WARNING: Group code '{groupCode}' not found for person '{fullName}'");
                        continue;
                    }

                    var membershipCommand = new AddPersonToGroupCommand(personId, groupId);
                    await _mediator.Send(membershipCommand);
                    logCallback($"Added '{fullName}' to group '{groupCode}'");
                }
                catch (Exception ex)
                {
                    logCallback($"ERROR adding '{fullName}' to group '{groupCode}': {ex.Message}");
                    // Continue with other memberships
                }
            }
        }

        // Step 6: Ensure coordinators are members, then set them as coordinators
        logCallback("Setting coordinators for groups...");
        foreach (var group in groups)
        {
            if (string.IsNullOrWhiteSpace(group.CoordinatorName))
                continue;

            try
            {
                // Try to find coordinator by name (case-insensitive)
                var coordinatorId = FindPersonByName(group.CoordinatorName, personNameToIdMap);
                
                if (coordinatorId == null)
                {
                    logCallback($"WARNING: Coordinator '{group.CoordinatorName}' not found for group {group.Name}");
                    continue;
                }

                if (!groupCodeToIdMap.TryGetValue(group.Code, out var groupId))
                {
                    logCallback($"WARNING: Group code '{group.Code}' not found when setting coordinator");
                    continue;
                }

                // Ensure coordinator is a member of the group
                try
                {
                    var membershipCommand = new AddPersonToGroupCommand(coordinatorId, groupId);
                    await _mediator.Send(membershipCommand);
                    logCallback($"Ensured coordinator '{group.CoordinatorName}' is a member of group: {group.Name}");
                }
                catch
                {
                    // Already a member, that's fine
                }

                // Update group to set coordinator
                var updateCommand = new UpdateGroupCommand(groupId, group.Name, coordinatorId);
                await _mediator.Send(updateCommand);
                logCallback($"Set coordinator '{group.CoordinatorName}' for group: {group.Name}");
            }
            catch (Exception ex)
            {
                logCallback($"ERROR setting coordinator for group {group.Name}: {ex.Message}");
                // Continue with other groups
            }
        }

        logCallback("Import completed successfully!");
    }

    private static string GetFullName(string firstName, string? prefix, string? surname)
    {
        var parts = new[] { firstName, prefix, surname }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());
        return string.Join(" ", parts);
    }

    private static string? FindPersonByName(string coordinatorName, Dictionary<string, string> personNameToIdMap)
    {
        // Try exact match first (case-insensitive)
        var normalizedName = coordinatorName.Trim();
        if (personNameToIdMap.TryGetValue(normalizedName, out var id))
            return id;

        // Try case-insensitive match
        var match = personNameToIdMap.FirstOrDefault(
            kvp => string.Equals(kvp.Key, normalizedName, StringComparison.OrdinalIgnoreCase));
        
        return match.Key != null ? match.Value : null;
    }
}
