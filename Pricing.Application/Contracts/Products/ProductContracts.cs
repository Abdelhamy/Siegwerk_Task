namespace Pricing.Application.Contracts;

public record ProductModel
{
    public string? Sku { get; init; }
    public string? Name { get; init; }
    public string? UnitOfMeasure { get; init; }
    public string? HazardClass { get; init; }
    public bool IsHazardous => !string.IsNullOrWhiteSpace(HazardClass);
}

public record ProductDto(
    int Id,
    string Sku,
    string Name,
    string UnitOfMeasure,
    string? HazardClass,
    bool IsHazardous
);