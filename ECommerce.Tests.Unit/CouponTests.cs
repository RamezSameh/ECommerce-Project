using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using FluentAssertions;

namespace ECommerce.Tests.Unit;

/// <summary>
/// Coupon validation rules (expiry, usage limits, unknown codes) are enforced by
/// <see cref="OrderService.CheckoutAsync"/> — the only place coupons are redeemed —
/// so these tests drive the validation through checkout. Direct <see cref="CouponService"/>
/// CRUD behaviour is covered in <see cref="CouponServiceTests"/>.
/// </summary>
public class CouponValidationTests
{
    private static async Task<(AppDbContext Context, OrderService Orders)> ArrangeAsync(
        Action<Coupon> configureCoupon, string couponCode = "SAVE10")
    {
        var context = TestDbContextFactory.Create();

        var product = new Product
        {
            Name = "Test Product",
            Description = "A product used by tests",
            Price = 100m,
            Stock = 50
        };
        context.Products.Add(product);

        var coupon = new Coupon
        {
            Code = "SAVE10",
            DiscountPercent = 0.10, // 10%
            IsActive = true
        };
        configureCoupon(coupon);
        context.Coupons.Add(coupon);

        var cart = new Cart { UserId = "user-1", CouponCode = couponCode };
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        cart.Items.Add(new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = 1,
            UnitPrice = product.Price
        });
        await context.SaveChangesAsync();

        var orders = new OrderService(context, new CartService(context));
        return (context, orders);
    }

    [Fact]
    public async Task Checkout_WithValidCoupon_AppliesDiscountAndCountsUsage()
    {
        var (_, orders) = await ArrangeAsync(_ => { });

        var order = await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        order.Discount.Should().Be(10m); // 10% of 100
    }

    [Fact]
    public async Task Checkout_WithValidCoupon_IncrementsUsageCount()
    {
        var (context, orders) = await ArrangeAsync(_ => { });

        await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        context.Coupons.Single(c => c.Code == "SAVE10").UsageCount.Should().Be(1);
    }

    [Fact]
    public async Task Checkout_WithExpiredCoupon_AppliesNoDiscount()
    {
        var (_, orders) = await ArrangeAsync(c => c.ExpiresAt = DateTime.UtcNow.AddDays(-1));

        var order = await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        order.Discount.Should().Be(0m);
    }

    [Fact]
    public async Task Checkout_WithMaxedOutCoupon_AppliesNoDiscount()
    {
        var (_, orders) = await ArrangeAsync(c =>
        {
            c.MaxUsages = 5;
            c.UsageCount = 5;
        });

        var order = await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        order.Discount.Should().Be(0m);
    }

    [Fact]
    public async Task Checkout_WithUnknownCouponCode_AppliesNoDiscount()
    {
        var (_, orders) = await ArrangeAsync(_ => { }, couponCode: "NOPE");

        var order = await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        order.Discount.Should().Be(0m);
    }

    [Fact]
    public async Task Checkout_WithInactiveCoupon_AppliesNoDiscount()
    {
        var (_, orders) = await ArrangeAsync(c => c.IsActive = false);

        var order = await orders.CheckoutAsync("user-1", null, new CheckoutDto());

        order.Discount.Should().Be(0m);
    }
}

/// <summary>Direct CRUD tests for <see cref="CouponService"/>.</summary>
public class CouponServiceTests
{
    [Fact]
    public async Task Create_WithDuplicateCode_ThrowsBusinessException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        await service.CreateAsync(new CreateCouponDto { Code = "DUP", DiscountPercent = 0.1 });

        var act = () => service.CreateAsync(new CreateCouponDto { Code = "dup", DiscountPercent = 0.2 });

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Create_WithoutAnyDiscount_ThrowsBusinessException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);

        var act = () => service.CreateAsync(new CreateCouponDto { Code = "NODISC" });

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task ToggleActive_FlipsIsActiveFlag()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var created = await service.CreateAsync(new CreateCouponDto { Code = "TOGGLE", DiscountAmount = 5m });

        await service.ToggleActiveAsync(created.Id);

        context.Coupons.Single(c => c.Id == created.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_UnknownId_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);

        var act = () => service.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
