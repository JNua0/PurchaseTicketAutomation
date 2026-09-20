using System.Globalization;
using PurchaseTicket.Domain.Enums;

namespace PurchaseTicket.Domain.Entities;

public class PurchaseTicket
{
    public int Id { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public int SupplierCustomerId { get; private set; }
    public int MaterialId { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;
    public string DriverName { get; private set; } = string.Empty;
    public decimal GrossWeight { get; private set; }
    public decimal? TareWeight { get; private set; }
    public decimal? NetWeight { get; private set; }
    public decimal? Discount { get; private set; }
    public decimal? DiscountWeight { get; private set; }
    public decimal? NetWeightAfterDiscount { get; private set; }
    public decimal? PricePerKg { get; private set; }
    public decimal? Amount { get; private set; }
    public TicketStatus Status { get; private set; }

    public PurchaseTicket(string ticketNumber, int supplierCustomerId, int materialId, string licensePlate, string driverName, decimal grossWeight)
    {
        ValidateTicketNumber(ticketNumber);
        ValidateSupplierCustomer(supplierCustomerId);
        ValidateMaterial(materialId);
        ValidateGrossWeight(grossWeight);

        LicensePlate = NormalizeLicensePlate(licensePlate);
        DriverName = NormalizeDriverName(driverName);

        TicketNumber = ticketNumber.Trim();
        CreatedAt = DateTime.Now;
        SupplierCustomerId = supplierCustomerId;
        MaterialId = materialId;
        GrossWeight = grossWeight;
        Status = TicketStatus.Pending;
    }

    public void Complete(decimal tareWeight, decimal discount, decimal pricePerKg)
    {
        ValidatePendingStatus();
        ValidateTareWeight(tareWeight);
        ValidateDiscount(discount);
        ValidatePricePerKg(pricePerKg);

        decimal netWeight =
            CalculateNetWeight(GrossWeight, tareWeight);

        decimal discountWeight =
            CalculateDiscountWeight(netWeight, discount);

        decimal netWeightAfterDiscount =
            CalculateNetWeightAfterDiscount(netWeight, discountWeight);

        decimal amount =
            CalculateAmount(netWeightAfterDiscount, pricePerKg);

        TareWeight = tareWeight;
        NetWeight = netWeight;
        Discount = discount;
        DiscountWeight = discountWeight;
        NetWeightAfterDiscount = netWeightAfterDiscount;
        PricePerKg = pricePerKg;
        Amount = amount;

        Status = TicketStatus.Completed;
    }

    public void Cancel()
    {
        ValidatePendingStatus();

        Status = TicketStatus.Cancelled;
    }

    public void Correct(int supplierCustomerId, int materialId, string licensePlate, string driverName, decimal grossWeight)
    {
        ValidatePendingStatus();

        ValidateSupplierCustomer(supplierCustomerId);
        ValidateMaterial(materialId);
        ValidateGrossWeight(grossWeight);

        string normalizedLicensePlate =
        NormalizeLicensePlate(licensePlate);

        string normalizedDriverName =
            NormalizeDriverName(driverName);

        SupplierCustomerId = supplierCustomerId;
        MaterialId = materialId;
        LicensePlate = normalizedLicensePlate;
        DriverName = normalizedDriverName;
        GrossWeight = grossWeight;
    }

    private void ValidatePendingStatus()
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("El ticket debe estar en estado pendiente.");
    }

    private void ValidateTareWeight(decimal tareWeight)
    {
        if (tareWeight <= 0)
            throw new ArgumentException("La tara debe ser mayor a cero.");

        if (tareWeight >= GrossWeight)
            throw new ArgumentException("La tara debe ser menor al peso bruto.");
    }

    private static void ValidateDiscount(decimal discount)
    {
        if (discount < 0 || discount > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100.");
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

    private static void ValidateSupplierCustomer(int supplierCustomerId)
    {
        if (supplierCustomerId <= 0)
            throw new ArgumentException("El proveedor o cliente es obligatorio.");
    }

    private static void ValidateMaterial(int materialId)
    {
        if (materialId <= 0)
            throw new ArgumentException("El material es obligatorio.");
    }

    private static void ValidateGrossWeight(decimal grossWeight)
    {
        if (grossWeight <= 0)
            throw new ArgumentException("El peso bruto debe ser mayor a cero");
    }

    private static string NormalizeDriverName(string driverName)
    {
        if (string.IsNullOrWhiteSpace(driverName))
            throw new ArgumentException("El nombre del chofer es obligatorio.");

        driverName = string.Join(
            " ",
            driverName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );

        if (!driverName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            throw new ArgumentException("El nombre del chofer solo puede contener letras.");

        TextInfo textInfo = CultureInfo.GetCultureInfo("es-MX").TextInfo;

        driverName = textInfo.ToTitleCase(driverName.ToLower(CultureInfo.GetCultureInfo("es-MX")));

        if (driverName.Length > 40)
            throw new ArgumentException(
                "El nombre del chofer no puede exceder los 40 caracteres."
            );

        return driverName;
    }

    private static string NormalizeLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("Las placas son obligatorias.");

        licensePlate = licensePlate.Trim().ToUpperInvariant();

        if (licensePlate.Length > 10)
            throw new ArgumentException("Las placas no pueden exceder los 10 caracteres");

        return licensePlate;
    }

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