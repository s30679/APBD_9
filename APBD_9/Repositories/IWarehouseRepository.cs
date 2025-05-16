using APBD_9.Models;

namespace APBD_9.Repositories;

public interface IWarehouseRepository
{
    Task<bool> ProductExistsAsync(int ProductId, CancellationToken cancellationToken);
    Task<bool> WarehouseExistsAsync(int WarehouseId, CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(int ProductId, int Amount, DateTime CreatedAt, CancellationToken cancellationToken);
    Task<bool> OrderFulfilledAsync(int OrderId, CancellationToken cancellationToken);
    Task<Product?> GetProductByIdAsync(int ProductId, CancellationToken cancellationToken);
    Task<int> AddProductToWarehouseAsync(int WarehouseId, int ProductId, int OrderId, int Amount, decimal Price, DateTime CreatedAt, CancellationToken cancellationToken);
    Task UpdateOrderFulfilledAtAsync(int OrderId, DateTime FulfilledAt, CancellationToken cancellationToken);
}