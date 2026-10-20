using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

// Formulario para registrar o editar un material
public class MaterialFormViewModel : ObservableObject
{
    // Campos

    private readonly IServiceScopeFactory _scopeFactory;

    private int? _materialId;
    private string _name = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

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
            if (SetProperty(ref _name, value))
                SaveCommand.NotifyCanExecuteChanged();
        }
    }

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
        ErrorMessage = string.Empty;
    }

    public void InitializeForEdit(int materialId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(materialId);

        MaterialId = materialId;
        Name = name;
        ErrorMessage = string.Empty;
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

        IsSaving = true;
        ErrorMessage = string.Empty;

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
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // Errores de validacion del dominio: el mensaje ya es para el usuario
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