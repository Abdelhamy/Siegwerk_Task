using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Suppliers.Queries;

public record GetSuppliersPagedQuery(
    PaginationRequest Pagination,
    SortRequest? Sort = null,
    string? SearchTerm = null,
    string? Name = null,
    string? Country = null,
    bool? Preferred = null,
    int? MinLeadTime = null,
    int? MaxLeadTime = null
);

public record GetSuppliersPagedResponse(
    PagedResult<SupplierDto> Result
);

