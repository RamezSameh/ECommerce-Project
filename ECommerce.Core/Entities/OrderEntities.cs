using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Entities;

/// <summary>Lifecycle of an order.</summary>
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

/// <summary>How the customer intends to pay.</summary>
public enum PaymentMethod
{
    CashOnDelivery = 0,
    Card = 1,
    Wallet = 2
}

/// <summary>Snapshot of a placed order.</summary>
public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }

    [Precision(18, 2)] public decimal Subtotal { get; set; }
    [Precision(18, 2)] public decimal Discount { get; set; }
    [Precision(18, 2)] public decimal Tax { get; set; }
    [Precision(18, 2)] public decimal Shipping { get; set; }
    [Precision(18, 2)] public decimal Total { get; set; }

    public string? CouponCode { get; set; }

    public string? ShippingAddress { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantSummary { get; set; }

    public int Quantity { get; set; }
    [Precision(18, 2)] public decimal UnitPrice { get; set; }
}

/// <summary>Promotion/discount code.</summary>
public class Coupon
{
    public int Id { get; set; }
    public required string Code { get; set; }
    [Precision(18, 2)] public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>A single payment / transaction attempt tied to an order.</summary>
public class Transaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public string? ExternalPaymentId { get; set; } // Stripe PaymentIntent/Payment token
    public PaymentMethod Method { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
    [Precision(18, 2)] public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Refresh token issued alongside the access (JWT) token.</summary>
public class RefreshToken
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public required string Token { get; set; }      // stored hashed
    public required string JwtId { get; set; }      // the jti of the access token it belongs to
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}