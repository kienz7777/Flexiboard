namespace FlexiBoard.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<OrderItem> Products { get; set; } = new();
    public decimal Total { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Price * Quantity;
}
