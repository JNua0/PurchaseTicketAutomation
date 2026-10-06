using System.Globalization;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Domain.Entities;

public class PurchaseTicket
{
    // Attributes
    public int Id { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public int SupplierId { get; private set; }
    public int MaterialId { get; private set; }
    public string? LicensePlate { get; private set; }
    public string Transporter { get; private set; } = string.Empty;
    public DateTime CheckInAt { get; private set; }
    public DateTime? DepartureAt { get; private set; }
    public decimal? GrossWeight { get; private set; }
    public decimal? TareWeight { get; private set; }
    public decimal? NetWeight { get; private set; }
    public decimal? Discount { get; private set; }
    public decimal? DiscountWeight { get; private set; }
    public decimal? NetWeightAfterDiscount { get; private set; }
    public decimal? PricePerKg { get; private set; }
    public decimal? Amount { get; private set; }
    public TicketStatus Status { get; private set; }
    public WeighingType WeighingType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Constructor
    private PurchaseTicket() {}

    // Methods
    public static PurchaseTicket CreateConventional(
        string ticketNumber,
        int supplierId,
        int materialId,
        string licensePlate,
        string transporter,
        decimal grossWeight)
    {
        ValidateTicketNumber(ticketNumber);
        ValidateSupplierId(supplierId);
        ValidateMaterialId(materialId);
        ValidateGrossWeight(grossWeight);

        string normalizedLicensePlate =
            NormalizeLicensePlate(licensePlate);

        string normalizedTransporter =
            NormalizeTransporter(transporter);

        var now = DateTime.UtcNow;

        return new PurchaseTicket
        {
            TicketNumber = ticketNumber.Trim(),
            SupplierId = supplierId,
            MaterialId = materialId,
            LicensePlate = normalizedLicensePlate,
            Transporter = normalizedTransporter,
            GrossWeight = grossWeight,
            CheckInAt = now,
            CreatedAt = now,
            WeighingType = WeighingType.Conventional,
            Status = TicketStatus.WeighingPending
        };
    }

    public static PurchaseTicket CreateSingle(
        string ticketNumber,
        int supplierId,
        int materialId,
        string? licensePlate,
        string transporter,
        decimal netWeight)
    {
        ValidateTicketNumber(ticketNumber);
        ValidateSupplierId(supplierId);
        ValidateMaterialId(materialId);
        ValidateNetWeight(netWeight);

        string? normalizedLicensePlate =
            NormalizeOptionalLicensePlate(licensePlate);

        string normalizedTransporter =
            NormalizeTransporter(transporter);

        var now = DateTime.UtcNow;

        return new PurchaseTicket
        {
            TicketNumber = ticketNumber.Trim(),
            SupplierId = supplierId,
            MaterialId = materialId,
            LicensePlate = normalizedLicensePlate,
            Transporter = normalizedTransporter,
            GrossWeight = null,
            TareWeight = null,
            NetWeight = netWeight,
            CheckInAt = now,
            DepartureAt = null,
            CreatedAt = now,
            WeighingType = WeighingType.Single,
            Status = TicketStatus.AmountPending
        };
    }

    public void Cancel()
    {
        if (Status == TicketStatus.Cancelled)
            throw new InvalidOperationException(
                "El ticket ya se encuentra cancelado.");

        if (Status == TicketStatus.Completed)
            throw new InvalidOperationException(
                "Un ticket completado no puede cancelarse.");

        Status = TicketStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterTare(decimal tareWeight)
    {
        ValidateConventionalWeighing();
        ValidateWeighingPendingStatus();

        decimal grossWeight = GrossWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener un peso bruto.");

        ValidateTareWeight(tareWeight, grossWeight);

        TareWeight = tareWeight;

        NetWeight = CalculateNetWeight(grossWeight, tareWeight);

        DepartureAt = DateTime.UtcNow;

        Status = TicketStatus.AmountPending;
    }

    public void RegisterAmount(decimal discount, decimal pricePerKg)
    {
        ValidateAmountPendingStatus();

        if (NetWeight is null)
            throw new InvalidOperationException("El ticket debe tener un peso neto.");

        ValidateDiscount(discount);
        ValidatePricePerKg(pricePerKg);

        Discount = discount;
        PricePerKg = pricePerKg;

        RecalculateAmount();

        Status = TicketStatus.Completed;
    }

    private void RecalculateAmount()
    {
        decimal netWeight = NetWeight
            ?? throw new InvalidOperationException(
                "El ticket debe tener un peso neto.");

        decimal discount = Discount
            ?? throw new InvalidOperationException(
                "El ticket debe tener un descuento.");

        decimal pricePerKg = PricePerKg
            ?? throw new InvalidOperationException(
                "El ticket debe tener un precio por kilogramo.");

        decimal discountWeight =
            CalculateDiscountWeight(
                netWeight,
                discount);

        decimal netWeightAfterDiscount =
            CalculateNetWeightAfterDiscount(
                netWeight,
                discountWeight);

        decimal amount =
            CalculateAmount(
                netWeightAfterDiscount,
                pricePerKg);

        DiscountWeight = discountWeight;
        NetWeightAfterDiscount = netWeightAfterDiscount;
        Amount = amount;
    }

    // Modifiers
    public void ChangeSupplier(int supplierId)
    {
        ValidateCanModifyGeneralData();
        ValidateSupplierId(supplierId);

        SupplierId = supplierId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeMaterial(int materialId)
    {
        ValidateCanModifyGeneralData();
        ValidateMaterialId(materialId);

        MaterialId = materialId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeLicensePlate(string? licensePlate)
    {
        ValidateCanModifyGeneralData();

        LicensePlate = WeighingType == WeighingType.Conventional
            ? NormalizeLicensePlate(licensePlate!)
            : NormalizeOptionalLicensePlate(licensePlate);

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeTransporter(string transporter)
    {
        ValidateCanModifyGeneralData();

        Transporter = NormalizeTransporter(transporter);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeGrossWeight(decimal grossWeight)
    {
        ValidateConventionalWeighing();
        ValidateCanModifyGrossWeight();
        ValidateGrossWeight(grossWeight);

        if (Status == TicketStatus.WeighingPending)
        {
            GrossWeight = grossWeight;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        decimal tareWeight = TareWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener una tara.");

        ValidateTareWeight(tareWeight, grossWeight);

        GrossWeight = grossWeight;

        NetWeight = CalculateNetWeight(
            grossWeight,
            tareWeight);

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeTareWeight(decimal tareWeight)
    {
        ValidateConventionalWeighing();
        ValidateCanModifyTareWeight();

        decimal grossWeight = GrossWeight
            ?? throw new InvalidOperationException(
                "El pesaje convencional debe tener un peso bruto.");

        ValidateTareWeight(
            tareWeight,
            grossWeight);

        TareWeight = tareWeight;

        NetWeight = CalculateNetWeight(
            grossWeight,
            tareWeight);

        if (Status == TicketStatus.Completed)
        {
            RecalculateAmount();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDiscount(decimal discount)
    {
        ValidateCompletedStatus();
        ValidateDiscount(discount);

        Discount = discount;

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePricePerKg(decimal pricePerKg)
    {
        ValidateCompletedStatus();
        ValidatePricePerKg(pricePerKg);

        PricePerKg = pricePerKg;

        RecalculateAmount();

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeNetWeight(decimal netWeight)
    {
        ValidateSingleWeighing();
        ValidateCanModifyNetWeight();
        ValidateNetWeight(netWeight);

        NetWeight = netWeight;

        if (Status == TicketStatus.Completed)
        {
            RecalculateAmount();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    // Normalize Functions
    private static string NormalizeTransporter(string transporter)
    {
        if (string.IsNullOrWhiteSpace(transporter))
            throw new ArgumentException("El transportista es obligatorio.");

        transporter = string.Join(
            " ",
            transporter
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (!transporter.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            throw new ArgumentException("El transportista solo puede contener letras.");

        TextInfo textInfo =
            CultureInfo
                .GetCultureInfo("es-MX")
                .TextInfo;

        transporter = textInfo.ToTitleCase(
            transporter.ToLower(
                CultureInfo.GetCultureInfo("es-MX")));

        if (transporter.Length > 50)
            throw new ArgumentException("El transportista no puede exceder los 50 caracteres.");

        return transporter;
    }

    private static string NormalizeLicensePlateValue(string licensePlate)
    {
        licensePlate =
            licensePlate.Trim().ToUpperInvariant();

        if (licensePlate.Length > 7)
            throw new ArgumentException("Las placas no pueden exceder los 7 caracteres.");

        if (!licensePlate.All(char.IsLetterOrDigit))
            throw new ArgumentException("Las placas solo pueden contener caracteres alfanuméricos.");

        return licensePlate;
    }

    private static string? NormalizeOptionalLicensePlate(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return null;

        return NormalizeLicensePlateValue(
            licensePlate);
    }

    private static string NormalizeLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("Las placas son obligatorias.");

        return NormalizeLicensePlateValue(
            licensePlate);
    }

    // Validate Functions
    private void ValidateConventionalWeighing()
    {
        if (WeighingType != WeighingType.Conventional)
            throw new InvalidOperationException("La tara solo puede registrarse en un pesaje convencional.");
    }

    private static void ValidateTareWeight(decimal tareWeight, decimal grossWeight)
    {
        if (tareWeight <= 0)
            throw new ArgumentException("La tara debe ser mayor a cero.");

        if (tareWeight >= grossWeight)
            throw new ArgumentException("La tara debe ser menor al peso bruto.");
    }

    private void ValidateWeighingPendingStatus()
    {
        if (Status != TicketStatus.WeighingPending)
        {
            throw new InvalidOperationException(
                "El ticket debe estar pendiente de pesaje.");
        }
    }

    private void ValidateAmountPendingStatus()
    {
        if (Status != TicketStatus.AmountPending)
            throw new InvalidOperationException("El ticket debe estar pendiente de importe.");
    }

    private static void ValidatePricePerKg(decimal pricePerKg)
    {
        if (pricePerKg <= 0)
            throw new ArgumentException("El precio por kilogramo debe ser mayor a cero.");
    }

    private static void ValidateTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
            throw new ArgumentException("El folio es obligatorio.");

        if (ticketNumber.Trim().Length > 20)
            throw new ArgumentException("El folio no puede exceder los 20 caracteres.");
    }

    private static void ValidateSupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(supplierId),
                "El identificador del proveedor debe ser mayor a cero.");
    }

    private static void ValidateMaterialId(int materialId)
    {
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(materialId),
                "El identificador del material debe ser mayor a cero.");
    }

    private static void ValidateGrossWeight(decimal grossWeight)
    {
        if (grossWeight <= 0)
            throw new ArgumentException("El peso bruto debe ser mayor a cero");
    }

    private static void ValidateNetWeight(decimal netWeight)
    {
        if (netWeight <= 0)
            throw new ArgumentException("El peso neto debe ser mayor a cero.");
    }

    private void ValidateCanModifyGeneralData()
    {
        if (Status != TicketStatus.WeighingPending && Status != TicketStatus.Completed)
            throw new InvalidOperationException("El ticket no permite modificar este campo en su estado actual.");
    }

    private void ValidateCanModifyTareWeight()
    {
        if (Status != TicketStatus.AmountPending && Status != TicketStatus.Completed)
            throw new InvalidOperationException("La tara no puede modificarse en el estado actual del ticket.");
    }

    private void ValidateCanModifyGrossWeight()
    {
        if (Status != TicketStatus.WeighingPending && Status != TicketStatus.Completed)
            throw new InvalidOperationException("El peso bruto no puede modificarse en el estado actual del ticket.");
    }
    
    private void ValidateCompletedStatus()
    {
        if (Status != TicketStatus.Completed)
            throw new InvalidOperationException("El ticket debe estar completado.");
    }

    private static void ValidateDiscount(decimal discount)
    {
        if (discount < 0 || discount >= 100)
            throw new ArgumentException(
                "El descuento debe ser mayor o igual a 0 y menor que 100.",
                nameof(discount));
    }

    private void ValidateSingleWeighing()
    {
        if (WeighingType != WeighingType.Single)
            throw new InvalidOperationException(
                "El peso neto solo puede modificarse directamente en un pesaje único.");
    }

    private void ValidateCanModifyNetWeight()
    {
        if (Status != TicketStatus.AmountPending && Status != TicketStatus.Completed)
            throw new InvalidOperationException(
                "El peso neto no puede modificarse en el estado actual del ticket.");
    }

    // Functions
    private static decimal CalculateNetWeight(decimal grossWeight, decimal tareWeight)
    {
        return grossWeight - tareWeight;
    }

    private static decimal CalculateDiscountWeight(decimal netWeight, decimal discount)
    {
        return netWeight * (discount / 100);
    }

    private static decimal CalculateNetWeightAfterDiscount(decimal netWeight, decimal discountWeight)
    {
        return netWeight - discountWeight;
    }

    private static decimal CalculateAmount(decimal netWeightAfterDiscount, decimal pricePerKg)
    {
        return netWeightAfterDiscount * pricePerKg;
    }
}