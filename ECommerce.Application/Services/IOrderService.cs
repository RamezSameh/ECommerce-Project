using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(string userId, string? guestId, CheckoutDto dto);
    Task<OrderDto> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<PagedResult<OrderDto>> GetMyOrdersAsync(string userId, int page, int pageSize);
    Task<OrderDto> CancelAsync(int id, string userId, bool isAdmin);
    Task<OrderDto> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    Task<PagedResult<OrderDto>> GetAllAsync(int page, int pageSize, Core.Entities.OrderStatus? status);
}

public interface ICouponService
{
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
    Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize);
    Task ToggleActiveAsync(int id);
    Task DeleteAsync(int id);
}