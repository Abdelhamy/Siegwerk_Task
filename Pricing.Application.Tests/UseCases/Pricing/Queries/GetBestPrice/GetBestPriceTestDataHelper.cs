using Pricing.Application.Contracts;
using Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;

namespace Pricing.Application.Tests.UseCases.Pricing.Queries.GetBestPrice;

public static class GetBestPriceTestDataHelper
{
    public static PriceCandidateDto CreatePriceCandidate(
        int id = 1,
        int supplierId = 1,
        string supplierName = "Test Supplier",
        bool supplierPreferred = false,
        int supplierLeadTimeDays = 5,
        string sku = "SKU-001",
        decimal pricePerUom = 25.00m,
        string currency = "USD",
        int minQty = 10,
        DateOnly? validFrom = null,
        DateOnly? validTo = null)
    {
        return new PriceCandidateDto(
            Id: id,
            SupplierId: supplierId,
            SupplierName: supplierName,
            SupplierPreferred: supplierPreferred,
            SupplierLeadTimeDays: supplierLeadTimeDays,
            Sku: sku,
            PricePerUom: pricePerUom,
            Currency: currency,
            MinQty: minQty,
            ValidFrom: validFrom ?? new DateOnly(2025, 1, 1),
            ValidTo: validTo ?? new DateOnly(2025, 12, 31));
    }

    public static List<PriceCandidateDto> CreateMultipleCandidates(
        int count,
        Func<int, PriceCandidateDto>? candidateFactory = null)
    {
        candidateFactory ??= (i => CreatePriceCandidate(
            id: i,
            supplierId: i,
            supplierName: $"Supplier {i}",
            pricePerUom: 20.00m + i));

        return Enumerable.Range(1, count)
            .Select(candidateFactory)
            .ToList();
    }

    public static class Scenarios
    {

        public static List<PriceCandidateDto> DifferentPrices => new()
        {
            CreatePriceCandidate(1, 1, "Expensive Supplier", pricePerUom: 30.00m),
            CreatePriceCandidate(2, 2, "Cheap Supplier", pricePerUom: 20.00m),
            CreatePriceCandidate(3, 3, "Medium Supplier", pricePerUom: 25.00m)
        };

     
        public static List<PriceCandidateDto> SamePriceDifferentPreference => new()
        {
            CreatePriceCandidate(1, 1, "Non-Preferred", supplierPreferred: false),
            CreatePriceCandidate(2, 2, "Preferred", supplierPreferred: true)
        };

    
        public static List<PriceCandidateDto> SamePriceAndPreferenceDifferentLeadTime => new()
        {
            CreatePriceCandidate(1, 1, "Slow Supplier", supplierPreferred: true, supplierLeadTimeDays: 10),
            CreatePriceCandidate(2, 2, "Fast Supplier", supplierPreferred: true, supplierLeadTimeDays: 3)
        };

     
        public static List<PriceCandidateDto> IdenticalExceptSupplierId => new()
        {
            CreatePriceCandidate(1, 5, "Supplier E", supplierPreferred: true, supplierLeadTimeDays: 5),
            CreatePriceCandidate(2, 2, "Supplier B", supplierPreferred: true, supplierLeadTimeDays: 5)
        };

     
        public static List<PriceCandidateDto> DifferentCurrencies => new()
        {
            CreatePriceCandidate(1, 1, "USD Supplier", currency: "USD", pricePerUom: 25.00m),
            CreatePriceCandidate(2, 2, "EUR Supplier", currency: "EUR", pricePerUom: 20.00m),
            CreatePriceCandidate(3, 3, "JPY Supplier", currency: "JPY", pricePerUom: 2500.00m)
        };

      
        public static List<PriceCandidateDto> EdgeCases => new()
        {
            CreatePriceCandidate(1, 1, "Zero Price", pricePerUom: 0.00m),
            CreatePriceCandidate(2, 2, "Very Small Price", pricePerUom: 0.0001m),
            CreatePriceCandidate(3, 3, "Very Large Price", pricePerUom: 999999.99m),
            CreatePriceCandidate(4, 4, "Negative Lead Time", supplierLeadTimeDays: -1),
            CreatePriceCandidate(5, 5, "No Expiry", validTo: null)
        };
    }

    public static class Queries
    {
        public static GetBestPriceQuery Standard => new("SKU-001", 10, "USD", new DateOnly(2025, 1, 15));
        
        public static GetBestPriceQuery WithHighQuantity => new("SKU-001", 1000, "USD", new DateOnly(2025, 1, 15));
        
        public static GetBestPriceQuery WithDifferentCurrency => new("SKU-001", 10, "EUR", new DateOnly(2025, 1, 15));
        
        public static GetBestPriceQuery WithDifferentDate => new("SKU-001", 10, "USD", new DateOnly(2025, 6, 15));
    }
}