using MediatR;
using OAUTH_VUE_NET.BLL.DTOs;
using OAUTH_VUE_NET.BLL.Queries.Products;
using OAUTH_VUE_NET.Data.Repositories;

namespace OAUTH_VUE_NET.BLL.Handlers.Products;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Description, p.Price, p.Quantity, p.Category, p.CreatedAt, p.UpdatedAt));
    }
}
