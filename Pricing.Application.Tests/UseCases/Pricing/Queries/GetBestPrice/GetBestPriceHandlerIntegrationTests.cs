using Pricing.Application.Common.Interfaces;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.Tests.UseCases.Pricing.Queries.GetBestPrice;

public class GetBestPriceHandlerIntegrationTests
{
    private readonly Mock<ISupplierRepository> _mockSupplierRepository;
    private readonly Mock<IRateProvider> _mockRateProvider;
    private readonly Mock<IAppLogger<GetBestPriceHandler>> _mockLogger;
    private readonly GetBestPriceHandler _handler;

    public GetBestPriceHandlerIntegrationTests()
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
    public async Task HandleAsync_WithDifferentPricesScenario_SelectsLowestPrice()
    {
        // Arrange
        var query = GetBestPriceTestDataHelper.Queries.Standard;
        var candidates = GetBestPriceTestDataHelper.Scenarios.DifferentPrices.AsReadOnly();

        SetupMocks(candidates);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.SupplierName.Should().Be("Cheap Supplier");
        result.BestPrice.UnitPrice.Should().Be(20.00m);
        result.BestPrice.SupplierId.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithSamePriceDifferentPreferenceScenario_SelectsPreferred()
    {
        // Arrange
        var query = GetBestPriceTestDataHelper.Queries.Standard;
        var candidates = GetBestPriceTestDataHelper.Scenarios.SamePriceDifferentPreference.AsReadOnly();

        SetupMocks(candidates);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.SupplierName.Should().Be("Preferred");
        result.BestPrice.SupplierPreferred.Should().BeTrue();
        result.BestPrice.SupplierId.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithSamePriceAndPreferenceDifferentLeadTimeScenario_SelectsFasterDelivery()
    {
        // Arrange
        var query = GetBestPriceTestDataHelper.Queries.Standard;
        var candidates = GetBestPriceTestDataHelper.Scenarios.SamePriceAndPreferenceDifferentLeadTime.AsReadOnly();

        SetupMocks(candidates);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.SupplierName.Should().Be("Fast Supplier");
        result.BestPrice.SupplierLeadTimeDays.Should().Be(3);
        result.BestPrice.SupplierId.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithDifferentCurrenciesScenario_ConvertsAndSelectsBest()
    {
        // Arrange
        var query = GetBestPriceTestDataHelper.Queries.Standard;
        var candidates = GetBestPriceTestDataHelper.Scenarios.DifferentCurrencies.AsReadOnly();

        _mockSupplierRepository
            .Setup(x => x.GetValidPriceCandidatesAsync(
                It.IsAny<Sku>(),
                It.IsAny<Quantity>(),
                It.IsAny<DateOnly>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _mockRateProvider.Setup(x => x.Convert(25.00m, "USD", "USD")).Returns(25.00m);
        _mockRateProvider.Setup(x => x.Convert(20.00m, "EUR", "USD")).Returns(22.00m);
        _mockRateProvider.Setup(x => x.Convert(2500.00m, "JPY", "USD")).Returns(18.50m);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.SupplierName.Should().Be("JPY Supplier");
        result.BestPrice.UnitPrice.Should().Be(18.50m);
        result.BestPrice.Currency.Should().Be("USD");
        result.BestPrice.SupplierId.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WithComplexRealWorldScenario_SelectsOptimalChoice()
    {
        // Arrange
        var query = GetBestPriceTestDataHelper.Queries.Standard;
        
        var candidates = new List<PriceCandidateDto>
        {
            GetBestPriceTestDataHelper.CreatePriceCandidate(
                1, 1, "Premium Fast Supplier", 
                supplierPreferred: true, 
                supplierLeadTimeDays: 1, 
                pricePerUom: 30.00m),
            
            GetBestPriceTestDataHelper.CreatePriceCandidate(
                2, 2, "Budget Slow Supplier", 
                supplierPreferred: false, 
                supplierLeadTimeDays: 30, 
                pricePerUom: 20.00m),
            
            GetBestPriceTestDataHelper.CreatePriceCandidate(
                3, 3, "Alternative Budget Supplier", 
                supplierPreferred: false, 
                supplierLeadTimeDays: 14, 
                pricePerUom: 20.00m)
        }.AsReadOnly();

        SetupMocks(candidates);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.BestPrice.Should().NotBeNull();
        result.BestPrice!.UnitPrice.Should().Be(20.00m);
        result.BestPrice.SupplierPreferred.Should().BeFalse();
        result.BestPrice.SupplierName.Should().Be("Alternative Budget Supplier");
        result.BestPrice.SupplierId.Should().Be(3);
        result.BestPrice.SupplierLeadTimeDays.Should().Be(14);
    }

    private void SetupMocks(IReadOnlyList<PriceCandidateDto> candidates)
    {
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
            .Setup(x => x.Convert(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<decimal, string, string>((amount, from, to) => 
                from == to ? amount : amount);
    }
}