using System.Windows.Input;

namespace PurchaseTicket.Desktop.Commands;

public class RelayCommand : ICommand
{
    // Campos

    private readonly Action _execute;

    // Constructor

    public RelayCommand(Action execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    // Eventos

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    // Metodos publicos

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _execute();
    }
}
