using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Mappings;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IECommerceMapper _mapper;

    public CategoryService(IUnitOfWork uow, IECommerceMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _uow.Categories.GetAllAsync();
        return _mapper.CategoriesToDto(categories);
    }

    public async Task<CategoryWithProductsDto> GetByIdAsync(Guid id)
    {
        var category = await _uow.Categories.GetByIdWithProductsAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        return _mapper.CategoryToWithProductsDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.CreateDtoToCategory(dto);
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return _mapper.CategoryToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        _mapper.UpdateDtoToCategory(dto, category);
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
        return _mapper.CategoryToDto(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        _uow.Categories.Delete(category);
        await _uow.SaveChangesAsync();
    }
}
