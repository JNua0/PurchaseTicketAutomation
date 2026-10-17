using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Desktop.Commands;

namespace PurchaseTicket.Desktop.ViewModels.Suppliers;

public class SupplierFormViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;
    private int? _supplierId;
    private string _name = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    public SupplierFormViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    public int? SupplierId
    {
        get => _supplierId;
        private set
        {
            if (_supplierId == value)
                return;

            _supplierId = value;
            OnPropertyChanged(nameof(SupplierId));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }
    public bool IsEditMode => SupplierId.HasValue;
    public string WindowTitle => IsEditMode ? "Editar proveedor" : "Nuevo proveedor";
    public string SaveButtonText => IsEditMode ? "Guardar cambios" : "Registrar";
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            OnPropertyChanged(nameof(Name));

            SaveCommand?.NotifyCanExecuteChanged();
        }
    }
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber == value)
                return;

            _phoneNumber = value;
            OnPropertyChanged(nameof(PhoneNumber));
        }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value)
                return;

            _isSaving = value;
            OnPropertyChanged(nameof(IsSaving));

            SaveCommand?.NotifyCanExecuteChanged();
        }
    }

    public AsyncRelayCommand SaveCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? Saved;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public void InitializeForCreate()
    {
        SupplierId = null;
        Name = string.Empty;
        PhoneNumber = string.Empty;
        ErrorMessage = string.Empty;
    }

    public void InitializeForEdit(
        int supplierId,
        string name,
        string? phoneNumber)
    {
        if (supplierId <= 0)
            throw new ArgumentOutOfRangeException(nameof(supplierId));

        SupplierId = supplierId;
        Name = name;
        PhoneNumber = phoneNumber ?? string.Empty;
        ErrorMessage = string.Empty;
    }

    private bool CanSave()
    {
        return !IsSaving &&
               !string.IsNullOrWhiteSpace(Name);
    }

    private async Task SaveAsync()
    {
        if (IsSaving)
            return;

        IsSaving = true;
        ErrorMessage = string.Empty;
        bool savedSuccessfully = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (IsEditMode)
            {
                var updateSupplier =
                    scope.ServiceProvider
                        .GetRequiredService<UpdateSupplier>();

                var command = new UpdateSupplierCommand(
                    SupplierId!.Value,
                    Name,
                    string.IsNullOrWhiteSpace(PhoneNumber)
                        ? null
                        : PhoneNumber);

                await updateSupplier.ExecuteAsync(command);
            }
            else
            {
                var createSupplier =
                    scope.ServiceProvider
                        .GetRequiredService<CreateSupplier>();

                var command = new CreateSupplierCommand(
                    Name,
                    string.IsNullOrWhiteSpace(PhoneNumber)
                        ? null
                        : PhoneNumber);

                await createSupplier.ExecuteAsync(command);
            }

            savedSuccessfully = true;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al guardar el proveedor.";

            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsSaving = false;
        }

        if (savedSuccessfully)
        {
            Saved?.Invoke(this, EventArgs.Empty);
        }
    }
}
