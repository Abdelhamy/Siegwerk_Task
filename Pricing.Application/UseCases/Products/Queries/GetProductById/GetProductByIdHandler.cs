using Pricing.Application.Common.Interfaces;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces.Repositories;

namespace Pricing.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdHandler : IGetProductByIdHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IAppLogger<GetProductByIdHandler> _logger;

    public GetProductByIdHandler(
        IProductRepository productRepository,
        IAppLogger<GetProductByIdHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<GetProductByIdResponse> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving product with ID {ProductId}", query.Id);

        var product = await _productRepository.GetByIdAsync(query.Id, cancellationToken);
        
        if (product is null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", query.Id);
            return new GetProductByIdResponse(null);
        }

        var productDto = new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.UnitOfMeasure,
            product.HazardClass,
            product.IsHazardous
        );

        _logger.LogInformation("Retrieved product {ProductName} (ID: {ProductId})", product.Name, product.Id);

        return new GetProductByIdResponse(productDto);
    }
}