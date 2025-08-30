using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.PriceLists.Queries.GetPricesPagedQuery;

public record GetPricesPagedQuery(
    PaginationRequest Pagination,
    SortRequest? Sort = null,
    string? Sku = null,
    int? Quantity = null,
    DateOnly? ValidOn = null,
    string? Currency = null,
    int? SupplierId = null
);

public record GetPricesPagedResponse(
    PagedResult<PriceCandidateDto> Result
);

public interface IGetPricesPagedHandler
{
    Task<GetPricesPagedResponse> HandleAsync(GetPricesPagedQuery query, CancellationToken cancellationToken = default);
}