using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<(IEnumerable<Order> Items, int TotalCount)> GetByUserIdPagedAsync(
        string userId, int page, int pageSize);

    Task<(IEnumerable<Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize);

    Task<Order?> GetByIdWithItemsAsync(Guid id);
    Task<int> GetNextOrderSequenceAsync();
}
