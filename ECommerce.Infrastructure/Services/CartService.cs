using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context) => _context = context;

    private const decimal TaxRate = 0.14m; // adjustable in production via config

    public async Task<CartDto> GetCartAsync(string? userId, string? guestId)
    {
        var cart = await LoadCart(userId, guestId);
        return await BuildAsync(cart);
    }

    public async Task<CartDto> AddItemAsync(string? userId, string? guestId, AddCartItemDto dto)
    {
        if (dto.Quantity < 1) throw new BusinessException("Quantity must be at least 1");
        var cart = await LoadCart(userId, guestId);

        var product = await _context.Products.FindAsync(dto.ProductId)
            ?? throw new NotFoundException($"Product {dto.ProductId} not found");

        decimal unitPrice = product.Price;
        string? variantSummary = null;

        if (dto.VariantId.HasValue)
        {
            var variant = await _context.ProductVariants.FindAsync(dto.VariantId.Value)
                ?? throw new NotFoundException($"Variant {dto.VariantId} not found");
            if (variant.ProductId != product.Id)
                throw new BusinessException("Variant does not belong to this product");
            unitPrice = variant.Price ?? product.Price;
            variantSummary = $"{variant.Name}: {variant.Value}";
        }

        if (product.Stock < dto.Quantity)
            throw new BusinessException($"Only {product.Stock} currently in stock");

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);
        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = dto.ProductId,
                VariantId = dto.VariantId,
                Quantity = dto.Quantity,
                UnitPrice = unitPrice
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await BuildAsync(cart);
    }

    public async Task<CartDto> UpdateQuantityAsync(string? userId, string? guestId, int productId, UpdateCartItemDto dto)
    {
        var cart = await LoadCart(userId, guestId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("Item not in cart");

        if (dto.Quantity < 1)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = dto.Quantity;
        }
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await BuildAsync(cart);
    }

    public async Task<CartDto> RemoveItemAsync(string? userId, string? guestId, int productId)
    {
        var cart = await LoadCart(userId, guestId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return await BuildAsync(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(string? userId, string? guestId, string code)
    {
        var cart = await LoadCart(userId, guestId);
        cart.CouponCode = code.Trim().ToUpperInvariant();
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await BuildAsync(cart);
    }

    public async Task<CartDto> RemoveCouponAsync(string? userId, string? guestId)
    {
        var cart = await LoadCart(userId, guestId);
        cart.CouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await BuildAsync(cart);
    }

    public async Task MergeGuestCartAsync(string guestId, string userId)
    {
        var guestCart = await _context.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.GuestId == guestId);
        if (guestCart is null || guestCart.Items.Count == 0) return;

        var userCart = await LoadCart(userId, null);
        foreach (var item in guestCart.Items)
        {
            var exists = userCart.Items.FirstOrDefault(i => i.ProductId == item.ProductId && i.VariantId == item.VariantId);
            if (exists != null)
                exists.Quantity += item.Quantity;
            else
            {
                item.CartId = userCart.Id;
                _context.CartItems.Update(item);
            }
        }
        // remove the (now empty) guest cart
        _context.Carts.Remove(guestCart);
        await _context.SaveChangesAsync();
    }

    public Task ClearAsync(int cartId)
    {
        var items = _context.CartItems.Where(i => i.CartId == cartId).ToList();
        _context.CartItems.RemoveRange(items);
        return _context.SaveChangesAsync();
    }

    // ---- private ----

    private async Task<Cart> LoadCart(string? userId, string? guestId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .Include(c => c.Items)
            .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => (userId != null && c.UserId == userId)
                                    || (guestId != null && c.GuestId == guestId));
        if (cart != null) return cart;

        cart = new Cart { UserId = userId, GuestId = guestId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        // reload with items
        return await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cart.Id);
    }

    private async Task<CartDto> BuildAsync(Cart cart)
    {
        // resolve coupon
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == cart.CouponCode && c.IsActive);
            if (coupon == null)
            {
                cart.CouponCode = null;
                await _context.SaveChangesAsync();
            }
            else
            {
                var subtotal = cart.Items.Sum(i => (i.UnitPrice) * i.Quantity);
                discount = coupon.DiscountPercent.HasValue
                    ? subtotal * (decimal)coupon.DiscountPercent.Value
                    : (coupon.DiscountAmount ?? 0);
                discount = Math.Min(discount, subtotal);
            }
        }

        var dto = new CartDto
        {
            Id = cart.Id,
            Subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity),
            CouponCode = cart.CouponCode,
            Items = cart.Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                VariantId = i.VariantId,
                VariantSummary = i.Variant == null ? null : $"{i.Variant.Name}: {i.Variant.Value}",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };
        dto.Discount = discount;
        dto.Tax = decimal.Round((dto.Subtotal - discount) * TaxRate, 2);
        dto.Total = dto.Subtotal - discount + dto.Tax;
        return dto;
    }
}