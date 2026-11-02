using SQLite;

namespace ShoppingApp.Models;

[Table("Product")]
public class Product
{
    [PrimaryKey, AutoIncrement, Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>FK -> Category.category_id</summary>
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Current price. Changing this must never alter past orders.</summary>
    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }
}
