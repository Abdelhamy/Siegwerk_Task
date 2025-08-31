using Pricing.Application.Common.Interfaces;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.Tests.UseCases.Pricing.Queries.GetBestPrice;

public class GetBestPriceHandlerTests
{
    private readonly Mock<ISupplierRepository> _mockSupplierRepository;
    private readonly Mock<IRateProvider> _mockRateProvider;
    private readonly Mock<IAppLogger<GetBestPriceHandler>> _mockLogger;
    private readonly GetBestPriceHandler _handler;

    public GetBestPriceHandlerTests()
    {
        _mockSupplierRepository = new Mock<ISupplierRepository>();
        _mockRateProvider = new Mock<IRateProvider>();
        _mockLogger = new Mock<IAppLogger<GetBestPriceHandler>>();
        
        _handler = new GetBestPriceHandler(
            _mockSupplierRepository.Object,
            _mockRateProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCandidatesFound_ReturnsNullResponse()
    {
        // Arrange
        var query = new GetBestPriceQuery("SKU-001", 10, "USD", new DateOnly(2025, 1, 15));
        var emptyCandidates = new List<PriceCandidateDto>().AsReadOnly();

        _mockSupplierRepository
            .Setup(x => x.GetValidPriceCandidatesAsync(
                It.IsAny<Sku>(),
                It.IsAny<Quantity>(),
                It.IsAny<DateOnly>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCandidates);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithSingleCandidate_ReturnsCorrectBestPrice()
    {
        // Arrange
        var query = new GetBestPriceQuery("SKU-001", 10, "USD", new DateOnly(2025, 1, 15));
        
        var candidates = new List<PriceCandidateDto>
        {
            new PriceCandidateDto(
                Id: 1,
                SupplierId: 1,
                SupplierName: "Supplier A",
                SupplierPreferred: true,
                SupplierLeadTimeDays: 5,
                Sku: "SKU-001",
                PricePerUom: 25.50m,
                Currency: "USD",
                MinQty: 10,
                ValidFrom: new DateOnly(2025, 1, 1),
                ValidTo: new DateOnly(2025, 12, 31))
        }.AsReadOnly();

        _mockSupplierRepository
            .Setup(x => x.GetValidPriceCandidatesAsync(
                It.IsAny<Sku>(),
                It.IsAny<Quantity>(),
                It.IsAny<DateOnly>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _mockRateProvider
            .Setup(x => x.Convert(25.50m, "USD", "USD"))
            .Returns(25.50m);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.Sku.Should().Be("SKU-001");
        result.BestPrice.Qty.Should().Be(10);
        result.BestPrice.Currency.Should().Be("USD");
        result.BestPrice.UnitPrice.Should().Be(25.50m);
        result.BestPrice.Total.Should().Be(255.00m);
        result.BestPrice.SupplierId.Should().Be(1);
        result.BestPrice.SupplierName.Should().Be("Supplier A");
        result.BestPrice.SupplierPreferred.Should().BeTrue();
        result.BestPrice.SupplierLeadTimeDays.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleCandidates_SelectsLowestUnitPrice()
    {
        // Arrange
        var query = new GetBestPriceQuery("SKU-001", 10, "USD", new DateOnly(2025, 1, 15));
        
        var candidates = new List<PriceCandidateDto>
        {
            new PriceCandidateDto(1, 1, "Supplier A", false, 7, "SKU-001", 30.00m, "USD", 10, 
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)),
            new PriceCandidateDto(2, 2, "Supplier B", true, 5, "SKU-001", 25.00m, "USD", 10, 
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)),
            new PriceCandidateDto(3, 3, "Supplier C", false, 3, "SKU-001", 28.00m, "USD", 10, 
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31))
        }.AsReadOnly();

        _mockSupplierRepository
            .Setup(x => x.GetValidPriceCandidatesAsync(
                It.IsAny<Sku>(),
                It.IsAny<Quantity>(),
                It.IsAny<DateOnly>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _mockRateProvider
            .Setup(x => x.Convert(It.IsAny<decimal>(), "USD", "USD"))
            .Returns<decimal, string, string>((amount, from, to) => amount);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.SupplierId.Should().Be(2);
        result.BestPrice.SupplierName.Should().Be("Supplier B");
        result.BestPrice.UnitPrice.Should().Be(25.00m);
        result.BestPrice.Total.Should().Be(250.00m);
    }

    [Fact]
    public async Task HandleAsync_WithCurrencyConversion_ConvertsCorrectly()
    {
        // Arrange
        var query = new GetBestPriceQuery("SKU-001", 10, "USD", new DateOnly(2025, 1, 15));
        
        var candidates = new List<PriceCandidateDto>
        {
            new PriceCandidateDto(1, 1, "Supplier A", false, 5, "SKU-001", 20.00m, "EUR", 10, 
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31))
        }.AsReadOnly();

        _mockSupplierRepository
            .Setup(x => x.GetValidPriceCandidatesAsync(
                It.IsAny<Sku>(),
                It.IsAny<Quantity>(),
                It.IsAny<DateOnly>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _mockRateProvider
            .Setup(x => x.Convert(20.00m, "EUR", "USD"))
            .Returns(22.00m);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.UnitPrice.Should().Be(22.00m);
        result.BestPrice.Total.Should().Be(220.00m);
        result.BestPrice.Currency.Should().Be("USD");

        _mockRateProvider.Verify(x => x.Convert(20.00m, "EUR", "USD"), Times.Once);
    }
}