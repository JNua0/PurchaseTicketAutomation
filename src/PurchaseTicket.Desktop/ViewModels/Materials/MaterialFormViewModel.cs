using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

// Formulario para registrar o editar un material
public class MaterialFormViewModel : ValidatableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    private int? _materialId;
    private string _name = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public MaterialFormViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

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
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public bool IsEditMode => MaterialId.HasValue;

    public string WindowTitle =>
        IsEditMode ? "Editar material" : "Nuevo material";

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

    // Metodos publicos

    public void InitializeForCreate()
    {
        MaterialId = null;
        Name = string.Empty;

        ResetValidation();
    }

    public void InitializeForEdit(int materialId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(materialId);

        MaterialId = materialId;
        Name = name;

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
        if (!ValidateName())
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
            ErrorMessage = "Ocurrió un error al guardar el material.";

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