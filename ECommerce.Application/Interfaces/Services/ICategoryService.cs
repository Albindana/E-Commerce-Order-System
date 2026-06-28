using ECommerce.Application.DTOs.Category;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryWithProductsDto> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryDto dto);
    Task DeleteAsync(Guid id);
}
