using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Materials.Create;
using PurchaseTicket.Application.UseCases.Materials.Update;
using PurchaseTicket.Desktop.Commands;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

public class MaterialFormViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;

    private int? _materialId;
    private string _name = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;

    public MaterialFormViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        SaveCommand = new AsyncRelayCommand(
            SaveAsync,
            CanSave);
    }

    // Identificador del material
    public int? MaterialId
    {
        get => _materialId;
        private set
        {
            if (_materialId == value)
                return;

            _materialId = value;
            OnPropertyChanged(nameof(MaterialId));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    // Informacion del formulario
    public bool IsEditMode => MaterialId.HasValue;

    public string WindowTitle =>
        IsEditMode ? "Editar material" : "Nuevo material";

    public string SaveButtonText =>
        IsEditMode ? "Guardar cambios" : "Registrar";

    // Nombre del material
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            OnPropertyChanged(nameof(Name));

            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    // Mensaje de error
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

    // Estado de guardado
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value)
                return;

            _isSaving = value;
            OnPropertyChanged(nameof(IsSaving));

            SaveCommand.NotifyCanExecuteChanged();
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

    // Preparar el formulario para registrar
    public void InitializeForCreate()
    {
        MaterialId = null;
        Name = string.Empty;
        ErrorMessage = string.Empty;
    }

    // Preparar el formulario para editar
    public void InitializeForEdit(int materialId, string name)
    {
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materialId));

        MaterialId = materialId;
        Name = name;
        ErrorMessage = string.Empty;
    }

    // Validar si se puede guardar
    private bool CanSave()
    {
        return !IsSaving
            && !string.IsNullOrWhiteSpace(Name);
    }

    // Registrar o actualizar el material
    private async Task SaveAsync()
    {
        if (IsSaving || !CanSave())
            return;

        IsSaving = true;
        ErrorMessage = string.Empty;

        bool savedSuccessfully = false;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (IsEditMode)
            {
                var updateMaterial = scope.ServiceProvider
                    .GetRequiredService<UpdateMaterial>();

                var command = new UpdateMaterialCommand(
                    MaterialId!.Value,
                    Name);

                await updateMaterial.ExecuteAsync(command);
            }
            else
            {
                var createMaterial = scope.ServiceProvider
                    .GetRequiredService<CreateMaterial>();

                var command = new CreateMaterialCommand(Name);

                await createMaterial.ExecuteAsync(command);
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
            ErrorMessage = "Ocurrió un error al guardar el material.";
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
