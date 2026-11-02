namespace ShoppingApp.Models;

/// <summary>Product joined with its category name, for the catalogue list.</summary>
public class ProductListItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
