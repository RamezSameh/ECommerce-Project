using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;

    public CouponService(AppDbContext context) => _context = context;

    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        if (dto.DiscountAmount.HasValue && dto.DiscountPercent.HasValue)
            throw new BusinessException("Specify either a fixed amount or a percentage, not both");
        if (!dto.DiscountAmount.HasValue && !dto.DiscountPercent.HasValue)
            throw new BusinessException("A discount amount or percent is required");

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _context.Coupons.AnyAsync(c => c.Code == code))
            throw new BusinessException("Coupon code already exists");

        var coupon = new Coupon
        {
            Code = code,
            DiscountAmount = dto.DiscountAmount,
            DiscountPercent = dto.DiscountPercent,
            ExpiresAt = dto.ExpiresAt,
            MaxUsages = dto.MaxUsages
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize)
    {
        var q = _context.Coupons.AsNoTracking();
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return PagedResult<CouponDto>.Create(items.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<CouponDto> UpdateAsync(int id, UpdateCouponDto dto)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new NotFoundException($"Coupon {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var code = dto.Code.Trim().ToUpperInvariant();
            if (await _context.Coupons.AnyAsync(c => c.Id != id && c.Code == code))
                throw new BusinessException("Coupon code already exists");
            coupon.Code = code;
        }

        // Discount fields: omitted = unchanged. Providing one replaces the
        // current discount (the other discount type is cleared, they are mutually exclusive).
        if (dto.DiscountAmount.HasValue || dto.DiscountPercent.HasValue)
        {
            if (dto.DiscountAmount.HasValue && dto.DiscountPercent.HasValue)
                throw new BusinessException("Specify either a fixed amount or a percentage, not both");
            coupon.DiscountAmount = dto.DiscountAmount;
            coupon.DiscountPercent = dto.DiscountPercent;
        }

        if (dto.IsActive.HasValue) coupon.IsActive = dto.IsActive.Value;
        if (dto.ExpiresAt.HasValue) coupon.ExpiresAt = dto.ExpiresAt;
        if (dto.MaxUsages.HasValue) coupon.MaxUsages = dto.MaxUsages;

        await _context.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new NotFoundException($"Coupon {id} not found");
        coupon.IsActive = !coupon.IsActive;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new NotFoundException($"Coupon {id} not found");
        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
    }

    private static CouponDto Map(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        DiscountAmount = c.DiscountAmount,
        DiscountPercent = c.DiscountPercent,
        IsActive = c.IsActive,
        ExpiresAt = c.ExpiresAt,
        MaxUsages = c.MaxUsages,
        UsageCount = c.UsageCount
    };
}