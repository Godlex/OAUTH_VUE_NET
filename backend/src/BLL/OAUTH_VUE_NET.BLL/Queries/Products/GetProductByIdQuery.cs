using MediatR;
using OAUTH_VUE_NET.BLL.DTOs;

namespace OAUTH_VUE_NET.BLL.Queries.Products;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
