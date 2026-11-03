namespace ShoppingApp.Models;

/// <summary>One purchased line of a past order (read-only projection of Order_item + Product).</summary>
public class OrderLine
{
    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>The price paid at checkout (Order_item.unit_price), not the product's current price.</summary>
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}

/// <summary>A past order with its lines, for the order history screen.</summary>
public class OrderSummary
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderLine> Lines { get; set; } = new();

    public int ItemCount => Lines.Sum(l => l.Quantity);
    public string ItemCountText => ItemCount == 1 ? "1 item" : $"{ItemCount} items";
}
