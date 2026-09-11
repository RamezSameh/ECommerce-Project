using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

/// <summary>
/// Shopping cart. Works for both registered users and guests. Guests pass a
/// GuestId in either the "X-Guest-Id" header or a "guestId" query parameter.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    private (string? UserId, string? GuestId) Identity()
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var guestId = Request.Headers["X-Guest-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(guestId))
            guestId = Request.Query["guestId"].FirstOrDefault();
        return (userId, guestId);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.GetCartAsync(userId, guestId)));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemDto dto)
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.AddItemAsync(userId, guestId, dto)));
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateItem(int productId, UpdateCartItemDto dto)
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.UpdateQuantityAsync(userId, guestId, productId, dto)));
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.RemoveItemAsync(userId, guestId, productId)));
    }

    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponDto dto)
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.ApplyCouponAsync(userId, guestId, dto.Code)));
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon()
    {
        var (userId, guestId) = Identity();
        return Ok(ApiResponse<CartDto>.Success(await _cartService.RemoveCouponAsync(userId, guestId)));
    }

    [Authorize]
    [HttpPost("merge")]
    public async Task<IActionResult> Merge(string guestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _cartService.MergeGuestCartAsync(guestId, userId);
        return Ok(ApiResponse<object>.Success(null!, "Guest cart merged"));
    }
}