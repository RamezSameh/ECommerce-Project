using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Stripe checkout-session based payment. Reads the Stripe secret from
/// configuration. When no key is configured the service reports that card
/// payments are disabled (the app still supports Cash on Delivery).
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly string? _secretKey;

    public StripePaymentService(IConfiguration configuration)
        => _secretKey = configuration["Payment:Stripe:SecretKey"];

    public async Task<string> CreatePaymentSessionAsync(
        int orderId, decimal amount, string currency, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            throw new BadRequestException("Stripe is not configured. Use Cash on Delivery instead.");

        Stripe.StripeConfiguration.ApiKey = _secretKey;

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = $"{returnUrl}/api/orders/{orderId}/payment-success",
            CancelUrl = $"{returnUrl}/api/orders/{orderId}/payment-cancel",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency.ToLowerInvariant(),
                        UnitAmount = (long)Math.Round(amount * 100), // cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order {orderId}"
                        }
                    },
                    Quantity = 1
                }
            },
            Metadata = new Dictionary<string, string> { ["orderId"] = orderId.ToString() }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }
}