using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Suppliers;

// Formulario para registrar o editar un proveedor
public class SupplierFormViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    private int? _supplierId;
    private string _name = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public SupplierFormViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que el proveedor se guardo para que se cierre
    public event EventHandler? Saved;

    // Propiedades

    // Null al registrar; con valor al editar
    public int? SupplierId
    {
        get => _supplierId;
        private set
        {
            if (!SetProperty(ref _supplierId, value))
                return;

            // Estas propiedades se calculan a partir de SupplierId
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public bool IsEditMode => SupplierId.HasValue;

    public string WindowTitle =>
        IsEditMode ? "Editar proveedor" : "Nuevo proveedor";

    public string SaveButtonText =>
        IsEditMode ? "Guardar cambios" : "Registrar";

    public string Name
    {
        get => _name;
        set
        {
            if (!SetProperty(ref _name, value))
                return;

            SaveCommand.NotifyCanExecuteChanged();

            if (_validateOnChange)
                ValidateName();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (SetProperty(ref _phoneNumber, value) && _validateOnChange)
                ValidatePhoneNumber();
        }
    }

    // Errores que no pertenecen a un campo, como fallas de conexion
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
                SaveCommand.NotifyCanExecuteChanged();
        }
    }

    // El telefono es opcional: un campo vacio se envia como null
    private string? PhoneNumberOrNull =>
        string.IsNullOrWhiteSpace(PhoneNumber)
            ? null
            : PhoneNumber;

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Metodos publicos

    public void InitializeForCreate()
    {
        SupplierId = null;
        Name = string.Empty;
        PhoneNumber = string.Empty;

        ResetValidation();
    }

    public void InitializeForEdit(
        int supplierId,
        string name,
        string? phoneNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(supplierId);

        SupplierId = supplierId;
        Name = name;
        PhoneNumber = phoneNumber ?? string.Empty;

        ResetValidation();
    }

    // Metodos privados

    private bool CanSave()
    {
        return !IsSaving
            && !string.IsNullOrWhiteSpace(Name);
    }

    private async Task SaveAsync()
    {
        if (!CanSave())
            return;

        ErrorMessage = string.Empty;
        _validateOnChange = true;

        // Las reglas se revisan antes de ir a la base de datos
        if (!ValidateAll())
            return;

        IsSaving = true;

        bool saved = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (IsEditMode)
                await UpdateAsync(scope.ServiceProvider);
            else
                await CreateAsync(scope.ServiceProvider);

            saved = true;
        }
        catch (NameAlreadyExistsException ex)
        {
            // Solo la base de datos sabe si el nombre ya existe
            SetError(nameof(Name), ex.Message);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = GetUserMessage(ex);
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

        // Fuera del try: un error al cerrar la ventana no debe
        // mostrarse como error al guardar
        if (saved)
            Saved?.Invoke(this, EventArgs.Empty);
    }

    // Se usan las mismas reglas del dominio; no se copian aqui
    private bool ValidateAll()
    {
        bool isNameValid = ValidateName();
        bool isPhoneNumberValid = ValidatePhoneNumber();

        return isNameValid && isPhoneNumberValid;
    }

    private bool ValidateName()
    {
        string? error = ValidateRule(
            () => Supplier.NormalizeName(Name));

        SetError(nameof(Name), error);

        return error is null;
    }

    private bool ValidatePhoneNumber()
    {
        string? error = ValidateRule(
            () => Supplier.NormalizePhoneNumber(PhoneNumber));

        SetError(nameof(PhoneNumber), error);

        return error is null;
    }

    private void ResetValidation()
    {
        _validateOnChange = false;
        ErrorMessage = string.Empty;

        ClearErrors();
    }

    private async Task CreateAsync(IServiceProvider services)
    {
        var createSupplier =
            services.GetRequiredService<CreateSupplier>();

        await createSupplier.ExecuteAsync(
            new CreateSupplierCommand(
                Name,
                PhoneNumberOrNull));
    }

    private async Task UpdateAsync(IServiceProvider services)
    {
        var updateSupplier =
            services.GetRequiredService<UpdateSupplier>();

        await updateSupplier.ExecuteAsync(
            new UpdateSupplierCommand(
                SupplierId!.Value,
                Name,
                PhoneNumberOrNull));
    }
}