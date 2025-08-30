namespace Pricing.Application.UseCases.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    string Sku,
    string Name,
    string UnitOfMeasure,
    string? HazardClass
);

public record UpdateProductResponse(
    int Id,
    string Sku,
    string Name,
    string UnitOfMeasure,
    string? HazardClass,
    bool IsHazardous
);

public interface IUpdateProductHandler
{
    Task<UpdateProductResponse?> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default);
}