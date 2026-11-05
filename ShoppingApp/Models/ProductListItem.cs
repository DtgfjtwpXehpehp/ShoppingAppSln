using CommunityToolkit.Mvvm.ComponentModel;

namespace ShoppingApp.Models;

/// <summary>
/// Product joined with its category name, for the catalogue cards.
/// Price and Stock are observable so the catalogue can refresh them in place (the list keeps its scroll position).
/// </summary>
public class ProductListItem : ObservableObject
{
    private const int LowStockLimit = 5;

    private decimal _price;
    private int _stock;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public int Stock
    {
        get => _stock;
        set
        {
            if (SetProperty(ref _stock, value))
            {
                OnPropertyChanged(nameof(HasBadge));
                OnPropertyChanged(nameof(BadgeText));
            }
        }
    }

    private string ResolvedImage => ProductVisuals.ToImageSource(ImagePath);

    /// <summary>Null when the product has no picture, so the Image control gets no source at all.</summary>
    public string? ImageSourcePath => ResolvedImage.Length > 0 ? ResolvedImage : null;
    public bool HasImage => ResolvedImage.Length > 0;
    public bool NoImage => !HasImage;
    public string Emoji => ProductVisuals.EmojiFor(CategoryName, ProductName);

    // Badge shown on the card image (like the "NEW" badge in the card design)
    public bool HasBadge => Stock <= LowStockLimit;
    public string BadgeText => Stock <= 0 ? "Sold out" : $"Only {Stock} left";
}
