using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto query)
        => Ok(ApiResponse<object>.Success(await _productService.GetAll(query)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<ProductDto>.Success(await _productService.GetById(id)));

    [Authorize(Roles = "Admin,Vendor")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
        => Ok(ApiResponse<object>.Success(await _productService.Create(dto), "Product created"));

    [Authorize(Roles = "Admin,Vendor")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateProductDto dto)
    {
        await _productService.Update(id, dto);
        return Ok(ApiResponse<object>.Success(null!, "Product updated"));
    }

    [Authorize(Roles = "Admin,Vendor")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.Delete(id);
        return Ok(ApiResponse<object>.Success(null!, "Product deleted"));
    }

    [Authorize(Roles = "Admin,Vendor")]
    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages(int id, List<IFormFile> files)
    {
        var uploads = new List<(Stream, string)>();
        foreach (var file in files)
            uploads.Add((file.OpenReadStream(), file.FileName));
        await _productService.AddImages(id, uploads);
        foreach (var (stream, _) in uploads) await stream.DisposeAsync();
        return Ok(ApiResponse<object>.Success(null!, "Images uploaded"));
    }

    // Reviews
    [HttpGet("{id:int}/reviews")]
    public async Task<IActionResult> Reviews(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(ApiResponse<object>.Success(await _productService.GetReviews(id, page, pageSize)));

    [Authorize]
    [HttpPost("{id:int}/reviews")]
    public async Task<IActionResult> AddReview(int id, CreateReviewDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<ReviewDto>.Success(await _productService.AddReview(id, userId, dto)));
    }

    // Wishlist
    [Authorize]
    [HttpGet("wishlist")]
    public async Task<IActionResult> Wishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<object>.Success(await _productService.GetWishlist(userId)));
    }

    [Authorize]
    [HttpPost("wishlist")]
    public async Task<IActionResult> AddToWishlist(CreateWishlistItemDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _productService.AddToWishlist(userId, dto.ProductId);
        return Ok(ApiResponse<object>.Success(null!, "Added to wishlist"));
    }

    [Authorize]
    [HttpDelete("wishlist/{productId:int}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _productService.RemoveFromWishlist(userId, productId);
        return Ok(ApiResponse<object>.Success(null!, "Removed from wishlist"));
    }

    // Categories
    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
        => Ok(ApiResponse<object>.Success(await _productService.GetCategories()));

    [HttpGet("categories/tree")]
    public async Task<IActionResult> CategoryTree()
        => Ok(ApiResponse<object>.Success(await _productService.GetCategoryTree()));

    [Authorize(Roles = "Admin")]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
    {
        await _productService.CreateCategory(dto);
        return Ok(ApiResponse<object>.Success(null!, "Category created"));
    }

    // Inventory
    [Authorize(Roles = "Admin")]
    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock(
        [FromQuery] int threshold = 5, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(ApiResponse<object>.Success(await _productService.GetLowStockProducts(threshold, page, pageSize)));
}