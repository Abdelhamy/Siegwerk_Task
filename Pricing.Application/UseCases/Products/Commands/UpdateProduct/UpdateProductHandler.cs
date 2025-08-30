using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IUpdateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<UpdateProductHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateProductResponse?> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID {ProductId}", command.Id);

        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", command.Id);
            return null;
        }
        
        var sku = Sku.Create(command.Sku);
        var existingProduct = await _productRepository.GetBySkuAsync(sku, cancellationToken);
        if (existingProduct is not null && existingProduct.Id != command.Id)
        {
            throw new ArgumentException($"Product with SKU '{command.Sku}' already exists");
        }

        product.UpdateSku(sku);
        product.UpdateDetails(command.Name, command.UnitOfMeasure, command.HazardClass);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated product {ProductName} with SKU {Sku} (ID: {ProductId})", 
            product.Name, product.Sku.Value, product.Id);

        return new UpdateProductResponse(
            product.Id,
            product.Sku.Value,
            product.Name,
            product.UnitOfMeasure,
            product.HazardClass,
            product.IsHazardous
        );
    }
}