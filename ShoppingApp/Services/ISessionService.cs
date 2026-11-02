using ShoppingApp.Models;

namespace ShoppingApp.Services;

/// <summary>Holds the customer chosen on the "Select Customer" screen (no login needed).</summary>
public interface ISessionService
{
    Customer? CurrentCustomer { get; set; }
}

public class SessionService : ISessionService
{
    public Customer? CurrentCustomer { get; set; }
}
