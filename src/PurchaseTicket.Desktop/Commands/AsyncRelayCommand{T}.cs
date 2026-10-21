using System.Windows.Input;

namespace PurchaseTicket.Desktop.Commands;

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Predicate<T>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(
        Func<T, Task> execute,
        Predicate<T>? canExecute = null)
    {
        _execute = execute
            ?? throw new ArgumentNullException(nameof(execute));

        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (_isExecuting || parameter is not T value)
            return false;

        return _canExecute?.Invoke(value) ?? true;
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter) || parameter is not T value)
            return;

        _isExecuting = true;
        NotifyCanExecuteChanged();

        try
        {
            await _execute(value);
        }
        finally
        {
            _isExecuting = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
