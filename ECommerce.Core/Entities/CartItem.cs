namespace ECommerce.Core.Entities;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Id of the chosen variant, if any.</summary>
    public int? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // snapshot at add-time

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}