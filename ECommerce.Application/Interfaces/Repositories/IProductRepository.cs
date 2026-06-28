using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? categoryId, string? search,
        decimal? minPrice, decimal? maxPrice);

    Task<Product?> GetByIdWithCategoryAsync(Guid id);
}
