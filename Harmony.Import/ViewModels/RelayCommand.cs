using System.Windows.Input;

namespace Harmony.Import.ViewModels;

public sealed class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _asyncExecute;
    private readonly Action<object?>? _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Func<object?, Task> asyncExecute, Predicate<object?>? canExecute = null)
    {
        _asyncExecute = asyncExecute ?? throw new ArgumentNullException(nameof(asyncExecute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        if (_asyncExecute != null)
        {
            // Fire-and-forget async operation with proper exception handling
            // Since ICommand.Execute is synchronous, we can't await here
            // Use ContinueWith to handle exceptions and prevent unhandled task exceptions
            _asyncExecute(parameter).ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    // Log exception - the ViewModel should handle exceptions in the async method
                    // but this prevents unhandled task exceptions from crashing the app
                    System.Diagnostics.Debug.WriteLine($"Unhandled exception in async command: {task.Exception.GetBaseException()}");
                    
                    // The exception is already handled in StartImportAsync, but we ensure
                    // any unhandled exceptions don't crash the application
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        else
        {
            _execute?.Invoke(parameter);
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
