using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Exceptions;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Unit;

public class CartServiceTests
{
    private const string GuestId = "guest-abc";

    private static (ECommerce.Infrastructure.Data.AppDbContext Context, CartService Service, int ProductId)
        ArrangeWithProduct(decimal price = 50m, int stock = 10)
    {
        var context = TestDbContextFactory.Create();
        var product = new Product
        {
            Name = "Test Product",
            Description = "A product used by tests",
            Price = price,
            Stock = stock
        };
        context.Products.Add(product);
        context.SaveChanges();
        return (context, new CartService(context), product.Id);
    }

    [Fact]
    public async Task AddItem_AddsProductToNewGuestCart()
    {
        var (context, service, productId) = ArrangeWithProduct();

        var cart = await service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = productId, Quantity = 2 });

        cart.Items.Should().ContainSingle();
        cart.Items.Single().ProductId.Should().Be(productId);
        cart.Items.Single().Quantity.Should().Be(2);
        cart.Subtotal.Should().Be(100m);
    }

    [Fact]
    public async Task AddItem_WithZeroQuantity_ThrowsBusinessException()
    {
        var (_, service, productId) = ArrangeWithProduct();

        var act = () => service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = productId, Quantity = 0 });

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task AddItem_UnknownProduct_ThrowsNotFoundException()
    {
        var (_, service, _) = ArrangeWithProduct();

        var act = () => service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = 999, Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateQuantity_ChangesItemQuantity()
    {
        var (_, service, productId) = ArrangeWithProduct();
        await service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = productId, Quantity = 1 });

        var cart = await service.UpdateQuantityAsync(null, GuestId, productId,
            new UpdateCartItemDto { Quantity = 3 });

        cart.Items.Single().Quantity.Should().Be(3);
        cart.Subtotal.Should().Be(150m);
    }

    [Fact]
    public async Task UpdateQuantity_ToZero_RemovesItem()
    {
        var (_, service, productId) = ArrangeWithProduct();
        await service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = productId, Quantity = 1 });

        var cart = await service.UpdateQuantityAsync(null, GuestId, productId,
            new UpdateCartItemDto { Quantity = 0 });

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task MergeGuestCart_MovesItemsIntoUserCart()
    {
        var (context, service, productId) = ArrangeWithProduct();

        // guest cart with 2 units
        await service.AddItemAsync(null, GuestId,
            new AddCartItemDto { ProductId = productId, Quantity = 2 });
        // existing user cart with 1 unit of the same product
        await service.AddItemAsync("user-1", null,
            new AddCartItemDto { ProductId = productId, Quantity = 1 });

        await service.MergeGuestCartAsync(GuestId, "user-1");

        var userCart = await service.GetCartAsync("user-1", null);
        userCart.Items.Should().ContainSingle();
        userCart.Items.Single().Quantity.Should().Be(3);

        // the guest cart itself is removed after the merge
        context.Carts.Any(c => c.GuestId == GuestId).Should().BeFalse();
    }

    [Fact]
    public async Task MergeGuestCart_WithEmptyGuestCart_DoesNothing()
    {
        var (_, service, _) = ArrangeWithProduct();

        var act = () => service.MergeGuestCartAsync("ghost-guest", "user-1");

        await act.Should().NotThrowAsync();
    }
}
