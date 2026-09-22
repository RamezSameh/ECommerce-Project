namespace ECommerce.Application.DTOs.Coupon;

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
}

/// <summary>
/// Fields for updating an existing coupon. All optional; omitted fields keep
/// their current values. Providing either discount field replaces the current
/// discount (the other discount type is cleared).
/// </summary>
public class UpdateCouponDto
{
    public string? Code { get; set; }
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
}

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUsages { get; set; }
    public int UsageCount { get; set; }
}
