namespace ShoppingApp.Models;

/// <summary>Helpers for showing a product picture, with an emoji fallback when there is no image.</summary>
public static class ProductVisuals
{
    /// <summary>
    /// image_path may hold "apple.png", "Images/apple.png" or a web URL.
    /// MAUI bundled images are referenced by file name only, so any folder prefix is dropped.
    /// </summary>
    public static string ToImageSource(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return string.Empty;
        var path = imagePath.Trim();
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
        return Path.GetFileName(path.Replace('\\', '/'));
    }

    /// <summary>Placeholder emoji shown when a product has no picture (product name first, then cuisine/category).</summary>
    public static string EmojiFor(string? categoryName, string? productName = null)
    {
        var p = productName?.ToLowerInvariant() ?? string.Empty;
        foreach (var (keyword, emoji) in ByProduct)
            if (p.Contains(keyword)) return emoji;

        var c = categoryName?.ToLowerInvariant() ?? string.Empty;
        foreach (var (keyword, emoji) in ByCategory)
            if (c.Contains(keyword)) return emoji;

        return "🍽️";
    }

    private static readonly (string, string)[] ByProduct =
    {
        ("pizza", "🍕"), ("cookie", "🍪"), ("tiramisu", "🍰"), ("smoothie", "🥤"), ("lassi", "🥛"),
        ("caipirinha", "🍹"), ("ramen", "🍜"), ("salad", "🥗"), ("pasta", "🍝"), ("bruschetta", "🍅"),
        ("kebab", "🍢"), ("borscht", "🥣"), ("corn", "🌽"), ("wrap", "🌯"), ("biryani", "🍛"),
        ("curry", "🍛"), ("bibimbap", "🍚"), ("moussaka", "🍆"), ("dosa", "🫓"), ("tagine", "🍲"),
        ("stir-fry", "🥡"), ("karahi", "🍛"), ("keema", "🍛"), ("saag", "🥬"), ("chicken", "🍗"),
    };

    private static readonly (string, string)[] ByCategory =
    {
        ("italian", "🍝"), ("asian", "🥡"), ("american", "🍔"), ("mexican", "🌮"), ("mediterranean", "🥗"),
        ("pakistani", "🍛"), ("japanese", "🍣"), ("moroccan", "🍲"), ("korean", "🍚"), ("greek", "🥙"),
        ("thai", "🍛"), ("indian", "🍛"), ("turkish", "🥙"), ("smoothie", "🥤"), ("russian", "🥣"),
        ("lebanese", "🧆"), ("brazilian", "🍹"),
    };
}
