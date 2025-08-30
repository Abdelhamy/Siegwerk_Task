

namespace Pricing.Application.UseCases.Suppliers.Queries.GetSupplierById;

public interface IGetSupplierByIdHandler
{
    Task<GetSupplierByIdResponse> HandleAsync(GetSupplierByIdQuery query, CancellationToken cancellationToken = default);
}