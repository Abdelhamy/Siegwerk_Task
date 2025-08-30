using Pricing.Application.Common.Interfaces;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Api.Endpoints.Development;

public static class DevEndpoints
{
    public static void MapDevelopmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dev")
            .WithTags("Development")
            .WithOpenApi();

        group.MapPost("/seed", SeedDatabase)
            .WithName("SeedDatabase");

    }

    private static async Task<IResult> SeedDatabase(
        PricingDbContext dbContext,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting database seeding operation...");

            var existingSuppliers = await dbContext.Suppliers.CountAsync(cancellationToken);
            if (existingSuppliers > 0)
            {
                logger.LogInformation("Database already contains {Count} suppliers. Skipping seed operation.", existingSuppliers);
                return Results.BadRequest(new
                {
                    success = false,
                    message = "Database already contains data. Use /dev/clear to reset first.",
                    currentData = await GetEntityCounts(dbContext, cancellationToken)
                });
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create 10 suppliers
                var suppliers = CreateSampleSuppliers();
                await dbContext.Suppliers.AddRangeAsync(suppliers, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Created {Count} suppliers", suppliers.Length);

                // Create 10 products
                var products = CreateSampleProducts();
                await dbContext.Products.AddRangeAsync(products, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Created {Count} products", products.Length);

                var priceEntries = CreateSamplePriceEntries(suppliers, products);
                await dbContext.PriceListEntries.AddRangeAsync(priceEntries, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Created {Count} price entries", priceEntries.Length);

                await unitOfWork.CommitTransactionAsync(cancellationToken);

                var entityCounts = await GetEntityCounts(dbContext, cancellationToken);
                logger.LogInformation("Database seeded successfully: {Suppliers} suppliers, {Products} products, {PriceEntries} price entries",
                    entityCounts.Suppliers, entityCounts.Products, entityCounts.PriceEntries);

                return Results.Ok(new
                {
                    success = true,
                    message = "Database seeded successfully with sample data using DDD patterns",
                    data = entityCounts
                });
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding database");
            return Results.Problem(
                title: "Database Seeding Failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

  
    private static Supplier[] CreateSampleSuppliers()
    {
        return new[]
        {
            Supplier.Create("ACME Corporation", "US", true, LeadTime.Create(2)),
            Supplier.Create("DeltaChem Ltd", "EG", false, LeadTime.Create(1)),
            Supplier.Create("EuroSupply GmbH", "DE", true, LeadTime.Create(3)),
            Supplier.Create("ChemTrade Inc", "US", false, LeadTime.Create(5)),
            Supplier.Create("Alpine Chemicals", "CH", true, LeadTime.Create(4)),
            Supplier.Create("Nordic Supply Co", "NO", false, LeadTime.Create(6)),
            Supplier.Create("MediterraneanChem", "IT", true, LeadTime.Create(3)),
            Supplier.Create("Asia Pacific Ltd", "SG", false, LeadTime.Create(8)),
            Supplier.Create("British Materials", "GB", true, LeadTime.Create(2)),
            Supplier.Create("Canadian Chemicals", "CA", false, LeadTime.Create(7))
        };
    }

    private static Product[] CreateSampleProducts()
    {

        return new[]
        {
            Product.Create(Sku.Create("SKU-1001"), "Industrial Solvent A", "L", "Flammable"),
            Product.Create(Sku.Create("SKU-1002"), "Chemical Compound B", "KG", "Corrosive"),
            Product.Create(Sku.Create("SKU-1003"), "Polymer Base C", "KG"),
            Product.Create(Sku.Create("SKU-1004"), "Cleaning Agent D", "L", "Hazardous"),
            Product.Create(Sku.Create("SKU-1005"), "Synthetic Resin E", "KG"),
            Product.Create(Sku.Create("SKU-1006"), "Metal Coating F", "L", "Toxic"),
            Product.Create(Sku.Create("SKU-1007"), "Adhesive Gel G", "KG", "Flammable"),
            Product.Create(Sku.Create("SKU-1008"), "Protective Film H", "M2"),
            Product.Create(Sku.Create("SKU-1009"), "Catalyst Powder I", "KG", "Reactive"),
            Product.Create(Sku.Create("SKU-1010"), "Lubricant Oil J", "L")
        };
    }

    private static PriceListEntry[] CreateSamplePriceEntries(Supplier[] suppliers, Product[] products)
    {
        var entries = new List<PriceListEntry>();

        // Create diverse price entries across suppliers and products
        var random = new Random(42); // Fixed seed for consistent results

        // Add price entries for each product from multiple suppliers
        for (int productIndex = 0; productIndex < products.Length; productIndex++)
        {
            var product = products[productIndex];
            
            // Each product will have 2-4 price entries from different suppliers
            var supplierCount = random.Next(2, 5);
            var selectedSuppliers = suppliers.OrderBy(x => random.Next()).Take(supplierCount).ToArray();

            foreach (var supplier in selectedSuppliers)
            {
                // Generate realistic pricing data
                var basePrice = 10m + (productIndex * 5m) + random.Next(1, 20);
                var currency = random.Next(3) switch
                {
                    0 => Currency.USD,
                    1 => Currency.EUR,
                    _ => Currency.EGP
                };

                // Adjust price based on currency
                var price = currency.Code switch
                {
                    "USD" => basePrice,
                    "EUR" => basePrice * 0.85m,
                    "EGP" => basePrice * 30m,
                    _ => basePrice
                };

                var minQuantity = random.Next(1, 5) * 5; // 5, 10, 15, 20
                
                // Create different validity periods
                var startDate = new DateOnly(2025, random.Next(1, 7), 1); // Start between Jan-Jun
                var hasEndDate = random.Next(2) == 0;
                var endDate = hasEndDate ? startDate.AddMonths(random.Next(3, 12)) : (DateOnly?)null;

                var dateRange = DateRange.Create(startDate, endDate);
                var money = Money.Create(Math.Round(price, 2), currency);
                var quantity = Quantity.Create(minQuantity);

                entries.Add(PriceListEntry.Create(
                    supplier.Id,
                    product.Sku,
                    dateRange,
                    money,
                    quantity
                ));
            }
        }

        return entries.ToArray();
    }

    private static async Task<EntityCounts> GetEntityCounts(PricingDbContext dbContext, CancellationToken cancellationToken)
    {
        return new EntityCounts
        {
            Suppliers = await dbContext.Suppliers.CountAsync(cancellationToken),
            Products = await dbContext.Products.CountAsync(cancellationToken),
            PriceEntries = await dbContext.PriceListEntries.CountAsync(cancellationToken)
        };
    }

    private record EntityCounts
    {
        public int Suppliers { get; init; }
        public int Products { get; init; }
        public int PriceEntries { get; init; }
    }
}