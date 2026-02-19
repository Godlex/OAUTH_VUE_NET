using MediatR;
using OAUTH_VUE_NET.BLL.Commands.Products;
using OAUTH_VUE_NET.BLL.DTOs;
using OAUTH_VUE_NET.Data.Repositories;

namespace OAUTH_VUE_NET.BLL.Handlers.Products;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IProductRepository _repository;

    public UpdateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null) return null;

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Price = request.Price;
        existing.Quantity = request.Quantity;
        existing.Category = request.Category;

        var updated = await _repository.UpdateAsync(existing, cancellationToken);

        return new ProductDto(
            updated.Id, updated.Name, updated.Description,
            updated.Price, updated.Quantity, updated.Category,
            updated.CreatedAt, updated.UpdatedAt);
    }
}
