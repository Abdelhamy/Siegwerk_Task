namespace Pricing.Application.UseCases.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id);

public record DeleteProductResponse(bool Success);

public interface IDeleteProductHandler
{
    Task<DeleteProductResponse> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default);
}