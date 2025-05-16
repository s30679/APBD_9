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
    public async Task<bool> ProductExistsAsync(int ProductId, CancellationToken cancellationToken)
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken); 
        await using var com = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @id", con);
        com.Parameters.AddWithValue("@id", ProductId); 
        return await com.ExecuteScalarAsync(cancellationToken) != null;
    }
    public async Task<bool> WarehouseExistsAsync(int WarehouseId, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken); 
        await using var com = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @id", con); 
        com.Parameters.AddWithValue("@id", WarehouseId); 
        return await com.ExecuteScalarAsync(cancellationToken) != null;
    }
    public async Task<Order?> GetOrderAsync(int ProductId, int Amount, DateTime CreatedAt, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken); 
        await using var com = new SqlCommand(@"SELECT TOP 1 IdOrder, IdProduct, Amount, CreatedAt, FulfilledAt 
            FROM [Order]
            WHERE IdProduct = @pid AND Amount = @amount AND CreatedAt < @createdAt", con); 
        com.Parameters.AddWithValue("@pid", ProductId); 
        com.Parameters.AddWithValue("@amount", Amount); 
        com.Parameters.AddWithValue("@createdAt", CreatedAt);
        await using var reader = await com.ExecuteReaderAsync(cancellationToken); 
        if (await reader.ReadAsync(cancellationToken)) 
        { 
            return new Order 
            { 
                IdOrder = reader.GetInt32(reader.GetOrdinal("IdOrder")), 
                IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")), 
                Amount = reader.GetInt32(reader.GetOrdinal("Amount")), 
                CrearedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                FulfilledAt = reader.IsDBNull(reader.GetOrdinal("FulfilledAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FulfilledAt"))
            };
        } 
        return null;
        }
    public async Task<bool> OrderFulfilledAsync(int OrderId, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken); 
        await using var com = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @id", con); 
        com.Parameters.AddWithValue("@id", OrderId); 
        return await com.ExecuteScalarAsync(cancellationToken) != null;
    }
    public async Task<Product?> GetProductByIdAsync(int ProductId, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken); 
        await using var com = new SqlCommand("SELECT IdProduct, Name, Description, Price FROM Product WHERE IdProduct = @id", con); 
        com.Parameters.AddWithValue("@id", ProductId);
        await using var reader = await com.ExecuteReaderAsync(cancellationToken); 
        if (await reader.ReadAsync(cancellationToken)) 
        { 
            return new Product 
            { 
                IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")), 
                Name = reader.GetString(reader.GetOrdinal("Name")), 
                Description = reader.GetString(reader.GetOrdinal("Description")), 
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                
            };
        } 
        return null;
    }
    public async Task UpdateOrderFulfilledAtAsync(int OrderId, DateTime FulfilledAt, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken);
        await using var com = new SqlCommand("UPDATE [Order] SET FulfilledAt = @fulfilledAt WHERE IdOrder = @id", con); 
        com.Parameters.AddWithValue("@id", OrderId); 
        com.Parameters.AddWithValue("@fulfilledAt", FulfilledAt); 
        await com.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<int> AddProductToWarehouseAsync(int WarehouseId, int ProductId, int OrderId, int Amount, decimal Price, DateTime CreatedAt, CancellationToken cancellationToken) 
    { 
        await using var con = new SqlConnection(_connectionString);
        await con.OpenAsync(cancellationToken);
        await using var com = new SqlCommand(@"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@wid, @pid, @oid, @amount, @price, @createdAt);
            SELECT SCOPE_IDENTITY();", con);
        com.Parameters.AddWithValue("@wid", WarehouseId); 
        com.Parameters.AddWithValue("@pid", ProductId); 
        com.Parameters.AddWithValue("@oid", OrderId); 
        com.Parameters.AddWithValue("@amount", Amount); 
        com.Parameters.AddWithValue("@price", Price); 
        com.Parameters.AddWithValue("@createdAt", CreatedAt);
        var wynik = await com.ExecuteScalarAsync(cancellationToken); 
        return Convert.ToInt32(wynik);
    }
}