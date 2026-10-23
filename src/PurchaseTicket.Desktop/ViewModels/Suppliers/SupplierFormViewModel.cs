using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Suppliers.Create;
using PurchaseTicket.Application.UseCases.Suppliers.Get;
using PurchaseTicket.Application.UseCases.Suppliers.Update;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Suppliers;

// Formulario para registrar o editar un proveedor. Al editar, cada campo
// avisa si cambio ("Antes ...") y solo se guarda si hay algun cambio
public class SupplierFormViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SupplierFormViewModel> _logger;
    private int? _supplierId;
    private string _name = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isActive;

    // Lo guardado al abrir para editar, ya normalizado como en la base de datos
    private string _originalName = string.Empty;
    private string? _originalPhoneNumber;
    private bool _isSaving;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public SupplierFormViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<SupplierFormViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

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
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public bool IsEditMode => SupplierId.HasValue;

    public string WindowTitle =>
        IsEditMode ? "Editar proveedor" : "Nuevo proveedor";

    public string Description =>
        IsEditMode ? $"Proveedor ID {SupplierId}" : "Información del proveedor";

    public string SaveButtonText =>
        IsEditMode ? "Guardar cambios" : "Registrar";

    // Estado en el encabezado; solo al editar
    public string StatusText => _isActive ? "Activo" : "Inactivo";

    public StatusKind StatusKind => _isActive ? StatusKind.Success : StatusKind.Error;

    public string Name
    {
        get => _name;
        set
        {
            if (!SetProperty(ref _name, value))
                return;

            OnPropertyChanged(nameof(NameHint));

            if (_validateOnChange)
                ValidateName();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (!SetProperty(ref _phoneNumber, value))
                return;

            OnPropertyChanged(nameof(PhoneNumberHint));

            if (_validateOnChange)
                ValidatePhoneNumber();
        }
    }

    // "Sin cambio" o "Antes ..." debajo de cada campo; vacio al registrar
    public string NameHint =>
        IsEditMode ? ChangeHint.Describe(IsNameChanged, _originalName) : string.Empty;

    public string PhoneNumberHint =>
        IsEditMode
            ? ChangeHint.Describe(
                IsPhoneNumberChanged,
                _originalPhoneNumber is null ? "sin teléfono" : DisplayFormat.PhoneNumber(_originalPhoneNumber))
            : string.Empty;

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

    // Comandos

    public AsyncRelayCommand SaveCommand { get; }

    // Nombre con el que quedo guardado, ya normalizado; lo usa el aviso de la lista
    public string SavedName { get; private set; } = string.Empty;

    // Propiedades privadas

    // El telefono es opcional: un campo vacio se envia como null
    private string? PhoneNumberOrNull =>
        string.IsNullOrWhiteSpace(PhoneNumber)
            ? null
            : PhoneNumber;

    // Se compara ya normalizado: "aceros del norte" es el mismo nombre que
    // "Aceros Del Norte". Un valor que no cumple la regla cuenta como cambio
    private bool IsNameChanged =>
        !DomainRule.TryNormalize(() => Supplier.NormalizeName(Name), out string? name)
        || name != _originalName;

    private bool IsPhoneNumberChanged =>
        !DomainRule.TryNormalize(() => Supplier.NormalizePhoneNumber(PhoneNumber), out string? phoneNumber)
        || phoneNumber != _originalPhoneNumber;

    // Metodos publicos

    public void InitializeForCreate()
    {
        SupplierId = null;
        _originalName = string.Empty;
        _originalPhoneNumber = null;
        Name = string.Empty;
        PhoneNumber = string.Empty;

        ResetValidation();
    }

    public void InitializeForEdit(SupplierDto supplier)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(supplier.Id);

        _originalName = supplier.Name;
        _originalPhoneNumber = supplier.PhoneNumber;
        _isActive = supplier.IsActive;

        SupplierId = supplier.Id;
        Name = supplier.Name;
        PhoneNumber = supplier.PhoneNumber ?? string.Empty;

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusKind));
        OnPropertyChanged(nameof(NameHint));
        OnPropertyChanged(nameof(PhoneNumberHint));

        ResetValidation();
    }

    // Metodos privados

    // Siempre habilitado, como en los demas formularios: los errores se
    // muestran al intentar guardar
    private bool CanSave()
    {
        return !IsSaving;
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

        if (IsEditMode && !IsNameChanged && !IsPhoneNumberChanged)
        {
            ErrorMessage = "No hay cambios que guardar.";

            return;
        }

        IsSaving = true;

        bool saved = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (IsEditMode)
                await UpdateAsync(scope.ServiceProvider);
            else
                await CreateAsync(scope.ServiceProvider);

            SavedName = Supplier.NormalizeName(Name);
            saved = true;
        }
        catch (NameAlreadyExistsException ex)
        {
            // Solo la base de datos sabe si el nombre ya existe
            SetError(nameof(Name), ex.Message);
        }
        catch (NotFoundException ex)
        {
            // El proveedor ya no existe, por ejemplo con la lista desactualizada
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = GetUserMessage(ex);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al guardar el proveedor.";

            if (IsEditMode)
            {
                _logger.LogError(
                    ex,
                    "No fue posible actualizar el proveedor {SupplierId}.",
                    SupplierId);
            }
            else
            {
                _logger.LogError(
                    ex,
                    "No fue posible registrar el proveedor {SupplierName}.",
                    Name);
            }
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