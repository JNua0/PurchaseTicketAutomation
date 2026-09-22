namespace PurchaseTicket.Domain.Entities;

public class Supplier
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public Supplier(string name)
    {
        Name = NormalizeName(name);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Supplier name is required.");

        name = name.Trim();

        if (name.Length > 50)
            throw new ArgumentException(
                "Supplier name cannot exceed 50 characters.");

        return name;
    }
}