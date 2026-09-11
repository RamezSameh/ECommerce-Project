using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Entities;

/// <summary>A product sold in the store.</summary>
[Index(nameof(Name))]
[Index(nameof(CategoryId))]
[Index(nameof(CreatedAt))]
public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }

    /// <summary>Base stock; variants override with their own stock counts.</summary>
    public int Stock { get; set; }

    /// <summary>Optional brand name, used partly for filtering.</summary>
    public string? Brand { get; set; }

    /// <summary>Computed from <see cref="Reviews"/>, cached for quick sorting/filtering.</summary>
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    /// <summary>True when stock is at or below the low-stock threshold.</summary>
    public bool LowStockAlert { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

/// <summary>A nested product category (child categories via ParentId).</summary>
public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Slug { get; set; }

    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>A single image for a product (multiple per product).</summary>
public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Url { get; set; }
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>A product variant such as Size/Color. Each variant has its own stock/price.</summary>
public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Name { get; set; }   // e.g. "Size", "Color"
    public required string Value { get; set; }   // e.g. "L", "Red"
    [Precision(18, 2)] public decimal? Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>Many-to-many join between a product and a tag keyword.</summary>
public class ProductTag
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Tag { get; set; }
}

/// <summary>A product review left by a user. One review per user per product.</summary>
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; } // 1..5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Cart owned by a user (registered) or by a guest token.</summary>
public class Cart
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Identifier for guest carts, e.g. a GUID sent as a header/cookie.</summary>
    public string? GuestId { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>The active discount coupon for this cart.</summary>
    public string? CouponCode { get; set; }
}