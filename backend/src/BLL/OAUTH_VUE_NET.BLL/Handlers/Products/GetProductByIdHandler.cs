using MediatR;
using OAUTH_VUE_NET.BLL.DTOs;
using OAUTH_VUE_NET.BLL.Queries.Products;
using OAUTH_VUE_NET.Data.Repositories;

namespace OAUTH_VUE_NET.BLL.Handlers.Products;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return null;

        return new ProductDto(
            product.Id, product.Name, product.Description,
            product.Price, product.Quantity, product.Category,
            product.CreatedAt, product.UpdatedAt);
    }
}
