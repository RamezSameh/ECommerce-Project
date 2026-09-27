using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Exceptions;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Tests.Unit;

/// <summary>
/// Covers <see cref="StripeWebhookService"/>: signature verification and the
/// payment-outcome mapping onto orders/transactions. Signatures are produced
/// with the same HMAC-SHA256 scheme Stripe uses (t={unixTs},v1={hex}).
/// </summary>
public class StripeWebhookServiceTests
{
    private const string WebhookSecret = "whsec_test_secret_123";

    private static IConfiguration ConfigWithSecret(string? secret) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payment:Stripe:WebhookSecret"] = secret
            })
            .Build();

    private static string Sign(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)))
            .ToLowerInvariant();
        return $"t={timestamp},v1={signature}";
    }

    private static string SessionCompletedPayload(int orderId, string paymentStatus = "paid") =>
        """
        {
          "id": "evt_test_123",
          "object": "event",
          "type": "checkout.session.completed",
          "data": { "object": {
              "id": "cs_test_123",
              "object": "checkout.session",
              "payment_status": "__STATUS__",
              "payment_intent": "pi_test_123",
              "amount_total": 5000,
              "metadata": { "orderId": "__ORDERID__" }
          } }
        }
        """.Replace("__STATUS__", paymentStatus).Replace("__ORDERID__", orderId.ToString());

    private static async Task<Order> SeedPendingOrderAsync(AppDbContext context)
    {
        var order = new Order
        {
            OrderNumber = $"TEST-{Guid.NewGuid():N}",
            UserId = "test-user",
            Status = OrderStatus.Pending,
            PaymentMethod = PaymentMethod.Card,
            Total = 50m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task ProcessAsync_Throws_WhenSecretNotConfigured()
    {
        var service = new StripeWebhookService(TestDbContextFactory.Create(), ConfigWithSecret(null));

        var act = () => service.ProcessAsync("{}", Sign("{}", WebhookSecret));

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*webhook secret*");
    }

    [Fact]
    public async Task ProcessAsync_Throws_OnInvalidSignature()
    {
        var service = new StripeWebhookService(TestDbContextFactory.Create(), ConfigWithSecret(WebhookSecret));

        var act = () => service.ProcessAsync("{}", "t=123,v1=deadbeef");

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*signature*");
    }

    [Fact]
    public async Task CheckoutSessionCompleted_MarksOrderPaid()
    {
        var context = TestDbContextFactory.Create();
        var order = await SeedPendingOrderAsync(context);
        var service = new StripeWebhookService(context, ConfigWithSecret(WebhookSecret));

        var payload = SessionCompletedPayload(order.Id);
        var result = await service.ProcessAsync(payload, Sign(payload, WebhookSecret));

        result.Should().NotBeNull();
        result!.PaymentSucceeded.Should().BeTrue();
        result.OrderId.Should().Be(order.Id);
        result.ExternalPaymentId.Should().Be("pi_test_123");
        result.AmountPaid.Should().Be(50m);

        (await context.Orders.FindAsync(order.Id))!.Status.Should().Be(OrderStatus.Processing);

        var tx = context.Transactions.Single(t => t.OrderId == order.Id);
        tx.Status.Should().Be("Paid");
        tx.ExternalPaymentId.Should().Be("pi_test_123");
        tx.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task CheckoutSessionCompleted_IsIdempotent()
    {
        var context = TestDbContextFactory.Create();
        var order = await SeedPendingOrderAsync(context);
        var service = new StripeWebhookService(context, ConfigWithSecret(WebhookSecret));

        var payload = SessionCompletedPayload(order.Id);
        var header = Sign(payload, WebhookSecret);

        await service.ProcessAsync(payload, header);
        await service.ProcessAsync(payload, header);

        context.Transactions.Count(t => t.OrderId == order.Id).Should().Be(1);
    }

    [Fact]
    public async Task CheckoutSessionCompleted_UnpaidStatus_IsIgnored()
    {
        var context = TestDbContextFactory.Create();
        var order = await SeedPendingOrderAsync(context);
        var service = new StripeWebhookService(context, ConfigWithSecret(WebhookSecret));

        var payload = SessionCompletedPayload(order.Id, paymentStatus: "unpaid");
        var result = await service.ProcessAsync(payload, Sign(payload, WebhookSecret));

        result.Should().BeNull();
        (await context.Orders.FindAsync(order.Id))!.Status.Should().Be(OrderStatus.Pending);
        context.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task UnknownEventType_ReturnsNull()
    {
        var context = TestDbContextFactory.Create();
        var service = new StripeWebhookService(context, ConfigWithSecret(WebhookSecret));

        var payload = """{"id":"evt_1","object":"event","type":"customer.created","data":{"object":{}}}""";
        var result = await service.ProcessAsync(payload, Sign(payload, WebhookSecret));

        result.Should().BeNull();
    }
}
