using ECommerce.Application.DTOs.Admin;
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