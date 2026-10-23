using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Components.Feedback;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

// Formulario para registrar o editar un material. Al editar, el nombre
// avisa si cambio ("Antes ...") y solo se guarda si cambio
public class MaterialFormViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MaterialFormViewModel> _logger;

    private int? _materialId;
    private string _name = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;
    private bool _isActive;

    // El nombre guardado al abrir para editar, ya normalizado
    private string _originalName = string.Empty;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public MaterialFormViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<MaterialFormViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    // Eventos

    // Avisa a la ventana que el material se guardo para que se cierre
    public event EventHandler? Saved;

    // Propiedades

    // Null al registrar; con valor al editar
    public int? MaterialId
    {
        get => _materialId;
        private set
        {
            if (!SetProperty(ref _materialId, value))
                return;

            // Estas propiedades se calculan a partir de MaterialId
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public bool IsEditMode => MaterialId.HasValue;

    public string WindowTitle =>
        IsEditMode ? "Editar material" : "Nuevo material";

    public string Description =>
        IsEditMode ? $"Material ID {MaterialId}" : "Información del material";

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

    // "Sin cambio" o "Antes ..." debajo del nombre; vacio al registrar
    public string NameHint =>
        IsEditMode ? ChangeHint.Describe(IsNameChanged, _originalName) : string.Empty;

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

    // Propiedades privadas

    // Se compara ya normalizado: "acero" es el mismo nombre que "Acero".
    // Un nombre que no cumple la regla cuenta como cambio
    private bool IsNameChanged
    {
        get
        {
            try
            {
                return Material.NormalizeName(Name) != _originalName;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }

    // Metodos publicos

    public void InitializeForCreate()
    {
        MaterialId = null;
        _originalName = string.Empty;
        Name = string.Empty;

        ResetValidation();
    }

    public void InitializeForEdit(MaterialDto material)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(material.Id);

        _originalName = material.Name;
        _isActive = material.IsActive;

        MaterialId = material.Id;
        Name = material.Name;

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusKind));
        OnPropertyChanged(nameof(NameHint));

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
        if (!ValidateName())
            return;

        if (IsEditMode && !IsNameChanged)
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

            saved = true;
        }
        catch (NameAlreadyExistsException ex)
        {
            // Solo la base de datos sabe si el nombre ya existe
            SetError(nameof(Name), ex.Message);
        }
        catch (NotFoundException ex)
        {
            // El material ya no existe, por ejemplo con la lista desactualizada
            ErrorMessage = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = GetUserMessage(ex);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al guardar el material.";

            if (IsEditMode)
            {
                _logger.LogError(
                    ex,
                    "No fue posible actualizar el material {MaterialId}.",
                    MaterialId);
            }
            else
            {
                _logger.LogError(
                    ex,
                    "No fue posible registrar el material {MaterialName}.",
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

    // Se usa la misma regla del dominio; no se copia aqui
    private bool ValidateName()
    {
        string? error = ValidateRule(
            () => Material.NormalizeName(Name));

        SetError(nameof(Name), error);

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
        var createMaterial =
            services.GetRequiredService<CreateMaterial>();

        await createMaterial.ExecuteAsync(
            new CreateMaterialCommand(Name));
    }

    private async Task UpdateAsync(IServiceProvider services)
    {
        var updateMaterial =
            services.GetRequiredService<UpdateMaterial>();

        await updateMaterial.ExecuteAsync(
            new UpdateMaterialCommand(
                MaterialId!.Value,
                Name));
    }
}