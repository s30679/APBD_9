using APBD_9.Models;
using APBD_9.Repositories;
using Microsoft.Data.SqlClient;

namespace APBD_9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly string _connectionString;
    public WarehouseService(IWarehouseRepository repository, IConfiguration configuration)
    {
        _warehouseRepository = repository;
        _connectionString = configuration.GetConnectionString("db-mssql");
    }
    public async Task<int> DodajProduktAsync(WarehouseDataE filter, CancellationToken cancellationToken)
        {
            //1
            if (filter.Amount <= 0)
            {
                throw new ArgumentException("Wartość ilości musi być większa niż 0");
            }
            if (!await _warehouseRepository.ProductExistsAsync(filter.ProductId, cancellationToken))
            {
                throw new ArgumentException("Produkt o takim ID nie istnieje");
            }
            if (!await _warehouseRepository.WarehouseExistsAsync(filter.WarehouseId, cancellationToken))
            {
                throw new ArgumentException("Magazyn o o takim ID nie istnieje");
            }

            //2
            var order =await _warehouseRepository.GetOrderAsync(filter.ProductId, filter.Amount, filter.CreatedAt, cancellationToken);
            if(order==null)
            {
                throw new InvalidOperationException("Nie znaleziono odpowiedniego zamówienia dla produktu i ilości");
            }

            //3
            if (await _warehouseRepository.OrderFulfilledAsync(order.IdOrder, cancellationToken))
            {
                throw new InvalidOperationException("Zamówienie o takim ID zostało już zrealizowane.");
            }
            var wybrany_produkt = await _warehouseRepository.GetProductByIdAsync(filter.ProductId, cancellationToken);
            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync(cancellationToken);
            await using var transaction = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
            try
            {
                //4
                var updateOrderCommand = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @id", con, transaction);
                updateOrderCommand.Parameters.AddWithValue("@id", order.IdOrder);
                await updateOrderCommand.ExecuteNonQueryAsync(cancellationToken);
                
                //5
                decimal koszt_calkowity = wybrany_produkt.Price * filter.Amount;
                DateTime czas_teraz = DateTime.UtcNow;

                var com = new SqlCommand(@"
                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    OUTPUT INSERTED.IdProductWarehouse
                    VALUES (@wid, @pid, @oid, @amount, @price, @createdAt);", con, transaction);
                com.Parameters.AddWithValue("@wid", filter.WarehouseId);
                com.Parameters.AddWithValue("@pid", filter.ProductId);
                com.Parameters.AddWithValue("@oid", order.IdOrder);
                com.Parameters.AddWithValue("@amount", filter.Amount);
                com.Parameters.AddWithValue("@price", koszt_calkowity);
                com.Parameters.AddWithValue("@createdAt", czas_teraz);

                var id = (int)await com.ExecuteScalarAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
}