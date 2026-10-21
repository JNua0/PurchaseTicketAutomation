using PurchaseTicket.Application.Abstractions.Weighing;

namespace PurchaseTicket.Application.UseCases.Weighing.ReadWeight;

public sealed class ReadWeightUseCase
{
    private readonly IWeightReader _weightReader;

    public ReadWeightUseCase(IWeightReader weightReader)
    {
        _weightReader = weightReader;
    }

    public async Task<decimal> ExecuteAsync()
    {
        return await Task.Run(
            async () => await _weightReader.ReadStableWeightAsync());
    }
}
