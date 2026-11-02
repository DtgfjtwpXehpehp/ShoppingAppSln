using SQLite;

namespace ShoppingApp.Models;

[Table("Order")]
public class Order
{
    [PrimaryKey, AutoIncrement, Column("order_id")]
    public int OrderId { get; set; }

    /// <summary>FK -> Customer.customer_id</summary>
    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("order_date")]
    public DateTime OrderDate { get; set; }

    [Column("total")]
    public decimal Total { get; set; }
}
