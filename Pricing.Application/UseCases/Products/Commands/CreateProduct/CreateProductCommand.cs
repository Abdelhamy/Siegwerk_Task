namespace Pricing.Application.UseCases.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Sku,
    string Name,
    string UnitOfMeasure,
    string? HazardClass
);

public record CreateProductResponse(
    int Id,
    string Sku,
    string Name,
    string UnitOfMeasure,
    string? HazardClass
);