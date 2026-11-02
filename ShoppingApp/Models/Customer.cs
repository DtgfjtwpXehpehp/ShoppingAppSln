using SQLite;

namespace ShoppingApp.Models;

[Table("Customer")]
public class Customer
{
    [PrimaryKey, AutoIncrement, Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("email_address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Ignore]
    public string FullName => $"{FirstName} {LastName}";

    [Ignore]
    public string Initials =>
        $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}".Trim().ToUpperInvariant();
}
