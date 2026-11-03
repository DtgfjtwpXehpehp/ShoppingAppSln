using SQLite;

namespace ShoppingApp.Models;

[Table("Order_item")]
public class OrderItem
{
    [PrimaryKey, AutoIncrement, Column("order_item_id")]
    public int OrderItemId { get; set; }

    /// <summary>FK -> Order.order_id</summary>
    [Column("order_id")]
    public int OrderId { get; set; }

    /// <summary>FK -> Product.product_id</summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>Price paid per unit at the moment of checkout (snapshot).</summary>
    [Column("unit_price")]
    public decimal UnitPrice { get; set; }
}
