namespace Pricing.Application.UseCases.Suppliers.Queries;
public interface IGetSuppliersPagedHandler
{
    Task<GetSuppliersPagedResponse> HandleAsync(GetSuppliersPagedQuery query, CancellationToken cancellationToken = default);
}