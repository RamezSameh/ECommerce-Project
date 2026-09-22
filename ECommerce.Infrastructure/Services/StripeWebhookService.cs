using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Handles Stripe webhook events: verifies the signature with the configured
/// webhook secret and applies payment outcomes to orders and transactions.
/// Register the endpoint URL (e.g. https://host/api/orders/webhooks/stripe)
/// in the Stripe dashboard so Stripe delivers events there.
/// </summary>
public class StripeWebhookService : IStripeWebhookService
{
    private readonly AppDbContext _context;
    private readonly string? _webhookSecret;

    public StripeWebhookService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _webhookSecret = configuration["Payment:Stripe:WebhookSecret"];
    }

    public async Task<StripeWebhookResult?> ProcessAsync(string payload, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(_webhookSecret))
            throw new BadRequestException(
                "Stripe webhook secret is not configured. Set Payment:Stripe:WebhookSecret.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _webhookSecret);
        }
        catch (StripeException ex)
        {
            throw new BadRequestException($"Invalid Stripe webhook signature: {ex.Message}");
        }

        return stripeEvent.Type switch
        {
            "checkout.session.completed" => await HandleCheckoutSessionCompleted(stripeEvent),
            "checkout.session.async_payment_failed" or "checkout.session.expired"
                => await HandleSessionFailed(stripeEvent),
            "payment_intent.payment_failed" => await HandlePaymentIntentFailed(stripeEvent),
            _ => null // unrelated event type: acknowledge so Stripe stops retrying
        };
    }

    private async Task<StripeWebhookResult?> HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session
            ?? throw new BadRequestException("Webhook payload did not contain a checkout session.");

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return null; // not actually paid yet (e.g. async payment pending)

        var orderId = GetOrderId(session.Metadata);
        var amountPaid = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : (decimal?)null;
        var externalId = session.PaymentIntentId ?? session.Id;

        await MarkPaidAsync(orderId, externalId, amountPaid);

        return new StripeWebhookResult
        {
            OrderId = orderId,
            EventType = stripeEvent.Type,
            PaymentSucceeded = true,
            ExternalPaymentId = externalId,
            AmountPaid = amountPaid
        };
    }

    private async Task<StripeWebhookResult?> HandleSessionFailed(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session
            ?? throw new BadRequestException("Webhook payload did not contain a checkout session.");

        var orderId = GetOrderId(session.Metadata);
        await MarkFailedAsync(orderId, session.PaymentIntentId ?? session.Id);

        return new StripeWebhookResult
        {
            OrderId = orderId,
            EventType = stripeEvent.Type,
            PaymentSucceeded = false,
            ExternalPaymentId = session.PaymentIntentId ?? session.Id
        };
    }

    private async Task<StripeWebhookResult?> HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent
            ?? throw new BadRequestException("Webhook payload did not contain a payment intent.");

        var orderId = GetOrderId(paymentIntent.Metadata);
        await MarkFailedAsync(orderId, paymentIntent.Id);

        return new StripeWebhookResult
        {
            OrderId = orderId,
            EventType = stripeEvent.Type,
            PaymentSucceeded = false,
            ExternalPaymentId = paymentIntent.Id
        };
    }

    private static int GetOrderId(Dictionary<string, string> metadata)
    {
        if (metadata.TryGetValue("orderId", out var raw) && int.TryParse(raw, out var orderId))
            return orderId;

        throw new BadRequestException("Webhook payload is missing the 'orderId' metadata.");
    }

    private async Task MarkPaidAsync(int orderId, string externalPaymentId, decimal? amountPaid)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new NotFoundException($"Order {orderId} not found.");

        // Idempotent: Stripe may redeliver the same event.
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.ExternalPaymentId == externalPaymentId);

        if (transaction is not null && transaction.Status == "Paid")
            return;

        if (transaction is null)
        {
            transaction = new Transaction
            {
                OrderId = orderId,
                ExternalPaymentId = externalPaymentId,
                Method = ECommerce.Core.Entities.PaymentMethod.Card,
                Amount = amountPaid ?? order.Total
            };
            _context.Transactions.Add(transaction);
        }

        transaction.Status = "Paid";
        transaction.ExternalPaymentId = externalPaymentId;

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Processing;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private async Task MarkFailedAsync(int orderId, string externalPaymentId)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new NotFoundException($"Order {orderId} not found.");

        // Don't overwrite a payment that already succeeded.
        if (await _context.Transactions.AnyAsync(t =>
                t.OrderId == orderId && t.ExternalPaymentId == externalPaymentId && t.Status == "Paid"))
            return;

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.ExternalPaymentId == externalPaymentId);

        if (transaction is null)
        {
            transaction = new Transaction
            {
                OrderId = orderId,
                ExternalPaymentId = externalPaymentId,
                Method = ECommerce.Core.Entities.PaymentMethod.Card,
                Amount = order.Total
            };
            _context.Transactions.Add(transaction);
        }

        transaction.Status = "Failed";
        await _context.SaveChangesAsync();
        // Order stays Pending so the customer can retry payment.
    }
}
