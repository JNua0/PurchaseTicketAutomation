namespace PurchaseTicket.Domain.Entities;

public class Supplier
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public Supplier(string name, string? phoneNumber = null)
    {
        Name = NormalizeName(name);
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
    }

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Supplier name is required.",
                nameof(name));
        }

        name = name.Trim();

        if (name.Length > 50)
        {
            throw new ArgumentException(
                "Supplier name cannot exceed 50 characters.",
                nameof(name));
        }

        return name;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        phoneNumber = phoneNumber.Trim();

        bool isNationalFormat =
            phoneNumber.Length == 10 &&
            phoneNumber.All(char.IsDigit);

        bool isInternationalFormat =
            phoneNumber.Length == 13 &&
            phoneNumber.StartsWith("+52") &&
            phoneNumber[3..].All(char.IsDigit);

        if (!isNationalFormat && !isInternationalFormat)
            throw new ArgumentException(
                "Phone number must contain 10 digits or +52 followed by 10 digits.",
                nameof(phoneNumber));

        return phoneNumber;
    }
}