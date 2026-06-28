using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context) { }

    public async Task<Cart?> GetByUserIdAsync(string userId) =>
        await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart?> GetByUserIdWithItemsAsync(string userId) =>
        await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
}
