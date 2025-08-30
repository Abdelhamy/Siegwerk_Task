namespace Pricing.Application.UseCases.Suppliers.Commands.DeleteSupplier;

public interface IDeleteSupplierHandler
{
    Task<DeleteSupplierResponse> HandleAsync(DeleteSupplierCommand command, CancellationToken cancellationToken = default);
}