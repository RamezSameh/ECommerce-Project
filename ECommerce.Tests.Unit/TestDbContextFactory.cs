using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Unit;

/// <summary>
/// Builds a brand-new <see cref="AppDbContext"/> backed by EF Core InMemory.
/// Every call uses a unique database name, so tests are fully independent.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"ecommerce-unit-tests-{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
