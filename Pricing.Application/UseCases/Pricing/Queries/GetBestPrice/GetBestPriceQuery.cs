using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;

public record GetBestPriceQuery(
    string Sku,
    int Qty,
    string Currency,
    DateOnly OnDate
);

public record GetBestPriceResponse(
    BestPriceResponse? BestPrice
);

public interface IGetBestPriceHandler
{
    Task<GetBestPriceResponse> HandleAsync(GetBestPriceQuery query, CancellationToken cancellationToken = default);
}