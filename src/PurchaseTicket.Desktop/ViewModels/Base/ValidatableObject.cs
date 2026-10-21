using System.Collections;
using System.ComponentModel;

namespace PurchaseTicket.Desktop.ViewModels.Base;

// Base de los formularios con validacion por campo.
// INotifyDataErrorInfo es la interfaz con la que WPF muestra errores junto a cada control.
public abstract class ValidatableObject : ObservableObject, INotifyDataErrorInfo
{
    // Campos

    // Un error por propiedad: el primero que se detecta
    private readonly Dictionary<string, string> _errors = new();

    // Eventos

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    // Propiedades

    public bool HasErrors => _errors.Count > 0;

    // Metodos publicos

    // WPF lo llama para saber que errores mostrar en el control de una propiedad
    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is not null
            && _errors.TryGetValue(propertyName, out var error))
        {
            return new[] { error };
        }

        return Array.Empty<string>();
    }

    // Metodos protegidos

    // Asigna o quita (con null) el error de una propiedad y avisa a la vista
    protected void SetError(string propertyName, string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            if (!_errors.Remove(propertyName))
                return;
        }
        else
        {
            if (_errors.TryGetValue(propertyName, out var current)
                && current == error)
            {
                return;
            }

            _errors[propertyName] = error;
        }

        ErrorsChanged?.Invoke(
            this,
            new DataErrorsChangedEventArgs(propertyName));

        OnPropertyChanged(nameof(HasErrors));
    }

    protected void ClearErrors()
    {
        foreach (var propertyName in _errors.Keys.ToList())
            SetError(propertyName, null);
    }

    // Ejecuta una regla del dominio y devuelve su mensaje si no se cumple.
    // Asi la interfaz reutiliza las reglas en lugar de copiarlas.
    protected static string? ValidateRule(Action rule)
    {
        try
        {
            rule();

            return null;
        }
        catch (ArgumentException ex)
        {
            return GetUserMessage(ex);
        }
    }

    // .NET agrega " (Parameter 'x')" al mensaje; no es para el usuario
    protected static string GetUserMessage(ArgumentException ex)
    {
        if (ex.ParamName is null)
            return ex.Message;

        return ex.Message.Replace(
            $" (Parameter '{ex.ParamName}')",
            string.Empty);
    }
}