using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Harmony.Import.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Harmony.Import.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IServiceProvider _serviceProvider;
    private string _personsSheetFilePath = string.Empty;
    private string _groupsAndCoordinatorsSheetFilePath = string.Empty;
    private string _logText = string.Empty;
    private string _statusText = "Klaar";
    private bool _canStartImport = true;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        BrowsePersonsSheetCommand = new RelayCommand(_ => BrowseFile(true));
        BrowseGroupsAndCoordinatorsSheetCommand = new RelayCommand(_ => BrowseFile(false));
        StartImportCommand = new RelayCommand(async _ => await StartImportAsync(), _ => CanStartImport);
    }

    public string PersonsSheetFilePath
    {
        get => _personsSheetFilePath;
        set
        {
            if (_personsSheetFilePath != value)
            {
                _personsSheetFilePath = value;
                OnPropertyChanged();
                UpdateCanStartImport();
            }
        }
    }

    public string GroupsAndCoordinatorsSheetFilePath
    {
        get => _groupsAndCoordinatorsSheetFilePath;
        set
        {
            if (_groupsAndCoordinatorsSheetFilePath != value)
            {
                _groupsAndCoordinatorsSheetFilePath = value;
                OnPropertyChanged();
                UpdateCanStartImport();
            }
        }
    }

    public string LogText
    {
        get => _logText;
        set
        {
            if (_logText != value)
            {
                _logText = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanStartImport
    {
        get => _canStartImport;
        set
        {
            if (_canStartImport != value)
            {
                _canStartImport = value;
                OnPropertyChanged();
                ((RelayCommand)StartImportCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand BrowsePersonsSheetCommand { get; }
    public ICommand BrowseGroupsAndCoordinatorsSheetCommand { get; }
    public ICommand StartImportCommand { get; }

    private void BrowseFile(bool isPersonsSheet)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = isPersonsSheet ? "Selecteer Personenbestand CSV" : "Selecteer Groepen & Coördinatorenbestand CSV"
        };

        if (dialog.ShowDialog() == true)
        {
            if (isPersonsSheet)
                PersonsSheetFilePath = dialog.FileName;
            else
                GroupsAndCoordinatorsSheetFilePath = dialog.FileName;
        }
    }

    private void UpdateCanStartImport()
    {
        CanStartImport = !string.IsNullOrWhiteSpace(PersonsSheetFilePath) &&
                        !string.IsNullOrWhiteSpace(GroupsAndCoordinatorsSheetFilePath) &&
                        System.IO.File.Exists(PersonsSheetFilePath) &&
                        System.IO.File.Exists(GroupsAndCoordinatorsSheetFilePath);
    }

    private async Task StartImportAsync()
    {
        CanStartImport = false;
        LogText = string.Empty;
        StatusText = "Import starten...";

        try
        {
            // Run import on background thread to keep UI responsive
            var importService = _serviceProvider.GetRequiredService<IImportService>();
            await Task.Run(async () =>
            {
                await importService.ImportAsync(PersonsSheetFilePath, GroupsAndCoordinatorsSheetFilePath, LogMessage).ConfigureAwait(false);
            }).ConfigureAwait(true); // Return to UI thread for final status update
            
            StatusText = "Import succesvol voltooid!";
        }
        catch (Exception ex)
        {
            LogMessage($"FOUT: {ex.Message}");
            LogMessage($"Stack trace: {ex.StackTrace}");
            StatusText = "Import mislukt. Controleer het log voor details.";
        }
        finally
        {
            CanStartImport = true;
        }
    }

    private void LogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logEntry = $"[{timestamp}] {message}\n";
        
        // Ensure UI updates happen on the UI thread using BeginInvoke for non-blocking updates
        if (Application.Current.Dispatcher.CheckAccess())
        {
            LogText += logEntry;
        }
        else
        {
            // Use BeginInvoke instead of Invoke to avoid blocking the background thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LogText += logEntry;
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
