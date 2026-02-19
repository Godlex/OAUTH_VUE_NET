using MediatR;
using OAUTH_VUE_NET.BLL.DTOs;

namespace OAUTH_VUE_NET.BLL.Commands.Products;

public record UpdateProductCommand(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    string Category
) : IRequest<ProductDto?>;
