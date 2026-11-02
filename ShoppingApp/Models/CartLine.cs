namespace ShoppingApp.Models;

/// <summary>A cart row joined with its product, for display and checkout.</summary>
public class CartLine
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
