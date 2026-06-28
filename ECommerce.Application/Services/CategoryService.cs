using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _uow.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryWithProductsDto> GetByIdAsync(Guid id)
    {
        var category = await _uow.Categories.GetByIdWithProductsAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        return _mapper.Map<CategoryWithProductsDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        _mapper.Map(dto, category);
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _uow.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found.");

        _uow.Categories.Delete(category);
        await _uow.SaveChangesAsync();
    }
}
