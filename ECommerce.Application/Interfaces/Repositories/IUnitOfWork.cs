namespace ECommerce.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
}
