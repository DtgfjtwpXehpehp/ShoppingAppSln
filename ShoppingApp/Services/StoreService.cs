using System.Security.Cryptography;
using ShoppingApp.Models;
using SQLite;

namespace ShoppingApp.Services;

/// <summary>
/// All SQLite access lives here. On first use it copies the bundled OfflineShoppingApp.db into the
/// app data folder, creates any missing tables (same schema as the ERD) and seeds sample
/// data only into tables that are empty.
/// </summary>
public class StoreService : IStoreService
{
    private const string DbFileName = "OfflineShoppingApp.db";
    private const string DbHashKey = "bundled_db_sha256";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _db;

    // ---------------------------------------------------------------- schema
    // Identical to the schema in the OfflineShoppingApp.db file; IF NOT EXISTS makes it a no-op when present.

    private static readonly string[] Schema =
    {
        @"CREATE TABLE IF NOT EXISTS Customer (
            customer_id   INTEGER PRIMARY KEY AUTOINCREMENT,
            first_name    TEXT NOT NULL,
            last_name     TEXT NOT NULL,
            phone_number  TEXT NOT NULL,
            email_address TEXT NOT NULL UNIQUE)",

        @"CREATE TABLE IF NOT EXISTS Category (
            category_id   INTEGER PRIMARY KEY AUTOINCREMENT,
            category_name TEXT NOT NULL)",

        @"CREATE TABLE IF NOT EXISTS Product (
            product_id          INTEGER PRIMARY KEY AUTOINCREMENT,
            category_id         INTEGER NOT NULL,
            product_name        TEXT NOT NULL,
            product_description TEXT NOT NULL,
            image_path          TEXT NOT NULL,
            price               REAL NOT NULL,
            stock               INTEGER NOT NULL,
            FOREIGN KEY (category_id) REFERENCES Category(category_id))",

        @"CREATE TABLE IF NOT EXISTS Cart_item (
            cart_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
            customer_id  INTEGER NOT NULL,
            product_id   INTEGER NOT NULL,
            quantity     INTEGER NOT NULL,
            FOREIGN KEY (customer_id) REFERENCES Customer(customer_id),
            FOREIGN KEY (product_id)  REFERENCES Product(product_id),
            UNIQUE (customer_id, product_id))",

        @"CREATE TABLE IF NOT EXISTS ""Order"" (
            order_id     INTEGER PRIMARY KEY AUTOINCREMENT,
            customer_id  INTEGER NOT NULL,
            order_date   TEXT NOT NULL DEFAULT (DATE('now')),
            total_amount REAL NOT NULL,
            FOREIGN KEY (customer_id) REFERENCES Customer(customer_id))",

        @"CREATE TABLE IF NOT EXISTS Order_item (
            order_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
            order_id      INTEGER NOT NULL,
            product_id    INTEGER NOT NULL,
            quantity      INTEGER NOT NULL,
            unit_price    REAL NOT NULL,
            FOREIGN KEY (order_id)   REFERENCES ""Order""(order_id),
            FOREIGN KEY (product_id) REFERENCES Product(product_id))"
    };

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is not null) return _db;

        await _initLock.WaitAsync();
        try
        {
            if (_db is not null) return _db;

            var path = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
            await EnsureDatabaseFileAsync(path);

            var db = new SQLiteAsyncConnection(
                path,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache,
                storeDateTimeAsTicks: false);   // readable ISO-8601 dates in the file

            await db.ExecuteAsync("PRAGMA foreign_keys = ON");
            foreach (var ddl in Schema)
                await db.ExecuteAsync(ddl);

            await SeedAsync(db);

            _db = db;
            return db;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Copies the bundled database to the app data folder on first launch, and again whenever a
    /// different database file is bundled (detected by hash) - so replacing the .db in the project
    /// takes effect on the next run without uninstalling the app.
    /// Orders placed inside the app are kept until the bundled file itself changes.
    /// </summary>
    private static async Task EnsureDatabaseFileAsync(string path)
    {
        byte[] bytes;
        await using (var asset = await FileSystem.OpenAppPackageFileAsync(DbFileName))
        using (var buffer = new MemoryStream())
        {
            await asset.CopyToAsync(buffer);
            bytes = buffer.ToArray();
        }

        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        if (File.Exists(path) && Preferences.Get(DbHashKey, string.Empty) == hash)
            return;

        // Remove leftovers of the previous database before writing the new one.
        foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            File.Delete(path + suffix);

        await File.WriteAllBytesAsync(path, bytes);
        Preferences.Set(DbHashKey, hash);
    }

    // ------------------------------------------------------------------ seed

    private static async Task SeedAsync(SQLiteAsyncConnection db)
    {
        if (await db.Table<Customer>().CountAsync() == 0)
        {
            await db.InsertAllAsync(new[]
            {
                new Customer { FirstName = "Thandi", LastName = "Nkosi",         PhoneNumber = "+27 82 555 0101", EmailAddress = "thandi.nkosi@example.com" },
                new Customer { FirstName = "Pieter", LastName = "van der Merwe", PhoneNumber = "+27 83 555 0102", EmailAddress = "pieter.vdm@example.com" },
                new Customer { FirstName = "Aisha",  LastName = "Patel",         PhoneNumber = "+27 84 555 0103", EmailAddress = "aisha.patel@example.com" },
                new Customer { FirstName = "Liam",   LastName = "Naidoo",        PhoneNumber = "+27 72 555 0104", EmailAddress = "liam.naidoo@example.com" },
                new Customer { FirstName = "Zanele", LastName = "Dlamini",       PhoneNumber = "+27 71 555 0105", EmailAddress = "zanele.dlamini@example.com" },
            });
        }

        // Categories and products are only seeded when BOTH are empty, so a pre-filled database is left alone.
        if (await db.Table<Category>().CountAsync() == 0 && await db.Table<Product>().CountAsync() == 0)
        {
            var categories = new[]
            {
                "Fruit & Vegetables", "Bakery", "Dairy & Eggs", "Meat & Poultry",
                "Beverages", "Pantry", "Snacks"
            }.Select(n => new Category { CategoryName = n }).ToList();

            await db.InsertAllAsync(categories);
            var id = categories.ToDictionary(c => c.CategoryName, c => c.CategoryId);

            // image_path is NOT NULL, so "" means "no picture yet" (the card then shows an emoji).
            // Put pictures in Resources/Images and store just the file name, e.g. "bananas.png".
            Product P(string cat, string name, string desc, decimal price, int stock) => new()
            {
                CategoryId = id[cat], ProductName = name, ProductDescription = desc,
                ImagePath = string.Empty, Price = price, Stock = stock
            };

            await db.InsertAllAsync(new[]
            {
                P("Fruit & Vegetables", "Bananas (1 kg)",        "Sweet, ripe bananas. Perfect for smoothies or a quick snack.", 24.99m, 40),
                P("Fruit & Vegetables", "Baby Spinach 200 g",    "Pre-washed baby spinach leaves, ready for salads and stir-fries.", 22.50m, 25),
                P("Fruit & Vegetables", "Avocados (4 pack)",     "Creamy Hass avocados. Limited stock this week.", 39.99m, 5),
                P("Fruit & Vegetables", "Cherry Tomatoes 250 g", "Sweet vine-ripened cherry tomatoes.", 27.99m, 30),

                P("Bakery", "Whole Wheat Bread",      "Freshly baked 700 g whole wheat loaf, sliced.", 18.99m, 35),
                P("Bakery", "Butter Croissants (6)",  "Flaky all-butter croissants, baked this morning.", 42.00m, 6),
                P("Bakery", "Buttermilk Rusks 500 g", "Classic South African rusks. Ideal with coffee or rooibos.", 37.50m, 20),

                P("Dairy & Eggs", "Full Cream Milk 2 L",  "Fresh full cream milk.", 39.99m, 50),
                P("Dairy & Eggs", "Free-Range Eggs (12)", "A dozen large free-range eggs.", 52.00m, 28),
                P("Dairy & Eggs", "Mature Cheddar 400 g", "Rich, mature cheddar block.", 79.99m, 15),

                P("Meat & Poultry", "Chicken Breasts 1 kg", "Skinless, boneless chicken breast fillets.", 99.99m, 18),
                P("Meat & Poultry", "Boerewors 500 g",      "Traditional coriander-spiced boerewors, ready for the braai.", 74.99m, 12),
                P("Meat & Poultry", "Beef Mince 500 g",     "Lean beef mince, great for bolognese or burgers.", 84.50m, 0),

                P("Beverages", "Rooibos Tea (80 bags)",      "Caffeine-free rooibos from the Cederberg.", 45.00m, 45),
                P("Beverages", "Orange Juice 1 L",           "100% pressed orange juice, no added sugar.", 32.99m, 30),
                P("Beverages", "Sparkling Water 6 x 500 ml", "Crisp sparkling mineral water.", 44.99m, 22),

                P("Pantry", "Basmati Rice 2 kg",      "Long-grain aromatic basmati rice.", 64.99m, 40),
                P("Pantry", "Spaghetti 500 g",        "Durum wheat spaghetti.", 19.99m, 60),
                P("Pantry", "Olive Oil 500 ml",       "Extra virgin olive oil, cold pressed.", 119.00m, 14),
                P("Pantry", "Peri-Peri Sauce 250 ml", "Medium-heat peri-peri sauce.", 34.99m, 33),

                P("Snacks", "Original Biltong 200 g",   "Thinly sliced air-dried beef biltong.", 89.99m, 4),
                P("Snacks", "Salted Peanuts 250 g",     "Roasted and lightly salted peanuts.", 27.99m, 38),
                P("Snacks", "Dark Chocolate Slab 80 g", "70% cocoa dark chocolate.", 29.99m, 26),
            });
        }
    }

    // --------------------------------------------------------------- queries

    public async Task<List<Customer>> GetCustomersAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<Customer>().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public async Task<List<ProductListItem>> GetProductsAsync()
    {
        var db = await GetDbAsync();
        return await db.QueryAsync<ProductListItem>(
            @"SELECT p.product_id          AS ProductId,
                     p.product_name        AS ProductName,
                     p.product_description AS ProductDescription,
                     p.image_path          AS ImagePath,
                     c.category_name       AS CategoryName,
                     p.price               AS Price,
                     p.stock               AS Stock
              FROM Product p
              JOIN Category c ON c.category_id = p.category_id
              ORDER BY c.category_name, p.product_name");
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        var db = await GetDbAsync();
        return await db.FindAsync<Product>(productId);
    }

    public async Task<string> GetCategoryNameAsync(int categoryId)
    {
        var db = await GetDbAsync();
        var category = await db.FindAsync<Category>(categoryId);
        return category?.CategoryName ?? string.Empty;
    }

    public async Task<int> GetCartCountAsync(int customerId)
    {
        var db = await GetDbAsync();
        return await db.ExecuteScalarAsync<int>(
            "SELECT COALESCE(SUM(quantity), 0) FROM Cart_item WHERE customer_id = ?", customerId);
    }

    public async Task<int> GetCartQuantityAsync(int customerId, int productId)
    {
        var db = await GetDbAsync();
        return await db.ExecuteScalarAsync<int>(
            "SELECT COALESCE(SUM(quantity), 0) FROM Cart_item WHERE customer_id = ? AND product_id = ?",
            customerId, productId);
    }

    public async Task<List<CartLine>> GetCartLinesAsync(int customerId)
    {
        var db = await GetDbAsync();
        return await db.QueryAsync<CartLine>(CartLinesSql, customerId);
    }

    private const string CartLinesSql =
        @"SELECT ci.cart_item_id AS CartItemId,
                 ci.product_id   AS ProductId,
                 p.product_name  AS ProductName,
                 ci.quantity     AS Quantity,
                 p.price         AS UnitPrice,
                 p.stock         AS Stock
          FROM Cart_item ci
          JOIN Product p ON p.product_id = ci.product_id
          WHERE ci.customer_id = ?
          ORDER BY p.product_name";

    public async Task<List<OrderSummary>> GetOrderHistoryAsync(int customerId)
    {
        var db = await GetDbAsync();

        var orders = await db.Table<Order>()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .ToListAsync();

        var lines = await db.QueryAsync<OrderLine>(
            @"SELECT oi.order_id   AS OrderId,
                     p.product_name AS ProductName,
                     oi.quantity    AS Quantity,
                     oi.unit_price  AS UnitPrice
              FROM Order_item oi
              JOIN Product p ON p.product_id = oi.product_id
              JOIN ""Order"" o ON o.order_id = oi.order_id
              WHERE o.customer_id = ?
              ORDER BY oi.order_item_id", customerId);

        var byOrder = lines.ToLookup(l => l.OrderId);

        return orders.Select(o => new OrderSummary
        {
            OrderId = o.OrderId,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Lines = byOrder[o.OrderId].ToList()
        }).ToList();
    }

    // -------------------------------------------------------------- commands

    public async Task<OperationResult> AddToCartAsync(int customerId, int productId, int quantity)
    {
        var db = await GetDbAsync();
        var result = new OperationResult(false, "Something went wrong.");

        // One transaction so the stock check and the write cannot be separated.
        await db.RunInTransactionAsync(conn =>
        {
            if (quantity <= 0)
            {
                result = new OperationResult(false, "Quantity must be greater than zero.");
                return;
            }

            var product = conn.Find<Product>(productId);
            if (product is null)
            {
                result = new OperationResult(false, "That product no longer exists.");
                return;
            }

            var existing = conn.Table<CartItem>()
                .FirstOrDefault(c => c.CustomerId == customerId && c.ProductId == productId);
            var inCart = existing?.Quantity ?? 0;

            if (product.Stock <= 0)
            {
                result = new OperationResult(false, $"Sorry, {product.ProductName} is sold out.");
                return;
            }

            if (inCart + quantity > product.Stock)
            {
                var remaining = Math.Max(0, product.Stock - inCart);
                result = new OperationResult(false,
                    remaining == 0
                        ? $"You already have all {product.Stock} available in your cart."
                        : $"Only {remaining} more can be added (stock {product.Stock}, in your cart {inCart}).");
                return;
            }

            if (existing is null)
            {
                conn.Insert(new CartItem { CustomerId = customerId, ProductId = productId, Quantity = quantity });
            }
            else
            {
                existing.Quantity += quantity;   // already in cart -> increase quantity, no new line
                conn.Update(existing);
            }

            result = new OperationResult(true, $"Added {quantity} x {product.ProductName} to your cart.");
        });

        return result;
    }

    public async Task RemoveCartItemAsync(int cartItemId)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<CartItem>(cartItemId);
    }

    public async Task<CheckoutResult> CheckoutAsync(int customerId)
    {
        var db = await GetDbAsync();
        CheckoutResult result = new(false, "Checkout failed.", 0, 0m);

        try
        {
            // Everything below commits together, or not at all.
            await db.RunInTransactionAsync(conn =>
            {
                var lines = conn.Query<CartLine>(CartLinesSql, customerId);
                if (lines.Count == 0)
                    throw new InvalidOperationException("Your cart is empty.");

                foreach (var line in lines)
                {
                    if (line.Quantity > line.Stock)
                        throw new InvalidOperationException(
                            $"Not enough stock for {line.ProductName} (only {line.Stock} left).");
                }

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = lines.Sum(l => l.LineTotal)
                };
                conn.Insert(order);   // sets order.OrderId

                foreach (var line in lines)
                {
                    // Cart_item -> Order_item, freezing the price paid right now.
                    conn.Insert(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice
                    });

                    conn.Execute(
                        "UPDATE Product SET stock = stock - ? WHERE product_id = ?",
                        line.Quantity, line.ProductId);
                }

                conn.Execute("DELETE FROM Cart_item WHERE customer_id = ?", customerId);

                result = new CheckoutResult(true, "Order placed.", order.OrderId, order.TotalAmount);
            });
        }
        catch (InvalidOperationException ex)
        {
            return new CheckoutResult(false, ex.Message, 0, 0m);
        }

        return result;
    }
}
