namespace OAUTH_VUE_NET.BLL.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
