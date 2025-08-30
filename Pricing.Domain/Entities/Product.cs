using Pricing.Domain.Common;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public class Product : Entity<int>
{
    public Sku Sku { get; private set; }
    public string Name { get; private set; }
    public string UnitOfMeasure { get; private set; }
    public string? HazardClass { get; private set; }

    private Product() { } // EF Constructor

    private Product(int id, Sku sku, string name, string unitOfMeasure, string? hazardClass) : base(id)
    {
        Sku = sku;
        Name = name;
        UnitOfMeasure = unitOfMeasure;
        HazardClass = hazardClass;
    }

    public static Product Create(Sku sku, string name, string unitOfMeasure = "EA", string? hazardClass = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be null or empty", nameof(unitOfMeasure));

        if (unitOfMeasure.Length > 10)
            throw new ArgumentException("Unit of measure cannot exceed 10 characters", nameof(unitOfMeasure));

        if (hazardClass?.Length > 50)
            throw new ArgumentException("Hazard class cannot exceed 50 characters", nameof(hazardClass));

        return new Product(
            0, // Let database generate ID
            sku,
            name.Trim(),
            unitOfMeasure.Trim().ToUpperInvariant(),
            hazardClass?.Trim());
    }

    public void UpdateDetails(string name, string unitOfMeasure, string? hazardClass)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be null or empty", nameof(unitOfMeasure));

        if (unitOfMeasure.Length > 10)
            throw new ArgumentException("Unit of measure cannot exceed 10 characters", nameof(unitOfMeasure));

        if (hazardClass?.Length > 50)
            throw new ArgumentException("Hazard class cannot exceed 50 characters", nameof(hazardClass));

        Name = name.Trim();
        UnitOfMeasure = unitOfMeasure.Trim().ToUpperInvariant();
        HazardClass = hazardClass?.Trim();
    }

    public void UpdateHazardClass(string? hazardClass)
    {
        if (hazardClass?.Length > 50)
            throw new ArgumentException("Hazard class cannot exceed 50 characters", nameof(hazardClass));

        HazardClass = hazardClass?.Trim();
    }
    public void UpdateSku(Sku sku)
    {
        Sku = sku;
    }

    public bool IsHazardous => !string.IsNullOrEmpty(HazardClass);
}