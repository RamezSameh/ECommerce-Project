using ECommerce.Core.Entities;

namespace ECommerce.Application.DTOs.Order;

public class CheckoutDto
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public string? ShippingAddress { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantSummary { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>DTO for admin status transitions.</summary>
public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
}

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
    public int UsageCount { get; set; }
}