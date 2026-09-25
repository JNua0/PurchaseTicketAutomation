namespace PurchaseTicket.Application.UseCases.Materials.Update;

public record UpdateMaterialCommand(
    int MaterialId,
    string Name);