using APBD_9.Models;
using APBD_9.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    public WarehouseController(IWarehouseService service)
    {
        _warehouseService = service;
    }
    [HttpPost("dodaj")]
    public async Task<IActionResult> DodajProdukt([FromQuery] WarehouseDataE filter, CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseService.DodajProduktAsync(filter, cancellationToken);
        return Ok(new {InsertedId = warehouses});   
    }
}