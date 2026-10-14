using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTicket.Application.UseCases.Materials.Activate;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

public class MaterialsViewModel : INotifyPropertyChanged
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMaterialDialogService _materialDialogService;
    private readonly IConfirmationDialogService _confirmationDialogService;

    private string _queryErrorMessage = string.Empty;
    private string _searchText = string.Empty;
    private bool? _isActiveFilter;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private bool _isLoading;
    private bool _isChangingStatus;
    private MaterialDto? _selectedMaterial;

    public int[] PageSizeOptions { get; } = [10, 20, 30, 50, 100];

    public MaterialsViewModel(
        IServiceScopeFactory scopeFactory,
        IMaterialDialogService materialDialogService,
        IConfirmationDialogService confirmationDialogService)
    {
        _scopeFactory = scopeFactory;
        _materialDialogService = materialDialogService;
        _confirmationDialogService = confirmationDialogService;

        // Comandos generales
        SearchMaterialsCommand = new AsyncRelayCommand(
            SearchMaterialsAsync,
            CanSearchMaterials);

        NextPageCommand = new AsyncRelayCommand(
            NextPageAsync,
            CanGoToNextPage);

        PreviousPageCommand = new AsyncRelayCommand(
            PreviousPageAsync,
            CanGoToPreviousPage);

        NewMaterialCommand = new AsyncRelayCommand(
            NewMaterialAsync,
            CanOpenMaterialForm);

        ChangePageSizeCommand = new AsyncRelayCommand(
            ChangePageSizeAsync,
            CanChangePageSize);

        // Comandos que reciben un material
        EditMaterialCommand = new AsyncRelayCommand<MaterialDto>(
            EditMaterialAsync,
            CanEditMaterial);

        ChangeMaterialStatusCommand = new AsyncRelayCommand<MaterialDto>(
            ChangeMaterialStatusAsync,
            CanChangeMaterialStatus);
    }

    // Coleccion de materiales
    public ObservableCollection<MaterialDto> Materials { get; } = new();

    // Comandos generales
    public AsyncRelayCommand SearchMaterialsCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public AsyncRelayCommand NewMaterialCommand { get; }
    public AsyncRelayCommand ChangePageSizeCommand { get; }

    // Comandos para acciones individuales
    public AsyncRelayCommand<MaterialDto> EditMaterialCommand { get; }
    public AsyncRelayCommand<MaterialDto> ChangeMaterialStatusCommand { get; }

    // Mensaje de error de consultas
    public string QueryErrorMessage
    {
        get => _queryErrorMessage;
        private set
        {
            if (_queryErrorMessage == value)
                return;

            _queryErrorMessage = value;
            OnPropertyChanged(nameof(QueryErrorMessage));
        }
    }

    // Texto de busqueda
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged(nameof(SearchText));
        }
    }

    // Filtro de materiales activos o inactivos
    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            if (_isActiveFilter == value)
                return;

            _isActiveFilter = value;
            OnPropertyChanged(nameof(IsActiveFilter));
        }
    }

    // Pagina actual
    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage == value)
                return;

            _currentPage = value;
            OnPropertyChanged(nameof(CurrentPage));
            NotifyPaginationCommands();
        }
    }

    // Cantidad de registros por pagina
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize == value || !PageSizeOptions.Contains(value))
                return;

            _pageSize = value;

            OnPropertyChanged(nameof(PageSize));
            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    // Cantidad total de materiales
    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (_totalCount == value)
                return;

            _totalCount = value;

            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TotalPages));
            NotifyPaginationCommands();
        }
    }

    // Cantidad total de paginas
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    // Estado de carga
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));

            NotifyCommandsCanExecuteChanged();
        }
    }

    // Material seleccionado
    public MaterialDto? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (_selectedMaterial == value)
                return;

            _selectedMaterial = value;
            OnPropertyChanged(nameof(SelectedMaterial));
        }
    }

    // Estado de actualizacion
    public bool IsChangingStatus
    {
        get => _isChangingStatus;
        private set
        {
            if (_isChangingStatus == value)
                return;

            _isChangingStatus = value;
            OnPropertyChanged(nameof(IsChangingStatus));

            NotifyCommandsCanExecuteChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    // Cargar materiales
    public async Task LoadMaterialsAsync()
    {
        await LoadPageAsync(CurrentPage);
    }

    // Validar navegacion a la siguiente pagina
    private bool CanGoToNextPage()
    {
        return !IsLoading
            && !IsChangingStatus
            && CurrentPage < TotalPages;
    }

    // Validar navegacion a la pagina anterior
    private bool CanGoToPreviousPage()
    {
        return !IsLoading
            && !IsChangingStatus
            && CurrentPage > 1;
    }

    // Navegar a la siguiente pagina
    private async Task NextPageAsync()
    {
        await GoToPageAsync(CurrentPage + 1);
    }

    // Navegar a la pagina anterior
    private async Task PreviousPageAsync()
    {
        await GoToPageAsync(CurrentPage - 1);
    }

    // Notificar cambios en la paginacion
    private void NotifyPaginationCommands()
    {
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
    }

    // Buscar materiales
    private async Task SearchMaterialsAsync()
    {
        await LoadPageAsync(1);
    }

    // Cambiar la cantidad de registros por pagina
    private async Task ChangePageSizeAsync()
    {
        await LoadPageAsync(1);
    }

    // Navegar a una pagina especifica
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        await LoadPageAsync(page);
    }

    // Validar disponibilidad de busqueda
    private bool CanSearchMaterials()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Consultar materiales con paginacion
    private async Task<bool> LoadPageAsync(int page)
    {
        if (IsLoading)
            return false;

        IsLoading = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var getMaterials = scope.ServiceProvider
                .GetRequiredService<GetMaterials>();

            var query = new GetMaterialsQuery(
                Page: page,
                PageSize: PageSize,
                Search: string.IsNullOrWhiteSpace(SearchText)
                    ? null
                    : SearchText.Trim(),
                IsActive: IsActiveFilter);

            var result = await getMaterials.ExecuteAsync(query);

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    (double)result.TotalCount / PageSize));

            // Corregir la pagina cuando disminuye el total
            if (page > totalPages)
            {
                page = totalPages;

                query = query with
                {
                    Page = page
                };

                result = await getMaterials.ExecuteAsync(query);
            }

            SelectedMaterial = null;
            Materials.Clear();

            foreach (var material in result.Items)
            {
                Materials.Add(material);
            }

            TotalCount = result.TotalCount;
            CurrentPage = page;

            return true;
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cargar los materiales.";

            Console.Error.WriteLine(ex);

            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Validar apertura del formulario
    private bool CanOpenMaterialForm()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Registrar un material
    private async Task NewMaterialAsync()
    {
        if (!CanOpenMaterialForm())
            return;

        var saved = _materialDialogService.ShowCreateDialog();

        if (!saved)
            return;

        var loaded = await LoadPageAsync(1);

        if (!loaded)
        {
            QueryErrorMessage =
                "El material se registro correctamente, " +
                "pero no fue posible actualizar el listado.";
        }
    }

    // Validar edicion de un material
    private bool CanEditMaterial(MaterialDto material)
    {
        return material is not null
            && !IsLoading
            && !IsChangingStatus;
    }

    // Editar el material recibido
    private async Task EditMaterialAsync(MaterialDto material)
    {
        if (!CanEditMaterial(material))
            return;

        var saved = _materialDialogService.ShowEditDialog(
            material.Id,
            material.Name);

        if (!saved)
            return;

        var loaded = await LoadPageAsync(CurrentPage);

        if (!loaded)
        {
            QueryErrorMessage =
                "El material se actualizo correctamente, " +
                "pero no fue posible actualizar el listado.";
        }
    }

    // Validar cambio de estado
    private bool CanChangeMaterialStatus(MaterialDto material)
    {
        return material is not null
            && !IsLoading
            && !IsChangingStatus;
    }

    // Activar o desactivar el material recibido
    private async Task ChangeMaterialStatusAsync(MaterialDto material)
    {
        if (!CanChangeMaterialStatus(material))
            return;

        var action = material.IsActive
            ? "desactivar"
            : "activar";

        var confirmed = _confirmationDialogService.Confirm(
            "Confirmar cambio de estado",
            $"¿Estás seguro de que deseas {action} el material " +
            $"\"{material.Name}\"?");

        if (!confirmed)
            return;

        IsChangingStatus = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (material.IsActive)
            {
                var deactivateMaterial = scope.ServiceProvider
                    .GetRequiredService<DeactivateMaterial>();

                await deactivateMaterial.ExecuteAsync(material.Id);
            }
            else
            {
                var activateMaterial = scope.ServiceProvider
                    .GetRequiredService<ActivateMaterial>();

                await activateMaterial.ExecuteAsync(material.Id);
            }

            var loaded = await LoadPageAsync(CurrentPage);

            if (!loaded)
            {
                QueryErrorMessage =
                    "El estado del material se actualizó correctamente, " +
                    "pero no fue posible actualizar el listado.";
            }
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cambiar el estado del material.";

            Console.Error.WriteLine(ex);
        }
        finally
        {
            IsChangingStatus = false;
        }
    }

    // Validar cambio de registros por pagina
    private bool CanChangePageSize()
    {
        return !IsLoading && !IsChangingStatus;
    }

    // Notificar disponibilidad de comandos
    private void NotifyCommandsCanExecuteChanged()
    {
        SearchMaterialsCommand?.NotifyCanExecuteChanged();
        NextPageCommand?.NotifyCanExecuteChanged();
        PreviousPageCommand?.NotifyCanExecuteChanged();
        NewMaterialCommand?.NotifyCanExecuteChanged();
        EditMaterialCommand?.NotifyCanExecuteChanged();
        ChangeMaterialStatusCommand?.NotifyCanExecuteChanged();
        ChangePageSizeCommand?.NotifyCanExecuteChanged();
    }
}
