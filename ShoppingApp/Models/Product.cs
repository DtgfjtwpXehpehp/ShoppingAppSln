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

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("product_description")]
    public string ProductDescription { get; set; } = string.Empty;

    /// <summary>File name of a bundled image (Resources/Images) or a URL.</summary>
    [Column("image_path")]
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Current price. Changing this must never alter past orders.</summary>
    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Ignore] public string ImageSourcePath => ProductVisuals.ToImageSource(ImagePath);
    [Ignore] public bool HasImage => ImageSourcePath.Length > 0;
    [Ignore] public bool NoImage => !HasImage;
}
