using APBD_9.Models;

namespace APBD_9.Repositories;

public interface IWarehouseRepository
{
    Task<int> DodajProduktAsync(WarehouseDataE filter, CancellationToken cancellationToken);
}