using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public AdminService(UserManager<User> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<PagedResult<PagedUserDto>> GetUsersAsync(int page, int pageSize, string? search)
    {
        var q = _userManager.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u => u.Email!.Contains(search) || u.UserName!.Contains(search));

        var total = await q.CountAsync();
        var users = await q.OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var dtos = new List<PagedUserDto>();
        foreach (var u in users)
        {
            dtos.Add(new PagedUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                EmailConfirmed = u.EmailConfirmed,
                IsBanned = u.IsBanned,
                CreatedAt = u.CreatedAt,
                Roles = (await _userManager.GetRolesAsync(u)).ToList()
            });
        }
        return PagedResult<PagedUserDto>.Create(dtos, total, page, pageSize);
    }

    public async Task BanUserAsync(string userId, bool ban)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found");
        user.IsBanned = ban;
        if (ban)
        {
            // lock all existing refresh tokens
            var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            foreach (var t in tokens) t.IsRevoked = true;
        }
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found");
        if (await _userManager.IsInRoleAsync(user, "Admin"))
            throw new BusinessException("Cannot delete an admin account");
        await _userManager.DeleteAsync(user);
    }

    public async Task PromoteToVendor(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found");
        await _userManager.AddToRoleAsync(user, "Vendor");
    }

    public async Task<SalesReportDto> GetSalesReportAsync()
    {
        var delivered = await _context.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered).ToListAsync();

        var revenueByDay = delivered
            .GroupBy(o => o.CreatedAt.Date)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"),
                g => g.Sum(o => o.Total));

        var topProducts = await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(10)
            .ToListAsync();

        return new SalesReportDto
        {
            TotalRevenue = delivered.Sum(o => o.Total),
            TotalOrders = delivered.Count,
            TotalProducts = await _context.Products.CountAsync(),
            TotalUsers = await _userManager.Users.CountAsync(),
            RevenueByDay = revenueByDay,
            TopProducts = topProducts
        };
    }
}