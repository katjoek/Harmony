using System.IO;
using System.Windows;

namespace Harmony.Import.Services;

public sealed class DatabaseBackupService : IDatabaseBackupService
{
    private readonly string _databasePath;

    public DatabaseBackupService(string databasePath)
    {
        _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
    }

    public async Task<bool> BackupDatabaseAsync()
    {
        if (!File.Exists(_databasePath))
        {
            return true; // No database to backup
        }

        // Ask user for confirmation
        var result = MessageBox.Show(
            "De huidige database zal worden hernoemd naar .old. Weet u zeker dat u wilt doorgaan?",
            "Bevestig database backup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            return false;
        }

        try
        {
            var directory = Path.GetDirectoryName(_databasePath);
            var fileName = Path.GetFileName(_databasePath);
            var baseBackupPath = Path.Combine(directory ?? string.Empty, fileName + ".old");
            
            // Find the next available backup filename
            var backupPath = GetNextAvailableBackupPath(baseBackupPath);

            // Rename current database to backup path
            File.Move(_databasePath, backupPath);
            
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fout bij het maken van backup: {ex.Message}",
                "Backup fout",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }

    private static string GetNextAvailableBackupPath(string baseBackupPath)
    {
        // Check if base backup path (without number) exists
        if (!File.Exists(baseBackupPath))
        {
            return baseBackupPath;
        }

        // Find all existing numbered backups
        var directory = Path.GetDirectoryName(baseBackupPath) ?? string.Empty;
        var baseFileName = Path.GetFileName(baseBackupPath);
        var pattern = baseFileName + ".*";
        
        var existingBackups = Directory.GetFiles(directory, pattern)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Select(f => f!)
            .ToList();

        // Extract numbers from existing backups (e.g., "harmony.db.old.1" -> 1)
        var numbers = new List<int>();
        
        // Explicitly add 0 for the base backup file since it exists (we checked above)
        // The pattern "harmony.db.old.*" doesn't match "harmony.db.old" itself
        numbers.Add(0);
        
        foreach (var backup in existingBackups)
        {
            if (backup.StartsWith(baseFileName + ".", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the number after the base filename
                var numberPart = backup.Substring(baseFileName.Length + 1);
                if (int.TryParse(numberPart, out var number))
                {
                    numbers.Add(number);
                }
            }
        }

        // Find the next available number
        var nextNumber = numbers.Count > 0 ? numbers.Max() + 1 : 1;
        return baseBackupPath + "." + nextNumber;
    }
}
