namespace ECommerce.Application.DTOs.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Brand { get; set; }
    public int? CategoryId { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int Stock { get; set; }
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int? ParentId { get; set; }
}

/// <summary>Category node with nested children, used by the category tree endpoint.</summary>
public class CategoryTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int? ParentId { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = new();
}

/// <summary>Full product detail returned by GetById.</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Brand { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool LowStockAlert { get; set; }
    public CategoryDto? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

/// <summary>Lightweight product card used by list/search endpoints.</summary>
public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

public class CreateWishlistItemDto
{
    public int ProductId { get; set; }
}