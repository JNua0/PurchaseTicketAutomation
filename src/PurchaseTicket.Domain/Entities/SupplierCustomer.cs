namespace PurchaseTicket.Domain.Entities;

public class SupplierCustomer
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public SupplierCustomer(string name)
    {
        Name = NormalizeName(name);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del proveedor o cliente es obligatorio.");

        name = name.Trim();

        if (name.Length > 50)
            throw new ArgumentException("El nombre del proveedor o cliente no puede exceder los 50 caracteres.");

        return name;
    }
}