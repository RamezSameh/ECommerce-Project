namespace ECommerce.Core.Entities;

/// <summary>A user's wishlist (a user may have several named lists; default is shared here).</summary>
public class Wishlist
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = "Default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

public class WishlistItem
{
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public Wishlist? Wishlist { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}