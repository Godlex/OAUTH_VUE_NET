using MediatR;
using OAUTH_VUE_NET.BLL.Commands.Products;
using OAUTH_VUE_NET.BLL.DTOs;
using OAUTH_VUE_NET.Data.Entities;
using OAUTH_VUE_NET.Data.Repositories;

namespace OAUTH_VUE_NET.BLL.Handlers.Products;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Quantity = request.Quantity,
            Category = request.Category
        };

        var created = await _repository.CreateAsync(product, cancellationToken);

        return new ProductDto(
            created.Id, created.Name, created.Description,
            created.Price, created.Quantity, created.Category,
            created.CreatedAt, created.UpdatedAt);
    }
}
