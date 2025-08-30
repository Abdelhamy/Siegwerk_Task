using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductHandler : ICreateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<CreateProductHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateProductResponse> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating new product: {ProductName}", command.Name);

            var sku = Sku.Create(command.Sku);
            var existingProduct = await _productRepository.GetBySkuAsync(sku, cancellationToken);
            if (existingProduct is not null)
            {
                throw new InvalidOperationException($"Product with SKU '{command.Sku}' already exists.");
            }

            var product = Product.Create(sku, command.Name, command.UnitOfMeasure, command.HazardClass);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new product: {ProductName} ({ProductId})", 
                product.Name, product.Id);

            return new CreateProductResponse(
                product.Id,
                product.Sku.Value,
                product.Name,
                product.UnitOfMeasure,
                product.HazardClass
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", command.Name);
            throw;
        }
    }
}