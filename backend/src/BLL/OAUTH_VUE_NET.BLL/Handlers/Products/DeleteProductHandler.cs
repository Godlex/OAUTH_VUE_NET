using MediatR;
using OAUTH_VUE_NET.BLL.Commands.Products;
using OAUTH_VUE_NET.Data.Repositories;

namespace OAUTH_VUE_NET.BLL.Handlers.Products;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        => await _repository.DeleteAsync(request.Id, cancellationToken);
}
