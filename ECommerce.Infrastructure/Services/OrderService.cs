using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(AppDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    private const decimal TaxRate = 0.14m;

    public async Task<OrderDto> CheckoutAsync(string userId, string? guestId, CheckoutDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.Items).ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => (userId != null && c.UserId == userId)
                                      || (guestId != null && c.GuestId == guestId))
            ?? throw new BadRequestException("Cart is empty");

        if (cart.Items.Count == 0)
            throw new BadRequestException("Cart is empty");

        // Compute totals (mirror the cart service calculations)
        var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            var coupon = await ValidateCoupon(cart.CouponCode);
            if (coupon != null)
            {
                discount = coupon.DiscountPercent.HasValue
                    ? subtotal * (decimal)coupon.DiscountPercent.Value
                    : (coupon.DiscountAmount ?? 0);
                discount = Math.Min(discount, subtotal);
                coupon.UsageCount++;
                _context.Coupons.Update(coupon);
            }
        }
        var tax = decimal.Round((subtotal - discount) * TaxRate, 2);
        var shipping = subtotal >= 100 ? 0 : 25; // free shipping above 100
        var total = subtotal - discount + tax + shipping;

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            PaymentMethod = dto.PaymentMethod,
            Subtotal = subtotal,
            Discount = discount,
            Tax = tax,
            Shipping = shipping,
            Total = total,
            CouponCode = cart.CouponCode,
            ShippingAddress = dto.ShippingAddress,
            Phone = dto.Phone,
            Notes = dto.Notes,
            Status = OrderStatus.Pending,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                VariantSummary = i.Variant == null ? null : $"{i.Variant.Name}: {i.Variant.Value}",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.Orders.Add(order);

        // Decrement stock transactionally (single SaveChanges commits everything together)
        foreach (var item in cart.Items)
        {
            if (item.VariantId.HasValue)
            {
                var variant = item.Variant ?? await _context.ProductVariants.FindAsync(item.VariantId.Value);
                if (variant is null || variant.Stock < item.Quantity)
                    throw new BusinessException($"Insufficient stock for {item.Product?.Name}");
                variant.Stock -= item.Quantity;
            }
            else if (item.Product is null || item.Product.Stock < item.Quantity)
            {
                throw new BusinessException($"Insufficient stock for {item.Product?.Name}");
            }
            else
            {
                item.Product.Stock -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        // empty the cart
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var order = await _context.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Order {id} not found");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedException("You can only view your own orders");
        return ToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetMyOrdersAsync(string userId, int page, int pageSize)
    {
        var q = _context.Orders.AsNoTracking().Include(o => o.Items).Where(o => o.UserId == userId);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var dtos = items.Select(ToDto).ToList();
        return PagedResult<OrderDto>.Create(dtos, total, page, pageSize);
    }

    public async Task<OrderDto> CancelAsync(int id, string userId, bool isAdmin)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Order {id} not found");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedException("You can only cancel your own orders");

        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessException("Order is already cancelled");
        if (!isAdmin && order.Status != OrderStatus.Pending)
            throw new BusinessException("Only pending orders can be cancelled");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // Restore stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null) product.Stock += item.Quantity;
        }
        await _context.SaveChangesAsync();
        return ToDto(order);
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id)
            ?? throw new NotFoundException($"Order {id} not found");
        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;
        if (dto.Status == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(int page, int pageSize, OrderStatus? status)
    {
        var q = _context.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return PagedResult<OrderDto>.Create(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    // ---- helpers ----

    private static string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20];

    private async Task<Coupon?> ValidateCoupon(string code)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        if (coupon is null || !coupon.IsActive) return null;
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow) return null;
        if (coupon.MaxUsages.HasValue && coupon.UsageCount >= coupon.MaxUsages) return null;
        return coupon;
    }

    private static OrderDto ToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status,
        PaymentMethod = o.PaymentMethod,
        Subtotal = o.Subtotal,
        Discount = o.Discount,
        Tax = o.Tax,
        Shipping = o.Shipping,
        Total = o.Total,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            VariantSummary = i.VariantSummary,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            LineTotal = i.UnitPrice * i.Quantity
        }).ToList()
    };
}