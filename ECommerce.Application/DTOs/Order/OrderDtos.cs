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

/// <summary>Outcome of processing a verified Stripe webhook event for an order payment.</summary>
public class StripeWebhookResult
{
    public int OrderId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool PaymentSucceeded { get; set; }
    public string? ExternalPaymentId { get; set; }
    public decimal? AmountPaid { get; set; }
}