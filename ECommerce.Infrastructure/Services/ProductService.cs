using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly IImageStorage _storage;

    public ProductService(AppDbContext context, ICacheService cache, IImageStorage storage)
    {
        _context = context;
        _cache = cache;
        _storage = storage;
    }

    // ---- Products ----

    public async Task<PagedResult<ProductSummaryDto>> GetAll(ProductQueryDto query)
    {
        var q = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }
        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId.Value);
        if (query.MinPrice.HasValue)
            q = q.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.Price <= query.MaxPrice.Value);
        if (!string.IsNullOrWhiteSpace(query.Brand))
            q = q.Where(p => p.Brand != null && p.Brand.ToLower() == query.Brand.ToLower());
        if (query.MinRating.HasValue)
            q = q.Where(p => p.AverageRating >= query.MinRating.Value);

        var (orderBy, descending) = (query.SortBy ?? "newest").ToLower() switch
        {
            "price_asc" => ("Price", false),
            "price_desc" => ("Price", true),
            "popularity" => ("Popularity", true),
            _ => ("Newest", true) // newest is default
        };

        var totalCount = await q.CountAsync();

        var ordered = (orderBy, descending) switch
        {
            ("Price", false) => q.OrderBy(p => p.Price),
            ("Price", true) => q.OrderByDescending(p => p.Price),
            ("Popularity", true) => q.OrderByDescending(p => p.ReviewCount),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                Stock = p.Stock,
                ImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault()
            }).ToListAsync();

        return PagedResult<ProductSummaryDto>.Create(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<ProductDto> GetById(int id)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Product {id} not found");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Brand = product.Brand,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            LowStockAlert = product.LowStockAlert,
            Category = product.Category == null ? null : new CategoryDto
            {
                Id = product.Category.Id, Name = product.Category.Name, Slug = product.Category.Slug
            },
            Tags = product.Tags.Select(t => t.Tag).ToList(),
            Images = product.Images.Select(i => new ProductImageDto { Id = i.Id, Url = i.Url, IsMain = i.IsMain }).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name, Value = v.Value, Price = v.Price, Stock = v.Stock
            }).ToList()
        };
    }

    public async Task<int> Create(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            Brand = string.IsNullOrWhiteSpace(dto.Brand) ? null : dto.Brand.Trim(),
            CategoryId = dto.CategoryId,
            Tags = dto.Tags.Distinct().Select(t => new ProductTag { Tag = t.Trim() }).ToList(),
            Variants = dto.Variants.Select(v => new ProductVariant
            {
                Name = v.Name, Value = v.Value, Price = v.Price, Stock = v.Stock
            }).ToList(),
            LowStockAlert = dto.Stock <= 5
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("categories");
        return product.Id;
    }

    public async Task Update(int id, CreateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Tags)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Product {id} not found");

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.Brand = string.IsNullOrWhiteSpace(dto.Brand) ? null : dto.Brand.Trim();
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        // replace tag/images/variants wholesale
        product.Tags.Clear();
        foreach (var t in dto.Tags) product.Tags.Add(new ProductTag { Tag = t.Trim() });

        product.Variants.Clear();
        foreach (var v in dto.Variants)
            product.Variants.Add(new ProductVariant { Name = v.Name, Value = v.Value, Price = v.Price, Stock = v.Stock });

        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new NotFoundException($"Product {id} not found");
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("categories");
    }

    public async Task AddImages(int productId, IEnumerable<(Stream Stream, string FileName)> files)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException($"Product {productId} not found");

        foreach (var (stream, fileName) in files)
        {
            var url = await _storage.SaveAsync(stream, fileName, "products");
            product.Images.Add(new ProductImage
            {
                Url = url,
                IsMain = product.Images.Count == 0 // first image becomes the main image
            });
        }
        await _context.SaveChangesAsync();
    }

    // ---- Reviews ----

    public async Task<PagedResult<ReviewDto>> GetReviews(int productId, int page, int pageSize)
    {
        var total = await _context.Reviews.Where(r => r.ProductId == productId).CountAsync();
        var items = await _context.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserName = r.User.UserName ?? r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToListAsync();
        return PagedResult<ReviewDto>.Create(items, total, page, pageSize);
    }

    public async Task<ReviewDto> AddReview(int productId, string userId, CreateReviewDto dto)
    {
        var exists = await _context.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        if (exists)
            throw new BusinessException("You have already reviewed this product");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Comment = dto.Comment?.Trim()
        };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await RecalculateRating(productId);
        return new ReviewDto
        {
            Id = review.Id, ProductId = productId, Rating = review.Rating,
            Comment = review.Comment, CreatedAt = review.CreatedAt
        };
    }

    // ---- Wishlist ----

    public async Task<List<ProductSummaryDto>> GetWishlist(string userId)
    {
        return await _context.WishlistItems.AsNoTracking()
            .Where(w => w.Wishlist.UserId == userId)
            .Select(w => new ProductSummaryDto
            {
                Id = w.Product.Id,
                Name = w.Product.Name,
                Price = w.Product.Price,
                Brand = w.Product.Brand,
                AverageRating = w.Product.AverageRating,
                ReviewCount = w.Product.ReviewCount,
                Stock = w.Product.Stock,
                ImageUrl = w.Product.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault()
            }).ToListAsync();
    }

    public async Task AddToWishlist(string userId, int productId)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == productId))
            throw new NotFoundException($"Product {productId} not found");

        var list = await GetOrCreateWishlist(userId);
        var already = await _context.WishlistItems.AnyAsync(w => w.WishlistId == list.Id && w.ProductId == productId);
        if (already) return;

        _context.WishlistItems.Add(new WishlistItem { WishlistId = list.Id, ProductId = productId });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromWishlist(string userId, int productId)
    {
        var list = await GetOrCreateWishlist(userId);
        var item = await _context.WishlistItems.FirstOrDefaultAsync(w => w.WishlistId == list.Id && w.ProductId == productId);
        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // ---- Categories ----

    public async Task<List<CategoryDto>> GetCategories()
    {
        var cached = await _cache.GetAsync<List<CategoryDto>>("categories");
        if (cached is not null) return cached;

        var cats = await _context.Categories.AsNoTracking()
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug, ParentId = c.ParentId })
            .ToListAsync();
        await _cache.SetAsync("categories", cats, TimeSpan.FromMinutes(30));
        return cats;
    }

    public async Task<List<CategoryTreeDto>> GetCategoryTree()
    {
        var cached = await _cache.GetAsync<List<CategoryTreeDto>>("category-tree");
        if (cached is not null) return cached;

        var flat = await _context.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryTreeDto { Id = c.Id, Name = c.Name, Slug = c.Slug, ParentId = c.ParentId })
            .ToListAsync();

        var byParent = flat.ToLookup(c => c.ParentId);

        List<CategoryTreeDto> Build(int? parentId)
        {
            var nodes = new List<CategoryTreeDto>();
            foreach (var node in byParent[parentId])
            {
                node.Children = Build(node.Id);
                nodes.Add(node);
            }
            return nodes;
        }

        var tree = Build(null);
        await _cache.SetAsync("category-tree", tree, TimeSpan.FromMinutes(30));
        return tree;
    }

    public async Task CreateCategory(CreateCategoryDto dto)
    {
        if (dto.ParentId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == dto.ParentId.Value))
            throw new NotFoundException($"Parent category {dto.ParentId} not found");

        _context.Categories.Add(new Category
        {
            Name = dto.Name.Trim(),
            Slug = dto.Name.Trim().ToLower().Replace(" ", "-"),
            ParentId = dto.ParentId
        });
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("categories");
        await _cache.RemoveAsync("category-tree");
    }

    // ---- Inventory ----

    public async Task<PagedResult<ProductSummaryDto>> GetLowStockProducts(int threshold, int page, int pageSize)
    {
        if (threshold < 0) threshold = 0;
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var q = _context.Products.AsNoTracking().Where(p => p.Stock <= threshold);
        var total = await q.CountAsync();
        var items = await q
            .OrderBy(p => p.Stock).ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Brand = p.Brand,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                Stock = p.Stock,
                ImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault()
            }).ToListAsync();

        return PagedResult<ProductSummaryDto>.Create(items, total, page, pageSize);
    }

    // ---- private ----

    private async Task RecalculateRating(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return;
        product.ReviewCount = await _context.Reviews.CountAsync(r => r.ProductId == productId);
        var avg = await _context.Reviews.Where(r => r.ProductId == productId).AverageAsync(r => (decimal)r.Rating);
        product.AverageRating = product.ReviewCount == 0 ? 0 : Math.Round(avg, 2);
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<Wishlist> GetOrCreateWishlist(string userId)
    {
        var list = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId);
        if (list == null)
        {
            list = new Wishlist { UserId = userId };
            _context.Wishlists.Add(list);
            await _context.SaveChangesAsync();
        }
        return list;
    }
}