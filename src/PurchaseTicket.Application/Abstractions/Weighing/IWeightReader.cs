namespace PurchaseTicket.Application.Abstractions.Weighing;

public interface IWeightReader
{
    Task<decimal> ReadStableWeightAsync();
}