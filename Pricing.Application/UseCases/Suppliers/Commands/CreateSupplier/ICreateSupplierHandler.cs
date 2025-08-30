namespace Pricing.Application.UseCases.Suppliers.Commands.CreateSupplier;

public interface ICreateSupplierHandler
{
    Task<CreateSupplierResponse> HandleAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default);
}