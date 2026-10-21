using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Common;
using PurchaseTicket.Application.UseCases.Materials.Activate;
using PurchaseTicket.Application.UseCases.Materials.Deactivate;
using PurchaseTicket.Application.UseCases.Materials.Get;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Services;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Desktop.ViewModels.Suppliers;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Materials;

// Listado de materiales: busqueda y paginacion vienen de la clase base
public class MaterialsViewModel : PagedListViewModel<MaterialDto>
{
    // Campos

    private readonly IMaterialDialogService _materialDialogService;
    private readonly IConfirmationDialogService _confirmationDialogService;
    private readonly INotificationService _notifications;
    private bool? _isActiveFilter;

    // Constructor

    public MaterialsViewModel(
        IServiceScopeFactory scopeFactory,
        IMaterialDialogService materialDialogService,
        IConfirmationDialogService confirmationDialogService,
        INotificationService notifications,
        ILogger<SuppliersViewModel> logger)
        : base(scopeFactory, logger)
    {
        _materialDialogService = materialDialogService;
        _confirmationDialogService = confirmationDialogService;
        _notifications = notifications;

        NewMaterialCommand = new AsyncRelayCommand(
            NewMaterialAsync,
            CanInteract);

        EditMaterialCommand = new AsyncRelayCommand<MaterialDto>(
            EditMaterialAsync,
            CanActOn);

        ChangeMaterialStatusCommand = new AsyncRelayCommand<MaterialDto>(
            ChangeMaterialStatusAsync,
            CanActOn);
    }

    // Propiedades

    // Null muestra todos; true solo activos; false solo inactivos
    public bool? IsActiveFilter
    {
        get => _isActiveFilter;
        set
        {
            // Elegir un filtro es una accion deliberada: recargar de inmediato
            if (SetProperty(ref _isActiveFilter, value))
                SearchCommand.Execute(null);
        }
    }

    // Comandos

    public AsyncRelayCommand NewMaterialCommand { get; }

    public AsyncRelayCommand<MaterialDto> EditMaterialCommand { get; }

    public AsyncRelayCommand<MaterialDto> ChangeMaterialStatusCommand { get; }

    // Propiedades protegidas

    protected override string LoadErrorMessage =>
        "No fue posible cargar los materiales.";

    protected override string EmptyListMessage =>
        "Aún no hay materiales registrados.";

    protected override string NoResultsMessage =>
        "No se encontraron materiales con los filtros aplicados.";

    protected override bool HasExtraFilters =>
        IsActiveFilter is not null;

    // Metodos protegidos

    protected override Task<PagedResult<MaterialDto>> FetchPageAsync(
        IServiceProvider services,
        int page,
        int pageSize,
        string? search)
    {
        var getMaterials =
            services.GetRequiredService<GetMaterials>();

        return getMaterials.ExecuteAsync(
            new GetMaterialsQuery(
                Page: page,
                PageSize: pageSize,
                Search: search,
                IsActive: IsActiveFilter));
    }

    protected override void NotifyCommandsCanExecuteChanged()
    {
        base.NotifyCommandsCanExecuteChanged();

        NewMaterialCommand?.NotifyCanExecuteChanged();
        EditMaterialCommand?.NotifyCanExecuteChanged();
        ChangeMaterialStatusCommand?.NotifyCanExecuteChanged();
    }

    // Metodos privados

    private bool CanActOn(MaterialDto material)
    {
        return material is not null && CanInteract();
    }

    private async Task NewMaterialAsync()
    {
        if (!_materialDialogService.ShowCreateDialog())
            return;

        _notifications.ShowSuccess("Material registrado correctamente.");

        await ReloadAfterActionAsync(
            page: 1,
            "El material se registró correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task EditMaterialAsync(MaterialDto material)
    {
        bool saved = _materialDialogService.ShowEditDialog(
            material.Id,
            material.Name);

        if (!saved)
            return;

        _notifications.ShowSuccess(
            $"Material \"{material.Name}\" actualizado correctamente.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El material se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }

    private async Task ChangeMaterialStatusAsync(MaterialDto material)
    {
        string action = material.IsActive
            ? "desactivar"
            : "activar";

        bool confirmed = _confirmationDialogService.Confirm(
            "Confirmar cambio de estado",
            $"¿Estás seguro de que deseas {action} el material " +
            $"\"{material.Name}\"?");

        if (!confirmed)
            return;

        IsBusy = true;
        QueryErrorMessage = string.Empty;

        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();

            if (material.IsActive)
            {
                await scope.ServiceProvider
                    .GetRequiredService<DeactivateMaterial>()
                    .ExecuteAsync(material.Id);
            }
            else
            {
                await scope.ServiceProvider
                    .GetRequiredService<ActivateMaterial>()
                    .ExecuteAsync(material.Id);
            }
        }
        catch (Exception ex)
        {
            QueryErrorMessage =
                "No fue posible cambiar el estado del material.";

            Logger.LogError(
                ex,
                "No fue posible cambiar el estado del material {MaterialId}.",
                material.Id);

            return;
        }
        finally
        {
            IsBusy = false;
        }

        string result = material.IsActive
            ? "desactivado"
            : "activado";

        _notifications.ShowSuccess(
            $"Material \"{material.Name}\" {result} correctamente.");

        await ReloadAfterActionAsync(
            CurrentPage,
            "El estado del material se actualizó correctamente, " +
            "pero no fue posible actualizar el listado.");
    }
}