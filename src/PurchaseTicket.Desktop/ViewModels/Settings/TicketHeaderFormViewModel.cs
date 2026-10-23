using System.Net.Mail;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PurchaseTicket.Application.Abstractions.Printing;
using PurchaseTicket.Desktop.Commands;
using PurchaseTicket.Desktop.Configuration;
using PurchaseTicket.Desktop.ViewModels.Base;
using PurchaseTicket.Domain.Entities;

namespace PurchaseTicket.Desktop.ViewModels.Settings;

// Ventana "Editar encabezado": los datos de la empresa que se imprimen al
// inicio de cada ticket, con la vista previa mientras se escribe
public sealed class TicketHeaderFormViewModel : ValidatableObject
{
    // Campos

    private readonly ApplicationSettingsService _settingsService;
    private readonly ILogger<TicketHeaderFormViewModel> _logger;

    private string _companyName = string.Empty;
    private string _address = string.Empty;
    private string _neighborhood = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _email = string.Empty;
    private string _errorMessage = string.Empty;

    // La vista previa al abrir: si no cambia, no hay nada que guardar
    private string _originalPreview = string.Empty;

    // Se activa tras el primer intento de guardar; desde ahi se valida al escribir
    private bool _validateOnChange;

    // Constructor

    public TicketHeaderFormViewModel(
        ApplicationSettingsService settingsService,
        ILogger<TicketHeaderFormViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;

        SaveCommand = new RelayCommand(Save);
    }

    // Eventos

    // Avisa a la ventana que el encabezado se guardo para que se cierre
    public event EventHandler? Saved;

    // Propiedades

    // Caracteres por linea del ticket; la vista lo usa en los contadores
    public int LineWidth => TicketFormat.LineWidth;

    public string CompanyName
    {
        get => _companyName;
        set => SetField(ref _companyName, value, ValidateCompanyName);
    }

    public string Address
    {
        get => _address;
        set => SetField(ref _address, value, ValidateAddress);
    }

    public string Neighborhood
    {
        get => _neighborhood;
        set => SetField(ref _neighborhood, value, ValidateNeighborhood);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetField(ref _phoneNumber, value, ValidatePhoneNumber);
    }

    public string Email
    {
        get => _email;
        set => SetField(ref _email, value, ValidateEmail);
    }

    // El encabezado como lo imprimira la impresora con los datos actuales
    public string Preview => BuildHeader().ToPreview();

    // Errores que no pertenecen a un campo, como no poder escribir el archivo
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    // Lo guardado, para que Configuracion actualice su vista previa
    public TicketHeaderSettings? SavedHeader { get; private set; }

    // Comandos

    public RelayCommand SaveCommand { get; }

    // Metodos publicos

    public void Initialize(TicketHeaderSettings header)
    {
        ArgumentNullException.ThrowIfNull(header);

        CompanyName = header.CompanyName;
        Address = header.Address;
        Neighborhood = header.Neighborhood;
        PhoneNumber = header.PhoneNumber;
        Email = header.Email;

        _originalPreview = Preview;

        _validateOnChange = false;
        ErrorMessage = string.Empty;
        ClearErrors();
    }

    // Metodos privados

    // Una linea mas larga que el papel se corta o se parte al imprimir
    private static string? CheckLength(string value, string fieldName)
    {
        return value.Trim().Length > TicketFormat.LineWidth
            ? $"{fieldName} no puede exceder los {TicketFormat.LineWidth} caracteres: no cabría en una línea del ticket."
            : null;
    }

    // MailAddress acepta formas como "Nombre <correo>"; se exige solo el correo
    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email, out var address)
            && address.Address == email
            && address.Host.Contains('.');
    }

    // Comun a los cinco campos. El nombre se pasa a SetProperty: sin esto
    // recibiria "SetField" y la vista no se enteraria del cambio
    private void SetField(
        ref string field,
        string value,
        Func<bool> validate,
        [CallerMemberName] string propertyName = "")
    {
        if (!SetProperty(ref field, value, propertyName))
            return;

        OnPropertyChanged(nameof(Preview));

        if (_validateOnChange)
            validate();
    }

    private TicketHeaderSettings BuildHeader()
    {
        return new TicketHeaderSettings
        {
            CompanyName = CompanyName.Trim(),
            Address = Address.Trim(),
            Neighborhood = Neighborhood.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            Email = Email.Trim()
        };
    }

    private void Save()
    {
        ErrorMessage = string.Empty;
        _validateOnChange = true;

        if (!ValidateAll())
            return;

        var header = BuildHeader();

        // Se compara lo que se imprimiria: espacios de mas al final no cuentan
        if (header.ToPreview() == _originalPreview)
        {
            ErrorMessage = "No hay cambios que guardar.";

            return;
        }

        try
        {
            _settingsService.SaveTicketHeader(header);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No fue posible guardar el encabezado.";

            _logger.LogError(ex, "No fue posible guardar el encabezado del ticket.");

            return;
        }

        SavedHeader = header;

        // Fuera del try: un error al cerrar la ventana no es un error al guardar
        Saved?.Invoke(this, EventArgs.Empty);
    }

    private bool ValidateAll()
    {
        // Se validan todos para mostrar todos los errores a la vez
        bool[] results =
        [
            ValidateCompanyName(),
            ValidateAddress(),
            ValidateNeighborhood(),
            ValidatePhoneNumber(),
            ValidateEmail()
        ];

        return results.All(isValid => isValid);
    }

    private bool ValidateCompanyName()
    {
        string? error = string.IsNullOrWhiteSpace(CompanyName)
            ? "El nombre de la empresa es obligatorio."
            : CheckLength(CompanyName, "El nombre de la empresa");

        return Apply(nameof(CompanyName), error);
    }

    private bool ValidateAddress()
    {
        return Apply(nameof(Address), CheckLength(Address, "La dirección"));
    }

    private bool ValidateNeighborhood()
    {
        return Apply(nameof(Neighborhood), CheckLength(Neighborhood, "La colonia"));
    }

    // Misma regla que el telefono de un proveedor. Se guarda como se escribio,
    // con sus espacios o guiones, porque asi se lee mejor en el ticket
    private bool ValidatePhoneNumber()
    {
        string? error = ValidateRule(
            () => Supplier.NormalizePhoneNumber(PhoneNumber));

        return Apply(nameof(PhoneNumber), error);
    }

    private bool ValidateEmail()
    {
        string email = Email.Trim();

        string? error = email.Length == 0 ? null
            : !IsValidEmail(email) ? "Escribe un correo válido, como compras@empresa.mx."
            : CheckLength(email, "El correo");

        return Apply(nameof(Email), error);
    }

    private bool Apply(string propertyName, string? error)
    {
        SetError(propertyName, error);

        return error is null;
    }
}