namespace ShoppingApp.Models;

/// <summary>Product joined with its category name, for the catalogue cards.</summary>
public class ProductListItem
{
    private const int LowStockLimit = 5;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public string ImageSourcePath => ProductVisuals.ToImageSource(ImagePath);
    public bool HasImage => ImageSourcePath.Length > 0;
    public bool NoImage => !HasImage;
    public string Emoji => ProductVisuals.EmojiFor(CategoryName, ProductName);

    // Badge shown on the card image (like the "NEW" badge in the card design)
    public bool HasBadge => Stock <= LowStockLimit;
    public string BadgeText => Stock <= 0 ? "Sold out" : $"Only {Stock} left";
}
