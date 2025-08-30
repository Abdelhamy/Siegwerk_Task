using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Products.Queries.GetProductsPaged;

public record GetProductsPagedQuery(
    PaginationRequest Pagination,
    SortRequest? Sort = null,
    string? SearchTerm = null,
    string? Name = null,
    string? Sku = null,
    string? UnitOfMeasure = null,
    string? HazardClass = null,
    bool? IsHazardous = null
);

public record GetProductsPagedResponse(
    PagedResult<ProductDto> Result
);



public interface IGetProductsPagedHandler
{
    Task<GetProductsPagedResponse> HandleAsync(GetProductsPagedQuery query, CancellationToken cancellationToken = default);
}