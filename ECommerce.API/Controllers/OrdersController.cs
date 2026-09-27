using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly InvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IStripeWebhookService _webhookService;

    public OrdersController(IOrderService orderService, InvoiceService invoiceService,
        IPaymentService paymentService, IStripeWebhookService webhookService)
    {
        _orderService = orderService;
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _webhookService = webhookService;
    }

    // ---- Customer ----

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var guestId = Request.Headers["X-Guest-Id"].FirstOrDefault();
        var order = await _orderService.CheckoutAsync(userId, guestId, dto);

        // Online card payments redirect to Stripe; COD completes immediately.
        if (order.PaymentMethod == PaymentMethod.Card)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = await _paymentService.CreatePaymentSessionAsync(order.Id, order.Total, "usd", baseUrl);
            return Ok(ApiResponse<object>.Success(new { Order = order, PaymentUrl = url }));
        }

        return Ok(ApiResponse<OrderDto>.Success(order, "Order placed"));
    }

    [Authorize]
    [HttpGet("my-orders")]
    public async Task<IActionResult> MyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<object>.Success(await _orderService.GetMyOrdersAsync(userId, page, pageSize)));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        return Ok(ApiResponse<OrderDto>.Success(await _orderService.GetByIdAsync(id, userId, isAdmin)));
    }

    [Authorize]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        return Ok(ApiResponse<OrderDto>.Success(await _orderService.CancelAsync(id, userId, isAdmin)));
    }

    [Authorize]
    [HttpGet("{id:int}/invoice")]
    public async Task<IActionResult> Invoice(int id)
    {
        // Ownership check (non-admins can only fetch their own invoices)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        await _orderService.GetByIdAsync(id, userId, isAdmin);

        var pdf = await _invoiceService.GenerateAsync(id);
        return File(pdf, "application/pdf", $"invoice-{id}.pdf");
    }

    [HttpGet("payment-success")]
    public async Task<IActionResult> PaymentSuccess([FromQuery] int id)
    {
        var order = await TryGetOrder(id);
        // Payment state is authoritative only via the Stripe webhook
        // (POST /api/orders/webhooks/stripe); this return URL is for display only.
        return Ok(ApiResponse<OrderDto>.Success(order));
    }

    [HttpGet("payment-cancel")]
    public async Task<IActionResult> PaymentCancel([FromQuery] int id)
    {
        var order = await TryGetOrder(id);
        return Ok(ApiResponse<OrderDto>.Fail($"Payment for order {id} was cancelled"));
    }

    // ---- Stripe webhook (Stripe sends no JWT; authenticity is proven by the
    // signature header, and it is exempted from the global rate limiter) ----

    [AllowAnonymous]
    [DisableRateLimiting]
    [HttpPost("webhooks/stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        // The raw body must be read untouched: signature verification fails on
        // any re-serialization of the payload.
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        var result = await _webhookService.ProcessAsync(payload, signature);
        return Ok(ApiResponse<object?>.Success(result, result is null ? "Event ignored" : "Webhook processed"));
    }

    // ---- Admin ----

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
        => Ok(ApiResponse<OrderDto>.Success(await _orderService.UpdateStatusAsync(id, dto)));

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> All([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null)
        => Ok(ApiResponse<object>.Success(await _orderService.GetAllAsync(page, pageSize, status)));

    private async Task<OrderDto> TryGetOrder(int id)
    {
        try
        {
            return await _orderService.GetByIdAsync(id, "", false);
        }
        catch
        {
            return null!; // safe fallback
        }
    }
}