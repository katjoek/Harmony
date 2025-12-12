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
    private string _sheet1FilePath = string.Empty;
    private string _sheet2FilePath = string.Empty;
    private string _logText = string.Empty;
    private string _statusText = "Ready";
    private bool _canStartImport = true;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        BrowseSheet1Command = new RelayCommand(_ => BrowseFile(true));
        BrowseSheet2Command = new RelayCommand(_ => BrowseFile(false));
        StartImportCommand = new RelayCommand(async _ => await StartImportAsync(), _ => CanStartImport);
    }

    public string Sheet1FilePath
    {
        get => _sheet1FilePath;
        set
        {
            if (_sheet1FilePath != value)
            {
                _sheet1FilePath = value;
                OnPropertyChanged();
                UpdateCanStartImport();
            }
        }
    }

    public string Sheet2FilePath
    {
        get => _sheet2FilePath;
        set
        {
            if (_sheet2FilePath != value)
            {
                _sheet2FilePath = value;
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

    public ICommand BrowseSheet1Command { get; }
    public ICommand BrowseSheet2Command { get; }
    public ICommand StartImportCommand { get; }

    private void BrowseFile(bool isSheet1)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = isSheet1 ? "Select Sheet 1 CSV file (Personen)" : "Select Sheet 2 CSV file (Groepen)"
        };

        if (dialog.ShowDialog() == true)
        {
            if (isSheet1)
                Sheet1FilePath = dialog.FileName;
            else
                Sheet2FilePath = dialog.FileName;
        }
    }

    private void UpdateCanStartImport()
    {
        CanStartImport = !string.IsNullOrWhiteSpace(Sheet1FilePath) &&
                        !string.IsNullOrWhiteSpace(Sheet2FilePath) &&
                        System.IO.File.Exists(Sheet1FilePath) &&
                        System.IO.File.Exists(Sheet2FilePath);
    }

    private async Task StartImportAsync()
    {
        CanStartImport = false;
        LogText = string.Empty;
        StatusText = "Starting import...";

        try
        {
            var importService = _serviceProvider.GetRequiredService<IImportService>();
            await importService.ImportAsync(Sheet1FilePath, Sheet2FilePath, LogMessage);
            StatusText = "Import completed successfully!";
        }
        catch (Exception ex)
        {
            LogMessage($"ERROR: {ex.Message}");
            LogMessage($"Stack trace: {ex.StackTrace}");
            StatusText = "Import failed. Check log for details.";
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
        
        // Ensure UI updates happen on the UI thread
        if (Application.Current.Dispatcher.CheckAccess())
        {
            LogText += logEntry;
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogText += logEntry;
            });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
