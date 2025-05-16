using APBD_9.Models;

namespace APBD_9.Services;

public interface IWarehouseService
{
    Task<int> DodajProduktAsync(WarehouseDataE filter,CancellationToken cancellationToken);
}