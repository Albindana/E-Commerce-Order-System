using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, Guid? categoryId, string? search,
        decimal? minPrice, decimal? maxPrice);

    Task<ProductDto> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto);
    Task DeleteAsync(Guid id);
}
