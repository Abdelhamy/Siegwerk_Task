using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;

namespace Pricing.Application.UseCases.Products.Commands.DeleteProduct;

public class DeleteProductHandler : IDeleteProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<DeleteProductHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteProductResponse> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}", command.Id);

        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", command.Id);
            return new DeleteProductResponse(false);
        }

        _productRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted product {ProductName} with SKU {Sku} (ID: {ProductId})", 
            product.Name, product.Sku, product.Id);

        return new DeleteProductResponse(true);
    }
}