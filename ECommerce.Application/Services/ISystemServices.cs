using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.Services;

public interface IAdminService
{
    Task<PagedResult<PagedUserDto>> GetUsersAsync(int page, int pageSize, string? search);
    Task BanUserAsync(string userId, bool ban);
    Task DeleteUserAsync(string userId);
    Task PromoteToVendor(string userId);
    Task<SalesReportDto> GetSalesReportAsync();
}

public interface IPaymentService
{
    Task<string> CreatePaymentSessionAsync(int orderId, decimal amount, string currency, string returnUrl);
}

public interface IStripeWebhookService
{
    /// <summary>
    /// Verifies the Stripe signature, applies the event to the order and its
    /// transaction record, and returns the payment outcome.
    /// Returns null for event types the store ignores (the caller should still
    /// answer 200 OK so Stripe stops retrying).
    /// Throws <see cref="BadRequestException"/> when the webhook secret is not
    /// configured or the signature/payload is invalid.
    /// </summary>
    Task<StripeWebhookResult?> ProcessAsync(string payload, string signatureHeader);
}