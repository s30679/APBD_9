using APBD_9.Models;
using APBD_9.Services;
using Microsoft.Data.SqlClient;

namespace APBD_9.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly string _connectionString;
    public WarehouseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("db-mssql");
    }
    public async Task<int> DodajProduktAsync(WarehouseDataE filter, CancellationToken cancellationToken)
    {
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken);
        var transaction = await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var com = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @id", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@id", filter.ProductId);
            //1
            if(await com.ExecuteScalarAsync(cancellationToken)==null)
            {
                throw new Exception("Produkt nie istnieje");
            }
            com = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @id", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@id", filter.WarehouseId);
            if (await com.ExecuteScalarAsync(cancellationToken)==null)
                throw new Exception("Magazyn nie istnieje");
            
            //2
            com = new SqlCommand(@"
                SELECT TOP 1 * FROM [Order]
                WHERE IdProduct = @pid AND Amount = @amount AND CreatedAt < @created", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@pid", filter.ProductId);
            com.Parameters.AddWithValue("@amount", filter.Amount);
            com.Parameters.AddWithValue("@created", filter.CreatedAt);
            var czy_istnieje_zamowienie = await com.ExecuteReaderAsync(cancellationToken);
            if (!await czy_istnieje_zamowienie.ReadAsync(cancellationToken))
            {
                await czy_istnieje_zamowienie.CloseAsync();
                throw new Exception("Nie znaleziono zamówienia");
            }
            int orderId = (int)czy_istnieje_zamowienie["IdOrder"];
            await czy_istnieje_zamowienie.CloseAsync();
            
            //3
            com = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @id", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@id", orderId);
            if (await com.ExecuteScalarAsync(cancellationToken) != null)
                throw new Exception("Zamówienie zostało już zrealizowane");
            
            //4
            com = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @id", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@id", orderId);
            await com.ExecuteNonQueryAsync(cancellationToken);
            
            //5
            com = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @id", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@id", filter.ProductId);
            decimal cena = (decimal)(await com.ExecuteScalarAsync(cancellationToken));
            decimal calkowita_cena = cena * filter.Amount;
            com = new SqlCommand(@"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                VALUES (@wid, @pid, @oid, @amount, @price, GETDATE());
                SELECT SCOPE_IDENTITY();", con, (SqlTransaction)transaction);
            com.Parameters.AddWithValue("@wid", filter.WarehouseId);
            com.Parameters.AddWithValue("@pid", filter.ProductId);
            com.Parameters.AddWithValue("@oid", orderId);
            com.Parameters.AddWithValue("@amount", filter.Amount);
            com.Parameters.AddWithValue("@price", calkowita_cena);

            //6
            var odpowiedz = await com.ExecuteScalarAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Convert.ToInt32(odpowiedz);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}