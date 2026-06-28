using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetByUserIdPagedAsync(
        string userId, int page, int pageSize)
    {
        var query = _dbSet.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize)
    {
        var query = _dbSet.OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id) =>
        await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<int> GetNextOrderSequenceAsync() =>
        await _dbSet.CountAsync() + 1;
}
