using Pricing.Application.Contracts;

namespace Pricing.Application.UseCases.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(int SupplierId);

public record GetSupplierByIdResponse(SupplierDto? Supplier);
