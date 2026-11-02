using ShoppingApp.Models;

namespace ShoppingApp.Services;

public record OperationResult(bool Success, string Message);

public record CheckoutResult(bool Success, string Message, int OrderId, decimal Total);

public interface IStoreService
{
    Task<List<Customer>> GetCustomersAsync();

    Task<List<ProductListItem>> GetProductsAsync();
    Task<Product?> GetProductAsync(int productId);
    Task<string> GetCategoryNameAsync(int categoryId);

    /// <summary>Total number of units in the customer's cart.</summary>
    Task<int> GetCartCountAsync(int customerId);

    /// <summary>Units of one product already in the customer's cart.</summary>
    Task<int> GetCartQuantityAsync(int customerId, int productId);

    Task<OperationResult> AddToCartAsync(int customerId, int productId, int quantity);
    Task<List<CartLine>> GetCartLinesAsync(int customerId);
    Task RemoveCartItemAsync(int cartItemId);

    Task<CheckoutResult> CheckoutAsync(int customerId);
}
