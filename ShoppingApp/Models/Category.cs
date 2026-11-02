using SQLite;

namespace ShoppingApp.Models;

[Table("Category")]
public class Category
{
    [PrimaryKey, AutoIncrement, Column("category_id")]
    public int CategoryId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
