using APBD_9.Models;
using APBD_9.Repositories;

namespace APBD_9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    public WarehouseService(IWarehouseRepository repository)
    {
        _warehouseRepository = repository;
    }
    public async Task<int> DodajProduktAsync(WarehouseDataE filter, CancellationToken cancellationToken)
    {
        if (filter.Amount <= 0)
        {
            throw new Exception("Wartość ilości musi być większa niż 0");
        }
        var dodaj = await _warehouseRepository.DodajProduktAsync(filter, cancellationToken);
        return dodaj;
    }
}