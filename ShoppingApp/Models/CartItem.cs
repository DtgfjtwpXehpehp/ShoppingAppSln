using SQLite;

namespace ShoppingApp.Models;

[Table("Cart_item")]
public class CartItem
{
    [PrimaryKey, AutoIncrement, Column("cart_item_id")]
    public int CartItemId { get; set; }

    /// <summary>FK -> Customer.customer_id</summary>
    [Column("customer_id")]
    public int CustomerId { get; set; }

    /// <summary>FK -> Product.product_id</summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }
}
