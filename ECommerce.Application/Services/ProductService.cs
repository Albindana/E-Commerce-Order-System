using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, Guid? categoryId, string? search,
        decimal? minPrice, decimal? maxPrice)
    {
        var (items, total) = await _uow.Products.GetPagedAsync(
            page, pageSize, categoryId, search, minPrice, maxPrice);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _uow.Products.GetByIdWithCategoryAsync(id)
            ?? throw new NotFoundException($"Product {id} not found.");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException($"Category {dto.CategoryId} not found.");

        var product = _mapper.Map<Product>(dto);
        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();

        product.Category = category;
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _uow.Products.GetByIdWithCategoryAsync(id)
            ?? throw new NotFoundException($"Product {id} not found.");

        if (dto.CategoryId != product.CategoryId)
        {
            _ = await _uow.Categories.GetByIdAsync(dto.CategoryId)
                ?? throw new NotFoundException($"Category {dto.CategoryId} not found.");
        }

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product {id} not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
    }
}
