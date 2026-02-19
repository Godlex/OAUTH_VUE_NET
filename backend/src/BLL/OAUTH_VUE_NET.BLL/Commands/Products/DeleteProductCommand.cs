using MediatR;

namespace OAUTH_VUE_NET.BLL.Commands.Products;

public record DeleteProductCommand(int Id) : IRequest<bool>;
