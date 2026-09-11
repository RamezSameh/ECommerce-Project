using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Services;

public interface ICartService
{
    /// <summary>Resolve or create the current cart. Registered users use their Id; guests use a GuestId.</summary>
    Task<CartDto> GetCartAsync(string? userId, string? guestId);
    Task<CartDto> AddItemAsync(string? userId, string? guestId, AddCartItemDto dto);
    Task<CartDto> UpdateQuantityAsync(string? userId, string? guestId, int productId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(string? userId, string? guestId, int productId);
    Task<CartDto> ApplyCouponAsync(string? userId, string? guestId, string code);
    Task<CartDto> RemoveCouponAsync(string? userId, string? guestId);
    Task MergeGuestCartAsync(string guestId, string userId);
    Task ClearAsync(int cartId);
}