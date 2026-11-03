using SQLite;

namespace ShoppingApp.Models;

[Table("Category")]
public class Category
{
    [PrimaryKey, AutoIncrement, Column("category_id")]
    public int CategoryId { get; set; }

    [Column("category_name")]
    public string CategoryName { get; set; } = string.Empty;
}
