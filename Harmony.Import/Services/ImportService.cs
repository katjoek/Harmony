using Harmony.ApplicationCore.Commands.Groups;
using Harmony.ApplicationCore.Commands.Membership;
using Harmony.ApplicationCore.Commands.Persons;
using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Queries.Groups;
using Harmony.ApplicationCore.Queries.Persons;
using Harmony.Import.Models;
using Harmony.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Import.Services;

public sealed class ImportService : IImportService
{
    private readonly IMediator _mediator;
    private readonly ICsvParserService _csvParser;
    private readonly IDatabaseBackupService _databaseBackup;
    private readonly IServiceProvider _serviceProvider;

    public ImportService(
        IMediator mediator,
        ICsvParserService csvParser,
        IDatabaseBackupService databaseBackup,
        IServiceProvider serviceProvider)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        _databaseBackup = databaseBackup ?? throw new ArgumentNullException(nameof(databaseBackup));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task ImportAsync(string personsSheetPath, string groupsAndCoordinatorsSheetPath, Action<string> logCallback)
    {
        logCallback("Importproces starten...");

        // Step 1: Backup database
        logCallback("Database back-uppen...");
        var backupSuccess = await _databaseBackup.BackupDatabaseAsync();
        if (!backupSuccess)
        {
            logCallback("Import geannuleerd door gebruiker.");
            return;
        }
        logCallback("Database back-up voltooid.");

        // Step 1.5: Initialize fresh database (after backup, file is no longer locked)
        logCallback("Database initialiseren...");
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
            dbContext.Database.EnsureCreated();
        }
        logCallback("Database geïnitialiseerd.");

        // Step 2: Parse CSV files
        logCallback("CSV-bestanden inlezen...");
        IReadOnlyList<GroupDefinition> groups;
        IReadOnlyList<PersonData> persons;

        try
        {
            groups = _csvParser.ParseGroupsAndCoordinatorsSheet(groupsAndCoordinatorsSheetPath);
            logCallback($"{groups.Count} groepen gevonden in het Groepen & Coördinatorenbestand.");
            
            // Create mapping of abbreviation to group name from Groups & Coordinators sheet
            var abbreviationToGroupNameMap = groups.ToDictionary(
                g => g.Code, 
                g => g.Name, 
                StringComparer.OrdinalIgnoreCase);
            
            persons = _csvParser.ParsePersonsSheet(personsSheetPath, abbreviationToGroupNameMap);
            logCallback($"{persons.Count} personen gevonden in het Personenbestand.");
        }
        catch (Exception ex)
        {
            logCallback($"FOUT bij het inlezen van CSV-bestanden: {ex.Message}");
            throw;
        }

        // Step 3: Create groups (without coordinators first)
        logCallback("Groepen aanmaken...");
        var groupCodeToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var groupNameToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var group in groups)
        {
            try
            {
                var command = new CreateGroupCommand(group.Name, null);
                var groupId = await _mediator.Send(command).ConfigureAwait(false);
                groupCodeToIdMap[group.Code] = groupId;
                groupNameToIdMap[group.Name] = groupId;
                logCallback($"Groep aangemaakt: {group.Name} (Code: {group.Code})");
                
                // Yield to allow UI updates
                await Task.Yield();
            }
            catch (Exception ex)
            {
                logCallback($"FOUT bij aanmaken groep {group.Name}: {ex.Message}");
                // Continue with other groups
            }
        }

        // Step 4: Create persons
        logCallback("Personen aanmaken...");
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

                var personId = await _mediator.Send(command).ConfigureAwait(false);
                
                var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
                personNameToIdMap[fullName] = personId;
                
                if (!string.IsNullOrWhiteSpace(personData.EmailAddress))
                {
                    personEmailToIdMap[personData.EmailAddress] = personId;
                }

                logCallback($"Persoon aangemaakt: {fullName}");
                
                // Yield periodically to allow UI updates (every 10 persons)
                if (personNameToIdMap.Count % 10 == 0)
                {
                    await Task.Yield();
                }
            }
            catch (Exception ex)
            {
                var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
                logCallback($"FOUT bij aanmaken persoon {fullName}: {ex.Message}");
                // Continue with other persons
            }
        }

        // Step 5: Create memberships
        logCallback("Groepslidmaatschappen aanmaken...");
        foreach (var personData in persons)
        {
            var fullName = GetFullName(personData.FirstName, personData.Prefix, personData.Surname);
            
            if (!personNameToIdMap.TryGetValue(fullName, out var personId))
            {
                logCallback($"WAARSCHUWING: Persoon '{fullName}' niet gevonden bij aanmaken lidmaatschappen");
                continue;
            }

            foreach (var groupName in personData.GroupCodes)
            {
                try
                {
                    // Match by group name (full name from Persons sheet, not abbreviation)
                    if (!groupNameToIdMap.TryGetValue(groupName, out var groupId))
                    {
                        logCallback($"WAARSCHUWING: Groep '{groupName}' niet gevonden voor persoon '{fullName}'");
                        continue;
                    }

                    var membershipCommand = new AddPersonToGroupCommand(personId, groupId);
                    await _mediator.Send(membershipCommand).ConfigureAwait(false);
                    logCallback($"'{fullName}' toegevoegd aan groep '{groupName}'");
                    
                    // Yield periodically to allow UI updates
                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    logCallback($"FOUT bij toevoegen '{fullName}' aan groep '{groupName}': {ex.Message}");
                    // Continue with other memberships
                }
            }
        }

        // Step 6: Ensure coordinators are members, then set them as coordinators
        logCallback("Coördinatoren instellen voor groepen...");
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
                    logCallback($"WAARSCHUWING: Coördinator '{group.CoordinatorName}' niet gevonden voor groep {group.Name}");
                    continue;
                }

                if (!groupCodeToIdMap.TryGetValue(group.Code, out var groupId))
                {
                    logCallback($"WAARSCHUWING: Groepcode '{group.Code}' niet gevonden bij instellen coördinator");
                    continue;
                }

                // Ensure coordinator is a member of the group
                try
                {
                    var membershipCommand = new AddPersonToGroupCommand(coordinatorId, groupId);
                    await _mediator.Send(membershipCommand).ConfigureAwait(false);
                    logCallback($"Coördinator '{group.CoordinatorName}' is lid van groep: {group.Name}");
                }
                catch
                {
                    // Already a member, that's fine
                }

                // Update group to set coordinator
                var updateCommand = new UpdateGroupCommand(groupId, group.Name, coordinatorId);
                await _mediator.Send(updateCommand).ConfigureAwait(false);
                logCallback($"Coördinator '{group.CoordinatorName}' ingesteld voor groep: {group.Name}");
                
                // Yield to allow UI updates
                await Task.Yield();
            }
            catch (Exception ex)
            {
                logCallback($"FOUT bij instellen coördinator voor groep {group.Name}: {ex.Message}");
                // Continue with other groups
            }
        }

        logCallback("Import succesvol voltooid!");
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
