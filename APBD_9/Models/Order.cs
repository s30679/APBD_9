namespace APBD_9.Models;

public class Order
{
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public int Amount { get; set; }
    public DateTime CrearedAt { get; set; }
    public DateTime? FullfiledAt { get; set; }
}