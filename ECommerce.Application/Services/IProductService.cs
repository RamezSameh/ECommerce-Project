using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductSummaryDto>> GetAll(ProductQueryDto query);
    Task<ProductDto> GetById(int id);
    Task<int> Create(CreateProductDto dto);
    Task Update(int id, CreateProductDto dto);
    Task Delete(int id);
    Task AddImages(int productId, IEnumerable<(Stream Stream, string FileName)> files);

    // Reviews
    Task<PagedResult<ReviewDto>> GetReviews(int productId, int page, int pageSize);
    Task<ReviewDto> AddReview(int productId, string userId, CreateReviewDto dto);

    // Wishlist
    Task<List<ProductSummaryDto>> GetWishlist(string userId);
    Task AddToWishlist(string userId, int productId);
    Task RemoveFromWishlist(string userId, int productId);

    // Categories
    Task<List<CategoryDto>> GetCategories();
    Task CreateCategory(CreateCategoryDto dto);
}

/// <summary>Query payload for the search/filter/sort/paginate product endpoint.</summary>
public class ProductQueryDto : QueryParameters
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Brand { get; set; }
    public int? MinRating { get; set; }
    public string? SortBy { get; set; } // price_asc | price_desc | newest | popularity
}